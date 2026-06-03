<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { ArrowRight, CheckCircle2, MapPin, Phone, RefreshCw, Star, Stethoscope } from '@lucide/vue'
import api from '../services/api'
import type { ApiResponse, PublicDoctorProfile, QueueAvailabilityItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const route = useRoute()
const doctor = ref<PublicDoctorProfile>()
const availability = ref<QueueAvailabilityItem>()
const loading = ref(false)
const booking = ref(false)
const otpMode = ref(false)
const message = ref('')
const bookingResult = ref<{ code: string; queueNumber: number }>()
const form = reactive({ clinicId: '', appointmentDate: today(), guestName: '', guestPhoneNumber: '', notes: '', otpCode: '' })
const doctorId = computed(() => Number(route.params.doctorId))
const apiOrigin = new URL(api.defaults.baseURL ?? 'https://localhost:7136/api').origin
const canBook = computed(() => doctor.value?.canBookOnline && form.clinicId && form.appointmentDate && form.guestName && form.guestPhoneNumber && (!availability.value || (availability.value.isAvailable && availability.value.remainingAppointments > 0)))

function today() {
  return new Date().toLocaleDateString('en-CA')
}

function imageUrl(imageName?: string) {
  return imageName ? `${apiOrigin}/DoctorImage/${imageName}` : ''
}

function formatTime(value?: string) {
  return value?.slice(0, 5) ?? '-'
}

async function loadDoctor() {
  loading.value = true
  message.value = ''
  try {
    const response = await api.get<ApiResponse<PublicDoctorProfile>>(`/Doctor/public/${doctorId.value}`)
    doctor.value = response.data.data
    form.clinicId = doctor.value.clinics[0]?.id ? String(doctor.value.clinics[0].id) : ''
    await loadAvailability()
  } catch (error) {
    message.value = getErrorMessage(error)
  } finally {
    loading.value = false
  }
}

async function loadAvailability() {
  availability.value = undefined
  if (!form.clinicId || !form.appointmentDate) return
  try {
    const response = await api.get<ApiResponse<QueueAvailabilityItem[]>>(`/Appointment/queue-availability/${form.clinicId}`, {
      params: { fromDate: form.appointmentDate, days: 1 },
    })
    availability.value = response.data.data[0]
  } catch (error) {
    message.value = getErrorMessage(error)
  }
}

async function createBooking() {
  booking.value = true
  message.value = ''
  try {
    const response = await api.post<ApiResponse<any>>('/Appointment', {
      doctorId: doctorId.value,
      clinicId: Number(form.clinicId),
      appointmentDate: form.appointmentDate,
      guestName: form.guestName,
      guestPhoneNumber: form.guestPhoneNumber,
      notes: form.notes || null,
    })
    bookingResult.value = {
      code: response.data.data?.code ?? response.data.data?.Code ?? '',
      queueNumber: response.data.data?.queueNumber ?? response.data.data?.QueueNumber ?? 0,
    }
    otpMode.value = true
    message.value = response.data.message || 'تم إنشاء الحجز. أدخل رمز التحقق المرسل إلى الهاتف.'
  } catch (error) {
    message.value = getErrorMessage(error)
  } finally {
    booking.value = false
  }
}

async function confirmOtp() {
  if (!bookingResult.value) return
  booking.value = true
  message.value = ''
  try {
    const response = await api.post<ApiResponse<object>>('/Appointment/otp/confirm', {
      phoneNumber: form.guestPhoneNumber,
      bookingCode: bookingResult.value.code,
      otpCode: form.otpCode,
    })
    message.value = response.data.message || 'تم تأكيد الحجز بنجاح.'
    otpMode.value = false
  } catch (error) {
    message.value = getErrorMessage(error)
  } finally {
    booking.value = false
  }
}

async function resendOtp() {
  if (!bookingResult.value) return
  booking.value = true
  try {
    const response = await api.post<ApiResponse<object>>('/Appointment/otp/resend', {
      phoneNumber: form.guestPhoneNumber,
      bookingCode: bookingResult.value.code,
    })
    message.value = response.data.message
  } catch (error) {
    message.value = getErrorMessage(error)
  } finally {
    booking.value = false
  }
}

watch(() => [form.clinicId, form.appointmentDate], loadAvailability)
onMounted(loadDoctor)
</script>

<template>
  <main class="public-page">
    <RouterLink class="secondary-button back-link" to="/directory"><ArrowRight :size="17" /> العودة للدليل</RouterLink>

    <div v-if="loading" class="empty-panel">جارِ تحميل بيانات الطبيب...</div>
    <p v-else-if="message && !doctor" class="form-error">{{ message }}</p>

    <template v-else-if="doctor">
      <section class="doctor-public-hero">
        <div class="doctor-public-photo"><img v-if="imageUrl(doctor.imageName)" :src="imageUrl(doctor.imageName)" :alt="doctor.name" /><Stethoscope v-else :size="44" /></div>
        <div>
          <span class="section-kicker">{{ doctor.specializationName }}</span>
          <h1>{{ doctor.name }}</h1>
          <p>{{ doctor.description }}</p>
          <span v-if="doctor.averageRating" class="rating-line"><Star :size="16" /> {{ doctor.averageRating.toFixed(1) }} · {{ doctor.reviewCount }} تقييم</span>
        </div>
      </section>

      <section class="public-details-grid">
        <div class="public-panel">
          <h2>العيادات والدوام</h2>
          <article v-for="clinic in doctor.clinics" :key="clinic.id" class="public-clinic-row">
            <strong>{{ clinic.name }}</strong>
            <span><MapPin :size="15" /> {{ clinic.iraqiProvinceName }}، {{ clinic.address }}</span>
            <span v-if="clinic.phoneNumber"><Phone :size="15" /> {{ clinic.phoneNumber }}</span>
            <div class="availability-tags"><span v-for="day in clinic.availabilities" :key="day.dayId">{{ day.dayName }} {{ formatTime(day.startTime) }}-{{ formatTime(day.endTime) }}</span></div>
          </article>
        </div>

        <div class="public-panel booking-panel">
          <h2>حجز دور</h2>
          <p v-if="!doctor.canBookOnline" class="form-error">الحجز الإلكتروني غير مفعل لهذا الطبيب حالياً.</p>
          <form v-else-if="!otpMode" class="modal-form" @submit.prevent="createBooking">
            <label><span>العيادة</span><select v-model="form.clinicId" required><option v-for="clinic in doctor.clinics" :key="clinic.id" :value="clinic.id">{{ clinic.name }}</option></select></label>
            <label><span>تاريخ الحجز</span><input v-model="form.appointmentDate" type="date" :min="today()" required /></label>
            <div v-if="availability" class="queue-box" :class="{ unavailable: !availability.isAvailable || availability.remainingAppointments <= 0 }">
              <strong>{{ availability.isAvailable ? `${availability.remainingAppointments} دور متبقي` : 'اليوم غير متاح' }}</strong>
              <span>{{ availability.closureReason || `${formatTime(availability.startTime)} - ${formatTime(availability.endTime)}` }}</span>
            </div>
            <label><span>اسم المراجع</span><input v-model="form.guestName" required maxlength="200" /></label>
            <label><span>رقم الهاتف</span><input v-model="form.guestPhoneNumber" required maxlength="30" /></label>
            <label><span>ملاحظات</span><textarea v-model="form.notes" rows="3" maxlength="1000" /></label>
            <button class="compact-primary" type="submit" :disabled="booking || !canBook">{{ booking ? 'جارِ الحجز...' : 'تأكيد الحجز' }}</button>
          </form>

          <form v-else class="modal-form" @submit.prevent="confirmOtp">
            <div class="booking-code"><CheckCircle2 :size="20" /> كود الحجز: <strong>{{ bookingResult?.code }}</strong> · الدور #{{ bookingResult?.queueNumber }}</div>
            <label><span>رمز التحقق OTP</span><input v-model="form.otpCode" required inputmode="numeric" /></label>
            <button class="compact-primary" type="submit" :disabled="booking">{{ booking ? 'جارِ التأكيد...' : 'تأكيد الهاتف' }}</button>
            <button class="secondary-button" type="button" :disabled="booking" @click="resendOtp"><RefreshCw :size="16" /> إعادة إرسال الرمز</button>
          </form>

          <p v-if="message" class="booking-message">{{ message }}</p>
        </div>
      </section>
    </template>
  </main>
</template>

<style scoped>
.public-page { min-height: 100vh; padding: 24px; background: #f6f9f8; }.back-link { width: fit-content; margin: 0 auto 18px; text-decoration: none; }
.doctor-public-hero { max-width: 1120px; margin: 0 auto 16px; display: flex; align-items: center; gap: 18px; padding: 22px; color: #fff; border-radius: 16px; background: linear-gradient(125deg, var(--primary-dark), #299789); box-shadow: var(--shadow); }
.doctor-public-photo { display: grid; place-items: center; width: 104px; height: 104px; overflow: hidden; color: var(--primary); border: 4px solid rgba(255,255,255,.82); border-radius: 28px; background: var(--primary-soft); }.doctor-public-photo img { width: 100%; height: 100%; object-fit: cover; }
.doctor-public-hero h1 { margin: 5px 0; font-size: 30px; }.doctor-public-hero p { margin: 0; color: #d9f1ed; line-height: 1.8; }.doctor-public-hero .section-kicker, .rating-line { color: #d7fffa; }
.public-details-grid { max-width: 1120px; margin: 0 auto; display: grid; grid-template-columns: 1.1fr .9fr; gap: 14px; }.public-panel { padding: 18px; border: 1px solid var(--line); border-radius: 14px; background: #fff; box-shadow: var(--shadow); }.public-panel h2 { margin: 0 0 14px; font-size: 21px; }
.public-clinic-row { display: grid; gap: 8px; padding: 13px; border: 1px solid var(--line); border-radius: 10px; background: #fbfdfc; margin-top: 10px; }.public-clinic-row span { display: flex; align-items: center; gap: 5px; color: var(--muted); font-size: 13px; }
.availability-tags { display: flex; flex-wrap: wrap; gap: 6px; }.availability-tags span { padding: 5px 7px; color: var(--primary); border-radius: 12px; background: var(--primary-soft); font-size: 11px; }
.queue-box { display: grid; gap: 4px; padding: 11px; color: #167163; border: 1px solid #c8eadf; border-radius: 9px; background: #f0faf6; font-size: 13px; }.queue-box.unavailable { color: #a23d3d; border-color: #ffd6d6; background: #fff3f3; }
.booking-code { display: flex; flex-wrap: wrap; align-items: center; gap: 6px; padding: 11px; color: #167163; border-radius: 9px; background: #e1f4ef; }.booking-message { color: var(--muted); line-height: 1.8; margin: 13px 0 0; }
@media (max-width: 820px) { .public-details-grid { grid-template-columns: 1fr; }.doctor-public-hero { align-items: flex-start; }.doctor-public-photo { width: 82px; height: 82px; } }
</style>
