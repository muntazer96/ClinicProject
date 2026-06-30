<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import api from '../../services/api'
import type { ApiResponse, DoctorNotificationItem } from '../../types/api'

interface Props {
  title?: string
  showMenuToggle?: boolean
  showNotifications?: boolean
  showUserMenu?: boolean
}

withDefaults(defineProps<Props>(), {
  title: '',
  showMenuToggle: true,
  showNotifications: true,
  showUserMenu: true,
})

defineEmits<{
  'menu-toggle': []
  'settings-click': []
}>()

const auth   = useAuthStore()
const router = useRouter()

const userMenuOpen  = ref(false)
const notifMenuOpen = ref(false)
const notifItems    = ref<DoctorNotificationItem[]>([])
const notifLoading  = ref(false)

const roleLabel = computed(() =>
  auth.primaryRole === 'SuperAdmin' ? 'مدير النظام' : 'حساب الطبيب'
)

const unreadCount = computed(() => notifItems.value.filter((n) => !n.readAt).length)

function formatDate(value: string) {
  return new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value))
}

async function loadNotifications() {
  // الإشعارات فقط للطبيب
  if (!auth.hasAnyRole(['DoctorUser'])) return
  notifLoading.value = true
  try {
    const r = await api.get<ApiResponse<DoctorNotificationItem[]>>('/Notification/doctor/my')
    notifItems.value = r.data.data
  } catch (e: any) {
    if (e.response?.status === 404) notifItems.value = []
  } finally {
    notifLoading.value = false
  }
}

async function markAsRead(item: DoctorNotificationItem) {
  if (item.readAt) return
  try {
    await api.post<ApiResponse<object>>(`/Notification/doctor/my/${item.id}/read`)
    await loadNotifications()
  } catch { /* silent */ }
}

async function markAllRead() {
  const unread = notifItems.value.filter((n) => !n.readAt)
  await Promise.all(unread.map((n) => markAsRead(n)))
}

function handleLogout() {
  userMenuOpen.value = false
  auth.logout()
  router.push('/login')
}

function handleSettings() {
  userMenuOpen.value = false
  router.push('/profile')
}

onMounted(loadNotifications)
</script>

<template>
  <v-app-bar elevation="1" color="surface" class="app-bar">
    <template #prepend>
      <v-btn
        v-if="showMenuToggle"
        icon variant="text" size="large"
        @click="$emit('menu-toggle')"
      >
        <v-icon icon="mdi-menu" />
      </v-btn>
    </template>

    <v-app-bar-title v-if="title" class="app-bar-title">
      {{ title }}
    </v-app-bar-title>

    <v-spacer />

    <template #append>

      <!-- Notifications Menu -->
      <v-menu
        v-if="showNotifications"
        v-model="notifMenuOpen"
        :close-on-content-click="false"
        location="bottom end"
        max-width="380"
        @update:model-value="(v) => v && loadNotifications()"
      >
        <template #activator="{ props }">
          <v-btn icon variant="text" size="large" v-bind="props">
            <v-badge
              :content="unreadCount"
              :model-value="unreadCount > 0"
              color="error"
            >
              <v-icon icon="mdi-bell" />
            </v-badge>
          </v-btn>
        </template>

        <v-card class="notif-card">
          <!-- Header -->
          <div class="notif-header">
            <div class="notif-header-left">
              <v-icon icon="mdi-bell" color="primary" size="18" />
              <strong>الإشعارات</strong>
              <v-chip v-if="unreadCount > 0" size="x-small" color="error" variant="tonal">
                {{ unreadCount }} جديد
              </v-chip>
            </div>
            <div class="notif-header-actions">
              <v-btn
                v-if="notifLoading"
                icon size="x-small" variant="text"
                loading
              />
              <v-btn
                v-else
                icon size="x-small" variant="text" color="primary"
                @click="loadNotifications"
              >
                <v-icon icon="mdi-refresh" size="16" />
              </v-btn>
              <v-btn
                v-if="unreadCount > 0"
                variant="text" size="x-small" color="primary"
                @click="markAllRead"
              >
                قراءة الكل
              </v-btn>
            </div>
          </div>
          <v-divider />

          <!-- Loading -->
          <div v-if="notifLoading" class="notif-loading">
            <v-skeleton-loader v-for="i in 3" :key="i" type="list-item-two-line" />
          </div>

          <!-- Empty -->
          <div v-else-if="!notifItems.length" class="notif-empty">
            <v-icon icon="mdi-bell-sleep" size="40" color="var(--color-text-muted)" />
            <p>لا توجد إشعارات</p>
            <span>ستظهر هنا التنبيهات المهمة فور وصولها</span>
          </div>

          <!-- List -->
          <div v-else class="notif-list">
            <div
              v-for="item in notifItems"
              :key="item.id"
              class="notif-item"
              :class="{ 'notif-item--unread': !item.readAt }"
            >
              <!-- Unread dot -->
              <div class="notif-dot-wrap">
                <div v-if="!item.readAt" class="notif-dot" />
              </div>

              <!-- Icon -->
              <div class="notif-icon-wrap" :class="{ 'notif-icon-wrap--unread': !item.readAt }">
                <v-icon icon="mdi-bell" size="16" :color="!item.readAt ? 'primary' : 'var(--color-text-muted)'" />
              </div>

              <!-- Content -->
              <div class="notif-content">
                <p class="notif-msg">{{ item.message }}</p>
                <span class="notif-time">
                  <v-icon icon="mdi-clock-outline" size="11" />
                  {{ formatDate(item.createdAt) }}
                </span>
              </div>

              <!-- Mark Read -->
              <v-btn
                v-if="!item.readAt"
                icon size="x-small" variant="text" color="primary"
                @click="markAsRead(item)"
              >
                <v-icon icon="mdi-check-all" size="16" />
              </v-btn>
            </div>
          </div>

          <!-- Footer -->
          <v-divider />
          <div class="notif-footer">
            <v-btn
              variant="text"
              size="small"
              color="primary"
              block
              @click="router.push('/notifications'); notifMenuOpen = false"
            >
              عرض جميع الإشعارات
              <v-icon icon="mdi-arrow-left" size="16" end />
            </v-btn>
          </div>
        </v-card>
      </v-menu>

      <!-- User Menu -->
      <v-menu
        v-if="showUserMenu"
        v-model="userMenuOpen"
        :close-on-content-click="true"
        location="bottom end"
      >
        <template #activator="{ props }">
          <v-btn icon variant="text" size="large" v-bind="props" class="user-button">
            <v-avatar color="primary" size="32">
              <span class="text-white font-weight-bold">
                {{ auth.primaryRole?.[0] || 'U' }}
              </span>
            </v-avatar>
          </v-btn>
        </template>

        <v-list class="user-menu">
          <v-list-item disabled class="user-info">
            <template #prepend>
              <v-avatar color="primary" size="40">
                <span class="text-white font-weight-bold text-h6">
                  {{ auth.primaryRole?.[0] || 'U' }}
                </span>
              </v-avatar>
            </template>
            <v-list-item-title class="user-role">{{ roleLabel }}</v-list-item-title>
            <v-list-item-subtitle>{{ auth.primaryRole }}</v-list-item-subtitle>
          </v-list-item>

          <v-divider />

          <v-list-item @click="handleSettings">
            <template #prepend>
              <v-icon icon="mdi-cog" size="18" />
            </template>
            <v-list-item-title>الإعدادات</v-list-item-title>
          </v-list-item>

          <v-divider />

          <v-list-item class="logout-item" @click="handleLogout">
            <template #prepend>
              <v-icon icon="mdi-logout" size="18" color="error" />
            </template>
            <v-list-item-title class="text-error">تسجيل الخروج</v-list-item-title>
          </v-list-item>
        </v-list>
      </v-menu>

    </template>
  </v-app-bar>
