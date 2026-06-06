<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import {
  BadgeCheck,
  CalendarDays,
  Camera,
  Edit,
  Eye,
  EyeOff,
  FileText,
  MapPin,
  Phone,
  RefreshCw,
  Save,
  Search,
  Stethoscope,
  UserRound,
  X,
} from '@lucide/vue'
import AppModal from '../components/AppModal.vue'
import { provinces } from '../constants/provinces'
import api from '../services/api'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, DoctorItem, SpecializationItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const notifications = useNotificationsStore()
const profile = ref<DoctorItem>()
const specializations = ref<SpecializationItem[]>([])
const loading = ref(false)
const saving = ref(false)
const editOpen = ref(false)
const imagePreview = ref('')
let localPreviewUrl = ''

const form = reactive({
  name: '',
  normalizedName: '',
  specializationId: '',
  description: '',
  iraqiProvince: '0',
  birthDay: '',
  phoneNumber: '',
  location: '',
  imageName: undefined as File | undefined,
})

const apiOrigin = computed(() => new URL(api.defaults.baseURL ?? 'https://localhost:7136/api').origin)
const currentImage = computed(() => {
  if (imagePreview.value) return imagePreview.value
  if (!profile.value?.imageName) return ''
  return `${apiOrigin.value}/DoctorImage/${profile.value.imageName}`
})

function fillForm(doctor: DoctorItem) {
  Object.assign(form, {
    name: doctor.name,
    normalizedName: doctor.normalizedName,
    specializationId: String(doctor.specialization.id),
    description: doctor.description,
    iraqiProvince: String(doctor.iraqiProvince),
    birthDay: doctor.birthDay,
    phoneNumber: doctor.phoneNumber,
    location: doctor.location,
    imageName: undefined,
  })
}

function provinceName(value: number | string) {
  return provinces.find((item) => String(item.value) === String(value))?.name ?? String(value)
}

async function loadProfile() {
  loading.value = true
  try {
    const response = await api.get<ApiResponse<DoctorItem>>('/Doctor/my')
    profile.value = response.data.data
    fillForm(response.data.data)
    clearLocalPreview()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    loading.value = false
  }
}

async function loadSpecializations() {
  try {
    const response = await api.get<ApiResponse<SpecializationItem[]>>('/Specialization')
    specializations.value = response.data.data
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

function clearLocalPreview() {
  if (localPreviewUrl) URL.revokeObjectURL(localPreviewUrl)
  localPreviewUrl = ''
  imagePreview.value = ''
}

function openEditDialog() {
  if (!profile.value) return
  fillForm(profile.value)
  clearLocalPreview()
  editOpen.value = true
}

function closeEditDialog() {
  editOpen.value = false
  if (profile.value) fillForm(profile.value)
  clearLocalPreview()
}

function setImage(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || file.size > 5 * 1024 * 1024) {
    form.imageName = undefined
    clearLocalPreview()
    notifications.show('الصورة يجب أن تكون JPG أو PNG أو WEBP وبحجم لا يتجاوز 5MB.', 'error')
    input.value = ''
    return
  }
  clearLocalPreview()
  form.imageName = file
  localPreviewUrl = URL.createObjectURL(file)
  imagePreview.value = localPreviewUrl
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
    const response = await api.put<ApiResponse<string>>('/Doctor/my', data)
    notifications.show(response.data.message)
    await loadProfile()
    editOpen.value = false
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    saving.value = false
  }
}

onMounted(() => Promise.all([loadProfile(), loadSpecializations()]))
onBeforeUnmount(clearLocalPreview)
</script>

