<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import api from '../services/api'
import { useNotifications } from '../composables/useNotifications'
import type { ApiResponse, DatabaseBackupResponse, DatabaseRestoreResponse, CreateDatabaseRestoreRequest } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import PageHeader from '../components/common/Pageheader.vue'
import EmptyState from '../components/common/Emptystate.vue'

const { success: showSuccess, error: showError } = useNotifications()

const backups = ref<DatabaseBackupResponse[]>([])
const restoreStatus = ref<DatabaseRestoreResponse | null>(null)
const loading = ref(false)
const creatingBackup = ref(false)
const restoring = ref(false)
const deleteDialog = ref(false)
const deleteId = ref<string | null>(null)
const password = ref('')
const useLatest = ref(true)
const selectedBackupId = ref('')

const lastCompleted = computed(() =>
  backups.value
    .filter(b => b.status === 'Completed')
    .sort((a, b) => new Date(b.completedAt ?? b.createdAt).getTime() - new Date(a.completedAt ?? a.createdAt).getTime())[0]
)

const incompleteBackup = computed(() =>
  backups.value.find(b => b.status === 'Queued' || b.status === 'Running')
)

const completedBackups = computed(() =>
  backups.value.filter(b => b.status === 'Completed')
)

async function fetchData() {
  loading.value = true
  try {
    const [listRes, restoreRes] = await Promise.all([
      api.get<ApiResponse<DatabaseBackupResponse[]>>('/DatabaseBackups'),
      api.get<ApiResponse<DatabaseRestoreResponse | null>>('/DatabaseBackups/restore-status'),
    ])
    backups.value = listRes.data.data ?? []
    restoreStatus.value = restoreRes.data.data ?? null
  } catch (e) {
    showError(getErrorMessage(e))
  } finally {
    loading.value = false
  }
}

async function createBackup() {
  creatingBackup.value = true
  try {
    await api.post<ApiResponse<DatabaseBackupResponse>>('/DatabaseBackups')
    showSuccess('تمت إضافة النسخة الاحتياطية إلى قائمة التنفيذ.')
    await fetchData()
  } catch (e) {
    showError(getErrorMessage(e))
  } finally {
    creatingBackup.value = false
  }
}

async function openFolder() {
  try {
    await api.post('/DatabaseBackups/open-folder')
  } catch (e) {
    showError(getErrorMessage(e))
  }
}

async function doRestore() {
  if (!password.value.trim()) {
    showError('يرجى إدخال كلمة المرور.')
    return
  }
  if (!useLatest.value && !selectedBackupId.value) {
    showError('يرجى اختيار نسخة احتياطية.')
    return
  }

  restoring.value = true
  try {
    const body: CreateDatabaseRestoreRequest = {
      backupId: useLatest.value ? '' : selectedBackupId.value,
      password: password.value,
      useLatest: useLatest.value,
    }
    await api.post<ApiResponse<DatabaseRestoreResponse>>('/DatabaseBackups/restore', body)
    showSuccess('تمت إضافة عملية الاستعادة إلى قائمة التنفيذ.')
    password.value = ''
    await fetchData()
  } catch (e) {
    showError(getErrorMessage(e))
  } finally {
    restoring.value = false
  }
}

async function downloadBackup(id: string) {
  try {
    const r = await api.get<Blob>(`/DatabaseBackups/${id}/download`, {
      responseType: 'blob',
    })
    const url = window.URL.createObjectURL(new Blob([r.data]))
    const link = document.createElement('a')
    link.href = url
    const backup = backups.value.find(b => b.id === id)
    link.setAttribute('download', backup?.fileName ?? `backup-${id}.backup`)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
  } catch (e) {
    showError(getErrorMessage(e))
  }
}

function confirmDelete(id: string) {
  deleteId.value = id
  deleteDialog.value = true
}

async function doDelete() {
  if (!deleteId.value) return
  try {
    await api.delete(`/DatabaseBackups/${deleteId.value}`)
    showSuccess('تم حذف النسخة الاحتياطية.')
    await fetchData()
  } catch (e) {
    showError(getErrorMessage(e))
  } finally {
    deleteDialog.value = false
    deleteId.value = null
  }
}

function formatSize(bytes?: number): string {
  if (!bytes) return '--'
  const units = ['B', 'KB', 'MB', 'GB']
  let i = 0; let size = bytes
  while (size >= 1024 && i < units.length - 1) { size /= 1024; i++ }
  return `${size.toFixed(i > 0 ? 1 : 0)} ${units[i]}`
}

function statusColor(status: string): string {
  switch (status) {
    case 'Completed': return 'success'
    case 'Running': return 'info'
    case 'Queued': return 'warning'
    case 'Failed': return 'error'
    default: return 'default'
  }
}

