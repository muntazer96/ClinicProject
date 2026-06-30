<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import api from '../services/api'
import { useNotifications } from '../composables/useNotifications'
import type { ApiResponse, PageResult, UserItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import EmptyState from '../components/common/Emptystate.vue'

const router = useRouter()
const { success: showSuccess, error: showError } = useNotifications()

const users       = ref<UserItem[]>([])
const loading     = ref(false)
const saving      = ref(false)
const search      = ref('')
const page        = ref(1)
const totalPages  = ref(1)
const totalItems  = ref(0)

const createDialog  = ref(false)
const editDialog    = ref(false)
const detailsDialog = ref(false)
const deleteDialog  = ref(false)
const selectedUser  = ref<UserItem>()
const detailsUser   = ref<UserItem>()
const editUser      = ref<UserItem>()

const createForm = ref({ name: '', phoneNumber: '', password: '' })
const editForm   = ref({ name: '', phoneNumber: '' })

const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i

function isProtected(user: UserItem) {
  return user.userName?.toLowerCase() === 'superadmin'
}

function roleLabel(role?: string) {
  return role === 'SuperAdmin' ? 'مدير النظام'
    : role === 'DoctorUser'   ? 'طبيب'
    : role === 'ClinicStaff'  ? 'موظف عيادة'
    : 'مستخدم'
}

function formatDate(date?: string) {
  return date
    ? new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(date))
    : 'لم يسجل دخولاً بعد'
}

async function copyUserId(id: string) {
  try {
    await navigator.clipboard.writeText(id)
    showSuccess('تم نسخ المعرّف بنجاح.')
  } catch { showError('تعذر نسخ المعرّف تلقائياً.') }
}

async function loadUsers() {
  loading.value = true
  try {
    const term = search.value.trim()
    const r = await api.get<ApiResponse<PageResult<UserItem>>>('/User', {
      params: {
        userGuid: guidPattern.test(term) ? term : undefined,
        search:   term && !guidPattern.test(term) ? term : undefined,
        page: page.value, pageSize: 10,
      },
    })
    users.value      = r.data.data.items
    totalPages.value = r.data.data.totalPages
    totalItems.value = r.data.data.totalItems
  } catch (e: any) {
    if (e.response?.status === 404) { users.value = []; totalPages.value = 1; totalItems.value = 0 }
    else showError(getErrorMessage(e))
  } finally { loading.value = false }
}

async function createUser() {
  saving.value = true
  try {
    const r = await api.post<ApiResponse<object>>('/User/signup', createForm.value)
    showSuccess(r.data.message)
    createDialog.value = false
    createForm.value = { name: '', phoneNumber: '', password: '' }
    await loadUsers()
  } catch (e) { showError(getErrorMessage(e)) }
  finally { saving.value = false }
}

function openEdit(user: UserItem) {
  editUser.value = user
  editForm.value = { name: user.name ?? '', phoneNumber: user.phoneNumber ?? '' }
  editDialog.value = true
}

async function updateUser() {
  if (!editUser.value) return
  saving.value = true
  try {
    const r = await api.put<ApiResponse<string>>(`/User/${editUser.value.id}`, editForm.value)
    showSuccess(r.data.message)
    editDialog.value = false
    editUser.value = undefined
    await loadUsers()
  } catch (e) { showError(getErrorMessage(e)) }
  finally { saving.value = false }
}

async function toggleLock(user: UserItem) {
  if (isProtected(user)) return
  try {
    const r = await api.post<ApiResponse<string>>(`/User/${user.id}/lock-toggle`)
    showSuccess(r.data.message)
    await loadUsers()
  } catch (e) { showError(getErrorMessage(e)) }
}

async function deleteUser() {
  if (!selectedUser.value || isProtected(selectedUser.value)) return
  try {
    const r = await api.delete<ApiResponse<string>>(`/User/${selectedUser.value.id}`)
    showSuccess(r.data.message)
    deleteDialog.value = false
    selectedUser.value = undefined
    await loadUsers()
  } catch (e) { showError(getErrorMessage(e)) }
}

function runSearch() { page.value = 1; loadUsers() }
function changePage(n: number) { page.value = n; loadUsers() }

onMounted(loadUsers)
</script>

