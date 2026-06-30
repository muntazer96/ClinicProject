<script setup lang="ts">
type AlertType = 'success' | 'error' | 'warning' | 'info'

interface Props {
  modelValue?: boolean
  type?: AlertType
  title: string
  message: string
  actionLabel?: string
}

withDefaults(defineProps<Props>(), {
  modelValue: false,
  type: 'info',
  title: '',
  message: '',
  actionLabel: 'حسناً',
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  action: []
}>()

const iconMap: Record<AlertType, string> = {
  success: 'mdi-check-circle',
  error: 'mdi-alert-circle',
  warning: 'mdi-alert-outline',
  info: 'mdi-information',
}

const colorMap: Record<AlertType, string> = {
  success: 'success',
  error: 'error',
  warning: 'warning',
  info: 'info',
}

const bgColorMap: Record<AlertType, string> = {
  success: 'var(--color-success-light)',
  error: 'var(--color-error-light)',
  warning: 'var(--color-warning-light)',
  info: 'var(--color-info-light)',
}

const handleAction = (): void => {
  emit('action')
  emit('update:modelValue', false)
}
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    max-width="400px"
    persistent
    class="alert-dialog"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card class="alert-dialog-card">
      <!-- Icon -->
      <div class="alert-icon" :style="{ backgroundColor: bgColorMap[type] }">
        <v-icon :icon="iconMap[type]" :color="colorMap[type]" size="36" />
      </div>

      <!-- Title -->
      <v-card-title class="alert-title">
        {{ title }}
      </v-card-title>

      <!-- Message -->
      <v-card-text class="alert-message">
        {{ message }}
      </v-card-text>

      <!-- Actions -->
      <v-card-actions class="alert-actions">
        <v-spacer />
        <v-btn
          variant="elevated"
          :color="colorMap[type]"
          @click="handleAction"
        >
          {{ actionLabel }}
        </v-btn>
        <v-spacer />
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.alert-dialog-card {
  border-radius: var(--radius-lg);
  text-align: center;
  overflow: hidden;
}

.alert-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100px;
  border-radius: 0;
}

.alert-title {
  font-size: 18px;
  font-weight: 700;
  color: var(--color-text);
  padding: var(--spacing-xl) var(--spacing-xl) var(--spacing-md);
  text-align: center;
  line-height: 1.3;
}

.alert-message {
  padding: 0 var(--spacing-xl) var(--spacing-xl);
  font-size: 14px;
  color: var(--color-text-muted);
  line-height: 1.6;
  text-align: center;
}

.alert-actions {
  padding: var(--spacing-lg) var(--spacing-xl);
  gap: var(--spacing-md);
}

:deep(.v-btn) {
  text-transform: none;
  font-weight: 600;
}

/* Responsive */
@media (max-width: 600px) {
  .alert-icon {
    height: 80px;
  }

  .alert-title {
    font-size: 16px;
    padding: var(--spacing-lg) var(--spacing-lg) var(--spacing-md);
  }

  .alert-message {
    font-size: 13px;
    padding: 0 var(--spacing-lg) var(--spacing-lg);
  }

  .alert-actions {
    padding: var(--spacing-lg);
  }

  :deep(.v-btn) {
    width: 100%;
  }
}
</style>