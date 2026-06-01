<script setup lang="ts">
import { CircleCheck, CircleX, X } from '@lucide/vue'
import { useNotificationsStore } from '../stores/notifications'

const notifications = useNotificationsStore()
</script>

<template>
  <Transition name="toast">
    <div v-if="notifications.message" class="app-toast" :class="`toast-${notifications.type}`">
      <CircleCheck v-if="notifications.type === 'success'" :size="19" />
      <CircleX v-else :size="19" />
      <span>{{ notifications.message }}</span>
      <button type="button" aria-label="إغلاق الرسالة" @click="notifications.clear"><X :size="15" /></button>
    </div>
  </Transition>
</template>
