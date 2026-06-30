<script setup lang="ts" generic="T extends Record<string, any>">
import { computed } from 'vue'

export interface TableHeader {
  key: string
  title: string
  sortable?: boolean
  width?: string | number
  align?: 'start' | 'center' | 'end'
}

interface PaginationConfig {
  page: number
  pageSize: number
  total: number
}

interface Props {
  items: T[]
  headers: TableHeader[]
  loading?: boolean
  pagination?: PaginationConfig
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
  selectable?: boolean
  selectedItems?: T[]
  noDataText?: string
  dense?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  items: () => [],
  headers: () => [],
  loading: false,
  pagination: undefined,
  sortBy: '',
  sortOrder: 'asc',
  selectable: false,
  selectedItems: () => [],
  noDataText: 'لا توجد بيانات',
  dense: false,
})

const emit = defineEmits<{
  'update:sortBy': [key: string]
  'update:sortOrder': [order: 'asc' | 'desc']
  'update:selectedItems': [items: T[]]
  'page-change': [page: number]
  'row-click': [item: T, index: number]
}>()

defineSlots<{
  'item.data-table': (props: { item: T; index: number }) => any
  actions(props: { item: T; index: number }): any
}>()

// Computed property for all selected state
const allSelected = computed({
  get: (): boolean | null => {
    if (props.selectedItems && props.selectedItems.length === props.items.length && props.items.length > 0) {
      return true
    }
    return false
  },
  set: (newVal: boolean | null): void => {
    // Convert null (indeterminate) to false
    const boolValue = newVal === null ? false : newVal
    if (boolValue) {
      emit('update:selectedItems', [...props.items])
    } else {
      emit('update:selectedItems', [])
    }
  },
})

// Check if a specific item is selected
const isItemSelected = (item: T): boolean => {
  if (!props.selectedItems) return false
  return props.selectedItems.some((selected: T) => selected === item)
}

// Toggle selection for a specific item
const toggleItemSelection = (item: T): void => {
  const currentSelected = props.selectedItems ?? []
  if (isItemSelected(item)) {
    emit(
      'update:selectedItems',
      currentSelected.filter((selected: T) => selected !== item)
    )
  } else {
    emit('update:selectedItems', [...currentSelected, item])
  }
}

// Handle sort when header is clicked
const handleSort = (key: string): void => {
  if (props.sortBy === key) {
    emit('update:sortOrder', props.sortOrder === 'asc' ? 'desc' : 'asc')
  } else {
    emit('update:sortBy', key)
    emit('update:sortOrder', 'asc')
  }
}

// Handle row click
const handleRowClick = (item: T, index: number): void => {
  emit('row-click', item, index)
}

// Handle page change
const handlePageChange = (page: number): void => {
  emit('page-change', page)
}
</script>

