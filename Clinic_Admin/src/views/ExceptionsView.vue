<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import api from '../services/api'
import { useNotifications } from '../composables/useNotifications'
import type { ApiResponse, ClinicExceptionItem, ClinicItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import EmptyState from '../components/common/Emptystate.vue'

const { success: showSuccess, error: showError } = useNotifications()

const clinics        = ref<ClinicItem[]>([])
const exceptions     = ref<ClinicExceptionItem[]>([])
const page           = ref(1)
const pageSize       = 6
const clinicId       = ref('')
const loading        = ref(false)
const saving         = ref(false)
const editorOpen     = ref(false)
const deleteDialog   = ref(false)
const deleteException = ref<ClinicExceptionItem>()

const filters = reactive({ fromDate: '', toDate: '' })
const form = reactive({
  id: 0, exceptionDate: '', isClosed: true, closureReason: '',
  maxAppointments: '', startTime: '', endTime: '',
  appointmentConflictAction: 'cancel', moveAppointmentsToDate: '',
})

const selectedClinic     = computed(() => clinics.value.find((c) => c.id === Number(clinicId.value)))
const selectedClinicName = computed(() => selectedClinic.value?.name)
const bookingWindowDays  = computed(() => Math.max(1, selectedClinic.value?.bookingWindowDays ?? 7))
const totalPages         = computed(() => Math.max(1, Math.ceil(exceptions.value.length / pageSize)))
const paginatedExceptions = computed(() => exceptions.value.slice((page.value - 1) * pageSize, page.value * pageSize))
const minMoveDate        = computed(() => form.exceptionDate ? addDays(form.exceptionDate, 1) : today())
const maxMoveDate        = computed(() => addDays(today(), bookingWindowDays.value - 1))
const moveDateHint       = computed(() => `تاريخ النقل يجب أن يكون بعد تاريخ الاستثناء وضمن نافذة الحجز (${bookingWindowDays.value} يوم).`)
const isMoveDateClosed   = computed(() =>
  exceptions.value.some((i) => i.id !== form.id && i.isClosed && i.exceptionDate === form.moveAppointmentsToDate)
)

function today()  { return new Date().toLocaleDateString('en-CA') }
function addDays(value: string, days: number) {
  const d = new Date(`${value}T00:00:00`)
  d.setDate(d.getDate() + days)
  return d.toLocaleDateString('en-CA')
}
function formatDate(value: string) {
  return new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'full' }).format(new Date(`${value}T00:00:00`))
}
function normalizePage() { if (page.value > totalPages.value) page.value = totalPages.value }

function validateMoveDate() {
  if (!form.isClosed || form.appointmentConflictAction !== 'move') return true
  if (!form.moveAppointmentsToDate) { showError('يجب تحديد تاريخ جديد عند نقل الحجوزات.'); return false }
  if (form.moveAppointmentsToDate < minMoveDate.value) { showError('تاريخ النقل يجب أن يكون بعد تاريخ الاستثناء.'); return false }
  if (form.moveAppointmentsToDate > maxMoveDate.value) { showError(`تاريخ النقل يجب أن يكون ضمن نافذة الحجز (${bookingWindowDays.value} يوم).`); return false }
  if (isMoveDateClosed.value) { showError('تاريخ النقل المحدد مغلق لهذه العيادة.'); return false }
  return true
}

async function loadClinics() {
  const r = await api.get<ApiResponse<ClinicItem[]>>('/Clinic/my')
  clinics.value = r.data.data
  if (!clinicId.value && clinics.value.length) clinicId.value = String(clinics.value[0].id)
}

async function loadExceptions() {
  if (!clinicId.value) { exceptions.value = []; page.value = 1; return }
  loading.value = true
  try {
    const r = await api.get<ApiResponse<ClinicExceptionItem[]>>(`/ClinicException/my/${clinicId.value}`, {
      params: { fromDate: filters.fromDate || undefined, toDate: filters.toDate || undefined },
    })
    exceptions.value = r.data.data
    normalizePage()
  } catch (e) { showError(getErrorMessage(e)) }
  finally { loading.value = false }
}