function statusLabel(status: string): string {
  switch (status) {
    case 'Completed': return 'مكتملة'
    case 'Running': return 'قيد التنفيذ'
    case 'Queued': return 'في الانتظار'
    case 'Failed': return 'فاشلة'
    default: return status
  }
}

function triggerLabel(trigger: string): string {
  switch (trigger) {
    case 'Manual': return 'يدوي'
    case 'Scheduled': return 'مجدول'
    case 'BeforeRestore': return 'قبل الاستعادة'
    default: return trigger
  }
}

onMounted(fetchData)
</script>

<template>
  <div class="backup-page">
    <PageHeader title="النسخ الاحتياطي واستعادة البيانات" subtitle="إدارة النسخ الاحتياطية لقاعدة البيانات">
      <template #actions>
        <v-btn
          variant="outlined"
          color="primary"
          prepend-icon="mdi-folder-open"
          @click="openFolder"
        >
          فتح المجلد
        </v-btn>
        <v-btn
          variant="outlined"
          color="primary"
          prepend-icon="mdi-refresh"
          :loading="loading"
          @click="fetchData"
        >
          تحديث
        </v-btn>
      </template>
    </PageHeader>

    <!-- Restore Status Banner -->
    <div v-if="restoreStatus && (restoreStatus.status === 'Queued' || restoreStatus.status === 'Running')" class="status-banner" :class="`status-${restoreStatus.status}`">
      <div class="status-banner-icon">
        <v-icon :icon="restoreStatus.status === 'Running' ? 'mdi-loading mdi-spin' : 'mdi-timer-sand'" color="white" size="24" />
      </div>
      <div class="status-banner-info">
        <p class="status-banner-title">عملية استعادة قيد التنفيذ</p>
        <p class="status-banner-desc">
          النسخة: {{ restoreStatus.backupFileName }} — الحالة: {{ statusLabel(restoreStatus.status) }}
        </p>
      </div>
    </div>

    <!-- Incomplete Backup Banner -->
    <div v-else-if="incompleteBackup" class="status-banner status-Queued">
      <div class="status-banner-icon">
        <v-icon icon="mdi-loading mdi-spin" color="white" size="24" />
      </div>
      <div class="status-banner-info">
        <p class="status-banner-title">نسخة احتياطية قيد الإنشاء</p>
        <p class="status-banner-desc">
          الملف: {{ incompleteBackup.fileName }} — الحالة: {{ statusLabel(incompleteBackup.status) }}
        </p>
      </div>
    </div>

    <!-- Action Cards -->
    <div class="action-grid">
      <!-- Create Backup Card -->
      <v-card elevation="0" class="action-card">
        <div class="card-header">
          <v-icon icon="mdi-database-arrow-up" color="primary" size="20" />
          <h3>إنشاء نسخة احتياطية</h3>
        </div>
        <v-divider />
        <div class="card-body">
          <div v-if="lastCompleted" class="last-backup-info">
            <div class="last-backup-header">آخر نسخة احتياطية</div>
            <div class="last-backup-row">
              <span class="info-label">التاريخ</span>
              <span class="info-value">{{ new Date(lastCompleted.completedAt ?? lastCompleted.createdAt).toLocaleDateString('ar-IQ', { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' }) }}</span>
            </div>
            <div class="last-backup-row">
              <span class="info-label">الحجم</span>
              <span class="info-value">{{ formatSize(lastCompleted.sizeBytes) }}</span>
            </div>
            <div class="last-backup-row">
              <span class="info-label">المشغل</span>
              <span class="info-value">{{ triggerLabel(lastCompleted.trigger) }}</span>
            </div>
          </div>
          <EmptyState
            v-else
            icon="mdi-database-off"
            title="لا توجد نسخ احتياطية"
            description="لم يتم إنشاء أي نسخة احتياطية بعد"
          />
          <v-btn
            color="primary"
            size="large"
            block
            prepend-icon="mdi-plus"
            :loading="creatingBackup"
            @click="createBackup"
          >
            {{ creatingBackup ? 'جاري الإنشاء...' : 'إنشاء نسخة احتياطية' }}
          </v-btn>
        </div>
      </v-card>

      <!-- Restore Backup Card -->
      <v-card elevation="0" class="action-card">
        <div class="card-header">
          <v-icon icon="mdi-database-arrow-down" color="warning" size="20" />
          <h3>استعادة قاعدة البيانات</h3>
        </div>
        <v-divider />
        <div class="card-body">
          <p class="restore-warning">
            <v-icon icon="mdi-alert" color="warning" size="16" />
            سيتم استبدال قاعدة البيانات الحالية بالنسخة الاحتياطية. لا يمكن التراجع عن هذه العملية.
          </p>

          <label class="toggle-row">
            <input v-model="useLatest" type="checkbox" class="toggle-native" />
            <span class="toggle-box">
              <v-icon v-if="useLatest" icon="mdi-check" size="12" color="white" />
            </span>
            <span class="toggle-label">استخدام أحدث نسخة احتياطية</span>
          </label>

          <div v-if="!useLatest" class="form-field">
            <label class="form-label">اختر النسخة الاحتياطية</label>
            <v-autocomplete
              v-model="selectedBackupId"
              :items="completedBackups.map(b => ({ value: b.id, label: `${b.fileName} - ${new Date(b.completedAt ?? b.createdAt).toLocaleDateString('ar-IQ')} - ${formatSize(b.sizeBytes)}` }))"
              item-title="label"
              item-value="value"
              class="form-select"
              density="compact"
              variant="outlined"
              hide-details
              placeholder="اختر نسخة"
            />
          </div>

          <div v-if="!useLatest && !completedBackups.length" class="no-backups-notice">
            لا توجد نسخ احتياطية مكتملة لاستعادتها.
          </div>

          <div class="form-field">
            <label class="form-label">كلمة المرور الحالية</label>
            <input v-model="password" type="password" class="form-input" placeholder="أدخل كلمة المرور للمصادقة" />
          </div>

          <v-btn
            color="warning"
            size="large"
            block
            prepend-icon="mdi-restore"
            :loading="restoring"
            :disabled="!password.trim() || (!useLatest && !selectedBackupId) || !completedBackups.length"
            @click="doRestore"
          >
            {{ restoring ? 'جاري الاستعادة...' : 'بدء الاستعادة' }}
          </v-btn>
        </div>
      </v-card>
    </div>

    <!-- Backups Table -->
    <v-card elevation="0" class="table-card">
      <div class="card-header">
        <v-icon icon="mdi-history" color="primary" size="20" />
        <h3>سجل النسخ الاحتياطية</h3>
        <v-chip size="small" color="primary" variant="tonal">{{ backups.length }}</v-chip>
      </div>
      <v-divider />

      <div v-if="loading" class="table-loading">
        <v-skeleton-loader v-for="i in 3" :key="i" type="table-row" />
      </div>

      <EmptyState
        v-else-if="!backups.length"
        icon="mdi-database-off"
        title="لا توجد نسخ احتياطية"
        description="لم يتم إنشاء أي نسخة احتياطية بعد"
      />

      <div v-else class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th>الملف</th>
              <th>الحالة</th>
              <th>المشغل</th>
              <th>الحجم</th>
              <th>التاريخ</th>
              <th>بواسطة</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="b in backups" :key="b.id" :class="{ 'row-completed': b.status === 'Completed', 'row-failed': b.status === 'Failed' }">
              <td>
                <div class="file-cell">
                  <v-icon icon="mdi-file-database" size="18" />
                  <span class="file-name">{{ b.fileName }}</span>
                </div>
              </td>
              <td>
                <v-chip size="small" :color="statusColor(b.status)" variant="tonal">
                  {{ statusLabel(b.status) }}
                </v-chip>
              </td>
              <td class="muted-cell">
                <v-chip size="x-small" variant="tonal">{{ triggerLabel(b.trigger) }}</v-chip>
              </td>
              <td class="muted-cell">{{ formatSize(b.sizeBytes) }}</td>
              <td class="muted-cell">
                <div class="date-cell">
                  <span>{{ new Date(b.createdAt).toLocaleDateString('ar-IQ', { year: 'numeric', month: 'short', day: 'numeric' }) }}</span>
                  <span class="time">{{ new Date(b.createdAt).toLocaleTimeString('ar-IQ', { hour: '2-digit', minute: '2-digit' }) }}</span>
                </div>
              </td>
              <td class="muted-cell">{{ b.requestedByUserName ?? 'النظام' }}</td>
              <td>
                <div class="row-actions">
                  <v-btn
                    icon
                    size="small"
                    variant="tonal"
                    color="primary"
                    aria-label="تحميل"
                    :disabled="b.status !== 'Completed'"
                    @click="downloadBackup(b.id)"
                  >
                    <v-icon icon="mdi-download" size="16" />
                  </v-btn>
                  <v-btn
                    icon
                    size="small"
                    variant="tonal"
                    color="error"
                    aria-label="حذف"
                    :disabled="b.status === 'Queued' || b.status === 'Running'"
                    @click="confirmDelete(b.id)"
                  >
                    <v-icon icon="mdi-delete" size="16" />
                  </v-btn>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </v-card>

    <!-- Delete Confirm Dialog -->
    <v-dialog v-model="deleteDialog" max-width="400">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-alert" color="error" size="22" />
          تأكيد الحذف
        </v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          هل أنت متأكد من حذف هذه النسخة الاحتياطية؟ لا يمكن التراجع عن هذه العملية.
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="deleteDialog = false">تراجع</v-btn>
          <v-btn color="error" @click="doDelete">تأكيد الحذف</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<style scoped>
.backup-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

/* Status Banner */
.status-banner {
  display: flex;
  gap: var(--spacing-lg);
  padding: var(--spacing-lg);
  border-radius: var(--radius-lg);
  align-items: center;
  animation: fadeInUp 0.35s ease both;
}

.status-Queued {
  background: linear-gradient(135deg, #fff8e1, #fffde7);
  border: 1px solid #ffd54f;
}

.status-Running {
  background: linear-gradient(135deg, #e3f2fd, #e8f5e9);
  border: 1px solid #64b5f6;
}

.status-Failed {
  background: linear-gradient(135deg, #ffebee, #fff3e0);
  border: 1px solid #ef9a9a;
}

.status-banner-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: rgba(0, 0, 0, 0.15);
  flex-shrink: 0;
}

.status-Queued .status-banner-icon { background: #f57f17; }
.status-Running .status-banner-icon { background: #1565c0; }
.status-Failed .status-banner-icon { background: #c62828; }

.status-banner-info { flex: 1; }

.status-banner-title {
  margin: 0 0 4px 0;
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text);
}

.status-banner-desc {
  margin: 0;
  font-size: 13px;
  color: var(--color-text-muted);
}

/* Action Grid */
.action-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-lg);
}

.action-card,
.table-card {
  border: 1px solid var(--color-border) !important;
  border-radius: var(--radius-lg) !important;
  overflow: hidden;
}

.card-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-lg);
  background: var(--color-surface);
}

.card-header h3 {
  flex: 1;
  margin: 0;
  font-size: 15px;
  font-weight: 700;
  color: var(--color-text);
}

.card-body {
  padding: var(--spacing-xl);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

/* Last Backup Info */
.last-backup-info {
  padding: var(--spacing-md);
  background: var(--color-primary-soft);
  border: 1px solid var(--color-primary-light);
  border-radius: var(--radius-md);
}

.last-backup-header {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-primary);
  margin-bottom: var(--spacing-md);
}

.last-backup-row {
  display: flex;
  justify-content: space-between;
  padding: 4px 0;
  font-size: 13px;
}

.last-backup-row + .last-backup-row {
  border-top: 1px solid var(--color-primary-light);
}

.info-label { color: var(--color-text-muted); }
.info-value { font-weight: 600; color: var(--color-text); }

/* Restore */
.restore-warning {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-md);
  background: #fff8e1;
  border: 1px solid #ffd54f;
  border-radius: var(--radius-md);
  font-size: 13px;
  color: var(--color-text);
  margin: 0;
}

.no-backups-notice {
  padding: var(--spacing-md);
  background: var(--color-background);
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-md);
  font-size: 13px;
  color: var(--color-text-muted);
  text-align: center;
}

/* Toggle */
.toggle-row {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  cursor: pointer;
  user-select: none;
}

.toggle-native {
  position: absolute;
  opacity: 0;
  width: 0;
  height: 0;
}

.toggle-box {
  width: 18px;
  height: 18px;
  border: 2px solid var(--color-border);
  border-radius: 4px;
  background: white;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: all 0.15s ease;
}

.toggle-native:checked ~ .toggle-box {
  background: var(--color-primary);
  border-color: var(--color-primary);
}

.toggle-label {
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text);
}

/* Form Fields */
.form-field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text);
}

