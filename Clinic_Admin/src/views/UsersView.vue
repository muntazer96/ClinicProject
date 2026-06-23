<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { CheckCircle2, Clipboard, Eye, LockKeyhole, LockKeyholeOpen, Pencil, RefreshCw, Search, Trash2, UserPlus, UserRound, UsersRound, XCircle } from '@lucide/vue'
import AppModal from '../components/AppModal.vue'
import AppPagination from '../components/AppPagination.vue'
import api from '../services/api'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, PageResult, UserItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const notifications = useNotificationsStore()
const users = ref<UserItem[]>([])
const loading = ref(false)
const search = ref('')
const page = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)
const selectedUser = ref<UserItem>()
const detailsUser = ref<UserItem>()
const createOpen = ref(false)
const saving = ref(false)
const createForm = ref({ name: '', phoneNumber: '', password: '' })
const editUser = ref<UserItem>()
const editForm = ref({ name: '', phoneNumber: '' })

const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i
function isProtected(user: UserItem) {
  return user.userName?.toLocaleLowerCase() === 'superadmin'
}

function roleLabel(role?: string) {
  return role === 'SuperAdmin' ? 'مدير النظام' : role === 'DoctorUser' ? 'طبيب' : role === 'ClinicStaff' ? 'موظف عيادة' : 'مستخدم'
}

function formatDate(date?: string) {
  return date ? new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(date)) : 'لم يسجل دخولاً بعد'
}

function confirmedLabel(confirmed: boolean) {
  return confirmed ? 'مؤكد' : 'غير مؤكد'
}

async function copyUserId(id: string) {
  try {
    await navigator.clipboard.writeText(id)
    notifications.show('تم نسخ المعرّف بنجاح.')
  } catch {
    notifications.show('تعذر نسخ المعرّف تلقائياً.', 'error')
  }
}

async function loadUsers() {
  loading.value = true
  try {
    const term = search.value.trim()
    const response = await api.get<ApiResponse<PageResult<UserItem>>>('/User', {
      params: { userGuid: guidPattern.test(term) ? term : undefined, search: term && !guidPattern.test(term) ? term : undefined, page: page.value, pageSize: 10 },
    })
    users.value = response.data.data.items
    totalPages.value = response.data.data.totalPages
    totalItems.value = response.data.data.totalItems
  } catch (error: any) {
    if (error.response?.status === 404) {
      users.value = []
      totalPages.value = 1
      totalItems.value = 0
    } else {
      notifications.show(getErrorMessage(error), 'error')
    }
  } finally {
    loading.value = false
  }
}

async function createUser() {
  saving.value = true
  try {
    const response = await api.post<ApiResponse<object>>('/User/signup', createForm.value)
    notifications.show(response.data.message)
    createOpen.value = false
    createForm.value = { name: '', phoneNumber: '', password: '' }
    await loadUsers()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    saving.value = false
  }
}

function openEditUser(user: UserItem) {
  editUser.value = user
  editForm.value = {
    name: user.name ?? '',
    phoneNumber: user.phoneNumber ?? '',
  }
}

async function updateUser() {
  if (!editUser.value) return
  saving.value = true
  try {
    const response = await api.put<ApiResponse<string>>(`/User/${editUser.value.id}`, editForm.value)
    notifications.show(response.data.message)
    editUser.value = undefined
    await loadUsers()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    saving.value = false
  }
}

function runSearch() {
  page.value = 1
  loadUsers()
}

