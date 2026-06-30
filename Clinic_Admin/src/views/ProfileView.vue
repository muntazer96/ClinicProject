<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { provinces } from '../constants/provinces'
import api from '../services/api'
import { useNotifications } from '../composables/useNotifications'
import type { ApiResponse, DoctorItem, SpecializationItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import EmptyState from '../components/common/Emptystate.vue'

const { success: showSuccess, error: showError } = useNotifications()

const profile         = ref<DoctorItem>()
const specializations = ref<SpecializationItem[]>([])
const loading         = ref(false)
const saving          = ref(false)
const editOpen        = ref(false)
const imagePreview    = ref('')
const doctorImage     = ref('')
const fileInput       = ref<HTMLInputElement>()
let localPreviewUrl   = ''
let doctorImageBlobUrl = ''

const form = reactive({
  name: '', normalizedName: '', specializationId: '',
  description: '', iraqiProvince: '0', birthDay: '',
  phoneNumber: '', location: '', imageName: undefined as File | undefined,
})

const currentImage = computed(() => {
  if (imagePreview.value) return imagePreview.value
  return doctorImage.value
})

function provinceName(value: number | string) {
  return provinces.find((p) => String(p.value) === String(value))?.name ?? String(value)
}

function fillForm(doctor: DoctorItem) {
  Object.assign(form, {
    name: doctor.name, normalizedName: doctor.normalizedName,
    specializationId: String(doctor.specialization.id),
    description: doctor.description,
    iraqiProvince: String(doctor.iraqiProvince),
    birthDay: doctor.birthDay, phoneNumber: doctor.phoneNumber,
    location: doctor.location, imageName: undefined,
  })
}

function clearLocalPreview() {
  if (localPreviewUrl) URL.revokeObjectURL(localPreviewUrl)
  localPreviewUrl = ''
  imagePreview.value = ''
}

function clearDoctorImage() {
  if (doctorImageBlobUrl) {
    URL.revokeObjectURL(doctorImageBlobUrl)
    doctorImageBlobUrl = ''
  }
  doctorImage.value = ''
}

async function loadDoctorImage(path?: string): Promise<string> {
  if (!path) return ''
  const fileName = path.replace('/DoctorImage/', '')
  const normalizedFileName = (() => {
    try {
      return decodeURIComponent(fileName)
    } catch {
      return fileName
    }
  })()

  try {
    const res = await api.get(`/Files/doctor-image/${encodeURIComponent(normalizedFileName)}`, {
      responseType: 'blob',
    })
    doctorImageBlobUrl = URL.createObjectURL(res.data)
    return doctorImageBlobUrl
  } catch {
    return ''
  }
}

function setImage(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || file.size > 5 * 1024 * 1024) {
    form.imageName = undefined
    clearLocalPreview()
    showError('الصورة يجب أن تكون JPG أو PNG أو WEBP وبحجم لا يتجاوز 5MB.')
    input.value = ''
    return
  }
  clearLocalPreview()
  form.imageName = file
  localPreviewUrl = URL.createObjectURL(file)
  imagePreview.value = localPreviewUrl
}

async function loadProfile() {
  loading.value = true
  clearDoctorImage()
  try {
    const r = await api.get<ApiResponse<DoctorItem>>('/Doctor/my')
    profile.value = r.data.data
    fillForm(r.data.data)
    clearLocalPreview()
    doctorImage.value = await loadDoctorImage(r.data.data.imageName)
  } catch (e) { showError(getErrorMessage(e)) }
  finally { loading.value = false }
}

async function loadSpecializations() {
  try {
    const r = await api.get<ApiResponse<SpecializationItem[]>>('/Specialization')
    specializations.value = r.data.data
  } catch (e) { showError(getErrorMessage(e)) }
}

function openEdit() {
  if (!profile.value) return
  fillForm(profile.value)
  clearLocalPreview()
  editOpen.value = true
}

function closeEdit() {
  editOpen.value = false
  if (profile.value) fillForm(profile.value)
  clearLocalPreview()
}

async function saveProfile() {
  saving.value = true
  try {
    const data = new FormData()
    data.append('Name', form.name)
    data.append('NormalizedName', form.normalizedName)
    data.append('SpecializationId', form.specializationId)
    data.append('Description', form.description)
    data.append('IraqiProvince', form.iraqiProvince)
    data.append('BirthDay', form.birthDay)
    data.append('PhoneNumber', form.phoneNumber)
    data.append('Location', form.location)
    if (form.imageName) data.append('ImageName', form.imageName)
    const r = await api.put<ApiResponse<string>>('/Doctor/my', data)
    showSuccess(r.data.message)
    await loadProfile()
    editOpen.value = false
  } catch (e) { showError(getErrorMessage(e)) }
  finally { saving.value = false }
}