</template>

<style scoped>
:deep(.v-app-bar) {
  border-bottom: 1px solid var(--color-border);
  background-color: var(--color-surface);
}

.app-bar-title {
  color: var(--color-text);
  font-weight: 600;
  font-size: 18px;
}

.user-button { margin-left: var(--spacing-md); }

/* Notification Card */
.notif-card {
  border: 1px solid var(--color-border) !important;
  border-radius: var(--radius-lg) !important;
  overflow: hidden;
}

.notif-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  background: var(--color-surface);
}

.notif-header-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.notif-header-left strong {
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text);
}

.notif-header-actions {
  display: flex;
  align-items: center;
  gap: 4px;
}

/* Loading */
.notif-loading { padding: 8px; }

/* Empty */
.notif-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  padding: 32px 16px;
  text-align: center;
}

.notif-empty p {
  margin: 0;
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text);
}

.notif-empty span {
  font-size: 12px;
  color: var(--color-text-muted);
  line-height: 1.5;
}

/* List */
.notif-list {
  max-height: 340px;
  overflow-y: auto;
}

.notif-list::-webkit-scrollbar { width: 4px; }
.notif-list::-webkit-scrollbar-thumb { background: var(--color-border); border-radius: 4px; }

/* Item */
.notif-item {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border-light);
  transition: background 0.15s;
}

.notif-item:last-child { border-bottom: none; }
.notif-item:hover { background: var(--color-background); }

.notif-item--unread {
  background: var(--color-primary-soft);
}

.notif-item--unread:hover {
  background: var(--color-primary-soft);
  filter: brightness(0.97);
}

/* Dot */
.notif-dot-wrap {
  width: 8px;
  display: flex;
  align-items: center;
  padding-top: 6px;
  flex-shrink: 0;
}

.notif-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: var(--color-primary);
}

/* Icon */
.notif-icon-wrap {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: var(--radius-md);
  background: var(--color-background);
  flex-shrink: 0;
}

.notif-icon-wrap--unread {
  background: white;
}

/* Content */
.notif-content {
  flex: 1;
  min-width: 0;
}

.notif-msg {
  margin: 0 0 4px 0;
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text);
  line-height: 1.5;
  word-break: break-word;
}

.notif-time {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  font-size: 11px;
  color: var(--color-text-muted);
}

/* Footer */
.notif-footer {
  padding: 8px;
  background: var(--color-surface-variant);
}

/* User Menu */
:deep(.user-menu) {
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  min-width: 200px;
}

:deep(.user-info) {
  padding: var(--spacing-md);
  background-color: var(--color-surface-variant);
}

.user-role {
  font-weight: 600;
  color: var(--color-primary);
}

:deep(.logout-item:hover) {
  background-color: rgba(179, 60, 60, 0.08);
}
</style>
