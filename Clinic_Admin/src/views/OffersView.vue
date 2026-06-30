<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'
import { useNotifications } from '../composables/useNotifications'
import type { ApiResponse, ClinicItem, DoctorItem, DoctorOfferItem, DoctorOfferQuota, PageResult } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import EmptyState from '../components/common/Emptystate.vue'

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
const { success: showSuccess, error: showError } = useNotifications()

const isDoctor = computed(() => auth.hasAnyRole(['DoctorUser']))
const isAdmin  = computed(() => auth.hasAnyRole(['SuperAdmin']) && !isDoctor.value)

const offers     = ref<DoctorOfferItem[]>([])
const doctors    = ref<DoctorItem[]>([])
const clinics    = ref<ClinicItem[]>([])
const quota      = ref<DoctorOfferQuota>()
const loading    = ref(false)
const saving     = ref(false)
const editorOpen = ref(false)
const deleteDialog = ref(false)
const deleting   = ref<DoctorOfferItem>()
const page       = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)

const filters = reactive({ search: '', doctorId: '', clinicId: '', isActive: '', currentlyVisible: '' })
const form = reactive<OfferForm>(blankForm())

const offerTypes = [
  { value: '0', label: 'خصم نسبة',          hint: 'اكتب نسبة الخصم من 1 إلى 100.'            },
  { value: '1', label: 'سعر خاص',            hint: 'اكتب السعر قبل العرض والسعر الجديد.'       },
  { value: '2', label: 'باقة خدمة',          hint: 'اكتب سعر الباقة وتفاصيلها.'               },
  { value: '3', label: 'استشارة مجانية',     hint: 'لا تحتاج إلى سعر، فقط وصف واضح.'         },
]

const selectedOfferType = computed(() => offerTypes.find((t) => t.value === form.offerType) ?? offerTypes[0])
const selectedDoctorName = computed(() => doctors.value.find((d) => String(d.id) === filters.doctorId)?.name)

const canSave = computed(() => {
  if (!form.title.trim() || !form.startsAt || !form.endsAt) return false
  if (isAdmin.value && !form.doctorId) return false
  if (!form.appliesToAllClinics && !form.clinicId) return false
  return !saving.value
})

const maxEndDate = computed(() => {
  if (!form.startsAt) return ''
  const d = new Date(`${form.startsAt}T00:00:00`)
  d.setDate(d.getDate() + 6)
  return d.toLocaleDateString('en-CA')
})

function blankForm(): OfferForm {
  const today = new Date().toLocaleDateString('en-CA')
  return {
    doctorId: '', appliesToAllClinics: true, clinicId: '',
    title: '', description: '', offerType: '0',
    originalPrice: '', offerPrice: '', discountPercent: '',
    badgeText: '', terms: '', startsAt: today, endsAt: today, isActive: true,
  }
}

function dateOnly(v: string) { return v ? v.slice(0, 10) : new Date().toLocaleDateString('en-CA') }
function formatDate(v: string) { return new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium' }).format(new Date(v)) }
function formatMoney(v?: number) { return v == null ? '-' : new Intl.NumberFormat('ar-IQ').format(v) }
function numberOrNull(v: string) { return v === '' ? null : Number(v) }

function statusMeta(item: DoctorOfferItem) {
  if (!item.isActive) return { label: 'متوقف', color: 'default' }
  if (item.isCurrentlyVisible) return { label: 'ظاهر الآن', color: 'success' }
  return { label: 'مجدول', color: 'warning' }
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
    id: item.id, doctorId: String(item.doctorId),
    appliesToAllClinics: item.appliesToAllClinics,
    clinicId: item.clinicId ? String(item.clinicId) : '',
    title: item.title, description: item.description ?? '',
    offerType: String(item.offerType),
    originalPrice: item.originalPrice == null ? '' : String(item.originalPrice),
    offerPrice: item.offerPrice == null ? '' : String(item.offerPrice),
    discountPercent: item.discountPercent == null ? '' : String(item.discountPercent),
    badgeText: item.badgeText ?? '', terms: item.terms ?? '',
    startsAt: dateOnly(item.startsAt), endsAt: dateOnly(item.endsAt),
    isActive: item.isActive,
  })
}

