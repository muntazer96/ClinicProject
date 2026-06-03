<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { CalendarOff, Pencil, Plus, RefreshCw, Trash2 } from '@lucide/vue'
import AppModal from '../components/AppModal.vue'
import AppPagination from '../components/AppPagination.vue'
import api from '../services/api'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, ClinicExceptionItem, ClinicItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const notifications = useNotificationsStore()
const clinics = ref<ClinicItem[]>([])
const exceptions = ref<ClinicExceptionItem[]>([])
const page = ref(1)
const pageSize = 6
const clinicId = ref('')
const loading = ref(false)
const saving = ref(false)
const editorOpen = ref(false)
const deleteException = ref<ClinicExceptionItem>()
const filters = reactive({ fromDate: '', toDate: '' })
const form = reactive({ id: 0, exceptionDate: '', isClosed: true, closureReason: '', maxAppointments: '', startTime: '', endTime: '' })
const selectedClinicName = computed(() => clinics.value.find((clinic) => clinic.id === Number(clinicId.value))?.name)
const totalPages = computed(() => Math.max(1, Math.ceil(exceptions.value.length / pageSize)))
const paginatedExceptions = computed(() => exceptions.value.slice((page.value - 1) * pageSize, page.value * pageSize))

function formatDate(value: string) { return new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'full' }).format(new Date(`${value}T00:00:00`)) }
function normalizePage() { if (page.value > totalPages.value) page.value = totalPages.value }
function changePage(newPage: number) { page.value = newPage }
function applyExceptionFilters() {
  page.value = 1
  loadExceptions()
}
async function loadClinics() {
  const response = await api.get<ApiResponse<ClinicItem[]>>('/Clinic/my')
  clinics.value = response.data.data
  if (!clinicId.value && clinics.value.length) clinicId.value = String(clinics.value[0].id)
}
async function loadExceptions() {
  if (!clinicId.value) { exceptions.value = []; page.value = 1; return }
  loading.value = true
  try {
    const response = await api.get<ApiResponse<ClinicExceptionItem[]>>(`/ClinicException/my/${clinicId.value}`, {
      params: {
        fromDate: filters.fromDate || undefined,
        toDate: filters.toDate || undefined,
      },
    })
    exceptions.value = response.data.data
    normalizePage()
  } catch (error) { notifications.show(getErrorMessage(error), 'error') }
  finally { loading.value = false }
}
function openEditor(item?: ClinicExceptionItem) {
  Object.assign(form, item ? {
    id: item.id, exceptionDate: item.exceptionDate, isClosed: item.isClosed, closureReason: item.closureReason ?? '',
    maxAppointments: item.maxAppointments?.toString() ?? '', startTime: item.startTime?.slice(0, 5) ?? '', endTime: item.endTime?.slice(0, 5) ?? '',
  } : { id: 0, exceptionDate: '', isClosed: true, closureReason: '', maxAppointments: '', startTime: '', endTime: '' })
  editorOpen.value = true
}
async function saveException() {
  saving.value = true
  try {
    const response = await api.post<ApiResponse<object>>('/ClinicException/my', {
      id: form.id || null, clinicId: Number(clinicId.value), exceptionDate: form.exceptionDate, isClosed: form.isClosed,
      closureReason: form.closureReason || null, maxAppointments: form.isClosed || form.maxAppointments === '' ? null : Number(form.maxAppointments),
      startTime: form.isClosed || !form.startTime ? null : `${form.startTime}:00`, endTime: form.isClosed || !form.endTime ? null : `${form.endTime}:00`,
    })
    notifications.show(response.data.message); editorOpen.value = false; await loadExceptions()
  } catch (error) { notifications.show(getErrorMessage(error), 'error') }
  finally { saving.value = false }
}
async function confirmDelete() {
  if (!deleteException.value) return
  try {
    const response = await api.delete<ApiResponse<object>>(`/ClinicException/my/${deleteException.value.id}`)
    notifications.show(response.data.message); deleteException.value = undefined; await loadExceptions()
  } catch (error) { notifications.show(getErrorMessage(error), 'error') }
}
onMounted(async () => {
  try { await loadClinics(); await loadExceptions() }
  catch (error) { notifications.show(getErrorMessage(error), 'error') }
})
</script>

