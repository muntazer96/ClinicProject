<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { BellRing, CheckCheck, RefreshCw } from '@lucide/vue'
import api from '../services/api'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, DoctorNotificationItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const toast = useNotificationsStore()
const items = ref<DoctorNotificationItem[]>([])
const loading = ref(false)

const unreadCount = computed(() => items.value.filter((item) => !item.readAt).length)

function formatDate(value: string) {
  return new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value))
}

async function loadNotifications() {
  loading.value = true
  try {
    const response = await api.get<ApiResponse<DoctorNotificationItem[]>>('/Notification/doctor/my')
    items.value = response.data.data
  } catch (error: any) {
    if (error.response?.status === 404) items.value = []
    else toast.show(getErrorMessage(error), 'error')
  } finally {
    loading.value = false
  }
}

async function markAsRead(item: DoctorNotificationItem) {
  if (item.readAt) return
  try {
    const response = await api.post<ApiResponse<object>>(`/Notification/doctor/my/${item.id}/read`)
    toast.show(response.data.message)
    await loadNotifications()
  } catch (error) {
    toast.show(getErrorMessage(error), 'error')
  }
}

onMounted(loadNotifications)
</script>

<template>
  <div>
    <div class="page-heading">
      <div>
        <span class="section-kicker">مركز التنبيهات</span>
        <h2>إشعارات الطبيب</h2>
        <p>راجع التنبيهات المرتبطة بالحجوزات والاشتراك وتغييرات جدول الدوام.</p>
      </div>
      <div class="heading-actions">
        <span class="records-count">{{ unreadCount }} غير مقروء</span>
        <button class="secondary-button" type="button" :disabled="loading" @click="loadNotifications"><RefreshCw :size="17" /> تحديث</button>
      </div>
    </div>

    <div v-if="loading" class="empty-panel">جارِ تحميل الإشعارات...</div>
    <div v-else-if="!items.length" class="empty-panel"><BellRing :size="31" /><h3>لا توجد إشعارات</h3><p>ستظهر هنا التنبيهات المهمة فور وصولها.</p></div>
    <section v-else class="notifications-list">
      <article v-for="item in items" :key="item.id" class="notification-card" :class="{ unread: !item.readAt }">
        <span class="notification-icon"><BellRing :size="18" /></span>
        <div>
          <p>{{ item.message }}</p>
          <small>{{ formatDate(item.createdAt) }}</small>
        </div>
        <button v-if="!item.readAt" class="secondary-button" type="button" @click="markAsRead(item)"><CheckCheck :size="16" /> مقروء</button>
      </article>
    </section>
  </div>
</template>

<style scoped>
.notifications-list { display: grid; gap: 10px; }
.notification-card { display: grid; grid-template-columns: 42px minmax(0, 1fr) auto; gap: 12px; align-items: center; padding: 14px; border: 1px solid var(--line); border-radius: 12px; background: #fff; box-shadow: var(--shadow); }
.notification-card.unread { border-color: #9acfc7; background: #f5fcfa; }
.notification-card p { margin: 0 0 5px; color: var(--ink); line-height: 1.7; font-weight: 700; }
.notification-card small { color: var(--muted); }
.notification-icon { display: grid; place-items: center; width: 42px; height: 42px; color: var(--primary); border-radius: 11px; background: var(--primary-soft); }
@media (max-width: 760px) {
  .notification-card { grid-template-columns: 42px minmax(0, 1fr); }
  .notification-card button { grid-column: 1 / -1; }
}
</style>
