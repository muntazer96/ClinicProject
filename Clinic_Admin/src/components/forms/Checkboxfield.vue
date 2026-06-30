<script setup lang="ts">
interface Props {
  modelValue?: boolean
  label?: string
  hint?: string
  error?: string | boolean
  disabled?: boolean
  required?: boolean
}

withDefaults(defineProps<Props>(), {
  modelValue: false,
  label: '',
  hint: '',
  error: false,
  disabled: false,
  required: false,
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  change: [value: boolean]
}>()

const handleChange = (event: Event): void => {
  const value = (event.target as HTMLInputElement).checked
  emit('update:modelValue', value)
  emit('change', value)
}
</script>

<template>
  <div class="checkbox-field">
    <label class="checkbox-label" :class="{ 'checkbox-disabled': disabled }">
      <input
        type="checkbox"
        :checked="modelValue"
        :disabled="disabled"
        class="checkbox-native"
        @change="handleChange"
      />
      <span class="checkbox-box">
        <v-icon v-if="modelValue" icon="mdi-check" size="14" class="check-icon" />
      </span>
      <span v-if="label" class="checkbox-text">{{ label }}</span>
    </label>
    <p v-if="error && typeof error === 'string'" class="checkbox-error">{{ error }}</p>
    <p v-else-if="hint" class="checkbox-hint">{{ hint }}</p>
  </div>
</template>

<style scoped>
.checkbox-field {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  user-select: none;
}

.checkbox-disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.checkbox-native {
  position: absolute;
  opacity: 0;
  width: 0;
  height: 0;
}

.checkbox-box {
  width: 18px;
  height: 18px;
  border: 2px solid var(--color-border);
  border-radius: 4px;
  background: white;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: all 0.15s ease;
}

.checkbox-label:hover .checkbox-box {
  border-color: var(--color-primary);
}

.checkbox-native:checked ~ .checkbox-box {
  background: var(--color-primary);
  border-color: var(--color-primary);
}

.check-icon {
  color: white;
}

.checkbox-text {
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text);
}

.checkbox-error {
  margin: 0;
  padding: 0;
  font-size: 12px;
  color: var(--color-error);
}

.checkbox-hint {
  margin: 0;
  padding: 0;
  font-size: 12px;
  color: var(--color-text-muted);
}
</style>