function applyFilters() { page.value = 1; loadExceptions() }

function openEditor(item?: ClinicExceptionItem) {
  Object.assign(form, item ? {
    id: item.id, exceptionDate: item.exceptionDate, isClosed: item.isClosed,
    closureReason: item.closureReason ?? '', maxAppointments: item.maxAppointments?.toString() ?? '',
    startTime: item.startTime?.slice(0, 5) ?? '', endTime: item.endTime?.slice(0, 5) ?? '',
    appointmentConflictAction: 'cancel', moveAppointmentsToDate: '',
  } : {
    id: 0, exceptionDate: '', isClosed: true, closureReason: '',
    maxAppointments: '', startTime: '', endTime: '',
    appointmentConflictAction: 'cancel', moveAppointmentsToDate: '',
  })
  editorOpen.value = true
}

async function saveException() {
  if (!validateMoveDate()) return
  saving.value = true
  try {
    const r = await api.post<ApiResponse<object>>('/ClinicException/my', {
      id: form.id || null,
      clinicId: Number(clinicId.value),
      exceptionDate: form.exceptionDate,
      isClosed: form.isClosed,
      closureReason: form.closureReason || null,
      maxAppointments: form.isClosed || form.maxAppointments === '' ? null : Number(form.maxAppointments),
      startTime: form.isClosed || !form.startTime ? null : `${form.startTime}:00`,
      endTime: form.isClosed || !form.endTime ? null : `${form.endTime}:00`,
      appointmentConflictAction: form.isClosed ? form.appointmentConflictAction : null,
      moveAppointmentsToDate: form.isClosed && form.appointmentConflictAction === 'move' ? form.moveAppointmentsToDate : null,
    })
    showSuccess(r.data.message)
    editorOpen.value = false
    await loadExceptions()
  } catch (e) { showError(getErrorMessage(e)) }
  finally { saving.value = false }
}

async function confirmDelete() {
  if (!deleteException.value) return
  try {
    const r = await api.delete<ApiResponse<object>>(`/ClinicException/my/${deleteException.value.id}`)
    showSuccess(r.data.message)
    deleteDialog.value = false
    deleteException.value = undefined
    await loadExceptions()
  } catch (e) { showError(getErrorMessage(e)) }
}

onMounted(async () => {
  try { await loadClinics(); await loadExceptions() }
  catch (e) { showError(getErrorMessage(e)) }
})
</script>

