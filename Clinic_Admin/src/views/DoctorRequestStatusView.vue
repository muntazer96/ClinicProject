<script setup lang="ts">
import { ref } from 'vue'
import { getRequestStatus } from '../services/doctorRequestService'
import { getErrorMessage } from '../utils/errors'
import type { DoctorRequestDetails } from '../types/api'

const phone = ref('')
const code = ref('')
const result = ref<DoctorRequestDetails | null>(null)
const loading = ref(false)
const errorMsg = ref('')

const statusConfig: Record<string, { label: string; icon: string; color: string }> = {
  Waiting: { label: 'قيد الانتظار', icon: 'mdi-clock-outline', color: 'warning' },
  Accepted: { label: 'تم القبول', icon: 'mdi-check-circle', color: 'success' },
  Rejected: { label: 'مرفوض', icon: 'mdi-close-circle', color: 'error' },
}

function statusLabel(s: string) {
  return statusConfig[s]?.label ?? s
}

function statusIcon(s: string) {
  return statusConfig[s]?.icon ?? 'mdi-help'
}

function statusColor(s: string) {
  return statusConfig[s]?.color ?? 'default'
}

function validatePhone(val: string) {
  return /^07\d{9}$/.test(val.replace(/\s/g, ''))
}

async function handleSearch() {
  errorMsg.value = ''
  result.value = null
  const p = phone.value.trim()
  if (!p) {
    errorMsg.value = 'يرجى إدخال رقم الهاتف.'
    return
  }
  if (!validatePhone(p)) {
    errorMsg.value = 'رقم الهاتف غير صحيح. يجب أن يبدأ بـ 07 ويتكون من 11 رقمًا.'
    return
  }
  if (!/^DR-[A-Z0-9]{6}$/i.test(code.value.trim())) {
    errorMsg.value = 'صيغة الكود غير صحيحة. مثال: DR-XXXXXX'
    return
  }
  loading.value = true
  try {
    const res = await getRequestStatus(code.value.trim().toUpperCase(), p)
    result.value = res.data!
  } catch (e) {
    errorMsg.value = getErrorMessage(e, 'لم يتم العثور على طلب بهذا الكود ورقم الهاتف.')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <main class="sr-page">
    <div class="sr-glow sr-glow-1"></div>
    <div class="sr-glow sr-glow-2"></div>

    <div class="sr-container">
      <div class="sr-card">
        <!-- Header band -->
        <div class="sr-header">
          <div class="sr-logo">
            <v-icon icon="mdi-heart-pulse" size="28" />
          </div>
          <h1 class="sr-title">متابعة حالة الطلب</h1>
          <p class="sr-subtitle">أدخل رقم الهاتف وكود المتابعة لعرض حالة طلبك</p>
        </div>

        <div class="sr-body">
          <!-- Search Fields -->
          <div class="sr-search">
            <div class="sr-field">
              <label class="sr-label-top">رقم الهاتف</label>
              <div class="sr-input-group">
                <v-icon icon="mdi-phone" size="17" class="sr-input-icon" />
                <input
                  v-model="phone"
                  type="tel"
                  class="sr-input"
                  placeholder="07XXXXXXXXX"
                  maxlength="11"
                  dir="ltr"
                  @keyup.enter="handleSearch"
                />
              </div>
            </div>

            <div class="sr-field">
              <label class="sr-label-top">كود المتابعة</label>
              <div class="sr-input-row">
                <div class="sr-input-group">
                  <v-icon icon="mdi-pound" size="17" class="sr-input-icon" />
                  <input
                    v-model="code"
                    type="text"
                    class="sr-input sr-input-code"
                    placeholder="DR-XXXXXX"
                    maxlength="10"
                    dir="ltr"
                    @keyup.enter="handleSearch"
                  />
                </div>
                <button
                  class="sr-btn"
                  :disabled="!code.trim() || !phone.trim() || loading"
                  @click="handleSearch"
                >
                  <span v-if="loading" class="sr-spinner"></span>
                  <v-icon v-else icon="mdi-magnify" size="18" />
                </button>
              </div>
            </div>
          </div>

          <Transition name="sr-fade">
            <div v-if="errorMsg" class="sr-error">
              <span class="sr-error-dot"></span>
              {{ errorMsg }}
            </div>
          </Transition>

          <!-- Loading -->
          <div v-if="loading" class="sr-loading">
            <span class="sr-spinner sr-spinner--lg"></span>
            جاري البحث...
          </div>

          <!-- Result -->
          <Transition name="sr-fade">
          <div v-if="result && !loading" class="sr-result">
            <div class="sr-status-badge" :class="`sr-status--${statusColor(result.status)}`">
              <v-icon :icon="statusIcon(result.status)" size="26" />
              <span>{{ statusLabel(result.status) }}</span>
            </div>

            <div class="sr-details">
              <div class="sr-row">
                <span class="sr-row-label"><v-icon icon="mdi-account" size="15" /> الاسم الكامل</span>
                <span class="sr-value">{{ result.fullName }}</span>
              </div>
              <div class="sr-row">
                <span class="sr-row-label"><v-icon icon="mdi-phone" size="15" /> رقم الهاتف</span>
                <span class="sr-value" dir="ltr">{{ result.phoneNumber }}</span>
              </div>
              <div class="sr-row">
                <span class="sr-row-label"><v-icon icon="mdi-stethoscope" size="15" /> التخصص</span>
                <span class="sr-value">{{ result.specializationName }}</span>
              </div>
              <div class="sr-row">
                <span class="sr-row-label"><v-icon icon="mdi-map-marker" size="15" /> المحافظة</span>
                <span class="sr-value">{{ result.province }}</span>
              </div>
              <div class="sr-row">
                <span class="sr-row-label"><v-icon icon="mdi-calendar" size="15" /> تاريخ الإرسال</span>
                <span class="sr-value">{{ new Date(result.createdAt).toLocaleDateString('ar-IQ', { year: 'numeric', month: 'short', day: 'numeric' }) }}</span>
              </div>
              <div v-if="result.rejectedReason" class="sr-row sr-row--alert">
                <span class="sr-row-label">سبب الرفض</span>
                <span class="sr-value sr-value--error">{{ result.rejectedReason }}</span>
              </div>
            </div>
          </div>
          </Transition>

          <RouterLink to="/doctor-request" class="sr-back-link">
            <v-icon icon="mdi-arrow-right" size="16" /> تقديم طلب جديد
          </RouterLink>
          
        </div>
      </div>
    </div>
  </main>
</template>

<style scoped>
.sr-page {
  position: relative;
  min-height: 100vh;
  overflow: hidden;
  background: linear-gradient(160deg, #f4faf8 0%, #eaf7f2 45%, #e0f3ec 100%);
  display: flex;
  align-items: flex-start;
  justify-content: center;
  padding: 48px 16px;
}

.sr-glow {
  position: absolute;
  border-radius: 50%;
  filter: blur(80px);
  opacity: 0.35;
  pointer-events: none;
  z-index: 0;
}

.sr-glow-1 {
  width: 420px;
  height: 420px;
  top: -160px;
  right: -120px;
  background: var(--color-primary);
}

.sr-glow-2 {
  width: 360px;
  height: 360px;
  bottom: -140px;
  left: -100px;
  background: #5ad1b3;
}

.sr-container {
  position: relative;
  z-index: 1;
  width: 100%;
  max-width: 480px;
  min-width: 0;
  margin-top: 32px;
}

.sr-card {
  background: rgba(255, 255, 255, 0.86);
  backdrop-filter: blur(28px);
  -webkit-backdrop-filter: blur(28px);
  border: 1px solid rgba(255, 255, 255, 0.6);
  border-radius: 28px;
  box-shadow: 0 20px 60px -12px rgba(15, 80, 65, 0.18), 0 4px 16px rgba(15, 80, 65, 0.08);
  overflow: hidden;
  animation: fadeInUp 0.5s ease both;
}

/* Header band */
.sr-header {
  position: relative;
  text-align: center;
  padding: 36px 32px 28px;
  background: linear-gradient(135deg, var(--color-primary) 0%, #15a589 100%);
  color: #fff;
  overflow: hidden;
}

.sr-header::before {
  content: '';
  position: absolute;
  inset: 0;
  background-image: radial-gradient(circle at 20% 20%, rgba(255,255,255,0.15) 0, transparent 40%),
                     radial-gradient(circle at 85% 75%, rgba(255,255,255,0.12) 0, transparent 45%);
}

.sr-logo {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 60px;
  height: 60px;
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.18);
  border: 1px solid rgba(255, 255, 255, 0.35);
  color: #fff;
  margin-bottom: 16px;
  box-shadow: 0 8px 24px rgba(0,0,0,0.12);
}

.sr-title {
  position: relative;
  margin: 0 0 6px;
  font-size: 21px;
  font-weight: 800;
  letter-spacing: -0.2px;
}

.sr-subtitle {
  position: relative;
  margin: 0;
  font-size: 13.5px;
  color: rgba(255, 255, 255, 0.88);
  line-height: 1.6;
}

.sr-body {
  padding: 28px 32px 32px;
}

/* Search */
.sr-search {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-bottom: 20px;
}

.sr-field {
  display: flex;
  flex-direction: column;
  gap: 7px;
}

.sr-label-top {
  font-size: 13px;
  font-weight: 700;
  color: var(--color-text);
}

.sr-input-row {
  display: flex;
  gap: 8px;
  min-width: 0;
}

.sr-input-group {
  position: relative;
  display: flex;
  align-items: center;
  flex: 1;
  min-width: 0;
}

.sr-input-icon {
  position: absolute;
  right: 13px;
  color: var(--color-primary);
  opacity: 0.7;
  pointer-events: none;
}

.sr-input {
  width: 100%;
  flex: 1;
  padding: 12px 42px 12px 14px;
  border: 1.5px solid var(--color-border);
  border-radius: 13px;
  background: var(--color-surface);
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-size: 15px;
  font-weight: 700;
  letter-spacing: 1px;
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s, background 0.2s;
  direction: ltr;
  text-align: center;
}

.sr-input:hover {
  border-color: #b9ded4;
}

.sr-input:focus {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 4px var(--color-primary-soft);
  background: #fff;
}

.sr-input-code {
  letter-spacing: 2px;
}

.sr-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  flex-shrink: 0;
  border: none;
  background: linear-gradient(135deg, var(--color-primary), #15a589);
  color: #fff;
  border-radius: 13px;
  cursor: pointer;
  box-shadow: 0 8px 20px -6px rgba(16, 159, 132, 0.55);
  transition: transform 0.15s, box-shadow 0.15s;
}

.sr-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 10px 24px -6px rgba(16, 159, 132, 0.65);
}

.sr-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  box-shadow: none;
}

/* Error */
.sr-error {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 12px 16px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #dc2626;
  border-radius: 14px;
  font-size: 13px;
  text-align: center;
  margin-bottom: 16px;
  overflow-wrap: anywhere;
}

.sr-error-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #dc2626;
  flex-shrink: 0;
}

