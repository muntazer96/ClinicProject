<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterView } from 'vue-router'
import AppModal from './components/AppModal.vue'
import AppToast from './components/AppToast.vue'
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
  <AppToast />

  <AppModal
    v-if="updateInfo"
    :title="updateInfo.title"
    @close="closeUpdateModal"
  >
    <div class="modal-form">
      <p class="modal-copy">{{ updateInfo.message }}</p>
      <div class="version-mini-panel">
        <span>النسخة الحالية: {{ updateInfo.currentVersion }}+{{ updateInfo.currentBuildNumber }}</span>
        <strong>النسخة المتوفرة: {{ updateInfo.latestVersion }}+{{ updateInfo.latestBuildNumber }}</strong>
      </div>
      <div class="modal-actions">
        <button
          v-if="!updateInfo.updateRequired"
          class="secondary-button"
          type="button"
          @click="closeUpdateModal"
        >
          لاحقاً
        </button>
        <button
          class="compact-primary"
          type="button"
          :disabled="!updateInfo.updateUrl"
          @click="openUpdateUrl"
        >
          تحديث اللوحة
        </button>
      </div>
    </div>
  </AppModal>
</template>
