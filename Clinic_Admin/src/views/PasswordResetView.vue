<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import api, { ACCESS_TOKEN_KEY, REFRESH_TOKEN_KEY } from '../services/api'
import { useNotifications } from '../composables/useNotifications'
import { getErrorMessage } from '../utils/errors'
import type { ApiResponse } from '../types/api'
import TextInput from '../components/forms/Textinput.vue'

type ResetStep = 'phone' | 'otp' | 'password'

interface PasswordResetOtpResult {
  phoneNumber: string
  resetToken: string
}

interface AuthTokenResult {
  token?: string
  refreshToken?: string
}

const router = useRouter()
const { error: showError, success: showSuccess } = useNotifications()

const loading = ref(false)
const done = ref(false)
const step = ref<ResetStep>('phone')
const resetToken = ref('')

const form = reactive({
  phoneNumber: '',
  otpCode: '',
  newPassword: '',
  confirmPassword: '',
})

const stepIndex = computed(() => {
  if (step.value === 'phone') return 1
  if (step.value === 'otp') return 2
  return 3
})

const normalizedPhone = computed(() => normalizeDigits(form.phoneNumber).trim())
const normalizedOtp = computed(() => normalizeDigits(form.otpCode).trim())

const isPhoneValid = computed(() => /^07\d{9}$/.test(normalizedPhone.value))
const isOtpValid = computed(() => /^\d{6}$/.test(normalizedOtp.value))

const passwordScore = computed(() => {
  let score = 0
  if (form.newPassword.length >= 6) score += 1
  if (form.newPassword.length >= 10) score += 1
  if (/[A-Z]/.test(form.newPassword)) score += 1
  if (/[0-9]/.test(form.newPassword)) score += 1
  if (/[^A-Za-z0-9]/.test(form.newPassword)) score += 1
  return score
})

const strengthLabel = computed(() => {
  if (!form.newPassword) return 'أدخل كلمة مرور لا تقل عن ستة أحرف'
  if (passwordScore.value <= 2) return 'كلمة المرور ضعيفة'
  if (passwordScore.value <= 4) return 'كلمة المرور متوسطة'
  return 'كلمة المرور قوية'
})

const strengthColor = computed(() => {
  if (!form.newPassword) return 'warning'
  if (passwordScore.value <= 2) return 'error'
  if (passwordScore.value <= 4) return 'warning'
  return 'success'
})

const strengthPercentage = computed(() => Math.max(passwordScore.value, 1) * 20)

const passwordsMatch = computed(() => {
  if (!form.confirmPassword) return null
  return form.newPassword === form.confirmPassword
})

const canSendOtp = computed(() => isPhoneValid.value && !loading.value)
const canVerifyOtp = computed(() => isOtpValid.value && !loading.value)
const canResetPassword = computed(() => {
  return (
    Boolean(resetToken.value) &&
    form.newPassword.length >= 6 &&
    form.confirmPassword.length >= 6 &&
    passwordsMatch.value === true &&
    !loading.value
  )
})

function normalizeDigits(value: string) {
  return value
    .replace(/[٠-٩]/g, (digit) => String('٠١٢٣٤٥٦٧٨٩'.indexOf(digit)))
    .replace(/[۰-۹]/g, (digit) => String('۰۱۲۳۴۵۶۷۸۹'.indexOf(digit)))
}

function phoneError() {
  if (!form.phoneNumber) return ''
  if (!isPhoneValid.value) return 'رقم الهاتف يجب أن يكون 11 رقماً ويبدأ بـ 07'
  return ''
}

function otpError() {
  if (!form.otpCode) return ''
  if (!isOtpValid.value) return 'رمز التحقق يجب أن يتكون من 6 أرقام'
  return ''
}

function passwordError() {
  if (!form.newPassword) return ''
  if (form.newPassword.length < 6) return 'كلمة المرور يجب أن لا تقل عن ستة أحرف'
  return ''
}

function confirmPasswordError() {
  if (!form.confirmPassword) return ''
  if (form.newPassword !== form.confirmPassword) return 'تأكيد كلمة المرور غير مطابق'
  return ''
}