/* Loading */
.sr-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  text-align: center;
  font-size: 14px;
  color: var(--color-text-muted);
  padding: 28px 0;
}

.sr-spinner {
  width: 14px;
  height: 14px;
  border: 2px solid var(--color-primary-soft);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
  flex-shrink: 0;
}

.sr-spinner--lg {
  width: 18px;
  height: 18px;
  border-width: 2.5px;
}

/* Result */
.sr-result {
  margin-top: 4px;
}

.sr-status-badge {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 16px;
  border-radius: 16px;
  font-size: 17px;
  font-weight: 800;
  margin-bottom: 20px;
}

.sr-status--success {
  background: #f0fdf4;
  color: #16a34a;
  border: 1px solid #bbf7d0;
}

.sr-status--warning {
  background: #fffbeb;
  color: #d97706;
  border: 1px solid #fde68a;
}

.sr-status--error {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
}

.sr-details {
  display: flex;
  flex-direction: column;
  gap: 2px;
  margin-bottom: 22px;
  background: #fafdfc;
  border: 1px solid var(--color-border-light);
  border-radius: 16px;
  padding: 6px 16px;
}

.sr-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 11px 0;
  border-bottom: 1px solid var(--color-border-light);
  gap: 12px;
  min-width: 0;
}

.sr-row:last-child {
  border-bottom: none;
}

