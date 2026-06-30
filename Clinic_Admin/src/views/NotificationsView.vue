<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import api from '../services/api'
import { useNotifications } from '../composables/useNotifications'
import type { ApiResponse, DoctorNotificationItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import EmptyState from '../components/common/Emptystate.vue'

const { success: showSuccess, error: showError } = useNotifications()

const items   = ref<DoctorNotificationItem[]>([])
const loading = ref(false)

const unreadCount = computed(() => items.value.filter((i) => !i.readAt).length)

function formatDate(value: string) {
  return new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value))
}

async function loadNotifications() {
  loading.value = true
  try {
    const r = await api.get<ApiResponse<DoctorNotificationItem[]>>('/Notification/doctor/my')
    items.value = r.data.data
  } catch (e: any) {
    if (e.response?.status === 404) items.value = []
    else showError(getErrorMessage(e))
  } finally { loading.value = false }
}

async function markAsRead(item: DoctorNotificationItem) {
  if (item.readAt) return
  try {
    const r = await api.post<ApiResponse<object>>(`/Notification/doctor/my/${item.id}/read`)
    showSuccess(r.data.message)
    await loadNotifications()
  } catch (e) { showError(getErrorMessage(e)) }
}

onMounted(loadNotifications)
</script>

<template>
  <div class="notifications-page">

    <!-- Header -->
    <div class="page-top">
      <div>
        <p class="page-kicker">مركز التنبيهات</p>
        <h1 class="page-title">إشعارات الطبيب</h1>
      </div>
      <div class="page-actions">
        <v-chip v-if="unreadCount > 0" color="error" variant="tonal" size="small">
          {{ unreadCount }} غير مقروء
        </v-chip>
        <v-btn variant="outlined" color="primary" prepend-icon="mdi-refresh" :loading="loading" @click="loadNotifications">
          تحديث
        </v-btn>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="notif-list">
      <v-skeleton-loader v-for="i in 4" :key="i" type="list-item-two-line" />
    </div>

    <!-- Empty -->
    <EmptyState
      v-else-if="!items.length"
      icon="mdi-bell-off"
      title="لا توجد إشعارات"
      description="ستظهر هنا التنبيهات المهمة فور وصولها"
    />

    <!-- Notifications List -->
    <div v-else class="notif-list">
      <div
        v-for="item in items"
        :key="item.id"
        class="notif-card"
        :class="{ 'notif-card--unread': !item.readAt }"
      >
        <!-- Icon -->
        <div class="notif-icon" :class="{ 'notif-icon--unread': !item.readAt }">
          <v-icon
            icon="mdi-bell"
            :color="item.readAt ? 'var(--color-text-muted)' : 'primary'"
            size="20"
          />
        </div>

        <!-- Content -->
        <div class="notif-content">
          <p class="notif-message">{{ item.message }}</p>
          <span class="notif-time">
            <v-icon icon="mdi-clock-outline" size="12" />
            {{ formatDate(item.createdAt) }}
          </span>
        </div>

        <!-- Unread Dot -->
        <div v-if="!item.readAt" class="notif-dot" />

        <!-- Mark Read Button -->
        <v-btn
          v-if="!item.readAt"
          size="small"
          variant="tonal"
          color="primary"
          prepend-icon="mdi-check-all"
          class="notif-btn"
          @click="markAsRead(item)"
        >
          مقروء
        </v-btn>

        <!-- Read Badge -->
        <v-chip v-else size="x-small" variant="tonal" color="default" class="notif-read-chip">
          <v-icon icon="mdi-check" size="12" start />
          مقروء
        </v-chip>
      </div>
    </div>

  </div>
</template>

<style scoped>
.notifications-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

/* Page Top */
.page-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-lg);
  flex-wrap: wrap;
}
.page-kicker {
  margin: 0 0 4px 0;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-muted);
}
.page-title {
  margin: 0;
  font-size: 28px;
  font-weight: 800;
  color: var(--color-text);
}
.page-actions {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

/* Notifications List */
.notif-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

/* Notification Card */
.notif-card {
  display: flex;
  align-items: center;
  gap: var(--spacing-lg);
  padding: var(--spacing-lg);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  transition: box-shadow 0.2s ease;
}

.notif-card:hover {
  box-shadow: var(--shadow-md);
}

.notif-card--unread {
  border-color: var(--color-primary-light);
  background: var(--color-primary-soft);
}

/* Icon */
.notif-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: var(--color-background);
  flex-shrink: 0;
}

.notif-icon--unread {
  background: white;
}

/* Content */
.notif-content {
  flex: 1;
  min-width: 0;
}

.notif-message {
  margin: 0 0 6px 0;
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text);
  line-height: 1.6;
}

.notif-time {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: var(--color-text-muted);
  font-weight: 500;
}

/* Unread Dot */
.notif-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--color-primary);
  flex-shrink: 0;
}

/* Mark Read Button */
.notif-btn {
  flex-shrink: 0;
}

.notif-read-chip {
  flex-shrink: 0;
}

/* Responsive */
@media (max-width: 600px) {
  .notif-card {
    flex-wrap: wrap;
  }
  .notif-btn {
    width: 100%;
    justify-content: center;
  }
}
</style>