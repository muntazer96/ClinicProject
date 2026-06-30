<script setup lang="ts" generic="T extends Record<string, any>">
import { computed } from 'vue'

interface Props {
  modelValue?: boolean
  title: string
  subtitle?: string
  maxWidth?: string
  loading?: boolean
  steps?: number
  currentStep?: number
}

withDefaults(defineProps<Props>(), {
  modelValue: false,
  title: '',
  subtitle: '',
  maxWidth: '600px',
  loading: false,
  steps: 1,
  currentStep: 1,
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  'step-change': [step: number]
  submit: []
  cancel: []
}>()

defineSlots<{
  default(): any
  header(): any
  footer(): any
  actions(): any
}>()

const isMultiStep = computed(() => {
  const props = defineProps as any
  return (props.steps?.defaultValue ?? 1) > 1
})
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    :max-width="maxWidth"
    persistent
    class="form-dialog"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card class="form-dialog-card">
      <!-- Dialog Header -->
      <v-card-title class="form-dialog-title">
        <slot name="header">
          <div class="title-section">
            <h2>{{ title }}</h2>
            <p v-if="subtitle" class="subtitle">{{ subtitle }}</p>
          </div>
        </slot>

        <v-btn
          icon
          size="small"
          variant="text"
          class="close-button"
          aria-label="Close dialog"
          @click="emit('update:modelValue', false)"
        >
          <v-icon icon="mdi-close" />
        </v-btn>
      </v-card-title>

      <!-- Step Indicator -->
      <v-stepper
        v-if="isMultiStep"
        :model-value="currentStep"
        editable
        class="form-stepper"
        @update:model-value="emit('step-change', $event)"
      >
        <template v-for="step in steps" :key=step>
          <v-stepper-item :step="step" :complete="currentStep > step">
            Step {{ step }}
          </v-stepper-item>
        </template>
      </v-stepper>

      <!-- Dialog Content -->
      <v-card-text class="form-dialog-content">
        <slot />
      </v-card-text>

      <!-- Dialog Footer / Actions -->
      <v-card-actions class="form-dialog-actions">
        <v-spacer />

        <slot name="actions">
          <v-btn
            variant="outlined"
            color="default"
            :disabled="loading"
            @click="emit('update:modelValue', false)"
          >
            إلغاء
          </v-btn>

          <v-btn
            variant="elevated"
            color="primary"
            :loading="loading"
            :disabled="loading"
            @click="emit('submit')"
          >
            حفظ
          </v-btn>
        </slot>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
:deep(.form-dialog .v-overlay__content) {
  padding: var(--spacing-lg);
}

.form-dialog-card {
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.form-dialog-title {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--spacing-lg);
  padding: var(--spacing-xl);
  border-bottom: 1px solid var(--color-border);
  background-color: var(--color-surface);
}

.title-section h2 {
  margin: 0 0 var(--spacing-xs) 0;
  font-size: 20px;
  font-weight: 700;
  color: var(--color-text);
  line-height: 1.2;
}

.subtitle {
  margin: 0;
  font-size: 13px;
  color: var(--color-text-muted);
  line-height: 1.5;
}

.close-button {
  position: absolute;
  right: var(--spacing-lg);
  top: var(--spacing-lg);
}

[dir='rtl'] .close-button {
  right: auto;
  left: var(--spacing-lg);
}

:deep(.form-stepper) {
  padding: var(--spacing-lg);
  border-bottom: 1px solid var(--color-border);
  background-color: var(--color-surface-variant);
}

.form-dialog-content {
  padding: var(--spacing-xl);
  max-height: 60vh;
  overflow-y: auto;
}

.form-dialog-actions {
  padding: var(--spacing-lg) var(--spacing-xl);
  border-top: 1px solid var(--color-border);
  background-color: var(--color-surface);
  gap: var(--spacing-md);
}

:deep(.v-btn) {
  text-transform: none;
  font-weight: 600;
  letter-spacing: 0;
}

/* Responsive */
@media (max-width: 600px) {
  .form-dialog-title {
    flex-direction: column;
    padding: var(--spacing-lg);
  }

  .close-button {
    position: static;
  }

  .form-dialog-content {
    padding: var(--spacing-lg);
  }

  .form-dialog-actions {
    flex-direction: column-reverse;
  }

  :deep(.v-btn) {
    width: 100%;
  }
}
</style>