<template>
  <div class="data-table-wrapper">
    <!-- Table -->
    <v-table
      :density="dense ? 'compact' : 'default'"
      class="data-table"
      :class="{ loading }"
    >
      <thead>
        <tr class="table-header-row">
          <!-- Checkbox for selection -->
          <th v-if="selectable" class="table-header-cell checkbox-cell">
            <v-checkbox
              :model-value="allSelected"
              @update:model-value="allSelected = $event"
              color="primary"
              hide-details
            />
          </th>

          <!-- Regular headers -->
          <th
            v-for="header in headers"
            :key="header.key"
            class="table-header-cell"
            :style="{ width: header.width }"
            :class="{ sortable: header.sortable }"
            @click="header.sortable && handleSort(header.key)"
          >
            <div class="header-content">
              <span>{{ header.title }}</span>
              <v-icon
                v-if="header.sortable && sortBy === header.key"
                :icon="sortOrder === 'asc' ? 'mdi-arrow-up' : 'mdi-arrow-down'"
                size="18"
              />
              <v-icon v-else-if="header.sortable" icon="mdi-unfold-more-vertical" size="18" />
            </div>
          </th>

          <!-- Actions column -->
          <th v-if="$slots.actions" class="table-header-cell actions-cell">
            الإجراءات
          </th>
        </tr>
      </thead>

      <tbody>
        <!-- Data rows -->
        <tr
          v-for="(item, index) in items"
          :key="`row-${index}`"
          class="table-data-row"
          @click="handleRowClick(item, index)"
        >
          <!-- Selection checkbox -->
          <td v-if="selectable" class="table-data-cell checkbox-cell">
            <v-checkbox
              :model-value="isItemSelected(item)"
              @update:model-value="toggleItemSelection(item)"
              @click.stop
              color="primary"
              hide-details
            />
          </td>

          <!-- Data cells -->
          <td
            v-for="header in headers"
            :key="`cell-${index}-${header.key}`"
            class="table-data-cell"
            :style="{ width: header.width }"
          >
            <slot :name="`item.data-table`" :item="item" :index="index">
              {{ item[header.key] }}
            </slot>
          </td>

          <!-- Actions cell -->
          <td v-if="$slots.actions" class="table-data-cell actions-cell">
            <slot name="actions" :item="item" :index="index" />
          </td>
        </tr>

        <!-- Empty state -->
        <tr v-if="!loading && items.length === 0" class="table-empty-row">
          <td :colspan="headers.length + (selectable ? 1 : 0) + ($slots.actions ? 1 : 0)">
            <div class="table-empty-message">
              {{ noDataText }}
            </div>
          </td>
        </tr>

        <!-- Loading state -->
        <tr v-if="loading" class="table-loading-row">
          <td :colspan="headers.length + (selectable ? 1 : 0) + ($slots.actions ? 1 : 0)">
            <div class="table-loading">
              <v-progress-circular indeterminate :size="32" color="primary" />
            </div>
          </td>
        </tr>
      </tbody>
    </v-table>

    <!-- Pagination -->
    <div v-if="pagination" class="table-pagination">
      <div class="pagination-info">
        يعرض {{ (pagination.page - 1) * pagination.pageSize + 1 }} إلى
        {{ Math.min(pagination.page * pagination.pageSize, pagination.total) }} من
        {{ pagination.total }}
      </div>

      <v-pagination
        :model-value="pagination.page"
        :length="Math.ceil(pagination.total / pagination.pageSize)"
        @update:model-value="handlePageChange"
      />
    </div>
  </div>
</template>

<style scoped>
.data-table-wrapper {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  overflow: hidden;
}

:deep(.v-table) {
  background-color: var(--color-surface);
}

:deep(.v-table.data-table) {
  font-family: var(--font-family-primary);
}

.table-header-row {
  background-color: var(--color-surface-variant);
  border-bottom: 2px solid var(--color-border);
}

.table-header-cell {
  padding: var(--spacing-lg);
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-muted);
  text-align: right;
  user-select: none;
}

[dir='ltr'] .table-header-cell {
  text-align: left;
}

.table-header-cell.sortable {
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.table-header-cell.sortable:hover {
  background-color: var(--color-border);
}

.header-content {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

[dir='rtl'] .header-content {
  flex-direction: row-reverse;
}

.table-data-row {
  border-bottom: 1px solid var(--color-border);
  transition: background-color 0.2s ease;
  cursor: pointer;
}

.table-data-row:hover {
  background-color: var(--color-surface-variant);
}

.table-data-cell {
  padding: var(--spacing-lg);
  font-size: 14px;
  color: var(--color-text);
  text-align: right;
}

[dir='ltr'] .table-data-cell {
  text-align: left;
}

.checkbox-cell {
  width: 50px;
  padding: var(--spacing-md);
  text-align: center;
}

.actions-cell {
  width: 100px;
  text-align: center;
}

.table-empty-row {
  border: none;
}

.table-empty-message {
  padding: var(--spacing-3xl) var(--spacing-lg);
  text-align: center;
  color: var(--color-text-muted);
  font-size: 14px;
}

.table-loading-row {
  border: none;
}

.table-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--spacing-3xl) var(--spacing-lg);
}

