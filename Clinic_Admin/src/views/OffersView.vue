<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { BadgePercent, CalendarDays, CheckCircle2, Edit3, Plus, RefreshCw, Search, Sparkles, Trash2 } from '@lucide/vue'
import AppModal from '../components/AppModal.vue'
import AppPagination from '../components/AppPagination.vue'
import LongPressButton from '../components/LongPressButton.vue'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, ClinicItem, DoctorItem, DoctorOfferItem, DoctorOfferQuota, PageResult } from '../types/api'
import { getErrorMessage } from '../utils/errors'

type OfferForm = {
  id?: number
  doctorId: string
  appliesToAllClinics: boolean
  clinicId: string
  title: string
  description: string
  offerType: string
  originalPrice: string
  offerPrice: string
  discountPercent: string
  badgeText: string
  terms: string
  startsAt: string
  endsAt: string
  isActive: boolean
}

const auth = useAuthStore()
const notifications = useNotificationsStore()
const isAdmin = computed(() => auth.hasAnyRole(['SuperAdmin']))
const offers = ref<DoctorOfferItem[]>([])
const doctors = ref<DoctorItem[]>([])
const clinics = ref<ClinicItem[]>([])
const quota = ref<DoctorOfferQuota>()
const loading = ref(false)
const saving = ref(false)
const deleting = ref<DoctorOfferItem>()
const editorOpen = ref(false)
const page = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)
const filters = reactive({ search: '', doctorId: '', clinicId: '', isActive: '', currentlyVisible: '' })
const form = reactive<OfferForm>(blankForm())

const offerTypes = [
  { value: '0', label: 'خصم نسبة', hint: 'اكتب نسبة الخصم من 1 إلى 100.' },
  { value: '1', label: 'سعر خاص', hint: 'اكتب السعر قبل العرض والسعر الجديد.' },
  { value: '2', label: 'باقة خدمة', hint: 'اكتب سعر الباقة وتفاصيلها.' },
  { value: '3', label: 'استشارة مجانية', hint: 'لا تحتاج إلى سعر، فقط وصف واضح.' },
]

const selectedOfferType = computed(() => offerTypes.find((item) => item.value === form.offerType) ?? offerTypes[0])
const canSave = computed(() => {
  if (!form.title.trim() || !form.startsAt || !form.endsAt) return false
  if (isAdmin && !form.doctorId) return false
  if (!form.appliesToAllClinics && !form.clinicId) return false
  return !saving.value
})
const maxEndDate = computed(() => {
  if (!form.startsAt) return ''
  const date = new Date(`${form.startsAt}T00:00:00`)
  date.setDate(date.getDate() + 6)
  return toDateInput(date)
})
const selectedDoctorName = computed(() => doctors.value.find((doctor) => String(doctor.id) === filters.doctorId)?.name)

function blankForm(): OfferForm {
  const today = toDateInput(new Date())
  return {
    doctorId: '',
    appliesToAllClinics: true,
    clinicId: '',
    title: '',
    description: '',
    offerType: '0',
    originalPrice: '',
    offerPrice: '',
    discountPercent: '',
    badgeText: '',
    terms: '',
    startsAt: today,
    endsAt: today,
    isActive: true,
  }
}

function toDateInput(value: Date) {
  return value.toLocaleDateString('en-CA')
}

function dateOnly(value: string) {
  return value ? value.slice(0, 10) : toDateInput(new Date())
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium' }).format(new Date(value))
}

function formatMoney(value?: number) {
  if (value == null) return '-'
  return new Intl.NumberFormat('ar-IQ').format(value)
}

function numberOrNull(value: string) {
  return value === '' ? null : Number(value)
}

function statusLabel(item: DoctorOfferItem) {
  if (!item.isActive) return { label: 'متوقف', className: 'status-neutral' }
  if (item.isCurrentlyVisible) return { label: 'ظاهر الآن', className: 'status-success' }
  return { label: 'مجدول', className: 'status-warning' }
}

function scopeLabel(item: DoctorOfferItem) {
  return item.appliesToAllClinics ? 'جميع العيادات' : item.clinicName ?? '-'
}

function priceLabel(item: DoctorOfferItem) {
  if (item.offerType === 0) return `${item.discountPercent ?? 0}%`
  if (item.offerType === 3) return 'مجاني'
  return `${formatMoney(item.offerPrice)} د.ع`
}

