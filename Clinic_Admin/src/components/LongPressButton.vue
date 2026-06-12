<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from 'vue'

const props = withDefaults(defineProps<{
  buttonClass?: string
  disabled?: boolean
  holdMs?: number
  title?: string
  type?: 'button' | 'submit' | 'reset'
}>(), {
  buttonClass: '',
  disabled: false,
  holdMs: 900,
  title: '',
  type: 'button',
})

const emit = defineEmits<{
  confirm: []
}>()

const holding = ref(false)
let timer: number | undefined

const classes = computed(() => ['long-press-button', props.buttonClass, { holding: holding.value }])

function start() {
  if (props.disabled) return
  holding.value = true
  window.clearTimeout(timer)
  timer = window.setTimeout(() => {
    holding.value = false
    emit('confirm')
  }, props.holdMs)
}

function cancel() {
  holding.value = false
  window.clearTimeout(timer)
}

onBeforeUnmount(cancel)
</script>

<template>
  <button
    :type="type"
    :class="classes"
    :disabled="disabled"
    :title="title"
    @pointerdown="start"
    @pointerup="cancel"
    @pointerleave="cancel"
    @pointercancel="cancel"
    @keydown.space.prevent="start"
    @keyup.space="cancel"
    @keydown.enter.prevent="start"
    @keyup.enter="cancel"
  >
    <slot />
  </button>
</template>

<style scoped>
.long-press-button {
  position: relative;
  overflow: hidden;
}

.long-press-button::after {
  content: '';
  position: absolute;
  inset: auto 0 0 0;
  height: 3px;
  background: currentColor;
  opacity: 0;
  transform: scaleX(0);
  transform-origin: right;
}

.long-press-button.holding::after {
  opacity: .45;
  animation: hold-progress var(--hold-duration, .9s) linear forwards;
}

@keyframes hold-progress {
  to { transform: scaleX(1); }
}
</style>
