<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { provinces } from '../constants/provinces'
import api from '../services/api'
import { useNotifications } from '../composables/useNotifications'
import type { ApiResponse, ClinicAvailability, ClinicItem, DayItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import EmptyState from '../components/common/Emptystate.vue'

interface ScheduleDay extends DayItem {
  enabled: boolean
  startTime: string
  endTime: string
  maxAppointments: number
}

const { success: showSuccess, error: showError } = useNotifications()

const clinics       = ref<ClinicItem[]>([])
const days          = ref<DayItem[]>([])
const loading       = ref(false)
const saving        = ref(false)
const editorOpen    = ref(false)
const deleteDialog  = ref(false)
const scheduleOpen  = ref(false)
const scheduleClinic = ref<ClinicItem>()
const deleteClinic  = ref<ClinicItem>()
const scheduleDays  = ref<ScheduleDay[]>([])
const page          = ref(1)
const pageSize      = 6

const form = reactive({
  id: 0, name: '', iraqiProvince: '0', address: '', latitude: '', longitude: '',
  mapUrl: '', phoneNumber: '', consultationPrice: '', showConsultationPrice: false,
  isVisible: true, bookingWindowDays: '7',
})

const totalPages       = computed(() => Math.max(1, Math.ceil(clinics.value.length / pageSize)))
const paginatedClinics = computed(() => clinics.value.slice((page.value - 1) * pageSize, page.value * pageSize))

function normalizePage() {
  if (page.value > totalPages.value) page.value = totalPages.value
}

function nullableNumber(value: string) {
  return value === '' ? null : Number(value)
}

async function loadClinics() {
  loading.value = true
  try {
    const r = await api.get<ApiResponse<ClinicItem[]>>('/Clinic/my')
    clinics.value = r.data.data
    normalizePage()
  } catch (e: any) {
    if (e.response?.status === 404) { clinics.value = []; page.value = 1 }
    else showError(getErrorMessage(e))
  } finally { loading.value = false }
}

async function loadDays() {
  try {
    const r = await api.get<ApiResponse<DayItem[]>>('/Day')
    days.value = r.data.data
  } catch (e) { showError(getErrorMessage(e)) }
}

function openEditor(clinic?: ClinicItem) {
  Object.assign(form, clinic ? {
    id: clinic.id, name: clinic.name, iraqiProvince: String(clinic.iraqiProvince),
    address: clinic.address, latitude: clinic.latitude?.toString() ?? '',
    longitude: clinic.longitude?.toString() ?? '', mapUrl: clinic.mapUrl ?? '',
    phoneNumber: clinic.phoneNumber ?? '',
    consultationPrice: clinic.consultationPrice == null ? '' : String(clinic.consultationPrice),
    showConsultationPrice: clinic.showConsultationPrice,
    bookingWindowDays: String(clinic.bookingWindowDays ?? 7),
    isVisible: clinic.isVisible,
  } : {
    id: 0, name: '', iraqiProvince: '0', address: '', latitude: '', longitude: '',
    mapUrl: '', phoneNumber: '', consultationPrice: '', showConsultationPrice: false,
    isVisible: true, bookingWindowDays: '7',
  })
  editorOpen.value = true
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
      bookingWindowDays: Math.min(31, Math.max(1, Number(form.bookingWindowDays || 7))),
    }
    const r = form.id
      ? await api.put<ApiResponse<object>>('/Clinic/my', body)
      : await api.post<ApiResponse<object>>('/Clinic/my', body)
    showSuccess(r.data.message)
    editorOpen.value = false
    await loadClinics()
  } catch (e) { showError(getErrorMessage(e)) }
  finally { saving.value = false }
}

async function confirmDelete() {
  if (!deleteClinic.value) return
  try {
    const r = await api.delete<ApiResponse<object>>(`/Clinic/my/${deleteClinic.value.id}`)
    showSuccess(r.data.message)
    deleteDialog.value = false
    deleteClinic.value = undefined
    await loadClinics()
  } catch (e) { showError(getErrorMessage(e)) }
}