.form-input,
.form-select {
  padding: 10px 12px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s;
  width: 100%;
}

.form-input:focus,
.form-select:focus { border-color: var(--color-primary); }

.form-select {
  appearance: auto;
}

/* Table */
.table-loading { padding: var(--spacing-lg); }

.table-scroll { overflow-x: auto; }

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

.data-table tbody tr:last-child td { border-bottom: none; }
.data-table tbody tr:hover { background: var(--color-background); }

.row-completed td { background: var(--color-primary-soft); }
.row-completed:hover td { background: var(--color-primary-soft); }
.row-failed td { background: #fff5f5; }
.row-failed:hover td { background: #fff5f5; }

.file-cell {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.file-name {
  font-weight: 600;
  direction: ltr;
  display: inline-block;
}

.muted-cell { color: var(--color-text-muted); }

.date-cell {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.date-cell .time {
  font-size: 11px;
  color: var(--color-text-muted);
}

.row-actions {
  display: flex;
  gap: var(--spacing-sm);
}

/* Dialog */
.dialog-title {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-lg) !important;
  font-size: 16px !important;
  font-weight: 700;
}

.dialog-body { padding: var(--spacing-lg) !important; font-size: 14px; }

.dialog-actions {
  padding: var(--spacing-lg) !important;
  gap: var(--spacing-md);
  justify-content: flex-end;
}

/* Responsive */
@media (max-width: 1100px) {
  .action-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 600px) {
  .status-banner { flex-direction: column; text-align: center; }
}
</style>