<template>
  <div class="exceptions-page">

    <!-- Header -->
    <div class="page-top">
      <div>
        <p class="page-kicker">تنظيم الدوام</p>
        <h1 class="page-title">الإجازات والاستثناءات</h1>
      </div>
      <div class="page-actions">
        <v-btn variant="outlined" color="primary" prepend-icon="mdi-refresh" :loading="loading" @click="loadExceptions">
          تحديث
        </v-btn>
        <v-btn color="primary" prepend-icon="mdi-plus" :disabled="!clinicId" @click="openEditor()">
          إضافة استثناء
        </v-btn>
      </div>
    </div>

    <!-- Filters -->
    <div class="filters-bar">
      <div class="filter-field">
        <label class="filter-label">العيادة</label>
        <v-autocomplete
          v-model="clinicId"
          :items="clinics.map(c => ({ value: c.id, label: c.name }))"
          item-title="label"
          item-value="value"
          class="filter-select"
          density="compact"
          variant="outlined"
          hide-details
          placeholder="اختر العيادة"
          @update:model-value="applyFilters"
        />
      </div>
      <div class="filter-field">
        <label class="filter-label">من تاريخ</label>
        <input v-model="filters.fromDate" type="date" class="filter-input" />
      </div>
      <div class="filter-field">
        <label class="filter-label">إلى تاريخ</label>
        <input v-model="filters.toDate" type="date" class="filter-input" />
      </div>
      <v-btn color="primary" prepend-icon="mdi-magnify" class="filter-btn" @click="applyFilters">
        تطبيق
      </v-btn>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="exceptions-grid">
      <v-skeleton-loader v-for="i in 3" :key="i" type="card" height="180" />
    </div>

    <!-- No Clinics -->
    <EmptyState
      v-else-if="!clinics.length"
      icon="mdi-hospital-building"
      title="لا توجد عيادات"
      description="أضف عيادة أولاً حتى تتمكن من إدارة الإجازات والاستثناءات"
    />

    <!-- No Exceptions -->
    <EmptyState
      v-else-if="!exceptions.length"
      icon="mdi-calendar-check"
      title="لا توجد استثناءات"
      :description="`دوام ${selectedClinicName} يعمل وفق الجدول الأسبوعي`"
    />

    <!-- Exceptions Grid -->
    <div v-else class="exceptions-grid">
      <div
        v-for="item in paginatedExceptions"
        :key="item.id"
        class="exception-card"
        :class="item.isClosed ? 'exception-card--closed' : 'exception-card--custom'"
      >
        <!-- Badge -->
        <div class="exception-card-top">
          <v-chip
            size="small"
            :color="item.isClosed ? 'error' : 'warning'"
            variant="tonal"
          >
            <v-icon :icon="item.isClosed ? 'mdi-lock' : 'mdi-calendar-edit'" size="14" start />
            {{ item.isClosed ? 'إغلاق كامل' : 'دوام مخصص' }}
          </v-chip>
        </div>

        <!-- Date -->
        <h3 class="exception-date">{{ formatDate(item.exceptionDate) }}</h3>

        <!-- Reason -->
        <p v-if="item.closureReason" class="exception-reason">{{ item.closureReason }}</p>

        <!-- Custom Schedule Details -->
        <div v-if="!item.isClosed" class="exception-details">
          <span class="detail-chip">
            <v-icon icon="mdi-account-group" size="14" />
            الأدوار: {{ item.maxAppointments ?? 'حسب الجدول' }}
          </span>
          <span v-if="item.startTime" class="detail-chip">
            <v-icon icon="mdi-clock" size="14" />
            {{ item.startTime.slice(0, 5) }} - {{ item.endTime?.slice(0, 5) }}
          </span>
        </div>

        <!-- Actions -->
        <div class="exception-actions">
          <v-btn size="small" variant="text" color="primary" prepend-icon="mdi-pencil" @click="openEditor(item)">
            تعديل
          </v-btn>
          <v-btn
            size="small"
            variant="text"
            color="error"
            prepend-icon="mdi-delete"
            @click="deleteException = item; deleteDialog = true"
          >
            حذف
          </v-btn>
        </div>
      </div>
    </div>

    <!-- Pagination -->
    <div v-if="totalPages > 1" class="pagination-bar">
      <v-pagination v-model="page" :length="totalPages" :total-visible="5" density="compact" color="primary" />
    </div>

    <!-- ── Editor Dialog ── -->
    <v-dialog v-model="editorOpen" max-width="560" scrollable>
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon :icon="form.id ? 'mdi-pencil' : 'mdi-calendar-plus'" color="primary" size="20" />
          {{ form.id ? 'تعديل الاستثناء' : 'إضافة استثناء جديد' }}
        </v-card-title>
        <v-divider />

        <v-card-text class="dialog-body">
          <div class="form-fields">
            <!-- Date -->
            <div class="form-field">
              <label class="form-label">التاريخ <span class="required">*</span></label>
              <input v-model="form.exceptionDate" type="date" class="form-input" required />
            </div>

            <!-- isClosed Toggle -->
            <label class="check-label">
              <input v-model="form.isClosed" type="checkbox" class="check-native" />
              <span class="check-box">
                <v-icon v-if="form.isClosed" icon="mdi-check" size="12" color="white" />
              </span>
              <span>إغلاق العيادة بالكامل في هذا اليوم</span>
            </label>

            <!-- Custom Schedule (not closed) -->
            <template v-if="!form.isClosed">
              <div class="form-field">
                <label class="form-label">عدد الأدوار المتاحة</label>
                <input
                  v-model="form.maxAppointments"
                  type="number"
                  min="0"
                  class="form-input"
                  placeholder="اتركه فارغاً لاستخدام الجدول الأسبوعي"
                />
              </div>
              <div class="form-row">
                <div class="form-field">
                  <label class="form-label">من الساعة</label>
                  <input v-model="form.startTime" type="time" class="form-input" />
                </div>
                <div class="form-field">
                  <label class="form-label">إلى الساعة</label>
                  <input v-model="form.endTime" type="time" class="form-input" />
                </div>
              </div>
            </template>

            <!-- Conflict Action (closed) -->
            <div v-if="form.isClosed" class="conflict-box">
              <p class="conflict-title">
                <v-icon icon="mdi-alert" color="warning" size="16" />
                معالجة حجوزات هذا اليوم
              </p>
              <label class="radio-label">
                <input v-model="form.appointmentConflictAction" type="radio" value="move" />
                <span>نقل وتوزيع الحجوزات على الأيام المتاحة ابتداءً من تاريخ جديد</span>
              </label>
              <label class="radio-label">
                <input v-model="form.appointmentConflictAction" type="radio" value="cancel" />
                <span>إلغاء جميع الحجوزات وإرسال إشعار للمرضى</span>
              </label>

              <!-- Move Date -->
              <div v-if="form.appointmentConflictAction === 'move'" class="form-field move-date-field">
                <label class="form-label">التاريخ الجديد للنقل</label>
                <input
                  v-model="form.moveAppointmentsToDate"
                  type="date"
                  class="form-input"
                  :min="minMoveDate"
                  :max="maxMoveDate"
                  required
                />
                <p class="field-hint">{{ moveDateHint }}</p>
              </div>
            </div>

            <!-- Notes -->
            <div class="form-field">
              <label class="form-label">ملاحظة أو سبب الإغلاق</label>
              <textarea v-model="form.closureReason" class="form-textarea" rows="3" maxlength="500" />
            </div>
          </div>
        </v-card-text>

        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="editorOpen = false">تراجع</v-btn>
          <v-btn color="primary" :loading="saving" @click="saveException">
            {{ form.id ? 'حفظ التعديلات' : 'حفظ الاستثناء' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Delete Dialog ── -->
    <v-dialog v-model="deleteDialog" max-width="420">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-alert" color="error" size="20" />
          حذف الاستثناء
        </v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          سيعود هذا اليوم للعمل وفق جدول الدوام الأسبوعي بعد حذف الاستثناء.
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="deleteDialog = false; deleteException = undefined">تراجع</v-btn>
          <v-btn color="error" @click="confirmDelete">تأكيد الحذف</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </div>
</template>

<style scoped>
.exceptions-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

/* Page Top */
.page-top { display: flex; align-items: center; justify-content: space-between; gap: var(--spacing-lg); flex-wrap: wrap; }
.page-kicker { margin: 0 0 4px 0; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--color-text-muted); }
.page-title { margin: 0; font-size: 28px; font-weight: 800; color: var(--color-text); }
.page-actions { display: flex; gap: var(--spacing-md); }

/* Filters */
.filters-bar {
  display: flex;
  align-items: flex-end;
  gap: var(--spacing-md);
  flex-wrap: wrap;
  padding: var(--spacing-lg);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}
.filter-field { display: flex; flex-direction: column; gap: 6px; }
.filter-label { font-size: 12px; font-weight: 700; color: var(--color-text-muted); }
.filter-input, .filter-select {
  height: 40px; padding: 0 12px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-size: 14px; outline: none; min-width: 150px;
  transition: border-color 0.2s;
}
.filter-input:focus, .filter-select:focus { border-color: var(--color-primary); }
.filter-btn { align-self: flex-end; }

/* Exceptions Grid */
.exceptions-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--spacing-lg);
}

