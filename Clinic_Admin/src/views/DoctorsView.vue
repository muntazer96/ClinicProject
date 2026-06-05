<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { Eye, EyeOff, FileText, Link2, Pencil, Plus, RefreshCw, Search, Stethoscope, Trash2, Unlink, UserRound } from '@lucide/vue'
import { RouterLink } from 'vue-router'
import AppModal from '../components/AppModal.vue'
import AppPagination from '../components/AppPagination.vue'
import { provinces } from '../constants/provinces'
import api from '../services/api'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, DoctorItem, PageResult, SpecializationItem, UserItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'

type Confirmation = { title: string; text: string; action: () => Promise<void> }

const notifications = useNotificationsStore()
const doctors = ref<DoctorItem[]>([])
const specializations = ref<SpecializationItem[]>([])
const loading = ref(false)
const saving = ref(false)
const page = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)
const editorOpen = ref(false)
const linkDoctor = ref<DoctorItem>()
const userId = ref('')
const linkSearch = ref('')
const linkUsers = ref<UserItem[]>([])
const linkLoading = ref(false)
const imagePreview = ref('')
let localPreviewUrl = ''
const confirmation = ref<Confirmation>()
const filters = reactive({ name: '', specialization: '', iraqiProvince: '' })
const form = reactive({ id: 0, name: '', normalizedName: '', specializationId: '', description: '', iraqiProvince: '0', birthDay: '', phoneNumber: '', location: '', imageName: undefined as File | undefined })
const isEditing = computed(() => Boolean(form.id))