function resetForm(item?: DoctorOfferItem) {
  Object.assign(form, blankForm())
  if (!item) return
  Object.assign(form, {
    id: item.id,
    doctorId: String(item.doctorId),
    appliesToAllClinics: item.appliesToAllClinics,
    clinicId: item.clinicId ? String(item.clinicId) : '',
    title: item.title,
    description: item.description ?? '',
    offerType: String(item.offerType),
    originalPrice: item.originalPrice == null ? '' : String(item.originalPrice),
    offerPrice: item.offerPrice == null ? '' : String(item.offerPrice),
    discountPercent: item.discountPercent == null ? '' : String(item.discountPercent),
    badgeText: item.badgeText ?? '',
    terms: item.terms ?? '',
    startsAt: dateOnly(item.startsAt),
    endsAt: dateOnly(item.endsAt),
    isActive: item.isActive,
  })
}

async function loadDoctors() {
  if (!isAdmin) return
  const response = await api.get<ApiResponse<PageResult<DoctorItem>>>('/Doctor', { params: { page: 1, pageSize: 200 } })
  doctors.value = response.data.data.items
}

async function loadClinics(doctorId?: string) {
  clinics.value = []
  const targetDoctorId = doctorId || filters.doctorId || form.doctorId
  if (isAdmin && !targetDoctorId) return
  try {
    const url = isAdmin ? `/Clinic/doctor/${targetDoctorId}/admin` : '/Clinic/my'
    const response = await api.get<ApiResponse<ClinicItem[]>>(url)
    clinics.value = response.data.data
  } catch (error: any) {
    if (error.response?.status !== 404) notifications.show(getErrorMessage(error), 'error')
  }
}

async function loadQuota(doctorId?: string) {
  quota.value = undefined
  const targetDoctorId = doctorId || filters.doctorId || form.doctorId
  if (isAdmin && !targetDoctorId) return
  try {
    const url = isAdmin ? `/DoctorOffer/quota/${targetDoctorId}` : '/DoctorOffer/my/quota'
    const response = await api.get<ApiResponse<DoctorOfferQuota>>(url)
    quota.value = response.data.data
  } catch (error: any) {
    if (error.response?.status !== 404) notifications.show(getErrorMessage(error), 'error')
  }
}

