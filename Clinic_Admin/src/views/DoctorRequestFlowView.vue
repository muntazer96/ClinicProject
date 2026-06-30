<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import api from '../services/api'
import { checkPhone, sendOtp, verifyOtp, submitDoctorRequest } from '../services/doctorRequestService'
import { getErrorMessage } from '../utils/errors'
import type { ApiResponse, SpecializationItem, DoctorRequestResponse } from '../types/api'

interface ProvinceItem {
  id: number
  name: string
  normalizedName: string
}

const steps = ['التحقق من رقم الهاتف', 'رمز التحقق', 'معلومات الطلب', 'تم الإرسال']
const currentStep = ref(0)
const loading = ref(false)
const errorMsg = ref('')

const phoneNumber = ref('')
const userId = ref('')
const otpCode = ref('')
const verificationTokenId = ref(0)
const fullName = ref('')
const knownName = ref('')
const selectedProvince = ref<number | null>(null)
const birthDay = ref('')
const selectedSpecialization = ref<number | null>(null)
const identityFront = ref<File | null>(null)
const identityBack = ref<File | null>(null)
const frontPreview = ref('')
const backPreview = ref('')
const requestResult = ref<DoctorRequestResponse | null>(null)

const provinces = ref<ProvinceItem[]>([])
const specializations = ref<SpecializationItem[]>([])

const captchaFirstNumber = ref(0)
const captchaSecondNumber = ref(0)
const captchaAnswer = ref('')
const captchaNonce = ref('')
const captchaExpectedAnswer = computed(() => captchaFirstNumber.value + captchaSecondNumber.value)
const captchaVerified = computed(() =>
  captchaAnswer.value.trim() !== '' &&
  Number(captchaAnswer.value.trim()) === captchaExpectedAnswer.value
)

const isPhoneValid = computed(() => /^07\d{9}$/.test(phoneNumber.value) && captchaVerified.value)
const isOtpValid = computed(() => /^\d{6}$/.test(otpCode.value))
const isFormValid = computed(() =>
  fullName.value.trim().length >= 3 &&
  knownName.value.trim().length >= 3 &&
  selectedProvince.value !== null &&
  birthDay.value !== '' &&
  selectedSpecialization.value !== null &&
  identityFront.value !== null
)

// --- إعادة إرسال الرمز مع عداد تنازلي ---
const resendCooldown = ref(0)
let resendTimer: ReturnType<typeof setInterval> | null = null

function startResendCooldown() {
  resendCooldown.value = 60
  if (resendTimer) clearInterval(resendTimer)
  resendTimer = setInterval(() => {
    resendCooldown.value--
    if (resendCooldown.value <= 0 && resendTimer) {
      clearInterval(resendTimer)
      resendTimer = null
    }
  }, 1000)
}

onUnmounted(() => {
  if (resendTimer) clearInterval(resendTimer)
})

onMounted(async () => {
  try {
    const [pRes, sRes] = await Promise.all([
      api.get<ApiResponse<ProvinceItem[]>>('/IraqiProvince'),
      api.get<ApiResponse<SpecializationItem[]>>('/Specialization'),
    ])
    provinces.value = pRes.data.data ?? []
    specializations.value = sRes.data.data ?? []
  } catch { }

  resetCaptchaChallenge()
})

function getCaptchaNonce() {
  if (typeof crypto !== 'undefined' && 'randomUUID' in crypto) return crypto.randomUUID()
  return `${Date.now()}-${Math.random().toString(36).slice(2)}`
}

function resetCaptchaChallenge() {
  captchaFirstNumber.value = Math.floor(Math.random() * 8) + 2
  captchaSecondNumber.value = Math.floor(Math.random() * 8) + 2
  captchaAnswer.value = ''
  captchaNonce.value = getCaptchaNonce()
}

function getCaptchaToken(): string {
  return `math-challenge:${captchaNonce.value}:${captchaFirstNumber.value}:${captchaSecondNumber.value}:${captchaAnswer.value.trim()}`
}

async function handleCheckPhone() {
  errorMsg.value = ''
  loading.value = true
  try {
    const token = getCaptchaToken()
    if (!captchaVerified.value) {
      errorMsg.value = 'يرجى حل التحقق السريع قبل المتابعة.'
      return
    }
    const res = await checkPhone(phoneNumber.value, token)
    userId.value = res.data!.userId
    currentStep.value = 1
    // إرسال رمز التحقق تلقائياً مرة واحدة فقط عند الدخول للخطوة
    await handleSendOtp()
  } catch (e) {
    errorMsg.value = getErrorMessage(e)
    resetCaptchaChallenge()
  } finally {
    loading.value = false
  }
}

