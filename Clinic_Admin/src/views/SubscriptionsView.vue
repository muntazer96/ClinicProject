<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { CalendarClock, Check, CircleX, Crown, PackageCheck, Plus, RefreshCw, RotateCw, SlidersHorizontal, Sparkles, TrendingUp } from '@lucide/vue'
import AppModal from '../components/AppModal.vue'
import AppPagination from '../components/AppPagination.vue'
import api from '../services/api'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, DoctorFeature, DoctorItem, DoctorSubscription, PageResult, SubscriptionPackage } from '../types/api'
import { getErrorMessage } from '../utils/errors'

type Tab = 'subscriptions' | 'packages' | 'features'
type Confirmation = { title: string; text: string; action: () => Promise<void> }

const notifications = useNotificationsStore()
const activeTab = ref<Tab>('subscriptions')
const subscriptions = ref<DoctorSubscription[]>([])
const packages = ref<SubscriptionPackage[]>([])
const doctors = ref<DoctorItem[]>([])
const features = ref<DoctorFeature[]>([])
const loading = ref(false)
const page = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)
const modal = ref<'create' | 'renew' | 'upgrade'>()
const selectedSubscription = ref<DoctorSubscription>()
const confirmation = ref<Confirmation>()
const filters = reactive({ doctorId: '', packageId: '', status: '', isActive: '' })
const featureDoctorId = ref('')
const createForm = reactive({ doctorId: '', packageId: '', isYearly: false, status: '0' })
const renewYearly = ref(false)
const upgradePackageId = ref('')

const statusOptions = [
  { value: '0', label: 'نشط' },
  { value: '1', label: 'قيد الانتظار' },
  { value: '2', label: 'منتهي' },
  { value: '3', label: 'ملغي' },
]
const enabledFeatures = computed(() => features.value.filter((feature) => feature.isEnabled).length)

function statusMeta(status: number) {
  return [
    { label: 'نشط', className: 'status-success' },
    { label: 'قيد الانتظار', className: 'status-warning' },
    { label: 'منتهي', className: 'status-neutral' },
    { label: 'ملغي', className: 'status-danger' },
  ][status] ?? { label: 'غير معروف', className: 'status-neutral' }
}

function formatDate(date?: string) {
  return date ? new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium' }).format(new Date(date)) : '-'
}

function money(value: number) {
  return new Intl.NumberFormat('ar-IQ').format(value)
}

async function loadLookups() {
  const [packagesResponse, doctorsResponse] = await Promise.all([
    api.get<ApiResponse<PageResult<SubscriptionPackage>>>('/SubscriptionPackages', { params: { page: 1, pageSize: 100 } }),
    api.get<ApiResponse<PageResult<DoctorItem>>>('/Doctor', { params: { page: 1, pageSize: 100 } }),
  ])
  packages.value = packagesResponse.data.data.items
  doctors.value = doctorsResponse.data.data.items
}

async function loadSubscriptions() {
  loading.value = true
  try {
    const response = await api.get<ApiResponse<PageResult<DoctorSubscription>>>('/DoctorSubscription', {
      params: {
        doctorId: filters.doctorId || undefined,
        packageId: filters.packageId || undefined,
        status: filters.status === '' ? undefined : filters.status,
        isActive: filters.isActive === '' ? undefined : filters.isActive,
        page: page.value,
        pageSize: 10,
      },
    })
    subscriptions.value = response.data.data.items
    totalPages.value = response.data.data.totalPages
    totalItems.value = response.data.data.totalItems
  } catch (error: any) {
    if (error.response?.status === 404) {
      subscriptions.value = []
      totalPages.value = 1
      totalItems.value = 0
    } else notifications.show(getErrorMessage(error), 'error')
  } finally {
    loading.value = false
  }
}

async function loadFeatures() {
  loading.value = true
  try {
    const response = await api.get<ApiResponse<PageResult<DoctorFeature>>>('/DoctorFeature', {
      params: { doctorId: featureDoctorId.value || undefined, page: page.value, pageSize: 10 },
    })
    features.value = response.data.data.items
    totalPages.value = response.data.data.totalPages
    totalItems.value = response.data.data.totalItems
  } catch (error: any) {
    if (error.response?.status === 404) {
      features.value = []
      totalPages.value = 1
      totalItems.value = 0
    } else notifications.show(getErrorMessage(error), 'error')
  } finally {
    loading.value = false
  }
}

async function initialize() {
  loading.value = true
  try {
    await loadLookups()
    await loadSubscriptions()
  } catch (error: any) {
    if (error.response?.status === 404) {
      packages.value = []
      doctors.value = []
    } else notifications.show(getErrorMessage(error), 'error')
  } finally {
    loading.value = false
  }
}

