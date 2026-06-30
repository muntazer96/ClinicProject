<script setup lang="ts">
import type { Component } from 'vue'

type CardColor = 'primary' | 'success' | 'warning' | 'error' | 'info'

interface Props {
  icon?: Component | string
  label: string
  value: string | number
  change?: number
  changeLabel?: string
  color?: CardColor
  loading?: boolean
  to?: string
}

withDefaults(defineProps<Props>(), {
  label: '',
  value: '',
  change: undefined,
  changeLabel: '',
  color: 'primary',
  loading: false,
  to: '',
})

defineSlots<{
  icon(): any
  footer(): any
}>()

const colorMap: Record<CardColor, { bg: string; icon: string; text: string }> = {
  primary: {
    bg: 'var(--color-primary-soft)',
    icon: 'var(--color-primary)',
    text: 'var(--color-primary)',
  },
  success: {
    bg: 'var(--color-success-light)',
    icon: 'var(--color-success)',
    text: 'var(--color-success)',
  },
  warning: {
    bg: 'var(--color-warning-light)',
    icon: 'var(--color-warning)',
    text: 'var(--color-warning)',
  },
  error: {
    bg: 'var(--color-error-light)',
    icon: 'var(--color-error)',
    text: 'var(--color-error)',
  },
  info: {
    bg: 'var(--color-info-light)',
    icon: 'var(--color-info)',
    text: 'var(--color-info)',
  },
}

const isPositiveChange = (change?: number): boolean => {
  return change !== undefined && change > 0
}
</script>

<template>
  <v-card
    :to="to"
    class="stat-card"
    :class="{ 'stat-card--link': to }"
  >
    <!-- Content wrapper -->
    <div class="stat-content">
      <!-- Icon section -->
      <div
        class="stat-icon"
        :style="{
          backgroundColor: colorMap[color].bg,
          color: colorMap[color].icon,
        }"
      >
        <slot name="icon">
          <v-icon v-if="typeof icon === 'string'" :icon="icon" size="28" />
          <component v-else-if="icon" :is="icon" :size="28" />
          <v-icon v-else icon="mdi-chart-box" size="28" />
        </slot>
      </div>

      <!-- Text section -->
      <div class="stat-info">
        <p class="stat-label">{{ label }}</p>
        <h3 class="stat-value">{{ value }}</h3>

        <!-- Change indicator -->
        <div v-if="change !== undefined" class="stat-change" :class="{ positive: isPositiveChange(change) }">
          <v-icon
            :icon="isPositiveChange(change) ? 'mdi-arrow-up' : 'mdi-arrow-down'"
            size="16"
          />
          <span>{{ Math.abs(change) }}%</span>
          <span v-if="changeLabel" class="change-label">{{ changeLabel }}</span>
        </div>
      </div>
    </div>

    <!-- Footer slot -->
    <div v-if="$slots.footer" class="stat-footer">
      <slot name="footer" />
    </div>

    <!-- Loading overlay -->
    <div v-if="loading" class="stat-loading">
      <v-progress-circular indeterminate :size="40" />
    </div>
  </v-card>
</template>

<style scoped>
.stat-card {
  position: relative;
  border-radius: var(--radius-lg);
  border: 1px solid var(--color-border);
  padding: var(--spacing-lg);
  transition: all 0.2s ease;
  height: 100%;
}

.stat-card--link {
  cursor: pointer;
}

.stat-card--link:hover {
  border-color: var(--color-primary);
  box-shadow: var(--shadow-md);
  transform: translateY(-2px);
}

.stat-content {
  display: flex;
  gap: var(--spacing-lg);
  align-items: flex-start;
}

.stat-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 52px;
  height: 52px;
  border-radius: var(--radius-md);
  flex-shrink: 0;
}

.stat-info {
  flex: 1;
}

.stat-label {
  margin: 0;
  padding: 0;
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  line-height: 1.4;
}

.stat-value {
  margin: var(--spacing-xs) 0 0 0;
  padding: 0;
  font-size: 28px;
  font-weight: 700;
  color: var(--color-text);
  line-height: 1.1;
}

.stat-change {
  display: flex;
  align-items: center;
  gap: 4px;
  margin-top: var(--spacing-sm);
  font-size: 12px;
  font-weight: 600;
  color: var(--color-error);
}

.stat-change.positive {
  color: var(--color-success);
}

.change-label {
  opacity: 0.8;
  margin-left: 4px;
}

[dir='rtl'] .change-label {
  margin-left: 0;
  margin-right: 4px;
}

.stat-footer {
  margin-top: var(--spacing-lg);
  padding-top: var(--spacing-lg);
  border-top: 1px solid var(--color-border);
  font-size: 12px;
  color: var(--color-text-muted);
}

.stat-loading {
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
  .stat-card {
    padding: var(--spacing-md);
  }

  .stat-icon {
    width: 44px;
    height: 44px;
  }

  .stat-value {
    font-size: 24px;
  }

  .stat-label {
    font-size: 11px;
  }
}
</style>