async function handleSendOtp() {
  errorMsg.value = ''
  loading.value = true
  try {
    await sendOtp(userId.value, phoneNumber.value)
    startResendCooldown()
  } catch (e) {
    errorMsg.value = getErrorMessage(e)
  } finally {
    loading.value = false
  }
}

async function handleVerifyOtp() {
  errorMsg.value = ''
  loading.value = true
  try {
    const res = await verifyOtp(userId.value, phoneNumber.value, otpCode.value)
    verificationTokenId.value = res.data!.verificationTokenId
    currentStep.value = 2
  } catch (e) {
    errorMsg.value = getErrorMessage(e)
  } finally {
    loading.value = false
  }
}

function onFrontUpload(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (file) {
    identityFront.value = file
    frontPreview.value = URL.createObjectURL(file)
  }
}

function onBackUpload(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (file) {
    identityBack.value = file
    backPreview.value = URL.createObjectURL(file)
  }
}

async function handleSubmit() {
  errorMsg.value = ''
  loading.value = true
  try {
    const fd = new FormData()
    fd.append('verificationTokenId', verificationTokenId.value.toString())
    fd.append('fullName', fullName.value.trim())
    fd.append('knownName', knownName.value.trim())
    fd.append('province', selectedProvince.value!.toString())
    fd.append('birthDay', birthDay.value)
    fd.append('specializationId', selectedSpecialization.value!.toString())
    fd.append('identityFront', identityFront.value!)
    if (identityBack.value) fd.append('identityBack', identityBack.value)
    const res = await submitDoctorRequest(fd)
    requestResult.value = res.data!
    currentStep.value = 3
  } catch (e) {
    errorMsg.value = getErrorMessage(e)
  } finally {
    loading.value = false
  }
}

function goBack() {
  if (currentStep.value > 0) {
    currentStep.value--
    errorMsg.value = ''
  }
}
</script>

