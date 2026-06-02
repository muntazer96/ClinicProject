<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { Clipboard, Eye, LockKeyhole, LockKeyholeOpen, RefreshCw, Search, Trash2, UserRound, UsersRound } from '@lucide/vue'
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
      <button class="secondary-button" type="button" :disabled="loading" @click="loadUsers"><RefreshCw :size="17" /> تحديث</button>
    </div>

    <section class="table-card">
      <div class="table-toolbar">
        <label class="search-box"><Search :size="18" /><input v-model="search" placeholder="ابحث بالاسم أو الهاتف أو المعرّف" @keyup.enter="runSearch" /></label>
        <button class="compact-primary" type="button" @click="runSearch">بحث</button>
        <span class="records-count"><UsersRound :size="16" /> {{ totalItems }} حساب</span>
      </div>
      <div class="table-scroll">
        <table class="data-table">
          <thead><tr><th>المستخدم</th><th>الدور</th><th>آخر دخول</th><th>الحالة</th><th>الإجراءات</th></tr></thead>
          <tbody>
            <tr v-if="loading"><td colspan="5" class="table-message">جارِ تحميل المستخدمين...</td></tr>
            <tr v-else-if="!users.length"><td colspan="5" class="table-message">لا توجد حسابات مطابقة.</td></tr>
            <tr v-for="user in users" v-else :key="user.id">
              <td><div class="entity-cell"><span class="small-avatar"><UserRound :size="17" /></span><div><strong>{{ user.name || user.userName }}</strong><small>{{ user.phoneNumber || user.id }}</small></div></div></td>
              <td><span class="soft-badge">{{ roleLabel(user.roleName) }}</span></td>
              <td class="muted-cell">{{ formatDate(user.lastLoginDate) }}</td>
              <td><span class="status-badge" :class="user.isLocked ? 'status-danger' : 'status-success'">{{ user.isLocked ? 'موقوف' : 'فعّال' }}</span></td>
              <td><div class="row-actions">
                <button type="button" title="عرض بيانات المستخدم" @click="detailsUser = user"><Eye :size="17" /></button>
                <button type="button" :title="user.isLocked ? 'إلغاء الإيقاف' : 'إيقاف الحساب'" :disabled="isProtected(user)" @click="toggleLock(user)"><LockKeyholeOpen v-if="user.isLocked" :size="17" /><LockKeyhole v-else :size="17" /></button>
                <button class="danger-action" type="button" title="حذف الحساب" :disabled="isProtected(user)" @click="selectedUser = user"><Trash2 :size="17" /></button>
              </div></td>
            </tr>
          </tbody>
        </table>
      </div>
      <AppPagination :page="page" :total-pages="totalPages" @change="changePage" />
    </section>

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
        <div><dt>رقم الهاتف</dt><dd>{{ detailsUser.phoneNumber || '-' }}</dd></div>
        <div><dt>البريد الإلكتروني</dt><dd>{{ detailsUser.email || '-' }}</dd></div>
        <div><dt>نوع الحساب</dt><dd>{{ roleLabel(detailsUser.roleName) }}</dd></div>
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
@media (max-width: 600px) { .user-details-grid { grid-template-columns: 1fr; }.user-details-grid .full-detail { grid-column: auto; } }
</style>
