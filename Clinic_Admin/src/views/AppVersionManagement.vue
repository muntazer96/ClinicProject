<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import api from '../services/api'
import { useNotifications } from '../composables/useNotifications'
import type { ApiResponse, AppReleaseItem, AppReleaseResponse } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import PageHeader from '../components/common/Pageheader.vue'
import EmptyState from '../components/common/Emptystate.vue'

const { success: showSuccess, error: showError } = useNotifications()

const releases       = ref<AppReleaseItem[]>([])
const activeRelease  = ref<AppReleaseResponse | null>(null)
const loading        = ref(false)
const uploading      = ref(false)
const uploadProgress = ref(0)
const deleteDialog   = ref(false)
const deleteId       = ref<number | null>(null)
const fileInput      = ref<HTMLInputElement>()

const form = ref({
  versionName:  '',
  versionCode:  0,
  releaseNotes: '',
  isActive:     true,
  file:         null as File | null,
})

const canUpload = computed(() =>
  form.value.versionName.trim() && form.value.versionCode > 0 && form.value.file && !uploading.value
)

async function fetchReleases() {
  loading.value = true
  try {
    const [listRes, latestRes] = await Promise.all([
      api.get<ApiResponse<AppReleaseItem[]>>('/app-release'),
      api.get<ApiResponse<AppReleaseResponse>>('/app-release/latest'),
    ])
    releases.value     = listRes.data.data   ?? []
    activeRelease.value = latestRes.data.data ?? null
  } catch (e) {
    showError(getErrorMessage(e))
  } finally {
    loading.value = false
  }
}

function onFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  if (input.files?.length) form.value.file = input.files[0]
}

function triggerFileInput() { fileInput.value?.click() }

async function handleUpload() {
  if (!canUpload.value || !form.value.file) return
  uploading.value = true
  uploadProgress.value = 0

  const fd = new FormData()
  fd.append('File',        form.value.file)
  fd.append('VersionName', form.value.versionName.trim())
  fd.append('VersionCode', String(form.value.versionCode))
  if (form.value.releaseNotes.trim()) fd.append('ReleaseNotes', form.value.releaseNotes.trim())
  fd.append('IsActive', String(form.value.isActive))

  try {
    await api.post('/app-release/upload', fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (e) => {
        if (e.total) uploadProgress.value = Math.round((e.loaded / e.total) * 100)
      },
    })
    showSuccess('تم رفع الإصدار بنجاح')
    form.value = { versionName: '', versionCode: 0, releaseNotes: '', isActive: true, file: null }
    await fetchReleases()
  } catch (e) {
    showError(getErrorMessage(e))
  } finally {
    uploading.value = false
    uploadProgress.value = 0
  }
}

async function toggleActive(id: number) {
  try {
    await api.put(`/app-release/${id}/toggle-active`)
    await fetchReleases()
  } catch (e) { showError(getErrorMessage(e)) }
}

function confirmDelete(id: number) { deleteId.value = id; deleteDialog.value = true }

async function doDelete() {
  if (!deleteId.value) return
  try {
    await api.delete(`/app-release/${deleteId.value}`)
    showSuccess('تم حذف الإصدار')
    await fetchReleases()
  } catch (e) { showError(getErrorMessage(e)) }
  finally { deleteDialog.value = false; deleteId.value = null }
}

function formatSize(bytes: number): string {
  const units = ['B', 'KB', 'MB', 'GB']
  let i = 0; let size = bytes
  while (size >= 1024 && i < units.length - 1) { size /= 1024; i++ }
  return `${size.toFixed(i > 0 ? 1 : 0)} ${units[i]}`
}

onMounted(fetchReleases)
</script>