<template>
  <main class="dr-page">
    <div class="dr-glow dr-glow-1"></div>
    <div class="dr-glow dr-glow-2"></div>

    <div class="dr-container">
      <div class="dr-card">
        <!-- Header band -->
        <div class="dr-header">
          <div class="dr-logo">
            <v-icon icon="mdi-heart-pulse" size="28" />
          </div>
          <h1 class="dr-title">تقديم طلب التحويل إلى طبيب</h1>
          <p class="dr-subtitle">قم بتعبئة البيانات التالية لتحويل حسابك إلى حساب طبيب معتمد</p>
        </div>

        <div class="dr-body">
          <!-- Steps Indicator -->
          <div class="dr-stepper">
            <div class="dr-stepper-track">
              <div
                class="dr-stepper-fill"
                :style="{ width: `${(currentStep / (steps.length - 1)) * 100}%` }"
              ></div>
            </div>
            <div class="dr-steps">
              <div
                v-for="(step, i) in steps"
                :key="i"
                class="dr-step"
                :class="{
                  'dr-step--active': currentStep === i,
                  'dr-step--done': currentStep > i,
                }"
              >
                <div class="dr-step-num">
                  <v-icon v-if="currentStep > i" icon="mdi-check-circle" size="14" />
                  <span v-else>{{ i + 1 }}</span>
                </div>
                <span class="dr-step-label">{{ step }}</span>
              </div>
            </div>
          </div>

          <!-- Error -->
          <Transition name="dr-fade">
            <div v-if="errorMsg" class="dr-error">
              <span class="dr-error-dot"></span>
              {{ errorMsg }}
            </div>
          </Transition>

          <!-- Step 0: Phone -->
          <Transition name="dr-fade" mode="out-in">
          <div v-if="currentStep === 0" class="dr-step-content" key="0">
            <div class="dr-field">
              <label class="dr-label">رقم الهاتف</label>
              <div class="dr-input-group">
                <v-icon icon="mdi-phone" size="18" class="dr-input-icon" />
                <input
                  v-model="phoneNumber"
                  type="tel"
                  class="dr-input"
                  placeholder="07XXXXXXXXX"
                  maxlength="11"
                  dir="ltr"
                />
              </div>
              <div class="dr-hint">أدخل رقم هاتفك المسجل في التطبيق</div>
            </div>

            <div class="dr-captcha">
              <div class="dr-captcha-question">
                <span class="dr-captcha-box" :class="{ 'dr-captcha-box--valid': captchaVerified }">
                  <v-icon :icon="captchaVerified ? 'mdi-check-circle' : 'mdi-shield-check'" size="14" />
                </span>
                <span class="dr-captcha-label">
                  تحقق سريع: كم ناتج {{ captchaFirstNumber }} + {{ captchaSecondNumber }}؟
                </span>
              </div>
              <input
                v-model="captchaAnswer"
                type="text"
                inputmode="numeric"
                pattern="[0-9]*"
                class="dr-input dr-captcha-answer"
                placeholder="اكتب الناتج"
                @keyup.enter="handleCheckPhone"
              />
            </div>

            <button
              class="dr-btn dr-btn-primary dr-btn-block"
              :disabled="!isPhoneValid || loading"
              @click="handleCheckPhone"
            >
              <span v-if="loading" class="dr-spinner"></span>
              <template v-if="loading">جاري التحقق...</template>
              <template v-else>
                تحقق من الرقم
                <v-icon icon="mdi-arrow-left" size="18" />
              </template>
            </button>
          </div>

          <!-- Step 1: OTP -->
          <div v-else-if="currentStep === 1" class="dr-step-content" key="1">
            <div class="dr-field">
              <label class="dr-label">رمز التحقق</label>
              <div class="dr-input-group">
                <v-icon icon="mdi-shield-check" size="18" class="dr-input-icon" />
                <input
                  v-model="otpCode"
                  type="text"
                  class="dr-input dr-otp"
                  placeholder="000000"
                  maxlength="6"
                  dir="ltr"
                  @keyup.enter="handleVerifyOtp"
                />
              </div>
              <div class="dr-hint">
                تم إرسال رمز التحقق إلى <strong class="dr-hint-strong">{{ phoneNumber }}</strong> عبر واتساب
              </div>

              <!-- إعادة إرسال الرمز -->
              <div class="dr-resend">
                <button
                  v-if="resendCooldown === 0"
                  type="button"
                  class="dr-resend-btn"
                  :disabled="loading"
                  @click="handleSendOtp"
                >
                  <v-icon icon="mdi-refresh" size="15" />
                  إعادة إرسال الرمز
                </button>
                <span v-else class="dr-resend-wait">
                  يمكنك إعادة الإرسال خلال {{ resendCooldown }} ثانية
                </span>
              </div>
            </div>
            <div class="dr-btn-row">
              <button class="dr-btn dr-btn-ghost" @click="goBack">
                <v-icon icon="mdi-arrow-right" size="18" /> رجوع
              </button>
              <button
                class="dr-btn dr-btn-primary"
                :disabled="!isOtpValid || loading"
                @click="handleVerifyOtp"
              >
                <span v-if="loading" class="dr-spinner"></span>
                <template v-if="loading">جاري التحقق...</template>
                <template v-else>
                  تحقق
                  <v-icon icon="mdi-arrow-left" size="18" />
                </template>
              </button>
            </div>
          </div>

          <!-- Step 2: Form -->
          <div v-else-if="currentStep === 2" class="dr-step-content" key="2">
            <div class="dr-form-grid">
              <div class="dr-field">
                <label class="dr-label">الاسم الكامل</label>
                <div class="dr-input-group">
                  <v-icon icon="mdi-account" size="18" class="dr-input-icon" />
                  <input v-model="fullName" type="text" class="dr-input" placeholder="الاسم الرباعي" />
                </div>
              </div>
              <div class="dr-field">
                <label class="dr-label">الاسم المعروف به</label>
                <div class="dr-input-group">
                  <v-icon icon="mdi-account" size="18" class="dr-input-icon" />
                  <input v-model="knownName" type="text" class="dr-input" placeholder="مثال: د. محمد" />
                </div>
              </div>
              <div class="dr-field">
                <label class="dr-label">المحافظة</label>
                <div class="dr-input-group">
                  <v-icon icon="mdi-map-marker" size="18" class="dr-input-icon" />
                  <v-autocomplete
                    v-model="selectedProvince"
                    :items="provinces.map(p => ({ value: p.id, label: p.name }))"
                    item-title="label"
                    item-value="value"
                    class="dr-input dr-select"
                    density="compact"
                    variant="plain"
                    hide-details
                    placeholder="اختر المحافظة"
                  />
                </div>
              </div>
              <div class="dr-field">
                <label class="dr-label">التخصص</label>
                <div class="dr-input-group">
                  <v-icon icon="mdi-stethoscope" size="18" class="dr-input-icon" />
                  <v-autocomplete
                    v-model="selectedSpecialization"
                    :items="specializations.map(s => ({ value: s.id, label: s.name }))"
                    item-title="label"
                    item-value="value"
                    class="dr-input dr-select"
                    density="compact"
                    variant="plain"
                    hide-details
                    placeholder="اختر التخصص"
                  />
                </div>
              </div>
              <div class="dr-field dr-field-full">
                <label class="dr-label">تاريخ الميلاد</label>
                <div class="dr-input-group">
                  <v-icon icon="mdi-cake-variant" size="18" class="dr-input-icon" />
                  <input v-model="birthDay" type="date" class="dr-input" />
                </div>
              </div>

              <div class="dr-field dr-field-full">
                <label class="dr-label">صورة الهوية (الوجه الأمامي)</label>
                <label class="dr-upload" :class="{ 'dr-upload--filled': identityFront }">
                  <input type="file" accept="image/*" hidden @change="onFrontUpload" />
                  <template v-if="!frontPreview">
                    <v-icon icon="mdi-cloud-upload" size="22" class="dr-upload-icon" />
                    <span class="dr-upload-text">اضغط لاختيار صورة الهوية (الوجه الأمامي)</span>
                  </template>
                  <div v-else class="dr-upload-filled">
                    <img :src="frontPreview" class="dr-preview" />
                    <span class="dr-upload-filename">
                      <v-icon icon="mdi-card-account-details" size="14" /> {{ identityFront?.name }}
                    </span>
                  </div>
                </label>
              </div>

              <div class="dr-field dr-field-full">
                <label class="dr-label">صورة الهوية (الوجه الخلفي) <span class="dr-optional">اختياري</span></label>
                <label class="dr-upload" :class="{ 'dr-upload--filled': identityBack }">
                  <input type="file" accept="image/*" hidden @change="onBackUpload" />
                  <template v-if="!backPreview">
                    <v-icon icon="mdi-image" size="22" class="dr-upload-icon" />
                    <span class="dr-upload-text">اضغط لاختيار صورة الهوية (الوجه الخلفي)</span>
                  </template>
                  <div v-else class="dr-upload-filled">
                    <img :src="backPreview" class="dr-preview" />
                    <span class="dr-upload-filename">
                      <v-icon icon="mdi-card-account-details" size="14" /> {{ identityBack?.name }}
                    </span>
                  </div>
                </label>
              </div>
            </div>

            <div class="dr-btn-row">
              <button class="dr-btn dr-btn-ghost" @click="goBack">
                <v-icon icon="mdi-arrow-right" size="18" /> رجوع
              </button>
              <button
                class="dr-btn dr-btn-primary"
                :disabled="!isFormValid || loading"
                @click="handleSubmit"
              >
                <span v-if="loading" class="dr-spinner"></span>
                <template v-if="loading">جاري الإرسال...</template>
                <template v-else>
                  إرسال الطلب
                  <v-icon icon="mdi-arrow-left" size="18" />
                </template>
              </button>
            </div>
          </div>

          <!-- Step 3: Success -->
          <div v-else-if="currentStep === 3 && requestResult" class="dr-step-content dr-success" key="3">
            <div class="dr-success-icon">
              <v-icon icon="mdi-check-circle" size="44" />
            </div>
            <h2 class="dr-success-title">تم إرسال الطلب بنجاح</h2>
            <p class="dr-success-code">
              كود متابعة الطلب
              <strong>{{ requestResult.code }}</strong>
            </p>
            <div class="dr-success-info">
              <p>يمكنك متابعة حالة الطلب باستخدام الكود أعلاه</p>
              <p>سيتم مراجعة طلبك من قبل الإدارة والتواصل معك عبر واتساب</p>
            </div>
            <div class="dr-btn-row dr-btn-row-center">
              <RouterLink to="/doctor-request/status" class="dr-btn dr-btn-outline">
                متابعة حالة الطلب
              </RouterLink>
              <RouterLink to="/download" class="dr-btn dr-btn-primary">
                العودة للصفحة الرئيسية
              </RouterLink>
            </div>
          </div>
          </Transition>
        </div>
      </div>

      <div class="dr-footer-link">
        <RouterLink to="/download">تحميل التطبيق</RouterLink>
        <span class="dr-footer-sep">|</span>
        لديك طلب موجود؟
        <RouterLink to="/doctor-request/status">متابعة حالة الطلب</RouterLink>
      </div>
    </div>
  </main>
