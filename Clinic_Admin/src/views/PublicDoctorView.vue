<script setup lang="ts">
import { computed, onMounted, onUnmounted, reactive, ref, watch } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { ArrowRight, CalendarDays, CheckCircle2, Copy, MapPin, Phone, RefreshCw, Star, Stethoscope } from '@lucide/vue'
import AppModal from '../components/AppModal.vue'
import LongPressButton from '../components/LongPressButton.vue'
import api from '../services/api'
import type { ApiResponse, PublicDoctorProfile, QueueAvailabilityItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import { specializationIcon } from '../utils/specializationIcons'

const route = useRoute()
const doctor = ref<PublicDoctorProfile>()
const availability = ref<QueueAvailabilityItem>()
const loading = ref(false)
const booking = ref(false)
const otpMode = ref(false)
const message = ref('')
const bookingConfirmOpen = ref(false)
const bookingResult = ref<{ code: string; queueNumber: number }>()
const resendSeconds = ref(0)
let resendTimer: number | undefined
const form = reactive({ clinicId: '', appointmentDate: today(), guestName: '', guestPhoneNumber: '', notes: '', otpCode: '' })
const doctorId = computed(() => Number(route.params.doctorId))
const apiOrigin = new URL(api.defaults.baseURL ?? 'https://localhost:7136/api').origin
const canBook = computed(() => doctor.value?.canBookOnline && form.clinicId && form.appointmentDate && form.guestName && form.guestPhoneNumber && (!availability.value || (availability.value.isAvailable && availability.value.remainingAppointments > 0)))
const selectedClinic = computed(() => doctor.value?.clinics.find((clinic) => clinic.id === Number(form.clinicId)))
const contactClinic = computed(() => doctor.value?.clinics.find((clinic) => clinic.phoneNumber))
const mapClinic = computed(() => doctor.value?.clinics.find((clinic) => clinic.mapUrl))
const firstBookableClinic = computed(() => doctor.value?.clinics.find((clinic) => clinic.availabilities.length > 0) ?? doctor.value?.clinics[0])
const canConfirmOtp = computed(() => Boolean(form.otpCode.trim() && !booking.value))
const maxBookingDate = computed(() => {
  const days = Math.max(1, selectedClinic.value?.bookingWindowDays ?? 7)
  const date = new Date()
  date.setDate(date.getDate() + days - 1)
  return date.toLocaleDateString('en-CA')
})

function today() {
  return new Date().toLocaleDateString('en-CA')
}

function imageUrl(imageName?: string) {
  return imageName ? `${apiOrigin}/DoctorImage/${imageName}` : ''
}

function formatTime(value?: string) {
  return value?.slice(0, 5) ?? '-'
}

function jumpToBooking() {
  if (firstBookableClinic.value) form.clinicId = String(firstBookableClinic.value.id)
  document.getElementById('public-booking-panel')?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

function startResendCooldown(seconds = 60) {
  window.clearInterval(resendTimer)
  resendSeconds.value = seconds
  resendTimer = window.setInterval(() => {
    if (resendSeconds.value <= 1) {
      window.clearInterval(resendTimer)
      resendSeconds.value = 0
      return
    }
    resendSeconds.value -= 1
  }, 1000)
}

async function copyBookingCode() {
  if (!bookingResult.value?.code) return
  try {
    await navigator.clipboard.writeText(bookingResult.value.code)
    message.value = 'تم نسخ كود الحجز.'
  } catch {
    message.value = 'تعذر نسخ الكود، انسخه يدوياً.'
  }
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
  bookingConfirmOpen.value = false
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
    startResendCooldown()
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
  if (!bookingResult.value || resendSeconds.value > 0) return
  booking.value = true
  try {
    const response = await api.post<ApiResponse<object>>('/Appointment/otp/resend', {
      phoneNumber: form.guestPhoneNumber,
      bookingCode: bookingResult.value.code,
    })
    message.value = response.data.message
    startResendCooldown()
  } catch (error) {
    message.value = getErrorMessage(error)
  } finally {
    booking.value = false
  }
}

watch(() => [form.clinicId, form.appointmentDate], loadAvailability)
onMounted(loadDoctor)
onUnmounted(() => window.clearInterval(resendTimer))
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
          <span class="section-kicker specialty-kicker"><component :is="specializationIcon(doctor.specializationIconName)" :size="16" /> {{ doctor.specializationName }}</span>
          <h1>{{ doctor.name }}</h1>
          <p>{{ doctor.description }}</p>
          <div class="hero-metrics">
            <span v-if="doctor.averageRating" class="rating-line"><Star :size="16" /> {{ doctor.averageRating.toFixed(1) }} · {{ doctor.reviewCount }} تقييم</span>
            <span><Stethoscope :size="16" /> {{ doctor.clinics.length }} عيادة</span>
            <span :class="doctor.canBookOnline ? 'available' : 'paused'"><CalendarDays :size="16" /> {{ doctor.canBookOnline ? 'الحجز متاح' : 'الحجز غير مفعل' }}</span>
          </div>
          <div class="hero-actions">
            <button v-if="doctor.canBookOnline && firstBookableClinic" class="hero-action primary" type="button" @click="jumpToBooking"><CalendarDays :size="16" /> حجز سريع</button>
            <a v-if="contactClinic?.phoneNumber" class="hero-action" :href="`tel:${contactClinic.phoneNumber}`"><Phone :size="16" /> اتصال</a>
            <a v-if="mapClinic?.mapUrl" class="hero-action" :href="mapClinic.mapUrl" target="_blank" rel="noreferrer"><MapPin :size="16" /> الموقع</a>
          </div>
        </div>
      </section>

      <section class="public-details-grid">
        <div class="public-panel">
          <h2>العيادات والدوام</h2>
          <article v-for="clinic in doctor.clinics" :key="clinic.id" class="public-clinic-row">
            <strong>{{ clinic.name }}</strong>
            <span><MapPin :size="15" /> {{ clinic.iraqiProvinceName }}، {{ clinic.address }}</span>
            <span><CalendarDays :size="15" /> الحجز متاح مسبقاً لمدة {{ clinic.bookingWindowDays ?? 7 }} يوم</span>
            <div class="clinic-actions">
              <a v-if="clinic.phoneNumber" :href="`tel:${clinic.phoneNumber}`"><Phone :size="15" /> {{ clinic.phoneNumber }}</a>
              <a v-if="clinic.mapUrl" :href="clinic.mapUrl" target="_blank" rel="noreferrer"><MapPin :size="15" /> موقع العيادة</a>
            </div>
            <div v-if="clinic.availabilities.length" class="availability-tags"><span v-for="day in clinic.availabilities" :key="day.dayId">{{ day.dayName }} {{ formatTime(day.startTime) }}-{{ formatTime(day.endTime) }} · {{ day.maxAppointments }} دور</span></div>
            <small v-else class="empty-schedule">لم يتم تحديد دوام لهذه العيادة.</small>
          </article>
        </div>

        <div id="public-booking-panel" class="public-panel booking-panel">
          <h2>حجز دور</h2>
          <p v-if="!doctor.canBookOnline" class="form-error">الحجز الإلكتروني غير مفعل لهذا الطبيب حالياً.</p>
          <form v-else-if="!otpMode" class="modal-form" @submit.prevent="bookingConfirmOpen = true">
            <label><span>العيادة</span><select v-model="form.clinicId" required><option v-for="clinic in doctor.clinics" :key="clinic.id" :value="clinic.id">{{ clinic.name }}</option></select></label>
            <label><span>تاريخ الحجز</span><input v-model="form.appointmentDate" type="date" :min="today()" :max="maxBookingDate" required /></label>
            <div v-if="availability" class="queue-box" :class="{ unavailable: !availability.isAvailable || availability.remainingAppointments <= 0 }">
              <strong>{{ availability.dayName }} - {{ availability.isAvailable ? `${availability.remainingAppointments} دور متبقي` : 'اليوم غير متاح' }}</strong>
              <span>{{ availability.closureReason || `${formatTime(availability.startTime)} - ${formatTime(availability.endTime)}` }}</span>
            </div>
            <label><span>اسم المراجع</span><input v-model="form.guestName" required maxlength="200" /></label>
            <label><span>رقم الهاتف</span><input v-model="form.guestPhoneNumber" required maxlength="30" /></label>
            <label><span>ملاحظات</span><textarea v-model="form.notes" rows="3" maxlength="1000" /></label>
            <button class="compact-primary" type="submit" :disabled="booking || !canBook">{{ booking ? 'جارِ الحجز...' : 'تأكيد الحجز' }}</button>
          </form>

          <form v-else class="modal-form" @submit.prevent="confirmOtp">
            <div class="booking-code">
              <CheckCircle2 :size="20" />
              كود الحجز: <strong>{{ bookingResult?.code }}</strong> · الدور #{{ bookingResult?.queueNumber }}
              <button class="icon-copy-button" type="button" :disabled="!bookingResult?.code" @click="copyBookingCode" title="نسخ الكود"><Copy :size="15" /></button>
            </div>
            <label><span>رمز التحقق OTP</span><input v-model="form.otpCode" required inputmode="numeric" /></label>
            <button class="compact-primary" type="submit" :disabled="!canConfirmOtp">{{ booking ? 'جارِ التأكيد...' : 'تأكيد الهاتف' }}</button>
            <button class="secondary-button" type="button" :disabled="booking || resendSeconds > 0" @click="resendOtp">
              <RefreshCw :size="16" />
              {{ resendSeconds > 0 ? `إعادة الإرسال بعد ${resendSeconds} ثانية` : 'إعادة إرسال الرمز' }}
            </button>
          </form>

          <p v-if="message" class="booking-message">{{ message }}</p>
        </div>
      </section>
    </template>

    <AppModal v-if="bookingConfirmOpen && doctor" title="تأكيد الحجز" @close="bookingConfirmOpen = false">
      <div class="confirm-summary">
        <span>الطبيب <strong>{{ doctor.name }}</strong></span>
        <span>العيادة <strong>{{ selectedClinic?.name ?? '-' }}</strong></span>
        <span>التاريخ <strong>{{ availability?.dayName ?? '' }} {{ form.appointmentDate }}</strong></span>
        <span>المراجع <strong>{{ form.guestName }}</strong></span>
        <span>الهاتف <strong>{{ form.guestPhoneNumber }}</strong></span>
      </div>
      <div class="modal-actions">
        <button class="secondary-button" type="button" @click="bookingConfirmOpen = false">تراجع</button>
        <LongPressButton button-class="compact-primary" :disabled="booking || !canBook" title="اضغط مطولاً لتأكيد الحجز" @confirm="createBooking">تأكيد الحجز</LongPressButton>
      </div>
    </AppModal>
  </main>
</template>

<style scoped>
.public-page { min-height: 100vh; padding: 24px; background: #f6f9f8; }.back-link { width: fit-content; margin: 0 auto 18px; text-decoration: none; }
.doctor-public-hero { max-width: 1120px; margin: 0 auto 16px; display: flex; align-items: center; gap: 18px; padding: 22px; color: #fff; border-radius: 16px; background: linear-gradient(125deg, var(--primary-dark), #299789); box-shadow: var(--shadow); }
.doctor-public-photo { display: grid; place-items: center; width: 104px; height: 104px; overflow: hidden; color: var(--primary); border: 4px solid rgba(255,255,255,.82); border-radius: 28px; background: var(--primary-soft); }.doctor-public-photo img { width: 100%; height: 100%; object-fit: cover; }
.doctor-public-hero h1 { margin: 5px 0; font-size: 30px; }.doctor-public-hero p { margin: 0; color: #d9f1ed; line-height: 1.8; }.doctor-public-hero .section-kicker, .rating-line { color: #d7fffa; }.specialty-kicker { display: inline-flex; align-items: center; gap: 6px; }
.hero-metrics, .hero-actions { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 12px; }.hero-metrics span { display: inline-flex; align-items: center; gap: 5px; padding: 6px 9px; color: #d7fffa; border-radius: 999px; background: rgba(255,255,255,.13); font-size: 12px; font-weight: 800; }.hero-metrics .paused { color: #ffe2ad; }.hero-metrics .available { color: #d7fffa; }
.hero-action { display: inline-flex; align-items: center; gap: 6px; padding: 8px 11px; color: #fff; border: 1px solid rgba(255,255,255,.32); border-radius: 9px; background: rgba(255,255,255,.12); text-decoration: none; cursor: pointer; font-weight: 800; }.hero-action.primary { color: var(--primary-dark); border-color: #fff; background: #fff; }
.public-details-grid { max-width: 1120px; margin: 0 auto; display: grid; grid-template-columns: 1.1fr .9fr; gap: 14px; }.public-panel { padding: 18px; border: 1px solid var(--line); border-radius: 14px; background: #fff; box-shadow: var(--shadow); }.public-panel h2 { margin: 0 0 14px; font-size: 21px; }
.public-clinic-row { display: grid; gap: 8px; padding: 13px; border: 1px solid var(--line); border-radius: 10px; background: #fbfdfc; margin-top: 10px; }.public-clinic-row span { display: flex; align-items: center; gap: 5px; color: var(--muted); font-size: 13px; }
.clinic-actions { display: flex; flex-wrap: wrap; gap: 8px; }.clinic-actions a { display: inline-flex; align-items: center; gap: 5px; padding: 6px 8px; color: var(--primary); border: 1px solid #c8eadf; border-radius: 8px; background: #f0faf6; text-decoration: none; font-size: 12px; font-weight: 800; }.empty-schedule { color: var(--muted); }
.availability-tags { display: flex; flex-wrap: wrap; gap: 6px; }.availability-tags span { padding: 5px 7px; color: var(--primary); border-radius: 12px; background: var(--primary-soft); font-size: 11px; }
.queue-box { display: grid; gap: 4px; padding: 11px; color: #167163; border: 1px solid #c8eadf; border-radius: 9px; background: #f0faf6; font-size: 13px; }.queue-box.unavailable { color: #a23d3d; border-color: #ffd6d6; background: #fff3f3; }
.booking-code { display: flex; flex-wrap: wrap; align-items: center; gap: 6px; padding: 11px; color: #167163; border-radius: 9px; background: #e1f4ef; }.booking-message { color: var(--muted); line-height: 1.8; margin: 13px 0 0; }
.icon-copy-button { display: inline-grid; place-items: center; width: 28px; height: 28px; margin-inline-start: auto; color: #167163; border: 1px solid #bde3d9; border-radius: 8px; background: #fff; cursor: pointer; }.icon-copy-button:disabled { opacity: .55; cursor: not-allowed; }
.confirm-summary { display: grid; gap: 9px; margin-bottom: 14px; }
.confirm-summary span { display: flex; justify-content: space-between; gap: 10px; padding: 9px 0; color: var(--muted); border-bottom: 1px solid var(--line); }
.confirm-summary strong { color: var(--ink); text-align: left; }
@media (max-width: 820px) { .public-details-grid { grid-template-columns: 1fr; }.doctor-public-hero { align-items: flex-start; }.doctor-public-photo { width: 82px; height: 82px; } }
</style>
