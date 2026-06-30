<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import PageHeader from '../../components/common/Pageheader.vue'
import EmptyState from '../../components/common/Emptystate.vue'
import { getRequests } from '../../services/doctorRequestService'
import { getErrorMessage } from '../../utils/errors'
import { useNotifications } from '../../composables/useNotifications'
import type { DoctorRequestListItem } from '../../types/api'

const { error: showError } = useNotifications()
const router = useRouter()

const items = ref<DoctorRequestListItem[]>([])
const loading = ref(false)
const page = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)
const search = ref('')
const statusFilter = ref('')
const pageSize = ref(10)

const statusOptions = [
  { value: '', label: 'الكل' },
  { value: 'Waiting', label: 'قيد الانتظار' },
  { value: 'Accepted', label: 'مقبول' },
  { value: 'Rejected', label: 'مرفوض' },
]

function statusLabel(s: string) {
  switch (s) {
    case 'Waiting': return 'قيد الانتظار'
    case 'Accepted': return 'مقبول'
    case 'Rejected': return 'مرفوض'
    default: return s
  }
}

function statusColor(s: string) {
  switch (s) {
    case 'Accepted': return 'success'
    case 'Waiting': return 'warning'
    case 'Rejected': return 'error'
    default: return 'default'
  }
}

async function fetchData() {
  loading.value = true
  try {
    const res = await getRequests({
      page: page.value,
      pageSize: pageSize.value,
      status: statusFilter.value || undefined,
      search: search.value || undefined,
    })
    items.value = res.data!.items
    totalPages.value = res.data!.totalPages
    totalItems.value = res.data!.totalItems
  } catch (e) {
    showError(getErrorMessage(e))
  } finally {
    loading.value = false
  }
}

function viewDetails(id: number) {
  router.push(`/doctor-requests/${id}`)
}

let searchTimer: ReturnType<typeof setTimeout>

function onSearchInput() {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    page.value = 1
    fetchData()
  }, 400)
}

function clearSearch() {
  search.value = ''
  page.value = 1
  fetchData()
}

onMounted(fetchData)
</script>

<template>
  <div class="dr-list-page">
    <PageHeader title="طلبات التحويل إلى طبيب" subtitle="إدارة طلبات تحويل حسابات المرضى إلى أطباء" />

    <!-- Filters -->
    <v-card elevation="0" class="filters-card">
      <div class="filters-row">
        <div class="request-search">
          <v-icon icon="mdi-magnify" size="20" class="request-search-icon" />
          <input
            v-model="search"
            class="request-search-input"
            placeholder="بحث بالاسم أو رقم الهاتف أو الكود..."
            @input="onSearchInput"
          />
          <button
            v-if="search"
            type="button"
            class="request-search-clear"
            aria-label="مسح البحث"
            @click="clearSearch"
          >
            <v-icon icon="mdi-close" size="16" />
          </button>
        </div>
        <v-select
          v-model="statusFilter"
          :items="statusOptions"
          item-title="label"
          item-value="value"
          density="compact"
          variant="outlined"
          hide-details
          class="status-select"
          @update:model-value="page = 1; fetchData()"
        />
        <v-btn
          class="refresh-button"
          variant="outlined"
          color="primary"
          prepend-icon="mdi-refresh"
          :loading="loading"
          @click="fetchData"
        >
          تحديث
        </v-btn>
      </div>
    </v-card>

    <!-- Table -->
    <v-card elevation="0" class="table-card">
      <v-skeleton-loader v-if="loading" type="table-row@5" />

      <EmptyState
        v-else-if="!items.length"
        icon="mdi-file-document-outline"
        title="لا توجد طلبات"
        description="لم يتم تقديم أي طلبات تحويل بعد"
      />

      <div v-else class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th>الكود</th>
              <th>الاسم</th>
              <th>رقم الهاتف</th>
              <th>التخصص</th>
              <th>الحالة</th>
              <th>التاريخ</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in items" :key="item.id" @click="viewDetails(item.id)">
              <td>
                <span class="code-cell" dir="ltr">{{ item.code }}</span>
              </td>
              <td>
                <div class="name-cell">
                  <span class="name-main">{{ item.fullName }}</span>
                  <span class="name-sub">{{ item.knownName }}</span>
                </div>
              </td>
              <td class="muted-cell" dir="ltr">{{ item.phoneNumber }}</td>
              <td class="muted-cell">{{ item.specializationName }}</td>
              <td>
                <v-chip size="small" :color="statusColor(item.status)" variant="tonal">
                  {{ statusLabel(item.status) }}
                </v-chip>
              </td>
              <td class="muted-cell">
                {{ new Date(item.createdAt).toLocaleDateString('ar-IQ', { year: 'numeric', month: 'short', day: 'numeric' }) }}
              </td>
              <td>
                <v-btn icon size="small" variant="tonal" color="primary" @click.stop="viewDetails(item.id)">
                  <v-icon icon="mdi-chevron-left" size="16" />
                </v-btn>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="pagination-row">
        <v-pagination
          v-model="page"
          :length="totalPages"
          :total-visible="5"
          @update:model-value="fetchData"
        />
        <span class="pagination-info">إجمالي {{ totalItems }} طلب</span>
      </div>
    </v-card>
  </div>
