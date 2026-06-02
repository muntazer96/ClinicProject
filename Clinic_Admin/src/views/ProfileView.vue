<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { BadgeCheck, Camera, Eye, EyeOff, RefreshCw, Save, Stethoscope, UserRound } from '@lucide/vue'
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
        <p>حدّث بياناتك الطبية والصورة التي تظهر للمرضى في دليل الأطباء.</p>
      </div>
      <button class="secondary-button" type="button" :disabled="loading" @click="loadProfile">
        <RefreshCw :size="17" /> تحديث
      </button>
    </div>

    <div v-if="loading && !profile" class="empty-panel">جارِ تحميل الملف الشخصي...</div>
    <section v-else-if="profile" class="profile-layout">
      <aside class="doctor-profile-card">
        <div class="profile-cover" />
        <div class="doctor-photo">
          <img v-if="currentImage" :src="currentImage" :alt="profile.name" />
          <UserRound v-else :size="48" />
        </div>
        <div class="doctor-profile-copy">
          <span class="section-kicker">ملف الطبيب</span>
          <h3>{{ profile.name }}</h3>
          <p>{{ profile.specialization.name }}</p>
          <span class="profile-visibility" :class="profile.isPubliclyVisible ? 'visible' : 'hidden'">
            <Eye v-if="profile.isPubliclyVisible" :size="15" />
            <EyeOff v-else :size="15" />
            {{ profile.isPubliclyVisible ? 'ظاهر للمرضى' : 'بانتظار الموافقة على الظهور' }}
          </span>
        </div>
        <div class="profile-note">
          <BadgeCheck :size="18" />
          <span>اعتماد الظهور العام يتم من مدير النظام بعد مراجعة بيانات الملف.</span>
        </div>
      </aside>

      <form class="profile-form-card" @submit.prevent="saveProfile">
        <div class="profile-form-heading">
          <div>
            <span class="section-kicker">البيانات الأساسية</span>
            <h3>تعديل معلومات الطبيب</h3>
          </div>
          <Stethoscope :size="23" />
        </div>

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
          <button class="compact-primary" type="submit" :disabled="saving">
            <Save :size="17" /> {{ saving ? 'جارِ الحفظ...' : 'حفظ التعديلات' }}
          </button>
        </div>
      </form>
    </section>
  </div>
</template>

<style scoped>
.profile-layout { display: grid; grid-template-columns: 290px minmax(0, 1fr); gap: 15px; align-items: start; }
.doctor-profile-card, .profile-form-card { overflow: hidden; border: 1px solid var(--line); border-radius: 15px; background: #fff; box-shadow: var(--shadow); }
.profile-cover { height: 94px; background: linear-gradient(125deg, var(--primary-dark), #36a294); }
.doctor-photo { display: grid; place-items: center; width: 94px; height: 94px; margin: -47px auto 0; overflow: hidden; color: var(--primary); border: 5px solid #fff; border-radius: 27px; background: var(--primary-soft); }
.doctor-photo img { width: 100%; height: 100%; object-fit: cover; }
.doctor-profile-copy { padding: 15px 16px 17px; text-align: center; }
.doctor-profile-copy h3 { margin: 6px 0 3px; font-size: 20px; }.doctor-profile-copy p { margin: 0; color: var(--muted); font-size: 13px; }
.profile-visibility { display: inline-flex; align-items: center; gap: 5px; margin-top: 14px; padding: 6px 9px; border-radius: 16px; font-size: 11px; font-weight: 700; }
.profile-visibility.visible { color: #167163; background: #e1f4ef; }.profile-visibility.hidden { color: #a46724; background: #fff1db; }
.profile-note { display: flex; gap: 8px; padding: 13px; color: #627d79; border-top: 1px solid var(--line); background: #fbfdfc; font-size: 12px; line-height: 1.7; }
.profile-note svg { flex: 0 0 auto; color: var(--primary); }
.profile-form-card { padding: 18px; }.profile-form-heading { display: flex; justify-content: space-between; align-items: center; margin-bottom: 18px; padding-bottom: 13px; border-bottom: 1px solid var(--line); }
.profile-form-heading h3 { margin: 5px 0 0; font-size: 19px; }.profile-form-heading svg { color: var(--primary); }
.profile-form-card label span { display: block; margin-bottom: 6px; color: var(--ink); font-size: 13px; font-weight: 700; }
.profile-form-card input, .profile-form-card select, .profile-form-card textarea { width: 100%; padding: 10px; color: var(--ink); border: 1px solid var(--line); border-radius: 8px; background: #fff; }
.image-upload small { display: flex; align-items: center; gap: 5px; margin-top: 7px; color: var(--muted); font-size: 12px; }
@media (max-width: 850px) { .profile-layout { grid-template-columns: 1fr; }.doctor-profile-card { max-width: none; } }
</style>