onMounted(() => Promise.all([loadProfile(), loadSpecializations()]))
onBeforeUnmount(() => {
  clearLocalPreview()
  clearDoctorImage()
})
</script>

<template>
  <div class="profile-page">

    <!-- Header -->
    <div class="page-top">
      <div>
        <p class="page-kicker">حساب الطبيب</p>
        <h1 class="page-title">الملف الشخصي</h1>
      </div>
      <div class="page-actions">
        <v-btn variant="outlined" color="primary" prepend-icon="mdi-refresh" :loading="loading" @click="loadProfile">
          تحديث
        </v-btn>
        <v-btn v-if="profile" color="primary" prepend-icon="mdi-pencil" @click="openEdit">
          تعديل الملف
        </v-btn>
      </div>
    </div>

    <!-- Loading -->
    <v-skeleton-loader v-if="loading && !profile" type="card" height="200" />

    <!-- Empty -->
    <EmptyState
      v-else-if="!profile"
      icon="mdi-account-off"
      title="لم يتم تحميل الملف"
      description="تعذّر تحميل بيانات الملف الشخصي"
    />

    <!-- Profile Content -->
    <template v-else>

      <!-- Hero Card -->
      <div class="hero-card">
        <!-- Avatar -->
        <div class="hero-avatar">
          <img v-if="currentImage" :src="currentImage" :alt="profile.name" />
          <v-icon v-else icon="mdi-account" size="48" color="primary" />
        </div>

        <!-- Info -->
        <div class="hero-info">
          <p class="hero-kicker">ملف الطبيب</p>
          <h2 class="hero-name">{{ profile.name }}</h2>
          <p class="hero-spec">{{ profile.specialization.name }}</p>
          <div class="hero-badges">
            <v-chip
              size="small"
              :color="profile.isPubliclyVisible ? 'success' : 'warning'"
              variant="tonal"
            >
              <v-icon :icon="profile.isPubliclyVisible ? 'mdi-eye' : 'mdi-eye-off'" size="14" start />
              {{ profile.isPubliclyVisible ? 'ظاهر للمرضى' : 'بانتظار الموافقة' }}
            </v-chip>
          </div>
        </div>

        <!-- Edit Button -->
        <v-btn color="primary" variant="outlined" prepend-icon="mdi-pencil" class="hero-edit-btn" @click="openEdit">
          تعديل بيانات الملف
        </v-btn>
      </div>

      <!-- Info Grid -->
      <div class="info-grid">

        <!-- Professional Info -->
        <div class="info-card">
          <div class="info-card-header">
            <v-icon icon="mdi-stethoscope" color="primary" size="20" />
            <h3>المعلومات المهنية</h3>
          </div>
          <div class="info-rows">
            <div class="info-row">
              <span class="info-key">
                <v-icon icon="mdi-stethoscope" size="15" color="primary" />
                الاختصاص
              </span>
              <strong>{{ profile.specialization.name }}</strong>
            </div>
            <div class="info-row">
              <span class="info-key">
                <v-icon icon="mdi-magnify" size="15" color="primary" />
                اسم البحث
              </span>
              <strong>{{ profile.normalizedName }}</strong>
            </div>
            <div class="info-row">
              <span class="info-key">
                <v-icon icon="mdi-calendar" size="15" color="primary" />
                تاريخ الميلاد
              </span>
              <strong>{{ profile.birthDay }}</strong>
            </div>
          </div>
        </div>

        <!-- Contact Info -->
        <div class="info-card">
          <div class="info-card-header">
            <v-icon icon="mdi-phone" color="primary" size="20" />
            <h3>التواصل والموقع</h3>
          </div>
          <div class="info-rows">
            <div class="info-row">
              <span class="info-key">
                <v-icon icon="mdi-phone" size="15" color="primary" />
                رقم الهاتف
              </span>
              <strong>{{ profile.phoneNumber }}</strong>
            </div>
            <div class="info-row">
              <span class="info-key">
                <v-icon icon="mdi-map-marker" size="15" color="primary" />
                المحافظة
              </span>
              <strong>{{ profile.iraqiProvinceName || provinceName(profile.iraqiProvince) }}</strong>
            </div>
            <div class="info-row">
              <span class="info-key">
                <v-icon icon="mdi-map-marker-outline" size="15" color="primary" />
                الموقع
              </span>
              <strong>{{ profile.location }}</strong>
            </div>
          </div>
        </div>

      </div>

      <!-- Bio Card -->
      <div class="info-card">
        <div class="info-card-header">
          <v-icon icon="mdi-text" color="primary" size="20" />
          <h3>نبذة الطبيب</h3>
        </div>
        <p class="bio-text">{{ profile.description }}</p>
      </div>

      <!-- Note -->
      <v-alert type="info" variant="tonal" icon="mdi-shield-check" density="compact">
        اعتماد الظهور العام يتم من مدير النظام بعد مراجعة بيانات الملف.
      </v-alert>

    </template>

    <!-- ── Edit Dialog ── -->
    <v-dialog v-model="editOpen" max-width="700" scrollable>
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-pencil" color="primary" size="20" />
          تعديل معلومات الطبيب
        </v-card-title>
        <v-divider />

        <v-card-text class="dialog-body">
          <div class="form-grid">
            <!-- Name -->
            <div class="form-field">
              <label class="form-label">اسم الطبيب <span class="required">*</span></label>
              <input v-model="form.name" class="form-input" required maxlength="200" />
            </div>
            <!-- Normalized Name -->
            <div class="form-field">
              <label class="form-label">الاسم المعتمد للبحث <span class="required">*</span></label>
              <input v-model="form.normalizedName" class="form-input" required maxlength="200" />
            </div>
            <!-- Specialization -->
            <div class="form-field">
              <label class="form-label">الاختصاص <span class="required">*</span></label>
              <v-autocomplete
                v-model="form.specializationId"
                :items="specializations.map(s => ({ value: s.id, label: s.name }))"
                item-title="label"
                item-value="value"
                class="form-select"
                density="compact"
                variant="outlined"
                hide-details
                placeholder="اختر الاختصاص"
              />
            </div>
            <!-- Province -->
            <div class="form-field">
              <label class="form-label">المحافظة <span class="required">*</span></label>
              <v-autocomplete
                v-model="form.iraqiProvince"
                :items="provinces.map(p => ({ value: p.value, label: p.name }))"
                item-title="label"
                item-value="value"
                class="form-select"
                density="compact"
                variant="outlined"
                hide-details
              />
            </div>
            <!-- Phone -->
            <div class="form-field">
              <label class="form-label">رقم الهاتف <span class="required">*</span></label>
              <input v-model="form.phoneNumber" class="form-input" required maxlength="30" />
            </div>
            <!-- Birth Day -->
            <div class="form-field">
              <label class="form-label">تاريخ الميلاد <span class="required">*</span></label>
              <input v-model="form.birthDay" type="date" class="form-input" required />
            </div>
            <!-- Location -->
            <div class="form-field form-field--full">
              <label class="form-label">الموقع المختصر <span class="required">*</span></label>
              <input v-model="form.location" class="form-input" required maxlength="500" placeholder="مثال: بغداد، المنصور" />
            </div>
            <!-- Description -->
            <div class="form-field form-field--full">
              <label class="form-label">نبذة عن الطبيب والخبرة <span class="required">*</span></label>
              <textarea v-model="form.description" class="form-textarea" rows="5" required maxlength="2000" />
            </div>
            <!-- Image -->
            <div class="form-field form-field--full">
              <label class="form-label">الصورة الشخصية</label>
              <div class="file-zone" @click="fileInput?.click()">
                <input ref="fileInput" type="file" accept=".jpg,.jpeg,.png,.webp" class="file-hidden" @change="setImage" />
                <v-icon
                  :icon="form.imageName ? 'mdi-image-check' : 'mdi-camera'"
                  :color="form.imageName ? 'success' : 'primary'"
                  size="26"
                />
                <div>
                  <strong>{{ form.imageName ? form.imageName.name : 'اختر صورة جديدة' }}</strong>
                  <span>اختر فقط إذا أردت استبدال الحالية • JPG أو PNG أو WEBP • بحد أقصى 5MB</span>
                </div>
              </div>
              <!-- Current Image Preview -->
              <div v-if="currentImage" class="image-preview-wrap">
                <img :src="currentImage" alt="معاينة" class="image-preview" />
                <span class="image-preview-label">{{ form.imageName ? 'الصورة الجديدة' : 'الصورة الحالية' }}</span>
              </div>
            </div>
          </div>
        </v-card-text>

        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" :disabled="saving" @click="closeEdit">إلغاء</v-btn>
          <v-btn color="primary" prepend-icon="mdi-content-save" :loading="saving" @click="saveProfile">
            حفظ التعديلات
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </div>
</template>

