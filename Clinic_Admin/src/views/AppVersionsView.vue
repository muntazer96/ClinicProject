<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { Check, RefreshCw, Save, Smartphone } from '@lucide/vue'
import api from '../services/api'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, AppVersionPolicy } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const notifications = useNotificationsStore()
const policies = ref<AppVersionPolicy[]>([])
const loading = ref(false)
const saving = ref(false)
const selectedPlatform = ref('android')
const form = reactive({
  platform: 'android',
  latestVersion: '1.0.0',
  latestBuildNumber: 1,
  minimumSupportedVersion: '1.0.0',
  minimumSupportedBuildNumber: 1,
  forceUpdate: false,
  isEnabled: true,
  title: 'تحديث جديد متوفر',
  message: 'تتوفر نسخة أحدث من التطبيق. يرجى التحديث للحصول على أفضل تجربة.',
  updateUrl: '',
})

const platformLabels: Record<string, string> = {
  android: 'تطبيق Android',
  ios: 'تطبيق iOS',
  web: 'تطبيق Web',
  admin: 'لوحة التحكم',
  backend: 'الخادم',
}

const selectedPolicy = computed(() => policies.value.find((item) => item.platform === selectedPlatform.value))

function platformLabel(platform: string) {
  return platformLabels[platform] ?? platform
}

function fillForm(policy?: AppVersionPolicy) {
  Object.assign(form, {
    platform: policy?.platform ?? selectedPlatform.value,
    latestVersion: policy?.latestVersion ?? '1.0.0',
    latestBuildNumber: policy?.latestBuildNumber ?? 1,
    minimumSupportedVersion: policy?.minimumSupportedVersion ?? '1.0.0',
    minimumSupportedBuildNumber: policy?.minimumSupportedBuildNumber ?? 1,
    forceUpdate: policy?.forceUpdate ?? false,
    isEnabled: policy?.isEnabled ?? true,
    title: policy?.title ?? 'تحديث جديد متوفر',
    message: policy?.message ?? 'تتوفر نسخة أحدث من التطبيق. يرجى التحديث للحصول على أفضل تجربة.',
    updateUrl: policy?.updateUrl ?? '',
  })
}

async function loadPolicies() {
  loading.value = true
  try {
    const response = await api.get<ApiResponse<AppVersionPolicy[]>>('/AppVersion')
    policies.value = response.data.data
    fillForm(selectedPolicy.value)
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    loading.value = false
  }
}

function selectPlatform(platform: string) {
  selectedPlatform.value = platform
  fillForm(selectedPolicy.value)
}

async function savePolicy() {
  saving.value = true
  try {
    const response = await api.put<ApiResponse<AppVersionPolicy>>('/AppVersion', {
      ...form,
      latestBuildNumber: Number(form.latestBuildNumber),
      minimumSupportedBuildNumber: Number(form.minimumSupportedBuildNumber),
      updateUrl: form.updateUrl || null,
    })
    const saved = response.data.data
    const index = policies.value.findIndex((item) => item.platform === saved.platform)
    if (index >= 0) policies.value[index] = saved
    else policies.value.push(saved)
    notifications.show(response.data.message)
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    saving.value = false
  }
}

onMounted(loadPolicies)
</script>

<template>
  <section>
    <div class="page-heading">
      <div>
        <span class="section-kicker">إدارة التحديثات</span>
        <h2>إصدارات النظام والتطبيق</h2>
        <p>تحكم بآخر نسخة، أقل نسخة مسموحة، ورسالة التحديث التي تظهر للمستخدم.</p>
      </div>
      <button class="secondary-button" type="button" :disabled="loading" @click="loadPolicies">
        <RefreshCw :size="17" /> تحديث البيانات
      </button>
    </div>

    <div class="version-layout">
      <aside class="version-platforms">
        <button
          v-for="policy in policies"
          :key="policy.platform"
          type="button"
          :class="{ active: selectedPlatform === policy.platform }"
          @click="selectPlatform(policy.platform)"
        >
          <span><Smartphone :size="18" /></span>
          <strong>{{ platformLabel(policy.platform) }}</strong>
          <small>{{ policy.latestVersion }}+{{ policy.latestBuildNumber }}</small>
          <Check v-if="policy.isEnabled" :size="16" />
        </button>
      </aside>

      <form class="panel-card version-form" @submit.prevent="savePolicy">
        <div class="form-grid">
          <label>
            <span>المنصة</span>
            <input v-model="form.platform" required maxlength="40" />
          </label>
          <label>
            <span>رابط التحديث</span>
            <input v-model="form.updateUrl" placeholder="https://..." maxlength="500" />
          </label>
          <label>
            <span>آخر نسخة</span>
            <input v-model="form.latestVersion" required maxlength="30" />
          </label>
          <label>
            <span>رقم بناء آخر نسخة</span>
            <input v-model.number="form.latestBuildNumber" required type="number" min="1" />
          </label>
          <label>
            <span>أقل نسخة مسموحة</span>
            <input v-model="form.minimumSupportedVersion" required maxlength="30" />
          </label>
          <label>
            <span>رقم بناء أقل نسخة</span>
            <input v-model.number="form.minimumSupportedBuildNumber" required type="number" min="1" />
          </label>
          <label class="full-field">
            <span>عنوان نافذة التحديث</span>
            <input v-model="form.title" required maxlength="120" />
          </label>
          <label class="full-field">
            <span>رسالة التحديث</span>
            <textarea v-model="form.message" required maxlength="600" rows="4" />
          </label>
        </div>

        <div class="version-switches">
          <label class="checkbox-field">
            <input v-model="form.isEnabled" type="checkbox" />
            <span>تفعيل فحص التحديث لهذه المنصة</span>
          </label>
          <label class="checkbox-field">
            <input v-model="form.forceUpdate" type="checkbox" />
            <span>تحديث إجباري</span>
          </label>
        </div>

        <div class="modal-actions">
          <button class="compact-primary" type="submit" :disabled="saving">
            <Save :size="17" /> {{ saving ? 'جارِ الحفظ...' : 'حفظ السياسة' }}
          </button>
        </div>
      </form>
    </div>
  </section>
</template>
