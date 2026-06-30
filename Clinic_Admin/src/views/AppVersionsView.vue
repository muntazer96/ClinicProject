<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import api from '../services/api'
import { useNotifications } from '../composables/useNotifications'
import type { ApiResponse, AppVersionPolicy } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import PageHeader from '../components/common/Pageheader.vue'
import EmptyState from '../components/common/Emptystate.vue'

const { success: showSuccess, error: showError } = useNotifications()

const policies         = ref<AppVersionPolicy[]>([])
const loading          = ref(false)
const saving           = ref(false)
const editOpen         = ref(false)
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
  android: 'Android',
  ios: 'iOS',
  web: 'Web',
  admin: 'لوحة التحكم',
  backend: 'الخادم',
}

const platformIcons: Record<string, string> = {
  android: 'mdi-android',
  ios: 'mdi-apple',
  web: 'mdi-web',
  admin: 'mdi-view-dashboard',
  backend: 'mdi-server',
}

const selectedPolicy = computed(() =>
  policies.value.find((p) => p.platform === selectedPlatform.value)
)

function platformLabel(p: string) { return platformLabels[p] ?? p }
function platformIcon(p: string)  { return platformIcons[p]  ?? 'mdi-cellphone' }

function fillForm(policy?: AppVersionPolicy) {
  Object.assign(form, {
    platform:                    policy?.platform                    ?? selectedPlatform.value,
    latestVersion:               policy?.latestVersion               ?? '1.0.0',
    latestBuildNumber:           policy?.latestBuildNumber           ?? 1,
    minimumSupportedVersion:     policy?.minimumSupportedVersion     ?? '1.0.0',
    minimumSupportedBuildNumber: policy?.minimumSupportedBuildNumber ?? 1,
    forceUpdate:                 policy?.forceUpdate                 ?? false,
    isEnabled:                   policy?.isEnabled                   ?? true,
    title:                       policy?.title                       ?? 'تحديث جديد متوفر',
    message:                     policy?.message                     ?? 'تتوفر نسخة أحدث من التطبيق.',
    updateUrl:                   policy?.updateUrl                   ?? '',
  })
}

async function loadPolicies() {
  loading.value = true
  try {
    const response = await api.get<ApiResponse<AppVersionPolicy[]>>('/AppVersion')
    policies.value = response.data.data
    if (!selectedPolicy.value && policies.value.length)
      selectedPlatform.value = policies.value[0].platform
    fillForm(selectedPolicy.value)
  } catch (e) {
    showError(getErrorMessage(e))
  } finally {
    loading.value = false
  }
}

function selectPlatform(platform: string) {
  selectedPlatform.value = platform
  fillForm(selectedPolicy.value)
}

function openEdit() {
  fillForm(selectedPolicy.value)
  editOpen.value = true
}

async function savePolicy() {
  saving.value = true
  try {
    const response = await api.put<ApiResponse<AppVersionPolicy>>('/AppVersion', {
      ...form,
      latestBuildNumber:           Number(form.latestBuildNumber),
      minimumSupportedBuildNumber: Number(form.minimumSupportedBuildNumber),
      updateUrl: form.updateUrl || null,
    })
    const saved = response.data.data
    const idx = policies.value.findIndex((p) => p.platform === saved.platform)
    if (idx >= 0) policies.value[idx] = saved
    else policies.value.push(saved)
    selectedPlatform.value = saved.platform
    fillForm(saved)
    editOpen.value = false
    showSuccess(response.data.message)
  } catch (e) {
    showError(getErrorMessage(e))
  } finally {
    saving.value = false
  }
}

onMounted(loadPolicies)
</script>

