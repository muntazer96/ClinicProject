<script setup lang="ts">
import { useRoute } from 'vue-router'

export interface Breadcrumb {
  label: string
  to?: string
}

interface Props {
  title: string
  subtitle?: string
  breadcrumbs?: Breadcrumb[]
  loading?: boolean
}

withDefaults(defineProps<Props>(), {
  subtitle: '',
  breadcrumbs: () => [],
  loading: false,
})

defineSlots<{
  actions(): any
  breadcrumbs(): any
  title(): any
}>()

useRoute()
</script>

<template>
  <div class="page-header">
    <!-- Breadcrumbs -->
    <nav v-if="breadcrumbs.length > 0" class="breadcrumbs" aria-label="Breadcrumb">
      <slot name="breadcrumbs">
        <div class="breadcrumb-list">
          <RouterLink
            v-for="(crumb, index) in breadcrumbs"
            :key="`crumb-${index}`"
            :to="crumb.to || '#'"
            :class="{ active: index === breadcrumbs.length - 1 }"
          >
            {{ crumb.label }}
          </RouterLink>
        </div>
      </slot>
    </nav>

    <!-- Title Section -->
    <div class="header-content">
      <div class="title-section">
        <slot name="title">
          <div>
            <h1 class="page-title">{{ title }}</h1>
            <p v-if="subtitle" class="page-subtitle">{{ subtitle }}</p>
          </div>
        </slot>
      </div>

      <!-- Actions Slot -->
      <div v-if="$slots.actions" class="header-actions">
        <slot name="actions" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-header {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
  margin-bottom: var(--spacing-xl);
  animation: fadeInUp 0.45s ease both;
}

.breadcrumbs {
  margin-top: -var(--spacing-md);
}

.breadcrumb-list {
  display: flex;
  gap: var(--spacing-sm);
  align-items: center;
  flex-wrap: wrap;
}

.breadcrumb-list a {
  color: var(--color-text-muted);
  font-size: 13px;
  font-weight: 500;
  text-decoration: none;
  transition: color 0.2s ease;
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--radius-sm);
}

.breadcrumb-list a:hover {
  color: var(--color-primary);
  background-color: var(--color-primary-soft);
}

.breadcrumb-list a.active {
  color: var(--color-text);
  pointer-events: none;
  cursor: default;
}

.breadcrumb-list a:not(.active)::after {
  content: '';
  display: inline-block;
  width: 16px;
  height: 16px;
  margin-right: var(--spacing-sm);
  color: var(--color-text-muted);
}

[dir='rtl'] .breadcrumb-list a:not(.active)::after {
  margin-right: 0;
  margin-left: var(--spacing-sm);
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--spacing-lg);
}

.title-section {
  flex: 1;
}

.page-title {
  margin: 0;
  padding: 0;
  font-size: 28px;
  font-weight: 700;
  color: var(--color-text);
  line-height: 1.2;
}

.page-subtitle {
  margin: var(--spacing-sm) 0 0 0;
  padding: 0;
  font-size: 14px;
  color: var(--color-text-muted);
  line-height: 1.6;
}

.header-actions {
  display: flex;
  gap: var(--spacing-md);
  align-items: center;
  flex-wrap: wrap;
}

/* Responsive */
@media (max-width: 960px) {
  .page-header {
    gap: var(--spacing-md);
    margin-bottom: var(--spacing-lg);
  }

  .header-content {
    flex-direction: column;
    align-items: stretch;
  }

  .header-actions {
    width: 100%;
  }

  .page-title {
    font-size: 24px;
  }
}

@media (max-width: 600px) {
  .breadcrumbs {
    display: none;
  }

  .page-title {
    font-size: 20px;
  }

  .page-subtitle {
    font-size: 13px;
  }
}

/* Animation */
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(12px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
