<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import api from '../services/api'
import { useNotifications } from '../composables/useNotifications'
import type { ApiResponse, DoctorFeature, DoctorItem, DoctorSubscription, PageResult, SubscriptionPackage } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import EmptyState from '../components/common/Emptystate.vue'

type Tab = 'subscriptions' | 'packages' | 'features'
type Confirmation = { title: string; text: string; action: () => Promise<void> }

const { success: showSuccess, error: showError } = useNotifications()

const activeTab     = ref<Tab>('subscriptions')
const subscriptions = ref<DoctorSubscription[]>([])
const packages      = ref<SubscriptionPackage[]>([])
const doctors       = ref<DoctorItem[]>([])
const features      = ref<DoctorFeature[]>([])
const loading       = ref(false)
const page          = ref(1)
const totalPages    = ref(1)
const totalItems    = ref(0)
const packagePage   = ref(1)
const packagePageSize = 6

const modal                = ref<'create' | 'renew' | 'upgrade' | 'editPackage' | 'confirm'>()
const selectedSubscription = ref<DoctorSubscription>()
const confirmation         = ref<Confirmation>()
const renewYearly          = ref(false)
const upgradePackageId     = ref('')
const featureDoctorId      = ref('')

const filters = reactive({ doctorId: '', packageId: '', status: '', isActive: '' })
const createForm = reactive({ doctorId: '', packageId: '', isYearly: false, status: '0' })
const editPackageForm = reactive({
  id: 0, name: '', normalizedName: '', price: 0, yearlyPrice: 0,
  maxClinics: 0, maxWeeklyDays: 0, maxDailyAppointments: 0,
  showReviews: false, showMessages: false, eBooking: false,
  ePayments: false, makeOffers: false, maxActiveOffers: 0,
})

const statusOptions = [
  { value: '0', label: 'نشط' }, { value: '1', label: 'قيد الانتظار' },
  { value: '2', label: 'منتهي' }, { value: '3', label: 'ملغي' },
]

const packageTotalPages  = computed(() => Math.max(1, Math.ceil(packages.value.length / packagePageSize)))
const paginatedPackages  = computed(() => packages.value.slice((packagePage.value - 1) * packagePageSize, packagePage.value * packagePageSize))
const enabledFeatures    = computed(() => features.value.filter((f) => f.isEnabled).length)

function statusMeta(status: number) {
  return [
    { label: 'نشط',            color: 'success' },
    { label: 'قيد الانتظار',  color: 'warning' },
    { label: 'منتهي',          color: 'default' },
    { label: 'ملغي',           color: 'error'   },
  ][status] ?? { label: 'غير معروف', color: 'default' }
}

function formatDate(d?: string) {
  return d ? new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium' }).format(new Date(d)) : '-'
}

function money(v: number) {
  return new Intl.NumberFormat('ar-IQ').format(v)
}

// API
async function loadLookups() {
  const [pkgRes, docRes] = await Promise.all([
    api.get<ApiResponse<PageResult<SubscriptionPackage>>>('/SubscriptionPackages', { params: { page: 1, pageSize: 100 } }),
    api.get<ApiResponse<PageResult<DoctorItem>>>('/Doctor', { params: { page: 1, pageSize: 100 } }),
  ])
  packages.value = pkgRes.data.data.items
  doctors.value  = docRes.data.data.items
}

async function loadSubscriptions() {
  loading.value = true
  try {
    const r = await api.get<ApiResponse<PageResult<DoctorSubscription>>>('/DoctorSubscription', {
      params: {
        doctorId:  filters.doctorId  || undefined,
        packageId: filters.packageId || undefined,
        status:    filters.status === '' ? undefined : filters.status,
        isActive:  filters.isActive  === '' ? undefined : filters.isActive,
        page: page.value, pageSize: 10,
      },
    })
    subscriptions.value = r.data.data.items
    totalPages.value    = r.data.data.totalPages
    totalItems.value    = r.data.data.totalItems
  } catch (e: any) {
    if (e.response?.status === 404) { subscriptions.value = []; totalPages.value = 1; totalItems.value = 0 }
    else showError(getErrorMessage(e))
  } finally { loading.value = false }
}

