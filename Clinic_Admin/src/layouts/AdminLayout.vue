<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'
import {
  BadgePercent, BarChart3, Bell, BellRing, Building2, CalendarDays, ChevronDown, ChevronLeft, ClipboardList, HeartPulse,
  House, KeyRound, LogOut, Menu, MessageCircle, MessageSquareText, Smartphone, Stethoscope, UserRound, UsersRound, X,
} from '@lucide/vue'
import AppModal from '../components/AppModal.vue'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'
import { useNotificationsStore } from '../stores/notifications'
import { getErrorMessage } from '../utils/errors'
import godevLogo from '../assets/godev_logo.png'
import type { ApiResponse, CurrentDoctorSubscription } from '../types/api'

const auth = useAuthStore()
const notifications = useNotificationsStore()
const route = useRoute()
const router = useRouter()
const sidebarOpen = ref(false)
const accountMenuOpen = ref(false)
const changePasswordOpen = ref(false)
const savingPassword = ref(false)
const passwordForm = ref({ currentPassword: '', newPassword: '', confirmPassword: '' })
const currentSubscription = ref<CurrentDoctorSubscription>()

const links = computed(() => [
  { label: 'الرئيسية', to: '/', icon: House, roles: ['SuperAdmin', 'DoctorUser'] },
  { label: 'الإحصائيات', to: '/analytics', icon: BarChart3, roles: ['SuperAdmin', 'DoctorUser'] },
  { label: 'المستخدمون', to: '/users', icon: UsersRound, roles: ['SuperAdmin'] },
  { label: 'الأطباء', to: '/doctors', icon: Stethoscope, roles: ['SuperAdmin'] },
  { label: 'الاشتراكات', to: '/subscriptions', icon: ClipboardList, roles: ['SuperAdmin'] },
  { label: 'WhatsApp OTP', to: '/whatsapp', icon: MessageCircle, roles: ['SuperAdmin'] },
  { label: 'العروض', to: '/offers', icon: BadgePercent, roles: ['SuperAdmin', 'DoctorUser'] },
  { label: 'إصدارات التطبيق', to: '/app-versions', icon: Smartphone, roles: ['SuperAdmin'] },
  { label: 'الحجوزات', to: '/appointments', icon: CalendarDays, roles: ['SuperAdmin', 'DoctorUser'] },
  { label: 'عياداتي', to: '/clinics', icon: Building2, roles: ['DoctorUser'] },
  { label: 'الإجازات', to: '/exceptions', icon: Bell, roles: ['DoctorUser'] },
  { label: 'الإشعارات', to: '/notifications', icon: BellRing, roles: ['DoctorUser'] },
  { label: 'التقييمات', to: '/reviews', icon: MessageSquareText, roles: ['DoctorUser'] },
  { label: 'الملف الشخصي', to: '/profile', icon: UserRound, roles: ['DoctorUser'] },
].filter((link) => auth.hasAnyRole(link.roles)))

const pageTitle = computed(() => (route.meta.title as string | undefined) ?? 'لوحة التحكم')
const roleLabel = computed(() => auth.primaryRole === 'SuperAdmin' ? 'مدير النظام' : 'حساب الطبيب')
const hasPremiumTheme = computed(() => currentSubscription.value?.isTopPackage ?? false)

function isLinkActive(path: string) {
  if (path === '/') return route.path === '/'
  return route.path === path || route.path.startsWith(`${path}/`)
}

function signOut() {
  auth.logout()
  router.push('/login')
}

function openChangePassword() {
  accountMenuOpen.value = false
  passwordForm.value = { currentPassword: '', newPassword: '', confirmPassword: '' }
  changePasswordOpen.value = true
}

async function changePassword() {
  if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
    notifications.show('تأكيد كلمة المرور غير مطابق.', 'error')
    return
  }

  savingPassword.value = true
  try {
    const response = await api.post<ApiResponse<string>>('/User/password/change', {
      currentPassword: passwordForm.value.currentPassword,
      newPassword: passwordForm.value.newPassword,
    })
    notifications.show(response.data.message)
    changePasswordOpen.value = false
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    savingPassword.value = false
  }
}

