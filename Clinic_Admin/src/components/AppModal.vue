<script setup lang="ts">
import { X } from '@lucide/vue'

defineProps<{ title: string; wide?: boolean }>()
defineEmits<{ close: [] }>()
</script>

<template>
  <Teleport to="body">
    <div class="modal-backdrop" @click.self="$emit('close')">
      <section class="modal-card" :class="{ 'modal-wide': wide }">
        <header>
          <h2>{{ title }}</h2>
          <button type="button" aria-label="إغلاق" @click="$emit('close')"><X :size="18" /></button>
        </header>
        <div class="modal-content"><slot /></div>
      </section>
    </div>
  </Teleport>
</template>