async function loadFeatures() {
  loading.value = true
  try {
    const r = await api.get<ApiResponse<PageResult<DoctorFeature>>>('/DoctorFeature', {
      params: { doctorId: featureDoctorId.value || undefined, page: page.value, pageSize: 10 },
    })
    features.value   = r.data.data.items
    totalPages.value = r.data.data.totalPages
    totalItems.value = r.data.data.totalItems
  } catch (e: any) {
    if (e.response?.status === 404) { features.value = []; totalPages.value = 1; totalItems.value = 0 }
    else showError(getErrorMessage(e))
  } finally { loading.value = false }
}

async function initialize() {
  loading.value = true
  try { await loadLookups(); await loadSubscriptions() }
  catch (e: any) { if (e.response?.status !== 404) showError(getErrorMessage(e)) }
  finally { loading.value = false }
}

function selectTab(tab: Tab) {
  activeTab.value = tab; page.value = 1
  if (tab === 'packages')       packagePage.value = 1
  if (tab === 'subscriptions')  loadSubscriptions()
  if (tab === 'features')       loadFeatures()
}

function openEditPackage(item: SubscriptionPackage) {
  Object.assign(editPackageForm, { ...item })
  modal.value = 'editPackage'
}

async function updatePackage() {
  try {
    const r = await api.put<ApiResponse<object>>('/SubscriptionPackages', { ...editPackageForm })
    showSuccess(r.data.message); modal.value = undefined; await loadLookups()
  } catch (e) { showError(getErrorMessage(e)) }
}

async function createSubscription() {
  try {
    const r = await api.post<ApiResponse<object>>('/DoctorSubscription', {
      doctorId: Number(createForm.doctorId), packageId: Number(createForm.packageId),
      isYearly: createForm.isYearly, status: Number(createForm.status),
    })
    showSuccess(r.data.message); modal.value = undefined
    Object.assign(createForm, { doctorId: '', packageId: '', isYearly: false, status: '0' })
    await loadSubscriptions()
  } catch (e) { showError(getErrorMessage(e)) }
}

async function activate(sub: DoctorSubscription) {
  try {
    const r = await api.post<ApiResponse<object>>(`/DoctorSubscription/${sub.id}/activate`)
    showSuccess(r.data.message); await loadSubscriptions()
  } catch (e) { showError(getErrorMessage(e)) }
}

function openRenew(sub: DoctorSubscription) { selectedSubscription.value = sub; renewYearly.value = false; modal.value = 'renew' }

async function renew() {
  if (!selectedSubscription.value) return
  try {
    const r = await api.post<ApiResponse<object>>(`/DoctorSubscription/${selectedSubscription.value.id}/renew`, { isYearly: renewYearly.value })
    showSuccess(r.data.message); modal.value = undefined; await loadSubscriptions()
  } catch (e) { showError(getErrorMessage(e)) }
}

function openUpgrade(sub: DoctorSubscription) { selectedSubscription.value = sub; upgradePackageId.value = ''; modal.value = 'upgrade' }

async function upgrade() {
  if (!selectedSubscription.value) return
  try {
    const r = await api.post<ApiResponse<object>>(`/DoctorSubscription/${selectedSubscription.value.id}/upgrade`, { packageId: Number(upgradePackageId.value) })
    showSuccess(r.data.message); modal.value = undefined; await loadSubscriptions()
  } catch (e) { showError(getErrorMessage(e)) }
}

function askCancel(sub: DoctorSubscription) {
  confirmation.value = {
    title: 'إلغاء الاشتراك',
    text: `سيتم إلغاء اشتراك الطبيب ${sub.doctor.name} وتعطيل مميزاته إذا لم يكن لديه اشتراك فعّال آخر.`,
    action: async () => {
      const r = await api.delete<ApiResponse<object>>(`/DoctorSubscription/${sub.id}`)
      showSuccess(r.data.message); await loadSubscriptions()
    },
  }
  modal.value = 'confirm'
}

async function runConfirmation() {
  if (!confirmation.value) return
  try { await confirmation.value.action(); modal.value = undefined; confirmation.value = undefined }
  catch (e) { showError(getErrorMessage(e)) }
}

async function toggleFeature(feature: DoctorFeature) {
  try {
    const r = await api.post<ApiResponse<object>>(`/DoctorFeature/${feature.id}/toggle`)
    showSuccess(r.data.message); await loadFeatures()
  } catch (e) { showError(getErrorMessage(e)) }
}

function applyFilters() { page.value = 1; activeTab.value === 'features' ? loadFeatures() : loadSubscriptions() }
function changePage(n: number) { page.value = n; activeTab.value === 'features' ? loadFeatures() : loadSubscriptions() }

