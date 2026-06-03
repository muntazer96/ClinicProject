<script setup lang="ts">
import { reactive, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowRight, Ban, CalendarDays, MapPin, Search } from '@lucide/vue'
import api from '../services/api'
import type { ApiResponse, BookingDetails } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const loading = ref(false)
const cancelling = ref(false)
const booking = ref<BookingDetails>()
const message = ref('')
const form = reactive({ phoneNumber: '', code: '', reason: '' })

function statusLabel(status: number) {
  return ['قيد الانتظار', 'مؤكد', 'ملغي', 'مكتمل'][status] ?? 'غير معروف'
}

function formatDate(value?: string) {
  return value ? new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium' }).format(new Date(value)) : '-'
}

async function findBooking() {
  loading.value = true
  message.value = ''
  booking.value = undefined
  try {
    const response = await api.get<ApiResponse<BookingDetails>>('/Appointment/guest', {
      params: { phoneNumber: form.phoneNumber, code: form.code },
    })
    booking.value = response.data.data
  } catch (error) {
    message.value = getErrorMessage(error)
  } finally {
    loading.value = false
  }
}

async function cancelBooking() {
  cancelling.value = true
  message.value = ''
  try {
    const response = await api.post<ApiResponse<object>>('/Appointment/guest/cancel', {
      phoneNumber: form.phoneNumber,
      code: form.code,
      reason: form.reason || null,
    })
    message.value = response.data.message
    await findBooking()
  } catch (error) {
    message.value = getErrorMessage(error)
  } finally {
    cancelling.value = false
  }
}
</script>

<template>
  <main class="guest-page">
    <RouterLink class="secondary-button back-link" to="/directory"><ArrowRight :size="17" /> العودة للدليل</RouterLink>
    <section class="guest-card">
      <span class="guest-icon"><CalendarDays :size="32" /></span>
      <h1>متابعة حجز زائر</h1>
      <form class="modal-form" @submit.prevent="findBooking">
        <label><span>رقم الهاتف</span><input v-model="form.phoneNumber" required maxlength="30" /></label>
        <label><span>كود الحجز</span><input v-model="form.code" required /></label>
        <button class="compact-primary" type="submit" :disabled="loading"><Search :size="16" /> {{ loading ? 'جارِ البحث...' : 'بحث' }}</button>
      </form>
      <p v-if="message" class="booking-message">{{ message }}</p>
    </section>

    <section v-if="booking" class="guest-card booking-result">
      <h2>{{ booking.doctorName }}</h2>
      <p><MapPin :size="16" /> {{ booking.clinicName }}، {{ booking.clinicAddress }}</p>
      <div class="guest-booking-grid">
        <span>التاريخ <strong>{{ formatDate(booking.appointmentDate) }}</strong></span>
        <span>الدور <strong>#{{ booking.queueNumber }}</strong></span>
        <span>الحالة <strong>{{ statusLabel(booking.status) }}</strong></span>
        <span>الهاتف <strong>{{ booking.isPhoneConfirmed ? 'مؤكد' : 'بانتظار التأكيد' }}</strong></span>
      </div>
      <form v-if="booking.status !== 2 && booking.status !== 3" class="modal-form cancel-form" @submit.prevent="cancelBooking">
        <label><span>سبب الإلغاء</span><textarea v-model="form.reason" rows="3" maxlength="500" /></label>
        <button class="danger-button" type="submit" :disabled="cancelling"><Ban :size="16" /> {{ cancelling ? 'جارِ الإلغاء...' : 'إلغاء الحجز' }}</button>
      </form>
    </section>
  </main>
</template>

<style scoped>
.guest-page { min-height: 100vh; display: grid; align-content: start; gap: 14px; padding: 24px; background: #f6f9f8; }.back-link { width: fit-content; margin: 0 auto; text-decoration: none; }
.guest-card { width: min(100%, 560px); margin: 0 auto; padding: 22px; border: 1px solid var(--line); border-radius: 15px; background: #fff; box-shadow: var(--shadow); }
.guest-icon { display: grid; place-items: center; width: 66px; height: 66px; margin: 0 auto 12px; color: var(--primary); border-radius: 20px; background: var(--primary-soft); }.guest-card h1 { margin: 0 0 16px; text-align: center; font-size: 26px; }
.booking-message { color: var(--muted); line-height: 1.8; margin: 13px 0 0; }.booking-result h2 { margin: 0 0 8px; }.booking-result p { display: flex; gap: 6px; color: var(--muted); margin: 0 0 14px; }
.guest-booking-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 9px; }.guest-booking-grid span { padding: 10px; color: var(--muted); border: 1px solid var(--line); border-radius: 9px; background: #fbfdfc; }.guest-booking-grid strong { display: block; margin-top: 4px; color: var(--ink); }
.cancel-form { margin-top: 14px; }
@media (max-width: 520px) { .guest-booking-grid { grid-template-columns: 1fr; } }
</style>
