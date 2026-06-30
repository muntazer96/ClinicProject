<script setup lang="ts">
interface Props {
  type?: 'card' | 'table' | 'list' | 'custom'
  count?: number
  dense?: boolean
}

withDefaults(defineProps<Props>(), {
  type: 'card',
  count: 3,
  dense: false,
})

defineSlots<{
  default(): any
}>()
</script>

<template>
  <!-- Card Skeleton -->
  <div v-if="type === 'card' && !$slots.default" class="skeleton-container">
    <div v-for="i in count" :key=i class="skeleton-card">
      <div class="skeleton-header">
        <div class="skeleton skeleton-avatar" />
        <div class="skeleton-texts">
          <div class="skeleton skeleton-title" />
          <div class="skeleton skeleton-subtitle" />
        </div>
      </div>
      <div class="skeleton skeleton-content" />
      <div class="skeleton skeleton-footer" />
    </div>
  </div>

  <!-- Table Skeleton -->
  <div v-else-if="type === 'table'" class="skeleton-table">
    <div class="skeleton-table-header">
      <div class="skeleton skeleton-col" />
      <div class="skeleton skeleton-col" />
      <div class="skeleton skeleton-col" />
      <div class="skeleton skeleton-col-small" />
    </div>
    <div v-for="i in count" :key=i class="skeleton-table-row">
      <div class="skeleton skeleton-col" />
      <div class="skeleton skeleton-col" />
      <div class="skeleton skeleton-col" />
      <div class="skeleton skeleton-col-small" />
    </div>
  </div>

  <!-- List Skeleton -->
  <div v-else-if="type === 'list'" class="skeleton-list">
    <div v-for="i in count" :key=i class="skeleton-item">
      <div class="skeleton skeleton-avatar-sm" />
      <div class="skeleton-texts">
        <div class="skeleton skeleton-title-sm" />
        <div class="skeleton skeleton-subtitle-sm" />
      </div>
    </div>
  </div>

  <!-- Custom Slot -->
  <div v-else class="skeleton-container">
    <slot />
  </div>
</template>

<style scoped>
/* Skeleton Base Style -->
.skeleton {
  background: linear-gradient(
    90deg,
    var(--color-border) 25%,
    var(--color-border-light) 50%,
    var(--color-border) 75%
  );
  background-size: 200% 100%;
  animation: shimmer 2s infinite;
  border-radius: var(--radius-md);
}

@keyframes shimmer {
  0% {
    background-position: 200% 0;
  }
  100% {
    background-position: -200% 0;
  }
}

/* Card Skeleton */
.skeleton-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: var(--spacing-lg);
}

.skeleton-card {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  padding: var(--spacing-lg);
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
}

.skeleton-header {
  display: flex;
  gap: var(--spacing-md);
  align-items: flex-start;
}

.skeleton-avatar {
  width: 48px;
  height: 48px;
  min-width: 48px;
}

.skeleton-texts {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.skeleton-title {
  height: 16px;
  width: 80%;
}

.skeleton-subtitle {
  height: 12px;
  width: 60%;
}

.skeleton-content {
  height: 80px;
  width: 100%;
}

.skeleton-footer {
  height: 40px;
  width: 100%;
}

/* Table Skeleton */
.skeleton-table {
  width: 100%;
  padding: var(--spacing-lg);
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
}

.skeleton-table-header,
.skeleton-table-row {
  display: grid;
  grid-template-columns: 1.5fr 1fr 1fr 100px;
  gap: var(--spacing-lg);
  margin-bottom: var(--spacing-lg);
  align-items: center;
}

.skeleton-table-row:last-child {
  margin-bottom: 0;
}

.skeleton-col {
  height: 20px;
}

.skeleton-col-small {
  height: 18px;
}

/* List Skeleton */
.skeleton-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.skeleton-item {
  display: flex;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  align-items: center;
}

.skeleton-avatar-sm {
  width: 40px;
  height: 40px;
  min-width: 40px;
}

.skeleton-title-sm {
  height: 14px;
  width: 70%;
}

.skeleton-subtitle-sm {
  height: 12px;
  width: 50%;
  margin-top: var(--spacing-xs);
}

/* Responsive */
@media (max-width: 960px) {
  .skeleton-container {
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  }
}

@media (max-width: 600px) {
  .skeleton-container {
    grid-template-columns: 1fr;
  }

  .skeleton-table-header,
  .skeleton-table-row {
    grid-template-columns: 1fr;
    gap: var(--spacing-md);
  }
}
</style>