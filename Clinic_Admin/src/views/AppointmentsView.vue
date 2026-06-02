<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { CalendarCheck, Check, CheckCheck, RefreshCw, Search, X } from '@lucide/vue'
import api from '../services/api'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, AppointmentItem, ClinicItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const notifications = useNotificationsStore()
const appointments = ref<AppointmentItem[]>([])
const clinics = ref<ClinicItem[]>([])
const loading = ref(false)
const filters = reactive({ clinicId: '', fromDate: today(), toDate: today(), status: '' })
const statusOptions = [
  { value: '', label: 'كل الحالات' }, { value: '0', label: 'قيد الانتظار' },
  { value: '1', label: 'مؤكد' }, { value: '2', label: 'ملغي' }, { value: '3', label: 'مكتمل' },
]

function today() { return new Date().toLocaleDateString('en-CA') }
function statusMeta(status: number) {
  return [
    { label: 'قيد الانتظار', className: 'status-warning' }, { label: 'مؤكد', className: 'status-success' },
    { label: 'ملغي', className: 'status-danger' }, { label: 'مكتمل', className: 'status-neutral' },
  ][status] ?? { label: 'غير معروف', className: 'status-neutral' }
}
function formatDate(value: string) { return new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium' }).format(new Date(value)) }

async function loadClinics() {
  const response = await api.get<ApiResponse<ClinicItem[]>>('/Clinic/my')
  clinics.value = response.data.data
}
async function loadAppointments() {
  loading.value = true
  try {
    const response = await api.get<ApiResponse<AppointmentItem[]>>('/Appointment/doctor/my', { params: {
      clinicId: filters.clinicId || undefined, fromDate: filters.fromDate || undefined,
      toDate: filters.toDate || undefined, status: filters.status === '' ? undefined : filters.status,
    } })
    appointments.value = response.data.data
  } catch (error) { notifications.show(getErrorMessage(error), 'error') }
  finally { loading.value = false }
}
async function toggleStatus(appointment: AppointmentItem) {
  try {
    const response = await api.post<ApiResponse<object>>('/Appointment/toggle-status', null, { params: { appointmentId: appointment.id } })
    notifications.show(response.data.message); await loadAppointments()
  } catch (error) { notifications.show(getErrorMessage(error), 'error') }
}
async function complete(appointment: AppointmentItem) {
  try {
    const response = await api.post<ApiResponse<object>>('/Appointment/complete', null, { params: { appointmentId: appointment.id } })
    notifications.show(response.data.message); await loadAppointments()
  } catch (error) { notifications.show(getErrorMessage(error), 'error') }
}
onMounted(async () => {
  try { await loadClinics(); await loadAppointments() }
  catch (error) { notifications.show(getErrorMessage(error), 'error') }
})
</script>

<template>
  <div>
    <div class="page-heading">
      <div><span class="section-kicker">متابعة المراجعين</span><h2>الحجوزات اليومية</h2><p>اعرض حجوزات عياداتك حسب التاريخ والحالة وحدّث مسار كل حجز.</p></div>
      <button class="secondary-button" type="button" :disabled="loading" @click="loadAppointments"><RefreshCw :size="17" /> تحديث</button>
    </div>
    <form class="filter-card appointment-filters" @submit.prevent="loadAppointments">
      <select v-model="filters.clinicId"><option value="">كل العيادات</option><option v-for="clinic in clinics" :key="clinic.id" :value="clinic.id">{{ clinic.name }}</option></select>
      <input v-model="filters.fromDate" type="date" aria-label="من تاريخ" /><input v-model="filters.toDate" type="date" aria-label="إلى تاريخ" />
      <select v-model="filters.status"><option v-for="status in statusOptions" :key="status.value" :value="status.value">{{ status.label }}</option></select>
      <button class="compact-primary" type="submit"><Search :size="16" /> بحث</button>
    </form>
    <section class="table-card">
      <div class="table-toolbar"><CalendarCheck :size="19" /><strong>قائمة الحجوزات</strong><span class="records-count">{{ appointments.length }} حجز</span></div>
      <div class="table-scroll"><table class="data-table">
        <thead><tr><th>الدور</th><th>المراجع</th><th>العيادة</th><th>التاريخ</th><th>التحقق</th><th>الحالة</th><th>الإجراءات</th></tr></thead>
        <tbody>
          <tr v-if="loading"><td class="table-message" colspan="7">جارِ تحميل الحجوزات...</td></tr>
          <tr v-else-if="!appointments.length"><td class="table-message" colspan="7">لا توجد حجوزات مطابقة للفلاتر المحددة.</td></tr>
          <tr v-for="appointment in appointments" v-else :key="appointment.id">
            <td><strong>#{{ appointment.queueNumber }}</strong><small class="block-muted">{{ appointment.code }}</small></td>
            <td><strong>{{ appointment.patientName || 'مراجع' }}</strong><small class="block-muted">{{ appointment.patientPhoneNumber || '-' }}</small></td>
            <td>{{ appointment.clinicName }}</td><td>{{ formatDate(appointment.appointmentDate) }}</td>
            <td><span class="status-badge" :class="appointment.isPhoneConfirmed ? 'status-success' : 'status-warning'">{{ appointment.isPhoneConfirmed ? 'مؤكد' : 'بانتظار OTP' }}</span></td>
            <td><span class="status-badge" :class="statusMeta(appointment.status).className">{{ statusMeta(appointment.status).label }}</span></td>
            <td><div class="row-actions">
              <button v-if="appointment.status === 0 || appointment.status === 2" type="button" title="تأكيد الحجز" @click="toggleStatus(appointment)"><Check :size="16" /></button>
              <button v-if="appointment.status === 1" class="danger-action" type="button" title="إلغاء الحجز" @click="toggleStatus(appointment)"><X :size="16" /></button>
              <button v-if="appointment.status === 1" type="button" title="إكمال الحجز" @click="complete(appointment)"><CheckCheck :size="16" /></button>
            </div></td>
          </tr>
        </tbody>
      </table></div>
    </section>
  </div>
</template>