</template>

<style scoped>
.dr-page {
  position: relative;
  min-height: 100vh;
  overflow: hidden;
  background: linear-gradient(160deg, #f4faf8 0%, #eaf7f2 45%, #e0f3ec 100%);
  display: flex;
  align-items: flex-start;
  justify-content: center;
  padding: 48px 16px;
}

.dr-glow {
  position: absolute;
  border-radius: 50%;
  filter: blur(80px);
  opacity: 0.35;
  pointer-events: none;
  z-index: 0;
}

.dr-glow-1 {
  width: 420px;
  height: 420px;
  top: -160px;
  right: -120px;
  background: var(--color-primary);
}

.dr-glow-2 {
  width: 360px;
  height: 360px;
  bottom: -140px;
  left: -100px;
  background: #5ad1b3;
}

.dr-container {
  position: relative;
  z-index: 1;
  width: 100%;
  max-width: 580px;
  min-width: 0;
}

.dr-card {
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
.dr-header {
  position: relative;
  text-align: center;
  padding: 36px 32px 28px;
  background: linear-gradient(135deg, var(--color-primary) 0%, #15a589 100%);
  color: #fff;
  overflow: hidden;
}

.dr-header::before {
  content: '';
  position: absolute;
  inset: 0;
  background-image: radial-gradient(circle at 20% 20%, rgba(255,255,255,0.15) 0, transparent 40%),
                     radial-gradient(circle at 85% 75%, rgba(255,255,255,0.12) 0, transparent 45%);
}

.dr-logo {
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

.dr-title {
  position: relative;
  margin: 0 0 6px;
  font-size: 21px;
  font-weight: 800;
  letter-spacing: -0.2px;
}

.dr-subtitle {
  position: relative;
  margin: 0;
  font-size: 13.5px;
  color: rgba(255, 255, 255, 0.88);
  line-height: 1.6;
}

.dr-body {
  padding: 28px 32px 32px;
}

/* Stepper */
.dr-stepper {
  margin-bottom: 28px;
}

.dr-stepper-track {
  position: relative;
  height: 4px;
  background: var(--color-border);
  border-radius: 999px;
  margin-bottom: 16px;
  overflow: hidden;
}

.dr-stepper-fill {
  position: absolute;
  inset-block: 0;
  right: 0;
  background: linear-gradient(90deg, var(--color-primary), #15a589);
  border-radius: 999px;
  transition: width 0.45s cubic-bezier(0.4, 0, 0.2, 1);
}

.dr-steps {
  display: flex;
  justify-content: space-between;
  gap: 4px;
}

.dr-step {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  font-size: 11px;
  color: var(--color-text-muted);
  flex: 1;
  min-width: 0;
  text-align: center;
}

.dr-step-num {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 26px;
  height: 26px;
  border-radius: 50%;
  background: var(--color-surface);
  border: 2px solid var(--color-border);
  color: var(--color-text-muted);
  font-weight: 700;
  font-size: 11.5px;
  flex-shrink: 0;
  transition: all 0.25s ease;
}

.dr-step--active .dr-step-num {
  background: var(--color-primary);
  border-color: var(--color-primary);
  color: #fff;
  box-shadow: 0 0 0 4px var(--color-primary-soft);
}

.dr-step--active .dr-step-label {
  color: var(--color-primary);
  font-weight: 700;
}

.dr-step--done .dr-step-num {
  background: #16a34a;
  border-color: #16a34a;
  color: #fff;
}

.dr-step--done .dr-step-label {
  color: #16a34a;
}

.dr-step-label {
  display: none;
  line-height: 1.3;
}

@media (min-width: 480px) {
  .dr-step-label {
    display: inline;
  }
}

/* Error */
.dr-error {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 13px 16px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #dc2626;
  border-radius: 14px;
  font-size: 13.5px;
  margin-bottom: 20px;
  overflow-wrap: anywhere;
}

.dr-error-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #dc2626;
  flex-shrink: 0;
}

/* Content */
.dr-step-content {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.dr-form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 18px;
  margin-bottom: 4px;
}

.dr-field {
  display: flex;
  flex-direction: column;
  gap: 7px;
}

.dr-field-full {
  grid-column: 1 / -1;
}

.dr-label {
  font-size: 13px;
  font-weight: 700;
  color: var(--color-text);
}

.dr-optional {
  font-weight: 500;
  color: var(--color-text-muted);
  font-size: 12px;
}

.dr-input-group {
  position: relative;
  display: flex;
  align-items: center;
  min-width: 0;
}

.dr-input-icon {
  position: absolute;
  right: 13px;
  color: var(--color-primary);
  opacity: 0.7;
  pointer-events: none;
}

.dr-input,
.dr-select {
  width: 100%;
  padding: 12px 42px 12px 14px;
  border: 1.5px solid var(--color-border);
  border-radius: 13px;
  background: var(--color-surface);
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s, background 0.2s;
}

.dr-input:hover,
.dr-select:hover {
  border-color: #b9ded4;
}

.dr-input:focus,
.dr-select:focus {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 4px var(--color-primary-soft);
  background: #fff;
}

.dr-otp {
  letter-spacing: 8px;
  font-weight: 700;
  font-size: 18px;
  text-align: center;
  padding-inline: 14px;
}

.dr-select {
  appearance: auto;
  cursor: pointer;
}

.dr-hint {
  font-size: 12.5px;
  color: var(--color-text-muted);
  line-height: 1.6;
}

.dr-hint-strong {
  color: var(--color-primary);
  direction: ltr;
  display: inline-block;
}

/* Resend OTP */
.dr-resend {
  margin-top: 4px;
}

.dr-resend-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: none;
  border: none;
  padding: 0;
  color: var(--color-primary);
  font-weight: 700;
  font-size: 12.5px;
  cursor: pointer;
}

.dr-resend-btn:hover:not(:disabled) {
  text-decoration: underline;
}

.dr-resend-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.dr-resend-wait {
  font-size: 12.5px;
  color: var(--color-text-muted);
}

/* Captcha */
.dr-captcha {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 14px 16px;
  margin: 6px 0 4px;
  background: #fafafa;
  border: 1.5px solid var(--color-border);
  border-radius: 14px;
  transition: border-color 0.2s, background 0.2s;
}

.dr-captcha:hover {
  border-color: var(--color-primary);
  background: var(--color-primary-soft);
}

.dr-captcha-question {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
}

.dr-captcha-box {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 22px;
  height: 22px;
  border: 2px solid var(--color-border);
  border-radius: 6px;
  background: #fff;
  color: var(--color-primary);
  flex-shrink: 0;
  transition: all 0.15s ease;
}

.dr-captcha-box--valid {
  background: var(--color-primary);
  border-color: var(--color-primary);
  color: #fff;
}

.dr-captcha-label {
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text);
  line-height: 1.45;
  overflow-wrap: anywhere;
}

.dr-captcha-answer {
  width: 120px;
  min-height: 38px;
  padding-block: 8px;
  text-align: center;
  direction: ltr;
}

/* Upload */
.dr-upload {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 22px 16px;
  border: 2px dashed var(--color-border);
  border-radius: 16px;
  cursor: pointer;
  font-size: 13px;
  color: var(--color-text-muted);
  background: #fafdfc;
  transition: border-color 0.2s, background 0.2s;
  text-align: center;
}

.dr-upload:hover {
  border-color: var(--color-primary);
  background: var(--color-primary-soft);
}

.dr-upload--filled {
  padding: 12px;
  border-style: solid;
  border-color: var(--color-primary);
  background: var(--color-primary-soft);
}

.dr-upload-icon {
  color: var(--color-primary);
}

.dr-upload-text {
  line-height: 1.5;
}

.dr-upload-filled {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  width: 100%;
}

.dr-upload-filename {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  max-width: 100%;
  font-size: 12.5px;
  font-weight: 600;
  color: var(--color-primary-dark);
  overflow-wrap: anywhere;
}

.dr-preview {
  width: 100%;
  max-height: 150px;
  object-fit: contain;
  border-radius: 10px;
  background: #fff;
  border: 1px solid var(--color-border);
}

/* Buttons */
.dr-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 13px 26px;
  border: none;
  font-family: var(--font-family-primary);
  font-weight: 700;
  font-size: 14px;
  border-radius: 14px;
  cursor: pointer;
  transition: background 0.15s, transform 0.15s, box-shadow 0.15s;
  user-select: none;
  min-width: 0;
  text-align: center;
  line-height: 1.4;
}

.dr-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.dr-btn-primary {
  background: linear-gradient(135deg, var(--color-primary), #15a589);
  color: #fff;
  box-shadow: 0 8px 20px -6px rgba(16, 159, 132, 0.55);
}

.dr-btn-primary:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 10px 24px -6px rgba(16, 159, 132, 0.65);
}

.dr-btn-primary:active:not(:disabled) {
  transform: translateY(0);
}

.dr-btn-outline {
  background: #fff;
  color: var(--color-text);
  border: 2px solid var(--color-border);
}

.dr-btn-outline:hover {
  border-color: var(--color-primary);
  color: var(--color-primary);
}

.dr-btn-ghost {
  background: transparent;
  color: var(--color-text-muted);
}

.dr-btn-ghost:hover {
  color: var(--color-text);
}

.dr-btn-block {
  width: 100%;
  margin-top: 6px;
}

.dr-btn-row {
  display: flex;
  gap: 12px;
  justify-content: space-between;
  margin-top: 24px;
}

.dr-btn-row-center {
  justify-content: center;
  flex-wrap: wrap;
}

.dr-spinner {
  width: 14px;
  height: 14px;
  border: 2px solid rgba(255, 255, 255, 0.4);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}

/* Success */
.dr-success {
  text-align: center;
  align-items: center;
}

.dr-success-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 84px;
  height: 84px;
  border-radius: 50%;
  background: #dcfce7;
  color: #16a34a;
  margin-bottom: 18px;
  animation: pop 0.4s cubic-bezier(0.34, 1.56, 0.64, 1) both;
}

