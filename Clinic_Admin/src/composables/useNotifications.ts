import { useNotificationsStore, type NotificationType } from '../stores/notifications'

/**
 * Composable for using notifications throughout the app
 * 
 * @example
 * const { notify, success, error, warning, info } = useNotifications()
 * 
 * success('Operation completed successfully!')
 * error('An error occurred', { duration: 5000 })
 * notify('Custom message', 'warning', { closeable: false })
 */
export function useNotifications() {
  const store = useNotificationsStore()

  return {
    /**
     * Show a generic notification
     */
    notify: (
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
    ) => store.addNotification(message, type, options),

    /**
     * Show success notification
     */
    success: (
      message: string,
      options?: {
        duration?: number
        action?: {
          label: string
          callback: () => void
        }
        closeable?: boolean
      }
    ) => store.success(message, options),

    /**
     * Show error notification
     */
    error: (
      message: string,
      options?: {
        duration?: number
        action?: {
          label: string
          callback: () => void
        }
        closeable?: boolean
      }
    ) => store.error(message, options),

    /**
     * Show warning notification
     */
    warning: (
      message: string,
      options?: {
        duration?: number
        action?: {
          label: string
          callback: () => void
        }
        closeable?: boolean
      }
    ) => store.warning(message, options),

    /**
     * Show info notification
     */
    info: (
      message: string,
      options?: {
        duration?: number
        action?: {
          label: string
          callback: () => void
        }
        closeable?: boolean
      }
    ) => store.info(message, options),

    /**
     * Clear all notifications
     */
    clear: () => store.clearAll(),

    /**
     * Clear notifications by type
     */
    clearByType: (type: NotificationType) => store.clearByType(type),

    /**
     * Get all notifications (reactive)
     */
    notifications: store.notifications,

    /**
     * Check if there are notifications
     */
    hasNotifications: store.hasNotifications,

    /**
     * Get notification count
     */
    notificationCount: store.notificationCount,
  }
}