<template>
  <div class="versions-page">

    <!-- Header -->
    <PageHeader title="إصدارات النظام والتطبيق" subtitle="إدارة التحديثات">
      <template #actions>
        <v-btn
          variant="outlined"
          color="primary"
          prepend-icon="mdi-refresh"
          :loading="loading"
          @click="loadPolicies"
        >
          تحديث
        </v-btn>
        <v-btn
          color="primary"
          prepend-icon="mdi-pencil"
          :disabled="!selectedPolicy"
          @click="openEdit"
        >
          تعديل السياسة
        </v-btn>
      </template>
    </PageHeader>

    <!-- Loading -->
    <div v-if="loading && !policies.length" class="loading-wrap">
      <v-skeleton-loader v-for="i in 4" :key="i" type="card" />
    </div>

    <!-- Content -->
    <div v-else class="versions-layout">

      <!-- Platform Sidebar -->
      <div class="platforms-sidebar">
        <p class="sidebar-title">المنصات</p>
        <button
          v-for="policy in policies"
          :key="policy.platform"
          type="button"
          class="platform-btn"
          :class="{ 'platform-active': selectedPlatform === policy.platform }"
          @click="selectPlatform(policy.platform)"
        >
          <div class="platform-icon-wrap" :class="{ 'platform-icon-active': selectedPlatform === policy.platform }">
            <v-icon :icon="platformIcon(policy.platform)" size="20" />
          </div>
          <div class="platform-info">
            <strong>{{ platformLabel(policy.platform) }}</strong>
            <span>{{ policy.latestVersion }}+{{ policy.latestBuildNumber }}</span>
          </div>
          <v-icon
            v-if="policy.isEnabled"
            icon="mdi-check-circle"
            color="success"
            size="16"
            class="platform-check"
          />
        </button>
      </div>

      <!-- Policy Details -->
      <div v-if="selectedPolicy" class="policy-details">

        <!-- Policy Header -->
        <div class="policy-header-card">
          <div class="policy-header-left">
            <div class="policy-platform-icon">
              <v-icon :icon="platformIcon(selectedPolicy.platform)" color="primary" size="28" />
            </div>
            <div>
              <p class="policy-kicker">سياسة الإصدار</p>
              <h2 class="policy-platform-name">{{ platformLabel(selectedPolicy.platform) }}</h2>
            </div>
          </div>
          <div class="policy-header-badges">
            <v-chip
              size="small"
              :color="selectedPolicy.forceUpdate ? 'error' : 'success'"
              variant="tonal"
            >
              {{ selectedPolicy.forceUpdate ? 'تحديث إجباري' : 'تحديث اختياري' }}
            </v-chip>
            <v-chip
              size="small"
              :color="selectedPolicy.isEnabled ? 'success' : 'warning'"
              variant="tonal"
            >
              {{ selectedPolicy.isEnabled ? 'فحص مفعّل' : 'فحص متوقف' }}
            </v-chip>
          </div>
        </div>

        <!-- Version Info Grid -->
        <div class="info-grid">
          <div class="info-card info-card--primary">
            <v-icon icon="mdi-tag" color="primary" size="18" class="info-icon" />
            <p class="info-label">آخر نسخة</p>
            <strong class="info-value">{{ selectedPolicy.latestVersion }}</strong>
            <span class="info-sub">بناء {{ selectedPolicy.latestBuildNumber }}</span>
          </div>

          <div class="info-card">
            <v-icon icon="mdi-shield-check" color="warning" size="18" class="info-icon" />
            <p class="info-label">أقل نسخة مسموحة</p>
            <strong class="info-value">{{ selectedPolicy.minimumSupportedVersion }}</strong>
            <span class="info-sub">بناء {{ selectedPolicy.minimumSupportedBuildNumber }}</span>
          </div>

          <div class="info-card">
            <v-icon icon="mdi-link" color="info" size="18" class="info-icon" />
            <p class="info-label">رابط التحديث</p>
            <strong class="info-value info-value--small">
              {{ selectedPolicy.updateUrl || 'غير محدد' }}
            </strong>
          </div>

          <div class="info-card">
            <v-icon icon="mdi-update" color="success" size="18" class="info-icon" />
            <p class="info-label">حالة الفحص</p>
            <strong class="info-value">{{ selectedPolicy.isEnabled ? 'مفعّل' : 'متوقف' }}</strong>
          </div>
        </div>

        <!-- Update Dialog Preview -->
        <div class="dialog-preview">
          <p class="preview-label">
            <v-icon icon="mdi-message-text" size="16" color="primary" />
            معاينة نافذة التحديث
          </p>
          <div class="preview-card">
            <div class="preview-icon">
              <v-icon icon="mdi-update" color="primary" size="28" />
            </div>
            <h4 class="preview-title">{{ selectedPolicy.title }}</h4>
            <p class="preview-message">{{ selectedPolicy.message }}</p>
            <div class="preview-actions">
              <div class="preview-btn preview-btn--primary">تحديث الآن</div>
              <div v-if="!selectedPolicy.forceUpdate" class="preview-btn preview-btn--outline">لاحقاً</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty -->
      <EmptyState
        v-else
        icon="mdi-package-variant"
        title="لا توجد سياسة"
        description="لا توجد سياسة إصدار لهذه المنصة"
      />
    </div>

    <!-- Edit Dialog -->
    <v-dialog v-model="editOpen" max-width="680">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-pencil" color="primary" size="22" />
          تعديل سياسة الإصدار
        </v-card-title>
        <v-divider />

        <v-card-text class="dialog-body">
          <div class="edit-grid">
            <!-- Platform -->
            <div class="form-field">
              <label class="form-label">المنصة</label>
              <input v-model="form.platform" class="form-input" required maxlength="40" />
            </div>

            <!-- Update URL -->
            <div class="form-field">
              <label class="form-label">رابط التحديث</label>
              <input v-model="form.updateUrl" class="form-input" placeholder="https://..." maxlength="500" />
            </div>

            <!-- Latest Version -->
            <div class="form-field">
              <label class="form-label">آخر نسخة</label>
              <input v-model="form.latestVersion" class="form-input" required maxlength="30" />
            </div>

            <!-- Latest Build -->
            <div class="form-field">
              <label class="form-label">رقم بناء آخر نسخة</label>
              <input v-model.number="form.latestBuildNumber" type="number" min="1" class="form-input" required />
            </div>

            <!-- Min Version -->
            <div class="form-field">
              <label class="form-label">أقل نسخة مسموحة</label>
              <input v-model="form.minimumSupportedVersion" class="form-input" required maxlength="30" />
            </div>

            <!-- Min Build -->
            <div class="form-field">
              <label class="form-label">رقم بناء أقل نسخة</label>
              <input v-model.number="form.minimumSupportedBuildNumber" type="number" min="1" class="form-input" required />
            </div>

            <!-- Title -->
            <div class="form-field form-field--full">
              <label class="form-label">عنوان نافذة التحديث</label>
              <input v-model="form.title" class="form-input" required maxlength="120" />
            </div>

            <!-- Message -->
            <div class="form-field form-field--full">
              <label class="form-label">رسالة التحديث</label>
              <textarea v-model="form.message" class="form-textarea" required maxlength="600" rows="4" />
            </div>
          </div>

          <!-- Toggles -->
          <div class="toggles-row">
            <label class="toggle-label">
              <input v-model="form.isEnabled" type="checkbox" class="toggle-native" />
              <span class="toggle-box">
                <v-icon v-if="form.isEnabled" icon="mdi-check" size="12" color="white" />
              </span>
              <span>تفعيل فحص التحديث لهذه المنصة</span>
            </label>
            <label class="toggle-label">
              <input v-model="form.forceUpdate" type="checkbox" class="toggle-native" />
              <span class="toggle-box" :class="{ 'toggle-box--danger': form.forceUpdate }">
                <v-icon v-if="form.forceUpdate" icon="mdi-check" size="12" color="white" />
              </span>
              <span>تحديث إجباري</span>
            </label>
          </div>
        </v-card-text>

        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" :disabled="saving" @click="editOpen = false">إلغاء</v-btn>
          <v-btn color="primary" prepend-icon="mdi-content-save" :loading="saving" @click="savePolicy">
            حفظ السياسة
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </div>
</template>

