<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { Activity, BarChart3, CalendarDays, Eye, RefreshCw, Search, Stethoscope, Tags, UsersRound, Zap } from '@lucide/vue'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'
import { useNotificationsStore } from '../stores/notifications'
import type { AnalyticsLabelValue, AnalyticsMetric, AnalyticsSummary, ApiResponse, DoctorItem, PageResult } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const auth = useAuthStore()
const notifications = useNotificationsStore()
const isAdmin = computed(() => auth.hasAnyRole(['SuperAdmin']))
const loading = ref(false)
const summary = ref<AnalyticsSummary | null>(null)
const doctors = ref<DoctorItem[]>([])
const selectedDoctorId = ref('')
const today = new Date()
const fromDate = ref(toInputDate(new Date(today.getFullYear(), today.getMonth(), today.getDate() - 30)))
const toDate = ref(toInputDate(today))
const doctorMode = computed(() => !isAdmin.value || selectedDoctorId.value !== '')

const metricKeys = computed(() => isAdmin.value && !selectedDoctorId.value
  ? ['usersInRange', 'appointmentsInRange', 'searches', 'profileViews', 'bookingClicks', 'createdBookingsFromEvents']
  : ['searchAppearances', 'profileViews', 'bookingClicks', 'createdBookingsFromEvents', 'completedBookings', 'averageRating'])

const mainMetrics = computed(() => metricKeys.value
  .map((key) => findMetric(key))
  .filter(Boolean) as AnalyticsMetric[])

const funnelRows = computed<AnalyticsLabelValue[]>(() => {
  const searches = findMetric('searches')?.value ?? findMetric('searchAppearances')?.value ?? 0
  return [
    { label: 'ظهور/بحث', value: searches },
    { label: 'فتح بروفايل', value: findMetric('profileViews')?.value ?? 0 },
    { label: 'ضغط حجز', value: findMetric('bookingClicks')?.value ?? 0 },
    { label: 'حجز من حدث', value: findMetric('createdBookingsFromEvents')?.value ?? 0 },
  ]
})

const offerRows = computed<AnalyticsLabelValue[]>(() => summary.value ? [
  { label: 'مشاهدات العروض', value: summary.value.offers.views },
  { label: 'ضغطات العروض', value: summary.value.offers.clicks },
  { label: 'حجوزات من العروض', value: summary.value.offers.bookingsFromOffers },
] : [])

const subscriptionRows = computed<AnalyticsLabelValue[]>(() => summary.value ? [
  { label: 'مشتركين نشطين', value: summary.value.subscriptions.activeSubscribers },
  { label: 'بريميوم', value: summary.value.subscriptions.premiumSubscribers },
  { label: 'عادي', value: summary.value.subscriptions.basicSubscribers },
  { label: 'منتهية', value: summary.value.subscriptions.expiredSubscriptions },
  { label: 'قريبة الانتهاء', value: summary.value.subscriptions.expiringSoon },
] : [])

const leftLists = computed(() => isAdmin.value && !selectedDoctorId.value ? [
  { title: 'الحجوزات حسب الحالة', icon: CalendarDays, rows: summary.value?.appointmentStatus ?? [] },
  { title: 'مصادر الحجوزات', icon: UsersRound, rows: summary.value?.appointmentSources ?? [] },
  { title: 'أكثر الأطباء مشاهدة', icon: Eye, rows: summary.value?.topDoctorsByViews ?? [] },
  { title: 'أكثر الأطباء حجزاً', icon: Stethoscope, rows: summary.value?.topDoctorsByBookings ?? [] },
] : [
  { title: 'مصادر حجوزات الطبيب', icon: UsersRound, rows: summary.value?.appointmentSources ?? [] },
  { title: 'أكثر عيادة عليها حجوزات', icon: Stethoscope, rows: summary.value?.topClinicsByBookings ?? [] },
  { title: 'أكثر أيام الحجز', icon: CalendarDays, rows: summary.value?.topBookingDays ?? [] },
  { title: 'أوقات الذروة', icon: Activity, rows: summary.value?.peakBookingHours ?? [] },
])

const rightLists = computed(() => isAdmin.value && !selectedDoctorId.value ? [
  { title: 'أكثر الاختصاصات بحثاً', icon: Search, rows: summary.value?.topSpecializationsBySearch ?? [] },
  { title: 'أكثر الاختصاصات حجزاً', icon: BarChart3, rows: summary.value?.topSpecializationsByBookings ?? [] },
  { title: 'أكثر المحافظات نشاطاً', icon: Activity, rows: summary.value?.topProvinces ?? [] },
  { title: 'كلمات البحث', icon: Search, rows: summary.value?.topSearchTerms ?? [] },
  { title: 'الصفحات المفتوحة', icon: Eye, rows: summary.value?.topPages ?? [] },
] : [
  { title: 'حالة الحجوزات', icon: CalendarDays, rows: summary.value?.appointmentStatus ?? [] },
  { title: 'أكثر محافظة/منطقة', icon: Activity, rows: summary.value?.topProvinces ?? [] },
  { title: 'أداء عروض الطبيب', icon: Tags, rows: offerRows.value },
])

