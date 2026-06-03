<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { CheckCircle2, HeartPulse, LoaderCircle, XCircle } from '@lucide/vue'
import axios from 'axios'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'
import type { ApiResponse } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const route = useRoute()
const auth = useAuthStore()
const loading = ref(true)
const success = ref(false)
const message = ref('')

const userId = computed(() => String(route.query.userId ?? ''))
const token = computed(() => String(route.query.token ?? ''))
const successLink = computed(() => auth.isAuthenticated ? '/' : '/login')
const successActionLabel = computed(() => auth.isAuthenticated ? 'العودة إلى لوحة التحكم' : 'الانتقال إلى تسجيل الدخول')

async function confirmEmail() {
  loading.value = true
  message.value = ''
  try {
    if (!userId.value || !token.value) {
      throw new Error('رابط تأكيد البريد الإلكتروني غير مكتمل.')
    }

    const response = await axios.get<ApiResponse<string>>(`${api.defaults.baseURL}/User/email-confirm`, {
      params: { userId: userId.value, token: token.value },
      timeout: 15000,
    })
    success.value = true
    message.value = response.data.message || 'تم تأكيد البريد الإلكتروني بنجاح.'
  } catch (error) {
    success.value = false
    message.value = getErrorMessage(error)
  } finally {
    loading.value = false
  }
}

onMounted(confirmEmail)
</script>

<template>
  <main class="confirm-page">
    <section class="confirm-card">
      <div class="confirm-brand"><HeartPulse :size="28" /> عيادتي</div>

      <div v-if="loading" class="confirm-state">
        <span class="confirm-icon loading-icon"><LoaderCircle :size="46" /></span>
        <h1>جارِ تأكيد البريد الإلكتروني</h1>
        <p>انتظر لحظة بينما نتحقق من رابط التأكيد.</p>
      </div>

      <div v-else-if="success" class="confirm-state">
        <span class="confirm-icon success-icon"><CheckCircle2 :size="50" /></span>
        <h1>تم تأكيد البريد الإلكتروني</h1>
        <p>{{ message }}</p>
        <RouterLink class="compact-primary confirm-action" :to="successLink">{{ successActionLabel }}</RouterLink>
      </div>

      <div v-else class="confirm-state">
        <span class="confirm-icon error-icon"><XCircle :size="50" /></span>
        <h1>تعذر تأكيد البريد الإلكتروني</h1>
        <p>{{ message }}</p>
        <RouterLink class="secondary-button confirm-action" to="/login">العودة إلى تسجيل الدخول</RouterLink>
      </div>
    </section>
  </main>
</template>

<style scoped>
.confirm-page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  padding: 24px;
  background: linear-gradient(140deg, #f7faf9, #eef8f5);
}

.confirm-card {
  width: min(100%, 460px);
  padding: 30px;
  text-align: center;
  border: 1px solid var(--line);
  border-radius: 16px;
  background: #fff;
  box-shadow: var(--shadow);
}

.confirm-brand {
  display: inline-flex;
  align-items: center;
  gap: 9px;
  margin-bottom: 28px;
  color: var(--primary);
  font-size: 24px;
  font-weight: 800;
}

.confirm-state {
  display: grid;
  justify-items: center;
}

.confirm-icon {
  display: grid;
  place-items: center;
  width: 78px;
  height: 78px;
  margin-bottom: 18px;
  border-radius: 24px;
}

.loading-icon {
  color: var(--primary);
  background: var(--primary-soft);
}

.loading-icon svg {
  animation: spin 1s linear infinite;
}

.success-icon {
  color: #167163;
  background: #e1f4ef;
}

.error-icon {
  color: #a23d3d;
  background: #ffeded;
}

.confirm-state h1 {
  margin: 0 0 8px;
  color: var(--ink);
  font-size: 25px;
}

.confirm-state p {
  margin: 0;
  color: var(--muted);
  line-height: 1.8;
}

.confirm-action {
  margin-top: 22px;
  text-decoration: none;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
