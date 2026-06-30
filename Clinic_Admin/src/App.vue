<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterView } from 'vue-router'
import NotificationCenter from './components/common/NotificationCenter.vue'
import api from './services/api'
import type { ApiResponse, AppVersionCheck } from './types/api'
import { adminAppVersion, adminBuildNumber } from './utils/appVersion'

const updateInfo = ref<AppVersionCheck>()

onMounted(async () => {
  try {
    const response = await api.get<ApiResponse<AppVersionCheck>>('/AppVersion/check', {
      params: {
        platform: 'admin',
        currentVersion: adminAppVersion,
        currentBuildNumber: adminBuildNumber,
      },
    })
    if (response.data.data.updateAvailable) {
      updateInfo.value = response.data.data
    }
  } catch {
    // Version checks should not block the admin panel when the API is unavailable.
  }
})

function closeUpdateModal() {
  if (updateInfo.value?.updateRequired) return
  updateInfo.value = undefined
}

function openUpdateUrl() {
  const url = updateInfo.value?.updateUrl
  if (url) window.open(url, '_blank', 'noopener,noreferrer')
}
</script>

<template>
  <RouterView />
  <NotificationCenter />

  <v-dialog
    :model-value="Boolean(updateInfo)"
    max-width="520"
    :persistent="updateInfo?.updateRequired"
    @update:model-value="(value) => !value && closeUpdateModal()"
  >
    <v-card v-if="updateInfo">
      <v-card-title class="dialog-title">
        <v-icon icon="mdi-update" color="primary" size="22" />
        {{ updateInfo.title }}
      </v-card-title>

      <v-card-text class="dialog-body">
        <p class="modal-copy">{{ updateInfo.message }}</p>
        <div class="version-mini-panel">
          <span>النسخة الحالية: {{ updateInfo.currentVersion }}+{{ updateInfo.currentBuildNumber }}</span>
          <strong>النسخة المتوفرة: {{ updateInfo.latestVersion }}+{{ updateInfo.latestBuildNumber }}</strong>
        </div>
      </v-card-text>

      <v-card-actions class="dialog-actions">
        <v-btn
          v-if="!updateInfo.updateRequired"
          variant="text"
          @click="closeUpdateModal"
        >
          لاحقا
        </v-btn>
        <v-btn
          color="primary"
          variant="flat"
          :disabled="!updateInfo.updateUrl"
          prepend-icon="mdi-open-in-new"
          @click="openUpdateUrl"
        >
          تحديث اللوحة
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.dialog-title {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-lg) !important;
  font-size: 16px !important;
  font-weight: 700;
}

.dialog-body {
  padding: var(--spacing-lg) !important;
}

.modal-copy {
  margin: 0 0 var(--spacing-lg) 0;
  color: var(--color-text);
  line-height: 1.7;
}

.version-mini-panel {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  padding: var(--spacing-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-background);
  color: var(--color-text);
}

.dialog-actions {
  padding: var(--spacing-lg) !important;
  gap: var(--spacing-md);
}
</style>