function selectTab(tab: Tab) {
  activeTab.value = tab
  page.value = 1
  if (tab === 'subscriptions') loadSubscriptions()
  if (tab === 'features') loadFeatures()
}

async function createSubscription() {
  try {
    const response = await api.post<ApiResponse<object>>('/DoctorSubscription', {
      doctorId: Number(createForm.doctorId),
      packageId: Number(createForm.packageId),
      isYearly: createForm.isYearly,
      status: Number(createForm.status),
    })
    notifications.show(response.data.message)
    modal.value = undefined
    Object.assign(createForm, { doctorId: '', packageId: '', isYearly: false, status: '0' })
    await loadSubscriptions()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

async function activate(subscription: DoctorSubscription) {
  try {
    const response = await api.post<ApiResponse<object>>(`/DoctorSubscription/${subscription.id}/activate`)
    notifications.show(response.data.message)
    await loadSubscriptions()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

function openRenew(subscription: DoctorSubscription) {
  selectedSubscription.value = subscription
  renewYearly.value = false
  modal.value = 'renew'
}

async function renew() {
  if (!selectedSubscription.value) return
  try {
    const response = await api.post<ApiResponse<object>>(`/DoctorSubscription/${selectedSubscription.value.id}/renew`, { isYearly: renewYearly.value })
    notifications.show(response.data.message)
    modal.value = undefined
    await loadSubscriptions()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

function openUpgrade(subscription: DoctorSubscription) {
  selectedSubscription.value = subscription
  upgradePackageId.value = ''
  modal.value = 'upgrade'
}

async function upgrade() {
  if (!selectedSubscription.value) return
  try {
    const response = await api.post<ApiResponse<object>>(`/DoctorSubscription/${selectedSubscription.value.id}/upgrade`, { packageId: Number(upgradePackageId.value) })
    notifications.show(response.data.message)
    modal.value = undefined
    await loadSubscriptions()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

function askCancel(subscription: DoctorSubscription) {
  confirmation.value = {
    title: 'إلغاء الاشتراك',
    text: `سيتم إلغاء اشتراك الطبيب ${subscription.doctor.name} وتعطيل مميزاته إذا لم يكن لديه اشتراك فعّال آخر.`,
    action: async () => {
      const response = await api.delete<ApiResponse<object>>(`/DoctorSubscription/${subscription.id}`)
      notifications.show(response.data.message)
      await loadSubscriptions()
    },
  }
}

async function runConfirmation() {
  if (!confirmation.value) return
  try {
    await confirmation.value.action()
    confirmation.value = undefined
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

async function toggleFeature(feature: DoctorFeature) {
  try {
    const response = await api.post<ApiResponse<object>>(`/DoctorFeature/${feature.id}/toggle`)
    notifications.show(response.data.message)
    await loadFeatures()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

function applyFilters() {
  page.value = 1
  activeTab.value === 'features' ? loadFeatures() : loadSubscriptions()
}

function changePage(newPage: number) {
  page.value = newPage
  activeTab.value === 'features' ? loadFeatures() : loadSubscriptions()
}

onMounted(initialize)
</script>

<template>
  <div>
    <div class="page-heading">
      <div><span class="section-kicker">إدارة الخدمات</span><h2>الاشتراكات والباقات</h2><p>تحكم باشتراكات الأطباء والمميزات المرتبطة بكل باقة.</p></div>
      <button class="compact-primary" type="button" @click="modal = 'create'"><Plus :size="17" /> اشتراك جديد</button>
    </div>

    <div class="content-tabs">
      <button :class="{ active: activeTab === 'subscriptions' }" type="button" @click="selectTab('subscriptions')"><CalendarClock :size="17" /> الاشتراكات</button>
      <button :class="{ active: activeTab === 'packages' }" type="button" @click="selectTab('packages')"><PackageCheck :size="17" /> الباقات</button>
      <button :class="{ active: activeTab === 'features' }" type="button" @click="selectTab('features')"><Sparkles :size="17" /> المميزات</button>
    </div>

    <template v-if="activeTab === 'subscriptions'">
      <section class="filter-card subscription-filters">
        <select v-model="filters.doctorId"><option value="">كل الأطباء</option><option v-for="doctor in doctors" :key="doctor.id" :value="doctor.id">{{ doctor.name }}</option></select>
        <select v-model="filters.packageId"><option value="">كل الباقات</option><option v-for="item in packages" :key="item.id" :value="item.id">{{ item.name }}</option></select>
        <select v-model="filters.status"><option value="">كل الحالات</option><option v-for="status in statusOptions" :key="status.value" :value="status.value">{{ status.label }}</option></select>
        <select v-model="filters.isActive"><option value="">الفعالية: الكل</option><option value="true">فعّال الآن</option><option value="false">غير فعّال</option></select>
        <button class="compact-primary" type="button" @click="applyFilters">تطبيق</button>
      </section>
      <section class="table-card">
        <div class="table-toolbar"><button class="secondary-button" type="button" :disabled="loading" @click="loadSubscriptions"><RefreshCw :size="16" /> تحديث</button><span class="records-count">{{ totalItems }} اشتراك</span></div>
        <div class="table-scroll"><table class="data-table">
          <thead><tr><th>الطبيب</th><th>الباقة</th><th>المدة</th><th>الحالة</th><th>الإجراءات</th></tr></thead>
          <tbody>
            <tr v-if="loading"><td class="table-message" colspan="5">جارِ تحميل الاشتراكات...</td></tr>
            <tr v-else-if="!subscriptions.length"><td class="table-message" colspan="5">لا توجد اشتراكات مطابقة.</td></tr>
            <tr v-for="subscription in subscriptions" v-else :key="subscription.id">
              <td><strong>{{ subscription.doctor.name }}</strong><small class="block-muted">{{ subscription.doctor.specialization.name }}</small></td>
              <td><strong>{{ subscription.package.name }}</strong><small class="block-muted">{{ money(subscription.package.price) }} د.ع شهرياً</small></td>
              <td><strong>{{ formatDate(subscription.startDate) }}</strong><small class="block-muted">حتى {{ formatDate(subscription.endDate) }}</small></td>
              <td><span class="status-badge" :class="statusMeta(subscription.status).className">{{ statusMeta(subscription.status).label }}</span></td>
              <td><div class="row-actions">
                <button v-if="subscription.status === 1" type="button" title="تفعيل الاشتراك" @click="activate(subscription)"><Check :size="17" /></button>
                <button v-if="subscription.status !== 3" type="button" title="تجديد الاشتراك" @click="openRenew(subscription)"><RotateCw :size="17" /></button>
                <button v-if="subscription.status === 0 && subscription.isActive" type="button" title="ترقية الباقة" @click="openUpgrade(subscription)"><TrendingUp :size="17" /></button>
                <button v-if="subscription.status !== 3" class="danger-action" type="button" title="إلغاء الاشتراك" @click="askCancel(subscription)"><CircleX :size="17" /></button>
              </div></td>
            </tr>
          </tbody>
        </table></div>
        <AppPagination :page="page" :total-pages="totalPages" @change="changePage" />
      </section>
    </template>

    <section v-if="activeTab === 'packages'" class="package-grid">
      <article v-for="item in packages" :key="item.id" class="package-card">
        <div class="package-header"><span><Crown :size="20" /></span><div><h3>{{ item.name }}</h3><small>{{ item.normalizedName }}</small></div></div>
        <div class="package-price"><strong>{{ money(item.price) }}</strong><span>د.ع / شهر</span></div>
        <small class="yearly-price">{{ money(item.yearlyPrice) }} د.ع سنوياً</small>
        <div class="package-limits"><span>{{ item.maxClinics }} عيادات</span><span>{{ item.maxWeeklyDays }} أيام أسبوعية</span><span>{{ item.maxDailyAppointments }} دور يومي</span></div>
        <div class="package-features">
          <span :class="{ enabled: item.eBooking }">الحجز الإلكتروني</span><span :class="{ enabled: item.showReviews }">التقييمات</span><span :class="{ enabled: item.showMessages }">الرسائل</span><span :class="{ enabled: item.ePayments }">الدفع الإلكتروني</span><span :class="{ enabled: item.makeOffers }">العروض</span>
        </div>
      </article>
      <div v-if="!packages.length" class="empty-panel">لا توجد باقات للعرض.</div>
    </section>

    <template v-if="activeTab === 'features'">
      <section class="filter-card feature-filters"><select v-model="featureDoctorId"><option value="">كل الأطباء</option><option v-for="doctor in doctors" :key="doctor.id" :value="doctor.id">{{ doctor.name }}</option></select><button class="compact-primary" type="button" @click="applyFilters">تطبيق</button></section>
      <section class="table-card">
        <div class="table-toolbar"><button class="secondary-button" type="button" :disabled="loading" @click="loadFeatures"><RefreshCw :size="16" /> تحديث</button><span class="records-count">{{ enabledFeatures }} مفعّلة ضمن الصفحة</span></div>
        <div class="table-scroll"><table class="data-table">
          <thead><tr><th>الطبيب</th><th>الميزة</th><th>الوصف</th><th>الحالة</th><th>التحكم</th></tr></thead>
          <tbody>
            <tr v-if="loading"><td class="table-message" colspan="5">جارِ تحميل المميزات...</td></tr>
            <tr v-else-if="!features.length"><td class="table-message" colspan="5">لا توجد مميزات مطابقة.</td></tr>
            <tr v-for="feature in features" v-else :key="feature.id">
              <td><strong>{{ feature.doctor.name }}</strong><small class="block-muted">{{ feature.doctor.specialization.name }}</small></td>
              <td><strong>{{ feature.feature.name }}</strong><small class="block-muted">{{ feature.feature.normalizedName }}</small></td>
              <td class="muted-cell">{{ feature.feature.description || '-' }}</td>
              <td><span class="status-badge" :class="feature.isEnabled ? 'status-success' : 'status-neutral'">{{ feature.isEnabled ? 'مفعّلة' : 'معطّلة' }}</span></td>
              <td><button class="status-button" :class="{ 'visible-status': feature.isEnabled }" type="button" @click="toggleFeature(feature)"><SlidersHorizontal :size="16" /> {{ feature.isEnabled ? 'تعطيل' : 'تفعيل' }}</button></td>
            </tr>
          </tbody>
        </table></div>
        <AppPagination :page="page" :total-pages="totalPages" @change="changePage" />
      </section>
    </template>

    <AppModal v-if="modal === 'create'" title="إنشاء اشتراك جديد" @close="modal = undefined">
      <form class="modal-form" @submit.prevent="createSubscription">
        <label><span>الطبيب</span><select v-model="createForm.doctorId" required><option disabled value="">اختر الطبيب</option><option v-for="doctor in doctors" :key="doctor.id" :value="doctor.id">{{ doctor.name }}</option></select></label>
        <label><span>الباقة</span><select v-model="createForm.packageId" required><option disabled value="">اختر الباقة</option><option v-for="item in packages" :key="item.id" :value="item.id">{{ item.name }}</option></select></label>
        <label><span>الحالة الابتدائية</span><select v-model="createForm.status"><option value="0">نشط</option><option value="1">قيد الانتظار</option></select></label>
        <label class="checkbox-field"><input v-model="createForm.isYearly" type="checkbox" /><span>اشتراك سنوي</span></label>
        <div class="modal-actions"><button class="secondary-button" type="button" @click="modal = undefined">تراجع</button><button class="compact-primary" type="submit">إنشاء الاشتراك</button></div>
      </form>
    </AppModal>

    <AppModal v-if="modal === 'renew' && selectedSubscription" title="تجديد الاشتراك" @close="modal = undefined">
      <p class="modal-copy">سيتم تجديد اشتراك الطبيب <strong>{{ selectedSubscription.doctor.name }}</strong> على باقة <strong>{{ selectedSubscription.package.name }}</strong>.</p>
      <label class="checkbox-field modal-checkbox"><input v-model="renewYearly" type="checkbox" /><span>تجديد سنوي بدلاً من شهر واحد</span></label>
      <div class="modal-actions"><button class="secondary-button" type="button" @click="modal = undefined">تراجع</button><button class="compact-primary" type="button" @click="renew">تجديد</button></div>
    </AppModal>

    <AppModal v-if="modal === 'upgrade' && selectedSubscription" title="ترقية الباقة" @close="modal = undefined">
      <form class="modal-form" @submit.prevent="upgrade">
        <p class="modal-copy">اختر باقة أعلى لاشتراك الطبيب <strong>{{ selectedSubscription.doctor.name }}</strong>.</p>
        <label><span>الباقة الجديدة</span><select v-model="upgradePackageId" required><option disabled value="">اختر الباقة</option><option v-for="item in packages" :key="item.id" :value="item.id">{{ item.name }}</option></select></label>
        <div class="modal-actions"><button class="secondary-button" type="button" @click="modal = undefined">تراجع</button><button class="compact-primary" type="submit">ترقية</button></div>
      </form>
    </AppModal>

    <AppModal v-if="confirmation" :title="confirmation.title" @close="confirmation = undefined">
      <p class="modal-copy">{{ confirmation.text }}</p>
      <div class="modal-actions"><button class="secondary-button" type="button" @click="confirmation = undefined">تراجع</button><button class="danger-button" type="button" @click="runConfirmation">تأكيد الإلغاء</button></div>
    </AppModal>
  </div>
</template>