const insightRows = computed(() => {
  const profileRate = summary.value?.conversions.searchToProfileRate ?? 0
  const bookingRate = summary.value?.conversions.profileToBookingRate ?? 0
  const topProvince = summary.value?.topProvinces[0]?.label
  const topSearch = summary.value?.topSearchTerms[0]?.label
  const rows = []
  if (!hasEventActivity.value) rows.push('لا توجد أحداث موبايل واصلة لهذه الفترة. إذا فتحت التطبيق وما ظهر حدث هنا، فالموبايل متصل بسيرفر لا يحتوي نسخة الإحصائيات.')
  if (profileRate < 20) rows.push('فتح البروفايل من البحث منخفض. ركز على صور الأطباء المميزين، ترتيب النتائج، ووضوح الاختصاص.')
  if (bookingRate < 10) rows.push('التحويل من البروفايل إلى الحجز منخفض. راقب وضوح الدوام والتقييمات وزر الحجز داخل البروفايل.')
  if (topProvince) rows.push(`أكثر نشاط من ${topProvince}. هذه منطقة مناسبة لحملة إعلانية محلية أو عروض أطباء.`)
  if (topSearch) rows.push(`كلمة البحث الأقوى: ${topSearch}. استخدمها بالإعلانات ومحتوى السوشيال.`)
  return rows
})

const hasEventActivity = computed(() => {
  return (summary.value?.recentEvents?.length ?? 0) > 0
    || (findMetric('profileViews')?.value ?? 0) > 0
    || (findMetric('searches')?.value ?? findMetric('searchAppearances')?.value ?? 0) > 0
})

function toInputDate(date: Date) {
  return date.toLocaleDateString('en-CA')
}

function findMetric(key: string) {
  return summary.value?.metrics.find((metric) => metric.key === key)
}

function formatNumber(value: number) {
  return new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 }).format(value)
}

