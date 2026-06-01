<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { Eye, EyeOff, Link2, Pencil, Plus, RefreshCw, Search, Stethoscope, Trash2, Unlink } from '@lucide/vue'
import AppModal from '../components/AppModal.vue'
import AppPagination from '../components/AppPagination.vue'
import { provinces } from '../constants/provinces'
import api from '../services/api'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, DoctorItem, PageResult, SpecializationItem } from '../types/api'
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
}

function setImage(event: Event) {
  form.imageName = (event.target as HTMLInputElement).files?.[0]
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
          <thead><tr><th>الطبيب</th><th>الاختصاص والمحافظة</th><th>التواصل</th><th>الحساب</th><th>الظهور</th><th>الإجراءات</th></tr></thead>
          <tbody>
            <tr v-if="loading"><td colspan="6" class="table-message">جارِ تحميل الأطباء...</td></tr>
            <tr v-else-if="!doctors.length"><td colspan="6" class="table-message">لا توجد سجلات مطابقة.</td></tr>
            <tr v-for="doctor in doctors" v-else :key="doctor.id">
              <td><div class="entity-cell"><span class="small-avatar"><Stethoscope :size="17" /></span><div><strong>{{ doctor.name }}</strong><small>{{ doctor.normalizedName }}</small></div></div></td>
              <td><strong>{{ doctor.specialization.name }}</strong><small class="block-muted">{{ doctor.iraqiProvinceName }}</small></td>
              <td><strong>{{ doctor.phoneNumber }}</strong><small class="block-muted">{{ doctor.location }}</small></td>
              <td><span class="status-badge" :class="doctor.userId ? 'status-success' : 'status-neutral'">{{ doctor.userId ? 'مربوط' : 'غير مربوط' }}</span></td>
              <td><button class="status-button" type="button" :class="doctor.isPubliclyVisible ? 'visible-status' : ''" @click="toggleVisibility(doctor)"><Eye v-if="doctor.isPubliclyVisible" :size="16" /><EyeOff v-else :size="16" />{{ doctor.isPubliclyVisible ? 'ظاهر' : 'مخفي' }}</button></td>
              <td><div class="row-actions">
                <button type="button" title="تعديل الطبيب" @click="resetForm(doctor)"><Pencil :size="17" /></button>
                <button v-if="doctor.userId" type="button" title="فصل الحساب" @click="askUnlink(doctor)"><Unlink :size="17" /></button>
                <button v-else type="button" title="ربط حساب" @click="linkDoctor = doctor"><Link2 :size="17" /></button>
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
        <label class="full-field"><span>صورة الطبيب {{ isEditing ? '(اختيارية عند التعديل)' : '' }}</span><input :required="!isEditing" type="file" accept="image/*" @change="setImage" /></label>
        <div class="modal-actions full-field"><button class="secondary-button" type="button" @click="editorOpen = false">تراجع</button><button class="compact-primary" type="submit" :disabled="saving">{{ saving ? 'جارِ الحفظ...' : 'حفظ البيانات' }}</button></div>
      </form>
    </AppModal>

    <AppModal v-if="linkDoctor" title="ربط حساب الطبيب" @close="linkDoctor = undefined">
      <p class="modal-copy">أدخل معرّف المستخدم الذي سيُمنح صلاحية إدارة ملف الطبيب <strong>{{ linkDoctor.name }}</strong>.</p>
      <form class="modal-form" @submit.prevent="linkAccount"><label><span>معرّف المستخدم</span><input v-model="userId" required placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" /></label><div class="modal-actions"><button class="secondary-button" type="button" @click="linkDoctor = undefined">تراجع</button><button class="compact-primary" type="submit">ربط الحساب</button></div></form>
    </AppModal>

    <AppModal v-if="confirmation" :title="confirmation.title" @close="confirmation = undefined">
      <p class="modal-copy">{{ confirmation.text }}</p>
      <div class="modal-actions"><button class="secondary-button" type="button" @click="confirmation = undefined">تراجع</button><button class="danger-button" type="button" @click="runConfirmation">تأكيد</button></div>
    </AppModal>
  </div>
</template>