.dr-success-title {
  margin: 0 0 10px;
  font-size: 21px;
  font-weight: 800;
  color: var(--color-text);
}

.dr-success-code {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-size: 13.5px;
  margin: 0 0 18px;
  padding: 10px 18px;
  background: var(--color-primary-soft);
  color: var(--color-primary-dark);
  border-radius: 12px;
}

.dr-success-code strong {
  font-size: 16px;
  font-weight: 800;
  direction: ltr;
  letter-spacing: 1px;
}

.dr-success-info {
  font-size: 13.5px;
  color: var(--color-text-muted);
  line-height: 1.8;
  margin-bottom: 22px;
}

.dr-success-info p {
  margin: 4px 0;
}

.dr-footer-link {
  text-align: center;
  margin-top: 22px;
  font-size: 13.5px;
  color: var(--color-text-muted);
}

.dr-footer-link a {
  color: var(--color-primary);
  font-weight: 700;
  text-decoration: none;
}

.dr-footer-link a:hover {
  text-decoration: underline;
}

.dr-footer-sep {
  color: var(--color-border);
  margin: 0 8px;
}

/* Transitions */
.dr-fade-enter-active,
.dr-fade-leave-active {
  transition: opacity 0.22s ease, transform 0.22s ease;
}

.dr-fade-enter-from {
  opacity: 0;
  transform: translateY(8px);
}