async function openSchedule(clinic: ClinicItem) {
  scheduleClinic.value = clinic
  let current: ClinicAvailability[] = []
  try {
    const r = await api.get<ApiResponse<ClinicAvailability[]>>(`/DoctorAvailability/${clinic.id}`)
    current = r.data.data ?? []
  } catch (e: any) {
    if (e.response?.status !== 404) showError(getErrorMessage(e))
  }
  scheduleDays.value = days.value.map((day) => {
    const saved = current.find((a) => a.dayId === day.id)
    return {
      ...day,
      enabled: saved?.isAvailable ?? false,
      startTime: saved?.startTime?.slice(0, 5) ?? '09:00',
      endTime: saved?.endTime?.slice(0, 5) ?? '14:00',
      maxAppointments: saved?.maxAppointments ?? 20,
    }
  })
  scheduleOpen.value = true
}

async function saveSchedule() {
  if (!scheduleClinic.value) return
  saving.value = true
  try {
    const r = await api.post<ApiResponse<object>>('/DoctorAvailability', {
      clinicId: scheduleClinic.value.id,
      days: scheduleDays.value.filter((d) => d.enabled).map((d) => ({
        dayId: d.id,
        startTime: `${d.startTime}:00`,
        endTime: `${d.endTime}:00`,
        maxAppointments: d.maxAppointments,
      })),
    })
    showSuccess(r.data.message)
    scheduleOpen.value = false
    scheduleClinic.value = undefined
  } catch (e) { showError(getErrorMessage(e)) }
  finally { saving.value = false }
}

async function saveSingleDay(day: ScheduleDay) {
  if (!scheduleClinic.value) return
  saving.value = true
  try {
    const r = await api.put<ApiResponse<object>>('/DoctorAvailability/single-day', {
      clinicId: scheduleClinic.value.id,
      dayId: day.id,
      startTime: `${day.startTime}:00`,
      endTime: `${day.endTime}:00`,
      maxAppointments: day.maxAppointments,
      isAvailable: day.enabled,
    })
    showSuccess(r.data.message)
  } catch (e) { showError(getErrorMessage(e)) }
  finally { saving.value = false }
}

onMounted(() => Promise.all([loadClinics(), loadDays()]))
</script>