async function loadDoctors() {
  loading.value = true
  try {
    const response = await api.get<ApiResponse<PageResult<DoctorItem>>>('/Doctor', {
      params: { name: filters.name || undefined, specialization: filters.specialization || undefined, iraqiProvince: filters.iraqiProvince === '' ? undefined : filters.iraqiProvince, page: page.value, pageSize: 10 },
    })
    doctors.value = response.data.data.items
    totalPages.value = response.data.data.totalPages
    totalItems.value = response.data.data.totalItems
  } catch (error: any) {
    if (error.response?.status === 404) {
      doctors.value = []
      totalPages.value = 1
      totalItems.value = 0
    } else notifications.show(getErrorMessage(error), 'error')
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

function resetForm(doctor?: DoctorItem) {
  Object.assign(form, doctor ? {
    id: doctor.id, name: doctor.name, normalizedName: doctor.normalizedName, specializationId: String(doctor.specialization.id),
    description: doctor.description, iraqiProvince: String(doctor.iraqiProvince), birthDay: doctor.birthDay,
    phoneNumber: doctor.phoneNumber, location: doctor.location, imageName: undefined,
  } : { id: 0, name: '', normalizedName: '', specializationId: '', description: '', iraqiProvince: '0', birthDay: '', phoneNumber: '', location: '', imageName: undefined })
  editorOpen.value = true
  clearImagePreview()
}

function setImage(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || file.size > 5 * 1024 * 1024) {
    form.imageName = undefined
    clearImagePreview()
    notifications.show('الصورة يجب أن تكون JPG أو PNG أو WEBP وبحجم لا يتجاوز 5MB.', 'error')
    input.value = ''
    return
  }
  clearImagePreview()
  form.imageName = file
  localPreviewUrl = URL.createObjectURL(file)
  imagePreview.value = localPreviewUrl
}

function clearImagePreview() {
  if (localPreviewUrl) URL.revokeObjectURL(localPreviewUrl)
  localPreviewUrl = ''
  imagePreview.value = ''
}

async function saveDoctor() {
  if (!isEditing.value && !form.imageName) {
    notifications.show('صورة الطبيب مطلوبة عند إنشاء السجل.', 'error')
    return
  }
  saving.value = true
  try {
    const data = new FormData()
    if (isEditing.value) data.append('Id', String(form.id))
    data.append('Name', form.name)
    data.append('NormalizedName', form.normalizedName)
    data.append('SpecializationId', form.specializationId)
    data.append('Description', form.description)
    data.append('IraqiProvince', form.iraqiProvince)
    data.append('BirthDay', form.birthDay)
    data.append('PhoneNumber', form.phoneNumber)
    data.append('Location', form.location)
    if (form.imageName) data.append('ImageName', form.imageName)
    const response = isEditing.value
      ? await api.put<ApiResponse<string>>('/Doctor', data)
      : await api.post<ApiResponse<string>>('/Doctor', data)
    notifications.show(response.data.message)
    editorOpen.value = false
    await loadDoctors()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    saving.value = false
  }
}

async function toggleVisibility(doctor: DoctorItem) {
  try {
    const response = await api.put<ApiResponse<string>>(`/Doctor/${doctor.id}/visibility`, { isPubliclyVisible: !doctor.isPubliclyVisible })
    notifications.show(response.data.message)
    await loadDoctors()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

async function linkAccount() {
  if (!linkDoctor.value) return
  try {
    const response = await api.post<ApiResponse<string>>(`/Doctor/${linkDoctor.value.id}/link-account`, { userId: userId.value })
    notifications.show(response.data.message)
    linkDoctor.value = undefined
    userId.value = ''
    await loadDoctors()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

function openLinkAccount(doctor: DoctorItem) {
  linkDoctor.value = doctor
  userId.value = ''
  linkSearch.value = ''
  linkUsers.value = []
}

async function searchLinkUsers() {
  linkLoading.value = true
  try {
    const response = await api.get<ApiResponse<PageResult<UserItem>>>('/User', {
      params: { search: linkSearch.value.trim() || undefined, page: 1, pageSize: 8 },
    })
    linkUsers.value = response.data.data.items.filter((user) => user.userName?.toLocaleLowerCase() !== 'superadmin' && !user.isLocked)
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    linkLoading.value = false
  }
}

function askDelete(doctor: DoctorItem) {
  confirmation.value = { title: 'تأكيد حذف الطبيب', text: `سيتم حذف سجل الطبيب ${doctor.name}.`, action: async () => {
    const response = await api.delete<ApiResponse<string>>(`/Doctor/${doctor.id}`)
    notifications.show(response.data.message)
    await loadDoctors()
  } }
}

function askUnlink(doctor: DoctorItem) {
  confirmation.value = { title: 'فصل حساب الطبيب', text: `سيتم فصل حساب الدخول المرتبط بالطبيب ${doctor.name}.`, action: async () => {
    const response = await api.delete<ApiResponse<string>>(`/Doctor/${doctor.id}/link-account`)
    notifications.show(response.data.message)
    await loadDoctors()
  } }
}

async function runConfirmation() {
  if (!confirmation.value) return
  try {
    await confirmation.value.action()
    confirmation.value = undefined
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

function applyFilters() {
  page.value = 1
  loadDoctors()
}

function changePage(newPage: number) {
  page.value = newPage
  loadDoctors()
}

onMounted(() => Promise.all([loadDoctors(), loadSpecializations()]))
onBeforeUnmount(clearImagePreview)
</script>

<template>
  <div>
    <div class="page-heading">
      <div><span class="section-kicker">دليل الأطباء</span><h2>الأطباء</h2><p>أنشئ سجلات الأطباء واربط حساباتهم وتحكم بظهورهم للعامة.</p></div>
      <div class="heading-actions"><button class="secondary-button" type="button" :disabled="loading" @click="loadDoctors"><RefreshCw :size="17" /> تحديث</button><button class="compact-primary" type="button" @click="resetForm()"><Plus :size="17" /> إضافة طبيب</button></div>
    </div>

    <section class="filter-card">
      <label class="search-box"><Search :size="18" /><input v-model="filters.name" placeholder="اسم الطبيب" @keyup.enter="applyFilters" /></label>
      <select v-model="filters.specialization"><option value="">كل الاختصاصات</option><option v-for="item in specializations" :key="item.id" :value="item.id">{{ item.name }}</option></select>
      <select v-model="filters.iraqiProvince"><option value="">كل المحافظات</option><option v-for="province in provinces" :key="province.value" :value="province.value">{{ province.name }}</option></select>
      <button class="compact-primary" type="button" @click="applyFilters">تطبيق</button>
    </section>

    <section class="table-card">
      <div class="table-toolbar"><span class="records-count"><Stethoscope :size="16" /> {{ totalItems }} طبيب</span></div>
      <div class="table-scroll">
        <table class="data-table">
          <thead><tr><th>الطبيب</th><th>الاختصاص والمحافظة</th><th>التواصل</th><th>الحساب المرتبط</th><th>الظهور</th><th>الإجراءات</th></tr></thead>
          <tbody>
            <tr v-if="loading"><td colspan="6" class="table-message">جارِ تحميل الأطباء...</td></tr>
            <tr v-else-if="!doctors.length"><td colspan="6" class="table-message">لا توجد سجلات مطابقة.</td></tr>
            <tr v-for="doctor in doctors" v-else :key="doctor.id">
              <td><div class="entity-cell"><span class="small-avatar"><Stethoscope :size="17" /></span><div><strong>{{ doctor.name }}</strong><small>{{ doctor.normalizedName }}</small></div></div></td>
              <td><strong>{{ doctor.specialization.name }}</strong><small class="block-muted">{{ doctor.iraqiProvinceName }}</small></td>
              <td><strong>{{ doctor.phoneNumber }}</strong><small class="block-muted">{{ doctor.location }}</small></td>
              <td>
                <div v-if="doctor.linkedUser" class="linked-account-cell">
                  <!-- <span class="status-badge status-success">مربوط</span>-->
                  <strong>{{ doctor.linkedUser.name || doctor.linkedUser.userName }}</strong>
                  <small>{{ doctor.linkedUser.phoneNumber || doctor.linkedUser.email || doctor.linkedUser.id }}</small>
                </div>
                <span v-else class="status-badge status-neutral">غير مربوط</span>
              </td>
              <td><button class="status-button" type="button" :class="doctor.isPubliclyVisible ? 'visible-status' : ''" @click="toggleVisibility(doctor)"><Eye v-if="doctor.isPubliclyVisible" :size="16" /><EyeOff v-else :size="16" />{{ doctor.isPubliclyVisible ? 'ظاهر' : 'مخفي' }}</button></td>
              <td><div class="row-actions">
                <RouterLink class="row-link" :to="`/doctors/${doctor.id}`" title="عرض صفحة الطبيب"><FileText :size="17" /></RouterLink>
                <button type="button" title="تعديل الطبيب" @click="resetForm(doctor)"><Pencil :size="17" /></button>
                <button v-if="doctor.userId" type="button" title="فصل الحساب" @click="askUnlink(doctor)"><Unlink :size="17" /></button>
                <button v-else type="button" title="ربط حساب" @click="openLinkAccount(doctor)"><Link2 :size="17" /></button>
                <button class="danger-action" type="button" title="حذف الطبيب" @click="askDelete(doctor)"><Trash2 :size="17" /></button>
              </div></td>
            </tr>
          </tbody>
        </table>
      </div>
      <AppPagination :page="page" :total-pages="totalPages" @change="changePage" />
    </section>

    <AppModal v-if="editorOpen" :title="isEditing ? 'تعديل بيانات الطبيب' : 'إضافة طبيب جديد'" wide @close="editorOpen = false">
      <form class="modal-form form-grid" @submit.prevent="saveDoctor">
        <label><span>اسم الطبيب</span><input v-model="form.name" required /></label>
        <label><span>الاسم المستخدم في البحث</span><input v-model="form.normalizedName" required /></label>
        <label><span>الاختصاص</span><select v-model="form.specializationId" required><option disabled value="">اختر الاختصاص</option><option v-for="item in specializations" :key="item.id" :value="item.id">{{ item.name }}</option></select></label>
        <label><span>المحافظة</span><select v-model="form.iraqiProvince" required><option v-for="province in provinces" :key="province.value" :value="province.value">{{ province.name }}</option></select></label>
        <label><span>تاريخ الميلاد</span><input v-model="form.birthDay" required type="date" /></label>
        <label><span>رقم الهاتف</span><input v-model="form.phoneNumber" required /></label>
        <label class="full-field"><span>الموقع</span><input v-model="form.location" required /></label>
        <label class="full-field"><span>الوصف</span><textarea v-model="form.description" required rows="3" /></label>
        <label class="full-field"><span>صورة الطبيب {{ isEditing ? '(اختيارية عند التعديل)' : '' }}</span><input :required="!isEditing" type="file" accept=".jpg,.jpeg,.png,.webp" @change="setImage" /><small class="upload-hint">JPG أو PNG أو WEBP، بحد أقصى 5MB.</small></label>
        <img v-if="imagePreview" class="doctor-image-preview full-field" :src="imagePreview" alt="معاينة صورة الطبيب" />
        <div class="modal-actions full-field"><button class="secondary-button" type="button" @click="editorOpen = false">تراجع</button><button class="compact-primary" type="submit" :disabled="saving">{{ saving ? 'جارِ الحفظ...' : 'حفظ البيانات' }}</button></div>
      </form>
    </AppModal>

    <AppModal v-if="linkDoctor" title="ربط حساب الطبيب" @close="linkDoctor = undefined">
      <p class="modal-copy">ابحث بالاسم أو الهاتف أو البريد ثم اختر الحساب الذي سيدير ملف الطبيب <strong>{{ linkDoctor.name }}</strong>.</p>
      <div class="link-user-search"><label class="search-box"><Search :size="17" /><input v-model="linkSearch" placeholder="اسم المستخدم أو الهاتف أو البريد" @keyup.enter="searchLinkUsers" /></label><button class="compact-primary" type="button" @click="searchLinkUsers">بحث</button></div>
      <div v-if="linkLoading" class="link-user-empty">جارِ البحث...</div>
      <div v-else-if="!linkUsers.length" class="link-user-empty">اكتب عبارة البحث لعرض الحسابات المتاحة.</div>
      <div v-else class="link-user-results">
        <label v-for="user in linkUsers" :key="user.id" class="link-user-option" :class="{ selected: userId === user.id }"><input v-model="userId" type="radio" :value="user.id" /><span class="small-avatar"><UserRound :size="17" /></span><span><strong>{{ user.name || user.userName }}</strong><small>{{ user.phoneNumber || user.email || user.id }}</small></span></label>
      </div>
      <div class="modal-actions"><button class="secondary-button" type="button" @click="linkDoctor = undefined">تراجع</button><button class="compact-primary" type="button" :disabled="!userId" @click="linkAccount">ربط الحساب</button></div>
    </AppModal>

    <AppModal v-if="confirmation" :title="confirmation.title" @close="confirmation = undefined">
      <p class="modal-copy">{{ confirmation.text }}</p>
      <div class="modal-actions"><button class="secondary-button" type="button" @click="confirmation = undefined">تراجع</button><button class="danger-button" type="button" @click="runConfirmation">تأكيد</button></div>
    </AppModal>
  </div>
</template>

<style scoped>
.upload-hint { display: block; margin-top: 6px; color: var(--muted); font-size: 12px; }.doctor-image-preview { width: 94px; height: 94px; object-fit: cover; border: 1px solid var(--line); border-radius: 14px; }
.linked-account-cell { display: grid; gap: 4px; min-width: 170px; }.linked-account-cell strong { color: var(--ink); font-size: 13px; }.linked-account-cell small { color: var(--muted); direction: ltr; text-align: right; font-size: 11px; }
.link-user-search { display: flex; gap: 8px; margin-top: 14px; }.link-user-search .search-box { flex: 1; min-width: 0; }.link-user-empty { margin-top: 12px; padding: 18px; color: var(--muted); text-align: center; border: 1px solid var(--line); border-radius: 9px; background: #fbfdfc; }
.link-user-results { display: grid; gap: 7px; max-height: 270px; margin-top: 12px; overflow-y: auto; }.link-user-option { display: flex; align-items: center; gap: 8px; padding: 9px; border: 1px solid var(--line); border-radius: 9px; cursor: pointer; }.link-user-option.selected { border-color: #9acfc7; background: var(--primary-soft); }.link-user-option strong, .link-user-option small { display: block; }.link-user-option small { margin-top: 3px; color: var(--muted); font-size: 12px; }
</style>
