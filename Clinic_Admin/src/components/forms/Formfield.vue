<script setup lang="ts">
interface Props {
  label?: string
  hint?: string
  error?: string | boolean
  required?: boolean
  disabled?: boolean
  density?: 'comfortable' | 'compact'
}

withDefaults(defineProps<Props>(), {
  label: '',
  hint: '',
  error: false,
  required: false,
  disabled: false,
  density: 'comfortable',
})

defineSlots<{
  default(): any
  hint(): any
}>()
</script>

<template>
  <div class="form-field" :class="{ 'form-field--error': error, disabled }">
    <!-- Label -->
    <label v-if="label" class="form-label">
      {{ label }}
      <span v-if="required" class="form-required" aria-label="required">*</span>
    </label>

    <!-- Input Slot -->
    <slot />

    <!-- Error Message -->
    <div v-if="error && typeof error === 'string'" class="form-error">
      {{ error }}
    </div>

    <!-- Hint Text -->
    <div v-else-if="hint" class="form-hint">
      <slot name="hint">
        {{ hint }}
      </slot>
    </div>
  </div>
</template>

<style scoped>
.form-field {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
  margin-bottom: var(--spacing-lg);
}

.form-field--error {
  margin-bottom: var(--spacing-lg);
}

.form-label {
  display: block;
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text);
  line-height: 1.4;
  cursor: pointer;
}

.form-required {
  color: var(--color-error);
  margin-right: 2px;
}

[dir='rtl'] .form-required {
  margin-right: 0;
  margin-left: 2px;
}

.form-error {
  font-size: 12px;
  color: var(--color-error);
  line-height: 1.4;
  animation: slideInRight 0.25s ease-out;
}

.form-hint {
  font-size: 12px;
  color: var(--color-text-muted);
  line-height: 1.4;
}

.form-field.disabled {
  opacity: 0.6;
  pointer-events: none;
}

@keyframes slideInRight {
  from {
    opacity: 0;
    transform: translateX(-8px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}
</style>