async function loadDoctors() {
  if (!isAdmin.value) return
  try {
    const r = await api.get<ApiResponse<PageResult<DoctorItem>>>('/Doctor', { params: { page: 1, pageSize: 200 } })
    doctors.value = r.data.data.items
  } catch (e: any) { if (e.response?.status !== 404) showError(getErrorMessage(e)) }
}

async function loadClinics(doctorId?: string) {
  clinics.value = []
  const id = doctorId || filters.doctorId || form.doctorId
  if (isAdmin.value && !id) return
  try {
    const url = isAdmin.value ? `/Clinic/doctor/${id}/admin` : '/Clinic/my'
    const r = await api.get<ApiResponse<ClinicItem[]>>(url)
    clinics.value = r.data.data
  } catch (e: any) { if (e.response?.status !== 404) showError(getErrorMessage(e)) }
}

async function loadQuota(doctorId?: string) {
  quota.value = undefined
  const id = doctorId || filters.doctorId || form.doctorId
  if (isAdmin.value && !id) return
  try {
    const url = isAdmin.value ? `/DoctorOffer/quota/${id}` : '/DoctorOffer/my/quota'
    const r = await api.get<ApiResponse<DoctorOfferQuota>>(url)
    quota.value = r.data.data
  } catch (e: any) { if (e.response?.status !== 404) showError(getErrorMessage(e)) }
}

async function loadOffers() {
  loading.value = true
  try {
    const r = await api.get<ApiResponse<PageResult<DoctorOfferItem>>>(isAdmin.value ? '/DoctorOffer' : '/DoctorOffer/my', {
      params: {
        page: page.value, pageSize: 10,
        search: filters.search || undefined,
        doctorId: isAdmin.value && filters.doctorId ? filters.doctorId : undefined,
        clinicId: filters.clinicId || undefined,
        isActive: filters.isActive === '' ? undefined : filters.isActive,
        currentlyVisible: filters.currentlyVisible === '' ? undefined : filters.currentlyVisible,
      },
    })
    offers.value = r.data.data.items
    totalPages.value = r.data.data.totalPages || 1
    totalItems.value = r.data.data.totalItems
  } catch (e: any) {
    if (e.response?.status === 404) { offers.value = []; totalPages.value = 1; totalItems.value = 0 }
    else showError(getErrorMessage(e))
  } finally { loading.value = false }
}

function applyFilters() { page.value = 1; loadOffers() }

async function openEditor(item?: DoctorOfferItem) {
  resetForm(item)
  if (!item && isAdmin.value && filters.doctorId) form.doctorId = filters.doctorId
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
  const end   = new Date(`${form.endsAt}T23:59:59`)
  if (end <= start) { showError('تاريخ نهاية العرض يجب أن يكون بعد تاريخ البداية.'); return false }
  if ((end.getTime() - start.getTime()) / 86400000 > 7) { showError('مدة ظهور العرض لا يمكن أن تتجاوز 7 أيام.'); return false }
  return true
}