<template>
  <div>
    <div class="page-heading">
      <div>
        <span class="section-kicker">حساب الطبيب</span>
        <h2>الملف الشخصي</h2>
        <p>مراجعة بيانات الطبيب التي تظهر للمرضى، مع تعديلها من نافذة مستقلة عند الحاجة.</p>
      </div>
      <div class="heading-actions">
        <button class="secondary-button" type="button" :disabled="loading" @click="loadProfile">
          <RefreshCw :size="17" /> تحديث
        </button>
        <button v-if="profile" class="compact-primary" type="button" @click="openEditDialog">
          <Edit :size="17" /> تعديل
        </button>
      </div>
    </div>

    <div v-if="loading && !profile" class="empty-panel">جارِ تحميل الملف الشخصي...</div>
    <section v-else-if="profile" class="profile-page">
      <section class="profile-header-card">
        <div class="profile-identity">
          <div class="doctor-photo">
            <img v-if="currentImage" :src="currentImage" :alt="profile.name" />
            <UserRound v-else :size="48" />
          </div>

          <div class="profile-title-block">
            <span class="section-kicker">ملف الطبيب</span>
            <h3>{{ profile.name }}</h3>
            <p>{{ profile.specialization.name }}</p>
          </div>
        </div>

        <div class="profile-header-actions">
          <span class="profile-status" :class="profile.isPubliclyVisible ? 'visible' : 'hidden'">
            <Eye v-if="profile.isPubliclyVisible" :size="15" />
            <EyeOff v-else :size="15" />
            {{ profile.isPubliclyVisible ? 'ظاهر للمرضى' : 'بانتظار الموافقة' }}
          </span>

          <button class="profile-edit-button" type="button" @click="openEditDialog">
            <Edit :size="16" /> تعديل بيانات الملف
          </button>
        </div>
      </section>

      <div class="profile-content">
        <div class="profile-sections-grid">
          <section class="profile-section">
            <div class="profile-section-heading">
              <Stethoscope :size="18" />
              <h3>المعلومات المهنية</h3>
            </div>

            <div class="profile-info-list">
              <div class="profile-info-row">
                <span><Stethoscope :size="16" /> الاختصاص</span>
                <strong>{{ profile.specialization.name }}</strong>
              </div>
              <div class="profile-info-row">
                <span><Search :size="16" /> الاسم المعتمد للبحث</span>
                <strong>{{ profile.normalizedName }}</strong>
              </div>
              <div class="profile-info-row">
                <span><CalendarDays :size="16" /> تاريخ الميلاد</span>
                <strong>{{ profile.birthDay }}</strong>
              </div>
            </div>
          </section>

          <section class="profile-section">
            <div class="profile-section-heading">
              <Phone :size="18" />
              <h3>التواصل والموقع</h3>
            </div>

            <div class="profile-info-list">
              <div class="profile-info-row">
                <span><Phone :size="16" /> رقم الهاتف</span>
                <strong>{{ profile.phoneNumber }}</strong>
              </div>
              <div class="profile-info-row">
                <span><MapPin :size="16" /> المحافظة</span>
                <strong>{{ profile.iraqiProvinceName || provinceName(profile.iraqiProvince) }}</strong>
              </div>
              <div class="profile-info-row">
                <span><MapPin :size="16" /> الموقع المختصر</span>
                <strong>{{ profile.location }}</strong>
              </div>
            </div>
          </section>
        </div>

        <section class="profile-section">
          <div class="profile-section-heading">
            <FileText :size="18" />
            <h3>نبذة الطبيب</h3>
          </div>
          <p class="profile-bio">{{ profile.description }}</p>
        </section>
      </div>

      <div class="profile-note">
        <BadgeCheck :size="18" />
        <span>اعتماد الظهور العام يتم من مدير النظام بعد مراجعة بيانات الملف.</span>
      </div>
    </section>

    <AppModal v-if="editOpen" title="تعديل معلومات الطبيب" wide @close="closeEditDialog">
      <form class="modal-form profile-edit-form" @submit.prevent="saveProfile">
        <div class="form-grid">
          <label><span>اسم الطبيب</span><input v-model="form.name" required maxlength="200" /></label>
          <label><span>الاسم المعتمد للبحث</span><input v-model="form.normalizedName" required maxlength="200" /></label>
          <label><span>الاختصاص</span><select v-model="form.specializationId" required><option disabled value="">اختر الاختصاص</option><option v-for="item in specializations" :key="item.id" :value="item.id">{{ item.name }}</option></select></label>
          <label><span>المحافظة الأساسية</span><select v-model="form.iraqiProvince" required><option v-for="item in provinces" :key="item.value" :value="item.value">{{ item.name }}</option></select></label>
          <label><span>رقم الهاتف</span><input v-model="form.phoneNumber" required maxlength="30" /></label>
          <label><span>تاريخ الميلاد</span><input v-model="form.birthDay" type="date" required /></label>
          <label class="full-field"><span>الموقع المختصر</span><input v-model="form.location" required maxlength="500" placeholder="مثال: بغداد، المنصور" /></label>
          <label class="full-field"><span>نبذة عن الطبيب والخبرة</span><textarea v-model="form.description" rows="5" required maxlength="2000" /></label>
          <label class="full-field image-upload">
            <span>الصورة الشخصية</span>
            <input type="file" accept=".jpg,.jpeg,.png,.webp" @change="setImage" />
            <small><Camera :size="15" /> اختر صورة جديدة فقط إذا أردت استبدال الحالية. الحد الأقصى 5MB.</small>
          </label>
        </div>

        <div class="modal-actions">
          <button class="secondary-button" type="button" :disabled="saving" @click="closeEditDialog">
            <X :size="17" /> إلغاء
          </button>
          <button class="compact-primary" type="submit" :disabled="saving">
            <Save :size="17" /> {{ saving ? 'جارِ الحفظ...' : 'حفظ التعديلات' }}
          </button>
        </div>
      </form>
    </AppModal>
  </div>