<template>
  <div class="clinics-page">

    <!-- Header -->
    <div class="page-top">
      <div>
        <p class="page-kicker">إدارة الفروع</p>
        <h1 class="page-title">عياداتي</h1>
      </div>
      <div class="page-actions">
        <v-btn variant="outlined" color="primary" prepend-icon="mdi-refresh" :loading="loading" @click="loadClinics">
          تحديث
        </v-btn>
        <v-btn color="primary" prepend-icon="mdi-plus" @click="openEditor()">
          إضافة عيادة
        </v-btn>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="clinics-grid">
      <v-skeleton-loader v-for="i in 3" :key="i" type="card" height="260" />
    </div>

    <!-- Empty -->
    <EmptyState
      v-else-if="!clinics.length"
      icon="mdi-hospital-building"
      title="لم تضف عيادات بعد"
      description="أنشئ أول فرع ثم أضف جدول الدوام الأسبوعي الخاص به"
    >
      <template #action>
        <v-btn color="primary" prepend-icon="mdi-plus" @click="openEditor()">إضافة عيادة</v-btn>
      </template>
    </EmptyState>

    <!-- Clinics Grid -->
    <div v-else class="clinics-grid">
      <div v-for="clinic in paginatedClinics" :key="clinic.id" class="clinic-card">

        <!-- Card Header -->
        <div class="clinic-card-header">
          <div class="clinic-icon">
            <v-icon icon="mdi-hospital-building" color="primary" size="22" />
          </div>
          <div class="clinic-header-info">
            <h3 class="clinic-name">{{ clinic.name }}</h3>
            <v-chip size="x-small" :color="clinic.isVisible ? 'success' : 'default'" variant="tonal">
              {{ clinic.isVisible ? 'ظاهرة للمرضى' : 'مخفية' }}
            </v-chip>
          </div>
        </div>

        <v-divider />

        <!-- Card Details -->
        <div class="clinic-details">
          <div class="detail-row">
            <v-icon icon="mdi-map-marker" size="16" color="primary" />
            <span><strong>{{ clinic.iraqiProvinceName }}</strong>، {{ clinic.address }}</span>
          </div>
          <div v-if="clinic.phoneNumber" class="detail-row">
            <v-icon icon="mdi-phone" size="16" color="primary" />
            <span>{{ clinic.phoneNumber }}</span>
          </div>
          <div v-if="clinic.consultationPrice != null" class="detail-row">
            <v-icon icon="mdi-currency-usd" size="16" color="primary" />
            <span>
              {{ clinic.consultationPrice.toLocaleString('ar-IQ') }} د.ع
              <v-chip size="x-small" variant="tonal" class="mr-1">
                {{ clinic.showConsultationPrice ? 'ظاهر' : 'مخفي' }}
              </v-chip>
            </span>
          </div>
          <div class="detail-row">
            <v-icon icon="mdi-calendar-range" size="16" color="primary" />
            <span>الحجز متاح مسبقاً لـ <strong>{{ clinic.bookingWindowDays ?? 7 }}</strong> يوم</span>
          </div>
          <div v-if="clinic.latitude != null && clinic.longitude != null" class="detail-row">
            <v-icon icon="mdi-crosshairs-gps" size="16" color="primary" />
            <span class="coords">{{ clinic.latitude }}, {{ clinic.longitude }}</span>
          </div>
        </div>

        <v-divider />

        <!-- Card Actions -->
        <div class="clinic-card-actions">
          <v-btn
            v-if="clinic.mapUrl"
            variant="text"
            size="small"
            color="info"
            prepend-icon="mdi-map"
            :href="clinic.mapUrl"
            target="_blank"
            tag="a"
          >
            الخارطة
          </v-btn>
          <v-btn
            variant="text"
            size="small"
            color="primary"
            prepend-icon="mdi-calendar-clock"
            @click="openSchedule(clinic)"
          >
            الدوام
          </v-btn>
          <v-btn
            variant="text"
            size="small"
            color="warning"
            prepend-icon="mdi-pencil"
            @click="openEditor(clinic)"
          >
            تعديل
          </v-btn>
          <v-btn
            variant="text"
            size="small"
            color="error"
            prepend-icon="mdi-delete"
            @click="deleteClinic = clinic; deleteDialog = true"
          >
            حذف
          </v-btn>
        </div>
      </div>
    </div>

    <!-- Pagination -->
    <div v-if="totalPages > 1" class="pagination-bar">
      <v-pagination
        v-model="page"
        :length="totalPages"
        :total-visible="5"
        density="compact"
        color="primary"
      />
    </div>

    <!-- ── Editor Dialog ── -->
    <v-dialog v-model="editorOpen" max-width="700" scrollable>
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon :icon="form.id ? 'mdi-pencil' : 'mdi-plus'" color="primary" size="20" />
          {{ form.id ? 'تعديل بيانات العيادة' : 'إضافة عيادة جديدة' }}
        </v-card-title>
        <v-divider />

        <v-card-text class="dialog-body">
          <div class="form-grid">
            <!-- Name -->
            <div class="form-field form-field--full">
              <label class="form-label">اسم العيادة <span class="required">*</span></label>
              <input v-model="form.name" class="form-input" required maxlength="200" />
            </div>

            <!-- Province -->
            <div class="form-field">
              <label class="form-label">المحافظة <span class="required">*</span></label>
              <v-autocomplete
                v-model="form.iraqiProvince"
                :items="provinces.map(p => ({ value: p.value, label: p.name }))"
                item-title="label"
                item-value="value"
                class="form-select"
                density="compact"
                variant="outlined"
                hide-details
              />
            </div>

            <!-- Phone -->
            <div class="form-field">
              <label class="form-label">رقم الهاتف</label>
              <input v-model="form.phoneNumber" class="form-input" />
            </div>

            <!-- Address -->
            <div class="form-field form-field--full">
              <label class="form-label">العنوان الكامل <span class="required">*</span></label>
              <input v-model="form.address" class="form-input" required maxlength="500" />
            </div>

            <!-- Map URL -->
            <div class="form-field form-field--full">
              <label class="form-label">رابط الخارطة</label>
              <input v-model="form.mapUrl" type="url" class="form-input" placeholder="https://maps.google.com/..." />
            </div>

            <!-- Booking Window -->
            <div class="form-field">
              <label class="form-label">أيام الحجز المتاحة مسبقاً</label>
              <input v-model="form.bookingWindowDays" type="number" min="1" max="31" class="form-input" required />
            </div>

            <!-- Consultation Price -->
            <div class="form-field">
              <label class="form-label">سعر المراجعة</label>
              <input v-model="form.consultationPrice" type="number" min="0" step="0.01" class="form-input" placeholder="اختياري" />
            </div>

            <!-- Latitude -->
            <div class="form-field">
              <label class="form-label">Latitude</label>
              <input v-model="form.latitude" type="number" min="-90" max="90" step="any" class="form-input" />
            </div>

            <!-- Longitude -->
            <div class="form-field">
              <label class="form-label">Longitude</label>
              <input v-model="form.longitude" type="number" min="-180" max="180" step="any" class="form-input" />
            </div>

            <!-- Checkboxes -->
            <div class="form-field form-field--full checkboxes-row">
              <label class="check-label">
                <input v-model="form.showConsultationPrice" type="checkbox" class="check-native" />
                <span class="check-box"><v-icon v-if="form.showConsultationPrice" icon="mdi-check" size="12" color="white" /></span>
                <span>إظهار سعر المراجعة للمرضى</span>
              </label>
              <label class="check-label">
                <input v-model="form.isVisible" type="checkbox" class="check-native" />
                <span class="check-box"><v-icon v-if="form.isVisible" icon="mdi-check" size="12" color="white" /></span>
                <span>إظهار العيادة للمرضى</span>
              </label>
            </div>
          </div>
        </v-card-text>

        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="editorOpen = false">تراجع</v-btn>
          <v-btn color="primary" :loading="saving" @click="saveClinic">
            {{ form.id ? 'حفظ التعديلات' : 'إضافة العيادة' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Schedule Dialog ── -->
    <v-dialog v-model="scheduleOpen" max-width="720" scrollable>
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-calendar-clock" color="primary" size="20" />
          دوام {{ scheduleClinic?.name }}
        </v-card-title>
        <v-divider />

        <v-card-text class="dialog-body">
          <v-alert type="info" variant="tonal" class="mb-4" icon="mdi-information">
            فعّل أيام الاستقبال وحدد وقت العرض والحد الأعلى للأدوار اليومية.
          </v-alert>
          <v-alert type="warning" variant="tonal" class="mb-4" icon="mdi-alert">
            عند تعطيل يوم دوام لديه حجوزات مستقبلية، سينقل النظام الحجوزات ويوزعها على الأيام المتاحة.
          </v-alert>

          <div class="schedule-list">
            <div
              v-for="day in scheduleDays"
              :key="day.id"
              class="schedule-row"
              :class="{ 'schedule-row--disabled': !day.enabled }"
            >
              <!-- Day Toggle -->
              <label class="day-toggle">
                <input v-model="day.enabled" type="checkbox" class="check-native" />
                <span class="check-box"><v-icon v-if="day.enabled" icon="mdi-check" size="12" color="white" /></span>
                <strong class="day-name">{{ day.name }}</strong>
              </label>

              <!-- Time Fields -->
              <div class="schedule-fields">
                <div class="schedule-field">
                  <label class="schedule-label">من</label>
                  <input v-model="day.startTime" type="time" class="time-input" :disabled="!day.enabled" />
                </div>
                <div class="schedule-field">
                  <label class="schedule-label">إلى</label>
                  <input v-model="day.endTime" type="time" class="time-input" :disabled="!day.enabled" />
                </div>
                <div class="schedule-field">
                  <label class="schedule-label">الأدوار</label>
                  <input v-model.number="day.maxAppointments" type="number" min="1" class="number-input" :disabled="!day.enabled" />
                </div>
                <v-btn
                  size="small"
                  variant="outlined"
                  color="primary"
                  :loading="saving"
                  class="save-day-btn"
                  @click="saveSingleDay(day)"
                >
                  حفظ
                </v-btn>
              </div>
            </div>
          </div>
        </v-card-text>

        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="scheduleOpen = false; scheduleClinic = undefined">تراجع</v-btn>
          <v-btn color="primary" prepend-icon="mdi-content-save" :loading="saving" @click="saveSchedule">
            حفظ الدوام
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Delete Dialog ── -->
    <v-dialog v-model="deleteDialog" max-width="420">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-alert" color="error" size="20" />
          حذف العيادة
        </v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          سيتم حذف عيادة <strong>{{ deleteClinic?.name }}</strong>. لن تظهر للمرضى بعد ذلك.
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="deleteDialog = false; deleteClinic = undefined">تراجع</v-btn>
          <v-btn color="error" @click="confirmDelete">تأكيد الحذف</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </div>
</template>

<style scoped>
.clinics-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

/* Page Top */
.page-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-lg);
  flex-wrap: wrap;
}
.page-kicker {
  margin: 0 0 4px 0;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-muted);
}
.page-title {
  margin: 0;
  font-size: 28px;
  font-weight: 800;
  color: var(--color-text);
}
.page-actions { display: flex; gap: var(--spacing-md); }