<template>
  <div>
    <div class="page-heading">
      <div><span class="section-kicker">تنظيم الدوام</span><h2>الإجازات والاستثناءات</h2><p>أغلق يوماً كاملاً أو خصص عدد الأدوار وساعات الدوام لتاريخ محدد.</p></div>
      <div class="heading-actions"><button class="secondary-button" type="button" :disabled="loading" @click="loadExceptions"><RefreshCw :size="17" /> تحديث</button><button class="compact-primary" type="button" :disabled="!clinicId" @click="openEditor()"><Plus :size="17" /> إضافة استثناء</button></div>
    </div>
    <form class="filter-card exception-filters" @submit.prevent="applyExceptionFilters">
      <select v-model="clinicId" @change="applyExceptionFilters"><option value="" disabled>اختر العيادة</option><option v-for="clinic in clinics" :key="clinic.id" :value="clinic.id">{{ clinic.name }}</option></select>
      <input v-model="filters.fromDate" type="date" aria-label="من تاريخ" />
      <input v-model="filters.toDate" type="date" aria-label="إلى تاريخ" />
      <button class="compact-primary" type="submit">تطبيق</button>
    </form>
    <div v-if="loading" class="empty-panel">جارِ تحميل الاستثناءات...</div>
    <div v-else-if="!clinics.length" class="empty-panel"><CalendarOff :size="31" /><h3>لا توجد عيادات</h3><p>أضف عيادة أولاً حتى تتمكن من إدارة الإجازات والاستثناءات.</p></div>
    <div v-else-if="!exceptions.length" class="empty-panel"><CalendarOff :size="31" /><h3>لا توجد استثناءات</h3><p>دوام {{ selectedClinicName }} يعمل وفق الجدول الأسبوعي بدون تعديلات إضافية.</p></div>
    <section v-else class="exception-grid">
      <article v-for="item in paginatedExceptions" :key="item.id" class="exception-card">
        <div><span class="status-badge" :class="item.isClosed ? 'status-danger' : 'status-warning'">{{ item.isClosed ? 'إغلاق كامل' : 'دوام مخصص' }}</span><h3>{{ formatDate(item.exceptionDate) }}</h3></div>
        <p v-if="item.closureReason">{{ item.closureReason }}</p>
        <div v-if="!item.isClosed" class="exception-details"><span>الأدوار: <b>{{ item.maxAppointments ?? 'حسب الجدول' }}</b></span><span v-if="item.startTime">الدوام: <b>{{ item.startTime.slice(0, 5) }} - {{ item.endTime?.slice(0, 5) }}</b></span></div>
        <div class="clinic-card-actions"><button type="button" @click="openEditor(item)"><Pencil :size="16" /> تعديل</button><button class="danger-text" type="button" @click="deleteException = item"><Trash2 :size="16" /> حذف</button></div>
      </article>
    </section>
    <AppPagination :page="page" :total-pages="totalPages" @change="changePage" />
    <AppModal v-if="editorOpen" :title="form.id ? 'تعديل الاستثناء' : 'إضافة استثناء جديد'" @close="editorOpen = false">
      <form class="modal-form" @submit.prevent="saveException">
        <label><span>التاريخ</span><input v-model="form.exceptionDate" type="date" required /></label>
        <label class="checkbox-field"><input v-model="form.isClosed" type="checkbox" /><span>إغلاق العيادة بالكامل في هذا اليوم</span></label>
        <template v-if="!form.isClosed">
          <label><span>عدد الأدوار المتاحة</span><input v-model="form.maxAppointments" type="number" min="0" placeholder="اتركه فارغاً لاستخدام الجدول الأسبوعي" /></label>
          <div class="form-grid"><label><span>من الساعة</span><input v-model="form.startTime" type="time" /></label><label><span>إلى الساعة</span><input v-model="form.endTime" type="time" /></label></div>
        </template>
        <label><span>ملاحظة أو سبب الإغلاق</span><textarea v-model="form.closureReason" rows="3" maxlength="500" /></label>
        <div class="modal-actions"><button class="secondary-button" type="button" @click="editorOpen = false">تراجع</button><button class="compact-primary" type="submit" :disabled="saving">{{ saving ? 'جارِ الحفظ...' : 'حفظ الاستثناء' }}</button></div>
      </form>
    </AppModal>
    <AppModal v-if="deleteException" title="حذف الاستثناء" @close="deleteException = undefined">
      <p class="modal-copy">سيعود هذا اليوم للعمل وفق جدول الدوام الأسبوعي بعد حذف الاستثناء.</p>
      <div class="modal-actions"><button class="secondary-button" type="button" @click="deleteException = undefined">تراجع</button><button class="danger-button" type="button" @click="confirmDelete">تأكيد الحذف</button></div>
    </AppModal>
  </div>
</template>