async function saveOffer() {
  if (!validateDuration()) return
  saving.value = true
  try {
    const payload = {
      id: form.id,
      doctorId: isAdmin.value ? Number(form.doctorId) : undefined,
      appliesToAllClinics: form.appliesToAllClinics,
      clinicId: form.appliesToAllClinics ? null : Number(form.clinicId),
      title: form.title.trim(), description: form.description.trim() || null,
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
    const url = isAdmin.value ? '/DoctorOffer' : '/DoctorOffer/my'
    const r = form.id
      ? await api.put<ApiResponse<boolean>>(url, payload)
      : await api.post<ApiResponse<boolean>>(url, payload)
    showSuccess(r.data.message)
    editorOpen.value = false
    await Promise.all([loadOffers(), loadQuota(form.doctorId)])
  } catch (e) { showError(getErrorMessage(e)) }
  finally { saving.value = false }
}

async function confirmDelete() {
  if (!deleting.value) return
  saving.value = true
  try {
    const url = isAdmin.value ? `/DoctorOffer/${deleting.value.id}` : `/DoctorOffer/my/${deleting.value.id}`
    const r = await api.delete<ApiResponse<boolean>>(url)
    showSuccess(r.data.message)
    deleteDialog.value = false
    deleting.value = undefined
    await Promise.all([loadOffers(), loadQuota(filters.doctorId)])
  } catch (e) { showError(getErrorMessage(e)) }
  finally { saving.value = false }
}

watch(() => filters.doctorId, async (id) => {
  filters.clinicId = ''
  await Promise.all([loadClinics(id), loadQuota(id)])
})

watch(() => form.startsAt, () => {
  if (form.endsAt > maxEndDate.value) form.endsAt = maxEndDate.value
})

onMounted(() => Promise.all([loadDoctors(), loadClinics(), loadQuota(), loadOffers()]))
</script>

<template>
  <div class="offers-page">

    <!-- Header -->
    <div class="page-top">
      <div>
        <p class="page-kicker">إدارة العروض</p>
        <h1 class="page-title">عروض الأطباء</h1>
      </div>
      <div class="page-actions">
        <v-btn variant="outlined" color="primary" prepend-icon="mdi-refresh" :loading="loading" @click="loadOffers">تحديث</v-btn>
        <v-btn color="primary" prepend-icon="mdi-plus" @click="openEditor()">إضافة عرض</v-btn>
      </div>
    </div>

    <!-- Quota Cards -->
    <div class="quota-grid">
      <div class="quota-card">
        <div class="quota-icon"><v-icon icon="mdi-package-variant" color="primary" size="22" /></div>
        <div>
          <p class="quota-label">الباقة</p>
          <strong class="quota-value">{{ quota?.packageName ?? (isAdmin && !filters.doctorId ? 'اختر طبيباً' : 'لا توجد باقة') }}</strong>
        </div>
      </div>
      <div class="quota-card">
        <div class="quota-icon"><v-icon icon="mdi-percent" color="primary" size="22" /></div>
        <div>
          <p class="quota-label">العروض الفعالة</p>
          <strong class="quota-value">{{ quota?.activeOffers ?? 0 }} / {{ quota?.maxActiveOffers ?? 0 }}</strong>
        </div>
      </div>
      <div class="quota-card">
        <div class="quota-icon"><v-icon icon="mdi-check-circle" color="success" size="22" /></div>
        <div>
          <p class="quota-label">المتبقي</p>
          <strong class="quota-value">{{ quota?.remainingOffers ?? 0 }}</strong>
        </div>
      </div>
      <div class="quota-card" :class="{ 'quota-card--blocked': quota && !quota.canMakeOffers }">
        <div class="quota-icon">
          <v-icon
            :icon="quota?.canMakeOffers ? 'mdi-shield-check' : 'mdi-shield-off'"
            :color="quota?.canMakeOffers ? 'success' : 'error'"
            size="22"
          />
        </div>
        <div>
          <p class="quota-label">حالة الإنشاء</p>
          <strong class="quota-value">{{ quota?.canMakeOffers ? 'مسموح' : 'غير مسموح' }}</strong>
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div class="filters-bar">
      <div class="filter-field filter-field--search">
        <label class="filter-label">بحث</label>
        <div class="search-wrap">
          <v-icon icon="mdi-magnify" size="18" class="search-icon" />
          <input
            v-model="filters.search"
            class="search-input"
            placeholder="ابحث بالعنوان أو الطبيب أو العيادة"
            @keyup.enter="applyFilters"
          />
        </div>
      </div>
      <div v-if="isAdmin" class="filter-field">
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
        <label class="filter-label">العيادة</label>
        <v-autocomplete
          v-model="filters.clinicId"
          :items="[{ value: '', label: 'كل العيادات' }, ...clinics.map(c => ({ value: c.id, label: c.name }))]"
          item-title="label"
          item-value="value"
          class="filter-select"
          density="compact"
          variant="outlined"
          hide-details
          :disabled="isAdmin && !filters.doctorId"
        />
      </div>
      <div class="filter-field">
        <label class="filter-label">الحالة</label>
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
      <div class="filter-field">
        <label class="filter-label">الظهور</label>
        <v-autocomplete
          v-model="filters.currentlyVisible"
          :items="[
            { value: '', label: 'الكل' },
            { value: 'true', label: 'ظاهرة الآن' },
            { value: 'false', label: 'غير ظاهرة' },
          ]"
          item-title="label"
          item-value="value"
          class="filter-select"
          density="compact"
          variant="outlined"
          hide-details
        />
      </div>
      <v-btn color="primary" prepend-icon="mdi-filter" class="filter-btn" @click="applyFilters">تطبيق</v-btn>
    </div>

    <!-- Table Card -->
    <v-card elevation="0" class="table-card">
      <div class="table-toolbar">
        <div class="toolbar-left">
          <v-icon icon="mdi-tag-multiple" color="primary" size="20" />
          <strong>قائمة العروض</strong>
          <v-chip size="small" color="primary" variant="tonal">{{ totalItems }}</v-chip>
        </div>
        <span v-if="selectedDoctorName" class="toolbar-doctor">
          <v-icon icon="mdi-account" size="16" />
          {{ selectedDoctorName }}
        </span>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="table-loading">
        <v-skeleton-loader v-for="i in 5" :key="i" type="table-row" />
      </div>

      <!-- Empty -->
      <EmptyState
        v-else-if="!offers.length"
        icon="mdi-tag-off"
        title="لا توجد عروض"
        description="أضف عرضاً قصير المدة ليظهر حسب باقة الطبيب"
      />

      <!-- Table -->
      <div v-else class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th>العرض</th>
              <th v-if="isAdmin">الطبيب</th>
              <th>النطاق</th>
              <th>السعر / النوع</th>
              <th>مدة الظهور</th>
              <th>الحالة</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="offer in offers" :key="offer.id">
              <!-- Title -->
              <td>
                <div class="offer-title-cell">
                  <strong>{{ offer.title }}</strong>
                  <span class="offer-desc">{{ offer.description || offer.terms || 'بدون تفاصيل' }}</span>
                  <span v-if="offer.badgeText" class="offer-badge">{{ offer.badgeText }}</span>
                </div>
              </td>
              <!-- Doctor (Admin) -->
              <td v-if="isAdmin">{{ offer.doctorName }}</td>
              <!-- Scope -->
              <td>
                <v-chip size="x-small" variant="tonal" color="primary">{{ scopeLabel(offer) }}</v-chip>
              </td>
              <!-- Price -->
              <td>
                <strong class="price-value">{{ priceLabel(offer) }}</strong>
                <p class="row-sub">{{ offer.offerTypeName }}</p>
              </td>
              <!-- Duration -->
              <td>
                <span>{{ formatDate(offer.startsAt) }}</span>
                <span class="date-sep">-</span>
                <span>{{ formatDate(offer.endsAt) }}</span>
                <p class="row-sub">{{ offer.remainingDays }} يوم متبقي</p>
              </td>
              <!-- Status -->
              <td>
                <v-chip size="small" :color="statusMeta(offer).color" variant="tonal">
                  {{ statusMeta(offer).label }}
                </v-chip>
              </td>
              <!-- Actions -->
              <td>
                <div class="row-actions">
                  <v-btn icon size="small" variant="tonal" color="primary" @click="openEditor(offer)">
                    <v-icon icon="mdi-pencil" size="16" />
                  </v-btn>
                  <v-btn icon size="small" variant="tonal" color="error" @click="deleting = offer; deleteDialog = true">
                    <v-icon icon="mdi-delete" size="16" />
                  </v-btn>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="pagination-bar">
        <v-pagination
          v-model="page"
          :length="totalPages"
          :total-visible="5"
          density="compact"
          color="primary"
          @update:model-value="loadOffers"
        />
      </div>
    </v-card>

    <!-- ── Editor Dialog ── -->
    <v-dialog v-model="editorOpen" max-width="680" scrollable>
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon :icon="form.id ? 'mdi-pencil' : 'mdi-plus'" color="primary" size="20" />
          {{ form.id ? 'تعديل العرض' : 'إضافة عرض جديد' }}
        </v-card-title>
        <v-divider />

        <v-card-text class="dialog-body">
          <div class="form-grid">
            <!-- Doctor (Admin) -->
            <div v-if="isAdmin" class="form-field form-field--full">
              <label class="form-label">الطبيب <span class="required">*</span></label>
              <v-autocomplete
                v-model="form.doctorId"
                :items="doctors.map(d => ({ value: d.id, label: d.name }))"
                item-title="label"
                item-value="value"
                class="form-select"
                density="compact"
                variant="outlined"
                hide-details
                placeholder="اختر الطبيب"
                @update:model-value="onFormDoctorChanged"
              />
            </div>

            <!-- Title -->
            <div class="form-field form-field--full">
              <label class="form-label">عنوان العرض <span class="required">*</span></label>
              <input v-model.trim="form.title" class="form-input" maxlength="160" required placeholder="مثال: خصم فحص شامل لمدة أسبوع" />
            </div>

            <!-- Offer Type -->
            <div class="form-field">
              <label class="form-label">نوع العرض</label>
              <v-autocomplete
                v-model="form.offerType"
                :items="offerTypes"
                item-title="label"
                item-value="value"
                class="form-select"
                density="compact"
                variant="outlined"
                hide-details
              />
              <p class="field-hint">{{ selectedOfferType.hint }}</p>
            </div>

            <!-- Badge -->
            <div class="form-field">
              <label class="form-label">شارة قصيرة</label>
              <input v-model.trim="form.badgeText" class="form-input" maxlength="40" placeholder="عرض محدود" />
            </div>

            <!-- Original Price -->
            <div v-if="form.offerType !== '3'" class="form-field">
              <label class="form-label">السعر قبل العرض</label>
              <input v-model="form.originalPrice" type="number" min="0" step="0.01" class="form-input" placeholder="اختياري" />
            </div>

            <!-- Discount % -->
            <div v-if="form.offerType === '0'" class="form-field">
              <label class="form-label">نسبة الخصم <span class="required">*</span></label>
              <input v-model="form.discountPercent" type="number" min="1" max="100" step="0.01" class="form-input" required />
            </div>

            <!-- Offer Price -->
            <div v-if="form.offerType === '1' || form.offerType === '2'" class="form-field">
              <label class="form-label">سعر العرض <span class="required">*</span></label>
              <input v-model="form.offerPrice" type="number" min="1" step="0.01" class="form-input" required />
            </div>

            <!-- Dates -->
            <div class="form-field">
              <label class="form-label">تاريخ البداية <span class="required">*</span></label>
              <input v-model="form.startsAt" type="date" class="form-input" required />
            </div>
            <div class="form-field">
              <label class="form-label">تاريخ النهاية <span class="required">*</span></label>
              <input v-model="form.endsAt" type="date" class="form-input" required :min="form.startsAt" :max="maxEndDate" />
              <p class="field-hint">أقصى مدة ظهور: 7 أيام</p>
            </div>

            <!-- Scope -->
            <div class="form-field form-field--full">
              <label class="form-label">نطاق العرض</label>
              <div class="scope-row">
                <label class="radio-label">
                  <input v-model="form.appliesToAllClinics" :value="true" type="radio" />
                  <span>جميع العيادات</span>
                </label>
                <label class="radio-label">
                  <input v-model="form.appliesToAllClinics" :value="false" type="radio" />
                  <span>عيادة محددة</span>
                </label>
              </div>
            </div>

            <!-- Clinic -->
            <div v-if="!form.appliesToAllClinics" class="form-field form-field--full">
              <label class="form-label">العيادة <span class="required">*</span></label>
              <v-autocomplete
                v-model="form.clinicId"
                :items="clinics.map(c => ({ value: c.id, label: c.name }))"
                item-title="label"
                item-value="value"
                class="form-select"
                density="compact"
                variant="outlined"
                hide-details
                placeholder="اختر العيادة"
              />
            </div>

            <!-- Description -->
            <div class="form-field form-field--full">
              <label class="form-label">وصف العرض</label>
              <textarea v-model.trim="form.description" class="form-textarea" maxlength="800" rows="3" placeholder="تفاصيل سريعة للمريض عن العرض" />
            </div>

            <!-- Terms -->
            <div class="form-field form-field--full">
              <label class="form-label">الشروط</label>
              <textarea v-model.trim="form.terms" class="form-textarea" maxlength="600" rows="2" placeholder="مثال: لا يشمل التحاليل أو الأدوية" />
            </div>

            <!-- Active Toggle -->
            <div class="form-field form-field--full">
              <label class="check-label">
                <input v-model="form.isActive" type="checkbox" class="check-native" />
                <span class="check-box"><v-icon v-if="form.isActive" icon="mdi-check" size="12" color="white" /></span>
                <span>العرض فعال وقابل للظهور ضمن المدة المحددة</span>
              </label>
            </div>

            <!-- Quota Note -->
            <div v-if="quota" class="form-field form-field--full">
              <div class="quota-note" :class="{ 'quota-note--blocked': !quota.canMakeOffers }">
                <v-icon :icon="quota.canMakeOffers ? 'mdi-information' : 'mdi-alert'" size="16" />
                <span>
                  الباقة: <strong>{{ quota.packageName ?? '-' }}</strong>،
                  العروض الفعالة <strong>{{ quota.activeOffers }} / {{ quota.maxActiveOffers }}</strong>،
                  المتبقي <strong>{{ quota.remainingOffers }}</strong>
                </span>
              </div>
            </div>
          </div>
        </v-card-text>

        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="editorOpen = false">تراجع</v-btn>
          <v-btn color="primary" :loading="saving" :disabled="!canSave" @click="saveOffer">
            {{ form.id ? 'حفظ التعديلات' : 'إضافة العرض' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Delete Dialog ── -->
    <v-dialog v-model="deleteDialog" max-width="420">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-alert" color="error" size="20" />
          حذف العرض
        </v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          سيتم حذف عرض <strong>{{ deleting?.title }}</strong> ولن يظهر ضمن عروض الطبيب.
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="deleteDialog = false; deleting = undefined">تراجع</v-btn>
          <v-btn color="error" :loading="saving" @click="confirmDelete">تأكيد الحذف</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </div>
</template>

<style scoped>
.offers-page { display: flex; flex-direction: column; gap: var(--spacing-lg); }

/* Page Top */
.page-top { display: flex; align-items: center; justify-content: space-between; gap: var(--spacing-lg); flex-wrap: wrap; }
.page-kicker { margin: 0 0 4px 0; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--color-text-muted); }
.page-title { margin: 0; font-size: 28px; font-weight: 800; color: var(--color-text); }
.page-actions { display: flex; gap: var(--spacing-md); }

/* Quota Grid */
.quota-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: var(--spacing-lg); }
.quota-card {
  display: flex; align-items: center; gap: var(--spacing-lg);
  padding: var(--spacing-lg);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}