<style scoped>
.versions-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.loading-wrap {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--spacing-lg);
}

/* Layout */
.versions-layout {
  display: grid;
  grid-template-columns: 240px 1fr;
  gap: var(--spacing-lg);
  align-items: start;
}

/* Platforms Sidebar */
.platforms-sidebar {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--spacing-md);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.sidebar-title {
  margin: 0 0 var(--spacing-sm) 0;
  padding: 0 var(--spacing-sm);
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-muted);
}

.platform-btn {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  width: 100%;
  padding: var(--spacing-md);
  border: none;
  border-radius: var(--radius-md);
  background: transparent;
  cursor: pointer;
  text-align: right;
  transition: all 0.15s ease;
  font-family: var(--font-family-primary);
}

.platform-btn:hover {
  background: var(--color-primary-soft);
}

.platform-active {
  background: var(--color-primary-soft) !important;
}

.platform-icon-wrap {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: var(--radius-md);
  background: var(--color-background);
  color: var(--color-text-muted);
  flex-shrink: 0;
  transition: all 0.15s ease;
}

.platform-icon-active {
  background: var(--color-primary);
  color: white !important;
}

.platform-icon-active :deep(.v-icon) {
  color: white !important;
}

.platform-info {
  flex: 1;
  min-width: 0;
  text-align: right;
}

.platform-info strong {
  display: block;
  font-size: 13px;
  font-weight: 700;
  color: var(--color-text);
  margin-bottom: 2px;
}

.platform-info span {
  font-size: 11px;
  color: var(--color-text-muted);
  font-weight: 600;
}

.platform-check { flex-shrink: 0; }

/* Policy Details */
.policy-details {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

/* Policy Header */
.policy-header-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-lg);
  padding: var(--spacing-xl);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}

