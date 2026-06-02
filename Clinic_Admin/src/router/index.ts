import { createRouter, createWebHistory } from 'vue-router'
import { pinia } from '../stores'
import { useAuthStore } from '../stores/auth'
import AdminLayout from '../layouts/AdminLayout.vue'
import AppointmentsView from '../views/AppointmentsView.vue'
import LoginView from '../views/LoginView.vue'
import ClinicsView from '../views/ClinicsView.vue'
import DashboardView from '../views/DashboardView.vue'
import DoctorsView from '../views/DoctorsView.vue'
import ExceptionsView from '../views/ExceptionsView.vue'
import PlaceholderView from '../views/PlaceholderView.vue'
import ReviewsView from '../views/ReviewsView.vue'
import SubscriptionsView from '../views/SubscriptionsView.vue'
import UsersView from '../views/UsersView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', name: 'login', component: LoginView, meta: { guestOnly: true } },
    {
      path: '/',
      component: AdminLayout,
      meta: { requiresAuth: true, roles: ['SuperAdmin', 'DoctorUser'] },
      children: [
        { path: '', name: 'dashboard', component: DashboardView, meta: { title: 'الرئيسية' } },
        { path: 'users', name: 'users', component: UsersView, meta: { title: 'إدارة المستخدمين', roles: ['SuperAdmin'] } },
        { path: 'doctors', name: 'doctors', component: DoctorsView, meta: { title: 'إدارة الأطباء', roles: ['SuperAdmin'] } },
        { path: 'subscriptions', name: 'subscriptions', component: SubscriptionsView, meta: { title: 'الاشتراكات والباقات', roles: ['SuperAdmin'] } },
        { path: 'clinics', name: 'clinics', component: ClinicsView, meta: { title: 'عياداتي', roles: ['DoctorUser'] } },
        { path: 'appointments', name: 'appointments', component: AppointmentsView, meta: { title: 'الحجوزات اليومية', roles: ['DoctorUser'] } },
        { path: 'exceptions', name: 'exceptions', component: ExceptionsView, meta: { title: 'الإجازات والاستثناءات', roles: ['DoctorUser'] } },
        { path: 'reviews', name: 'reviews', component: ReviewsView, meta: { title: 'التقييمات', roles: ['DoctorUser'] } },
        { path: 'profile', name: 'profile', component: PlaceholderView, meta: { title: 'الملف الشخصي', roles: ['DoctorUser'] } },
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