function formatTime(value: string) {
  return new Intl.DateTimeFormat('en-US', { month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit' }).format(new Date(value))
}

function barWidth(value: number, rows: Array<{ value: number }>) {
  const max = Math.max(...rows.map((row) => Number(row.value)), 1)
  return `${Math.max(5, (Number(value) / max) * 100)}%`
}

async function loadAnalytics() {
  loading.value = true
  try {
    const endpoint = isAdmin.value
      ? selectedDoctorId.value
        ? `/Analytics/admin/doctors/${selectedDoctorId.value}/summary`
        : '/Analytics/admin/summary'
      : '/Analytics/doctor/summary'
    const response = await api.get<ApiResponse<AnalyticsSummary>>(endpoint, {
      params: { fromDate: fromDate.value, toDate: toDate.value },
    })
    summary.value = response.data.data
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    loading.value = false
  }
}

async function loadDoctors() {
  if (!isAdmin.value) return
  const response = await api.get<ApiResponse<PageResult<DoctorItem>>>('/Doctor', {
    params: { page: 1, pageSize: 300 },
  })
  doctors.value = response.data.data.items
}

onMounted(async () => {
  await Promise.all([loadDoctors(), loadAnalytics()])
})
</script>

<template>
  <div class="analytics-view">
    <section class="analytics-toolbar">
      <div>
        <span class="section-kicker">{{ doctorMode ? 'تحليل طبيب' : 'تحليل النظام' }}</span>
        <h2>{{ doctorMode ? 'مسار وصول المريض إلى الحجز' : 'لوحة تسويق وتشغيل التطبيق' }}</h2>
        <p>{{ hasEventActivity ? 'الأحداث واصلة لهذه الفترة.' : 'لا توجد أحداث موبايل واصلة لهذه الفترة.' }}</p>
      </div>
      <form class="toolbar-filters" @submit.prevent="loadAnalytics">
        <label v-if="isAdmin">
          الطبيب
          <select v-model="selectedDoctorId" @change="loadAnalytics">
            <option value="">كل النظام</option>
            <option v-for="doctor in doctors" :key="doctor.id" :value="doctor.id">{{ doctor.name }}</option>
          </select>
        </label>
        <label>من<input v-model="fromDate" type="date"></label>
        <label>إلى<input v-model="toDate" type="date"></label>
        <button type="submit" :disabled="loading"><RefreshCw :size="16" :class="{ spin: loading }" /> تحديث</button>
      </form>
    </section>

    <section v-if="!hasEventActivity" class="live-warning">
      <Zap :size="18" />
      <span>افتح التطبيق بعد نشر الباك الجديد على نفس سيرفر الموبايل، ثم راقب “آخر الأحداث” هنا.</span>
    </section>

    <section class="metric-grid" :class="{ muted: loading }">
      <article v-for="metric in mainMetrics" :key="metric.key" class="metric-card">
        <span>{{ metric.label }}</span>
        <strong>{{ formatNumber(metric.value) }}</strong>
      </article>
    </section>

    <section class="top-grid">
      <article class="panel-card">
        <header><BarChart3 :size="18" /><h3>فنل التحويل</h3></header>
        <div class="funnel-list">
          <div v-for="row in funnelRows" :key="row.label">
            <span>{{ row.label }}</span>
            <strong>{{ formatNumber(row.value) }}</strong>
            <i :style="{ width: barWidth(row.value, funnelRows) }"></i>
          </div>
        </div>
        <footer>
          <span>بحث إلى بروفايل: {{ formatNumber(summary?.conversions.searchToProfileRate ?? 0) }}%</span>
          <span>بروفايل إلى حجز: {{ formatNumber(summary?.conversions.profileToBookingRate ?? 0) }}%</span>
        </footer>
      </article>

      <article class="panel-card">
        <header><Activity :size="18" /><h3>{{ doctorMode ? 'ترند الحجوزات' : 'ترند النمو' }}</h3></header>
        <div class="trend-chart">
          <span
            v-for="point in doctorMode ? summary?.appointmentTrend ?? [] : summary?.userGrowth ?? []"
            :key="`${point.date}-${point.label}`"
            :title="`${point.label}: ${formatNumber(point.value)}`"
            :style="{ height: barWidth(point.value, doctorMode ? summary?.appointmentTrend ?? [] : summary?.userGrowth ?? []) }"
          ></span>
        </div>
      </article>

      <article class="panel-card insight-card">
        <header><Zap :size="18" /><h3>قراءة تسويقية</h3></header>
        <ul>
          <li v-for="item in insightRows" :key="item">{{ item }}</li>
        </ul>
      </article>
    </section>

    
    <section class="details-grid">
      <div class="panel-stack">
        <article v-for="group in leftLists" :key="group.title" class="panel-card">
          <header><component :is="group.icon" :size="18" /><h3>{{ group.title }}</h3></header>
          <div v-if="group.rows.length" class="bar-list">
            <div v-for="row in group.rows" :key="row.label" class="bar-row">
              <div><span>{{ row.label }}</span><strong>{{ formatNumber(row.value) }}</strong></div>
              <i :style="{ width: barWidth(row.value, group.rows) }"></i>
            </div>
          </div>
          <p v-else class="empty-text">لا توجد بيانات لهذه الفترة.</p>
        </article>
      </div>

      <div class="panel-stack">
        <article v-for="group in rightLists" :key="group.title" class="panel-card">
          <header><component :is="group.icon" :size="18" /><h3>{{ group.title }}</h3></header>
          <div v-if="group.rows.length" class="bar-list">
            <div v-for="row in group.rows" :key="row.label" class="bar-row">
              <div><span>{{ row.label }}</span><strong>{{ formatNumber(row.value) }}</strong></div>
              <i :style="{ width: barWidth(row.value, group.rows) }"></i>
            </div>
          </div>
          <p v-else class="empty-text">لا توجد بيانات لهذه الفترة.</p>
        </article>
      </div>
    </section>
    <section class="activity-grid">
      <!-- <article class="panel-card">
        <header><Activity :size="18" /><h3>آخر الأحداث</h3></header>
        <div v-if="summary?.recentEvents?.length" class="event-list">
          <div v-for="event in summary.recentEvents" :key="`${event.eventType}-${event.occurredAt}-${event.doctorId ?? ''}`">
            <strong>{{ event.label }}</strong>
            <span>{{ formatTime(event.occurredAt) }}</span>
            <small>{{ event.source || event.page || 'mobile' }}</small>
          </div>
        </div>
        <p v-else class="empty-text">لا يوجد أي حدث واصل ضمن التاريخ المحدد.</p>
      </article> -->

      <article class="panel-card">
        <header><Tags :size="18" /><h3>{{ isAdmin && !selectedDoctorId ? 'العروض والاشتراكات' : 'أداء العروض' }}</h3></header>
        <div class="mini-columns">
          <div>
            <div v-for="row in offerRows" :key="row.label" class="mini-row"><span>{{ row.label }}</span><strong>{{ formatNumber(row.value) }}</strong></div>
          </div>
          <div v-if="isAdmin && !selectedDoctorId">
            <div v-for="row in subscriptionRows" :key="row.label" class="mini-row"><span>{{ row.label }}</span><strong>{{ formatNumber(row.value) }}</strong></div>
          </div>
        </div>
      </article>
    </section>

  </div>
</template>

<style scoped>
.analytics-view { display: flex; flex-direction: column; gap: 16px; }
.analytics-toolbar, .panel-card, .metric-card, .live-warning { border: 1px solid var(--line); border-radius: 8px; background: var(--surface); box-shadow: var(--shadow); }
.analytics-toolbar { display: flex; align-items: end; justify-content: space-between; gap: 18px; padding: 18px; }
.analytics-toolbar h2 { margin: 5px 0 5px; color: var(--ink); font-size: 24px; line-height: 1.35; }
.analytics-toolbar p { margin: 0; color: var(--muted); font-size: 13px; }
.toolbar-filters { display: flex; align-items: end; gap: 10px; flex-wrap: wrap; }
.toolbar-filters label { display: grid; gap: 6px; color: var(--muted); font-size: 12px; font-weight: 900; }
.toolbar-filters input, .toolbar-filters select { height: 40px; min-width: 142px; border: 1px solid var(--line); border-radius: 8px; padding: 0 10px; background: #fff; color: var(--ink); }
.toolbar-filters select { min-width: 230px; }
.toolbar-filters button { height: 40px; display: inline-flex; align-items: center; gap: 7px; border: 0; border-radius: 8px; padding: 0 15px; background: var(--primary); color: #fff; font-weight: 900; cursor: pointer; }
.live-warning { display: flex; align-items: center; gap: 9px; padding: 12px 14px; color: #9a6400; background: #fff8e8; border-color: #f0c66a; font-weight: 800; }
.metric-grid { display: grid; grid-template-columns: repeat(6, 1fr); gap: 10px; }
.metric-card { min-height: 104px; padding: 14px; }
.metric-card span { color: var(--muted); font-size: 12px; font-weight: 900; }
.metric-card strong { display: block; margin-top: 12px; color: var(--ink); font-size: 28px; line-height: 1; }
.top-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 12px; align-items: stretch; }
.activity-grid, .details-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; align-items: start; }
.panel-card { padding: 15px; }
.panel-card header { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; color: var(--primary); }
.panel-card h3 { margin: 0; color: var(--ink); font-size: 16px; }
.funnel-list, .bar-list, .event-list, .panel-stack { display: grid; gap: 11px; }
.funnel-list div, .bar-row { display: grid; gap: 7px; }
.funnel-list div { grid-template-columns: 1fr auto; }
.funnel-list span, .bar-row span, .mini-row span { color: var(--ink); font-size: 13px; font-weight: 800; overflow-wrap: anywhere; }
.funnel-list strong, .bar-row strong, .mini-row strong { color: var(--primary); white-space: nowrap; }
.funnel-list i, .bar-row i { grid-column: 1 / -1; display: block; height: 7px; border-radius: 999px; background: linear-gradient(90deg, #0f8f83, #f2b84b); }
.panel-card footer { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 13px; color: var(--muted); font-size: 12px; font-weight: 800; }
.trend-chart { height: 162px; display: flex; align-items: end; gap: 5px; padding: 10px 2px 0; border-bottom: 1px solid var(--line); }
.trend-chart span { flex: 1; min-width: 4px; min-height: 5px; border-radius: 999px 999px 0 0; background: linear-gradient(180deg, #0f8f83, #f2b84b); }
.insight-card ul { margin: 0; padding: 0 18px 0 0; display: grid; gap: 9px; color: var(--ink); font-size: 13px; line-height: 1.8; }
.event-list div { display: grid; grid-template-columns: 1fr auto; gap: 3px 10px; padding-bottom: 9px; border-bottom: 1px solid var(--line); }
.event-list strong { color: var(--ink); font-size: 13px; }
.event-list span, .event-list small { color: var(--muted); font-size: 12px; font-weight: 800; }
.mini-columns { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.mini-row { display: flex; align-items: center; justify-content: space-between; gap: 10px; padding: 9px 0; border-bottom: 1px solid var(--line); }
.bar-row div { display: flex; align-items: center; justify-content: space-between; gap: 12px; }
.empty-text { margin: 0; color: var(--muted); font-size: 13px; }
.muted { opacity: .62; }
.spin { animation: spin .8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 1180px) {
  .metric-grid { grid-template-columns: repeat(3, 1fr); }
  .top-grid { grid-template-columns: 1fr; }
}
@media (max-width: 860px) {
  .analytics-toolbar { align-items: stretch; flex-direction: column; }
  .activity-grid, .details-grid { grid-template-columns: 1fr; }
}
@media (max-width: 560px) {
  .metric-grid, .mini-columns { grid-template-columns: 1fr; }
  .toolbar-filters { display: grid; grid-template-columns: 1fr; }
  .toolbar-filters input, .toolbar-filters select, .toolbar-filters button { width: 100%; min-width: 0; }
}
</style>