<style scoped>
.profile-page { display: flex; flex-direction: column; gap: var(--spacing-lg); }

/* Page Top */
.page-top { display: flex; align-items: center; justify-content: space-between; gap: var(--spacing-lg); flex-wrap: wrap; }
.page-kicker { margin: 0 0 4px 0; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--color-text-muted); }
.page-title { margin: 0; font-size: 28px; font-weight: 800; color: var(--color-text); }
.page-actions { display: flex; gap: var(--spacing-md); }

/* Hero Card */
.hero-card {
  display: flex;
  align-items: center;
  gap: var(--spacing-xl);
  padding: var(--spacing-xl);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  flex-wrap: wrap;
}

.hero-avatar {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 90px;
  height: 90px;
  border-radius: var(--radius-lg);
  border: 2px solid var(--color-border);
  background: var(--color-primary-soft);
  overflow: hidden;
  flex-shrink: 0;
}

.hero-avatar img { width: 100%; height: 100%; object-fit: cover; }

.hero-info { flex: 1; min-width: 0; }

.hero-kicker {
  margin: 0 0 4px 0;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-muted);
}

.hero-name {
  margin: 0 0 4px 0;
  font-size: 24px;
  font-weight: 800;
  color: var(--color-text);
  line-height: 1.2;
}

