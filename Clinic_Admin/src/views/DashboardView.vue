<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { Activity, ArrowLeft, BadgePercent, CalendarClock, CalendarDays, Crown, Eye, RefreshCw, Search, Stethoscope, UsersRound } from '@lucide/vue'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'
import { useNotificationsStore } from '../stores/notifications'
import type { AnalyticsSummary, ApiResponse, CurrentDoctorSubscription } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const auth = useAuthStore()
const notifications = useNotificationsStore()
const isAdmin = computed(() => auth.hasAnyRole(['SuperAdmin']) && !auth.hasAnyRole(['DoctorUser']))
const loading = ref(false)
const summary = ref<AnalyticsSummary | null>(null)
const currentSubscription = ref<CurrentDoctorSubscription | null>(null)

const today = new Date()
const fromDate = toInputDate(new Date(today.getFullYear(), today.getMonth(), today.getDate() - 7))
const toDate = toInputDate(today)

const cards = computed(() => {
  const adminCards = [
    card('appointmentsInRange', CalendarDays, 'stat-green'),
    card('profileViews', Eye, 'stat-blue'),
    card('bookingClicks', Activity, 'stat-orange'),
    card('usersInRange', UsersRound, 'stat-purple'),
  ]
  const doctorCards = [
    card('todayBookings', CalendarDays, 'stat-green'),
    card('profileViews', Eye, 'stat-blue'),
    card('bookingClicks', Activity, 'stat-orange'),
    card('averageRating', Stethoscope, 'stat-purple'),
  ]
  return isAdmin.value ? adminCards : doctorCards
})

const quickRows = computed(() => isAdmin.value ? [
  { label: 'الأطباء الأكثر زيارة', rows: summary.value?.topDoctorsByViews ?? [] },
  { label: 'الأطباء الأكثر حجزا', rows: summary.value?.topDoctorsByBookings ?? [] },
  { label: 'أكثر كلمات البحث', rows: summary.value?.topSearchTerms ?? [] },
] : [
  { label: 'مصادر الحجز', rows: summary.value?.appointmentSources ?? [] },
  { label: 'أكثر أيام الحجز', rows: summary.value?.topBookingDays ?? [] },
  { label: 'أوقات الذروة', rows: summary.value?.peakBookingHours ?? [] },
])

const offerStats = computed(() => [
  { label: 'مشاهدات العروض', value: summary.value?.offers.views ?? 0 },
  { label: 'ضغطات العروض', value: summary.value?.offers.clicks ?? 0 },
  { label: 'حجوزات من العروض', value: summary.value?.offers.bookingsFromOffers ?? 0 },
])

const trend = computed(() => isAdmin.value ? summary.value?.userGrowth ?? [] : summary.value?.appointmentTrend ?? [])
const hasEvents = computed(() => (summary.value?.recentEvents.length ?? 0) > 0)
const daysRemaining = computed(() => currentSubscription.value?.daysRemaining ?? null)
const subscriptionTone = computed(() => {
  const days = daysRemaining.value
  if (days == null) return 'green'
  if (days <= 1) return 'red'
  if (days <= 3) return 'orange'
  return 'green'
})
const isPremiumSubscription = computed(() => currentSubscription.value?.isTopPackage ?? false)

function toInputDate(date: Date) {
  return date.toLocaleDateString('en-CA')
}

function metric(key: string) {
  return summary.value?.metrics.find((item) => item.key === key)
}

function card(key: string, icon: unknown, color: string) {
  return { metric: metric(key), icon, color }
}

function formatNumber(value = 0) {
  return new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 }).format(value)
}

function barHeight(value: number) {
  const max = Math.max(...trend.value.map((item) => Number(item.value)), 1)
  return `${Math.max(8, (Number(value) / max) * 100)}%`
}

async function loadSummary() {
  loading.value = true
  try {
    const endpoint = isAdmin.value ? '/Analytics/admin/summary' : '/Analytics/doctor/summary'
    const response = await api.get<ApiResponse<AnalyticsSummary>>(endpoint, {
      params: { fromDate, toDate },
    })
    summary.value = response.data.data
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    loading.value = false
  }
}

async function loadCurrentSubscription() {
  if (isAdmin.value) return
  try {
    const response = await api.get<ApiResponse<CurrentDoctorSubscription>>('/DoctorSubscription/my/current')
    currentSubscription.value = response.data.data
  } catch (error: any) {
    if (error.response?.status !== 404) notifications.show(getErrorMessage(error), 'error')
  }
}

async function initialize() {
  await Promise.all([loadSummary(), loadCurrentSubscription()])
}

onMounted(initialize)
</script>

