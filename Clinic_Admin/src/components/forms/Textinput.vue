<script setup lang="ts">
import { computed, ref } from 'vue'

interface Props {
  modelValue?: string
  label?: string
  hint?: string
  error?: string | boolean
  type?: 'text' | 'email' | 'password' | 'tel' | 'url' | 'number'
  placeholder?: string
  dir?: 'rtl' | 'ltr' | 'auto'
  inputmode?: 'none' | 'text' | 'tel' | 'url' | 'email' | 'numeric' | 'decimal' | 'search'
  autocomplete?: string
  disabled?: boolean
  readonly?: boolean
  required?: boolean
  clearable?: boolean
  counter?: number
  prefix?: string
  suffix?: string
  icon?: string
  rules?: ((v: string) => boolean | string)[]
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '',
  label: '',
  hint: '',
  error: false,
  type: 'text',
  placeholder: '',
  dir: 'rtl',
  inputmode: undefined,
  autocomplete: undefined,
  disabled: false,
  readonly: false,
  required: false,
  clearable: false,
  counter: undefined,
  prefix: '',
  suffix: '',
  icon: '',
  rules: () => [],
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
  blur: []
  focus: []
  change: [value: string]
}>()

const showPassword = ref(false)

const inputType = computed(() => {
  if (props.type === 'password' && showPassword.value) return 'text'
  return props.type
})

const handleInput = (value: string) => {
  emit('update:modelValue', value)
  emit('change', value)
}

const togglePassword = () => {
  showPassword.value = !showPassword.value
}
</script>

<template>
  <div class="text-input-wrapper">
    <label v-if="label" class="input-label">
      {{ label }}
      <span v-if="required" class="required-star">*</span>
    </label>

    <div class="input-container" :class="{ 'input-error': !!error, 'input-disabled': disabled }">
      <!-- Leading Icon -->
      <span v-if="icon" class="input-icon-leading">
        <v-icon :icon="icon" size="18" />
      </span>

      <!-- Input -->
      <input
        :value="modelValue"
        :type="inputType"
        :placeholder="placeholder"
        :dir="dir"
        :inputmode="inputmode"
        :autocomplete="autocomplete"
        :disabled="disabled"
        :readonly="readonly"
        :required="required"
        class="input-field"
        :class="{ 'has-leading-icon': !!icon, 'has-trailing-icon': type === 'password' }"
        @input="handleInput(($event.target as HTMLInputElement).value)"
        @focus="emit('focus')"
        @blur="emit('blur')"
      />

      <!-- Password Toggle -->
      <button
        v-if="type === 'password'"
        type="button"
        class="input-icon-trailing"
        @click="togglePassword"
      >
        <v-icon :icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'" size="18" />
      </button>
    </div>

    <!-- Error Message -->
    <p v-if="error && typeof error === 'string'" class="input-error-msg">{{ error }}</p>
    <!-- Hint -->
    <p v-else-if="hint" class="input-hint">{{ hint }}</p>
  </div>
</template>

<style scoped>
.text-input-wrapper {
  display: flex;
  flex-direction: column;
  gap: 6px;
  width: 100%;
}

.input-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text);
  display: flex;
  align-items: center;
  gap: 4px;
}

.required-star {
  color: var(--color-error);
}

.input-container {
  position: relative;
  display: flex;
  align-items: center;
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: white;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.input-container:focus-within {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(19, 121, 107, 0.08);
}

.input-container.input-error {
  border-color: var(--color-error);
}

.input-container.input-error:focus-within {
  box-shadow: 0 0 0 3px rgba(179, 60, 60, 0.08);
}

.input-container.input-disabled {
  background: var(--color-background);
  opacity: 0.6;
  cursor: not-allowed;
}

.input-icon-leading {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 10px 0 12px;
  color: var(--color-text-muted);
  flex-shrink: 0;
  pointer-events: none;
}

/* RTL: leading icon is on the right */
[dir='rtl'] .input-icon-leading {
  padding: 0 12px 0 10px;
}

.input-container:focus-within .input-icon-leading {
  color: var(--color-primary);
}

.input-field {
  flex: 1;
  height: 42px;
  border: none;
  outline: none;
  background: transparent;
  font-size: 14px;
  color: var(--color-text);
  font-family: var(--font-family-primary);
  padding: 0 12px;
  min-width: 0;
  text-align: start;
}

.input-field.has-leading-icon {
  padding-right: 0;
}

[dir='rtl'] .input-field.has-leading-icon {
  padding-left: 0;
  padding-right: 12px;
}

.input-field.has-trailing-icon {
  padding-left: 0;
}

[dir='rtl'] .input-field.has-trailing-icon {
  padding-right: 0;
  padding-left: 12px;
}

.input-field::placeholder {
  color: var(--color-text-muted);
  opacity: 0.7;
}

.input-field:disabled {
  cursor: not-allowed;
}

.input-icon-trailing {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 42px;
  border: none;
  background: transparent;
  color: var(--color-text-muted);
  cursor: pointer;
  flex-shrink: 0;
  transition: color 0.2s ease;
}

.input-icon-trailing:hover {
  color: var(--color-primary);
}

.input-error-msg {
  margin: 0;
  padding: 0;
  font-size: 12px;
  color: var(--color-error);
  font-weight: 500;
}

.input-hint {
  margin: 0;
  padding: 0;
  font-size: 12px;
  color: var(--color-text-muted);
}
</style>