.quota-card--blocked { border-color: var(--color-error-light); background: var(--color-error-light); }
.quota-icon {
  display: flex; align-items: center; justify-content: center;
  width: 44px; height: 44px;
  border-radius: var(--radius-md);
  background: var(--color-primary-soft);
  flex-shrink: 0;
}
.quota-label { margin: 0 0 4px 0; font-size: 12px; font-weight: 600; color: var(--color-text-muted); }
.quota-value { font-size: 18px; font-weight: 800; color: var(--color-text); }

/* Filters */
.filters-bar {
  display: flex; align-items: flex-end; gap: var(--spacing-md); flex-wrap: wrap;
  padding: var(--spacing-lg);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}
.filter-field { display: flex; flex-direction: column; gap: 6px; }
.filter-field--search { flex: 1; min-width: 200px; }
.filter-label { font-size: 12px; font-weight: 700; color: var(--color-text-muted); }
.filter-select {
  height: 40px; padding: 0 12px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-size: 14px; outline: none; min-width: 140px;
  transition: border-color 0.2s;
}
.filter-select:focus { border-color: var(--color-primary); }
.search-wrap { position: relative; }
.search-icon { position: absolute; right: 10px; top: 50%; transform: translateY(-50%); color: var(--color-text-muted); pointer-events: none; }
[dir='rtl'] .search-icon { right: auto; left: 10px; }
.search-input {
  width: 100%; height: 40px;
  padding: 0 36px 0 12px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-size: 14px; outline: none;
  transition: border-color 0.2s;
}
[dir='rtl'] .search-input { padding: 0 12px 0 36px; }
.search-input:focus { border-color: var(--color-primary); }
.filter-btn { align-self: flex-end; }