/* Exception Card */
.exception-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  box-shadow: var(--shadow-sm);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  border-top: 3px solid transparent;
  transition: box-shadow 0.2s ease;
}
.exception-card:hover { box-shadow: var(--shadow-md); }
.exception-card--closed { border-top-color: var(--color-error); }
.exception-card--custom { border-top-color: var(--color-warning); }

.exception-card-top { display: flex; }

.exception-date {
  margin: 0;
  font-size: 15px;
  font-weight: 700;
  color: var(--color-text);
  line-height: 1.4;
}

.exception-reason {
  margin: 0;
  font-size: 13px;
  color: var(--color-text-muted);
  line-height: 1.5;
}

.exception-details { display: flex; gap: var(--spacing-sm); flex-wrap: wrap; }
.detail-chip {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px 10px;
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: 999px;
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text);
}

.exception-actions {
  display: flex;
  gap: var(--spacing-sm);
  margin-top: auto;
  padding-top: var(--spacing-sm);
  border-top: 1px solid var(--color-border-light);
}

/* Pagination */
.pagination-bar { display: flex; justify-content: center; }

/* Dialog */
.dialog-title {
  display: flex; align-items: center; gap: var(--spacing-md);
  padding: var(--spacing-lg) !important;
  font-size: 16px !important; font-weight: 700; color: var(--color-text);
}
.dialog-body { padding: var(--spacing-lg) !important; }
.dialog-actions { padding: var(--spacing-lg) !important; gap: var(--spacing-md); justify-content: flex-end; }

