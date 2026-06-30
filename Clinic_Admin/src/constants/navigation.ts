import type { NavItem } from '../components/common/Navigationdrawer.vue'

// Admin Navigation Items
const adminNavigation: NavItem[] = [
  {
    label: 'الرئيسية',
    to: '/',
    icon: 'mdi-home',
    roles: ['SuperAdmin', 'DoctorUser'],
  },
  {
    label: 'الإحصائيات',
    to: '/analytics',
    icon: 'mdi-chart-box',
    roles: ['SuperAdmin', 'DoctorUser'],
  },
  {
    label: 'إدارة النظام',
    icon: 'mdi-cog',
    roles: ['SuperAdmin'],
    children: [
      {
        label: 'المستخدمون',
        to: '/users',
        icon: 'mdi-account-multiple',
        roles: ['SuperAdmin'],
      },
      {
        label: 'الأطباء',
        to: '/doctors',
        icon: 'mdi-stethoscope',
        roles: ['SuperAdmin'],
      },
      {
        label: 'الاشتراكات',
        to: '/subscriptions',
        icon: 'mdi-receipt',
        roles: ['SuperAdmin'],
      },
      {
        label: 'WhatsApp OTP',
        to: '/whatsapp',
        icon: 'mdi-whatsapp',
        roles: ['SuperAdmin'],
      },
      {
        label: 'النسخ الاحتياطي',
        to: '/backup-restore',
        icon: 'mdi-database',
        roles: ['SuperAdmin'],
      },
      {
        label: 'طلبات التحويل',
        to: '/doctor-requests',
        icon: 'mdi-file-document-edit',
        roles: ['SuperAdmin'],
      },
    ],
  },
  {
    label: 'إصدارات التطبيق',
    icon: 'mdi-package',
    roles: ['SuperAdmin'],
    children: [
      {
        label: 'سياسات التحديث',
        to: '/app-versions',
        icon: 'mdi-file-document',
        roles: ['SuperAdmin'],
      },
      {
        label: 'إصدارات APK',
        to: '/app-releases',
        icon: 'mdi-android',
        roles: ['SuperAdmin'],
      },
    ],
  },
  {
    divider: true,
    roles: ['SuperAdmin', 'DoctorUser'],
  },
  {
    label: 'العروض والحجوزات',
    icon: 'mdi-offer',
    roles: ['SuperAdmin', 'DoctorUser'],
    children: [
      {
        label: 'العروض',
        to: '/offers',
        icon: 'mdi-percent',
        roles: ['SuperAdmin', 'DoctorUser'],
      },
      {
        label: 'الحجوزات',
        to: '/appointments',
        icon: 'mdi-calendar',
        roles: ['SuperAdmin', 'DoctorUser'],
      },
    ],
  },
  {
    divider: true,
    roles: ['DoctorUser'],
  },
  {
    label: 'عياداتي',
    to: '/clinics',
    icon: 'mdi-hospital-box',
    roles: ['DoctorUser'],
  },
  {
    label: 'الإجازات والاستثناءات',
    to: '/exceptions',
    icon: 'mdi-calendar-alert',
    roles: ['DoctorUser'],
  },
  {
    label: 'الإشعارات',
    to: '/notifications',
    icon: 'mdi-bell',
    roles: ['DoctorUser'],
  },
  {
    label: 'التقييمات',
    to: '/reviews',
    icon: 'mdi-star',
    roles: ['DoctorUser'],
  },
  {
    label: 'الملف الشخصي',
    to: '/profile',
    icon: 'mdi-account',
    roles: ['DoctorUser'],
  },
]

/**
 * Get navigation items filtered by user role
 * @param userRole - The user's role
 * @returns Filtered navigation items
 */
export function getNavigationItems(userRole: string | string[] | undefined): NavItem[] {
  if (!userRole) return []

  const roles = Array.isArray(userRole) ? userRole : [userRole]

  return filterNavItemsByRole(adminNavigation, roles)
}

/**
 * Filter navigation items recursively based on user roles
 */
function filterNavItemsByRole(items: NavItem[], userRoles: string[]): NavItem[] {
  return items
    .filter((item) => {
      // If divider, check roles
      if (item.divider) {
        return !item.roles || item.roles.some((role) => userRoles.includes(role))
      }

      // Regular item - check if user has required role
      if (!item.roles || item.roles.length === 0) return true
      return item.roles.some((role) => userRoles.includes(role))
    })
    .map((item) => {
      // Recursively filter children
      if (item.children && item.children.length > 0) {
        return {
          ...item,
          children: filterNavItemsByRole(item.children, userRoles),
        }
      }
      return item
    })
}

export default adminNavigation