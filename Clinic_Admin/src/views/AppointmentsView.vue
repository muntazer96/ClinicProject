<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'
import { useNotifications } from '../composables/useNotifications'
import type { ApiResponse, AppointmentItem, ClinicItem, DoctorItem, PageResult, QueueAvailabilityItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import PageHeader from '../components/common/Pageheader.vue'
import EmptyState from '../components/common/Emptystate.vue'

interface AppointmentRow {
  id: number
  code: string
  patientName: string
  patientPhoneNumber?: string
  appointmentDate: string
  queueNumber: number
  status: number
  isPhoneConfirmed: boolean
  clinicId: number
  clinicName: string
  doctorName?: string
  paymentStatus?: number
  paymentAmount?: number
  isGuestBooking: boolean
  bookingSource: string
}

const auth = useAuthStore()
const { success: showSuccess, error: showError } = useNotifications()

const isDoctor = computed(() => auth.hasAnyRole(['DoctorUser']))
const isAdmin  = computed(() => auth.hasAnyRole(['SuperAdmin']) && !isDoctor.value)

const appointments     = ref<AppointmentRow[]>([])
const clinics          = ref<ClinicItem[]>([])
const doctors          = ref<DoctorItem[]>([])
const loading          = ref(false)
const saving           = ref(false)
const manualOpen       = ref(false)
const cancelDialog     = ref(false)
const cancelAppointment = ref<AppointmentRow>()
const page             = ref(1)
const pageSize         = 10
const totalPages       = ref(1)
const totalItems       = ref(0)
const queueAvailability = ref<QueueAvailabilityItem>()
const queueLoading     = ref(false)

const filters = reactive({
  doctorId: '', clinicId: '', fromDate: today(), toDate: today(), status: '',
})
const manualForm = reactive({
  clinicId: '', appointmentDate: today(), patientName: '', patientPhoneNumber: '', notes: '',
})

const statusOptions = [
  { value: '', label: 'كل الحالات' },
  { value: '0', label: 'قيد الانتظار' },
  { value: '1', label: 'مؤكد' },
  { value: '2', label: 'ملغي' },
  { value: '3', label: 'مكتمل' },
]

const canCreateManual = computed(() => {
  if (!manualForm.clinicId || !manualForm.appointmentDate) return false
  if (!queueAvailability.value) return true
  return queueAvailability.value.isAvailable && queueAvailability.value.remainingAppointments > 0
})

const visibleAppointments = computed(() => {
  return appointments.value
})

// Helpers
function today() { return new Date().toLocaleDateString('en-CA') }

function normalizeStatus(status: unknown) {
  if (typeof status === 'number') return status
  if (typeof status !== 'string') return 0
  const s = status.toLowerCase()
  if (s === 'confirmed') return 1
  if (s === 'cancelled' || s === 'canceled') return 2
  if (s === 'completed') return 3
  return 0
}

function normalizePaymentStatus(status: unknown) {
  if (typeof status === 'number') return status
  if (typeof status !== 'string') return 0
  const s = status.toLowerCase()
  if (s === 'completed' || s === 'paid') return 1
  if (s === 'failed') return 2
  return 0
}

function statusMeta(status: number) {
  return [
    { label: 'قيد الانتظار', color: 'warning' },
    { label: 'مؤكد',         color: 'success' },
    { label: 'ملغي',          color: 'error'   },
    { label: 'مكتمل',        color: 'default'  },
  ][status] ?? { label: 'غير معروف', color: 'default' }
}

function paymentMeta(status?: number) {
  return [
    { label: 'قيد الانتظار', color: 'default'  },
    { label: 'مدفوع',        color: 'success'  },
    { label: 'فشل الدفع',   color: 'error'    },
  ][status ?? 0] ?? { label: 'غير معروف', color: 'default' }
}

function bookingSourceMeta(a: AppointmentRow) {
  return a.isGuestBooking
    ? { label: 'زائر',         color: 'warning' }
    : { label: 'حساب مسجل',   color: 'success' }
}

function isGuestBooking(item: any) {
  if (typeof item.isGuestBooking === 'boolean') return item.isGuestBooking
  if (typeof item.bookingSource === 'string') return item.bookingSource.toLowerCase() === 'guest'
  return !item.user && !item.userId
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium' }).format(new Date(value))
}

function mapDoctorAppointment(item: AppointmentItem): AppointmentRow {
  return {
    id: item.id, code: item.code,
    patientName: item.patientName, patientPhoneNumber: item.patientPhoneNumber,
    appointmentDate: item.appointmentDate, queueNumber: item.queueNumber,
    status: normalizeStatus(item.status), isPhoneConfirmed: item.isPhoneConfirmed,
    isGuestBooking: isGuestBooking(item),
    bookingSource: item.bookingSource ?? (isGuestBooking(item) ? 'Guest' : 'Registered'),
    clinicId: item.clinicId, clinicName: item.clinicName,
  }
}

function mapAdminAppointment(item: any): AppointmentRow {
  return {
    id: item.id, code: item.code,
    patientName: item.user?.name ?? item.guestName ?? item.name ?? 'مراجع',
    patientPhoneNumber: item.user?.phoneNumber ?? item.guestPhoneNumber ?? item.phoneNumber,
    appointmentDate: item.appointmentDate, queueNumber: item.queueNumber,
    status: normalizeStatus(item.status), isPhoneConfirmed: item.isPhoneConfirmed,
    isGuestBooking: isGuestBooking(item),
    bookingSource: item.bookingSource ?? (isGuestBooking(item) ? 'Guest' : 'Registered'),
    clinicId: item.clinic?.id ?? item.clinicId,
    clinicName: item.clinic?.name ?? item.clinicName ?? '-',
    doctorName: item.doctor?.name ?? item.doctorName,
    paymentAmount: item.paymentAmount,
    paymentStatus: normalizePaymentStatus(item.paymentStatus),
  }
}

function unwrapPageResult<T>(payload: ApiResponse<PageResult<T>> | PageResult<T>): PageResult<T> {
  return 'items' in payload ? payload : payload.data
}

// API calls
async function loadClinics() {
  if (isAdmin.value) return
  try {
    const r = await api.get<ApiResponse<ClinicItem[]>>('/Clinic/my')
    clinics.value = r.data.data
  } catch (e: any) {
    if (e.response?.status === 404) clinics.value = []
    else showError(getErrorMessage(e))
  }
}

async function loadDoctors() {
  if (!isAdmin.value) return
  try {
    const r = await api.get<ApiResponse<PageResult<DoctorItem>>>('/Doctor', { params: { page: 1, pageSize: 100 } })
    doctors.value = r.data.data.items
  } catch (e: any) {
    if (e.response?.status === 404) doctors.value = []
    else showError(getErrorMessage(e))
  }
}

async function loadAdminClinics(doctorId: string) {
  filters.clinicId = ''
  clinics.value = []
  if (!doctorId) return
  try {
    const r = await api.get<ApiResponse<ClinicItem[]>>(`/Clinic/doctor/${doctorId}/admin`)
    clinics.value = r.data.data
  } catch (e: any) {
    if (e.response?.status !== 404) showError(getErrorMessage(e))
  }
}

async function loadAppointments() {
  loading.value = true
  try {
    if (isAdmin.value) {
      const r = await api.get<ApiResponse<PageResult<any>>>('/Appointment/GetListAsync', {
        params: {
          doctorId: filters.doctorId || undefined,
          clinicId: filters.clinicId || undefined,
          fromDate: filters.fromDate || undefined,
          toDate: filters.toDate || undefined,
          status: filters.status === '' ? undefined : filters.status,
          page: page.value, pageSize,
        },
      })
      const pageData = unwrapPageResult(r.data)
      appointments.value = pageData.items.map(mapAdminAppointment)
      totalPages.value = pageData.totalPages
      totalItems.value = pageData.totalItems
    } else {
      const r = await api.get<ApiResponse<PageResult<AppointmentItem>> | PageResult<AppointmentItem>>('/Appointment/doctor/my', {
        params: {
          clinicId: filters.clinicId || undefined,
          fromDate: filters.fromDate || undefined,
          toDate: filters.toDate || undefined,
          status: filters.status === '' ? undefined : filters.status,
          page: page.value,
          pageSize,
        },
      })
      const pageData = unwrapPageResult(r.data)
      appointments.value = pageData.items.map(mapDoctorAppointment)
      totalPages.value = pageData.totalPages
      totalItems.value = pageData.totalItems
    }
  } catch (e: any) {
    if (e.response?.status === 404) {
      appointments.value = []; totalPages.value = 1; totalItems.value = 0
    } else showError(getErrorMessage(e))
  } finally { loading.value = false }
}

async function loadQueueAvailability() {
  queueAvailability.value = undefined
  if (!manualForm.clinicId || !manualForm.appointmentDate) return
  queueLoading.value = true
  try {
    const r = await api.get<ApiResponse<QueueAvailabilityItem[]>>(
      `/Appointment/queue-availability/${manualForm.clinicId}`,
      { params: { fromDate: manualForm.appointmentDate, days: 1 } },
    )
    queueAvailability.value = r.data.data[0]
  } catch (e) { showError(getErrorMessage(e)) }
  finally { queueLoading.value = false }
}

async function toggleStatus(a: AppointmentRow) {
  try {
    const r = await api.post<ApiResponse<object>>('/Appointment/toggle-status', null, { params: { appointmentId: a.id } })
    showSuccess(r.data.message)
    await loadAppointments()
  } catch (e) { showError(getErrorMessage(e)) }
}

async function rejectPending(a: AppointmentRow) {
  try {
    const r = await api.post<ApiResponse<object>>('/Appointment/reject-pending', null, { params: { appointmentId: a.id } })
    showSuccess(r.data.message)
    await loadAppointments()
  } catch (e) { showError(getErrorMessage(e)) }
}

async function confirmCancel() {
  if (!cancelAppointment.value) return
  if (cancelAppointment.value.status === 0) await rejectPending(cancelAppointment.value)
  else await toggleStatus(cancelAppointment.value)
  cancelAppointment.value = undefined
  cancelDialog.value = false
}

async function complete(a: AppointmentRow) {
  try {
    const r = await api.post<ApiResponse<object>>('/Appointment/complete', null, { params: { appointmentId: a.id } })
    showSuccess(r.data.message)
    await loadAppointments()
  } catch (e) { showError(getErrorMessage(e)) }
}

function openManualBooking() {
  Object.assign(manualForm, {
    clinicId: clinics.value[0]?.id ? String(clinics.value[0].id) : '',
    appointmentDate: today(), patientName: '', patientPhoneNumber: '', notes: '',
  })
  manualOpen.value = true
  loadQueueAvailability()
}

async function createManualBooking() {
  saving.value = true
  try {
    const r = await api.post<ApiResponse<object>>('/Appointment/manual', {
      clinicId: Number(manualForm.clinicId),
      appointmentDate: manualForm.appointmentDate,
      patientName: manualForm.patientName,
      patientPhoneNumber: manualForm.patientPhoneNumber,
      notes: manualForm.notes || null,
    })
    showSuccess(r.data.message)
    manualOpen.value = false
    await loadAppointments()
  } catch (e) { showError(getErrorMessage(e)) }
  finally { saving.value = false }
}

function applyFilters() { page.value = 1; loadAppointments() }

function changePage(n: number) {
  page.value = n
  loadAppointments()
}

watch(() => filters.doctorId, (id) => { if (isAdmin.value) loadAdminClinics(id) })
watch(() => [manualForm.clinicId, manualForm.appointmentDate], loadQueueAvailability)

onMounted(() => Promise.all([loadClinics(), loadDoctors()]).then(loadAppointments))
</script>

<template>
  <div class="appointments-page">

    <!-- Header -->
    <PageHeader
      :title="isAdmin ? 'كل الحجوزات' : 'الحجوزات اليومية'"
      subtitle="متابعة المراجعين"
    >
      <template #actions>
        <v-btn
          variant="outlined"
          color="primary"
          prepend-icon="mdi-refresh"
          :loading="loading"
          @click="loadAppointments"
        >
          تحديث
        </v-btn>
        <v-btn
          v-if="!isAdmin"
          color="primary"
          prepend-icon="mdi-plus"
          :disabled="!clinics.length"
          @click="openManualBooking"
        >
          حجز يدوي
        </v-btn>
      </template>
    </PageHeader>

    <!-- Filters -->
    <div class="filters-bar">
      <!-- Doctor (Admin) -->
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

      <!-- Clinic -->
      <div class="filter-field">
        <label class="filter-label">العيادة</label>
        <v-autocomplete
          v-model="filters.clinicId"
          :items="[{ value: '', label: isAdmin ? 'كل عيادات الطبيب' : 'كل العيادات' }, ...clinics.map(c => ({ value: c.id, label: c.name }))]"
          item-title="label"
          item-value="value"
          class="filter-select"
          density="compact"
          variant="outlined"
          hide-details
          :disabled="isAdmin && !filters.doctorId"
        />
      </div>

      <!-- From Date -->
      <div class="filter-field">
        <label class="filter-label">من</label>
        <input v-model="filters.fromDate" type="date" class="filter-input" />
      </div>

      <!-- To Date -->
      <div class="filter-field">
        <label class="filter-label">إلى</label>
        <input v-model="filters.toDate" type="date" class="filter-input" />
      </div>

      <!-- Status -->
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

      <!-- Search Button -->
      <v-btn color="primary" prepend-icon="mdi-magnify" class="filter-btn" @click="applyFilters">
        بحث
      </v-btn>
    </div>

    <!-- Table -->
    <v-card elevation="0" class="table-card">
      <div class="table-toolbar">
        <v-icon icon="mdi-calendar-check" color="primary" size="20" />
        <strong>قائمة الحجوزات</strong>
        <v-chip size="small" color="primary" variant="tonal">{{ totalItems }} حجز</v-chip>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="table-loading">
        <v-skeleton-loader v-for="i in 5" :key="i" type="table-row" />
      </div>

      <!-- Empty -->
      <EmptyState
        v-else-if="!appointments.length"
        icon="mdi-calendar-blank"
        title="لا توجد حجوزات"
        description="لا توجد حجوزات مطابقة للفلاتر المحددة"
      />

      <!-- Table -->
      <div v-else class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th>الدور</th>
              <th>المراجع</th>
              <th>نوع الحجز</th>
              <th v-if="isAdmin">الطبيب</th>
              <th>العيادة</th>
              <th>التاريخ</th>
              <th>التحقق</th>
              <th>الحالة</th>
              <th v-if="isAdmin">الدفع</th>
              <th v-if="!isAdmin">الإجراءات</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="a in visibleAppointments" :key="a.id">
              <!-- Queue # -->
              <td>
                <strong class="queue-number">#{{ a.queueNumber }}</strong>
                <p class="row-sub">{{ a.code }}</p>
              </td>

              <!-- Patient -->
              <td>
                <strong>{{ a.patientName || 'مراجع' }}</strong>
                <p class="row-sub">{{ a.patientPhoneNumber || '-' }}</p>
              </td>

              <!-- Booking Source -->
              <td>
                <v-chip
                  size="small"
                  :color="bookingSourceMeta(a).color"
                  variant="tonal"
                >
                  {{ bookingSourceMeta(a).label }}
                </v-chip>
              </td>

              <!-- Doctor (Admin) -->
              <td v-if="isAdmin">{{ a.doctorName || '-' }}</td>

              <!-- Clinic -->
              <td>{{ a.clinicName }}</td>

              <!-- Date -->
              <td>{{ formatDate(a.appointmentDate) }}</td>

              <!-- Phone Confirmed -->
              <td>
                <v-chip
                  size="small"
                  :color="a.isPhoneConfirmed ? 'success' : 'warning'"
                  variant="tonal"
                >
                  {{ a.isPhoneConfirmed ? 'مؤكد' : 'بانتظار OTP' }}
                </v-chip>
              </td>

              <!-- Status -->
              <td>
                <v-chip
                  size="small"
                  :color="statusMeta(a.status).color"
                  variant="tonal"
                >
                  {{ statusMeta(a.status).label }}
                </v-chip>
              </td>

              <!-- Payment (Admin) -->
              <td v-if="isAdmin">
                <v-chip
                  size="small"
                  :color="paymentMeta(a.paymentStatus).color"
                  variant="tonal"
                >
                  {{ paymentMeta(a.paymentStatus).label }}
                </v-chip>
              </td>

              <!-- Actions (Doctor) -->
              <td v-if="!isAdmin">
                <div class="row-actions">
                  <!-- Confirm -->
                  <v-btn
                    v-if="a.status === 0"
                    icon
                    size="small"
                    variant="tonal"
                    color="success"
                    aria-label="تأكيد الحجز"
                    @click="toggleStatus(a)"
                  >
                    <v-icon icon="mdi-check" size="16" />
                  </v-btn>

                  <!-- Call -->
                  <v-btn
                    v-if="a.patientPhoneNumber"
                    icon
                    size="small"
                    variant="tonal"
                    color="info"
                    :href="`tel:${a.patientPhoneNumber}`"
                    tag="a"
                    aria-label="اتصال بالمريض"
                  >
                    <v-icon icon="mdi-phone" size="16" />
                  </v-btn>

                  <!-- Cancel -->
                  <v-btn
                    v-if="a.status === 0 || a.status === 1"
                    icon
                    size="small"
                    variant="tonal"
                    color="error"
                    aria-label="إلغاء الحجز"
                    @click="cancelAppointment = a; cancelDialog = true"
                  >
                    <v-icon icon="mdi-close" size="16" />
                  </v-btn>

                  <!-- Complete -->
                  <v-btn
                    v-if="a.status === 1"
                    icon
                    size="small"
                    variant="tonal"
                    color="primary"
                    aria-label="إكمال الحجز"
                    @click="complete(a)"
                  >
                    <v-icon icon="mdi-check-all" size="16" />
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
          @update:model-value="changePage"
        />
      </div>
    </v-card>

    <!-- Manual Booking Dialog -->
    <v-dialog v-model="manualOpen" max-width="500">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-calendar-plus" color="primary" size="22" />
          إضافة حجز يدوي
        </v-card-title>
        <v-divider />

        <v-card-text class="dialog-body">
          <p class="dialog-desc">
            أدخل بيانات المراجع القادم عبر الهاتف أو الاستقبال. يضاف الحجز مؤكداً مباشرة بدون OTP.
          </p>

          <div class="form-fields">
            <!-- Clinic -->
            <div class="form-field">
              <label class="form-label">العيادة</label>
              <v-autocomplete
                v-model="manualForm.clinicId"
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

            <!-- Date -->
            <div class="form-field">
              <label class="form-label">تاريخ الحجز</label>
              <input v-model="manualForm.appointmentDate" type="date" class="form-input" :min="today()" required />
            </div>

            <!-- Queue Availability -->
            <div v-if="queueLoading" class="queue-box">
              <v-progress-circular size="16" width="2" indeterminate color="primary" />
              جارِ فحص توفر الأدوار...
            </div>
            <div
              v-else-if="queueAvailability"
              class="queue-box"
              :class="{ 'queue-unavailable': !queueAvailability.isAvailable }"
            >
              <v-icon
                :icon="queueAvailability.isAvailable ? 'mdi-check-circle' : 'mdi-alert-circle'"
                :color="queueAvailability.isAvailable ? 'success' : 'error'"
                size="18"
              />
              <div>
                <strong>{{ queueAvailability.isAvailable ? 'الأدوار المتاحة' : 'اليوم غير متاح' }}</strong>
                <p v-if="queueAvailability.isAvailable">
                  {{ queueAvailability.remainingAppointments }} متبقي من {{ queueAvailability.maxAppointments }}
                </p>
                <p v-else>{{ queueAvailability.closureReason || 'لا يوجد دوام لهذا اليوم.' }}</p>
              </div>
            </div>

            <!-- Patient Name -->
            <div class="form-field">
              <label class="form-label">اسم المراجع</label>
              <input v-model="manualForm.patientName" class="form-input" required maxlength="200" />
            </div>

            <!-- Phone -->
            <div class="form-field">
              <label class="form-label">رقم الهاتف</label>
              <input v-model="manualForm.patientPhoneNumber" class="form-input" required maxlength="30" placeholder="07XXXXXXXXX" />
            </div>

            <!-- Notes -->
            <div class="form-field">
              <label class="form-label">ملاحظات</label>
              <textarea v-model="manualForm.notes" class="form-textarea" rows="3" maxlength="1000" />
            </div>
          </div>
        </v-card-text>

        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="manualOpen = false">تراجع</v-btn>
          <v-btn
            color="primary"
            :loading="saving"
            :disabled="saving || queueLoading || !canCreateManual"
            @click="createManualBooking"
          >
            تثبيت الحجز
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Cancel Confirm Dialog -->
    <v-dialog v-model="cancelDialog" max-width="420">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-alert" color="error" size="22" />
          تأكيد إلغاء الحجز
        </v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          <p>
            سيتم إلغاء حجز
            <strong>{{ cancelAppointment?.patientName || 'المراجع' }}</strong>
            ذي الدور
            <strong>#{{ cancelAppointment?.queueNumber }}</strong>.
            هل تريد المتابعة؟
          </p>
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="cancelDialog = false; cancelAppointment = undefined">تراجع</v-btn>
          <v-btn color="error" @click="confirmCancel">تأكيد الإلغاء</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </div>
</template>

<style scoped>
.appointments-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

/* Filters */
.filters-bar {
  display: flex;
  align-items: flex-end;
  gap: var(--spacing-md);
  flex-wrap: wrap;
  padding: var(--spacing-lg);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}

.filter-field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.filter-label {
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-muted);
}

