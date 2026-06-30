<script setup lang="ts">
interface Item {
  title: string
  value: any
  disabled?: boolean
}

interface Props {
  modelValue?: any
  label?: string
  hint?: string
  error?: string | boolean
  items: Item[]
  disabled?: boolean
  readonly?: boolean
  required?: boolean
  clearable?: boolean
  multiple?: boolean
  searchable?: boolean
  itemTitle?: string
  itemValue?: string
}

withDefaults(defineProps<Props>(), {
  modelValue: undefined,
  label: '',
  hint: '',
  error: false,
  disabled: false,
  readonly: false,
  required: false,
  clearable: true,
  multiple: false,
  searchable: true,
  itemTitle: 'title',
  itemValue: 'value',
})

const emit = defineEmits<{
  'update:modelValue': [value: any]
  change: [value: any]
  blur: []
  focus: []
}>()

const handleInput = (value: any) => {
  emit('update:modelValue', value)
  emit('change', value)
}

defineSlots<{
  'prepend-item'(): any
  'append-item'(): any
  item(props: { item: Item }): any
  selection(props: { item: Item }): any
}>()
</script>

<template>
  <v-select
    :model-value="modelValue"
    :label="label"
    :hint="hint"
    :error="!!error"
    :error-messages="typeof error === 'string' ? error : ''"
    :items="items"
    :disabled="disabled"
    :readonly="readonly"
    :required="required"
    :clearable="clearable"
    :multiple="multiple"
    :searchable="searchable"
    :item-title="itemTitle"
    :item-value="itemValue"
    variant="outlined"
    density="comfortable"
    class="select-input"
    @update:model-value="handleInput"
    @blur="emit('blur')"
    @focus="emit('focus')"
  >
    <template #prepend-item>
      <slot name="prepend-item" />
    </template>

    <template #append-item>
      <slot name="append-item" />
    </template>

    <template #item="{ props: itemProps, item }">
      <slot name="item" :item="item">
        <v-list-item v-bind="itemProps" />
      </slot>
    </template>

    <template #selection="{ item }">
      <slot name="selection" :item="item">
        {{ item.title }}
      </slot>
    </template>
  </v-select>
</template>

<style scoped>
:deep(.v-select.select-input .v-field) {
  border-color: var(--color-border);
  border-radius: var(--radius-md);
}

:deep(.v-select.select-input .v-field:hover) {
  border-color: var(--color-primary);
}

:deep(.v-select.select-input.v-input--focused .v-field) {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(19, 121, 107, 0.08);
}

:deep(.v-select.select-input.v-input--error .v-field) {
  border-color: var(--color-error);
}

:deep(.v-select.select-input.v-input--error.v-input--focused .v-field) {
  box-shadow: 0 0 0 3px rgba(179, 60, 60, 0.08);
}

:deep(.v-select .v-field__input) {
  font-size: 14px;
  color: var(--color-text);
}

:deep(.v-select .v-label) {
  font-size: 13px;
  font-weight: 500;
  color: var(--color-text);
}

:deep(.v-select .v-field__outline) {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
}

:deep(.v-select .v-messages) {
  font-size: 12px;
  color: var(--color-text-muted);
}

:deep(.v-select.v-input--error .v-messages) {
  color: var(--color-error);
}

:deep(.v-select-menu) {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
}

:deep(.v-list) {
  padding: var(--spacing-sm) 0;
}

:deep(.v-list-item) {
  font-size: 14px;
  color: var(--color-text);
}

:deep(.v-list-item:hover) {
  background-color: var(--color-primary-soft);
}

:deep(.v-list-item--active) {
  background-color: var(--color-primary-soft);
  color: var(--color-primary);
}
</style>