</template>

<style scoped>
.profile-page { display: grid; gap: 14px; }
.profile-header-card, .profile-section, .profile-note { border: 1px solid var(--line); border-radius: 14px; background: #fff; box-shadow: var(--shadow); }
.profile-header-card { display: flex; justify-content: space-between; align-items: center; gap: 18px; padding: 18px; }
.profile-identity { display: flex; align-items: center; gap: 15px; min-width: 0; }
.doctor-photo { display: grid; place-items: center; flex: 0 0 auto; width: 82px; height: 82px; overflow: hidden; color: var(--primary); border: 1px solid #cfe4e0; border-radius: 18px; background: var(--primary-soft); }
.doctor-photo img { width: 100%; height: 100%; object-fit: cover; }
.profile-title-block { min-width: 0; }
.profile-title-block h3 { margin: 5px 0 3px; color: var(--ink); font-size: 22px; line-height: 1.35; overflow-wrap: anywhere; }
.profile-title-block p { margin: 0; color: var(--muted); font-size: 13px; font-weight: 700; }
.profile-header-actions { display: flex; align-items: center; gap: 9px; flex-wrap: wrap; justify-content: flex-end; }
.profile-status { display: inline-flex; align-items: center; gap: 6px; padding: 7px 10px; border-radius: 18px; color: #167163; background: #e1f4ef; font-size: 12px; font-weight: 800; white-space: nowrap; }
.profile-status.hidden { color: #a46724; background: #fff1db; }
.profile-edit-button { display: flex; align-items: center; justify-content: center; gap: 7px; padding: 10px 12px; color: var(--primary); border: 1px solid #b9dad5; border-radius: 9px; background: #fff; font-weight: 800; white-space: nowrap; }
.profile-edit-button:hover { border-color: #8dcac1; background: var(--primary-soft); }
.profile-note { display: flex; gap: 9px; padding: 13px 15px; color: #627d79; background: #fbfdfc; font-size: 12px; line-height: 1.7; }
.profile-note svg { flex: 0 0 auto; color: var(--primary); }
.profile-content { display: grid; gap: 14px; }
.profile-sections-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 14px; }
.profile-section { padding: 18px; }
.profile-section-heading { display: flex; align-items: center; gap: 8px; margin-bottom: 14px; padding-bottom: 12px; border-bottom: 1px solid var(--line); }
.profile-section-heading svg { color: var(--primary); }
.profile-section-heading h3 { margin: 0; color: var(--ink); font-size: 18px; }
.profile-info-list { display: grid; gap: 2px; }
.profile-info-row { display: grid; grid-template-columns: 185px minmax(0, 1fr); gap: 12px; align-items: center; padding: 12px 0; border-bottom: 1px solid #eef4f3; }
.profile-info-row:last-child { border-bottom: 0; }
.profile-info-row span { display: flex; align-items: center; gap: 7px; color: var(--muted); font-size: 12px; font-weight: 800; }
.profile-info-row span svg { flex: 0 0 auto; color: #7aa9a2; }
.profile-info-row strong { color: var(--ink); font-size: 14px; font-weight: 800; line-height: 1.6; overflow-wrap: anywhere; }
.profile-bio { margin: 0; color: #58706c; line-height: 1.9; overflow-wrap: anywhere; }
.profile-edit-form .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 13px; }
.image-upload small { display: flex; align-items: center; gap: 5px; margin-top: 7px; color: var(--muted); font-size: 12px; }
@media (max-width: 1100px) {
  .profile-sections-grid { grid-template-columns: 1fr; }
}
@media (max-width: 850px) {
  .profile-header-card { align-items: flex-start; flex-direction: column; }
  .profile-header-actions { justify-content: flex-start; width: 100%; }
  .profile-edit-button { flex: 1; }
  .profile-edit-form .form-grid { grid-template-columns: 1fr; }
  .profile-info-row { grid-template-columns: 1fr; gap: 5px; }
}
@media (max-width: 560px) {
  .profile-identity { align-items: flex-start; }
  .doctor-photo { width: 66px; height: 66px; border-radius: 15px; }
  .profile-title-block h3 { font-size: 19px; }
  .profile-header-actions { display: grid; grid-template-columns: 1fr; }
}
</style>