async function loadOffers() {
  loading.value = true
  try {
    const response = await api.get<ApiResponse<PageResult<DoctorOfferItem>>>(isAdmin ? '/DoctorOffer' : '/DoctorOffer/my', {
      params: {
        page: page.value,
        pageSize: 10,
        search: filters.search || undefined,
        doctorId: isAdmin && filters.doctorId ? filters.doctorId : undefined,
        clinicId: filters.clinicId || undefined,
        isActive: filters.isActive === '' ? undefined : filters.isActive,
        currentlyVisible: filters.currentlyVisible === '' ? undefined : filters.currentlyVisible,
      },
    })
    offers.value = response.data.data.items
    totalPages.value = response.data.data.totalPages || 1
    totalItems.value = response.data.data.totalItems
  } catch (error: any) {
    if (error.response?.status === 404) {
      offers.value = []
      totalPages.value = 1
      totalItems.value = 0
    } else notifications.show(getErrorMessage(error), 'error')
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  page.value = 1
  loadOffers()
}

async function openEditor(item?: DoctorOfferItem) {
  resetForm(item)
  if (!item && isAdmin && filters.doctorId) form.doctorId = filters.doctorId
  editorOpen.value = true
  await Promise.all([loadClinics(form.doctorId), loadQuota(form.doctorId)])
}

async function onFormDoctorChanged() {
  form.clinicId = ''
  form.appliesToAllClinics = true
  await Promise.all([loadClinics(form.doctorId), loadQuota(form.doctorId)])
}

function validateDuration() {
  const start = new Date(`${form.startsAt}T00:00:00`)
  const end = new Date(`${form.endsAt}T23:59:59`)
  if (end <= start) {
    notifications.show('تاريخ نهاية العرض يجب أن يكون بعد تاريخ البداية.', 'error')
    return false
  }
  if ((end.getTime() - start.getTime()) / 86400000 > 7) {
    notifications.show('مدة ظهور العرض لا يمكن أن تتجاوز 7 أيام.', 'error')
    return false
  }
  return true
}

async function saveOffer() {
  if (!validateDuration()) return
  saving.value = true
  try {
    const payload = {
      id: form.id,
      doctorId: isAdmin ? Number(form.doctorId) : undefined,
      appliesToAllClinics: form.appliesToAllClinics,
      clinicId: form.appliesToAllClinics ? null : Number(form.clinicId),
      title: form.title.trim(),
      description: form.description.trim() || null,
      offerType: Number(form.offerType),
      originalPrice: numberOrNull(form.originalPrice),
      offerPrice: numberOrNull(form.offerPrice),
      discountPercent: numberOrNull(form.discountPercent),
      badgeText: form.badgeText.trim() || null,
      terms: form.terms.trim() || null,
      startsAt: `${form.startsAt}T00:00:00`,
      endsAt: `${form.endsAt}T23:59:59`,
      isActive: form.isActive,
    }
    const url = isAdmin ? '/DoctorOffer' : '/DoctorOffer/my'
    const response = form.id
      ? await api.put<ApiResponse<boolean>>(url, payload)
      : await api.post<ApiResponse<boolean>>(url, payload)
    notifications.show(response.data.message)
    editorOpen.value = false
    await Promise.all([loadOffers(), loadQuota(form.doctorId)])
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    saving.value = false
  }
}

async function confirmDelete() {
  if (!deleting.value) return
  saving.value = true
  try {
    const url = isAdmin ? `/DoctorOffer/${deleting.value.id}` : `/DoctorOffer/my/${deleting.value.id}`
    const response = await api.delete<ApiResponse<boolean>>(url)
    notifications.show(response.data.message)
    deleting.value = undefined
    await Promise.all([loadOffers(), loadQuota(filters.doctorId)])
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    saving.value = false
  }
}

watch(() => filters.doctorId, async (doctorId) => {
  filters.clinicId = ''
  await Promise.all([loadClinics(doctorId), loadQuota(doctorId)])
})

watch(() => form.startsAt, () => {
  if (form.endsAt > maxEndDate.value) form.endsAt = maxEndDate.value
})

onMounted(async () => {
  await Promise.all([loadDoctors(), loadClinics(), loadQuota(), loadOffers()])
})
</script>

<template>
  <div class="offers-page">
    <section class="page-heading">
      <div>
        <span class="eyebrow">إدارة العروض</span>
        <h2>عروض الأطباء</h2>
        <p>العروض مرتبطة بباقة الطبيب، ومدة ظهورها القصوى 7 أيام.</p>
      </div>
      <div class="heading-actions">
        <button class="secondary-button" type="button" :disabled="loading" @click="loadOffers"><RefreshCw :size="17" /> تحديث</button>
        <button class="compact-primary" type="button" @click="openEditor()"><Plus :size="17" /> إضافة عرض</button>
      </div>
    </section>

    <section class="offer-summary">
      <article>
        <span><Sparkles :size="18" /></span>
        <small>الباقة</small>
        <strong>{{ quota?.packageName ?? (isAdmin && !filters.doctorId ? 'اختر طبيباً' : 'لا توجد باقة فعالة') }}</strong>
      </article>
      <article>
        <span><BadgePercent :size="18" /></span>
        <small>العروض الفعالة</small>
        <strong>{{ quota?.activeOffers ?? 0 }} / {{ quota?.maxActiveOffers ?? 0 }}</strong>
      </article>
      <article>
        <span><CheckCircle2 :size="18" /></span>
        <small>المتبقي</small>
        <strong>{{ quota?.remainingOffers ?? 0 }}</strong>
      </article>
      <article :class="{ blocked: quota && !quota.canMakeOffers }">
        <span><CalendarDays :size="18" /></span>
        <small>حالة إنشاء العروض</small>
        <strong>{{ quota?.canMakeOffers ? 'مسموح' : 'غير مسموح' }}</strong>
      </article>
    </section>

    <section class="filter-card offers-filter">
      <label class="search-box">
        <Search :size="17" />
        <input v-model="filters.search" placeholder="ابحث بالعنوان أو الطبيب أو العيادة" @keyup.enter="applyFilters" />
      </label>
      <select v-if="isAdmin" v-model="filters.doctorId">
        <option value="">كل الأطباء</option>
        <option v-for="doctor in doctors" :key="doctor.id" :value="doctor.id">{{ doctor.name }}</option>
      </select>
      <select v-model="filters.clinicId" :disabled="isAdmin && !filters.doctorId">
        <option value="">كل العيادات</option>
        <option v-for="clinic in clinics" :key="clinic.id" :value="clinic.id">{{ clinic.name }}</option>
      </select>
      <select v-model="filters.isActive">
        <option value="">كل الحالات</option>
        <option value="true">فعال</option>
        <option value="false">متوقف</option>
      </select>
      <select v-model="filters.currentlyVisible">
        <option value="">كل الظهور</option>
        <option value="true">ظاهر الآن</option>
        <option value="false">غير ظاهر الآن</option>
      </select>
      <button class="compact-primary" type="button" @click="applyFilters">تطبيق</button>
    </section>

    <section class="table-card">
      <div class="table-toolbar">
        <strong>{{ totalItems }} عرض</strong>
        <span v-if="selectedDoctorName">الطبيب: {{ selectedDoctorName }}</span>
      </div>
      <div v-if="loading" class="empty-panel">جاري تحميل العروض...</div>
      <div v-else-if="!offers.length" class="empty-panel">
        <BadgePercent :size="31" />
        <h3>لا توجد عروض</h3>
        <p>أضف عرضاً قصير المدة ليظهر حسب باقة الطبيب والعيادات المختارة.</p>
      </div>
      <div v-else class="table-scroll">
        <table class="data-table offers-table">
          <thead>
            <tr>
              <th>العرض</th>
              <th>الطبيب</th>
              <th>النطاق</th>
              <th>السعر / النوع</th>
              <th>مدة الظهور</th>
              <th>الحالة</th>
              <th>إجراءات</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="offer in offers" :key="offer.id">
              <td>
                <div class="offer-title-cell">
                  <strong>{{ offer.title }}</strong>
                  <small>{{ offer.description || offer.terms || 'بدون تفاصيل إضافية' }}</small>
                  <span v-if="offer.badgeText" class="offer-badge">{{ offer.badgeText }}</span>
                </div>
              </td>
              <td>{{ offer.doctorName }}</td>
              <td>{{ scopeLabel(offer) }}</td>
              <td><strong>{{ priceLabel(offer) }}</strong><small>{{ offer.offerTypeName }}</small></td>
              <td>{{ formatDate(offer.startsAt) }} - {{ formatDate(offer.endsAt) }}<small>{{ offer.remainingDays }} يوم متبقي</small></td>
              <td><span class="status-badge" :class="statusLabel(offer).className">{{ statusLabel(offer).label }}</span></td>
              <td>
                <div class="row-actions">
                  <button type="button" class="secondary-button icon-action" @click="openEditor(offer)"><Edit3 :size="15" /></button>
                  <LongPressButton button-class="danger-button icon-action" title="اضغط مطولاً لحذف العرض" @confirm="deleting = offer"><Trash2 :size="15" /></LongPressButton>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <AppPagination :page="page" :total-pages="totalPages" @change="(nextPage) => { page = nextPage; loadOffers() }" />
    </section>

    <AppModal v-if="editorOpen" :title="form.id ? 'تعديل العرض' : 'إضافة عرض'" wide @close="editorOpen = false">
      <form class="modal-form form-grid" @submit.prevent="saveOffer">
        <label v-if="isAdmin" class="full-field">
          <span>الطبيب</span>
          <select v-model="form.doctorId" required @change="onFormDoctorChanged">
            <option value="">اختر الطبيب</option>
            <option v-for="doctor in doctors" :key="doctor.id" :value="doctor.id">{{ doctor.name }}</option>
          </select>
        </label>

        <label class="full-field">
          <span>عنوان العرض</span>
          <input v-model.trim="form.title" maxlength="160" required placeholder="مثال: خصم فحص شامل لمدة أسبوع" />
        </label>

        <label>
          <span>نوع العرض</span>
          <select v-model="form.offerType">
            <option v-for="type in offerTypes" :key="type.value" :value="type.value">{{ type.label }}</option>
          </select>
          <small class="field-hint">{{ selectedOfferType.hint }}</small>
        </label>

        <label>
          <span>شارة قصيرة</span>
          <input v-model.trim="form.badgeText" maxlength="40" placeholder="عرض محدود" />
        </label>

        <label v-if="form.offerType !== '3'">
          <span>السعر قبل العرض</span>
          <input v-model="form.originalPrice" min="0" step="0.01" type="number" placeholder="اختياري" />
        </label>

        <label v-if="form.offerType === '0'">
          <span>نسبة الخصم</span>
          <input v-model="form.discountPercent" min="1" max="100" step="0.01" type="number" required />
        </label>

        <label v-if="form.offerType === '1' || form.offerType === '2'">
          <span>سعر العرض</span>
          <input v-model="form.offerPrice" min="1" step="0.01" type="number" required />
        </label>

        <label>
          <span>تاريخ البداية</span>
          <input v-model="form.startsAt" required type="date" />
        </label>

        <label>
          <span>تاريخ النهاية</span>
          <input v-model="form.endsAt" required type="date" :min="form.startsAt" :max="maxEndDate" />
          <small class="field-hint">أقصى مدة ظهور: 7 أيام.</small>
        </label>

        <div class="full-field clinic-scope">
          <label><input v-model="form.appliesToAllClinics" :value="true" type="radio" /> جميع العيادات</label>
          <label><input v-model="form.appliesToAllClinics" :value="false" type="radio" /> عيادة محددة</label>
        </div>

        <label v-if="!form.appliesToAllClinics" class="full-field">
          <span>العيادة</span>
          <select v-model="form.clinicId" required>
            <option value="">اختر العيادة</option>
            <option v-for="clinic in clinics" :key="clinic.id" :value="clinic.id">{{ clinic.name }}</option>
          </select>
        </label>

        <label class="full-field">
          <span>وصف العرض</span>
          <textarea v-model.trim="form.description" maxlength="800" rows="3" placeholder="تفاصيل سريعة للمريض عن العرض"></textarea>
        </label>

        <label class="full-field">
          <span>الشروط</span>
          <textarea v-model.trim="form.terms" maxlength="600" rows="2" placeholder="مثال: لا يشمل التحاليل أو الأدوية"></textarea>
        </label>

        <label class="switch-field full-field">
          <input v-model="form.isActive" type="checkbox" />
          <span>العرض فعال وقابل للظهور ضمن المدة المحددة</span>
        </label>

        <div v-if="quota" class="quota-note full-field" :class="{ blocked: !quota.canMakeOffers || quota.remainingOffers <= 0 && !form.id }">
          الباقة: {{ quota.packageName ?? '-' }}، العروض الفعالة {{ quota.activeOffers }} / {{ quota.maxActiveOffers }}، المتبقي {{ quota.remainingOffers }}.
        </div>

        <div class="modal-actions full-field">
          <button class="secondary-button" type="button" @click="editorOpen = false">تراجع</button>
          <button class="compact-primary" type="submit" :disabled="!canSave">{{ saving ? 'جاري الحفظ...' : 'حفظ العرض' }}</button>
        </div>
      </form>
    </AppModal>

    <AppModal v-if="deleting" title="حذف العرض" @close="deleting = undefined">
      <p class="modal-copy">سيتم حذف عرض <strong>{{ deleting.title }}</strong> ولن يظهر ضمن عروض الطبيب.</p>
      <div class="modal-actions">
        <button class="secondary-button" type="button" @click="deleting = undefined">تراجع</button>
        <LongPressButton button-class="danger-button" :disabled="saving" title="اضغط مطولاً لتأكيد الحذف" @confirm="confirmDelete">تأكيد الحذف</LongPressButton>
      </div>
    </AppModal>
  </div>
</template>

<style scoped>
.offers-page { display: grid; gap: 16px; }
.page-heading p { margin: 5px 0 0; color: var(--muted); }
.offer-summary { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px; }
.offer-summary article { display: grid; gap: 6px; padding: 15px; border: 1px solid var(--line); border-radius: 12px; background: #fff; box-shadow: var(--shadow); }
.offer-summary span { display: grid; place-items: center; width: 36px; height: 36px; color: var(--primary); border-radius: 10px; background: var(--primary-soft); }
.offer-summary small, .data-table small { display: block; color: var(--muted); font-size: 12px; }
.offer-summary strong { color: var(--ink); font-size: 18px; }
.offer-summary article.blocked span { color: #a46724; background: #fff1db; }
.offers-filter { grid-template-columns: minmax(240px, 1.2fr) repeat(4, minmax(130px, .7fr)) auto; }
.table-toolbar { justify-content: space-between; padding: 14px 16px; border-bottom: 1px solid var(--line); }
.table-scroll { overflow-x: auto; }
.offer-title-cell { display: grid; gap: 5px; min-width: 220px; }
.offer-badge { width: fit-content; padding: 5px 9px; color: #9a6500; border: 1px solid #f0c05b; border-radius: 999px; background: #fff8df; font-size: 11px; font-weight: 800; }
.icon-action { width: 35px; height: 35px; padding: 0; }
.field-hint { display: block; margin-top: 6px; color: var(--muted); font-size: 12px; }
.clinic-scope { display: flex; flex-wrap: wrap; gap: 10px; padding: 10px; border: 1px solid var(--line); border-radius: 10px; background: #fbfdfc; }
.clinic-scope label, .switch-field { display: flex; align-items: center; gap: 7px; font-weight: 700; }
.clinic-scope input, .switch-field input { width: auto; }
.quota-note { padding: 10px 12px; color: #167163; border: 1px solid #b8e0d8; border-radius: 10px; background: #eaf7f4; font-size: 13px; font-weight: 700; }
.quota-note.blocked { color: #a46724; border-color: #f2cf9a; background: #fff8eb; }
@media (max-width: 900px) {
  .offer-summary, .offers-filter { grid-template-columns: 1fr; }
}
</style>