.policy-header-left {
  display: flex;
  align-items: center;
  gap: var(--spacing-lg);
}

.policy-platform-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 56px;
  height: 56px;
  border-radius: var(--radius-lg);
  background: var(--color-primary-soft);
  flex-shrink: 0;
}

.policy-kicker {
  margin: 0 0 4px 0;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-muted);
}

.policy-platform-name {
  margin: 0;
  font-size: 22px;
  font-weight: 800;
  color: var(--color-text);
}

.policy-header-badges {
  display: flex;
  gap: var(--spacing-sm);
  flex-wrap: wrap;
  justify-content: flex-end;
}

/* Info Grid */
.info-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--spacing-lg);
}

.info-card {
  padding: var(--spacing-lg);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}

.info-card--primary {
  background: var(--color-primary-soft);
  border-color: var(--color-primary-light);
}

.info-icon { margin-bottom: var(--spacing-md); }

.info-label {
  margin: 0 0 6px 0;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-muted);
}

.info-value {
  display: block;
  font-size: 22px;
  font-weight: 800;
  color: var(--color-text);
  margin-bottom: 4px;
  line-height: 1.2;
}

.info-value--small {
  font-size: 13px;
  overflow-wrap: anywhere;
}

.info-sub {
  font-size: 12px;
  color: var(--color-text-muted);
}

/* Dialog Preview */
.dialog-preview {}

.preview-label {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  margin: 0 0 var(--spacing-md) 0;
  font-size: 13px;
  font-weight: 700;
  color: var(--color-text-muted);
}

.preview-card {
  max-width: 320px;
  margin: 0 auto;
  padding: var(--spacing-xl);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-lg);
  text-align: center;
}

.preview-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: var(--color-primary-soft);
  margin: 0 auto var(--spacing-lg);
}

.preview-title {
  margin: 0 0 var(--spacing-md) 0;
  font-size: 16px;
  font-weight: 700;
  color: var(--color-text);
}

.preview-message {
  margin: 0 0 var(--spacing-xl) 0;
  font-size: 13px;
  color: var(--color-text-muted);
  line-height: 1.7;
}

.preview-actions {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.preview-btn {
  padding: 10px;
  border-radius: var(--radius-md);
  font-size: 14px;
  font-weight: 700;
  text-align: center;
}

.preview-btn--primary {
  background: var(--color-primary);
  color: white;
}

.preview-btn--outline {
  border: 1.5px solid var(--color-border);
  color: var(--color-text-muted);
}

/* Edit Form */
.dialog-title {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-lg) !important;
  font-size: 16px !important;
  font-weight: 700;
}

.dialog-body { padding: var(--spacing-lg) !important; }

.edit-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-lg);
  margin-bottom: var(--spacing-lg);
}

.form-field { display: flex; flex-direction: column; gap: 6px; }
.form-field--full { grid-column: 1 / -1; }

.form-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text);
}

.form-input,
.form-textarea {
  padding: 10px 12px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-size: 14px;
  outline: none;
  width: 100%;
  transition: border-color 0.2s;
}

.form-input:focus,
.form-textarea:focus { border-color: var(--color-primary); }

.form-textarea { resize: vertical; }

/* Toggles */
.toggles-row {
  display: flex;
  gap: var(--spacing-xl);
  flex-wrap: wrap;
}

.toggle-label {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text);
  user-select: none;
}

.toggle-native { position: absolute; opacity: 0; width: 0; height: 0; }

.toggle-box {
  width: 18px;
  height: 18px;
  border: 2px solid var(--color-border);
  border-radius: 4px;
  background: white;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: all 0.15s ease;
}

.toggle-native:checked ~ .toggle-box {
  background: var(--color-primary);
  border-color: var(--color-primary);
}

.toggle-box--danger {
  background: var(--color-error) !important;
  border-color: var(--color-error) !important;
}

.dialog-actions {
  padding: var(--spacing-lg) !important;
  gap: var(--spacing-md);
  justify-content: flex-end;
}

/* Responsive */
@media (max-width: 1100px) {
  .info-grid { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 768px) {
  .versions-layout { grid-template-columns: 1fr; }
  .platforms-sidebar { display: grid; grid-template-columns: repeat(auto-fill, minmax(140px, 1fr)); gap: var(--spacing-sm); }
  .sidebar-title { grid-column: 1 / -1; }
  .policy-header-card { flex-direction: column; align-items: flex-start; }
  .edit-grid { grid-template-columns: 1fr; }
}

@media (max-width: 600px) {
  .info-grid { grid-template-columns: 1fr; }
}
</style>