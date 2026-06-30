<script setup lang="ts">
interface Props {
  modelValue?: string
  label?: string
  hint?: string
  error?: string | boolean
  placeholder?: string
  disabled?: boolean
  readonly?: boolean
  required?: boolean
  clearable?: boolean
  type?: 'date' | 'month' | 'time' | 'datetime-local'
  min?: string
  max?: string
}

withDefaults(defineProps<Props>(), {
  modelValue: '',
  label: '',
  hint: '',
  error: false,
  placeholder: '',
  disabled: false,
  readonly: false,
  required: false,
  clearable: true,
  type: 'date',
  min: '',
  max: '',
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
  change: [value: string]
  blur: []
  focus: []
}>()

const handleInput = (value: string) => {
  emit('update:modelValue', value)
  emit('change', value)
}
</script>

<template>
  <v-text-field
    :model-value="modelValue"
    :label="label"
    :hint="hint"
    :error="!!error"
    :error-messages="typeof error === 'string' ? error : ''"
    :placeholder="placeholder"
    :disabled="disabled"
    :readonly="readonly"
    :required="required"
    :clearable="clearable"
    :type="type"
    :min="min"
    :max="max"
    variant="outlined"
    density="comfortable"
    class="date-input"
    @update:model-value="handleInput"
    @blur="emit('blur')"
    @focus="emit('focus')"
  />
</template>

<style scoped>
:deep(.v-text-field.date-input .v-field) {
  border-color: var(--color-border);
  border-radius: var(--radius-md);
}

:deep(.v-text-field.date-input .v-field:hover) {
  border-color: var(--color-primary);
}

:deep(.v-text-field.date-input.v-input--focused .v-field) {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(19, 121, 107, 0.08);
}

:deep(.v-text-field.date-input.v-input--error .v-field) {
  border-color: var(--color-error);
}

:deep(.v-text-field.date-input .v-field__input) {
  font-size: 14px;
  color: var(--color-text);
}

:deep(.v-text-field.date-input .v-label) {
  font-size: 13px;
  font-weight: 500;
  color: var(--color-text);
}

:deep(.v-text-field.date-input .v-field__outline) {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
}

:deep(.v-text-field.date-input .v-messages) {
  font-size: 12px;
  color: var(--color-text-muted);
}

:deep(.v-text-field.date-input.v-input--error .v-messages) {
  color: var(--color-error);
}
</style>