async function sendOtp() {
  if (!canSendOtp.value) {
    showError('أدخل رقم هاتف عراقي صحيح يبدأ بـ 07.')
    return
  }

  loading.value = true
  try {
    const response = await api.post<ApiResponse<string>>('/User/password/forgot/send-otp', {
      phoneNumber: normalizedPhone.value,
    })
    step.value = 'otp'
    showSuccess(response.data.message || 'تم إرسال رمز التحقق إلى رقم الهاتف.')
  } catch (error) {
    showError(getErrorMessage(error))
  } finally {
    loading.value = false
  }
}

async function verifyOtp() {
  if (!canVerifyOtp.value) {
    showError('أدخل رمز التحقق المكون من 6 أرقام.')
    return
  }

  loading.value = true
  try {
    const response = await api.post<ApiResponse<PasswordResetOtpResult>>('/User/password/forgot/verify-otp', {
      phoneNumber: normalizedPhone.value,
      otpCode: normalizedOtp.value,
    })

    resetToken.value = response.data.data?.resetToken ?? ''
    if (!resetToken.value) throw new Error('لم يرجع الخادم رمز إعادة تعيين صالحاً.')

    step.value = 'password'
    showSuccess(response.data.message || 'تم التحقق من الرمز بنجاح.')
  } catch (error) {
    showError(getErrorMessage(error))
  } finally {
    loading.value = false
  }
}

async function resetPassword() {
  if (!canResetPassword.value) {
    showError('تأكد من إدخال كلمة مرور صحيحة ومطابقة للتأكيد.')
    return
  }

  loading.value = true
  try {
    const response = await api.post<ApiResponse<AuthTokenResult>>('/User/password/reset', {
      phoneNumber: normalizedPhone.value,
      resetToken: resetToken.value,
      newPassword: form.newPassword,
    })

    const token = response.data.data?.token
    const refreshToken = response.data.data?.refreshToken
    if (token) localStorage.setItem(ACCESS_TOKEN_KEY, token)
    if (refreshToken) localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
    if (token) {
      window.dispatchEvent(new CustomEvent('clinic-auth-refreshed', { detail: { token, refreshToken } }))
    }

    done.value = true
    showSuccess(response.data.message || 'تم تغيير كلمة المرور بنجاح.')
  } catch (error) {
    showError(getErrorMessage(error))
  } finally {
    loading.value = false
  }
}

function goToLogin() {
  router.push('/login')
}

function goToDashboard() {
  router.push('/')
}

function backToPhone() {
  if (loading.value) return
  step.value = 'phone'
  form.otpCode = ''
  resetToken.value = ''
}

function handleSubmit() {
  if (step.value === 'phone') sendOtp()
  else if (step.value === 'otp') verifyOtp()
  else resetPassword()
}
</script>

