<script setup lang="ts">
import type { Component } from 'vue'

type CardVariant = 'filled' | 'outlined' | 'elevated'

interface Props {
  icon?: Component | string
  title: string
  description?: string
  actionLabel?: string
  variant?: CardVariant
  color?: string
  to?: string
  loading?: boolean
}

withDefaults(defineProps<Props>(), {
  title: '',
  description: '',
  actionLabel: '',
  variant: 'elevated',
  color: 'primary',
  to: '',
  loading: false,
})

defineEmits<{
  action: []
}>()

defineSlots<{
  icon(): any
  footer(): any
}>()

const variantMap: Record<CardVariant, { bg: string; border: string }> = {
  filled: {
    bg: 'var(--color-primary-soft)',
    border: 'var(--color-primary-soft)',
  },
  outlined: {
    bg: 'transparent',
    border: 'var(--color-border)',
  },
  elevated: {
    bg: 'var(--color-surface)',
    border: 'var(--color-border)',
  },
}
</script>

<template>
  <v-card
    :to="to"
    :class="['info-card', `info-card--${variant}`]"
    :style="{
      backgroundColor: variantMap[variant].bg,
      borderColor: variantMap[variant].border,
    }"
  >
    <!-- Header with icon and title -->
    <div class="info-header">
      <div
        class="info-icon"
        :style="{ color }"
      >
        <slot name="icon">
          <v-icon v-if="typeof icon === 'string'" :icon="icon" size="32" />
          <component v-else-if="icon" :is="icon" :size="32" />
        </slot>
      </div>

      <div class="info-title-section">
        <h3 class="info-title">{{ title }}</h3>
      </div>
    </div>

    <!-- Description -->
    <div v-if="description" class="info-description">
      {{ description }}
    </div>

    <!-- Footer content -->
    <div v-if="$slots.footer" class="info-footer">
      <slot name="footer" />
    </div>

    <!-- Action button -->
    <div v-if="actionLabel" class="info-actions">
      <v-btn
        variant="text"
        :color="color"
        size="small"
        @click.prevent="$emit('action')"
      >
        {{ actionLabel }}
        <v-icon icon="mdi-arrow-left" size="18" />
      </v-btn>
    </div>

    <!-- Loading overlay -->
    <div v-if="loading" class="info-loading">
      <v-progress-circular indeterminate :size="40" />
    </div>
  </v-card>
</template>

<style scoped>
.info-card {
  position: relative;
  border-radius: var(--radius-lg);
  border: 1px solid;
  padding: var(--spacing-lg);
  transition: all 0.2s ease;
  height: 100%;
}

.info-card--elevated {
  box-shadow: var(--shadow-sm);
}

.info-card:hover {
  box-shadow: var(--shadow-md);
  transform: translateY(-2px);
}

.info-header {
  display: flex;
  gap: var(--spacing-md);
  align-items: flex-start;
  margin-bottom: var(--spacing-md);
}

.info-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: var(--radius-md);
  background-color: rgba(19, 121, 107, 0.1);
  flex-shrink: 0;
}

.info-title-section {
  flex: 1;
}

.info-title {
  margin: 0;
  padding: 0;
  font-size: 16px;
  font-weight: 700;
  color: var(--color-text);
  line-height: 1.3;
}

.info-description {
  font-size: 14px;
  color: var(--color-text-muted);
  line-height: 1.6;
  margin-bottom: var(--spacing-md);
}

.info-footer {
  margin: var(--spacing-md) 0;
  padding: var(--spacing-md) 0;
  border-top: 1px solid var(--color-border);
  border-bottom: 1px solid var(--color-border);
  font-size: 12px;
  color: var(--color-text-muted);
}

.info-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: var(--spacing-md);
}

:deep(.v-btn) {
  text-transform: none;
  font-weight: 600;
}

:deep(.v-btn .v-icon) {
  margin-left: 4px;
}

[dir='rtl'] :deep(.v-btn .v-icon) {
  margin-left: 0;
  margin-right: 4px;
  transform: scaleX(-1);
}

.info-loading {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: rgba(255, 255, 255, 0.8);
  border-radius: var(--radius-lg);
  z-index: 10;
}

/* Responsive */
@media (max-width: 600px) {
  .info-card {
    padding: var(--spacing-md);
  }

  .info-header {
    margin-bottom: var(--spacing-sm);
  }

  .info-icon {
    width: 40px;
    height: 40px;
  }

  .info-title {
    font-size: 14px;
  }

  .info-description {
    font-size: 13px;
    margin-bottom: var(--spacing-sm);
  }

  .info-actions {
    justify-content: center;
  }
}
</style>