/* Table Card */
.table-card { border: 1px solid var(--color-border) !important; border-radius: var(--radius-lg) !important; overflow: hidden; }
.table-toolbar {
  display: flex; align-items: center; justify-content: space-between;
  padding: var(--spacing-lg);
  border-bottom: 1px solid var(--color-border);
}
.toolbar-left { display: flex; align-items: center; gap: var(--spacing-md); }
.toolbar-left strong { font-size: 15px; color: var(--color-text); }
.toolbar-doctor { display: flex; align-items: center; gap: 4px; font-size: 13px; color: var(--color-text-muted); }
.table-loading { padding: var(--spacing-lg); }
.table-scroll { overflow-x: auto; }

.data-table { width: 100%; border-collapse: collapse; font-size: 13px; }
.data-table th {
  padding: 12px 16px; text-align: right;
  font-size: 12px; font-weight: 700; color: var(--color-text-muted);
  background: var(--color-background);
  border-bottom: 1px solid var(--color-border);
  white-space: nowrap;
}
.data-table td {
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border-light);
  vertical-align: middle; color: var(--color-text);
}
.data-table tbody tr:hover { background: var(--color-background); }
.data-table tbody tr:last-child td { border-bottom: none; }

.offer-title-cell { display: flex; flex-direction: column; gap: 4px; min-width: 200px; }
.offer-title-cell strong { font-size: 14px; font-weight: 700; }
.offer-desc { font-size: 12px; color: var(--color-text-muted); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 220px; }
.offer-badge {
  display: inline-block; width: fit-content;
  padding: 3px 8px;
  border: 1px solid #f0c05b;
  border-radius: 999px;
  background: #fff8df;
  color: #9a6500;
  font-size: 11px; font-weight: 800;
}
.price-value { font-size: 15px; font-weight: 800; color: var(--color-primary); }
.row-sub { margin: 2px 0 0 0; font-size: 11px; color: var(--color-text-muted); }
.date-sep { margin: 0 4px; color: var(--color-text-muted); }
.row-actions { display: flex; gap: var(--spacing-sm); }
.pagination-bar { display: flex; justify-content: center; padding: var(--spacing-lg); border-top: 1px solid var(--color-border); }