<template>
  <div class="reset-container">
    <div class="reset-intro">
      <div class="intro-content">
        <div class="brand-intro">
          <v-icon icon="mdi-heart-pulse" size="40" class="brand-icon" />
          <div>
            <h1 class="brand-title">عيادتي</h1>
            <p class="brand-subtitle">لوحة التحكم الطبية</p>
          </div>
        </div>

        <div class="intro-description">
          <h2>استرجاع حسابك</h2>
          <p>أدخل رقم هاتفك، تحقق من رمز OTP، ثم أنشئ كلمة مرور جديدة لحسابك.</p>
        </div>

        <div class="intro-features">
          <div class="feature">
            <v-icon icon="mdi-cellphone-key" class="feature-icon" />
            <span>تحقق بالهاتف</span>
          </div>
          <div class="feature">
            <v-icon icon="mdi-shield-check" class="feature-icon" />
            <span>رمز مؤقت</span>
          </div>
          <div class="feature">
            <v-icon icon="mdi-lock-reset" class="feature-icon" />
            <span>كلمة مرور جديدة</span>
          </div>
        </div>
      </div>
    </div>

    <div class="reset-panel">
      <v-card class="reset-card" elevation="0">
        <div class="mobile-brand">
          <v-icon icon="mdi-heart-pulse" size="28" class="brand-icon-mobile" />
          <h2>عيادتي</h2>
        </div>

        <div class="reset-header">
          <div class="reset-icon">
            <v-icon :icon="done ? 'mdi-check-circle' : 'mdi-key-variant'" size="32" class="icon-content" />
          </div>
          <h2 class="reset-title">{{ done ? 'تم تغيير كلمة المرور' : 'هل نسيت كلمة المرور؟' }}</h2>
          <p class="reset-subtitle">
            {{ done ? 'يمكنك الآن متابعة استخدام حسابك بكلمة المرور الجديدة.' : 'اتبع خطوات التحقق لإعادة تعيين كلمة المرور.' }}
          </p>
        </div>

        <div v-if="!done" class="stepper" aria-label="خطوات إعادة تعيين كلمة المرور">
          <div class="step-item" :class="{ active: stepIndex >= 1 }">
            <span>1</span>
            <p>الهاتف</p>
          </div>
          <div class="step-line" :class="{ active: stepIndex >= 2 }" />
          <div class="step-item" :class="{ active: stepIndex >= 2 }">
            <span>2</span>
            <p>OTP</p>
          </div>
          <div class="step-line" :class="{ active: stepIndex >= 3 }" />
          <div class="step-item" :class="{ active: stepIndex >= 3 }">
            <span>3</span>
            <p>كلمة المرور</p>
          </div>
        </div>

        <div v-if="done" class="reset-success">
          <v-alert type="success" variant="tonal" class="success-alert">
            <v-icon icon="mdi-check-circle" start />
            تم تغيير كلمة المرور بنجاح!
          </v-alert>

          <v-btn color="primary" size="large" block class="reset-button" @click="goToDashboard">
            المتابعة إلى لوحة التحكم
          </v-btn>
          <v-btn variant="text" color="primary" block @click="goToLogin">
            العودة إلى تسجيل الدخول
          </v-btn>
        </div>

        <form v-else class="reset-form" @submit.prevent="handleSubmit">
          <template v-if="step === 'phone'">
            <TextInput
              v-model="form.phoneNumber"
              label="رقم الهاتف"
              type="tel"
              icon="mdi-phone"
              placeholder="07XXXXXXXXX"
              :error="phoneError()"
              :disabled="loading"
              required
              clearable
            />

            <v-btn
              type="submit"
              color="primary"
              size="large"
              block
              class="reset-button"
              :loading="loading"
              :disabled="!canSendOtp"
            >
              إرسال رمز التحقق
            </v-btn>
          </template>

          <template v-else-if="step === 'otp'">
            <TextInput
              v-model="form.otpCode"
              label="رمز التحقق OTP"
              type="tel"
              icon="mdi-message-processing"
              placeholder="أدخل الرمز المكون من 6 أرقام"
              :error="otpError()"
              :disabled="loading"
              required
              clearable
            />

            <p class="helper-text">
              <v-icon icon="mdi-information" size="14" />
              تم إرسال الرمز إلى {{ normalizedPhone }}
            </p>

            <v-btn
              type="submit"
              color="primary"
              size="large"
              block
              class="reset-button"
              :loading="loading"
              :disabled="!canVerifyOtp"
            >
              تأكيد الرمز
            </v-btn>

            <div class="form-actions">
              <v-btn variant="text" color="primary" :disabled="loading" @click="sendOtp">
                إعادة إرسال الرمز
              </v-btn>
              <v-btn variant="text" color="secondary" :disabled="loading" @click="backToPhone">
                تغيير رقم الهاتف
              </v-btn>
            </div>
          </template>

          <template v-else>
            <TextInput
              v-model="form.newPassword"
              label="كلمة المرور الجديدة"
              type="password"
              icon="mdi-lock"
              placeholder="أدخل كلمة المرور الجديدة"
              :error="passwordError()"
              :disabled="loading"
              required
              clearable
            />

            <div class="password-strength">
              <div class="strength-bar">
                <div
                  class="strength-fill"
                  :class="`strength-${strengthColor}`"
                  :style="{ width: `${strengthPercentage}%` }"
                />
              </div>
              <p class="strength-label" :class="`text-${strengthColor}`">
                {{ strengthLabel }}
              </p>
            </div>

            <TextInput
              v-model="form.confirmPassword"
              label="تأكيد كلمة المرور"
              type="password"
              icon="mdi-lock-check"
              placeholder="أدخل كلمة المرور مرة أخرى"
              :error="confirmPasswordError()"
              :disabled="loading"
              required
              clearable
            />

            <div v-if="form.confirmPassword" class="match-indicator">
              <v-icon
                :icon="passwordsMatch ? 'mdi-check-circle' : 'mdi-alert-circle'"
                :color="passwordsMatch ? 'success' : 'error'"
                size="small"
              />
              <span :class="passwordsMatch ? 'text-success' : 'text-error'">
                {{ passwordsMatch ? 'كلمات المرور متطابقة' : 'كلمات المرور غير متطابقة' }}
              </span>
            </div>

            <v-btn
              type="submit"
              color="primary"
              size="large"
              block
              class="reset-button"
              :loading="loading"
              :disabled="!canResetPassword"
            >
              حفظ كلمة المرور الجديدة
            </v-btn>
          </template>
        </form>
      </v-card>
    </div>
  </div>
