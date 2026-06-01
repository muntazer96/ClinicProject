<script setup lang="ts">
import { ChevronLeft, ChevronRight } from '@lucide/vue'

const props = defineProps<{ page: number; totalPages: number }>()
const emit = defineEmits<{ change: [page: number] }>()

function go(page: number) {
  if (page >= 1 && page <= props.totalPages && page !== props.page) emit('change', page)
}
</script>

<template>
  <div v-if="totalPages > 1" class="pagination">
    <button type="button" :disabled="page <= 1" @click="go(page - 1)"><ChevronRight :size="17" /></button>
    <span>صفحة {{ page }} من {{ totalPages }}</span>
    <button type="button" :disabled="page >= totalPages" @click="go(page + 1)"><ChevronLeft :size="17" /></button>
  </div>
</template>