<template>
  <div class="dashboard-page" :class="{ 'stats-loading': loading, 'premium-theme': isPremiumSubscription }">
    <section class="overview-hero">
      <div>
        <span class="section-kicker">{{ isAdmin ? 'Super Admin' : 'Doctor' }}</span>
        <h2>{{ isAdmin ? 'نظرة عامة على النظام' : 'نظرة عامة على عيادتك' }}</h2>
      </div>
      <div class="hero-actions">
        <button type="button" class="secondary-button" :disabled="loading" @click="loadSummary">
          <RefreshCw :size="16" :class="{ spin: loading }" />
          تحديث
        </button>
        <RouterLink class="compact-primary" to="/analytics">
          الإحصائيات
          <ArrowLeft :size="16" />
        </RouterLink>
      </div>
    </section>

    <section v-if="!isAdmin && currentSubscription" class="subscription-counter" :class="[`tone-${subscriptionTone}`, { premium: isPremiumSubscription }]">
      <span class="subscription-icon"><Crown v-if="isPremiumSubscription" :size="21" /><CalendarClock v-else :size="21" /></span>
      <div>
        <span class="section-kicker">{{ isPremiumSubscription ? 'حساب مميز' : 'الاشتراك الحالي' }}</span>
        <h3>{{ currentSubscription.packageArabicName || currentSubscription.packageName }}</h3>
        <p>متبقي {{ daysRemaining }} يوم حتى انتهاء الاشتراك</p>
      </div>
    </section>

    <section class="stats-grid">
      <article v-for="item in cards" :key="item.metric?.key" class="stat-card">
        <span class="stat-icon" :class="item.color"><component :is="item.icon" :size="20" /></span>
        <span>{{ item.metric?.label ?? '-' }}</span>
        <strong>{{ formatNumber(item.metric?.value) }}</strong>
      </article>
    </section>

    <section class="overview-grid">
      <article class="panel-card">
        <header class="panel-title">
          <Activity :size="18" />
          <h3>{{ isAdmin ? 'نمو المستخدمين' : 'حركة الحجوزات' }}</h3>
        </header>
        <div class="mini-chart">
          <span
            v-for="point in trend"
            :key="point.date"
            :title="`${point.label}: ${formatNumber(point.value)}`"
            :style="{ height: barHeight(point.value) }"
          />
        </div>
      </article>

      <article class="panel-card">
        <header class="panel-title">
          <BadgePercent :size="18" />
          <h3>العروض</h3>
        </header>
        <div class="summary-list">
          <div v-for="row in offerStats" :key="row.label">
            <span>{{ row.label }}</span>
            <strong>{{ formatNumber(row.value) }}</strong>
          </div>
        </div>
      </article>
    </section>

    <section class="overview-grid">
      <article v-for="group in quickRows" :key="group.label" class="panel-card">
        <header class="panel-title">
          <Search :size="18" />
          <h3>{{ group.label }}</h3>
        </header>
        <div v-if="group.rows.length" class="rank-list">
          <div v-for="row in group.rows.slice(0, 5)" :key="row.label">
            <span>{{ row.label }}</span>
            <strong>{{ formatNumber(row.value) }}</strong>
          </div>
        </div>
        <p v-else class="empty-text">لا توجد بيانات</p>
      </article>
    </section>

    <section v-if="!hasEvents" class="dashboard-alert">
      <Activity :size="17" />
      <span>لا توجد أحداث تطبيق مسجلة لهذه الفترة</span>
    </section>
  </div>
</template>

<style scoped>
.dashboard-page { display: flex; flex-direction: column; gap: 16px; }
.premium-theme { --primary: #a47126; --primary-dark: #7f561f; --primary-soft: #fff1db; }
.overview-hero {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 22px;
  border: 1px solid var(--line);
  border-radius: 8px;
  background: #fff;
  box-shadow: var(--shadow);
}
.overview-hero h2 { margin: 6px 0 0; color: var(--ink); font-size: 26px; }
.hero-actions { display: flex; flex-wrap: wrap; gap: 9px; }
.hero-actions a { text-decoration: none; }
.subscription-counter { display: flex; align-items: center; gap: 13px; padding: 16px; border: 1px solid var(--line); border-radius: 8px; background: #fff; box-shadow: var(--shadow); }
.subscription-counter h3 { margin: 4px 0; color: var(--ink); }
.subscription-counter p { margin: 0; color: var(--muted); font-weight: 800; }
.subscription-icon { display: grid; place-items: center; width: 46px; height: 46px; border-radius: 12px; }
.tone-green .subscription-icon { color: #167163; background: #e1f4ef; }
.tone-orange .subscription-icon { color: #a46724; background: #fff1db; }
.tone-red .subscription-icon { color: #a23d3d; background: #ffeded; }
.subscription-counter.premium { border-color: #eed29d; background: #fffaf0; }
.subscription-counter.premium .subscription-icon { color: #a47126; background: #fff1db; }
.overview-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 15px; }
.panel-title { display: flex; align-items: center; gap: 8px; margin-bottom: 15px; color: var(--primary); }
.panel-title h3 { margin: 0; font-size: 16px; }
.mini-chart { height: 170px; display: flex; align-items: end; gap: 6px; padding-top: 10px; border-bottom: 1px solid var(--line); }
.mini-chart span { flex: 1; min-width: 5px; min-height: 8px; border-radius: 8px 8px 0 0; background: linear-gradient(180deg, #13796b, #e3a536); }
.summary-list, .rank-list { display: grid; gap: 10px; }
.summary-list div, .rank-list div {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 10px 0;
  border-bottom: 1px solid var(--line);
}
.summary-list span, .rank-list span { color: var(--ink); font-weight: 800; overflow-wrap: anywhere; }
.summary-list strong, .rank-list strong { color: var(--primary); white-space: nowrap; }
.empty-text { margin: 0; color: var(--muted); }
.spin { animation: spin .8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 760px) {
  .overview-hero { align-items: stretch; flex-direction: column; }
  .overview-grid { grid-template-columns: 1fr; }
}
</style>
