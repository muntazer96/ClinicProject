<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import axios from 'axios'
import { HeartPulse, KeyRound } from '@lucide/vue'
import api from '../services/api'
import type { ApiResponse } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const route = useRoute()
const loading = ref(false)
const done = ref(false)
const message = ref('')
const form = reactive({ newPassword: '', confirmPassword: '' })
const userId = computed(() => String(route.query.userId ?? ''))
const token = computed(() => String(route.query.token ?? ''))

async function resetPassword() {
  message.value = ''
  if (!userId.value || !token.value) {
    message.value = 'رابط إعادة تعيين كلمة المرور غير مكتمل.'
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
        <label><span>كلمة المرور الجديدة</span><input v-model="form.newPassword" type="password" required minlength="6" /></label>
        <label><span>تأكيد كلمة المرور</span><input v-model="form.confirmPassword" type="password" required minlength="6" /></label>
        <button class="compact-primary" type="submit" :disabled="loading">{{ loading ? 'جارِ الحفظ...' : 'حفظ كلمة المرور' }}</button>
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
.public-auth-link { margin-top: 10px; text-decoration: none; }
</style>