<template>
  <div class="users-page">

    <!-- Header -->
    <div class="page-top">
      <div>
        <p class="page-kicker">إدارة الحسابات</p>
        <h1 class="page-title">المستخدمون</h1>
      </div>
      <div class="page-actions">
        <v-btn variant="outlined" color="primary" prepend-icon="mdi-refresh" :loading="loading" @click="loadUsers">تحديث</v-btn>
        <v-btn color="primary" prepend-icon="mdi-account-plus" @click="createDialog = true">حساب جديد</v-btn>
      </div>
    </div>

    <!-- Table Card -->
    <v-card elevation="0" class="table-card">
      <!-- Toolbar -->
      <div class="table-toolbar">
        <div class="search-wrap">
          <v-icon icon="mdi-magnify" size="18" class="search-icon" />
          <input
            v-model="search"
            class="search-input"
            placeholder="ابحث بالاسم أو الهاتف أو المعرّف"
            @keyup.enter="runSearch"
          />
        </div>
        <v-btn color="primary" size="small" prepend-icon="mdi-magnify" @click="runSearch">بحث</v-btn>
        <v-chip size="small" color="primary" variant="tonal">
          <v-icon icon="mdi-account-group" size="14" start />
          {{ totalItems }} حساب
        </v-chip>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="table-loading">
        <v-skeleton-loader v-for="i in 5" :key="i" type="table-row" />
      </div>

      <!-- Empty -->
      <EmptyState v-else-if="!users.length" icon="mdi-account-off" title="لا توجد حسابات" description="لا توجد حسابات مطابقة للبحث" />

      <!-- Table -->
      <div v-else class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th>المستخدم</th>
              <th>بيانات التواصل</th>
              <th>الدور</th>
              <th>الطبيب المرتبط</th>
              <th>آخر دخول</th>
              <th>الحالة</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in users" :key="user.id">
              <!-- User -->
              <td>
                <div class="user-cell">
                  <div class="user-avatar">
                    <v-icon icon="mdi-account" size="18" color="primary" />
                  </div>
                  <div>
                    <strong>{{ user.name || user.userName }}</strong>
                    <p class="row-sub">{{ user.userName || user.id }}</p>
                  </div>
                </div>
              </td>
              <!-- Contact -->
              <td>
                <strong class="ltr">{{ user.phoneNumber || '-' }}</strong>
                <v-chip
                  size="x-small"
                  :color="user.phoneNumberConfirmed ? 'success' : 'warning'"
                  variant="tonal"
                  class="d-block mt-1"
                >
                  <v-icon :icon="user.phoneNumberConfirmed ? 'mdi-check-circle' : 'mdi-clock'" size="10" start />
                  {{ user.phoneNumberConfirmed ? 'مؤكد' : 'غير مؤكد' }}
                </v-chip>
              </td>
              <!-- Role -->
              <td>
                <v-chip size="small" variant="tonal" color="primary">{{ roleLabel(user.roleName) }}</v-chip>
              </td>
              <!-- Linked Doctor -->
              <td>
                <div
                  v-if="user.linkedDoctor"
                  class="linked-doctor"
                  style="cursor:pointer"
                  @click="router.push(`/doctors/${user.linkedDoctor.id}`)"
                >
                  <strong>{{ user.linkedDoctor.name }}</strong>
                  <p class="row-sub">{{ user.linkedDoctor.specializationName }}</p>
                </div>
                <v-chip v-else size="x-small" variant="tonal">غير مرتبط</v-chip>
              </td>
              <!-- Last Login -->
              <td class="muted-cell">{{ formatDate(user.lastLoginDate) }}</td>
              <!-- Status -->
              <td>
                <v-chip size="small" :color="user.isLocked ? 'error' : 'success'" variant="tonal">
                  {{ user.isLocked ? 'موقوف' : 'فعّال' }}
                </v-chip>
              </td>
              <!-- Actions -->
              <td>
                <div class="row-actions">
                  <v-btn icon size="small" variant="text" color="info" aria-label="عرض التفاصيل" @click="detailsUser = user; detailsDialog = true">
                    <v-icon icon="mdi-eye" size="16" />
                  </v-btn>
                  <v-btn icon size="small" variant="text" color="primary" aria-label="تعديل" :disabled="isProtected(user)" @click="openEdit(user)">
                    <v-icon icon="mdi-pencil" size="16" />
                  </v-btn>
                  <v-btn
                    icon size="small" variant="text"
                    :color="user.isLocked ? 'success' : 'warning'"
                    :aria-label="user.isLocked ? 'إلغاء الإيقاف' : 'إيقاف الحساب'"
                    :disabled="isProtected(user)"
                    @click="toggleLock(user)"
                  >
                    <v-icon :icon="user.isLocked ? 'mdi-lock-open' : 'mdi-lock'" size="16" />
                  </v-btn>
                  <v-btn icon size="small" variant="text" color="error" aria-label="حذف" :disabled="isProtected(user)" @click="selectedUser = user; deleteDialog = true">
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

    <!-- ── Create Dialog ── -->
    <v-dialog v-model="createDialog" max-width="460">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-account-plus" color="primary" size="20" />
          إنشاء حساب جديد
        </v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          <v-alert type="info" variant="tonal" density="compact" class="mb-4" icon="mdi-information">
            ينشئ هذا الإجراء حساب مستخدم عادي يمكن ربطه لاحقاً بملف طبيب عند الحاجة.
          </v-alert>
          <div class="form-fields">
            <div class="form-field">
              <label class="form-label">الاسم الكامل <span class="required">*</span></label>
              <input v-model="createForm.name" class="form-input" required maxlength="200" />
            </div>
            <div class="form-field">
              <label class="form-label">رقم الهاتف <span class="required">*</span></label>
              <input v-model="createForm.phoneNumber" class="form-input" required maxlength="30" />
            </div>
            <div class="form-field">
              <label class="form-label">كلمة المرور <span class="required">*</span></label>
              <input v-model="createForm.password" type="password" class="form-input" required minlength="6" />
            </div>
          </div>
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="createDialog = false">تراجع</v-btn>
          <v-btn color="primary" :loading="saving" @click="createUser">إنشاء الحساب</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Edit Dialog ── -->
    <v-dialog v-model="editDialog" max-width="460">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-pencil" color="primary" size="20" />
          تعديل المستخدم
        </v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          <v-alert type="warning" variant="tonal" density="compact" class="mb-4" icon="mdi-alert">
            إذا تم تغيير رقم الهاتف ستعود حالة التأكيد إلى غير مؤكد.
          </v-alert>
          <div class="form-fields">
            <div class="form-field">
              <label class="form-label">الاسم الكامل <span class="required">*</span></label>
              <input v-model="editForm.name" class="form-input" required maxlength="200" />
            </div>
            <div class="form-field">
              <label class="form-label">رقم الهاتف <span class="required">*</span></label>
              <input v-model="editForm.phoneNumber" class="form-input" required maxlength="30" />
            </div>
          </div>
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="editDialog = false">تراجع</v-btn>
          <v-btn color="primary" :loading="saving" @click="updateUser">حفظ التعديل</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Details Dialog ── -->
    <v-dialog v-model="detailsDialog" max-width="580" scrollable>
      <v-card v-if="detailsUser">
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-account-details" color="primary" size="20" />
          بيانات المستخدم
        </v-card-title>
        <v-divider />

        <v-card-text class="dialog-body">
          <!-- User Header -->
          <div class="details-header">
            <div class="details-avatar">
              <v-icon icon="mdi-account" size="26" color="primary" />
            </div>
            <div class="details-header-info">
              <h3>{{ detailsUser.name || detailsUser.userName || 'مستخدم' }}</h3>
              <p>{{ roleLabel(detailsUser.roleName) }}</p>
            </div>
            <v-chip :color="detailsUser.isLocked ? 'error' : 'success'" variant="tonal" size="small">
              {{ detailsUser.isLocked ? 'موقوف' : 'فعّال' }}
            </v-chip>
          </div>

          <!-- Details Grid -->
          <div class="details-grid">
            <!-- User ID -->
            <div class="detail-item detail-item--full">
              <p class="detail-label">المعرّف</p>
              <div class="id-row">
                <code class="id-value ltr">{{ detailsUser.id }}</code>
                <v-btn icon size="x-small" variant="outlined" @click="copyUserId(detailsUser.id)">
                  <v-icon icon="mdi-content-copy" size="14" />
                </v-btn>
              </div>
            </div>
            <!-- Full Name -->
            <div class="detail-item">
              <p class="detail-label">الاسم الكامل</p>
              <strong>{{ detailsUser.name || '-' }}</strong>
            </div>
            <!-- Username -->
            <div class="detail-item">
              <p class="detail-label">اسم المستخدم</p>
              <strong>{{ detailsUser.userName || '-' }}</strong>
            </div>
            <!-- Phone -->
            <div class="detail-item">
              <p class="detail-label">رقم الهاتف</p>
              <div class="phone-row">
                <strong class="ltr">{{ detailsUser.phoneNumber || '-' }}</strong>
                <v-chip
                  size="x-small"
                  :color="detailsUser.phoneNumberConfirmed ? 'success' : 'warning'"
                  variant="tonal"
                >
                  {{ detailsUser.phoneNumberConfirmed ? 'مؤكد' : 'غير مؤكد' }}
                </v-chip>
              </div>
            </div>
            <!-- Role -->
            <div class="detail-item">
              <p class="detail-label">نوع الحساب</p>
              <strong>{{ roleLabel(detailsUser.roleName) }}</strong>
            </div>
            <!-- First Login -->
            <div class="detail-item">
              <p class="detail-label">حالة أول دخول</p>
              <strong>{{ detailsUser.isFirstLogin ? 'لم يكتمل بعد' : 'مكتمل' }}</strong>
            </div>
            <!-- Last Login -->
            <div class="detail-item">
              <p class="detail-label">آخر تسجيل دخول</p>
              <strong>{{ formatDate(detailsUser.lastLoginDate) }}</strong>
            </div>
            <!-- Linked Doctor -->
            <div class="detail-item">
              <p class="detail-label">الطبيب المرتبط</p>
              <span
                v-if="detailsUser.linkedDoctor"
                class="linked-link"
                @click="router.push(`/doctors/${detailsUser.linkedDoctor.id}`); detailsDialog = false"
              >
                {{ detailsUser.linkedDoctor.name }} - {{ detailsUser.linkedDoctor.specializationName }}
              </span>
              <strong v-else>-</strong>
            </div>
            <!-- Role ID -->
            <div v-if="detailsUser.roleId" class="detail-item detail-item--full">
              <p class="detail-label">معرّف الدور</p>
              <code class="id-value ltr">{{ detailsUser.roleId }}</code>
            </div>
          </div>
        </v-card-text>

        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="detailsDialog = false">إغلاق</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Delete Dialog ── -->
    <v-dialog v-model="deleteDialog" max-width="420">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-alert" color="error" size="20" />
          تأكيد حذف الحساب
        </v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          سيتم حذف حساب <strong>{{ selectedUser?.name || selectedUser?.userName }}</strong> حذفاً منطقياً ومنعه من استخدام النظام.
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="deleteDialog = false; selectedUser = undefined">تراجع</v-btn>
          <v-btn color="error" @click="deleteUser">تأكيد الحذف</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </div>