onMounted(initialize)
</script>

<template>
  <div class="subscriptions-page">

    <!-- Header -->
    <div class="page-top">
      <div>
        <p class="page-kicker">إدارة الخدمات</p>
        <h1 class="page-title">الاشتراكات والباقات</h1>
      </div>
      <v-btn color="primary" prepend-icon="mdi-plus" @click="modal = 'create'">اشتراك جديد</v-btn>
    </div>

    <!-- Tabs -->
    <div class="tabs-bar">
      <button class="tab-btn" :class="{ 'tab-active': activeTab === 'subscriptions' }" @click="selectTab('subscriptions')">
        <v-icon icon="mdi-calendar-clock" size="17" /> الاشتراكات
      </button>
      <button class="tab-btn" :class="{ 'tab-active': activeTab === 'packages' }" @click="selectTab('packages')">
        <v-icon icon="mdi-package-variant" size="17" /> الباقات
      </button>
      <button class="tab-btn" :class="{ 'tab-active': activeTab === 'features' }" @click="selectTab('features')">
        <v-icon icon="mdi-star-circle" size="17" /> المميزات
      </button>
    </div>

    <!-- ── Subscriptions Tab ── -->
    <template v-if="activeTab === 'subscriptions'">
      <!-- Filters -->
      <div class="filters-bar">
        <div class="filter-field">
          <label class="filter-label">الطبيب</label>
          <v-autocomplete
            v-model="filters.doctorId"
            :items="[{ value: '', label: 'كل الأطباء' }, ...doctors.map(d => ({ value: d.id, label: d.name }))]"
            item-title="label"
            item-value="value"
            class="filter-select"
            density="compact"
            variant="outlined"
            hide-details
          />
        </div>
        <div class="filter-field">
          <label class="filter-label">الباقة</label>
          <v-autocomplete
            v-model="filters.packageId"
            :items="[{ value: '', label: 'كل الباقات' }, ...packages.map(p => ({ value: p.id, label: p.name }))]"
            item-title="label"
            item-value="value"
            class="filter-select"
            density="compact"
            variant="outlined"
            hide-details
          />
        </div>
        <div class="filter-field">
          <label class="filter-label">الحالة</label>
          <v-autocomplete
            v-model="filters.status"
            :items="statusOptions"
            item-title="label"
            item-value="value"
            class="filter-select"
            density="compact"
            variant="outlined"
            hide-details
          />
        </div>
        <div class="filter-field">
          <label class="filter-label">الفعالية</label>
          <v-autocomplete
            v-model="filters.isActive"
            :items="[
              { value: '', label: 'الكل' },
              { value: 'true', label: 'مفعلة' },
              { value: 'false', label: 'متوقفة' },
            ]"
            item-title="label"
            item-value="value"
            class="filter-select"
            density="compact"
            variant="outlined"
            hide-details
          />
        </div>
        <v-btn color="primary" prepend-icon="mdi-magnify" class="filter-btn" @click="applyFilters">تطبيق</v-btn>
      </div>

      <!-- Table -->
      <v-card elevation="0" class="table-card">
        <div class="table-toolbar">
          <v-icon icon="mdi-calendar-check" color="primary" size="20" />
          <strong>الاشتراكات</strong>
          <v-chip size="small" color="primary" variant="tonal">{{ totalItems }}</v-chip>
          <v-btn variant="text" size="small" prepend-icon="mdi-refresh" :loading="loading" @click="loadSubscriptions">تحديث</v-btn>
        </div>

        <div v-if="loading" class="table-loading">
          <v-skeleton-loader v-for="i in 4" :key="i" type="table-row" />
        </div>
        <EmptyState v-else-if="!subscriptions.length" icon="mdi-calendar-blank" title="لا توجد اشتراكات" description="لا توجد اشتراكات مطابقة للفلاتر" />

        <div v-else class="table-scroll">
          <table class="data-table">
            <thead>
              <tr>
                <th>الطبيب</th>
                <th>الباقة</th>
                <th>المدة</th>
                <th>الحالة</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="sub in subscriptions" :key="sub.id">
                <td>
                  <strong>{{ sub.doctor.name }}</strong>
                  <p class="row-sub">{{ sub.doctor.specialization.name }}</p>
                </td>
                <td>
                  <strong>{{ sub.package.name }}</strong>
                  <p class="row-sub">{{ money(sub.package.price) }} د.ع / شهر</p>
                </td>
                <td>
                  <strong>{{ formatDate(sub.startDate) }}</strong>
                  <p class="row-sub">حتى {{ formatDate(sub.endDate) }}</p>
                </td>
                <td>
                  <v-chip size="small" :color="statusMeta(sub.status).color" variant="tonal">
                    {{ statusMeta(sub.status).label }}
                  </v-chip>
                </td>
                <td>
                  <div class="row-actions">
                    <v-btn v-if="sub.status === 1" icon size="small" variant="tonal" color="success" aria-label="تفعيل" @click="activate(sub)">
                      <v-icon icon="mdi-check" size="16" />
                    </v-btn>
                    <v-btn v-if="sub.status !== 3" icon size="small" variant="tonal" color="primary" aria-label="تجديد" @click="openRenew(sub)">
                      <v-icon icon="mdi-refresh" size="16" />
                    </v-btn>
                    <v-btn v-if="sub.status === 0 && sub.isActive" icon size="small" variant="tonal" color="info" aria-label="ترقية" @click="openUpgrade(sub)">
                      <v-icon icon="mdi-trending-up" size="16" />
                    </v-btn>
                    <v-btn v-if="sub.status !== 3" icon size="small" variant="tonal" color="error" aria-label="إلغاء" @click="askCancel(sub)">
                      <v-icon icon="mdi-close-circle" size="16" />
                    </v-btn>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="totalPages > 1" class="pagination-bar">
          <v-pagination v-model="page" :length="totalPages" :total-visible="5" density="compact" color="primary" @update:model-value="changePage" />
        </div>
      </v-card>
    </template>

    <!-- ── Packages Tab ── -->
    <template v-if="activeTab === 'packages'">
      <EmptyState v-if="!packages.length" icon="mdi-package-variant" title="لا توجد باقات" />

      <div v-else class="packages-grid">
        <div v-for="item in paginatedPackages" :key="item.id" class="package-card">
          <!-- Header -->
          <div class="package-header">
            <div class="package-icon">
              <v-icon icon="mdi-crown" color="primary" size="22" />
            </div>
            <div class="package-title">
              <h3>{{ item.name }}</h3>
              <p>{{ item.normalizedName }}</p>
            </div>
            <v-btn icon size="small" variant="text" color="primary" @click="openEditPackage(item)">
              <v-icon icon="mdi-tune" size="18" />
            </v-btn>
          </div>

          <!-- Price -->
          <div class="package-price">
            <strong>{{ money(item.price) }}</strong>
            <span>د.ع / شهر</span>
          </div>
          <p class="package-yearly">{{ money(item.yearlyPrice) }} د.ع سنوياً</p>

          <!-- Limits -->
          <div class="package-limits">
            <span><v-icon icon="mdi-hospital-building" size="14" /> {{ item.maxClinics }} عيادات</span>
            <span><v-icon icon="mdi-calendar-week" size="14" /> {{ item.maxWeeklyDays }} أيام</span>
            <span><v-icon icon="mdi-account-group" size="14" /> {{ item.maxDailyAppointments }} دور/يوم</span>
          </div>

          <v-divider class="my-3" />

          <!-- Features -->
          <div class="package-features">
            <span class="feature-chip" :class="{ 'feature-chip--on': item.eBooking }">
              <v-icon :icon="item.eBooking ? 'mdi-check' : 'mdi-close'" size="12" />
              الحجز الإلكتروني
            </span>
            <span class="feature-chip" :class="{ 'feature-chip--on': item.showReviews }">
              <v-icon :icon="item.showReviews ? 'mdi-check' : 'mdi-close'" size="12" />
              التقييمات
            </span>
            <span class="feature-chip" :class="{ 'feature-chip--on': item.showMessages }">
              <v-icon :icon="item.showMessages ? 'mdi-check' : 'mdi-close'" size="12" />
              الرسائل
            </span>
            <span class="feature-chip" :class="{ 'feature-chip--on': item.ePayments }">
              <v-icon :icon="item.ePayments ? 'mdi-check' : 'mdi-close'" size="12" />
              الدفع الإلكتروني
            </span>
            <span class="feature-chip" :class="{ 'feature-chip--on': item.makeOffers }">
              <v-icon :icon="item.makeOffers ? 'mdi-check' : 'mdi-close'" size="12" />
              العروض
            </span>
          </div>
        </div>
      </div>

      <div v-if="packageTotalPages > 1" class="pagination-bar">
        <v-pagination v-model="packagePage" :length="packageTotalPages" :total-visible="5" density="compact" color="primary" />
      </div>
    </template>

    <!-- ── Features Tab ── -->
    <template v-if="activeTab === 'features'">
      <div class="filters-bar">
        <div class="filter-field">
          <label class="filter-label">الطبيب</label>
          <v-autocomplete
            v-model="featureDoctorId"
            :items="[{ value: '', label: 'اختر الطبيب' }, ...doctors.map(d => ({ value: d.id, label: d.name }))]"
            item-title="label"
            item-value="value"
            class="filter-select"
            density="compact"
            variant="outlined"
            hide-details
          />
        </div>
        <v-btn color="primary" prepend-icon="mdi-magnify" class="filter-btn" @click="applyFilters">تطبيق</v-btn>
      </div>

      <v-card elevation="0" class="table-card">
        <div class="table-toolbar">
          <v-icon icon="mdi-star-circle" color="primary" size="20" />
          <strong>المميزات</strong>
          <v-chip size="small" color="success" variant="tonal">{{ enabledFeatures }} مفعّلة</v-chip>
          <v-btn variant="text" size="small" prepend-icon="mdi-refresh" :loading="loading" @click="loadFeatures">تحديث</v-btn>
        </div>

        <div v-if="loading" class="table-loading">
          <v-skeleton-loader v-for="i in 4" :key="i" type="table-row" />
        </div>
        <EmptyState v-else-if="!features.length" icon="mdi-star-off" title="لا توجد مميزات" />

        <div v-else class="table-scroll">
          <table class="data-table">
            <thead>
              <tr>
                <th>الطبيب</th>
                <th>الميزة</th>
                <th>الوصف</th>
                <th>الحالة</th>
                <th>التحكم</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="feature in features" :key="feature.id">
                <td>
                  <strong>{{ feature.doctor.name }}</strong>
                  <p class="row-sub">{{ feature.doctor.specialization.name }}</p>
                </td>
                <td>
                  <strong>{{ feature.feature.name }}</strong>
                  <p class="row-sub">{{ feature.feature.normalizedName }}</p>
                </td>
                <td class="muted-cell">{{ feature.feature.description || '-' }}</td>
                <td>
                  <v-chip size="small" :color="feature.isEnabled ? 'success' : 'default'" variant="tonal">
                    {{ feature.isEnabled ? 'مفعّلة' : 'معطّلة' }}
                  </v-chip>
                </td>
                <td>
                  <v-btn
                    size="small"
                    :color="feature.isEnabled ? 'warning' : 'success'"
                    variant="tonal"
                    :prepend-icon="feature.isEnabled ? 'mdi-toggle-switch-off' : 'mdi-toggle-switch'"
                    @click="toggleFeature(feature)"
                  >
                    {{ feature.isEnabled ? 'تعطيل' : 'تفعيل' }}
                  </v-btn>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="totalPages > 1" class="pagination-bar">
          <v-pagination v-model="page" :length="totalPages" :total-visible="5" density="compact" color="primary" @update:model-value="changePage" />
        </div>
      </v-card>
    </template>

    <!-- ── Create Dialog ── -->
    <v-dialog :model-value="modal === 'create'" max-width="480" @update:model-value="modal = undefined">
      <v-card>
        <v-card-title class="dialog-title"><v-icon icon="mdi-plus" color="primary" size="20" />اشتراك جديد</v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          <div class="form-fields">
            <div class="form-field">
              <label class="form-label">الطبيب <span class="required">*</span></label>
              <v-autocomplete
                v-model="createForm.doctorId"
                :items="doctors.map(d => ({ value: d.id, label: d.name }))"
                item-title="label"
                item-value="value"
                class="form-select"
                density="compact"
                variant="outlined"
                hide-details
                placeholder="اختر الطبيب"
              />
            </div>
            <div class="form-field">
              <label class="form-label">الباقة <span class="required">*</span></label>
              <v-autocomplete
                v-model="createForm.packageId"
                :items="packages.map(p => ({ value: p.id, label: p.name }))"
                item-title="label"
                item-value="value"
                class="form-select"
                density="compact"
                variant="outlined"
                hide-details
                placeholder="اختر الباقة"
              />
            </div>
            <div class="form-field">
              <label class="form-label">الحالة الابتدائية</label>
              <v-autocomplete
                v-model="createForm.status"
                :items="statusOptions"
                item-title="label"
                item-value="value"
                class="form-select"
                density="compact"
                variant="outlined"
                hide-details
              />
            </div>
            <label class="check-label">
              <input v-model="createForm.isYearly" type="checkbox" class="check-native" />
              <span class="check-box"><v-icon v-if="createForm.isYearly" icon="mdi-check" size="12" color="white" /></span>
              <span>اشتراك سنوي</span>
            </label>
          </div>
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="modal = undefined">تراجع</v-btn>
          <v-btn color="primary" @click="createSubscription">إنشاء الاشتراك</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Renew Dialog ── -->
    <v-dialog :model-value="modal === 'renew'" max-width="420" @update:model-value="modal = undefined">
      <v-card>
        <v-card-title class="dialog-title"><v-icon icon="mdi-refresh" color="primary" size="20" />تجديد الاشتراك</v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          <p class="dialog-desc">سيتم تجديد اشتراك الطبيب <strong>{{ selectedSubscription?.doctor.name }}</strong> على باقة <strong>{{ selectedSubscription?.package.name }}</strong>.</p>
          <label class="check-label">
            <input v-model="renewYearly" type="checkbox" class="check-native" />
            <span class="check-box"><v-icon v-if="renewYearly" icon="mdi-check" size="12" color="white" /></span>
            <span>تجديد سنوي بدلاً من شهر واحد</span>
          </label>
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="modal = undefined">تراجع</v-btn>
          <v-btn color="primary" @click="renew">تجديد</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Upgrade Dialog ── -->
    <v-dialog :model-value="modal === 'upgrade'" max-width="420" @update:model-value="modal = undefined">
      <v-card>
        <v-card-title class="dialog-title"><v-icon icon="mdi-trending-up" color="primary" size="20" />ترقية الباقة</v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          <p class="dialog-desc">اختر باقة أعلى لاشتراك الطبيب <strong>{{ selectedSubscription?.doctor.name }}</strong>.</p>
          <div class="form-field">
            <label class="form-label">الباقة الجديدة</label>
            <v-autocomplete
              v-model="upgradePackageId"
              :items="packages.map(p => ({ value: p.id, label: p.name }))"
              item-title="label"
              item-value="value"
              class="form-select"
              density="compact"
              variant="outlined"
              hide-details
              placeholder="اختر الباقة"
            />
          </div>
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="modal = undefined">تراجع</v-btn>
          <v-btn color="primary" :disabled="!upgradePackageId" @click="upgrade">ترقية</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Edit Package Dialog ── -->
    <v-dialog :model-value="modal === 'editPackage'" max-width="600" scrollable @update:model-value="modal = undefined">
      <v-card>
        <v-card-title class="dialog-title"><v-icon icon="mdi-tune" color="primary" size="20" />تعديل الباقة</v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          <div class="form-grid">
            <div class="form-field"><label class="form-label">اسم الباقة</label><input v-model="editPackageForm.name" class="form-input" required /></div>
            <div class="form-field"><label class="form-label">الاسم الموحد</label><input v-model="editPackageForm.normalizedName" class="form-input" required /></div>
            <div class="form-field"><label class="form-label">السعر الشهري</label><input v-model.number="editPackageForm.price" type="number" min="0" class="form-input" required /></div>
            <div class="form-field"><label class="form-label">السعر السنوي</label><input v-model.number="editPackageForm.yearlyPrice" type="number" min="0" class="form-input" required /></div>
            <div class="form-field"><label class="form-label">عدد العيادات</label><input v-model.number="editPackageForm.maxClinics" type="number" min="0" class="form-input" required /></div>
            <div class="form-field"><label class="form-label">أيام الاستقبال</label><input v-model.number="editPackageForm.maxWeeklyDays" type="number" min="0" class="form-input" required /></div>
            <div class="form-field"><label class="form-label">الحجوزات اليومية</label><input v-model.number="editPackageForm.maxDailyAppointments" type="number" min="0" class="form-input" required /></div>
            <div class="form-field"><label class="form-label">الحد الأقصى للعروض</label><input v-model.number="editPackageForm.maxActiveOffers" type="number" min="0" class="form-input" required /></div>
            <div class="form-field form-field--full">
              <label class="form-label">المميزات</label>
              <div class="features-checks">
                <label class="check-label" v-for="(label, key) in { eBooking: 'الحجز الإلكتروني', showReviews: 'التقييمات', showMessages: 'الرسائل', ePayments: 'الدفع الإلكتروني', makeOffers: 'العروض' }" :key="key">
                  <input v-model="(editPackageForm as any)[key]" type="checkbox" class="check-native" />
                  <span class="check-box"><v-icon v-if="(editPackageForm as any)[key]" icon="mdi-check" size="12" color="white" /></span>
                  <span>{{ label }}</span>
                </label>
              </div>
            </div>
          </div>
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="modal = undefined">تراجع</v-btn>
          <v-btn color="primary" @click="updatePackage">حفظ التعديلات</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Confirm Dialog ── -->
    <v-dialog :model-value="modal === 'confirm'" max-width="420" @update:model-value="modal = undefined">
      <v-card>
        <v-card-title class="dialog-title"><v-icon icon="mdi-alert" color="error" size="20" />{{ confirmation?.title }}</v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">{{ confirmation?.text }}</v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="modal = undefined">تراجع</v-btn>
          <v-btn color="error" @click="runConfirmation">تأكيد الإلغاء</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </div>