<template>
  <div class="releases-page">

    <!-- Header -->
    <PageHeader title="إصدارات التطبيق" subtitle="إدارة الإصدارات">
      <template #actions>
        <v-btn
          variant="outlined"
          color="primary"
          prepend-icon="mdi-refresh"
          :loading="loading"
          @click="fetchReleases"
        >
          تحديث
        </v-btn>
      </template>
    </PageHeader>

    <!-- Active Release Banner -->
    <div v-if="activeRelease" class="active-banner">
      <div class="active-banner-icon">
        <v-icon icon="mdi-android" color="primary" size="28" />
      </div>
      <div class="active-banner-info">
        <p class="active-label">الإصدار النشط حالياً</p>
        <div class="active-meta">
          <strong class="active-version">{{ activeRelease.versionName }}</strong>
          <v-chip size="small" color="primary" variant="tonal">
            كود {{ activeRelease.versionCode }}
          </v-chip>
        </div>
        <div class="active-stats">
          <span><v-icon icon="mdi-download" size="14" /> {{ activeRelease.downloadCount }} تحميل</span>
          <span><v-icon icon="mdi-file" size="14" /> {{ activeRelease.fileSize }}</span>
        </div>
        <ul v-if="activeRelease.releaseNotes?.length" class="release-notes">
          <li v-for="(note, i) in activeRelease.releaseNotes" :key="i">{{ note }}</li>
        </ul>
      </div>
    </div>

    <!-- Upload Form -->
    <v-card elevation="0" class="upload-card">
      <div class="card-header">
        <v-icon icon="mdi-cloud-upload" color="primary" size="20" />
        <h3>رفع إصدار جديد</h3>
      </div>
      <v-divider />

      <div class="upload-form">
        <!-- File Upload Zone -->
        <div class="file-zone" :class="{ 'file-zone-has': !!form.file }" @click="triggerFileInput">
          <input
            ref="fileInput"
            type="file"
            accept=".apk"
            class="file-hidden"
            @change="onFileChange"
          />
          <v-icon
            :icon="form.file ? 'mdi-file-check' : 'mdi-upload'"
            :color="form.file ? 'success' : 'primary'"
            size="36"
          />
          <div class="file-zone-text">
            <strong>{{ form.file ? form.file.name : 'اضغط لاختيار ملف APK' }}</strong>
            <span>{{ form.file ? formatSize(form.file.size) : 'يقبل ملفات .apk فقط' }}</span>
          </div>
        </div>

        <!-- Version Fields -->
        <div class="form-row">
          <div class="form-field">
            <label class="form-label">اسم الإصدار</label>
            <input v-model="form.versionName" class="form-input" placeholder="مثال: 1.0.5" />
          </div>
          <div class="form-field">
            <label class="form-label">رقم الإصدار (Version Code)</label>
            <input v-model.number="form.versionCode" type="number" min="1" class="form-input" placeholder="5" />
          </div>
        </div>

        <!-- Release Notes -->
        <div class="form-field">
          <label class="form-label">ملاحظات الإصدار</label>
          <textarea v-model="form.releaseNotes" class="form-textarea" rows="4" placeholder="سطر واحد لكل ملاحظة" />
        </div>

        <!-- Activate Toggle -->
        <label class="toggle-row">
          <input v-model="form.isActive" type="checkbox" class="toggle-native" />
          <span class="toggle-box">
            <v-icon v-if="form.isActive" icon="mdi-check" size="12" color="white" />
          </span>
          <span class="toggle-label">تفعيل هذا الإصدار فور الرفع</span>
        </label>

        <!-- Upload Progress -->
        <div v-if="uploading" class="progress-wrap">
          <div class="progress-track">
            <div class="progress-fill" :style="{ width: `${uploadProgress}%` }" />
          </div>
          <span class="progress-label">{{ uploadProgress }}% — جاري رفع الملف...</span>
        </div>

        <!-- Submit -->
        <v-btn
          color="primary"
          size="large"
          prepend-icon="mdi-upload"
          :loading="uploading"
          :disabled="!canUpload"
          @click="handleUpload"
        >
          {{ uploading ? 'جاري الرفع...' : 'رفع الإصدار' }}
        </v-btn>
      </div>
    </v-card>

    <!-- Releases Table -->
    <v-card elevation="0" class="table-card">
      <div class="card-header">
        <v-icon icon="mdi-history" color="primary" size="20" />
        <h3>سجل الإصدارات</h3>
        <v-chip size="small" color="primary" variant="tonal">{{ releases.length }}</v-chip>
      </div>
      <v-divider />

      <div v-if="loading" class="table-loading">
        <v-skeleton-loader v-for="i in 3" :key="i" type="table-row" />
      </div>

      <EmptyState
        v-else-if="!releases.length"
        icon="mdi-package-variant"
        title="لا توجد إصدارات"
        description="لم يتم رفع أي إصدارات بعد"
      />

      <div v-else class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th>الإصدار</th>
              <th>الكود</th>
              <th>الحجم</th>
              <th>التحميلات</th>
              <th>الحالة</th>
              <th>تاريخ الرفع</th>
              <th>رفع بواسطة</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in releases" :key="r.id" :class="{ 'row-active': r.isActive }">
              <td>
                <div class="version-cell">
                  <v-icon
                    :icon="r.isActive ? 'mdi-android' : 'mdi-android'"
                    :color="r.isActive ? 'success' : 'default'"
                    size="18"
                  />
                  <strong>{{ r.versionName }}</strong>
                </div>
              </td>
              <td>
                <v-chip size="x-small" variant="tonal">{{ r.versionCode }}</v-chip>
              </td>
              <td class="muted-cell">{{ formatSize(r.fileSize) }}</td>
              <td class="muted-cell">{{ r.downloadCount }}</td>
              <td>
                <v-chip
                  size="small"
                  :color="r.isActive ? 'success' : 'default'"
                  variant="tonal"
                >
                  {{ r.isActive ? 'نشط' : 'غير نشط' }}
                </v-chip>
              </td>
              <td class="muted-cell">{{ new Date(r.createdAt).toLocaleDateString('ar-IQ') }}</td>
              <td class="muted-cell">{{ r.createdBy }}</td>
              <td>
                <div class="row-actions">
                  <v-btn
                    icon
                    size="small"
                    variant="tonal"
                    :color="r.isActive ? 'warning' : 'success'"
                    :aria-label="r.isActive ? 'إلغاء التفعيل' : 'تفعيل'"
                    @click="toggleActive(r.id)"
                  >
                    <v-icon :icon="r.isActive ? 'mdi-close-circle' : 'mdi-check-circle'" size="16" />
                  </v-btn>
                  <v-btn
                    icon
                    size="small"
                    variant="tonal"
                    color="error"
                    aria-label="حذف"
                    @click="confirmDelete(r.id)"
                  >
                    <v-icon icon="mdi-delete" size="16" />
                  </v-btn>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </v-card>

    <!-- Delete Confirm Dialog -->
    <v-dialog v-model="deleteDialog" max-width="400">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-alert" color="error" size="22" />
          تأكيد الحذف
        </v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          هل أنت متأكد من حذف هذا الإصدار؟ لا يمكن التراجع عن هذه العملية.
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="deleteDialog = false">تراجع</v-btn>
          <v-btn color="error" @click="doDelete">تأكيد الحذف</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </div>
