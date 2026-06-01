import { ref } from 'vue'
import { defineStore } from 'pinia'

export type NotificationType = 'success' | 'error'

export const useNotificationsStore = defineStore('notifications', () => {
  const message = ref('')
  const type = ref<NotificationType>('success')
  let timer: number | undefined

  function show(newMessage: string, newType: NotificationType = 'success') {
    message.value = newMessage
    type.value = newType
    window.clearTimeout(timer)
    timer = window.setTimeout(() => (message.value = ''), 3500)
  }

  function clear() {
    message.value = ''
  }

  return { message, type, show, clear }
})