</template>

<style scoped>
.dr-list-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.filters-card {
  border: 1px solid var(--color-border) !important;
  border-radius: var(--radius-lg) !important;
  padding: var(--spacing-lg);
}

.filters-row {
  display: grid;
  grid-template-columns: auto 180px minmax(0, 1fr);
  grid-template-areas: "refresh status search";
  gap: var(--spacing-md);
  align-items: center;
  direction: ltr;
}

.request-search {
  grid-area: search;
  position: relative;
  min-width: 0;
  direction: rtl;
}

.request-search-icon {
  position: absolute;
  right: 12px;
  top: 50%;
  z-index: 1;
  color: var(--color-text-muted);
  transform: translateY(-50%);
  pointer-events: none;
}

.request-search-input {
  width: 100%;
  height: 40px;
  padding: 0 40px 0 38px;
  border: 1px solid rgba(0, 0, 0, 0.38);
  border-radius: 4px;
  background: var(--color-surface);
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-size: 14px;
  outline: none;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.request-search-input:hover {
  border-color: rgba(0, 0, 0, 0.72);
}

.request-search-input:focus {
  border-color: var(--color-primary);
  box-shadow: inset 0 0 0 1px var(--color-primary);
}

.request-search-clear {
  position: absolute;
  left: 8px;
  top: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border: 0;
  border-radius: 50%;
  background: transparent;
  color: var(--color-text-muted);
  transform: translateY(-50%);
}

.request-search-clear:hover {
  background: var(--color-primary-soft);
  color: var(--color-primary);
}

.status-select {
  grid-area: status;
  width: 180px;
  direction: rtl;
}

.refresh-button {
  grid-area: refresh;
  justify-self: start;
  white-space: nowrap;
}

.table-card {
  border: 1px solid var(--color-border) !important;
  border-radius: var(--radius-lg) !important;
  overflow: hidden;
}

.table-scroll {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.data-table th {
  padding: 12px 16px;
  text-align: right;
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-muted);
  background: var(--color-background);
  border-bottom: 1px solid var(--color-border);
  white-space: nowrap;
}

.data-table td {
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border-light);
  vertical-align: middle;
  color: var(--color-text);
}

.data-table tbody tr {
  cursor: pointer;
  transition: background 0.15s;
}

.data-table tbody tr:hover {
  background: var(--color-primary-soft);
}

.data-table tbody tr:last-child td {
  border-bottom: none;
}

.code-cell {
  font-weight: 700;
  font-family: monospace;
  color: var(--color-primary);
}

.name-cell {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.name-main {
  font-weight: 600;
}

.name-sub {
  font-size: 11px;
  color: var(--color-text-muted);
}

.muted-cell {
  color: var(--color-text-muted);
}

.pagination-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-lg);
  border-top: 1px solid var(--color-border);
}

.pagination-info {
  font-size: 13px;
  color: var(--color-text-muted);
}

@media (max-width: 768px) {
  .filters-row {
    grid-template-columns: 1fr;
    grid-template-areas:
      "search"
      "status"
      "refresh";
  }
  .status-select {
    width: 100%;
  }
  .request-search {
    width: 100%;
  }
  .refresh-button {
    justify-self: stretch;
  }
}
</style>