.dr-fade-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}

@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(16px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes pop {
  from { opacity: 0; transform: scale(0.6); }
  to { opacity: 1; transform: scale(1); }
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 600px) {
  .dr-page { padding: 24px 12px; }
  .dr-header { padding: 28px 22px 22px; }
  .dr-body { padding: 22px 20px 26px; }
  .dr-form-grid { grid-template-columns: 1fr; }
  .dr-captcha { flex-direction: column; align-items: stretch; }
  .dr-captcha-answer { width: 100%; }
  .dr-btn-row { flex-direction: column; }
  .dr-btn-row-center { flex-direction: row; }
}

@media (max-width: 380px) {
  .dr-page { padding: 14px 8px; }
  .dr-card { border-radius: 20px; }
  .dr-header { padding: 22px 16px 18px; }
  .dr-body { padding: 18px 14px 22px; }
  .dr-title { font-size: 18px; }
  .dr-subtitle { font-size: 12.5px; }
  .dr-steps { gap: 2px; }
  .dr-step-num {
    width: 24px;
    height: 24px;
    font-size: 10.5px;
  }
  .dr-btn,
  .dr-btn-row-center .dr-btn {
    width: 100%;
    padding-inline: 14px;
  }
  .dr-btn-row-center { flex-direction: column; }
  .dr-footer-link {
    display: flex;
    flex-direction: column;
    gap: 6px;
  }
  .dr-footer-sep { display: none; }
}
</style>