</template>

<style scoped>
.users-page { display: flex; flex-direction: column; gap: var(--spacing-lg); }

/* Page Top */
.page-top { display: flex; align-items: center; justify-content: space-between; gap: var(--spacing-lg); flex-wrap: wrap; }
.page-kicker { margin: 0 0 4px 0; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--color-text-muted); }
.page-title { margin: 0; font-size: 28px; font-weight: 800; color: var(--color-text); }
.page-actions { display: flex; gap: var(--spacing-md); }

/* Table */
.table-card { border: 1px solid var(--color-border) !important; border-radius: var(--radius-lg) !important; overflow: hidden; }
.table-toolbar { display: flex; align-items: center; gap: var(--spacing-md); padding: var(--spacing-lg); border-bottom: 1px solid var(--color-border); flex-wrap: wrap; }
.search-wrap { position: relative; flex: 1; min-width: 200px; }
.search-icon { position: absolute; right: 10px; top: 50%; transform: translateY(-50%); color: var(--color-text-muted); pointer-events: none; }
[dir='rtl'] .search-icon { right: auto; left: 10px; }
.search-input { width: 100%; height: 38px; padding: 0 36px 0 12px; border: 1.5px solid var(--color-border); border-radius: var(--radius-md); background: var(--color-surface); color: var(--color-text); font-family: var(--font-family-primary); font-size: 14px; outline: none; transition: border-color 0.2s; }
[dir='rtl'] .search-input { padding: 0 12px 0 36px; }
.search-input:focus { border-color: var(--color-primary); }
.table-loading { padding: var(--spacing-lg); }
.table-scroll { overflow-x: auto; }
.data-table { width: 100%; border-collapse: collapse; font-size: 13px; }
.data-table th { padding: 12px 16px; text-align: right; font-size: 12px; font-weight: 700; color: var(--color-text-muted); background: var(--color-background); border-bottom: 1px solid var(--color-border); white-space: nowrap; }
.data-table td { padding: 12px 16px; border-bottom: 1px solid var(--color-border-light); vertical-align: middle; color: var(--color-text); }
.data-table tbody tr:hover { background: var(--color-background); }
.data-table tbody tr:last-child td { border-bottom: none; }
.user-cell { display: flex; align-items: center; gap: var(--spacing-md); }
.user-avatar { display: flex; align-items: center; justify-content: center; width: 36px; height: 36px; border-radius: var(--radius-md); background: var(--color-primary-soft); flex-shrink: 0; }
.row-sub { margin: 2px 0 0 0; font-size: 11px; color: var(--color-text-muted); }
.muted-cell { color: var(--color-text-muted); font-size: 12px; }
.ltr { direction: ltr; text-align: right; }
.linked-doctor { cursor: pointer; }
.linked-doctor strong { font-size: 13px; font-weight: 700; color: var(--color-primary); }
.row-actions { display: flex; gap: 2px; }
.pagination-bar { display: flex; justify-content: center; padding: var(--spacing-lg); border-top: 1px solid var(--color-border); }