</template>

<style scoped>
.reset-container {
  display: grid;
  grid-template-columns: 1.05fr 0.95fr;
  min-height: 100vh;
  background: var(--color-background);
}

.reset-intro {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--spacing-3xl);
  background: linear-gradient(140deg, #0a5149, #13796b 58%, #42a799);
  color: white;
  position: relative;
  overflow: hidden;
}

.reset-intro::before {
  content: '';
  position: absolute;
  width: 460px;
  height: 460px;
  border: 70px solid rgba(255, 255, 255, 0.08);
  border-radius: 50%;
  top: -200px;
  left: -180px;
}

.intro-content {
  position: relative;
  z-index: 1;
  max-width: 520px;
}

.brand-intro {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-3xl);
}

.brand-icon {
  color: #d7fffa;
}

.brand-title {
  margin: 0;
  padding: 0;
  font-size: clamp(32px, 5vw, 48px);
  font-weight: 800;
  line-height: 1.2;
  color: white;
}

.brand-subtitle {
  margin: var(--spacing-sm) 0 0 0;
  padding: 0;
  font-size: 14px;
  color: #9ac6bf;
  font-weight: 500;
}

.intro-description {
  margin: var(--spacing-2xl) 0;
}

.intro-description h2 {
  margin: 0 0 var(--spacing-md) 0;
  padding: 0;
  font-size: 32px;
  font-weight: 700;
  line-height: 1.3;
  color: white;
}

.intro-description p {
  margin: 0;
  padding: 0;
  font-size: 15px;
  color: rgba(255, 255, 255, 0.85);
  line-height: 1.8;
}

.intro-features {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--spacing-lg);
  margin: var(--spacing-2xl) 0;
  padding: var(--spacing-lg);
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: var(--radius-lg);
}

.feature {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-sm);
}

.feature-icon {
  font-size: 24px;
  color: #d7fffa;
}

.feature span {
  font-size: 12px;
  font-weight: 600;
  color: white;
}

.reset-panel {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--spacing-lg);
  background-color: var(--color-surface);
}

.reset-card {
  width: 100%;
  max-width: 440px;
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
}

.mobile-brand {
  display: none;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-md);
  padding: var(--spacing-xl);
  color: var(--color-primary);
  border-bottom: 1px solid var(--color-border);
}

.mobile-brand h2 {
  margin: 0;
  padding: 0;
  font-size: 20px;
  font-weight: 700;
}

.brand-icon-mobile {
  color: var(--color-primary);
}

.reset-header {
  padding: var(--spacing-xl);
  text-align: center;
  border-bottom: 1px solid var(--color-border);
}

