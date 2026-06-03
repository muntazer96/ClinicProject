<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { CalendarCheck, Check, CheckCheck, Plus, RefreshCw, Search, X } from '@lucide/vue'
import AppModal from '../components/AppModal.vue'
import AppPagination from '../components/AppPagination.vue'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, AppointmentItem, ClinicItem, DoctorItem, PageResult, QueueAvailabilityItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'

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
}

const auth = useAuthStore()
const notifications = useNotificationsStore()
const isAdmin = auth.hasAnyRole(['SuperAdmin'])
const appointments = ref<AppointmentRow[]>([])
const clinics = ref<ClinicItem[]>([])
const doctors = ref<DoctorItem[]>([])
const loading = ref(false)
const saving = ref(false)
const manualOpen = ref(false)
const cancelAppointment = ref<AppointmentRow>()
const page = ref(1)
const doctorPage = ref(1)
const doctorPageSize = 10
const totalPages = ref(1)
const totalItems = ref(0)
const queueAvailability = ref<QueueAvailabilityItem>()
const queueLoading = ref(false)
const filters = reactive({ doctorId: '', clinicId: '', fromDate: today(), toDate: today(), status: '' })
const manualForm = reactive({ clinicId: '', appointmentDate: today(), patientName: '', patientPhoneNumber: '', notes: '' })
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
const doctorTotalPages = computed(() => Math.max(1, Math.ceil(appointments.value.length / doctorPageSize)))
const visibleAppointments = computed(() => {
  if (isAdmin) return appointments.value
  return appointments.value.slice((doctorPage.value - 1) * doctorPageSize, doctorPage.value * doctorPageSize)
})

function today() {
  return new Date().toLocaleDateString('en-CA')
}

function normalizeStatus(status: unknown) {
  if (typeof status === 'number') return status
  if (typeof status !== 'string') return 0
  const normalized = status.toLowerCase()
  if (normalized === 'confirmed') return 1
  if (normalized === 'cancelled' || normalized === 'canceled') return 2
  if (normalized === 'completed') return 3
  return 0
}

function normalizePaymentStatus(status: unknown) {
  if (typeof status === 'number') return status
  if (typeof status !== 'string') return 0
  const normalized = status.toLowerCase()
  if (normalized === 'completed' || normalized === 'paid') return 1
  if (normalized === 'failed') return 2
  return 0
}

function statusMeta(status: number) {
  return [
    { label: 'قيد الانتظار', className: 'status-warning' },
    { label: 'مؤكد', className: 'status-success' },
    { label: 'ملغي', className: 'status-danger' },
    { label: 'مكتمل', className: 'status-neutral' },
  ][status] ?? { label: 'غير معروف', className: 'status-neutral' }
}

function paymentMeta(status?: number) {
  return [
    { label: 'قيد الانتظار', className: 'status-neutral' },
    { label: 'مدفوع', className: 'status-success' },
    { label: 'فشل الدفع', className: 'status-danger' },
  ][status ?? 0] ?? { label: 'غير معروف', className: 'status-neutral' }
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium' }).format(new Date(value))
}

function mapDoctorAppointment(item: AppointmentItem): AppointmentRow {
  return {
    id: item.id,
    code: item.code,
    patientName: item.patientName,
    patientPhoneNumber: item.patientPhoneNumber,
    appointmentDate: item.appointmentDate,
    queueNumber: item.queueNumber,
    status: normalizeStatus(item.status),
    isPhoneConfirmed: item.isPhoneConfirmed,
    clinicId: item.clinicId,
    clinicName: item.clinicName,
  }
}

function mapAdminAppointment(item: any): AppointmentRow {
  return {
    id: item.id,
    code: item.code,
    patientName: item.user?.name ?? item.guestName ?? item.name ?? 'مراجع',
    patientPhoneNumber: item.user?.phoneNumber ?? item.guestPhoneNumber ?? item.phoneNumber,
    appointmentDate: item.appointmentDate,
    queueNumber: item.queueNumber,
    status: normalizeStatus(item.status),
    isPhoneConfirmed: item.isPhoneConfirmed,
    clinicId: item.clinic?.id ?? item.clinicId,
    clinicName: item.clinic?.name ?? item.clinicName ?? '-',
    doctorName: item.doctor?.name ?? item.doctorName,
    paymentAmount: item.paymentAmount,
    paymentStatus: normalizePaymentStatus(item.paymentStatus),
  }
}