/* Dialog */
.dialog-title { display: flex; align-items: center; gap: var(--spacing-md); padding: var(--spacing-lg) !important; font-size: 16px !important; font-weight: 700; color: var(--color-text); }
.dialog-body { padding: var(--spacing-lg) !important; }
.dialog-actions { padding: var(--spacing-lg) !important; gap: var(--spacing-md); justify-content: flex-end; }

/* Form */
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--spacing-lg); }
.form-field { display: flex; flex-direction: column; gap: 6px; }
.form-field--full { grid-column: 1 / -1; }
.form-label { font-size: 13px; font-weight: 600; color: var(--color-text); }
.required { color: var(--color-error); }
.form-input, .form-select, .form-textarea {
  padding: 10px 12px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface); color: var(--color-text);
  font-family: var(--font-family-primary); font-size: 14px;
  outline: none; width: 100%; transition: border-color 0.2s;
}
.form-input:focus, .form-select:focus, .form-textarea:focus { border-color: var(--color-primary); }
.form-textarea { resize: vertical; }
.field-hint { margin: 2px 0 0 0; font-size: 12px; color: var(--color-text-muted); }

/* Scope Row */
.scope-row {
  display: flex; gap: var(--spacing-xl); flex-wrap: wrap;
  padding: var(--spacing-md) var(--spacing-lg);
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-background);
}
.radio-label { display: flex; align-items: center; gap: var(--spacing-md); font-size: 14px; font-weight: 500; color: var(--color-text); cursor: pointer; }
.radio-label input[type="radio"] { accent-color: var(--color-primary); }

/* Check */
.check-label { display: flex; align-items: center; gap: var(--spacing-md); cursor: pointer; font-size: 14px; font-weight: 500; color: var(--color-text); user-select: none; }
.check-native { position: absolute; opacity: 0; width: 0; height: 0; }
.check-box { width: 18px; height: 18px; border: 2px solid var(--color-border); border-radius: 4px; background: white; display: flex; align-items: center; justify-content: center; flex-shrink: 0; transition: all 0.15s ease; }
.check-native:checked ~ .check-box { background: var(--color-primary); border-color: var(--color-primary); }

/* Quota Note */
.quota-note {
  display: flex; align-items: center; gap: var(--spacing-md);
  padding: var(--spacing-md) var(--spacing-lg);
  border-radius: var(--radius-md);
  background: var(--color-success-light);
  color: var(--color-success);
  font-size: 13px;
}
.quota-note--blocked { background: var(--color-warning-light); color: var(--color-warning); }

/* Responsive */
@media (max-width: 1100px) { .quota-grid { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 768px) { .quota-grid { grid-template-columns: 1fr; } .filters-bar { flex-direction: column; } .form-grid { grid-template-columns: 1fr; } }
</style>