.reset-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 64px;
  height: 64px;
  margin: 0 auto var(--spacing-lg);
  background: var(--color-primary-soft);
  border-radius: var(--radius-lg);
}

.icon-content {
  color: var(--color-primary);
}

.reset-title {
  margin: 0 0 var(--spacing-sm) 0;
  padding: 0;
  font-size: 24px;
  font-weight: 700;
  color: var(--color-text);
  line-height: 1.2;
}

.reset-subtitle {
  margin: 0;
  padding: 0;
  font-size: 14px;
  color: var(--color-text-muted);
  line-height: 1.6;
}

.stepper {
  display: grid;
  grid-template-columns: auto 1fr auto 1fr auto;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-lg) var(--spacing-xl) 0;
}

.step-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  color: var(--color-text-muted);
  min-width: 54px;
}

.step-item span {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  border: 1px solid var(--color-border);
  font-size: 12px;
  font-weight: 700;
  background: var(--color-surface);
}

.step-item p {
  margin: 0;
  font-size: 11px;
  font-weight: 700;
}

.step-item.active {
  color: var(--color-primary);
}

.step-item.active span {
  color: white;
  background: var(--color-primary);
  border-color: var(--color-primary);
}

.step-line {
  height: 2px;
  background: var(--color-border);
  transform: translateY(-11px);
}

.step-line.active {
  background: var(--color-primary);
}

.reset-form {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
  padding: var(--spacing-xl);
}

.reset-success {
  padding: var(--spacing-xl);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.success-alert {
  margin: 0 0 var(--spacing-sm);
}

.password-strength {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.strength-bar {
  width: 100%;
  height: 6px;
  background: var(--color-border);
  border-radius: 999px;
  overflow: hidden;
}

.strength-fill {
  height: 100%;
  border-radius: inherit;
  transition: width 0.3s ease;
}

.strength-fill.strength-error {
  background-color: var(--color-error);
}

.strength-fill.strength-warning {
  background-color: var(--color-warning);
}

.strength-fill.strength-success {
  background-color: var(--color-success);
}

.strength-label {
  margin: 0;
  padding: 0;
  font-size: 12px;
  font-weight: 600;
}

.text-error {
  color: var(--color-error);
}

.text-warning {
  color: var(--color-warning);
}

.text-success {
  color: var(--color-success);
}

.match-indicator,
.helper-text {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  margin: 0;
  padding: var(--spacing-md);
  background: var(--color-background);
  border-radius: var(--radius-md);
  font-size: 12px;
  font-weight: 600;
  line-height: 1.5;
}

.helper-text {
  color: var(--color-text-muted);
  font-weight: 500;
}

.reset-button {
  font-weight: 600;
  text-transform: none;
  letter-spacing: 0;
  margin-top: var(--spacing-md);
}

.form-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-sm);
}

@media (max-width: 1000px) {
  .reset-intro {
    display: none;
  }

  .reset-container {
    grid-template-columns: 1fr;
  }

  .reset-panel {
    min-height: 100vh;
    padding: var(--spacing-lg);
  }

  .mobile-brand {
    display: flex;
  }

  .reset-card {
    max-width: 100%;
  }
}

@media (max-width: 600px) {
  .reset-panel {
    padding: var(--spacing-md);
  }

  .reset-card {
    border: none;
    box-shadow: none;
  }

  .reset-header {
    padding: var(--spacing-lg);
  }

  .reset-form,
  .reset-success {
    padding: var(--spacing-lg);
  }

  .stepper {
    padding: var(--spacing-lg) var(--spacing-lg) 0;
  }

  .reset-title {
    font-size: 20px;
  }

  .reset-subtitle {
    font-size: 13px;
  }

  .reset-icon {
    width: 56px;
    height: 56px;
  }

  .mobile-brand {
    padding: var(--spacing-lg);
  }

  .mobile-brand h2 {
    font-size: 18px;
  }

  .form-actions {
    align-items: stretch;
    flex-direction: column;
  }
}
</style>
