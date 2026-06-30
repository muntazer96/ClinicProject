<script setup lang="ts">
import { computed } from 'vue'

interface Step {
  title: string
  description?: string
}

interface Props {
  modelValue?: boolean
  title: string
  steps: Step[]
  currentStep?: number
  loading?: boolean
  canPrevious?: boolean
  canNext?: boolean
  canSubmit?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: false,
  title: '',
  steps: () => [],
  currentStep: 1,
  loading: false,
  canPrevious: true,
  canNext: true,
  canSubmit: true,
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  'update:currentStep': [step: number]
  previous: []
  next: []
  submit: []
  cancel: []
}>()

const isFirstStep = computed(() => props.currentStep === 1)
const isLastStep = computed(() => props.currentStep === props.steps.length)
const progressPercent = computed(
  () => ((props.currentStep - 1) / Math.max(props.steps.length - 1, 1)) * 100
)

const handlePrevious = (): void => {
  if (!isFirstStep.value && props.canPrevious) {
    emit('update:currentStep', props.currentStep - 1)
    emit('previous')
  }
}

const handleNext = (): void => {
  if (!isLastStep.value && props.canNext) {
    emit('update:currentStep', props.currentStep + 1)
    emit('next')
  }
}

const handleSubmit = (): void => {
  if (props.canSubmit) {
    emit('submit')
  }
}

const handleCancel = (): void => {
  emit('update:modelValue', false)
  emit('cancel')
}
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    max-width="600px"
    persistent
    class="step-dialog"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card class="step-dialog-card">
      <!-- Header -->
      <v-card-title class="step-dialog-title">
        {{ title }}
        <v-btn
          icon
          size="small"
          variant="text"
          class="close-button"
          :disabled="loading"
          @click="handleCancel"
        >
          <v-icon icon="mdi-close" />
        </v-btn>
      </v-card-title>

      <!-- Progress Bar -->
      <div class="progress-container">
        <div class="progress-bar">
          <div class="progress-fill" :style="{ width: `${progressPercent}%` }" />
        </div>
        <div class="step-counter">
          {{ currentStep }} / {{ steps.length }}
        </div>
      </div>

      <!-- Step Indicator -->
      <div class="steps-indicator">
        <div
          v-for="(step, index) in steps"
          :key="`step-${index}`"
          class="step-item"
          :class="{ active: index + 1 === currentStep, completed: index + 1 < currentStep }"
        >
          <div class="step-number">
            <span v-if="index + 1 < currentStep" class="step-checkmark">✓</span>
            <span v-else>{{ index + 1 }}</span>
          </div>
          <div class="step-label">
            <p class="step-title">{{ step.title }}</p>
            <p v-if="step.description" class="step-description">
              {{ step.description }}
            </p>
          </div>
        </div>
      </div>

      <!-- Content -->
      <v-card-text class="step-dialog-content">
        <slot />
      </v-card-text>

      <!-- Actions -->
      <v-card-actions class="step-dialog-actions">
        <v-btn
          variant="outlined"
          :disabled="isFirstStep || loading"
          @click="handlePrevious"
        >
          السابق
        </v-btn>

        <v-spacer />

        <v-btn
          v-if="!isLastStep"
          variant="elevated"
          color="primary"
          :loading="loading"
          :disabled="loading || !canNext"
          @click="handleNext"
        >
          التالي
        </v-btn>

        <v-btn
          v-else
          variant="elevated"
          color="primary"
          :loading="loading"
          :disabled="loading || !canSubmit"
          @click="handleSubmit"
        >
          إنهاء
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.step-dialog-card {
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.step-dialog-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-xl);
  border-bottom: 1px solid var(--color-border);
  font-size: 20px;
  font-weight: 700;
  color: var(--color-text);
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

.progress-container {
  padding: var(--spacing-lg) var(--spacing-xl);
  background-color: var(--color-surface-variant);
  border-bottom: 1px solid var(--color-border);
}

.progress-bar {
  width: 100%;
  height: 6px;
  border-radius: 999px;
  background-color: var(--color-border);
  overflow: hidden;
  margin-bottom: var(--spacing-sm);
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, var(--color-primary), var(--color-primary-light));
  border-radius: 999px;
  transition: width 0.3s ease;
}

.step-counter {
  text-align: center;
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text-muted);
}

.steps-indicator {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  padding: var(--spacing-lg) var(--spacing-xl);
  background-color: var(--color-surface-variant);
}

.step-item {
  display: flex;
  gap: var(--spacing-md);
  align-items: flex-start;
  opacity: 0.5;
  transition: opacity 0.2s ease;
}

.step-item.active {
  opacity: 1;
}

.step-item.completed {
  opacity: 0.7;
}

.step-number {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 32px;
  height: 32px;
  border-radius: 50%;
  background-color: var(--color-border);
  color: var(--color-text-muted);
  font-weight: 600;
  font-size: 14px;
  flex-shrink: 0;
}

.step-item.active .step-number {
  background-color: var(--color-primary);
  color: white;
}

.step-item.completed .step-number {
  background-color: var(--color-success);
  color: white;
}

.step-checkmark {
  font-weight: bold;
}

.step-label {
  flex: 1;
}

.step-title {
  margin: 0;
  padding: 0;
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text);
  line-height: 1.3;
}

.step-description {
  margin: 2px 0 0 0;
  padding: 0;
  font-size: 12px;
  color: var(--color-text-muted);
  line-height: 1.4;
}

.step-dialog-content {
  padding: var(--spacing-xl);
  max-height: 50vh;
  overflow-y: auto;
}

.step-dialog-actions {
  padding: var(--spacing-lg) var(--spacing-xl);
  border-top: 1px solid var(--color-border);
  background-color: var(--color-surface);
  gap: var(--spacing-md);
}

:deep(.v-btn) {
  text-transform: none;
  font-weight: 600;
}

/* Responsive */
@media (max-width: 600px) {
  .step-dialog-title {
    padding: var(--spacing-lg);
    font-size: 18px;
  }

  .close-button {
    position: static;
    margin-left: auto;
  }

  [dir='rtl'] .close-button {
    margin-left: 0;
    margin-right: auto;
  }

  .steps-indicator {
    gap: var(--spacing-sm);
    padding: var(--spacing-lg);
  }

  .step-item {
    gap: var(--spacing-sm);
  }

  .step-number {
    min-width: 28px;
    height: 28px;
    font-size: 12px;
  }

  .step-title {
    font-size: 13px;
  }

  .step-description {
    font-size: 11px;
  }

  .step-dialog-content {
    padding: var(--spacing-lg);
    max-height: 40vh;
  }

  .step-dialog-actions {
    flex-direction: column;
    padding: var(--spacing-lg);
  }

  :deep(.v-btn) {
    width: 100%;
  }
}
</style>
