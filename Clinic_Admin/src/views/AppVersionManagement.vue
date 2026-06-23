<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  CheckCircle,
  CloudUpload,
  Download,
  LoaderCircle,
  Smartphone,
  Trash2,
  Upload,
  XCircle,
} from '@lucide/vue'
import api from '../services/api'
import type { ApiResponse, AppReleaseItem, AppReleaseResponse } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const releases = ref<AppReleaseItem[]>([])
const activeRelease = ref<AppReleaseResponse | null>(null)
const loading = ref(false)
const uploading = ref(false)
const uploadProgress = ref(0)
const error = ref('')
const success = ref('')

const form = ref({
  versionName: '',
  versionCode: 0,
  releaseNotes: '',
  isActive: true,
  file: null as File | null,
})

const canUpload = computed(() =>
  form.value.versionName.trim()
  && form.value.versionCode > 0
  && form.value.file
  && !uploading.value
)

async function fetchReleases() {
  loading.value = true
  error.value = ''
  try {
    const [listRes, latestRes] = await Promise.all([
      api.get<ApiResponse<AppReleaseItem[]>>('/app-release'),
      api.get<ApiResponse<AppReleaseResponse>>('/app-release/latest'),
    ])
    releases.value = listRes.data.data ?? []
    activeRelease.value = latestRes.data.data ?? null
  } catch (err) {
    error.value = getErrorMessage(err)
  } finally {
    loading.value = false
  }
}

function onFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  if (input.files && input.files.length > 0) {
    form.value.file = input.files[0]
  }
}

async function handleUpload() {
  if (!canUpload.value || !form.value.file) return

  uploading.value = true
  uploadProgress.value = 0
  error.value = ''
  success.value = ''

  const formData = new FormData()
  formData.append('File', form.value.file)
  formData.append('VersionName', form.value.versionName.trim())
  formData.append('VersionCode', String(form.value.versionCode))
  if (form.value.releaseNotes.trim()) {
    formData.append('ReleaseNotes', form.value.releaseNotes.trim())
  }
  formData.append('IsActive', String(form.value.isActive))

  try {
    await api.post('/app-release/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (e) => {
        if (e.total) uploadProgress.value = Math.round((e.loaded / e.total) * 100)
      },
    })
    success.value = 'تم رفع الإصدار بنجاح.'
    form.value = { versionName: '', versionCode: 0, releaseNotes: '', isActive: true, file: null }
    await fetchReleases()
  } catch (err) {
    error.value = getErrorMessage(err)
  } finally {
    uploading.value = false
    uploadProgress.value = 0
  }
}

async function toggleActive(id: number) {
  try {
    await api.put(`/app-release/${id}/toggle-active`)
    await fetchReleases()
  } catch (err) {
    error.value = getErrorMessage(err)
  }
}

async function deleteRelease(id: number) {
  if (!confirm('هل أنت متأكد من حذف هذا الإصدار؟')) return
  try {
    await api.delete(`/app-release/${id}`)
    await fetchReleases()
  } catch (err) {
    error.value = getErrorMessage(err)
  }
}

function formatSize(bytes: number): string {
  const units = ['B', 'KB', 'MB', 'GB']
  let i = 0
  let size = bytes
  while (size >= 1024 && i < units.length - 1) { size /= 1024; i++ }
  return `${size.toFixed(i > 0 ? 1 : 0)} ${units[i]}`
}

onMounted(fetchReleases)
</script>