async function loadCurrentSubscription() {
  if (!auth.hasAnyRole(['DoctorUser'])) return
  try {
    const response = await api.get<ApiResponse<CurrentDoctorSubscription>>('/DoctorSubscription/my/current')
    currentSubscription.value = response.data.data
  } catch {
    currentSubscription.value = undefined
  }
}

onMounted(loadCurrentSubscription)
</script>

<template>
  <div class="admin-shell" :class="{ 'premium-shell': hasPremiumTheme }">
    <div v-if="sidebarOpen" class="sidebar-overlay" @click="sidebarOpen = false" />
    <aside class="sidebar" :class="{ 'sidebar-open': sidebarOpen }">
      <div class="brand">
        <span class="brand-icon"><HeartPulse :size="25" /></span>
        <div>
          <strong>عيادتي</strong>
          <small>لوحة التحكم الطبية</small>
        </div>
        <button class="mobile-close" type="button" aria-label="إغلاق القائمة" @click="sidebarOpen = false">
          <X :size="20" />
        </button>
      </div>

      <nav class="side-nav" aria-label="القائمة الرئيسية">
        <RouterLink
          v-for="link in links"
          :key="link.to"
          :to="link.to"
          :class="{ 'nav-active': isLinkActive(link.to) }"
          @click="sidebarOpen = false"
        >
          <component :is="link.icon" :size="19" />
          <span>{{ link.label }}</span>
          <ChevronLeft class="nav-arrow" :size="15" />
        </RouterLink>
      </nav>
    </aside>

    <main class="main-area">
      <header class="topbar">
        <div class="topbar-title">
          <button class="menu-button" type="button" aria-label="فتح القائمة" @click="sidebarOpen = true">
            <Menu :size="21" />
          </button>
          <div>
            <span>مرحباً بعودتك</span>
            <h1>{{ pageTitle }}</h1>
          </div>
        </div>
        <div class="topbar-actions">
          <div v-if="accountMenuOpen" class="account-menu-overlay" @click="accountMenuOpen = false" />
          <div class="account-menu-wrap">
            <button
              class="profile-chip profile-button"
              type="button"
              aria-haspopup="menu"
              :aria-expanded="accountMenuOpen"
              @click="accountMenuOpen = !accountMenuOpen"
            >
              <span class="avatar"><UserRound :size="17" /></span>
              <div>
                <strong>{{ roleLabel }}</strong>
                <small>{{ auth.primaryRole }}</small>
              </div>
              <ChevronDown :size="15" />
            </button>
            <div v-if="accountMenuOpen" class="account-menu" role="menu">
              <button type="button" role="menuitem" @click="openChangePassword"><KeyRound :size="17" /> تعديل كلمة السر</button>
              <button type="button" role="menuitem" @click="signOut"><LogOut :size="17" /> تسجيل الخروج</button>
            </div>
          </div>
        </div>
      </header>

      <section class="page-content">
        <RouterView />
      </section>
      <footer class="page-footer developer-page-footer">
        <span>Powered by</span>
        <img :src="godevLogo" alt="GoDev" />
      </footer>
    </main>

    <AppModal v-if="changePasswordOpen" title="تعديل كلمة السر" @close="changePasswordOpen = false">
      <form class="modal-form" @submit.prevent="changePassword">
        <label><span>كلمة السر الحالية</span><input v-model="passwordForm.currentPassword" required type="password" autocomplete="current-password" /></label>
        <label><span>كلمة السر الجديدة</span><input v-model="passwordForm.newPassword" required type="password" minlength="6" autocomplete="new-password" /></label>
        <label><span>تأكيد كلمة السر الجديدة</span><input v-model="passwordForm.confirmPassword" required type="password" minlength="6" autocomplete="new-password" /></label>
        <div class="modal-actions">
          <button class="secondary-button" type="button" @click="changePasswordOpen = false">تراجع</button>
          <button class="compact-primary" type="submit" :disabled="savingPassword">{{ savingPassword ? 'جاري الحفظ...' : 'حفظ كلمة السر' }}</button>
        </div>
      </form>
    </AppModal>
  </div>
</template>
