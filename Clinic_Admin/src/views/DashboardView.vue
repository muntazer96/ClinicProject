<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { Building2, CalendarCheck2, CircleDollarSign, Clock3, MessageSquareText, Stethoscope, UsersRound } from '@lucide/vue'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, AppointmentItem, ClinicItem, DoctorItem, DoctorReviews, DoctorSubscription, PageResult, UserItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const auth = useAuthStore()
const notifications = useNotificationsStore()
const isAdmin = auth.hasAnyRole(['SuperAdmin'])
const loading = ref(false)
const stats = ref({ doctors: 0, users: 0, subscriptions: 0, appointments: 0, pending: 0, clinics: 0, reviews: 0 })

const adminStats = computed(() => [
  { label: 'الأطباء المسجلون', value: stats.value.doctors, note: 'إجمالي ملفات الأطباء', icon: Stethoscope, color: 'green' },
  { label: 'المستخدمون', value: stats.value.users, note: 'إجمالي حسابات النظام', icon: UsersRound, color: 'blue' },
  { label: 'الاشتراكات', value: stats.value.subscriptions, note: 'جميع سجلات الاشتراك', icon: CircleDollarSign, color: 'orange' },
])
const doctorStats = computed(() => [
  { label: 'حجوزات اليوم', value: stats.value.appointments, note: 'جميع العيادات التابعة لك', icon: CalendarCheck2, color: 'green' },
  { label: 'بانتظار التأكيد', value: stats.value.pending, note: 'حجوزات تحتاج المتابعة', icon: Clock3, color: 'orange' },
  { label: 'العيادات', value: stats.value.clinics, note: 'الفروع المسجلة في حسابك', icon: Building2, color: 'blue' },
  { label: 'التقييمات', value: stats.value.reviews, note: 'آراء المراجعين الموثقة', icon: MessageSquareText, color: 'purple' },
])
const quickLinks = computed(() => isAdmin ? [
  { to: '/users', label: 'إدارة المستخدمين' }, { to: '/doctors', label: 'إدارة الأطباء' }, { to: '/subscriptions', label: 'إدارة الاشتراكات' },
] : [
  { to: '/appointments', label: 'حجوزات اليوم' }, { to: '/clinics', label: 'العيادات والدوام' }, { to: '/exceptions', label: 'الإجازات' }, { to: '/reviews', label: 'التقييمات' },
])

async function pageTotal<T>(url: string) {
  try {
    const response = await api.get<ApiResponse<PageResult<T>>>(url, { params: { page: 1, pageSize: 1 } })
    return response.data.data.totalItems
  } catch (error: any) {
    if (error.response?.status === 404) return 0
    throw error
  }
}
async function loadAdminStats() {
  const [users, doctors, subscriptions] = await Promise.all([
    pageTotal<UserItem>('/User'), pageTotal<DoctorItem>('/Doctor'), pageTotal<DoctorSubscription>('/DoctorSubscription'),
  ])
  Object.assign(stats.value, { users, doctors, subscriptions })
}
async function loadDoctorStats() {
  const date = new Date().toLocaleDateString('en-CA')
  const [clinicsResponse, appointmentsResponse, reviewsResponse] = await Promise.all([
    api.get<ApiResponse<ClinicItem[]>>('/Clinic/my'),
    api.get<ApiResponse<AppointmentItem[]>>('/Appointment/doctor/my', { params: { fromDate: date, toDate: date } }),
    api.get<ApiResponse<DoctorReviews>>('/Review/doctor/my'),
  ])
  const appointments = appointmentsResponse.data.data
  Object.assign(stats.value, {
    clinics: clinicsResponse.data.data.length,
    appointments: appointments.length,
    pending: appointments.filter((appointment) => appointment.status === 0).length,
    reviews: reviewsResponse.data.data.reviewCount,
  })
}
onMounted(async () => {
  loading.value = true
  try { await (isAdmin ? loadAdminStats() : loadDoctorStats()) }
  catch (error) { notifications.show(getErrorMessage(error), 'error') }
  finally { loading.value = false }
})
</script>

<template>
  <div class="dashboard-view">
    <section class="welcome-card">
      <div>
        <span class="section-kicker">{{ isAdmin ? 'نظرة عامة على النظام' : 'ملخص يومك' }}</span>
        <h2>{{ isAdmin ? 'إدارة أكثر هدوءاً ووضوحاً.' : 'تابع عياداتك وحجوزاتك اليومية.' }}</h2>
        <p>{{ loading ? 'جارِ تحديث مؤشرات لوحة التحكم...' : 'تعرض هذه الصفحة آخر البيانات المتاحة من النظام.' }}</p>
      </div>
      <span class="welcome-badge">المرحلة الأولى جاهزة</span>
    </section>

    <section class="stats-grid" :class="{ 'stats-loading': loading }">
      <article v-for="stat in isAdmin ? adminStats : doctorStats" :key="stat.label" class="stat-card">
        <div class="stat-icon" :class="`stat-${stat.color}`"><component :is="stat.icon" :size="21" /></div>
        <span>{{ stat.label }}</span><strong>{{ stat.value }}</strong><small>{{ stat.note }}</small>
      </article>
    </section>

    <section class="content-grid">
      <article class="panel-card">
        <span class="section-kicker">الوصول السريع</span><h3>انتقل إلى وحدات العمل</h3>
        <div class="quick-links"><RouterLink v-for="link in quickLinks" :key="link.to" :to="link.to">{{ link.label }}</RouterLink></div>
      </article>
      <article class="panel-card muted-panel">
        <span class="section-kicker">حالة اللوحة</span><h3>البيانات متصلة بالنظام</h3>
        <p>المسارات محمية حسب الدور، وحالات التحميل والفراغ والأخطاء مهيأة للوحدات المنجزة.</p>
      </article>
    </section>
  </div>
</template>
