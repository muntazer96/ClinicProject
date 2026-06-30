<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { provinces } from '../constants/provinces'
import api from '../services/api'
import { useNotifications } from '../composables/useNotifications'
import type { ApiResponse, DoctorItem, PageResult, SpecializationItem, UserItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import EmptyState from '../components/common/Emptystate.vue'

type Confirmation = { title: string; text: string; action: () => Promise<void> }

const router = useRouter()
const { success: showSuccess, error: showError } = useNotifications()

const doctors         = ref<DoctorItem[]>([])
const specializations = ref<SpecializationItem[]>([])
const loading         = ref(false)
const saving          = ref(false)
const page            = ref(1)
const totalPages      = ref(1)
const totalItems      = ref(0)
const editorOpen      = ref(false)
const linkDialog      = ref(false)
const confirmDialog   = ref(false)
const linkDoctor      = ref<DoctorItem>()
const userId          = ref('')
const linkSearch      = ref('')
const linkUsers       = ref<UserItem[]>([])
const linkLoading     = ref(false)
const imagePreview    = ref('')
const confirmation    = ref<Confirmation>()
const fileInput       = ref<HTMLInputElement>()
let localPreviewUrl   = ''

const filters = reactive({ name: '', specialization: '', iraqiProvince: '' })
const defaultProvinceValue = provinces[0]?.value ?? 0
const form    = reactive({
  id: 0, name: '', normalizedName: '', specializationId: '',
  description: '', iraqiProvince: defaultProvinceValue, birthDay: '', phoneNumber: '',
  location: '', imageName: undefined as File | undefined,
})

const isEditing = computed(() => Boolean(form.id))

// API
async function loadDoctors() {
  loading.value = true
  try {
    const r = await api.get<ApiResponse<PageResult<DoctorItem>>>('/Doctor', {
      params: {
        name: filters.name || undefined,
        specialization: filters.specialization || undefined,
        iraqiProvince: filters.iraqiProvince === '' ? undefined : filters.iraqiProvince,
        page: page.value, pageSize: 10,
      },
    })
    doctors.value     = r.data.data.items
    totalPages.value  = r.data.data.totalPages
    totalItems.value  = r.data.data.totalItems
  } catch (e: any) {
    if (e.response?.status === 404) { doctors.value = []; totalPages.value = 1; totalItems.value = 0 }
    else showError(getErrorMessage(e))
  } finally { loading.value = false }
}

async function loadSpecializations() {
  try {
    const r = await api.get<ApiResponse<SpecializationItem[]>>('/Specialization')
    specializations.value = r.data.data
  } catch (e) { showError(getErrorMessage(e)) }
}

function resetForm(doctor?: DoctorItem) {
  Object.assign(form, doctor ? {
    id: doctor.id, name: doctor.name, normalizedName: doctor.normalizedName,
    specializationId: String(doctor.specialization.id),
    description: doctor.description, iraqiProvince: doctor.iraqiProvince,
    birthDay: doctor.birthDay, phoneNumber: doctor.phoneNumber,
    location: doctor.location, imageName: undefined,
  } : {
    id: 0, name: '', normalizedName: '', specializationId: '',
    description: '', iraqiProvince: defaultProvinceValue, birthDay: '', phoneNumber: '',
    location: '', imageName: undefined,
  })
  clearImagePreview()
  editorOpen.value = true
}

function setImage(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || file.size > 5 * 1024 * 1024) {
    form.imageName = undefined
    clearImagePreview()
    showError('الصورة يجب أن تكون JPG أو PNG أو WEBP وبحجم لا يتجاوز 5MB.')
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
  if (!isEditing.value && !form.imageName) { showError('صورة الطبيب مطلوبة عند إنشاء السجل.'); return }
  saving.value = true
  try {
    const data = new FormData()
    if (isEditing.value) data.append('Id', String(form.id))
    data.append('Name', form.name)
    data.append('NormalizedName', form.normalizedName)
    data.append('SpecializationId', form.specializationId)
    data.append('Description', form.description)
    data.append('IraqiProvince', String(form.iraqiProvince))
    data.append('BirthDay', form.birthDay)
    data.append('PhoneNumber', form.phoneNumber)
    data.append('Location', form.location)
    if (form.imageName) data.append('ImageName', form.imageName)
    const r = isEditing.value
      ? await api.put<ApiResponse<string>>('/Doctor', data)
      : await api.post<ApiResponse<string>>('/Doctor', data)
    showSuccess(r.data.message)
    editorOpen.value = false
    await loadDoctors()
  } catch (e) { showError(getErrorMessage(e)) }
  finally { saving.value = false }
}

async function toggleVisibility(doctor: DoctorItem) {
  try {
    const r = await api.put<ApiResponse<string>>(`/Doctor/${doctor.id}/visibility`, { isPubliclyVisible: !doctor.isPubliclyVisible })
    showSuccess(r.data.message)
    await loadDoctors()
  } catch (e) { showError(getErrorMessage(e)) }
}

function openLinkAccount(doctor: DoctorItem) {
  linkDoctor.value = doctor
  userId.value = ''
  linkSearch.value = ''
  linkUsers.value = []
  linkDialog.value = true
}

async function searchLinkUsers() {
  linkLoading.value = true
  try {
    const r = await api.get<ApiResponse<PageResult<UserItem>>>('/User', {
      params: { search: linkSearch.value.trim() || undefined, page: 1, pageSize: 8 },
    })
    linkUsers.value = r.data.data.items.filter((u) => u.userName?.toLowerCase() !== 'superadmin' && !u.isLocked)
  } catch (e) { showError(getErrorMessage(e)) }
  finally { linkLoading.value = false }
}

async function linkAccount() {
  if (!linkDoctor.value) return
  try {
    const r = await api.post<ApiResponse<string>>(`/Doctor/${linkDoctor.value.id}/link-account`, { userId: userId.value })
    showSuccess(r.data.message)
    linkDialog.value = false
    linkDoctor.value = undefined
    userId.value = ''
    await loadDoctors()
  } catch (e) { showError(getErrorMessage(e)) }
}

function askDelete(doctor: DoctorItem) {
  confirmation.value = {
    title: 'تأكيد حذف الطبيب',
    text: `سيتم حذف سجل الطبيب ${doctor.name}.`,
    action: async () => {
      const r = await api.delete<ApiResponse<string>>(`/Doctor/${doctor.id}`)
      showSuccess(r.data.message)
      await loadDoctors()
    },
  }
  confirmDialog.value = true
}

function askUnlink(doctor: DoctorItem) {
  confirmation.value = {
    title: 'فصل حساب الطبيب',
    text: `سيتم فصل حساب الدخول المرتبط بالطبيب ${doctor.name}.`,
    action: async () => {
      const r = await api.delete<ApiResponse<string>>(`/Doctor/${doctor.id}/link-account`)
      showSuccess(r.data.message)
      await loadDoctors()
    },
  }
  confirmDialog.value = true
}

async function runConfirmation() {
  if (!confirmation.value) return
  try {
    await confirmation.value.action()
    confirmDialog.value = false
    confirmation.value = undefined
  } catch (e) { showError(getErrorMessage(e)) }
}

function applyFilters() { page.value = 1; loadDoctors() }
function changePage(n: number) { page.value = n; loadDoctors() }

onMounted(() => Promise.all([loadDoctors(), loadSpecializations()]))
onBeforeUnmount(clearImagePreview)
</script>

<template>
  <div class="doctors-page">

    <!-- Header -->
    <div class="page-top">
      <div>
        <p class="page-kicker">دليل الأطباء</p>
        <h1 class="page-title">الأطباء</h1>
      </div>
      <div class="page-actions">
        <v-btn variant="outlined" color="primary" prepend-icon="mdi-refresh" :loading="loading" @click="loadDoctors">تحديث</v-btn>
        <v-btn color="primary" prepend-icon="mdi-plus" @click="resetForm()">إضافة طبيب</v-btn>
      </div>
    </div>

    <!-- Filters -->
    <div class="filters-bar">
      <div class="filter-field filter-field--search">
        <label class="filter-label">بحث</label>
        <div class="search-wrap">
          <v-icon icon="mdi-magnify" size="18" class="search-icon" />
          <input v-model="filters.name" class="search-input" placeholder="اسم الطبيب" @keyup.enter="applyFilters" />
        </div>
      </div>
      <div class="filter-field">
        <label class="filter-label">الاختصاص</label>
        <v-autocomplete
          v-model="filters.specialization"
          :items="[{ value: '', label: 'كل الاختصاصات' }, ...specializations.map(s => ({ value: s.id, label: s.name }))]"
          item-title="label"
          item-value="value"
          class="filter-select"
          density="compact"
          variant="outlined"
          hide-details
        />
      </div>
      <div class="filter-field">
        <label class="filter-label">المحافظة</label>
        <v-autocomplete
          v-model="filters.iraqiProvince"
          :items="[{ value: '', label: 'كل المحافظات' }, ...provinces.map(p => ({ value: p.value, label: p.name }))]"
          item-title="label"
          item-value="value"
          class="filter-select"
          density="compact"
          variant="outlined"
          hide-details
        />
      </div>
      <v-btn color="primary" prepend-icon="mdi-magnify" class="filter-btn" @click="applyFilters">تطبيق</v-btn>
    </div>

    <!-- Table Card -->
    <v-card elevation="0" class="table-card">
      <div class="table-toolbar">
        <v-icon icon="mdi-stethoscope" color="primary" size="20" />
        <strong>قائمة الأطباء</strong>
        <v-chip size="small" color="primary" variant="tonal">{{ totalItems }} طبيب</v-chip>
      </div>

      <div v-if="loading" class="table-loading">
        <v-skeleton-loader v-for="i in 5" :key="i" type="table-row" />
      </div>

      <EmptyState
        v-else-if="!doctors.length"
        icon="mdi-stethoscope"
        title="لا توجد سجلات"
        description="لا توجد سجلات مطابقة للفلاتر المحددة"
      />

      <div v-else class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th>الطبيب</th>
              <th>الاختصاص والمحافظة</th>
              <th>التواصل</th>
              <th>الحساب المرتبط</th>
              <th>الظهور</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="doctor in doctors" :key="doctor.id">
              <!-- Doctor -->
              <td>
                <div class="doctor-cell">
                  <div class="doctor-avatar">
                    <v-icon icon="mdi-stethoscope" color="primary" size="18" />
                  </div>
                  <div>
                    <strong>{{ doctor.name }}</strong>
                    <p class="row-sub">{{ doctor.normalizedName }}</p>
                  </div>
                </div>
              </td>
              <!-- Specialization -->
              <td>
                <strong>{{ doctor.specialization.name }}</strong>
                <p class="row-sub">{{ doctor.iraqiProvinceName }}</p>
              </td>
              <!-- Contact -->
              <td>
                <strong>{{ doctor.phoneNumber }}</strong>
                <p class="row-sub">{{ doctor.location }}</p>
              </td>
              <!-- Linked Account -->
              <td>
                <div v-if="doctor.linkedUser">
                  <strong>{{ doctor.linkedUser.name || doctor.linkedUser.userName }}</strong>
                  <p class="row-sub ltr">{{ doctor.linkedUser.phoneNumber || doctor.linkedUser.id }}</p>
                </div>
                <v-chip v-else size="small" variant="tonal" color="default">غير مربوط</v-chip>
              </td>
              <!-- Visibility -->
              <td>
                <v-btn
                  size="small"
                  :color="doctor.isPubliclyVisible ? 'success' : 'default'"
                  :variant="doctor.isPubliclyVisible ? 'tonal' : 'outlined'"
                  :prepend-icon="doctor.isPubliclyVisible ? 'mdi-eye' : 'mdi-eye-off'"
                  @click="toggleVisibility(doctor)"
                >
                  {{ doctor.isPubliclyVisible ? 'ظاهر' : 'مخفي' }}
                </v-btn>
              </td>
              <!-- Actions -->
              <td>
                <div class="row-actions">
                  <v-btn icon size="small" variant="text" color="info" aria-label="عرض صفحة الطبيب" @click="router.push(`/doctors/${doctor.id}`)">
                    <v-icon icon="mdi-file-document" size="16" />
                  </v-btn>
                  <v-btn icon size="small" variant="text" color="primary" aria-label="تعديل الطبيب" @click="resetForm(doctor)">
                    <v-icon icon="mdi-pencil" size="16" />
                  </v-btn>
                  <v-btn
                    v-if="doctor.userId"
                    icon size="small" variant="text" color="warning"
                    aria-label="فصل الحساب"
                    @click="askUnlink(doctor)"
                  >
                    <v-icon icon="mdi-link-off" size="16" />
                  </v-btn>
                  <v-btn
                    v-else
                    icon size="small" variant="text" color="success"
                    aria-label="ربط حساب"
                    @click="openLinkAccount(doctor)"
                  >
                    <v-icon icon="mdi-link" size="16" />
                  </v-btn>
                  <v-btn icon size="small" variant="text" color="error" aria-label="حذف الطبيب" @click="askDelete(doctor)">
                    <v-icon icon="mdi-delete" size="16" />
                  </v-btn>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="pagination-bar">
        <v-pagination v-model="page" :length="totalPages" :total-visible="5" density="compact" color="primary" @update:model-value="changePage" />
      </div>
    </v-card>

    <!-- ── Editor Dialog ── -->
    <v-dialog v-model="editorOpen" max-width="700" scrollable>
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon :icon="isEditing ? 'mdi-pencil' : 'mdi-plus'" color="primary" size="20" />
          {{ isEditing ? 'تعديل بيانات الطبيب' : 'إضافة طبيب جديد' }}
        </v-card-title>
        <v-divider />

        <v-card-text class="dialog-body">
          <div class="form-grid">
            <div class="form-field">
              <label class="form-label">اسم الطبيب <span class="required">*</span></label>
              <input v-model="form.name" class="form-input" required />
            </div>
            <div class="form-field">
              <label class="form-label">الاسم في البحث <span class="required">*</span></label>
              <input v-model="form.normalizedName" class="form-input" required />
            </div>
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
            <div class="form-field">
              <label class="form-label">تاريخ الميلاد <span class="required">*</span></label>
              <input v-model="form.birthDay" type="date" class="form-input" required />
            </div>
            <div class="form-field">
              <label class="form-label">رقم الهاتف <span class="required">*</span></label>
              <input v-model="form.phoneNumber" class="form-input" required />
            </div>
            <div class="form-field form-field--full">
              <label class="form-label">الموقع <span class="required">*</span></label>
              <input v-model="form.location" class="form-input" required />
            </div>
            <div class="form-field form-field--full">
              <label class="form-label">الوصف <span class="required">*</span></label>
              <textarea v-model="form.description" class="form-textarea" required rows="3" />
            </div>
            <div class="form-field form-field--full">
              <label class="form-label">
                صورة الطبيب
                <span v-if="isEditing" class="optional">(اختيارية عند التعديل)</span>
                <span v-else class="required">*</span>
              </label>
              <div class="file-zone" @click="fileInput?.click()">
                <input ref="fileInput" type="file" accept=".jpg,.jpeg,.png,.webp" class="file-hidden" :required="!isEditing" @change="setImage" />
                <v-icon :icon="form.imageName ? 'mdi-image-check' : 'mdi-image-plus'" :color="form.imageName ? 'success' : 'primary'" size="28" />
                <div>
                  <strong>{{ form.imageName ? form.imageName.name : 'اضغط لاختيار صورة' }}</strong>
                  <span>JPG أو PNG أو WEBP، بحد أقصى 5MB</span>
                </div>
              </div>
              <img v-if="imagePreview" :src="imagePreview" alt="معاينة" class="image-preview" />
            </div>
          </div>
        </v-card-text>

        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="editorOpen = false">تراجع</v-btn>
          <v-btn color="primary" :loading="saving" @click="saveDoctor">
            {{ isEditing ? 'حفظ التعديلات' : 'إضافة الطبيب' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Link Account Dialog ── -->
    <v-dialog v-model="linkDialog" max-width="520">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-link" color="primary" size="20" />
          ربط حساب الطبيب
        </v-card-title>
        <v-divider />

        <v-card-text class="dialog-body">
          <p class="dialog-desc">
            ابحث بالاسم أو الهاتف ثم اختر الحساب الذي سيدير ملف الطبيب
            <strong>{{ linkDoctor?.name }}</strong>.
          </p>

          <div class="link-search-bar">
            <div class="search-wrap" style="flex:1">
              <v-icon icon="mdi-magnify" size="18" class="search-icon" />
              <input
                v-model="linkSearch"
                class="search-input"
                placeholder="اسم المستخدم أو الهاتف"
                @keyup.enter="searchLinkUsers"
              />
            </div>
            <v-btn color="primary" :loading="linkLoading" @click="searchLinkUsers">بحث</v-btn>
          </div>

          <div v-if="linkLoading" class="link-empty">جارِ البحث...</div>
          <div v-else-if="!linkUsers.length" class="link-empty">
            <v-icon icon="mdi-account-search" size="32" color="var(--color-text-muted)" />
            <p>اكتب عبارة البحث لعرض الحسابات المتاحة</p>
          </div>

          <div v-else class="link-results">
            <label
              v-for="user in linkUsers"
              :key="user.id"
              class="link-option"
              :class="{ 'link-option--selected': userId === user.id }"
            >
              <input v-model="userId" type="radio" :value="user.id" class="check-native" />
              <div class="link-option-avatar">
                <v-icon icon="mdi-account" size="18" color="primary" />
              </div>
              <div>
                <strong>{{ user.name || user.userName }}</strong>
                <p class="row-sub ltr">{{ user.phoneNumber || user.id }}</p>
              </div>
              <v-icon v-if="userId === user.id" icon="mdi-check-circle" color="success" size="20" class="mr-auto" />
            </label>
          </div>
        </v-card-text>

        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="linkDialog = false">تراجع</v-btn>
          <v-btn color="primary" :disabled="!userId" @click="linkAccount">ربط الحساب</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Confirm Dialog ── -->
    <v-dialog v-model="confirmDialog" max-width="420">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-alert" color="error" size="20" />
          {{ confirmation?.title }}
        </v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">{{ confirmation?.text }}</v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="confirmDialog = false">تراجع</v-btn>
          <v-btn color="error" @click="runConfirmation">تأكيد</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </div>
</template>

<style scoped>
.doctors-page { display: flex; flex-direction: column; gap: var(--spacing-lg); }

/* Page Top */
.page-top { display: flex; align-items: center; justify-content: space-between; gap: var(--spacing-lg); flex-wrap: wrap; }
.page-kicker { margin: 0 0 4px 0; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--color-text-muted); }
.page-title { margin: 0; font-size: 28px; font-weight: 800; color: var(--color-text); }
.page-actions { display: flex; gap: var(--spacing-md); }

/* Filters */
.filters-bar { display: flex; align-items: flex-end; gap: var(--spacing-md); flex-wrap: wrap; padding: var(--spacing-lg); background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-lg); box-shadow: var(--shadow-sm); }
.filter-field { display: flex; flex-direction: column; gap: 6px; }
.filter-field--search { flex: 1; min-width: 200px; }
.filter-label { font-size: 12px; font-weight: 700; color: var(--color-text-muted); }
.filter-select { height: 40px; padding: 0 12px; border: 1.5px solid var(--color-border); border-radius: var(--radius-md); background: var(--color-surface); color: var(--color-text); font-family: var(--font-family-primary); font-size: 14px; outline: none; min-width: 160px; transition: border-color 0.2s; }
.filter-select:focus { border-color: var(--color-primary); }
.filter-btn { align-self: flex-end; }
.search-wrap { position: relative; }
.search-icon { position: absolute; right: 10px; top: 50%; transform: translateY(-50%); color: var(--color-text-muted); pointer-events: none; }
[dir='rtl'] .search-icon { right: auto; left: 10px; }
.search-input { width: 100%; height: 40px; padding: 0 36px 0 12px; border: 1.5px solid var(--color-border); border-radius: var(--radius-md); background: var(--color-surface); color: var(--color-text); font-family: var(--font-family-primary); font-size: 14px; outline: none; transition: border-color 0.2s; }
[dir='rtl'] .search-input { padding: 0 12px 0 36px; }
.search-input:focus { border-color: var(--color-primary); }

/* Table */
.table-card { border: 1px solid var(--color-border) !important; border-radius: var(--radius-lg) !important; overflow: hidden; }
.table-toolbar { display: flex; align-items: center; gap: var(--spacing-md); padding: var(--spacing-lg); border-bottom: 1px solid var(--color-border); }
.table-toolbar strong { flex: 1; font-size: 15px; color: var(--color-text); }
.table-loading { padding: var(--spacing-lg); }
.table-scroll { overflow-x: auto; }
.data-table { width: 100%; border-collapse: collapse; font-size: 13px; }
.data-table th { padding: 12px 16px; text-align: right; font-size: 12px; font-weight: 700; color: var(--color-text-muted); background: var(--color-background); border-bottom: 1px solid var(--color-border); white-space: nowrap; }
.data-table td { padding: 12px 16px; border-bottom: 1px solid var(--color-border-light); vertical-align: middle; color: var(--color-text); }
.data-table tbody tr:hover { background: var(--color-background); }
.data-table tbody tr:last-child td { border-bottom: none; }

.doctor-cell { display: flex; align-items: center; gap: var(--spacing-md); }
.doctor-avatar { display: flex; align-items: center; justify-content: center; width: 36px; height: 36px; border-radius: var(--radius-md); background: var(--color-primary-soft); flex-shrink: 0; }
.row-sub { margin: 2px 0 0 0; font-size: 11px; color: var(--color-text-muted); }
.ltr { direction: ltr; text-align: right; }
.row-actions { display: flex; gap: 2px; }
.pagination-bar { display: flex; justify-content: center; padding: var(--spacing-lg); border-top: 1px solid var(--color-border); }

/* Dialog */
.dialog-title { display: flex; align-items: center; gap: var(--spacing-md); padding: var(--spacing-lg) !important; font-size: 16px !important; font-weight: 700; color: var(--color-text); }
.dialog-body { padding: var(--spacing-lg) !important; }
.dialog-desc { margin: 0 0 var(--spacing-lg) 0; font-size: 13px; color: var(--color-text-muted); line-height: 1.6; }
.dialog-actions { padding: var(--spacing-lg) !important; gap: var(--spacing-md); justify-content: flex-end; }

/* Form */
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--spacing-lg); }
.form-field { display: flex; flex-direction: column; gap: 6px; }
.form-field--full { grid-column: 1 / -1; }
.form-label { font-size: 13px; font-weight: 600; color: var(--color-text); }
.required { color: var(--color-error); }
.optional { color: var(--color-text-muted); font-weight: 400; font-size: 12px; }
.form-input, .form-select, .form-textarea { padding: 10px 12px; border: 1.5px solid var(--color-border); border-radius: var(--radius-md); background: var(--color-surface); color: var(--color-text); font-family: var(--font-family-primary); font-size: 14px; outline: none; width: 100%; transition: border-color 0.2s; }
.form-input:focus, .form-select:focus, .form-textarea:focus { border-color: var(--color-primary); }
.form-textarea { resize: vertical; }

