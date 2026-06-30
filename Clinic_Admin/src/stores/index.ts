import { createPinia } from 'pinia'

export const pinia = createPinia()

// Export all stores
export { useAuthStore } from './auth.ts'
export { useNotificationsStore } from './notifications'
export { useLayoutStore } from './layout'