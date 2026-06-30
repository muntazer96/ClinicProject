import { computed, ref } from 'vue'
import { defineStore } from 'pinia'

export type NotificationType = 'success' | 'error' | 'warning' | 'info'

export interface Notification {
  id: string
  message: string
  type: NotificationType
  duration?: number
  action?: {
    label: string
    callback: () => void
  }
  closeable?: boolean
}

export const useNotificationsStore = defineStore('notifications', () => {
  // State
  const notifications = ref<Notification[]>([])
  const nextId = ref<number>(0)

  // Computed
  const hasNotifications = computed(() => notifications.value.length > 0)

  const notificationCount = computed(() => notifications.value.length)

  // Default durations for each type (in ms)
  const defaultDurations: Record<NotificationType, number> = {
    success: 3500,
    error: 5000,
    warning: 4000,
    info: 3500,
  }

  /**
   * Generate unique notification ID
   */
  const generateId = (): string => {
    return `notification-${nextId.value++}`
  }

  /**
   * Add notification to queue
   */
  const addNotification = (
    message: string,
    type: NotificationType = 'info',
    options?: {
      duration?: number
      action?: {
        label: string
        callback: () => void
      }
      closeable?: boolean
    }
  ): string => {
    const id = generateId()
    const duration = options?.duration ?? defaultDurations[type]

    const notification: Notification = {
      id,
      message,
      type,
      duration,
      action: options?.action,
      closeable: options?.closeable ?? true,
    }

    notifications.value.push(notification)

    // Auto-remove notification after duration (if duration > 0)
    if (duration > 0) {
      setTimeout(() => {
        removeNotification(id)
      }, duration)
    }

    return id
  }

  /**
   * Show success notification
   */
  const success = (
    message: string,
    options?: {
      duration?: number
      action?: {
        label: string
        callback: () => void
      }
      closeable?: boolean
    }
  ): string => {
    return addNotification(message, 'success', options)
  }

  /**
   * Show error notification
   */
  const error = (
    message: string,
    options?: {
      duration?: number
      action?: {
        label: string
        callback: () => void
      }
      closeable?: boolean
    }
  ): string => {
    return addNotification(message, 'error', options)
  }

  /**
   * Show warning notification
   */
  const warning = (
    message: string,
    options?: {
      duration?: number
      action?: {
        label: string
        callback: () => void
      }
      closeable?: boolean
    }
  ): string => {
    return addNotification(message, 'warning', options)
  }

  /**
   * Show info notification
   */
  const info = (
    message: string,
    options?: {
      duration?: number
      action?: {
        label: string
        callback: () => void
      }
      closeable?: boolean
    }
  ): string => {
    return addNotification(message, 'info', options)
  }

  /**
   * Remove notification by ID
   */
  const removeNotification = (id: string): void => {
    const index = notifications.value.findIndex((n) => n.id === id)
    if (index !== -1) {
      notifications.value.splice(index, 1)
    }
  }

  /**
   * Clear all notifications
   */
  const clearAll = (): void => {
    notifications.value = []
  }

  /**
   * Clear notifications by type
   */
  const clearByType = (type: NotificationType): void => {
    notifications.value = notifications.value.filter((n) => n.type !== type)
  }

  /**
   * Execute notification action and close
   */
  const executeAction = (id: string): void => {
    const notification = notifications.value.find((n) => n.id === id)
    if (notification?.action) {
      notification.action.callback()
      removeNotification(id)
    }
  }

  return {
    // State
    notifications,

    // Computed
    hasNotifications,
    notificationCount,

    // Methods
    addNotification,
    success,
    error,
    warning,
    info,
    removeNotification,
    clearAll,
    clearByType,
    executeAction,
  }
})