async function loadClinics() {
  if (isAdmin) return
  const response = await api.get<ApiResponse<ClinicItem[]>>('/Clinic/my')
  clinics.value = response.data.data
}

async function loadDoctors() {
  if (!isAdmin) return
  const response = await api.get<ApiResponse<PageResult<DoctorItem>>>('/Doctor', { params: { page: 1, pageSize: 100 } })
  doctors.value = response.data.data.items
}

async function loadAdminClinics(doctorId: string) {
  filters.clinicId = ''
  clinics.value = []
  if (!doctorId) return
  try {
    const response = await api.get<ApiResponse<ClinicItem[]>>(`/Clinic/doctor/${doctorId}/admin`)
    clinics.value = response.data.data
  } catch (error: any) {
    if (error.response?.status !== 404) notifications.show(getErrorMessage(error), 'error')
  }
}

async function loadAppointments() {
  loading.value = true
  try {
    if (isAdmin) {
      const response = await api.get<ApiResponse<PageResult<any>>>('/Appointment/GetListAsync', {
        params: {
          doctorId: filters.doctorId || undefined,
          clinicId: filters.clinicId || undefined,
          fromDate: filters.fromDate || undefined,
          toDate: filters.toDate || undefined,
          status: filters.status === '' ? undefined : filters.status,
          page: page.value,
          pageSize: 10,
        },
      })
      appointments.value = response.data.data.items.map(mapAdminAppointment)
      totalPages.value = response.data.data.totalPages
      totalItems.value = response.data.data.totalItems
    } else {
      const response = await api.get<ApiResponse<AppointmentItem[]>>('/Appointment/doctor/my', {
        params: {
          clinicId: filters.clinicId || undefined,
          fromDate: filters.fromDate || undefined,
          toDate: filters.toDate || undefined,
          status: filters.status === '' ? undefined : filters.status,
        },
      })
      appointments.value = response.data.data.map(mapDoctorAppointment)
      totalPages.value = 1
      totalItems.value = appointments.value.length
      if (doctorPage.value > doctorTotalPages.value) doctorPage.value = doctorTotalPages.value
    }
  } catch (error: any) {
    if (error.response?.status === 404) {
      appointments.value = []
      totalPages.value = 1
      totalItems.value = 0
    } else notifications.show(getErrorMessage(error), 'error')
  } finally {
    loading.value = false
  }
}

async function loadQueueAvailability() {
  queueAvailability.value = undefined
  if (!manualForm.clinicId || !manualForm.appointmentDate) return
  queueLoading.value = true
  try {
    const response = await api.get<ApiResponse<QueueAvailabilityItem[]>>(`/Appointment/queue-availability/${manualForm.clinicId}`, {
      params: { fromDate: manualForm.appointmentDate, days: 1 },
    })
    queueAvailability.value = response.data.data[0]
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    queueLoading.value = false
  }
}