/* Grid */
.clinics-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--spacing-lg);
}

/* Clinic Card */
.clinic-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  transition: box-shadow 0.2s ease;
}
.clinic-card:hover { box-shadow: var(--shadow-md); }

.clinic-card-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-lg);
}

.clinic-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: var(--color-primary-soft);
  flex-shrink: 0;
}

.clinic-header-info { flex: 1; min-width: 0; }

.clinic-name {
  margin: 0 0 4px 0;
  font-size: 15px;
  font-weight: 700;
  color: var(--color-text);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* Details */
.clinic-details {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  padding: var(--spacing-lg);
  flex: 1;
}

.detail-row {
  display: flex;
  align-items: flex-start;
  gap: var(--spacing-sm);
  font-size: 13px;
  color: var(--color-text);
  line-height: 1.5;
}

.detail-row .v-icon { flex-shrink: 0; margin-top: 2px; }

.coords { font-size: 11px; color: var(--color-text-muted); font-family: monospace; }

/* Card Actions */
.clinic-card-actions {
  display: flex;
  gap: 2px;
  padding: var(--spacing-md);
  flex-wrap: wrap;
}

/* Pagination */
.pagination-bar {
  display: flex;
  justify-content: center;
  padding: var(--spacing-md) 0;
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
.dialog-actions {
  padding: var(--spacing-lg) !important;
  gap: var(--spacing-md);
  justify-content: flex-end;
}

/* Form Grid */
.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-lg);
}
.form-field { display: flex; flex-direction: column; gap: 6px; }
.form-field--full { grid-column: 1 / -1; }