</template>

<style scoped>
.subscriptions-page { display: flex; flex-direction: column; gap: var(--spacing-lg); }

/* Page Top */
.page-top { display: flex; align-items: center; justify-content: space-between; gap: var(--spacing-lg); flex-wrap: wrap; }
.page-kicker { margin: 0 0 4px 0; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--color-text-muted); }
.page-title { margin: 0; font-size: 28px; font-weight: 800; color: var(--color-text); }

/* Tabs */
.tabs-bar { display: flex; gap: 6px; padding: 6px; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-lg); box-shadow: var(--shadow-sm); flex-wrap: wrap; }
.tab-btn { display: inline-flex; align-items: center; gap: 7px; padding: 9px 14px; border: none; border-radius: var(--radius-md); background: transparent; color: var(--color-text-muted); font-family: var(--font-family-primary); font-size: 13px; font-weight: 600; cursor: pointer; white-space: nowrap; transition: all 0.15s ease; }
.tab-btn:hover { background: var(--color-primary-soft); color: var(--color-primary); }
.tab-active { background: var(--color-primary-soft) !important; color: var(--color-primary) !important; font-weight: 700; }

/* Filters */
.filters-bar { display: flex; align-items: flex-end; gap: var(--spacing-md); flex-wrap: wrap; padding: var(--spacing-lg); background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-lg); box-shadow: var(--shadow-sm); }
.filter-field { display: flex; flex-direction: column; gap: 6px; }
.filter-label { font-size: 12px; font-weight: 700; color: var(--color-text-muted); }
.filter-select { height: 40px; padding: 0 12px; border: 1.5px solid var(--color-border); border-radius: var(--radius-md); background: var(--color-surface); color: var(--color-text); font-family: var(--font-family-primary); font-size: 14px; outline: none; min-width: 150px; transition: border-color 0.2s; }
.filter-select:focus { border-color: var(--color-primary); }
.filter-btn { align-self: flex-end; }

