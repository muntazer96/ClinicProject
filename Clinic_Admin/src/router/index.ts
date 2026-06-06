import { createRouter, createWebHistory } from 'vue-router'
import { pinia } from '../stores'
import { useAuthStore } from '../stores/auth'
import AdminLayout from '../layouts/AdminLayout.vue'
import AnalyticsView from '../views/AnalyticsView.vue'
import AppointmentsView from '../views/AppointmentsView.vue'
import AppVersionsView from '../views/AppVersionsView.vue'
import LoginView from '../views/LoginView.vue'
import ClinicsView from '../views/ClinicsView.vue'
import DashboardView from '../views/DashboardView.vue'
import DoctorDetailsView from '../views/DoctorDetailsView.vue'
import DoctorsView from '../views/DoctorsView.vue'
import EmailConfirmView from '../views/EmailConfirmView.vue'
import ExceptionsView from '../views/ExceptionsView.vue'
import GuestBookingView from '../views/GuestBookingView.vue'
import OffersView from '../views/OffersView.vue'
import PasswordResetView from '../views/PasswordResetView.vue'
import ProfileView from '../views/ProfileView.vue'
import PublicDirectoryView from '../views/PublicDirectoryView.vue'
import PublicDoctorView from '../views/PublicDoctorView.vue'
import ReviewsView from '../views/ReviewsView.vue'
import SubscriptionsView from '../views/SubscriptionsView.vue'
import UsersView from '../views/UsersView.vue'
import WhatsAppView from '../views/WhatsAppView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', name: 'login', component: LoginView, meta: { guestOnly: true } },
    { path: '/email-confirm', name: 'email-confirm', component: EmailConfirmView },
    { path: '/password-reset', name: 'password-reset', component: PasswordResetView },
    { path: '/directory', name: 'public-directory', component: PublicDirectoryView },
    { path: '/directory/:doctorId', name: 'public-doctor', component: PublicDoctorView },
    { path: '/guest-booking', name: 'guest-booking', component: GuestBookingView },
    {
      path: '/',
      component: AdminLayout,
      meta: { requiresAuth: true, roles: ['SuperAdmin', 'DoctorUser'] },
      children: [
        { path: '', name: 'dashboard', component: DashboardView, meta: { title: 'الرئيسية' } },
        { path: 'analytics', name: 'analytics', component: AnalyticsView, meta: { title: 'الإحصائيات', roles: ['SuperAdmin', 'DoctorUser'] } },
        { path: 'users', name: 'users', component: UsersView, meta: { title: 'إدارة المستخدمين', roles: ['SuperAdmin'] } },
        { path: 'doctors', name: 'doctors', component: DoctorsView, meta: { title: 'إدارة الأطباء', roles: ['SuperAdmin'] } },
        { path: 'doctors/:doctorId', name: 'doctor-details', component: DoctorDetailsView, meta: { title: 'تفاصيل الطبيب', roles: ['SuperAdmin'] } },
        { path: 'subscriptions', name: 'subscriptions', component: SubscriptionsView, meta: { title: 'الاشتراكات والباقات', roles: ['SuperAdmin'] } },
        { path: 'app-versions', name: 'app-versions', component: AppVersionsView, meta: { title: 'إصدارات التطبيق', roles: ['SuperAdmin'] } },
        { path: 'whatsapp', name: 'whatsapp', component: WhatsAppView, meta: { title: 'WhatsApp OTP', roles: ['SuperAdmin'] } },
        { path: 'offers', name: 'offers', component: OffersView, meta: { title: 'العروض', roles: ['SuperAdmin', 'DoctorUser'] } },
        { path: 'clinics', name: 'clinics', component: ClinicsView, meta: { title: 'عياداتي', roles: ['DoctorUser'] } },
        { path: 'appointments', name: 'appointments', component: AppointmentsView, meta: { title: 'الحجوزات اليومية', roles: ['SuperAdmin', 'DoctorUser'] } },
        { path: 'exceptions', name: 'exceptions', component: ExceptionsView, meta: { title: 'الإجازات والاستثناءات', roles: ['DoctorUser'] } },
        { path: 'reviews', name: 'reviews', component: ReviewsView, meta: { title: 'التقييمات', roles: ['DoctorUser'] } },
        { path: 'profile', name: 'profile', component: ProfileView, meta: { title: 'الملف الشخصي', roles: ['DoctorUser'] } },
      ],
    },
    { path: '/:pathMatch(.*)*', redirect: '/' },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore(pinia)
  if (to.meta.guestOnly && auth.isAuthenticated) return { name: 'dashboard' }
  if (to.meta.requiresAuth && !auth.isAuthenticated) return { name: 'login' }

  const roles = (to.meta.roles as string[] | undefined) ?? []
  if (roles.length && !auth.hasAnyRole(roles)) return { name: 'dashboard' }
})

export default router
