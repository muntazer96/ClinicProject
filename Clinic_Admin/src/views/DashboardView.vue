<script setup lang="ts">
import { CalendarCheck2, CircleDollarSign, Clock3, Stethoscope, UsersRound } from '@lucide/vue'
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()
const isAdmin = auth.hasAnyRole(['SuperAdmin'])

const adminStats = [
  { label: 'الأطباء المسجلون', value: '—', note: 'سيتم ربط الإحصائيات في الخطوة التالية', icon: Stethoscope, color: 'green' },
  { label: 'المستخدمون', value: '—', note: 'إجمالي الحسابات الفعالة', icon: UsersRound, color: 'blue' },
  { label: 'الاشتراكات النشطة', value: '—', note: 'تحديث مباشر من النظام', icon: CircleDollarSign, color: 'orange' },
  { label: 'حجوزات اليوم', value: '—', note: 'عبر جميع العيادات', icon: CalendarCheck2, color: 'purple' },
]
const doctorStats = [
  { label: 'حجوزات اليوم', value: '—', note: 'جميع العيادات التابعة لك', icon: CalendarCheck2, color: 'green' },
  { label: 'بانتظار التأكيد', value: '—', note: 'حجوزات تحتاج المتابعة', icon: Clock3, color: 'orange' },
  { label: 'العيادات', value: '—', note: 'الفروع المسجلة في حسابك', icon: Stethoscope, color: 'blue' },
]
</script>

<template>
  <div class="dashboard-view">
    <section class="welcome-card">
      <div>
        <span class="section-kicker">{{ isAdmin ? 'نظرة عامة على النظام' : 'ملخص يومك' }}</span>
        <h2>{{ isAdmin ? 'إدارة أكثر هدوءاً ووضوحاً.' : 'تابع عياداتك وحجوزاتك اليومية.' }}</h2>
        <p>تم تجهيز الهيكل الأساسي للوحة، وستظهر البيانات المباشرة عند ربط وحدات الإدارة في الخطوات القادمة.</p>
      </div>
      <span class="welcome-badge">المرحلة الأولى جاهزة</span>
    </section>

    <section class="stats-grid">
      <article v-for="stat in isAdmin ? adminStats : doctorStats" :key="stat.label" class="stat-card">
        <div class="stat-icon" :class="`stat-${stat.color}`"><component :is="stat.icon" :size="21" /></div>
        <span>{{ stat.label }}</span>
        <strong>{{ stat.value }}</strong>
        <small>{{ stat.note }}</small>
      </article>
    </section>

    <section class="content-grid">
      <article class="panel-card">
        <div class="panel-heading">
          <div>
            <span class="section-kicker">البدء السريع</span>
            <h3>الوحدات القادمة</h3>
          </div>
        </div>
        <div class="timeline">
          <div><b>01</b><span>إدارة المستخدمين والأطباء</span></div>
          <div><b>02</b><span>إدارة الاشتراكات والباقات</span></div>
          <div><b>03</b><span>العيادات والدوام والحجوزات</span></div>
        </div>
      </article>
      <article class="panel-card muted-panel">
        <span class="section-kicker">حالة الاتصال</span>
        <h3>الواجهة جاهزة للربط</h3>
        <p>تم إعداد Axios وJWT وحماية المسارات حسب دور المستخدم.</p>
      </article>
    </section>
  </div>
</template>