/* Table */
.table-card { border: 1px solid var(--color-border) !important; border-radius: var(--radius-lg) !important; overflow: hidden; }
.table-toolbar { display: flex; align-items: center; gap: var(--spacing-md); padding: var(--spacing-lg); border-bottom: 1px solid var(--color-border); }
.table-toolbar strong { flex: 1; font-size: 15px; color: var(--color-text); }
.table-loading { padding: var(--spacing-lg); }
.table-scroll { overflow-x: auto; }
.data-table { width: 100%; border-collapse: collapse; font-size: 13px; }
.data-table th { padding: 12px 16px; text-align: right; font-size: 12px; font-weight: 700; color: var(--color-text-muted); background: var(--color-background); border-bottom: 1px solid var(--color-border); white-space: nowrap; }
.data-table td { padding: 12px 16px; border-bottom: 1px solid var(--color-border-light); vertical-align: middle; color: var(--color-text); }
.data-table tbody tr:hover { background: var(--color-background); }
.data-table tbody tr:last-child td { border-bottom: none; }
.row-sub { margin: 2px 0 0 0; font-size: 11px; color: var(--color-text-muted); }
.muted-cell { color: var(--color-text-muted); font-size: 12px; }
.row-actions { display: flex; gap: 4px; }
.pagination-bar { display: flex; justify-content: center; padding: var(--spacing-lg); border-top: 1px solid var(--color-border); }