async function toggleLock(user: UserItem) {
  if (isProtected(user)) return
  try {
    const response = await api.post<ApiResponse<string>>(`/User/${user.id}/lock-toggle`)
    notifications.show(response.data.message)
    await loadUsers()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

async function deleteUser() {
  if (!selectedUser.value || isProtected(selectedUser.value)) return
  try {
    const response = await api.delete<ApiResponse<string>>(`/User/${selectedUser.value.id}`)
    notifications.show(response.data.message)
    selectedUser.value = undefined
    await loadUsers()
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  }
}

function changePage(newPage: number) {
  page.value = newPage
  loadUsers()
}

onMounted(loadUsers)
</script>

<template>
  <div>
    <div class="page-heading">
      <div>
        <span class="section-kicker">إدارة الحسابات</span>
        <h2>المستخدمون</h2>
        <p>راجع حسابات النظام وتحكم بحالة الوصول عند الحاجة.</p>
      </div>
      <div class="heading-actions">
        <button class="secondary-button" type="button" :disabled="loading" @click="loadUsers"><RefreshCw :size="17" /> تحديث</button>
        <button class="compact-primary" type="button" @click="createOpen = true"><UserPlus :size="17" /> حساب جديد</button>
      </div>
    </div>

    <section class="table-card">
      <div class="table-toolbar">
        <label class="search-box"><Search :size="18" /><input v-model="search" placeholder="ابحث بالاسم أو الهاتف أو المعرّف" @keyup.enter="runSearch" /></label>
        <button class="compact-primary" type="button" @click="runSearch">بحث</button>
        <span class="records-count"><UsersRound :size="16" /> {{ totalItems }} حساب</span>
      </div>
      <div class="table-scroll">
        <table class="data-table">
          <thead><tr><th>المستخدم</th><th>بيانات التواصل</th><th>الدور</th><th>الطبيب المرتبط</th><th>آخر دخول</th><th>الحالة</th><th>الإجراءات</th></tr></thead>
          <tbody>
            <tr v-if="loading"><td colspan="7" class="table-message">جارِ تحميل المستخدمين...</td></tr>
            <tr v-else-if="!users.length"><td colspan="7" class="table-message">لا توجد حسابات مطابقة.</td></tr>
            <tr v-for="user in users" v-else :key="user.id">
              <td><div class="entity-cell"><span class="small-avatar"><UserRound :size="17" /></span><div><strong>{{ user.name || user.userName }}</strong><small>{{ user.userName || user.id }}</small></div></div></td>
              <td>
                <div class="contact-stack">
                  <span>
                    <b>{{ user.phoneNumber || '-' }}</b>
                    <small :class="user.phoneNumberConfirmed ? 'confirm-ok' : 'confirm-wait'"><CheckCircle2 v-if="user.phoneNumberConfirmed" :size="14" /><XCircle v-else :size="14" /> {{ confirmedLabel(user.phoneNumberConfirmed) }}</small>
                  </span>
                </div>
              </td>
              <td><span class="soft-badge">{{ roleLabel(user.roleName) }}</span></td>
              <td>
                <RouterLink v-if="user.linkedDoctor" class="linked-doctor-cell" :to="`/doctors/${user.linkedDoctor.id}`">
                  <strong>{{ user.linkedDoctor.name }}</strong>
                  <small>{{ user.linkedDoctor.specializationName }}</small>
                </RouterLink>
                <span v-else class="status-badge status-neutral">غير مرتبط</span>
              </td>
              <td class="muted-cell">{{ formatDate(user.lastLoginDate) }}</td>
              <td><span class="status-badge" :class="user.isLocked ? 'status-danger' : 'status-success'">{{ user.isLocked ? 'موقوف' : 'فعّال' }}</span></td>
              <td><div class="row-actions">
                <button type="button" title="عرض بيانات المستخدم" @click="detailsUser = user"><Eye :size="17" /></button>
                <button type="button" title="تعديل المستخدم" :disabled="isProtected(user)" @click="openEditUser(user)"><Pencil :size="17" /></button>
                <button type="button" :title="user.isLocked ? 'إلغاء الإيقاف' : 'إيقاف الحساب'" :disabled="isProtected(user)" @click="toggleLock(user)"><LockKeyholeOpen v-if="user.isLocked" :size="17" /><LockKeyhole v-else :size="17" /></button>
                <button class="danger-action" type="button" title="حذف الحساب" :disabled="isProtected(user)" @click="selectedUser = user"><Trash2 :size="17" /></button>
              </div></td>
            </tr>
          </tbody>
        </table>
      </div>
      <AppPagination :page="page" :total-pages="totalPages" @change="changePage" />
    </section>

    <AppModal v-if="editUser" title="تعديل المستخدم" @close="editUser = undefined">
      <form class="modal-form" @submit.prevent="updateUser">
        <p class="modal-copy">إذا تم تغيير رقم الهاتف ستعود حالة التأكيد إلى غير مؤكد حتى يتم التحقق من القيمة الجديدة.</p>
        <label><span>الاسم الكامل</span><input v-model="editForm.name" required maxlength="200" /></label>
        <label><span>رقم الهاتف</span><input v-model="editForm.phoneNumber" required maxlength="30" /></label>
        <div class="modal-actions"><button class="secondary-button" type="button" @click="editUser = undefined">تراجع</button><button class="compact-primary" type="submit" :disabled="saving">{{ saving ? 'جارِ الحفظ...' : 'حفظ التعديل' }}</button></div>
      </form>
    </AppModal>

    <AppModal v-if="createOpen" title="إنشاء حساب جديد" @close="createOpen = false">
      <form class="modal-form" @submit.prevent="createUser">
        <p class="modal-copy">ينشئ هذا الإجراء حساب مستخدم عادي يمكن ربطه لاحقاً بملف طبيب عند الحاجة.</p>
        <label><span>الاسم الكامل</span><input v-model="createForm.name" required maxlength="200" /></label>
        <label><span>رقم الهاتف</span><input v-model="createForm.phoneNumber" required maxlength="30" /></label>
        <label><span>كلمة المرور</span><input v-model="createForm.password" required type="password" minlength="6" /></label>
        <div class="modal-actions"><button class="secondary-button" type="button" @click="createOpen = false">تراجع</button><button class="compact-primary" type="submit" :disabled="saving">{{ saving ? 'جارِ الإنشاء...' : 'إنشاء الحساب' }}</button></div>
      </form>
    </AppModal>

    <AppModal v-if="detailsUser" title="بيانات المستخدم" wide @close="detailsUser = undefined">
      <div class="user-details-heading">
        <span class="user-details-avatar"><UserRound :size="25" /></span>
        <div>
          <h3>{{ detailsUser.name || detailsUser.userName || 'مستخدم' }}</h3>
          <p>{{ roleLabel(detailsUser.roleName) }}</p>
        </div>
        <span class="status-badge" :class="detailsUser.isLocked ? 'status-danger' : 'status-success'">{{ detailsUser.isLocked ? 'موقوف' : 'فعّال' }}</span>
      </div>

      <dl class="user-details-grid">
        <div class="full-detail">
          <dt>المعرّف</dt>
          <dd class="identifier-value"><code>{{ detailsUser.id }}</code><button type="button" title="نسخ المعرّف" @click="copyUserId(detailsUser.id)"><Clipboard :size="15" /></button></dd>
        </div>
        <div><dt>الاسم الكامل</dt><dd>{{ detailsUser.name || '-' }}</dd></div>
        <div><dt>اسم المستخدم</dt><dd>{{ detailsUser.userName || '-' }}</dd></div>
        <div><dt>رقم الهاتف</dt><dd class="verified-value">{{ detailsUser.phoneNumber || '-' }} <span :class="detailsUser.phoneNumberConfirmed ? 'confirm-ok' : 'confirm-wait'"><CheckCircle2 v-if="detailsUser.phoneNumberConfirmed" :size="14" /><XCircle v-else :size="14" /> {{ confirmedLabel(detailsUser.phoneNumberConfirmed) }}</span></dd></div>
        <div><dt>نوع الحساب</dt><dd>{{ roleLabel(detailsUser.roleName) }}</dd></div>
        <div><dt>الطبيب المرتبط</dt><dd><RouterLink v-if="detailsUser.linkedDoctor" class="details-link" :to="`/doctors/${detailsUser.linkedDoctor.id}`">{{ detailsUser.linkedDoctor.name }} - {{ detailsUser.linkedDoctor.specializationName }}</RouterLink><span v-else>-</span></dd></div>
        <div><dt>حالة أول دخول</dt><dd>{{ detailsUser.isFirstLogin ? 'لم يكتمل بعد' : 'مكتمل' }}</dd></div>
        <div><dt>آخر تسجيل دخول</dt><dd>{{ formatDate(detailsUser.lastLoginDate) }}</dd></div>
        <div v-if="detailsUser.roleId" class="full-detail"><dt>معرّف الدور</dt><dd><code>{{ detailsUser.roleId }}</code></dd></div>
      </dl>

      <div class="modal-actions"><button class="secondary-button" type="button" @click="detailsUser = undefined">إغلاق</button></div>
    </AppModal>

    <AppModal v-if="selectedUser" title="تأكيد حذف الحساب" @close="selectedUser = undefined">
      <p class="modal-copy">سيتم حذف حساب <strong>{{ selectedUser.name || selectedUser.userName }}</strong> حذفاً منطقياً ومنعه من استخدام النظام.</p>
      <div class="modal-actions"><button class="secondary-button" type="button" @click="selectedUser = undefined">تراجع</button><button class="danger-button" type="button" @click="deleteUser">تأكيد الحذف</button></div>
    </AppModal>
  </div>
</template>

<style scoped>
.user-details-heading { display: flex; align-items: center; gap: 11px; padding-bottom: 15px; border-bottom: 1px solid var(--line); }
.user-details-heading h3 { margin: 0 0 4px; font-size: 18px; }.user-details-heading p { margin: 0; color: var(--muted); font-size: 13px; }.user-details-heading .status-badge { margin-right: auto; }
.user-details-avatar { display: grid; place-items: center; width: 48px; height: 48px; color: var(--primary); border-radius: 14px; background: var(--primary-soft); }
.user-details-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 10px; margin: 15px 0 0; }
.user-details-grid div { padding: 11px; border: 1px solid var(--line); border-radius: 9px; background: #fbfdfc; }.user-details-grid .full-detail { grid-column: 1 / -1; }
.user-details-grid dt { margin-bottom: 5px; color: var(--muted); font-size: 12px; }.user-details-grid dd { margin: 0; color: var(--ink); font-weight: 700; overflow-wrap: anywhere; }
.identifier-value { display: flex; align-items: center; gap: 7px; }.identifier-value code { direction: ltr; }.identifier-value button { display: grid; place-items: center; width: 29px; height: 29px; color: var(--primary); border: 1px solid var(--line); border-radius: 7px; background: #fff; }
.contact-stack { display: grid; gap: 7px; min-width: 210px; }.contact-stack span { display: flex; flex-wrap: wrap; align-items: center; gap: 6px; }.contact-stack b { direction: ltr; font-weight: 700; }
.linked-doctor-cell { display: grid; gap: 4px; min-width: 160px; color: var(--ink); text-decoration: none; }.linked-doctor-cell strong { font-size: 13px; }.linked-doctor-cell small { color: var(--muted); font-size: 11px; }.linked-doctor-cell:hover strong, .details-link:hover { color: var(--primary); }
.details-link { color: var(--primary); text-decoration: none; }
.confirm-ok, .confirm-wait { display: inline-flex; align-items: center; gap: 4px; padding: 3px 6px; border-radius: 13px; font-size: 11px; font-weight: 700; }
.confirm-ok { color: #167163; background: #e1f4ef; }.confirm-wait { color: #a46724; background: #fff1db; }
.verified-value { display: flex; flex-wrap: wrap; align-items: center; gap: 7px; }
@media (max-width: 600px) { .user-details-grid { grid-template-columns: 1fr; }.user-details-grid .full-detail { grid-column: auto; } }
</style>