/* Form */
.form-fields { display: flex; flex-direction: column; gap: var(--spacing-lg); }
.form-field { display: flex; flex-direction: column; gap: 6px; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: var(--spacing-lg); }
.form-label { font-size: 13px; font-weight: 600; color: var(--color-text); }
.required { color: var(--color-error); }
.form-input, .form-textarea {
  padding: 10px 12px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-size: 14px; outline: none; width: 100%;
  transition: border-color 0.2s;
}
.form-input:focus, .form-textarea:focus { border-color: var(--color-primary); }
.form-textarea { resize: vertical; }
.field-hint { margin: 4px 0 0 0; font-size: 12px; color: var(--color-text-muted); }

/* Check */
.check-label {
  display: flex; align-items: center; gap: var(--spacing-md);
  cursor: pointer; font-size: 14px; font-weight: 500;
  color: var(--color-text); user-select: none;
}
.check-native { position: absolute; opacity: 0; width: 0; height: 0; }
.check-box {
  width: 18px; height: 18px;
  border: 2px solid var(--color-border);
  border-radius: 4px; background: white;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0; transition: all 0.15s ease;
}
.check-native:checked ~ .check-box { background: var(--color-primary); border-color: var(--color-primary); }

/* Conflict Box */
.conflict-box {
  display: flex; flex-direction: column; gap: var(--spacing-md);
  padding: var(--spacing-lg);
  border: 1px solid var(--color-warning-light);
  border-radius: var(--radius-md);
  background: var(--color-warning-light);
}
.conflict-title {
  display: flex; align-items: center; gap: var(--spacing-sm);
  margin: 0; font-size: 13px; font-weight: 700;
  color: var(--color-warning);
}
.radio-label {
  display: flex; align-items: flex-start; gap: var(--spacing-md);
  font-size: 13px; font-weight: 500; color: var(--color-text);
  cursor: pointer; line-height: 1.5;
}
.radio-label input[type="radio"] { margin-top: 2px; flex-shrink: 0; accent-color: var(--color-primary); }
.move-date-field { background: white; padding: var(--spacing-md); border-radius: var(--radius-md); }

/* Responsive */
@media (max-width: 768px) { .exceptions-grid { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 600px) { .exceptions-grid { grid-template-columns: 1fr; } .filters-bar { flex-direction: column; } .form-row { grid-template-columns: 1fr; } }
</style>