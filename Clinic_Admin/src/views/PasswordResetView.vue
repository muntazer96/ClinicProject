<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import axios from 'axios'
import { Eye, EyeOff, HeartPulse, KeyRound } from '@lucide/vue'
import api from '../services/api'
import type { ApiResponse } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const route = useRoute()
const loading = ref(false)
const done = ref(false)
const message = ref('')
const showPassword = ref(false)
const showConfirm = ref(false)
const form = reactive({ newPassword: '', confirmPassword: '' })
const userId = computed(() => String(route.query.userId ?? ''))
const token = computed(() => String(route.query.token ?? ''))
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
  if (!form.newPassword) return 'أدخل كلمة مرور لا تقل عن ستة أحرف.'
  if (passwordScore.value <= 2) return 'كلمة المرور ضعيفة'
  if (passwordScore.value <= 4) return 'كلمة المرور متوسطة'
  return 'كلمة المرور قوية'
})
const strengthClass = computed(() => {
  if (!form.newPassword || passwordScore.value <= 2) return 'weak'
  if (passwordScore.value <= 4) return 'medium'
  return 'strong'
})
const canSubmit = computed(() => Boolean(form.newPassword && form.confirmPassword && !loading.value))

async function resetPassword() {
  message.value = ''
  if (!userId.value || !token.value) {
    message.value = 'رابط إعادة تعيين كلمة المرور غير مكتمل.'
    return
  }
  if (form.newPassword.length < 6) {
    message.value = 'كلمة المرور يجب أن لا تقل عن ستة أحرف.'
    return
  }
  if (form.newPassword !== form.confirmPassword) {
    message.value = 'تأكيد كلمة المرور غير مطابق.'
    return
  }

  loading.value = true
  try {
    const response = await axios.post<ApiResponse<string>>(`${api.defaults.baseURL}/User/password/reset`, {
      userId: userId.value,
      token: token.value,
      newPassword: form.newPassword,
    }, { timeout: 15000 })
    done.value = true
    message.value = response.data.message || 'تم تغيير كلمة المرور بنجاح.'
  } catch (error) {
    message.value = getErrorMessage(error)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <main class="public-auth-page">
    <section class="public-auth-card">
      <div class="public-auth-brand"><HeartPulse :size="28" /> عيادتي</div>
      <span class="public-auth-icon"><KeyRound :size="34" /></span>
      <h1>{{ done ? 'تم تغيير كلمة المرور' : 'إعادة تعيين كلمة المرور' }}</h1>
      <p>{{ done ? message : 'أدخل كلمة مرور جديدة لحسابك.' }}</p>

      <form v-if="!done" class="modal-form" @submit.prevent="resetPassword">
        <p v-if="message" class="form-error">{{ message }}</p>
        <label>
          <span>كلمة المرور الجديدة</span>
          <div class="public-password-wrap">
            <input v-model="form.newPassword" :type="showPassword ? 'text' : 'password'" required minlength="6" autocomplete="new-password" />
            <button type="button" class="public-password-toggle" @click="showPassword = !showPassword" :title="showPassword ? 'إخفاء كلمة المرور' : 'إظهار كلمة المرور'">
              <EyeOff v-if="showPassword" :size="17" />
              <Eye v-else :size="17" />
            </button>
          </div>
        </label>
        <div class="password-strength" :class="strengthClass">
          <span><i :style="{ width: `${Math.max(passwordScore, 1) * 20}%` }" /></span>
          <small>{{ strengthLabel }}</small>
        </div>
        <label>
          <span>تأكيد كلمة المرور</span>
          <div class="public-password-wrap">
            <input v-model="form.confirmPassword" :type="showConfirm ? 'text' : 'password'" required minlength="6" autocomplete="new-password" />
            <button type="button" class="public-password-toggle" @click="showConfirm = !showConfirm" :title="showConfirm ? 'إخفاء كلمة المرور' : 'إظهار كلمة المرور'">
              <EyeOff v-if="showConfirm" :size="17" />
              <Eye v-else :size="17" />
            </button>
          </div>
        </label>
        <button class="compact-primary" type="submit" :disabled="!canSubmit">{{ loading ? 'جارِ الحفظ...' : 'حفظ كلمة المرور' }}</button>
      </form>

      <RouterLink v-else class="compact-primary public-auth-link" to="/login">الانتقال إلى تسجيل الدخول</RouterLink>
    </section>
  </main>
</template>

<style scoped>
.public-auth-page { min-height: 100vh; display: grid; place-items: center; padding: 24px; background: linear-gradient(140deg, #f7faf9, #eef8f5); }
.public-auth-card { width: min(100%, 450px); padding: 30px; text-align: center; border: 1px solid var(--line); border-radius: 16px; background: #fff; box-shadow: var(--shadow); }
.public-auth-brand { display: inline-flex; align-items: center; gap: 9px; margin-bottom: 22px; color: var(--primary); font-size: 24px; font-weight: 800; }
.public-auth-icon { display: grid; place-items: center; width: 72px; height: 72px; margin: 0 auto 16px; color: var(--primary); border-radius: 22px; background: var(--primary-soft); }
.public-auth-card h1 { margin: 0 0 8px; color: var(--ink); font-size: 25px; }.public-auth-card p { margin: 0 0 18px; color: var(--muted); line-height: 1.8; }
.public-password-wrap { display: flex; align-items: center; border: 1px solid var(--line); border-radius: 8px; background: #fff; }.public-password-wrap input { border: 0 !important; }.public-password-toggle { display: grid; place-items: center; width: 38px; height: 38px; margin-inline-end: 4px; color: var(--muted); border: 0; background: transparent; cursor: pointer; }
.password-strength { display: grid; gap: 6px; margin: -2px 0 4px; text-align: start; }.password-strength span { height: 6px; overflow: hidden; border-radius: 999px; background: var(--line); }.password-strength i { display: block; height: 100%; border-radius: inherit; background: #b23a3a; transition: width .18s ease; }.password-strength small { color: #b23a3a; font-size: 12px; }.password-strength.medium i { background: #e5a43c; }.password-strength.medium small { color: #9a6416; }.password-strength.strong i { background: var(--primary); }.password-strength.strong small { color: var(--primary); }
.public-auth-link { margin-top: 10px; text-decoration: none; }
</style>
