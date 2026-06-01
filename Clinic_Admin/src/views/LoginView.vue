<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { Eye, EyeOff, HeartPulse, LockKeyhole, LogIn, Phone } from '@lucide/vue'
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()
const router = useRouter()
const phoneNumber = ref('')
const password = ref('')
const showPassword = ref(false)

async function submit() {
  try {
    await auth.login(phoneNumber.value.trim(), password.value)
    if (!auth.hasAnyRole(['SuperAdmin', 'DoctorUser'])) {
      auth.logout()
      auth.error = 'هذا الحساب لا يملك صلاحية الدخول إلى لوحة التحكم.'
      return
    }
    await router.push('/')
  } catch {
    // The store exposes the API message.
  }
}
</script>

<template>
  <main class="login-page">
    <section class="login-intro">
      <div class="intro-content">
        <div class="intro-brand"><HeartPulse :size="30" /> عيادتي</div>
        <h1>إدارة عياداتك بوضوح، من مكان واحد.</h1>
        <p>تابع الحجوزات اليومية، حدّث جداول الدوام، وأدر الاشتراكات والعيادات بسهولة.</p>
        <div class="intro-card">
          <span>منصة طبية موحدة</span>
          <strong>لوحة عملية للإدارة والطبيب</strong>
        </div>
      </div>
    </section>

    <section class="login-panel">
      <form class="login-card" @submit.prevent="submit">
        <div class="mobile-brand"><HeartPulse :size="27" /> عيادتي</div>
        <div class="form-heading">
          <span>أهلاً بعودتك</span>
          <h2>تسجيل الدخول</h2>
          <p>أدخل بيانات حسابك للوصول إلى لوحة التحكم.</p>
        </div>

        <p v-if="auth.error" class="form-error">{{ auth.error }}</p>

        <label class="form-field">
          <span>رقم الهاتف أو اسم المستخدم</span>
          <div class="input-wrap">
            <Phone :size="18" />
            <input v-model="phoneNumber" required autocomplete="username" placeholder="مثال: 0770XXXXXXX" />
          </div>
        </label>

        <label class="form-field">
          <span>كلمة المرور</span>
          <div class="input-wrap">
            <LockKeyhole :size="18" />
            <input v-model="password" required :type="showPassword ? 'text' : 'password'" autocomplete="current-password" placeholder="أدخل كلمة المرور" />
            <button type="button" class="password-toggle" @click="showPassword = !showPassword">
              <EyeOff v-if="showPassword" :size="17" />
              <Eye v-else :size="17" />
            </button>
          </div>
        </label>

        <button class="primary-button" type="submit" :disabled="auth.loading">
          <LogIn :size="19" />
          {{ auth.loading ? 'جاري تسجيل الدخول...' : 'دخول إلى لوحة التحكم' }}
        </button>

        <small class="login-note">لوحة التحكم مخصصة لمدير النظام والطبيب المسجل فقط.</small>
      </form>
    </section>
  </main>
</template>
