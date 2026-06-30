<script setup lang="ts">
import type { Component } from 'vue'

interface Props {
  icon?: Component | string
  title: string
  description?: string
  actionLabel?: string
  loading?: boolean
  compact?: boolean
}

withDefaults(defineProps<Props>(), {
  description: '',
  actionLabel: '',
  loading: false,
  compact: false,
})

defineEmits<{
  action: []
}>()

defineSlots<{
  default(): any
  icon(): any
}>()
</script>

<template>
  <div :class="['empty-state', { compact }]">
    <!-- Icon -->
    <div v-if="!loading" class="empty-icon">
      <slot name="icon">
        <v-icon v-if="typeof icon === 'string'" :icon="icon" />
        <component v-else-if="icon" :is="icon" :size="48" />
      </slot>
    </div>

    <!-- Loading State -->
    <div v-else class="empty-loader">
      <v-progress-circular indeterminate :size="48" :width="4" color="primary" />
    </div>

    <!-- Title -->
    <h3 class="empty-title">{{ title }}</h3>

    <!-- Description -->
    <p v-if="description" class="empty-description">{{ description }}</p>

    <!-- Action Button -->
    <v-btn
      v-if="actionLabel && !loading"
      color="primary"
      variant="elevated"
      size="small"
      @click="$emit('action')"
    >
      {{ actionLabel }}
    </v-btn>

    <!-- Custom Slot -->
    <slot />
  </div>
</template>

<style scoped>
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-lg);
  padding: var(--spacing-3xl);
  min-height: 300px;
  text-align: center;
  background-color: var(--color-surface-variant);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  animation: fadeIn 0.4s ease both;
}

.empty-state.compact {
  min-height: 200px;
  padding: var(--spacing-2xl);
  gap: var(--spacing-md);
}

.empty-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 72px;
  height: 72px;
  border-radius: var(--radius-lg);
  background-color: var(--color-primary-soft);
  color: var(--color-primary);
  font-size: 36px;
}

.empty-state.compact .empty-icon {
  width: 56px;
  height: 56px;
  font-size: 28px;
}

.empty-loader {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 72px;
}

.empty-title {
  margin: 0;
  padding: 0;
  font-size: 18px;
  font-weight: 600;
  color: var(--color-text);
  line-height: 1.3;
}

.empty-state.compact .empty-title {
  font-size: 16px;
}

.empty-description {
  margin: 0;
  padding: 0;
  max-width: 380px;
  font-size: 14px;
  color: var(--color-text-muted);
  line-height: 1.6;
}

.empty-state.compact .empty-description {
  max-width: 300px;
  font-size: 13px;
}

/* Animation */
@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

/* Responsive */
@media (max-width: 600px) {
  .empty-state {
    padding: var(--spacing-2xl);
    min-height: 240px;
  }

  .empty-state.compact {
    padding: var(--spacing-lg);
  }

  .empty-icon {
    width: 64px;
    height: 64px;
    font-size: 32px;
  }

  .empty-title {
    font-size: 16px;
  }

  .empty-description {
    font-size: 13px;
  }
}
</style>