<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowRight, Ban, CalendarDays, Copy, MapPin, Phone, Search, Trash2 } from '@lucide/vue'
import api from '../services/api'
import type { ApiResponse, BookingDetails } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const storageKey = 'clinic_guest_booking_lookup'
const loading = ref(false)
const cancelling = ref(false)
const booking = ref<BookingDetails>()
const message = ref('')
const hasSavedLookup = ref(false)
const form = reactive({ phoneNumber: '', code: '', reason: '' })
const canSearch = computed(() => form.phoneNumber.trim() && form.code.trim())

function statusLabel(status: number) {
  return ['قيد الانتظار', 'مؤكد', 'ملغي', 'مكتمل'][status] ?? 'غير معروف'
}

function statusClass(status: number) {
  return status === 2 ? 'cancelled' : status === 3 ? 'completed' : status === 1 ? 'confirmed' : 'pending'
}

function canCancel(status: number) {
  return status === 0 || status === 1
}

function formatDate(value?: string) {
  return value ? new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium' }).format(new Date(value)) : '-'
}

function saveLookup() {
  localStorage.setItem(storageKey, JSON.stringify({ phoneNumber: form.phoneNumber.trim(), code: form.code.trim() }))
  hasSavedLookup.value = true
}

function restoreLookup() {
  const raw = localStorage.getItem(storageKey)
  if (!raw) return
  try {
    const saved = JSON.parse(raw) as { phoneNumber?: string; code?: string }
    form.phoneNumber = saved.phoneNumber ?? ''
    form.code = saved.code ?? ''
    hasSavedLookup.value = Boolean(form.phoneNumber || form.code)
  } catch {
    localStorage.removeItem(storageKey)
  }
}

function clearLookup() {
  localStorage.removeItem(storageKey)
  hasSavedLookup.value = false
  booking.value = undefined
  message.value = ''
  form.phoneNumber = ''
  form.code = ''
}

async function copyCode() {
  if (!booking.value?.code) return
  try {
    await navigator.clipboard.writeText(booking.value.code)
    message.value = 'تم نسخ كود الحجز.'
  } catch {
    message.value = 'تعذر نسخ الكود، انسخه يدوياً.'
  }
}

async function findBooking() {
  if (!canSearch.value) {
    message.value = 'أدخل رقم الهاتف وكود الحجز.'
    return
  }
  loading.value = true
  message.value = ''
  booking.value = undefined
  try {
    const response = await api.get<ApiResponse<BookingDetails>>('/Appointment/guest', {
      params: { phoneNumber: form.phoneNumber, code: form.code },
    })
    booking.value = response.data.data
    saveLookup()
  } catch (error) {
    message.value = getErrorMessage(error)
  } finally {
    loading.value = false
  }
}