.form-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text);
}
.required { color: var(--color-error); }

.form-input,
.form-select {
  height: 42px;
  padding: 0 12px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-size: 14px;
  outline: none;
  width: 100%;
  transition: border-color 0.2s;
}
.form-input:focus, .form-select:focus { border-color: var(--color-primary); }

/* Checkboxes */
.checkboxes-row { display: flex; gap: var(--spacing-xl); flex-wrap: wrap; }

.check-label {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text);
  user-select: none;
}
.check-native { position: absolute; opacity: 0; width: 0; height: 0; }
.check-box {
  width: 18px; height: 18px;
  border: 2px solid var(--color-border);
  border-radius: 4px;
  background: white;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
  transition: all 0.15s ease;
}
.check-native:checked ~ .check-box {
  background: var(--color-primary);
  border-color: var(--color-primary);
}

/* Schedule */
.schedule-list { display: flex; flex-direction: column; gap: var(--spacing-md); }

.schedule-row {
  display: flex;
  align-items: center;
  gap: var(--spacing-lg);
  padding: var(--spacing-md) var(--spacing-lg);
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  transition: all 0.15s ease;
  flex-wrap: wrap;
}
.schedule-row--disabled { opacity: 0.5; background: var(--color-background); }

.day-toggle {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  cursor: pointer;
  min-width: 100px;
  user-select: none;
}
.day-name { font-size: 14px; font-weight: 700; color: var(--color-text); }

.schedule-fields {
  display: flex;
  align-items: flex-end;
  gap: var(--spacing-md);
  flex: 1;
  flex-wrap: wrap;
}

.schedule-field { display: flex; flex-direction: column; gap: 4px; }
.schedule-label { font-size: 11px; font-weight: 700; color: var(--color-text-muted); }

.time-input,
.number-input {
  height: 36px;
  padding: 0 10px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-size: 13px;
  outline: none;
  transition: border-color 0.2s;
}
.time-input { min-width: 110px; }
.number-input { width: 80px; }
.time-input:focus, .number-input:focus { border-color: var(--color-primary); }
.time-input:disabled, .number-input:disabled { background: var(--color-background); cursor: not-allowed; }

.save-day-btn { align-self: flex-end; }

/* Responsive */
@media (max-width: 1100px) { .clinics-grid { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 600px)  { .clinics-grid { grid-template-columns: 1fr; } .form-grid { grid-template-columns: 1fr; } }
</style>