.filter-input,
.filter-select {
  height: 40px;
  padding: 0 12px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-size: 14px;
  outline: none;
  min-width: 140px;
  transition: border-color 0.2s;
}

.filter-input:focus,
.filter-select:focus { border-color: var(--color-primary); }

.filter-btn { align-self: flex-end; }

/* Table Card */
.table-card {
  border: 1px solid var(--color-border) !important;
  border-radius: var(--radius-lg) !important;
  overflow: hidden;
}

.table-toolbar {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-lg);
  border-bottom: 1px solid var(--color-border);
  background: var(--color-surface);
}

.table-toolbar strong {
  flex: 1;
  font-size: 15px;
  color: var(--color-text);
}

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

.data-table tbody tr:hover {
  background: var(--color-background);
}

.data-table tbody tr:last-child td {
  border-bottom: none;
}

.queue-number {
  font-size: 15px;
  font-weight: 800;
  color: var(--color-primary);
}

.row-sub {
  margin: 2px 0 0 0;
  font-size: 11px;
  color: var(--color-text-muted);
}

.row-actions {
  display: flex;
  gap: var(--spacing-sm);
  align-items: center;
}

/* Pagination */
.pagination-bar {
  display: flex;
  justify-content: center;
  padding: var(--spacing-lg);
  border-top: 1px solid var(--color-border);
}