<template>
  <div>
    <div class="page-heading">
      <div>
        <span class="section-kicker">إدارة الإصدارات</span>
        <h2>إصدارات التطبيق</h2>
        <p>رفع وإدارة إصدارات تطبيق العيادات الطبية (APK)</p>
      </div>
    </div>

    <p v-if="error" class="form-error">{{ error }}</p>
    <p v-if="success" class="success-banner">{{ success }}</p>

    <!-- Active Version Card -->
    <div v-if="activeRelease" class="active-version-card">
      <div class="active-version-header">
        <Smartphone :size="22" />
        <span>الإصدار النشط حالياً</span>
      </div>
      <div class="active-version-body">
        <div class="version-meta">
          <strong>{{ activeRelease.versionName }}</strong>
          <span class="version-badge">الإصدار {{ activeRelease.versionCode }}</span>
        </div>
        <div class="version-stats">
          <span><Download :size="14" /> {{ activeRelease.downloadCount }}</span>
          <span>{{ activeRelease.fileSize }}</span>
        </div>
      </div>
      <ul v-if="activeRelease.releaseNotes.length" class="release-notes-preview">
        <li v-for="(note, i) in activeRelease.releaseNotes" :key="i">{{ note }}</li>
      </ul>
    </div>

    <!-- Upload Form -->
    <div class="upload-card">
      <div class="upload-header">
        <CloudUpload :size="20" />
        <span>رفع إصدار جديد</span>
      </div>
      <div class="upload-form">
        <label class="form-field">
          <span>ملف APK</span>
          <div class="file-input-wrap">
            <input type="file" accept=".apk" @change="onFileChange" />
            <span v-if="form.file" class="file-name">{{ form.file.name }}</span>
            <span v-else class="file-placeholder">اختر ملف APK</span>
          </div>
        </label>

        <div class="form-row">
          <label class="form-field">
            <span>اسم الإصدار</span>
            <div class="input-wrap">
              <input v-model="form.versionName" placeholder="مثال: 1.0.5" />
            </div>
          </label>
          <label class="form-field">
            <span>رقم الإصدار (Version Code)</span>
            <div class="input-wrap">
              <input v-model.number="form.versionCode" type="number" min="1" placeholder="5" />
            </div>
          </label>
        </div>

        <label class="form-field">
          <span>ملاحظات الإصدار</span>
          <div class="input-wrap textarea-wrap">
            <textarea v-model="form.releaseNotes" rows="4" placeholder="سطر واحد لكل ملاحظة"></textarea>
          </div>
        </label>

        <label class="checkbox-field">
          <input v-model="form.isActive" type="checkbox" />
          <span>تفعيل هذا الإصدار فور الرفع</span>
        </label>

        <!-- Upload Progress -->
        <div v-if="uploading" class="progress-bar-wrap">
          <div class="progress-bar">
            <i :style="{ width: `${uploadProgress}%` }"></i>
          </div>
          <small>{{ uploadProgress }}% — جاري رفع الملف...</small>
        </div>

        <button class="compact-primary" :disabled="!canUpload" @click="handleUpload">
          <Upload :size="18" />
          {{ uploading ? 'جاري الرفع...' : 'رفع الإصدار' }}
        </button>
      </div>
    </div>

    <!-- Releases Table -->
    <div class="table-card" style="margin-top: 18px;">
      <div class="table-toolbar">
        <span class="records-count">
          إجمالي الإصدارات: {{ releases.length }}
        </span>
      </div>
      <div class="table-scroll">
        <table v-if="releases.length" class="data-table">
          <thead>
            <tr>
              <th>الإصدار</th>
              <th>رقم الإصدار</th>
              <th>الحجم</th>
              <th>التحميلات</th>
              <th>الحالة</th>
              <th>تاريخ الرفع</th>
              <th>رفع بواسطة</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in releases" :key="r.id">
              <td><strong>{{ r.versionName }}</strong></td>
              <td><span class="status-badge status-neutral">{{ r.versionCode }}</span></td>
              <td class="muted-cell">{{ formatSize(r.fileSize) }}</td>
              <td class="muted-cell">{{ r.downloadCount }}</td>
              <td>
                <span v-if="r.isActive" class="status-badge status-success">نشط</span>
                <span v-else class="status-badge status-neutral">غير نشط</span>
              </td>
              <td class="muted-cell">{{ new Date(r.createdAt).toLocaleDateString('ar-IQ') }}</td>
              <td class="muted-cell">{{ r.createdBy }}</td>
              <td>
                <div class="row-actions">
                  <button v-if="!r.isActive" title="تفعيل" @click="toggleActive(r.id)">
                    <CheckCircle :size="16" />
                  </button>
                  <button v-if="r.isActive" title="إلغاء التفعيل" @click="toggleActive(r.id)">
                    <XCircle :size="16" />
                  </button>
                  <button class="danger-action" title="حذف" @click="deleteRelease(r.id)">
                    <Trash2 :size="16" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <p v-else-if="!loading" class="table-message">لا توجد إصدارات بعد.</p>
        <p v-else class="table-message">
          <LoaderCircle class="spin-icon" :size="20" /> جاري التحميل...
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.success-banner {
  color: #167163;
  background: #e1f4ef;
  border: 1px solid #b5e3d6;
  border-radius: 10px;
  padding: 10px 13px;
  font-size: 14px;
  margin-bottom: 14px;
}

