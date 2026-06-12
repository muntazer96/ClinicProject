<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { BadgeDollarSign, Building2, CalendarDays, ExternalLink, MapPin, Pencil, Phone, Plus, RefreshCw, Trash2 } from '@lucide/vue'
import AppModal from '../components/AppModal.vue'
import AppPagination from '../components/AppPagination.vue'
import LongPressButton from '../components/LongPressButton.vue'
import { provinces } from '../constants/provinces'
import api from '../services/api'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, ClinicAvailability, ClinicItem, DayItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'

interface ScheduleDay extends DayItem {
  enabled: boolean
  startTime: string
  endTime: string
  maxAppointments: number
}

const notifications = useNotificationsStore()
const clinics = ref<ClinicItem[]>([])
const page = ref(1)
const pageSize = 6
const days = ref<DayItem[]>([])
const loading = ref(false)
const saving = ref(false)
const editorOpen = ref(false)
const scheduleClinic = ref<ClinicItem>()
const scheduleDays = ref<ScheduleDay[]>([])
const deleteClinic = ref<ClinicItem>()
const form = reactive({
  id: 0, name: '', iraqiProvince: '0', address: '', latitude: '', longitude: '',
  mapUrl: '', phoneNumber: '', consultationPrice: '', showConsultationPrice: false, isVisible: true,
  bookingWindowDays: '7',
})
const totalPages = computed(() => Math.max(1, Math.ceil(clinics.value.length / pageSize)))
const paginatedClinics = computed(() => clinics.value.slice((page.value - 1) * pageSize, page.value * pageSize))

function normalizePage() {
  if (page.value > totalPages.value) page.value = totalPages.value
}

function changePage(newPage: number) {
  page.value = newPage
}

async function loadClinics() {
  loading.value = true
  try {
    const response = await api.get<ApiResponse<ClinicItem[]>>('/Clinic/my')
    clinics.value = response.data.data
    normalizePage()
  } catch (error: any) {
    if (error.response?.status === 404) {
      clinics.value = []
      page.value = 1
    }
    else notifications.show(getErrorMessage(error), 'error')
  } finally {
    loading.value = false
  }
}