/* File Zone */
.file-zone { display: flex; align-items: center; gap: var(--spacing-lg); padding: var(--spacing-lg); border: 2px dashed var(--color-border); border-radius: var(--radius-md); background: var(--color-background); cursor: pointer; transition: all 0.2s ease; }
.file-zone:hover { border-color: var(--color-primary); background: var(--color-primary-soft); }
.file-hidden { display: none; }
.file-zone strong { display: block; font-size: 14px; font-weight: 600; color: var(--color-text); margin-bottom: 2px; }
.file-zone span { font-size: 12px; color: var(--color-text-muted); }
.image-preview { width: 88px; height: 88px; object-fit: cover; border-radius: var(--radius-md); border: 1px solid var(--color-border); margin-top: var(--spacing-md); }

/* Link */
.link-search-bar { display: flex; gap: var(--spacing-md); margin-bottom: var(--spacing-lg); }
.link-empty { display: flex; flex-direction: column; align-items: center; gap: var(--spacing-md); padding: var(--spacing-xl); color: var(--color-text-muted); text-align: center; }
.link-empty p { margin: 0; font-size: 13px; }
.link-results { display: flex; flex-direction: column; gap: var(--spacing-sm); max-height: 280px; overflow-y: auto; }
.link-option { display: flex; align-items: center; gap: var(--spacing-md); padding: var(--spacing-md); border: 1.5px solid var(--color-border); border-radius: var(--radius-md); cursor: pointer; transition: all 0.15s ease; }
.link-option:hover { border-color: var(--color-primary); background: var(--color-primary-soft); }
.link-option--selected { border-color: var(--color-primary); background: var(--color-primary-soft); }
.link-option-avatar { display: flex; align-items: center; justify-content: center; width: 36px; height: 36px; border-radius: var(--radius-md); background: var(--color-primary-soft); flex-shrink: 0; }
.link-option strong { font-size: 13px; font-weight: 700; color: var(--color-text); }
.check-native { position: absolute; opacity: 0; width: 0; height: 0; }
.mr-auto { margin-right: auto !important; }

/* Responsive */
@media (max-width: 768px) { .form-grid { grid-template-columns: 1fr; } .filters-bar { flex-direction: column; } }
</style>
