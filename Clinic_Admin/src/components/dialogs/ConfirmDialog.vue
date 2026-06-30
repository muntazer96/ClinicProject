<script setup lang="ts">
interface Props {
  modelValue?: boolean
  title: string
  message: string
  confirmLabel?: string
  cancelLabel?: string
  isDangerous?: boolean
  loading?: boolean
}

withDefaults(defineProps<Props>(), {
  modelValue: false,
  title: '',
  message: '',
  confirmLabel: 'تأكيد',
  cancelLabel: 'إلغاء',
  isDangerous: false,
  loading: false,
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirm: []
  cancel: []
}>()

const handleConfirm = (): void => {
  emit('confirm')
}

const handleCancel = (): void => {
  emit('update:modelValue', false)
  emit('cancel')
}
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    max-width="400px"
    persistent
    class="confirm-dialog"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card class="confirm-dialog-card">
      <!-- Icon -->
      <div class="confirm-icon" :class="{ danger: isDangerous }">
        <v-icon :icon="isDangerous ? 'mdi-alert-circle' : 'mdi-help-circle'" />
      </div>

      <!-- Title -->
      <v-card-title class="confirm-title">
        {{ title }}
      </v-card-title>

      <!-- Message -->
      <v-card-text class="confirm-message">
        {{ message }}
      </v-card-text>

      <!-- Actions -->
      <v-card-actions class="confirm-actions">
        <v-btn
          variant="outlined"
          :disabled="loading"
          @click="handleCancel"
        >
          {{ cancelLabel }}
        </v-btn>

        <v-spacer />

        <v-btn
          variant="elevated"
          :color="isDangerous ? 'error' : 'primary'"
          :loading="loading"
          :disabled="loading"
          @click="handleConfirm"
        >
          {{ confirmLabel }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.confirm-dialog-card {
  border-radius: var(--radius-lg);
  text-align: center;
}

.confirm-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 72px;
  height: 72px;
  margin: var(--spacing-xl) auto var(--spacing-lg);
  border-radius: var(--radius-lg);
  background-color: var(--color-primary-soft);
  color: var(--color-primary);
  font-size: 36px;
}

.confirm-icon.danger {
  background-color: var(--color-error-light);
  color: var(--color-error);
}

.confirm-title {
  font-size: 18px;
  font-weight: 700;
  color: var(--color-text);
  padding: 0 var(--spacing-xl) var(--spacing-md);
  text-align: center;
  line-height: 1.3;
}

.confirm-message {
  padding: 0 var(--spacing-xl) var(--spacing-xl);
  font-size: 14px;
  color: var(--color-text-muted);
  line-height: 1.6;
  text-align: center;
}

.confirm-actions {
  padding: var(--spacing-lg) var(--spacing-xl);
  gap: var(--spacing-md);
}

:deep(.v-btn) {
  text-transform: none;
  font-weight: 600;
}

/* Responsive */
@media (max-width: 600px) {
  .confirm-icon {
    width: 64px;
    height: 64px;
    font-size: 32px;
  }

  .confirm-title {
    font-size: 16px;
  }

  .confirm-message {
    font-size: 13px;
  }

  .confirm-actions {
    flex-direction: column-reverse;
  }

  :deep(.v-btn) {
    width: 100%;
  }
}
</style>