/* Dialog */
.dialog-title {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-lg) !important;
  font-size: 16px !important;
  font-weight: 700;
  color: var(--color-text);
}

.dialog-body { padding: var(--spacing-lg) !important; }

.dialog-desc {
  margin: 0 0 var(--spacing-lg) 0;
  font-size: 13px;
  color: var(--color-text-muted);
  line-height: 1.6;
}

.form-fields {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

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
.form-select,
.form-textarea {
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
.form-select:focus,
.form-textarea:focus { border-color: var(--color-primary); }

.form-textarea { resize: vertical; min-height: 80px; }

/* Queue Box */
.queue-box {
  display: flex;
  align-items: flex-start;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  border-radius: var(--radius-md);
  background: var(--color-success-light);
  color: var(--color-success);
  font-size: 13px;
}

.queue-box strong { display: block; font-weight: 700; margin-bottom: 2px; }
.queue-box p { margin: 0; opacity: 0.8; font-size: 12px; }

.queue-unavailable {
  background: var(--color-error-light);
  color: var(--color-error);
}

.dialog-actions {
  padding: var(--spacing-lg) !important;
  gap: var(--spacing-md);
  justify-content: flex-end;
}

/* Responsive */
@media (max-width: 768px) {
  .filters-bar { flex-direction: column; }
  .filter-input, .filter-select { min-width: 100%; }
  .data-table th, .data-table td { padding: 10px 12px; }
}
</style>