/* Packages Grid */
.packages-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: var(--spacing-lg); }
.package-card { background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-lg); padding: var(--spacing-lg); box-shadow: var(--shadow-sm); display: flex; flex-direction: column; gap: var(--spacing-md); transition: box-shadow 0.2s ease; }
.package-card:hover { box-shadow: var(--shadow-md); }
.package-header { display: flex; align-items: center; gap: var(--spacing-md); }
.package-icon { display: flex; align-items: center; justify-content: center; width: 40px; height: 40px; border-radius: var(--radius-md); background: var(--color-primary-soft); flex-shrink: 0; }
.package-title { flex: 1; min-width: 0; }
.package-title h3 { margin: 0; font-size: 15px; font-weight: 700; color: var(--color-text); }
.package-title p { margin: 0; font-size: 11px; color: var(--color-text-muted); }
.package-price { display: flex; align-items: baseline; gap: 6px; }
.package-price strong { font-size: 26px; font-weight: 800; color: var(--color-primary); }
.package-price span { font-size: 13px; color: var(--color-text-muted); }
.package-yearly { margin: 0; font-size: 12px; color: var(--color-text-muted); }
.package-limits { display: flex; gap: var(--spacing-sm); flex-wrap: wrap; }
.package-limits span { display: inline-flex; align-items: center; gap: 4px; padding: 4px 10px; background: var(--color-background); border: 1px solid var(--color-border); border-radius: 999px; font-size: 11px; font-weight: 600; color: var(--color-text); }
.package-features { display: flex; flex-wrap: wrap; gap: 6px; }
.feature-chip { display: inline-flex; align-items: center; gap: 4px; padding: 3px 8px; border-radius: 999px; font-size: 11px; font-weight: 600; background: var(--color-background); border: 1px solid var(--color-border); color: var(--color-text-muted); }
.feature-chip--on { background: var(--color-success-light); border-color: var(--color-success); color: var(--color-success); }