async function cancelBooking() {
  if (!booking.value) return
  const accepted = window.confirm(`هل تريد إلغاء حجز الدور #${booking.value.queueNumber}؟`)
  if (!accepted) return
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

onMounted(restoreLookup)
</script>

<template>
  <main class="guest-page">
    <RouterLink class="secondary-button back-link" to="/directory"><ArrowRight :size="17" /> العودة للدليل</RouterLink>
    <section class="guest-card">
      <span class="guest-icon"><CalendarDays :size="32" /></span>
      <h1>متابعة حجز زائر</h1>
      <div v-if="hasSavedLookup" class="saved-lookup">
        <span>تم تعبئة آخر حجز محفوظ.</span>
        <button type="button" class="mini-action" :disabled="loading" @click="findBooking"><Search :size="14" /> عرض</button>
        <button type="button" class="mini-action danger" @click="clearLookup"><Trash2 :size="14" /> مسح</button>
      </div>
      <form class="modal-form" @submit.prevent="findBooking">
        <label><span>رقم الهاتف</span><input v-model="form.phoneNumber" required maxlength="30" /></label>
        <label><span>كود الحجز</span><input v-model="form.code" required /></label>
        <button class="compact-primary" type="submit" :disabled="loading || !canSearch"><Search :size="16" /> {{ loading ? 'جارِ البحث...' : 'بحث' }}</button>
      </form>
      <p v-if="message" class="booking-message">{{ message }}</p>
    </section>

    <section v-if="booking" class="guest-card booking-result">
      <div class="booking-heading">
        <div>
          <h2>{{ booking.doctorName }}</h2>
          <p><MapPin :size="16" /> {{ booking.clinicName }}، {{ booking.clinicAddress }}</p>
        </div>
        <span class="status-pill" :class="statusClass(booking.status)">{{ statusLabel(booking.status) }}</span>
      </div>
      <div class="guest-booking-grid">
        <span>التاريخ <strong>{{ formatDate(booking.appointmentDate) }}</strong></span>
        <span>الدور <strong>#{{ booking.queueNumber }}</strong></span>
        <span>كود الحجز <strong class="code-line">{{ booking.code }} <button type="button" class="copy-inline" @click="copyCode"><Copy :size="14" /></button></strong></span>
        <span>الهاتف <strong>{{ booking.isPhoneConfirmed ? 'مؤكد' : 'بانتظار التأكيد' }}</strong></span>
      </div>
      <div class="booking-actions">
        <a v-if="booking.clinicPhoneNumber" class="secondary-button" :href="`tel:${booking.clinicPhoneNumber}`"><Phone :size="16" /> اتصال بالعيادة</a>
        <a v-if="booking.mapUrl" class="secondary-button" :href="booking.mapUrl" target="_blank" rel="noreferrer"><MapPin :size="16" /> موقع العيادة</a>
      </div>
      <form v-if="canCancel(booking.status)" class="modal-form cancel-form" @submit.prevent="cancelBooking">
        <label><span>سبب الإلغاء</span><textarea v-model="form.reason" rows="3" maxlength="500" /></label>
        <button class="danger-button" type="submit" :disabled="cancelling"><Ban :size="16" /> {{ cancelling ? 'جارِ الإلغاء...' : 'إلغاء الحجز' }}</button>
      </form>
    </section>
  </main>
</template>

<style scoped>
.guest-page { min-height: 100vh; display: grid; align-content: start; gap: 14px; padding: 24px; background: #f6f9f8; }.back-link { width: fit-content; margin: 0 auto; text-decoration: none; }
.guest-card { width: min(100%, 620px); margin: 0 auto; padding: 22px; border: 1px solid var(--line); border-radius: 15px; background: #fff; box-shadow: var(--shadow); }
.guest-icon { display: grid; place-items: center; width: 66px; height: 66px; margin: 0 auto 12px; color: var(--primary); border-radius: 20px; background: var(--primary-soft); }.guest-card h1 { margin: 0 0 16px; text-align: center; font-size: 26px; }
.saved-lookup { display: flex; flex-wrap: wrap; align-items: center; gap: 8px; margin-bottom: 13px; padding: 10px; color: #167163; border: 1px solid #c8eadf; border-radius: 10px; background: #f0faf6; font-size: 13px; }.saved-lookup span { flex: 1; min-width: 170px; font-weight: 700; }
.mini-action { display: inline-flex; align-items: center; gap: 4px; padding: 6px 9px; color: var(--primary); border: 1px solid #bde3d9; border-radius: 8px; background: #fff; cursor: pointer; }.mini-action.danger { color: #b23a3a; border-color: #ffd1d1; }.mini-action:disabled { opacity: .6; cursor: not-allowed; }
.booking-message { color: var(--muted); line-height: 1.8; margin: 13px 0 0; }.booking-result h2 { margin: 0 0 8px; }.booking-result p { display: flex; gap: 6px; color: var(--muted); margin: 0; }
.booking-heading { display: flex; justify-content: space-between; gap: 12px; align-items: flex-start; margin-bottom: 14px; }.status-pill { padding: 6px 10px; border-radius: 999px; font-size: 12px; font-weight: 800; white-space: nowrap; }.status-pill.pending { color: #a46724; background: #fff1db; }.status-pill.confirmed { color: #167163; background: #e1f4ef; }.status-pill.cancelled { color: #b23a3a; background: #ffeded; }.status-pill.completed { color: #126d61; background: #e4f4f0; }
.guest-booking-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 9px; }.guest-booking-grid span { padding: 10px; color: var(--muted); border: 1px solid var(--line); border-radius: 9px; background: #fbfdfc; }.guest-booking-grid strong { display: block; margin-top: 4px; color: var(--ink); }
.code-line { display: flex !important; align-items: center; justify-content: space-between; gap: 8px; }.copy-inline { display: inline-grid; place-items: center; width: 26px; height: 26px; color: var(--primary); border: 1px solid var(--line); border-radius: 8px; background: #fff; cursor: pointer; }
.booking-actions { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 12px; }.booking-actions a { text-decoration: none; }
.cancel-form { margin-top: 14px; }
@media (max-width: 520px) { .guest-booking-grid { grid-template-columns: 1fr; }.booking-heading { display: grid; }.status-pill { width: fit-content; } }
</style>