/* Dialog */
.dialog-title { display: flex; align-items: center; gap: var(--spacing-md); padding: var(--spacing-lg) !important; font-size: 16px !important; font-weight: 700; color: var(--color-text); }
.dialog-body { padding: var(--spacing-lg) !important; }
.dialog-actions { padding: var(--spacing-lg) !important; gap: var(--spacing-md); justify-content: flex-end; }

/* Form */
.form-fields { display: flex; flex-direction: column; gap: var(--spacing-lg); }
.form-field { display: flex; flex-direction: column; gap: 6px; }
.form-label { font-size: 13px; font-weight: 600; color: var(--color-text); }
.required { color: var(--color-error); }
.form-input { padding: 10px 12px; border: 1.5px solid var(--color-border); border-radius: var(--radius-md); background: var(--color-surface); color: var(--color-text); font-family: var(--font-family-primary); font-size: 14px; outline: none; width: 100%; transition: border-color 0.2s; }
.form-input:focus { border-color: var(--color-primary); }

/* Details */
.details-header { display: flex; align-items: center; gap: var(--spacing-lg); padding-bottom: var(--spacing-lg); border-bottom: 1px solid var(--color-border); margin-bottom: var(--spacing-lg); }
.details-avatar { display: flex; align-items: center; justify-content: center; width: 52px; height: 52px; border-radius: var(--radius-lg); background: var(--color-primary-soft); flex-shrink: 0; }
.details-header-info { flex: 1; }
.details-header-info h3 { margin: 0 0 4px 0; font-size: 18px; font-weight: 700; color: var(--color-text); }
.details-header-info p { margin: 0; font-size: 13px; color: var(--color-text-muted); }
.details-grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--spacing-md); }
.detail-item { padding: var(--spacing-md); background: var(--color-background); border: 1px solid var(--color-border); border-radius: var(--radius-md); }
.detail-item--full { grid-column: 1 / -1; }
.detail-label { margin: 0 0 6px 0; font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--color-text-muted); }
.detail-item strong { font-size: 14px; font-weight: 700; color: var(--color-text); overflow-wrap: anywhere; }
.id-row { display: flex; align-items: center; gap: var(--spacing-md); }
.id-value { font-size: 12px; color: var(--color-text); background: white; padding: 4px 8px; border-radius: 4px; border: 1px solid var(--color-border); flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.phone-row { display: flex; align-items: center; gap: var(--spacing-sm); flex-wrap: wrap; }
.linked-link { color: var(--color-primary); font-weight: 700; cursor: pointer; font-size: 13px; }
.linked-link:hover { text-decoration: underline; }

/* Responsive */
@media (max-width: 600px) { .details-grid { grid-template-columns: 1fr; } .detail-item--full { grid-column: auto; } }
</style>