/* Dialog */
.dialog-title { display: flex; align-items: center; gap: var(--spacing-md); padding: var(--spacing-lg) !important; font-size: 16px !important; font-weight: 700; color: var(--color-text); }
.dialog-body { padding: var(--spacing-lg) !important; }
.dialog-desc { margin: 0 0 var(--spacing-lg) 0; font-size: 14px; color: var(--color-text-muted); line-height: 1.6; }
.dialog-actions { padding: var(--spacing-lg) !important; gap: var(--spacing-md); justify-content: flex-end; }

/* Form */
.form-fields { display: flex; flex-direction: column; gap: var(--spacing-lg); }
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--spacing-lg); }
.form-field { display: flex; flex-direction: column; gap: 6px; }
.form-field--full { grid-column: 1 / -1; }
.form-label { font-size: 13px; font-weight: 600; color: var(--color-text); }
.required { color: var(--color-error); }
.form-input, .form-select { padding: 10px 12px; border: 1.5px solid var(--color-border); border-radius: var(--radius-md); background: var(--color-surface); color: var(--color-text); font-family: var(--font-family-primary); font-size: 14px; outline: none; width: 100%; transition: border-color 0.2s; }
.form-input:focus, .form-select:focus { border-color: var(--color-primary); }
.features-checks { display: flex; flex-wrap: wrap; gap: var(--spacing-lg); padding: var(--spacing-md); background: var(--color-background); border-radius: var(--radius-md); }

/* Checkbox */
.check-label { display: flex; align-items: center; gap: var(--spacing-md); cursor: pointer; font-size: 14px; font-weight: 500; color: var(--color-text); user-select: none; }
.check-native { position: absolute; opacity: 0; width: 0; height: 0; }
.check-box { width: 18px; height: 18px; border: 2px solid var(--color-border); border-radius: 4px; background: white; display: flex; align-items: center; justify-content: center; flex-shrink: 0; transition: all 0.15s ease; }
.check-native:checked ~ .check-box { background: var(--color-primary); border-color: var(--color-primary); }

/* Responsive */
@media (max-width: 1100px) { .packages-grid { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 600px) { .packages-grid { grid-template-columns: 1fr; } .filters-bar { flex-direction: column; } .form-grid { grid-template-columns: 1fr; } }
</style>