.active-version-card {
  border: 1px solid var(--line);
  border-radius: 14px;
  background: linear-gradient(135deg, #e4f5f1, #f6fdfa);
  box-shadow: var(--shadow);
  padding: 18px;
  margin-bottom: 18px;
}
.active-version-header {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--primary);
  font-weight: 700;
  font-size: 14px;
  margin-bottom: 12px;
}
.active-version-body {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}
.version-meta {
  display: flex;
  align-items: center;
  gap: 10px;
}
.version-meta strong {
  font-size: 22px;
  color: var(--ink);
}
.version-badge {
  padding: 4px 10px;
  border-radius: 20px;
  background: var(--primary-soft);
  color: var(--primary);
  font-size: 12px;
  font-weight: 700;
}
.version-stats {
  display: flex;
  align-items: center;
  gap: 14px;
  color: var(--muted);
  font-size: 13px;
}
.version-stats span {
  display: flex;
  align-items: center;
  gap: 4px;
}
.release-notes-preview {
  margin: 12px 0 0;
  padding: 0 0 0 18px;
  color: var(--muted);
  font-size: 13px;
  line-height: 1.7;
}

.upload-card {
  border: 1px solid var(--line);
  border-radius: 14px;
  background: var(--surface);
  box-shadow: var(--shadow);
}
.upload-header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 16px 18px;
  border-bottom: 1px solid var(--line);
  color: var(--ink);
  font-weight: 700;
  font-size: 15px;
}
.upload-form {
  padding: 18px;
  display: grid;
  gap: 14px;
}
.file-input-wrap {
  display: flex;
  align-items: center;
  gap: 10px;
  height: 52px;
  padding: 0 14px;
  border: 1px dashed var(--line);
  border-radius: 11px;
  background: #fbfdfc;
  cursor: pointer;
  transition: 0.2s;
}
.file-input-wrap:hover {
  border-color: var(--primary);
  background: var(--primary-soft);
}
.file-input-wrap input {
  position: absolute;
  width: 0;
  height: 0;
  opacity: 0;
}
.file-name {
  color: var(--ink);
  font-size: 13px;
  font-weight: 600;
}
.file-placeholder {
  color: var(--muted);
  font-size: 13px;
}
.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
}
.textarea-wrap {
  align-items: flex-start !important;
  height: auto !important;
  padding: 10px 14px !important;
}
.textarea-wrap textarea {
  width: 100%;
  border: 0;
  outline: 0;
  color: var(--ink);
  background: transparent;
  resize: vertical;
  font-family: inherit;
}
.spin-icon {
  animation: spin 1s linear infinite;
  vertical-align: middle;
  margin-inline-end: 6px;
}
@keyframes spin {
  to { transform: rotate(360deg); }
}
.progress-bar-wrap {
  display: grid;
  gap: 6px;
}
.progress-bar-wrap small {
  color: var(--muted);
  font-size: 12px;
}
@media (max-width: 760px) {
  .active-version-body {
    flex-direction: column;
    align-items: flex-start;
  }
  .form-row {
    grid-template-columns: 1fr;
  }
}
</style>