async function loadDays() {
  try {
    const response = await api.get<ApiResponse<DayItem[]>>('/Day')
    days.value = response.data.data
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

function openEditor(clinic?: ClinicItem) {
  Object.assign(form, clinic ? {
    id: clinic.id, name: clinic.name, iraqiProvince: String(clinic.iraqiProvince), address: clinic.address,
    latitude: clinic.latitude?.toString() ?? '', longitude: clinic.longitude?.toString() ?? '',
    mapUrl: clinic.mapUrl ?? '', phoneNumber: clinic.phoneNumber ?? '',
    consultationPrice: clinic.consultationPrice == null ? '' : String(clinic.consultationPrice),
    showConsultationPrice: clinic.showConsultationPrice,
    bookingWindowDays: String(clinic.bookingWindowDays ?? 7),
    isVisible: clinic.isVisible,
  } : {
    id: 0, name: '', iraqiProvince: '0', address: '', latitude: '', longitude: '',
    mapUrl: '', phoneNumber: '', consultationPrice: '', showConsultationPrice: false, isVisible: true,
    bookingWindowDays: '7',
  })
  editorOpen.value = true
}

function nullableNumber(value: string) {
  return value === '' ? null : Number(value)
}

async function saveClinic() {
  saving.value = true
  try {
    const body = {
      ...form,
      iraqiProvince: Number(form.iraqiProvince),
      latitude: nullableNumber(form.latitude),
      longitude: nullableNumber(form.longitude),
      mapUrl: form.mapUrl || null,
      phoneNumber: form.phoneNumber || null,
      consultationPrice: nullableNumber(form.consultationPrice),
      showConsultationPrice: form.showConsultationPrice,
      bookingWindowDays: Math.min(31, Math.max(1, Number(form.bookingWindowDays || 7))),
    }
    const response = form.id
      ? await api.put<ApiResponse<object>>('/Clinic/my', body)
      : await api.post<ApiResponse<object>>('/Clinic/my', body)
    notifications.show(response.data.message)
    editorOpen.value = false
    await loadClinics()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    saving.value = false
  }
}

async function confirmDelete() {
  if (!deleteClinic.value) return
  try {
    const response = await api.delete<ApiResponse<object>>(`/Clinic/my/${deleteClinic.value.id}`)
    notifications.show(response.data.message)
    deleteClinic.value = undefined
    await loadClinics()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

async function openSchedule(clinic: ClinicItem) {
  scheduleClinic.value = clinic
  let current: ClinicAvailability[] = []
  try {
    const response = await api.get<ApiResponse<ClinicAvailability[]>>(`/DoctorAvailability/${clinic.id}`)
    current = response.data.data ?? []
  } catch (error: any) {
    if (error.response?.status !== 404) notifications.show(getErrorMessage(error), 'error')
  }
  scheduleDays.value = days.value.map((day) => {
    const saved = current.find((availability) => availability.dayId === day.id)
    return {
      ...day,
      enabled: saved?.isAvailable ?? false,
      startTime: saved?.startTime?.slice(0, 5) ?? '09:00',
      endTime: saved?.endTime?.slice(0, 5) ?? '14:00',
      maxAppointments: saved?.maxAppointments ?? 20,
    }
  })
}

async function saveSchedule() {
  if (!scheduleClinic.value) return
  saving.value = true
  try {
    const response = await api.post<ApiResponse<object>>('/DoctorAvailability', {
      clinicId: scheduleClinic.value.id,
      days: scheduleDays.value.filter((day) => day.enabled).map((day) => ({
        dayId: day.id,
        startTime: `${day.startTime}:00`,
        endTime: `${day.endTime}:00`,
        maxAppointments: day.maxAppointments,
      })),
    })
    notifications.show(response.data.message)
    scheduleClinic.value = undefined
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    saving.value = false
  }
}

async function saveSingleDay(day: ScheduleDay) {
  if (!scheduleClinic.value) return
  saving.value = true
  try {
    const response = await api.put<ApiResponse<object>>('/DoctorAvailability/single-day', {
      clinicId: scheduleClinic.value.id,
      dayId: day.id,
      startTime: `${day.startTime}:00`,
      endTime: `${day.endTime}:00`,
      maxAppointments: day.maxAppointments,
      isAvailable: day.enabled,
    })
    notifications.show(response.data.message)
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    saving.value = false
  }
}

onMounted(() => Promise.all([loadClinics(), loadDays()]))
</script>

<template>
  <div>
    <div class="page-heading">
      <div><span class="section-kicker">إدارة الفروع</span><h2>عياداتي</h2><p>أدر بيانات فروعك وحدد جدول الدوام والأدوار اليومية لكل عيادة.</p></div>
      <div class="heading-actions"><button class="secondary-button" type="button" :disabled="loading" @click="loadClinics"><RefreshCw :size="17" /> تحديث</button><button class="compact-primary" type="button" @click="openEditor()"><Plus :size="17" /> إضافة عيادة</button></div>
    </div>

    <div v-if="loading" class="empty-panel">جارِ تحميل العيادات...</div>
    <div v-else-if="!clinics.length" class="empty-panel"><Building2 :size="31" /><h3>لم تضف عيادات بعد</h3><p>أنشئ أول فرع ثم أضف جدول الدوام الأسبوعي الخاص به.</p><button class="compact-primary" type="button" @click="openEditor()"><Plus :size="17" /> إضافة عيادة</button></div>
    <section v-else class="clinic-grid">
      <article v-for="clinic in paginatedClinics" :key="clinic.id" class="clinic-card">
        <div class="clinic-card-header"><span class="clinic-icon"><Building2 :size="21" /></span><div><h3>{{ clinic.name }}</h3><span class="status-badge" :class="clinic.isVisible ? 'status-success' : 'status-neutral'">{{ clinic.isVisible ? 'ظاهرة للمرضى' : 'مخفية' }}</span></div></div>
        <div class="clinic-details">
          <span><MapPin :size="16" /><b>{{ clinic.iraqiProvinceName }}</b>، {{ clinic.address }}</span>
          <span v-if="clinic.phoneNumber"><Phone :size="16" />{{ clinic.phoneNumber }}</span>
          <span v-if="clinic.consultationPrice != null">
            <BadgeDollarSign :size="16" />
            سعر المراجعة {{ clinic.consultationPrice.toLocaleString('ar-IQ') }} د.ع
            <small>{{ clinic.showConsultationPrice ? 'ظاهر للمرضى' : 'مخفي للمرضى' }}</small>
          </span>
          <span><CalendarDays :size="16" /> الحجز متاح مسبقاً لمدة <b>{{ clinic.bookingWindowDays ?? 7 }}</b> يوم</span>
          <span v-if="clinic.latitude != null && clinic.longitude != null"><MapPin :size="16" />{{ clinic.latitude }}, {{ clinic.longitude }}</span>
        </div>
        <div class="clinic-card-actions">
          <a v-if="clinic.mapUrl" class="clinic-link" :href="clinic.mapUrl" target="_blank" rel="noopener"><ExternalLink :size="15" /> الخارطة</a>
          <button type="button" @click="openSchedule(clinic)"><CalendarDays :size="16" /> الدوام</button>
          <button type="button" @click="openEditor(clinic)"><Pencil :size="16" /> تعديل</button>
          <LongPressButton button-class="danger-text" title="اضغط مطولاً لحذف العيادة" @confirm="deleteClinic = clinic"><Trash2 :size="16" /> حذف</LongPressButton>
        </div>
      </article>
    </section>
    <AppPagination :page="page" :total-pages="totalPages" @change="changePage" />

    <AppModal v-if="editorOpen" :title="form.id ? 'تعديل بيانات العيادة' : 'إضافة عيادة جديدة'" wide @close="editorOpen = false">
      <form class="modal-form form-grid" @submit.prevent="saveClinic">
        <label><span>اسم العيادة</span><input v-model="form.name" required maxlength="200" /></label>
        <label><span>المحافظة</span><select v-model="form.iraqiProvince" required><option v-for="province in provinces" :key="province.value" :value="province.value">{{ province.name }}</option></select></label>
        <label class="full-field"><span>العنوان الكامل</span><input v-model="form.address" required maxlength="500" /></label>
        <label><span>رقم الهاتف</span><input v-model="form.phoneNumber" /></label>
        <label><span>رابط الخارطة</span><input v-model="form.mapUrl" type="url" placeholder="https://maps.google.com/..." /></label>
        <label><span>عدد أيام الحجز المتاحة مسبقاً</span><input v-model="form.bookingWindowDays" type="number" min="1" max="31" required /></label>
        <label><span>سعر المراجعة</span><input v-model="form.consultationPrice" type="number" min="0" step="0.01" placeholder="اختياري" /></label>
        <label class="checkbox-field"><input v-model="form.showConsultationPrice" type="checkbox" /><span>إظهار سعر المراجعة للمرضى</span></label>
        <label><span>Latitude</span><input v-model="form.latitude" type="number" min="-90" max="90" step="any" /></label>
        <label><span>Longitude</span><input v-model="form.longitude" type="number" min="-180" max="180" step="any" /></label>
        <label class="checkbox-field full-field"><input v-model="form.isVisible" type="checkbox" /><span>إظهار العيادة للمرضى</span></label>
        <div class="modal-actions full-field"><button class="secondary-button" type="button" @click="editorOpen = false">تراجع</button><button class="compact-primary" type="submit" :disabled="saving">{{ saving ? 'جارِ الحفظ...' : 'حفظ العيادة' }}</button></div>
      </form>
    </AppModal>

    <AppModal v-if="scheduleClinic" :title="`دوام ${scheduleClinic.name}`" wide @close="scheduleClinic = undefined">
      <p class="modal-copy">فعّل أيام الاستقبال وحدد وقت العرض والحد الأعلى للأدوار اليومية. تطبّق حدود باقتك عند الحفظ.</p>
      <p class="schedule-warning">عند تعطيل يوم دوام لديه حجوزات مستقبلية، سينقل النظام الحجوزات إلى أول يوم دوام متاح ويرسل إشعاراً للمرضى.</p>
      <div class="schedule-list">
        <div v-for="day in scheduleDays" :key="day.id" class="schedule-row" :class="{ disabled: !day.enabled }">
          <label class="schedule-check"><input v-model="day.enabled" type="checkbox" /><strong>{{ day.name }}</strong></label>
          <label><span>من</span><input v-model="day.startTime" type="time" :disabled="!day.enabled" required /></label>
          <label><span>إلى</span><input v-model="day.endTime" type="time" :disabled="!day.enabled" required /></label>
          <label><span>الأدوار</span><input v-model.number="day.maxAppointments" type="number" min="1" :disabled="!day.enabled" required /></label>
          <button class="secondary-button schedule-save-day" type="button" :disabled="saving" @click="saveSingleDay(day)">حفظ اليوم</button>
        </div>
      </div>
      <div class="modal-actions"><button class="secondary-button" type="button" @click="scheduleClinic = undefined">تراجع</button><button class="compact-primary" type="button" :disabled="saving" @click="saveSchedule">{{ saving ? 'جارِ الحفظ...' : 'حفظ الدوام' }}</button></div>
    </AppModal>

    <AppModal v-if="deleteClinic" title="حذف العيادة" @close="deleteClinic = undefined">
      <p class="modal-copy">سيتم حذف عيادة <strong>{{ deleteClinic.name }}</strong>. لن تظهر للمرضى بعد ذلك.</p>
      <div class="modal-actions"><button class="secondary-button" type="button" @click="deleteClinic = undefined">تراجع</button><LongPressButton button-class="danger-button" title="اضغط مطولاً لتأكيد الحذف" @confirm="confirmDelete">تأكيد الحذف</LongPressButton></div>
    </AppModal>
  </div>
</template>

<style scoped>
.schedule-save-day { align-self: end; min-height: 36px; padding: 7px 9px; font-size: 12px; }
.schedule-warning { margin: 12px 0 0; padding: 10px 12px; color: #8a5b12; border: 1px solid #f2d49c; border-radius: 8px; background: #fff8e8; font-size: 13px; font-weight: 700; }
</style>
