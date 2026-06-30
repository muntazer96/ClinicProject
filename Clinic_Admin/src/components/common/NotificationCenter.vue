<script setup lang="ts">
import { useNotificationsStore, type NotificationType } from '../../stores/notifications'

const notifications = useNotificationsStore()

// Icons for each notification type
const iconMap: Record<NotificationType, string> = {
  success: 'mdi-check-circle',
  error: 'mdi-alert-circle',
  warning: 'mdi-alert-outline',
  info: 'mdi-information',
}

// Background colors for each notification type
const bgColorMap: Record<NotificationType, string> = {
  success: 'var(--color-success-light)',
  error: 'var(--color-error-light)',
  warning: 'var(--color-warning-light)',
  info: 'var(--color-info-light)',
}

// Text colors for each notification type
const textColorMap: Record<NotificationType, string> = {
  success: 'var(--color-success)',
  error: 'var(--color-error)',
  warning: 'var(--color-warning)',
  info: 'var(--color-info)',
}
</script>

<template>
  <Teleport to="body">
    <!-- Notification Container -->
    <div class="notification-container">
      <!-- Notifications List with Transition Group -->
      <TransitionGroup name="notification-transition" tag="div">
        <div
          v-for="notification in notifications.notifications"
          :key="notification.id"
          class="notification-wrapper"
        >
          <div
            class="notification"
            :style="{
              backgroundColor: bgColorMap[notification.type],
              borderColor: textColorMap[notification.type],
            }"
          >
            <!-- Icon -->
            <v-icon
              :icon="iconMap[notification.type]"
              :color="textColorMap[notification.type]"
              size="24"
              class="notification-icon"
            />

            <!-- Content -->
            <div class="notification-content">
              <p class="notification-message">{{ notification.message }}</p>

              <!-- Action Button -->
              <v-btn
                v-if="notification.action"
                size="small"
                variant="text"
                :color="textColorMap[notification.type]"
                @click="notifications.executeAction(notification.id)"
              >
                {{ notification.action.label }}
              </v-btn>
            </div>

            <!-- Close Button -->
            <v-btn
              v-if="notification.closeable"
              icon
              size="small"
              variant="text"
              :color="textColorMap[notification.type]"
              @click="notifications.removeNotification(notification.id)"
            >
              <v-icon icon="mdi-close" size="20" />
            </v-btn>
          </div>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<style scoped>
.notification-container {
  position: fixed;
  top: var(--spacing-lg);
  right: var(--spacing-lg);
  z-index: 9999;
  max-width: 400px;
  pointer-events: none;
}

[dir='rtl'] .notification-container {
  right: auto;
  left: var(--spacing-lg);
}

.notification-wrapper {
  pointer-events: auto;
  margin-bottom: var(--spacing-md);
}

.notification {
  display: flex;
  align-items: flex-start;
  gap: var(--spacing-md);
  padding: var(--spacing-md) var(--spacing-lg);
  border-radius: var(--radius-lg);
  border: 1px solid;
  background-color: var(--color-surface);
  box-shadow: var(--shadow-lg);
  animation: slideInRight 0.3s ease-out;
}

.notification-icon {
  margin-top: 2px;
  flex-shrink: 0;
}

.notification-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.notification-message {
  margin: 0;
  padding: 0;
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text);
  line-height: 1.5;
}

:deep(.v-btn) {
  text-transform: none;
  font-weight: 600;
  font-size: 12px;
  padding: 0;
  height: auto;
}

/* Animations */
@keyframes slideInRight {
  from {
    opacity: 0;
    transform: translateX(24px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

@keyframes slideInRightRTL {
  from {
    opacity: 0;
    transform: translateX(calc(-1 * 24px));
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

[dir='rtl'] .notification {
  animation: slideInRightRTL 0.3s ease-out;
}

.notification-transition-enter-active,
.notification-transition-leave-active {
  transition: all 0.3s ease;
}

.notification-transition-enter-from {
  opacity: 0;
  transform: translateX(24px);
}

.notification-transition-enter-from[dir='rtl'],
[dir='rtl'] .notification-transition-enter-from {
  transform: translateX(calc(-1 * 24px));
}

.notification-transition-leave-to {
  opacity: 0;
  transform: translateX(24px);
}

.notification-transition-leave-to[dir='rtl'],
[dir='rtl'] .notification-transition-leave-to {
  transform: translateX(calc(-1 * 24px));
}

/* Responsive */
@media (max-width: 600px) {
  .notification-container {
    right: var(--spacing-md);
    left: var(--spacing-md);
    max-width: calc(100% - var(--spacing-xl));
    top: auto;
    bottom: var(--spacing-lg);
  }

  [dir='rtl'] .notification-container {
    right: auto;
    left: var(--spacing-md);
  }

  .notification {
    padding: var(--spacing-md);
  }

  .notification-message {
    font-size: 13px;
  }
}
</style>