.table-pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-lg);
  border-top: 1px solid var(--color-border);
  background-color: var(--color-surface-variant);
  font-size: 12px;
  color: var(--color-text-muted);
}

.pagination-info {
  flex: 1;
}

:deep(.v-pagination) {
  flex: 1;
  justify-content: flex-end;
}

/* Responsive */
@media (max-width: 960px) {
  .table-header-cell,
  .table-data-cell {
    padding: var(--spacing-md);
    font-size: 13px;
  }

  .table-pagination {
    flex-direction: column;
    gap: var(--spacing-md);
    text-align: center;
  }

  .pagination-info {
    width: 100%;
  }
}

@media (max-width: 600px) {
  :deep(.v-table) {
    font-size: 12px;
  }

  .table-header-cell,
  .table-data-cell {
    padding: var(--spacing-sm);
  }

  .table-header-cell {
    font-size: 11px;
  }

  .table-data-cell {
    font-size: 12px;
  }

  .actions-cell {
    width: auto;
  }
}
</style>

<style scoped>
.data-table-wrapper {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  overflow: hidden;
}

:deep(.v-table) {
  background-color: var(--color-surface);
}

:deep(.v-table.data-table) {
  font-family: var(--font-family-primary);
}

.table-header-row {
  background-color: var(--color-surface-variant);
  border-bottom: 2px solid var(--color-border);
}

.table-header-cell {
  padding: var(--spacing-lg);
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-muted);
  text-align: right;
  user-select: none;
}

[dir='ltr'] .table-header-cell {
  text-align: left;
}

.table-header-cell.sortable {
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.table-header-cell.sortable:hover {
  background-color: var(--color-border);
}

.header-content {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

[dir='rtl'] .header-content {
  flex-direction: row-reverse;
}

.table-data-row {
  border-bottom: 1px solid var(--color-border);
  transition: background-color 0.2s ease;
  cursor: pointer;
}

.table-data-row:hover {
  background-color: var(--color-surface-variant);
}

.table-data-cell {
  padding: var(--spacing-lg);
  font-size: 14px;
  color: var(--color-text);
  text-align: right;
}

[dir='ltr'] .table-data-cell {
  text-align: left;
}

.checkbox-cell {
  width: 50px;
  padding: var(--spacing-md);
  text-align: center;
}

.actions-cell {
  width: 100px;
  text-align: center;
}

.table-empty-row {
  border: none;
}

.table-empty-message {
  padding: var(--spacing-3xl) var(--spacing-lg);
  text-align: center;
  color: var(--color-text-muted);
  font-size: 14px;
}

.table-loading-row {
  border: none;
}

.table-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--spacing-3xl) var(--spacing-lg);
}

.table-pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-lg);
  border-top: 1px solid var(--color-border);
  background-color: var(--color-surface-variant);
  font-size: 12px;
  color: var(--color-text-muted);
}

.pagination-info {
  flex: 1;
}

:deep(.v-pagination) {
  flex: 1;
  justify-content: flex-end;
}

/* Responsive */
@media (max-width: 960px) {
  .table-header-cell,
  .table-data-cell {
    padding: var(--spacing-md);
    font-size: 13px;
  }

  .table-pagination {
    flex-direction: column;
    gap: var(--spacing-md);
    text-align: center;
  }

  .pagination-info {
    width: 100%;
  }
}

@media (max-width: 600px) {
  :deep(.v-table) {
    font-size: 12px;
  }

  .table-header-cell,
  .table-data-cell {
    padding: var(--spacing-sm);
  }

  .table-header-cell {
    font-size: 11px;
  }

  .table-data-cell {
    font-size: 12px;
  }

  .actions-cell {
    width: auto;
  }
}
</style>