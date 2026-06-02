<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'
import {
  Bell, Building2, CalendarDays, ChevronLeft, ClipboardList, HeartPulse,
  House, LogOut, Menu, MessageSquareText, Stethoscope, UserRound, UsersRound, X,
} from '@lucide/vue'
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const sidebarOpen = ref(false)

const links = computed(() => [
  { label: 'الرئيسية', to: '/', icon: House, roles: ['SuperAdmin', 'DoctorUser'] },
  { label: 'المستخدمون', to: '/users', icon: UsersRound, roles: ['SuperAdmin'] },
  { label: 'الأطباء', to: '/doctors', icon: Stethoscope, roles: ['SuperAdmin'] },
  { label: 'الاشتراكات', to: '/subscriptions', icon: ClipboardList, roles: ['SuperAdmin'] },
  { label: 'عياداتي', to: '/clinics', icon: Building2, roles: ['DoctorUser'] },
  { label: 'الحجوزات', to: '/appointments', icon: CalendarDays, roles: ['DoctorUser'] },
  { label: 'الإجازات', to: '/exceptions', icon: Bell, roles: ['DoctorUser'] },
  { label: 'التقييمات', to: '/reviews', icon: MessageSquareText, roles: ['DoctorUser'] },
  { label: 'الملف الشخصي', to: '/profile', icon: UserRound, roles: ['DoctorUser'] },
].filter((link) => auth.hasAnyRole(link.roles)))

const pageTitle = computed(() => (route.meta.title as string | undefined) ?? 'لوحة التحكم')

function signOut() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <div class="admin-shell">
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
        <RouterLink v-for="link in links" :key="link.to" :to="link.to" @click="sidebarOpen = false">
          <component :is="link.icon" :size="19" />
          <span>{{ link.label }}</span>
          <ChevronLeft class="nav-arrow" :size="15" />
        </RouterLink>
      </nav>

      <div class="sidebar-footer">
        <div class="role-pill">{{ auth.primaryRole || 'مستخدم' }}</div>
        <button class="logout-button" type="button" @click="signOut">
          <LogOut :size="18" />
          تسجيل الخروج
        </button>
      </div>
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
          <div class="profile-chip">
            <span class="avatar"><UserRound :size="17" /></span>
            <div>
              <strong>{{ auth.primaryRole === 'SuperAdmin' ? 'مدير النظام' : 'حساب الطبيب' }}</strong>
              <small>{{ auth.primaryRole }}</small>
            </div>
          </div>
        </div>
      </header>

      <section class="page-content">
        <RouterView />
      </section>
    </main>
  </div>
</template>