</template>

<style scoped>
.releases-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

/* Active Banner */
.active-banner {
  display: flex;
  gap: var(--spacing-lg);
  padding: var(--spacing-xl);
  background: linear-gradient(135deg, var(--color-primary-soft), #f6fdfa);
  border: 1px solid var(--color-primary-light);
  border-radius: var(--radius-lg);
}

.active-banner-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 56px;
  height: 56px;
  border-radius: var(--radius-md);
  background: white;
  flex-shrink: 0;
  box-shadow: var(--shadow-sm);
}

.active-banner-info { flex: 1; }

.active-label {
  margin: 0 0 6px 0;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-primary);
}

.active-meta {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-sm);
}

.active-version {
  font-size: 26px;
  font-weight: 800;
  color: var(--color-text);
}

.active-stats {
  display: flex;
  gap: var(--spacing-lg);
  font-size: 13px;
  color: var(--color-text-muted);
  margin-bottom: var(--spacing-md);
}

.active-stats span {
  display: flex;
  align-items: center;
  gap: 4px;
}

.release-notes {
  margin: 0;
  padding: 0 var(--spacing-lg) 0 0;
  font-size: 13px;
  color: var(--color-text-muted);
  line-height: 1.8;
}

/* Cards */
.upload-card,
.table-card {
  border: 1px solid var(--color-border) !important;
  border-radius: var(--radius-lg) !important;
  overflow: hidden;
}

.card-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-lg);
  background: var(--color-surface);
}

.card-header h3 {
  flex: 1;
  margin: 0;
  font-size: 15px;
  font-weight: 700;
  color: var(--color-text);
}

/* Upload Form */
.upload-form {
  padding: var(--spacing-xl);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

/* File Zone */
.file-zone {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-md);
  padding: var(--spacing-2xl);
  border: 2px dashed var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-background);
  cursor: pointer;
  transition: all 0.2s ease;
  text-align: center;
}

.file-zone:hover,
.file-zone-has {
  border-color: var(--color-primary);
  background: var(--color-primary-soft);
}

.file-hidden {
  display: none;
}

.file-zone-text strong {
  display: block;
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text);
  margin-bottom: 4px;
}

.file-zone-text span {
  font-size: 12px;
  color: var(--color-text-muted);
}

/* Form Fields */
.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-lg);
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

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
  transition: border-color 0.2s;
  width: 100%;
}

.form-input:focus,
.form-textarea:focus { border-color: var(--color-primary); }

.form-textarea { resize: vertical; }

/* Toggle */
.toggle-row {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  cursor: pointer;
  user-select: none;
}

.toggle-native {
  position: absolute;
  opacity: 0;
  width: 0;
  height: 0;
}

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

.toggle-label {
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text);
}

/* Progress */
.progress-wrap {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.progress-track {
  height: 6px;
  background: var(--color-border);
  border-radius: 999px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, var(--color-primary), var(--color-primary-light));
  transition: width 0.3s ease;
}

.progress-label {
  font-size: 12px;
  color: var(--color-text-muted);
}

/* Table */
.table-loading { padding: var(--spacing-lg); }

.table-scroll { overflow-x: auto; }

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.data-table th {
  padding: 12px 16px;
  text-align: right;
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-muted);
  background: var(--color-background);
  border-bottom: 1px solid var(--color-border);
  white-space: nowrap;
}

.data-table td {
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border-light);
  vertical-align: middle;
  color: var(--color-text);
}

.data-table tbody tr:last-child td { border-bottom: none; }
.data-table tbody tr:hover { background: var(--color-background); }

.row-active td { background: var(--color-primary-soft); }
.row-active:hover td { background: var(--color-primary-soft); }

.version-cell {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.muted-cell { color: var(--color-text-muted); }

.row-actions {
  display: flex;
  gap: var(--spacing-sm);
}

/* Dialog */
.dialog-title {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-lg) !important;
  font-size: 16px !important;
  font-weight: 700;
}

.dialog-body { padding: var(--spacing-lg) !important; font-size: 14px; }

.dialog-actions {
  padding: var(--spacing-lg) !important;
  gap: var(--spacing-md);
  justify-content: flex-end;
}

/* Responsive */
@media (max-width: 768px) {
  .active-banner { flex-direction: column; }
  .form-row { grid-template-columns: 1fr; }
}
</style>