async function toggleStatus(appointment: AppointmentRow) {
  try {
    const response = await api.post<ApiResponse<object>>('/Appointment/toggle-status', null, { params: { appointmentId: appointment.id } })
    notifications.show(response.data.message)
    await loadAppointments()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

async function confirmCancel() {
  if (!cancelAppointment.value) return
  await toggleStatus(cancelAppointment.value)
  cancelAppointment.value = undefined
}

async function complete(appointment: AppointmentRow) {
  try {
    const response = await api.post<ApiResponse<object>>('/Appointment/complete', null, { params: { appointmentId: appointment.id } })
    notifications.show(response.data.message)
    await loadAppointments()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

function openManualBooking() {
  Object.assign(manualForm, {
    clinicId: clinics.value[0]?.id ? String(clinics.value[0].id) : '',
    appointmentDate: today(),
    patientName: '',
    patientPhoneNumber: '',
    notes: '',
  })
  manualOpen.value = true
  loadQueueAvailability()
}

async function createManualBooking() {
  saving.value = true
  try {
    const response = await api.post<ApiResponse<object>>('/Appointment/manual', {
      clinicId: Number(manualForm.clinicId),
      appointmentDate: manualForm.appointmentDate,
      patientName: manualForm.patientName,
      patientPhoneNumber: manualForm.patientPhoneNumber,
      notes: manualForm.notes || null,
    })
    notifications.show(response.data.message)
    manualOpen.value = false
    await loadAppointments()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    saving.value = false
  }
}

function applyFilters() {
  page.value = 1
  doctorPage.value = 1
  loadAppointments()
}

function changePage(newPage: number) {
  if (isAdmin) {
    page.value = newPage
    loadAppointments()
  } else {
    doctorPage.value = newPage
  }
}

watch(() => filters.doctorId, (doctorId) => {
  if (isAdmin) loadAdminClinics(doctorId)
})
watch(() => [manualForm.clinicId, manualForm.appointmentDate], loadQueueAvailability)

onMounted(async () => {
  try {
    await Promise.all([loadClinics(), loadDoctors()])
    await loadAppointments()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
})
</script>

<template>
  <div>
    <div class="page-heading">
      <div>
        <span class="section-kicker">متابعة المراجعين</span>
        <h2>{{ isAdmin ? 'كل الحجوزات' : 'الحجوزات اليومية' }}</h2>
        <p>{{ isAdmin ? 'اعرض حجوزات النظام حسب الطبيب والعيادة والتاريخ والحالة.' : 'اعرض حجوزات عياداتك حسب التاريخ والحالة وحدّث مسار كل حجز.' }}</p>
      </div>
      <div class="heading-actions">
        <button class="secondary-button" type="button" :disabled="loading" @click="loadAppointments"><RefreshCw :size="17" /> تحديث</button>
        <button v-if="!isAdmin" class="compact-primary" type="button" :disabled="!clinics.length" @click="openManualBooking"><Plus :size="17" /> حجز يدوي</button>
      </div>
    </div>

    <form class="filter-card appointment-filters" :class="{ 'admin-appointment-filters': isAdmin }" @submit.prevent="applyFilters">
      <select v-if="isAdmin" v-model="filters.doctorId">
        <option value="">كل الأطباء</option>
        <option v-for="doctor in doctors" :key="doctor.id" :value="doctor.id">{{ doctor.name }}</option>
      </select>
      <select v-model="filters.clinicId" :disabled="isAdmin && !filters.doctorId">
        <option value="">{{ isAdmin ? 'كل عيادات الطبيب' : 'كل العيادات' }}</option>
        <option v-for="clinic in clinics" :key="clinic.id" :value="clinic.id">{{ clinic.name }}</option>
      </select>
      <input v-model="filters.fromDate" type="date" aria-label="من تاريخ" />
      <input v-model="filters.toDate" type="date" aria-label="إلى تاريخ" />
      <select v-model="filters.status">
        <option v-for="status in statusOptions" :key="status.value" :value="status.value">{{ status.label }}</option>
      </select>
      <button class="compact-primary" type="submit"><Search :size="16" /> بحث</button>
    </form>

    <section class="table-card">
      <div class="table-toolbar">
        <CalendarCheck :size="19" />
        <strong>قائمة الحجوزات</strong>
        <span class="records-count">{{ totalItems }} حجز</span>
      </div>
      <div class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th>الدور</th>
              <th>المراجع</th>
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
            <tr v-if="loading"><td class="table-message" :colspan="isAdmin ? 8 : 7">جارِ تحميل الحجوزات...</td></tr>
            <tr v-else-if="!appointments.length"><td class="table-message" :colspan="isAdmin ? 8 : 7">لا توجد حجوزات مطابقة للفلاتر المحددة.</td></tr>
            <tr v-for="appointment in visibleAppointments" v-else :key="appointment.id">
              <td><strong>#{{ appointment.queueNumber }}</strong><small class="block-muted">{{ appointment.code }}</small></td>
              <td><strong>{{ appointment.patientName || 'مراجع' }}</strong><small class="block-muted">{{ appointment.patientPhoneNumber || '-' }}</small></td>
              <td v-if="isAdmin">{{ appointment.doctorName || '-' }}</td>
              <td>{{ appointment.clinicName }}</td>
              <td>{{ formatDate(appointment.appointmentDate) }}</td>
              <td><span class="status-badge" :class="appointment.isPhoneConfirmed ? 'status-success' : 'status-warning'">{{ appointment.isPhoneConfirmed ? 'مؤكد' : 'بانتظار OTP' }}</span></td>
              <td><span class="status-badge" :class="statusMeta(appointment.status).className">{{ statusMeta(appointment.status).label }}</span></td>
              <td v-if="isAdmin"><span class="status-badge" :class="paymentMeta(appointment.paymentStatus).className">{{ paymentMeta(appointment.paymentStatus).label }}</span></td>
              <td v-if="!isAdmin">
                <div class="row-actions">
                  <button v-if="appointment.status === 0" type="button" title="تأكيد الحجز" @click="toggleStatus(appointment)"><Check :size="16" /></button>
                  <button v-if="appointment.status === 1" class="danger-action" type="button" title="إلغاء الحجز" @click="cancelAppointment = appointment"><X :size="16" /></button>
                  <button v-if="appointment.status === 1" type="button" title="إكمال الحجز" @click="complete(appointment)"><CheckCheck :size="16" /></button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <AppPagination :page="isAdmin ? page : doctorPage" :total-pages="isAdmin ? totalPages : doctorTotalPages" @change="changePage" />
    </section>

    <AppModal v-if="manualOpen" title="إضافة حجز يدوي" @close="manualOpen = false">
      <form class="modal-form" @submit.prevent="createManualBooking">
        <p class="modal-copy">أدخل بيانات المراجع القادم عبر الهاتف أو الاستقبال. يضاف الحجز مؤكداً مباشرة بدون OTP.</p>
        <label><span>العيادة</span><select v-model="manualForm.clinicId" required><option disabled value="">اختر العيادة</option><option v-for="clinic in clinics" :key="clinic.id" :value="clinic.id">{{ clinic.name }}</option></select></label>
        <label><span>تاريخ الحجز</span><input v-model="manualForm.appointmentDate" type="date" :min="today()" required /></label>
        <div v-if="queueAvailability" class="queue-box" :class="{ unavailable: !canCreateManual }">
          <strong>{{ queueAvailability.isAvailable ? 'الأدوار المتاحة' : 'اليوم غير متاح للحجز' }}</strong>
          <span v-if="queueAvailability.isAvailable">{{ queueAvailability.remainingAppointments }} متبقي من {{ queueAvailability.maxAppointments }}</span>
          <span v-else>{{ queueAvailability.closureReason || 'لا يوجد دوام لهذا اليوم.' }}</span>
        </div>
        <div v-else-if="queueLoading" class="queue-box">جارِ فحص توفر الأدوار...</div>
        <label><span>اسم المراجع</span><input v-model="manualForm.patientName" required maxlength="200" /></label>
        <label><span>رقم الهاتف</span><input v-model="manualForm.patientPhoneNumber" required maxlength="30" placeholder="07XXXXXXXXX" /></label>
        <label><span>ملاحظات</span><textarea v-model="manualForm.notes" rows="3" maxlength="1000" /></label>
        <div class="modal-actions"><button class="secondary-button" type="button" @click="manualOpen = false">تراجع</button><button class="compact-primary" type="submit" :disabled="saving || queueLoading || !canCreateManual">{{ saving ? 'جارِ الحفظ...' : 'تثبيت الحجز' }}</button></div>
      </form>
    </AppModal>

    <AppModal v-if="cancelAppointment" title="تأكيد إلغاء الحجز" @close="cancelAppointment = undefined">
      <p class="modal-copy">سيتم إلغاء حجز <strong>{{ cancelAppointment.patientName || 'المراجع' }}</strong> ذي الدور <strong>#{{ cancelAppointment.queueNumber }}</strong>. هل تريد المتابعة؟</p>
      <div class="modal-actions"><button class="secondary-button" type="button" @click="cancelAppointment = undefined">تراجع</button><button class="danger-button" type="button" @click="confirmCancel">تأكيد الإلغاء</button></div>
    </AppModal>
  </div>
</template>

<style scoped>
.admin-appointment-filters { grid-template-columns: minmax(160px, 1fr) minmax(160px, 1fr) 150px 150px 140px auto; }
.queue-box { display: grid; gap: 4px; padding: 11px; color: #167163; border: 1px solid #c8eadf; border-radius: 9px; background: #f0faf6; font-size: 13px; }
.queue-box.unavailable { color: #a23d3d; border-color: #ffd6d6; background: #fff3f3; }
@media (max-width: 760px) { .admin-appointment-filters { grid-template-columns: 1fr; } }
</style>