.hero-spec {
  margin: 0 0 var(--spacing-md) 0;
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text-muted);
}

.hero-badges { display: flex; gap: var(--spacing-sm); flex-wrap: wrap; }

.hero-edit-btn { flex-shrink: 0; }

/* Info Grid */
.info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-lg);
}

.info-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  box-shadow: var(--shadow-sm);
}

.info-card-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-lg);
  padding-bottom: var(--spacing-md);
  border-bottom: 1px solid var(--color-border);
}

.info-card-header h3 {
  margin: 0;
  font-size: 15px;
  font-weight: 700;
  color: var(--color-text);
}

.info-rows { display: flex; flex-direction: column; }

.info-row {
  display: grid;
  grid-template-columns: 160px 1fr;
  gap: var(--spacing-lg);
  align-items: center;
  padding: var(--spacing-md) 0;
  border-bottom: 1px solid var(--color-border-light);
}

.info-row:last-child { border-bottom: none; }

.info-key {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-muted);
}

.info-row strong {
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text);
  overflow-wrap: anywhere;
}

.bio-text {
  margin: 0;
  font-size: 14px;
  color: var(--color-text-muted);
  line-height: 1.9;
  overflow-wrap: anywhere;
}

/* Dialog */
.dialog-title { display: flex; align-items: center; gap: var(--spacing-md); padding: var(--spacing-lg) !important; font-size: 16px !important; font-weight: 700; color: var(--color-text); }
.dialog-body { padding: var(--spacing-lg) !important; }
.dialog-actions { padding: var(--spacing-lg) !important; gap: var(--spacing-md); justify-content: flex-end; }

/* Form */
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--spacing-lg); }
.form-field { display: flex; flex-direction: column; gap: 6px; }
.form-field--full { grid-column: 1 / -1; }
.form-label { font-size: 13px; font-weight: 600; color: var(--color-text); }
.required { color: var(--color-error); }
.form-input, .form-select, .form-textarea {
  padding: 10px 12px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface); color: var(--color-text);
  font-family: var(--font-family-primary); font-size: 14px;
  outline: none; width: 100%; transition: border-color 0.2s;
}
.form-input:focus, .form-select:focus, .form-textarea:focus { border-color: var(--color-primary); }
.form-textarea { resize: vertical; }

/* File Zone */
.file-zone {
  display: flex; align-items: center; gap: var(--spacing-lg);
  padding: var(--spacing-lg);
  border: 2px dashed var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-background);
  cursor: pointer; transition: all 0.2s ease;
}
.file-zone:hover { border-color: var(--color-primary); background: var(--color-primary-soft); }
.file-hidden { display: none; }
.file-zone strong { display: block; font-size: 14px; font-weight: 600; color: var(--color-text); margin-bottom: 2px; }
.file-zone span { font-size: 12px; color: var(--color-text-muted); }

/* Image Preview */
.image-preview-wrap {
  display: flex; align-items: center; gap: var(--spacing-md);
  margin-top: var(--spacing-md);
}
.image-preview {
  width: 88px; height: 88px;
  object-fit: cover;
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border);
}
.image-preview-label { font-size: 12px; color: var(--color-text-muted); font-weight: 600; }

/* Responsive */
@media (max-width: 1000px) { .info-grid { grid-template-columns: 1fr; } }
@media (max-width: 768px) {
  .hero-card { flex-direction: column; align-items: flex-start; }
  .hero-edit-btn { width: 100%; }
  .form-grid { grid-template-columns: 1fr; }
  .info-row { grid-template-columns: 1fr; gap: var(--spacing-sm); }
}
</style>
