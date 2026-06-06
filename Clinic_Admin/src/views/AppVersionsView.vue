<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { Check, Edit, RefreshCw, Save, Smartphone, X } from '@lucide/vue'
import AppModal from '../components/AppModal.vue'
import api from '../services/api'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, AppVersionPolicy } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const notifications = useNotificationsStore()
const policies = ref<AppVersionPolicy[]>([])
const loading = ref(false)
const saving = ref(false)
const editOpen = ref(false)
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
    if (!selectedPolicy.value && policies.value.length) selectedPlatform.value = policies.value[0].platform
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

function openEditDialog() {
  fillForm(selectedPolicy.value)
  editOpen.value = true
}

function closeEditDialog() {
  editOpen.value = false
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
    selectedPlatform.value = saved.platform
    fillForm(saved)
    editOpen.value = false
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
        <p>اعرض سياسة كل منصة، وافتح التعديل فقط عند تغيير النسخ أو رسالة التحديث.</p>
      </div>
      <div class="heading-actions">
        <button class="secondary-button" type="button" :disabled="loading" @click="loadPolicies">
          <RefreshCw :size="17" /> تحديث البيانات
        </button>
        <button class="compact-primary" type="button" :disabled="loading || !selectedPolicy" @click="openEditDialog">
          <Edit :size="17" /> تعديل
        </button>
      </div>
    </div>

    <div v-if="loading && !policies.length" class="empty-panel">جارِ تحميل إصدارات التطبيق...</div>
    <div v-else class="version-layout">
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

      <section v-if="selectedPolicy" class="panel-card version-details">
        <div class="version-details-heading">
          <div>
            <span class="section-kicker">{{ platformLabel(selectedPolicy.platform) }}</span>
            <h3>سياسة الإصدار الحالية</h3>
          </div>
          <span class="soft-badge" :class="selectedPolicy.forceUpdate ? 'status-danger' : 'status-success'">
            {{ selectedPolicy.forceUpdate ? 'تحديث إجباري' : 'تحديث اختياري' }}
          </span>
        </div>

        <div class="version-details-grid">
          <div><span>آخر نسخة</span><strong>{{ selectedPolicy.latestVersion }}+{{ selectedPolicy.latestBuildNumber }}</strong></div>
          <div><span>أقل نسخة مسموحة</span><strong>{{ selectedPolicy.minimumSupportedVersion }}+{{ selectedPolicy.minimumSupportedBuildNumber }}</strong></div>
          <div><span>فحص التحديث</span><strong>{{ selectedPolicy.isEnabled ? 'مفعل' : 'متوقف' }}</strong></div>
          <div><span>رابط التحديث</span><strong>{{ selectedPolicy.updateUrl || 'غير محدد' }}</strong></div>
          <div class="full-field"><span>عنوان نافذة التحديث</span><strong>{{ selectedPolicy.title }}</strong></div>
          <div class="full-field"><span>رسالة التحديث</span><p>{{ selectedPolicy.message }}</p></div>
        </div>
      </section>

      <div v-else class="empty-panel">لا توجد سياسة إصدار لهذه المنصة.</div>
    </div>

    <AppModal v-if="editOpen" title="تعديل سياسة الإصدار" wide @close="closeEditDialog">
      <form class="modal-form version-form" @submit.prevent="savePolicy">
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
          <button class="secondary-button" type="button" :disabled="saving" @click="closeEditDialog">
            <X :size="17" /> إلغاء
          </button>
          <button class="compact-primary" type="submit" :disabled="saving">
            <Save :size="17" /> {{ saving ? 'جارِ الحفظ...' : 'حفظ السياسة' }}
          </button>
        </div>
      </form>
    </AppModal>
  </section>
</template>

<style scoped>
.version-details { padding: 18px; }
.version-details-heading { display: flex; justify-content: space-between; align-items: center; gap: 12px; margin-bottom: 16px; padding-bottom: 13px; border-bottom: 1px solid var(--line); }
.version-details-heading h3 { margin: 5px 0 0; font-size: 19px; }
.version-details-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.version-details-grid div { padding: 12px; border: 1px solid var(--line); border-radius: 10px; background: #fbfdfc; }
.version-details-grid span { display: block; margin-bottom: 5px; color: var(--muted); font-size: 12px; font-weight: 700; }
.version-details-grid strong { color: var(--ink); font-size: 14px; font-weight: 800; overflow-wrap: anywhere; }
.version-details-grid p { margin: 0; color: #58706c; line-height: 1.8; overflow-wrap: anywhere; }
@media (max-width: 760px) {
  .version-details-heading { display: block; }
  .version-details-heading .soft-badge { margin-top: 10px; }
  .version-details-grid { grid-template-columns: 1fr; }
}
</style>