.sr-row--alert .sr-row-label {
  color: #dc2626;
}

.sr-row-label {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 12.5px;
  color: var(--color-text-muted);
  flex-shrink: 0;
}

.sr-value {
  font-size: 13.5px;
  font-weight: 700;
  color: var(--color-text);
  text-align: left;
  min-width: 0;
  overflow-wrap: anywhere;
}

.sr-value--error {
  color: #dc2626;
}

.sr-back-link {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  font-size: 13.5px;
  color: var(--color-primary);
  text-decoration: none;
  font-weight: 700;
  margin-top: 8px;
}

.sr-back-link:hover {
  text-decoration: underline;
}

/* Transitions */
.sr-fade-enter-active,
.sr-fade-leave-active {
  transition: opacity 0.22s ease, transform 0.22s ease;
}

.sr-fade-enter-from {
  opacity: 0;
  transform: translateY(8px);
}

.sr-fade-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}

@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(16px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 600px) {
  .sr-page { padding: 24px 12px; }
  .sr-header { padding: 28px 22px 22px; }
  .sr-body { padding: 22px 20px 26px; }
}

@media (max-width: 380px) {
  .sr-page { padding: 14px 8px; }
  .sr-container { margin-top: 8px; }
  .sr-card { border-radius: 20px; }
  .sr-header { padding: 22px 16px 18px; }
  .sr-body { padding: 18px 14px 22px; }
  .sr-title { font-size: 18px; }
  .sr-subtitle { font-size: 12.5px; }
  .sr-input-row { flex-direction: column; }
  .sr-btn {
    width: 100%;
    min-height: 44px;
  }
  .sr-row {
    flex-direction: column;
    align-items: flex-start;
  }
  .sr-value { text-align: right; }
}
</style>
