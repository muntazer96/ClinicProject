<script setup lang="ts">
import { computed, defineComponent, h, onMounted, ref } from 'vue'
import type { PropType } from 'vue'
import {
  Activity, BadgePercent, BarChart3, CalendarDays, Eye, MousePointerClick,
  RefreshCw, Search, Stethoscope, TrendingUp, UsersRound,
} from '@lucide/vue'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'
import { useNotificationsStore } from '../stores/notifications'
import type { AnalyticsLabelValue, AnalyticsMetric, AnalyticsSummary, ApiResponse, DoctorItem, PageResult } from '../types/api'
import { getErrorMessage } from '../utils/errors'

type TabKey = 'overview' | 'offers' | 'doctors' | 'bookings' | 'search' | 'activity'

const auth = useAuthStore()
const notifications = useNotificationsStore()
const isAdmin = computed(() => auth.hasAnyRole(['SuperAdmin']))
const loading = ref(false)
const summary = ref<AnalyticsSummary | null>(null)
const doctors = ref<DoctorItem[]>([])
const selectedDoctorId = ref('')
const activeTab = ref<TabKey>('overview')
const today = new Date()
const fromDate = ref(toInputDate(new Date(today.getFullYear(), today.getMonth(), today.getDate() - 30)))
const toDate = ref(toInputDate(today))

const tabs = computed(() => [
  { key: 'overview' as TabKey, label: 'الملخص', icon: BarChart3 },
  { key: 'offers' as TabKey, label: 'الإعلانات والعروض', icon: BadgePercent },
  ...(isAdmin.value && !selectedDoctorId.value ? [{ key: 'doctors' as TabKey, label: 'الأطباء', icon: Stethoscope }] : []),
  { key: 'bookings' as TabKey, label: 'الحجوزات', icon: CalendarDays },
  { key: 'search' as TabKey, label: 'البحث والوصول', icon: Search },
  { key: 'activity' as TabKey, label: 'النشاط', icon: Activity },
])

const BarList = defineComponent({
  props: {
    rows: { type: Array as PropType<AnalyticsLabelValue[]>, required: true },
    formatNumber: { type: Function as PropType<(value: number) => string>, required: true },
  },
  setup(props) {
    return () => {
      const max = Math.max(...props.rows.map((row) => Number(row.value)), 1)
      if (!props.rows.length) return h('p', { class: 'empty-state' }, 'لا توجد بيانات')
      return h('div', { class: 'bar-list' }, props.rows.map((row) => h('div', { class: 'bar-row', key: row.label }, [
        h('div', [
          h('span', row.label),
          h('strong', props.formatNumber(Number(row.value))),
        ]),
        h('i', { style: { width: `${Math.max(5, (Number(row.value) / max) * 100)}%` } }),
      ])))
    }
  },
})

const doctorMode = computed(() => !isAdmin.value || selectedDoctorId.value !== '')
const hasEvents = computed(() => (summary.value?.recentEvents.length ?? 0) > 0)

const kpiKeys = computed(() => isAdmin.value && !selectedDoctorId.value
  ? ['appointmentsInRange', 'searches', 'profileViews', 'bookingClicks', 'createdBookingsFromEvents', 'usersInRange']
  : ['searchAppearances', 'profileViews', 'bookingClicks', 'createdBookingsFromEvents', 'todayBookings', 'averageRating'])

const kpis = computed(() => kpiKeys.value
  .map((key) => findMetric(key))
  .filter(Boolean) as AnalyticsMetric[])

const funnelRows = computed<AnalyticsLabelValue[]>(() => {
  const searches = findMetric('searches')?.value ?? findMetric('searchAppearances')?.value ?? 0
  return [
    { label: 'بحث/ظهور', value: searches },
    { label: 'فتح بروفايل', value: findMetric('profileViews')?.value ?? 0 },
    { label: 'ضغط حجز', value: findMetric('bookingClicks')?.value ?? 0 },
    { label: 'حجز مسجل', value: findMetric('createdBookingsFromEvents')?.value ?? 0 },
  ]
})

const offerRows = computed<AnalyticsLabelValue[]>(() => [
  { label: 'وصول الإعلانات', value: summary.value?.offers.views ?? 0 },
  { label: 'مشاهدات العروض', value: summary.value?.offers.views ?? 0 },
  { label: 'ضغطات العروض', value: summary.value?.offers.clicks ?? 0 },
  { label: 'حجوزات من العروض', value: summary.value?.offers.bookingsFromOffers ?? 0 },
])

const subscriptionRows = computed<AnalyticsLabelValue[]>(() => [
  { label: 'مشتركين', value: summary.value?.subscriptions.activeSubscribers ?? 0 },
  { label: 'Premium', value: summary.value?.subscriptions.premiumSubscribers ?? 0 },
  { label: 'Basic', value: summary.value?.subscriptions.basicSubscribers ?? 0 },
  { label: 'منتهية', value: summary.value?.subscriptions.expiredSubscriptions ?? 0 },
  { label: 'قريبة الانتهاء', value: summary.value?.subscriptions.expiringSoon ?? 0 },
])

const trend = computed(() => doctorMode.value ? summary.value?.appointmentTrend ?? [] : summary.value?.userGrowth ?? [])

const insightRows = computed(() => {
  const rows = []
  const topProvince = summary.value?.topProvinces[0]?.label
  const topSearch = summary.value?.topSearchTerms[0]?.label
  const topDoctor = summary.value?.topDoctorsByBookings[0]?.label
  const profileRate = summary.value?.conversions.searchToProfileRate ?? 0
  const bookingRate = summary.value?.conversions.profileToBookingRate ?? 0
  rows.push({ label: 'تحويل البحث إلى بروفايل', value: `${formatNumber(profileRate)}%` })
  rows.push({ label: 'تحويل البروفايل إلى حجز', value: `${formatNumber(bookingRate)}%` })
  if (topProvince) rows.push({ label: 'أقوى محافظة', value: topProvince })
  if (topSearch) rows.push({ label: 'أقوى كلمة بحث', value: topSearch })
  if (topDoctor && isAdmin.value && !selectedDoctorId.value) rows.push({ label: 'أقوى طبيب بالحجز', value: topDoctor })
  return rows
})

function toInputDate(date: Date) {
  return date.toLocaleDateString('en-CA')
}

function findMetric(key: string) {
  return summary.value?.metrics.find((metric) => metric.key === key)
}

function formatNumber(value = 0) {
  return new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 }).format(value)
}

function formatTime(value: string) {
  return new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(value))
}

function barWidth(value: number, rows: Array<{ value: number }>) {
  const max = Math.max(...rows.map((row) => Number(row.value)), 1)
  return `${Math.max(5, (Number(value) / max) * 100)}%`
}

function trendHeight(value: number) {
  const max = Math.max(...trend.value.map((row) => Number(row.value)), 1)
  return `${Math.max(7, (Number(value) / max) * 100)}%`
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
  <div class="analytics-page">
    <section class="analytics-head">
      <div>
        <span class="section-kicker">{{ doctorMode ? 'Doctor Analytics' : 'System Analytics' }}</span>
        <h2>{{ doctorMode ? 'تحليل أداء الطبيب' : 'تحليل نمو التطبيق' }}</h2>
      </div>
      <form class="analytics-filters" @submit.prevent="loadAnalytics">
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

    <nav class="analytics-tabs" aria-label="تبويب الإحصائيات">
      <button
        v-for="tab in tabs"
        :key="tab.key"
        type="button"
        :class="{ active: activeTab === tab.key }"
        @click="activeTab = tab.key"
      >
        <component :is="tab.icon" :size="17" />
        {{ tab.label }}
      </button>
    </nav>

    <section v-if="activeTab === 'overview'" class="tab-panel">
      <div class="analytics-kpis" :class="{ muted: loading }">
        <article v-for="item in kpis" :key="item.key">
          <span>{{ item.label }}</span>
          <strong>{{ formatNumber(item.value) }}</strong>
        </article>
      </div>

      <div class="analytics-grid">
        <article class="analytics-card large-card">
          <header><TrendingUp :size="18" /><h3>{{ doctorMode ? 'ترند الحجوزات' : 'ترند النمو' }}</h3></header>
          <div class="trend-chart">
            <span
              v-for="point in trend"
              :key="point.date"
              :title="`${point.label}: ${formatNumber(point.value)}`"
              :style="{ height: trendHeight(point.value) }"
            />
          </div>
        </article>

        <article class="analytics-card">
          <header><BarChart3 :size="18" /><h3>مسار التحويل</h3></header>
          <div class="funnel-list">
            <div v-for="row in funnelRows" :key="row.label">
              <div><span>{{ row.label }}</span><strong>{{ formatNumber(row.value) }}</strong></div>
              <i :style="{ width: barWidth(row.value, funnelRows) }"></i>
            </div>
          </div>
        </article>

        <article class="analytics-card">
          <header><Activity :size="18" /><h3>قراءة سريعة</h3></header>
          <div class="insight-list">
            <div v-for="row in insightRows" :key="row.label">
              <span>{{ row.label }}</span>
              <strong>{{ row.value }}</strong>
            </div>
          </div>
        </article>
      </div>
    </section>

    <section v-else-if="activeTab === 'offers'" class="tab-panel two-columns">
      <article class="analytics-card">
        <header><BadgePercent :size="18" /><h3>أداء الإعلانات والعروض</h3></header>
        <BarList :rows="offerRows" :format-number="formatNumber" />
      </article>
      <article v-if="isAdmin && !selectedDoctorId" class="analytics-card">
        <header><UsersRound :size="18" /><h3>الاشتراكات</h3></header>
        <BarList :rows="subscriptionRows" :format-number="formatNumber" />
      </article>
    </section>

    <section v-else-if="activeTab === 'doctors'" class="tab-panel two-columns">
      <article class="analytics-card">
        <header><Eye :size="18" /><h3>الأطباء الأكثر زيارة</h3></header>
        <BarList :rows="summary?.topDoctorsByViews ?? []" :format-number="formatNumber" />
      </article>
      <article class="analytics-card">
        <header><CalendarDays :size="18" /><h3>الأطباء الأكثر حجزا</h3></header>
        <BarList :rows="summary?.topDoctorsByBookings ?? []" :format-number="formatNumber" />
      </article>
    </section>

    <section v-else-if="activeTab === 'bookings'" class="tab-panel two-columns">
      <article class="analytics-card">
        <header><CalendarDays :size="18" /><h3>الحجوزات حسب الحالة</h3></header>
        <BarList :rows="summary?.appointmentStatus ?? []" :format-number="formatNumber" />
      </article>
      <article class="analytics-card">
        <header><UsersRound :size="18" /><h3>الحجوزات حسب المصدر</h3></header>
        <BarList :rows="summary?.appointmentSources ?? []" :format-number="formatNumber" />
      </article>
      <article class="analytics-card">
        <header><Stethoscope :size="18" /><h3>{{ doctorMode ? 'العيادات الأكثر حجزا' : 'الاختصاصات الأكثر حجزا' }}</h3></header>
        <BarList :rows="doctorMode ? summary?.topClinicsByBookings ?? [] : summary?.topSpecializationsByBookings ?? []" :format-number="formatNumber" />
      </article>
      <article class="analytics-card">
        <header><Activity :size="18" /><h3>أوقات الذروة</h3></header>
        <BarList :rows="summary?.peakBookingHours ?? []" :format-number="formatNumber" />
      </article>
    </section>

    <section v-else-if="activeTab === 'search'" class="tab-panel two-columns">
      <article class="analytics-card">
        <header><Search :size="18" /><h3>كلمات البحث</h3></header>
        <BarList :rows="summary?.topSearchTerms ?? []" :format-number="formatNumber" />
      </article>
      <article class="analytics-card">
        <header><Stethoscope :size="18" /><h3>الاختصاصات الأكثر بحثا</h3></header>
        <BarList :rows="summary?.topSpecializationsBySearch ?? []" :format-number="formatNumber" />
      </article>
      <article class="analytics-card">
        <header><Activity :size="18" /><h3>المحافظات النشطة</h3></header>
        <BarList :rows="summary?.topProvinces ?? []" :format-number="formatNumber" />
      </article>
      <article class="analytics-card">
        <header><Eye :size="18" /><h3>الصفحات الأكثر فتحا</h3></header>
        <BarList :rows="summary?.topPages ?? []" :format-number="formatNumber" />
      </article>
    </section>

    <section v-else class="tab-panel two-columns">
      <article class="analytics-card">
        <header><Activity :size="18" /><h3>آخر الأحداث</h3></header>
        <div v-if="summary?.recentEvents.length" class="event-list">
          <div v-for="event in summary.recentEvents" :key="`${event.eventType}-${event.occurredAt}-${event.doctorId ?? ''}`">
            <strong>{{ event.label }}</strong>
            <span>{{ formatTime(event.occurredAt) }}</span>
            <small>{{ event.source || event.page || 'mobile' }}</small>
          </div>
        </div>
        <p v-else class="empty-state">لا توجد أحداث</p>
      </article>
      <article class="analytics-card">
        <header><MousePointerClick :size="18" /><h3>حالة الربط</h3></header>
        <div class="connection-state" :class="{ live: hasEvents }">
          <strong>{{ hasEvents ? 'Live' : 'No Events' }}</strong>
          <span>{{ hasEvents ? 'الأحداث تصل للنظام' : 'لا توجد أحداث ضمن الفترة المحددة' }}</span>
        </div>
      </article>
    </section>
  </div>
</template>

<style scoped>
.analytics-page { display: flex; flex-direction: column; gap: 14px; }
.analytics-head, .analytics-card, .analytics-kpis article {
  border: 1px solid var(--line);
  border-radius: 8px;
  background: #fff;
  box-shadow: var(--shadow);
}
.analytics-head { display: flex; align-items: end; justify-content: space-between; gap: 16px; padding: 18px; }
.analytics-head h2 { margin: 5px 0 0; color: var(--ink); font-size: 25px; }
.analytics-filters { display: flex; align-items: end; gap: 9px; flex-wrap: wrap; }
.analytics-filters label { display: grid; gap: 6px; color: var(--muted); font-size: 12px; font-weight: 900; }
.analytics-filters input, .analytics-filters select {
  height: 40px;
  min-width: 142px;
  padding: 0 10px;
  border: 1px solid var(--line);
  border-radius: 8px;
  background: #fff;
  color: var(--ink);
}
.analytics-filters select { min-width: 230px; }
.analytics-filters button {
  height: 40px;
  display: inline-flex;
  align-items: center;
  gap: 7px;
  padding: 0 15px;
  border: 0;
  border-radius: 8px;
  background: var(--primary);
  color: #fff;
  font-weight: 900;
}
.analytics-tabs {
  display: flex;
  gap: 7px;
  padding: 6px;
  border: 1px solid var(--line);
  border-radius: 8px;
  background: #fff;
  box-shadow: var(--shadow);
  overflow-x: auto;
}
.analytics-tabs button {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  padding: 10px 13px;
  border: 0;
  border-radius: 7px;
  background: transparent;
  color: var(--muted);
  font-weight: 900;
  white-space: nowrap;
}
.analytics-tabs button.active { color: var(--primary); background: var(--primary-soft); }
.tab-panel { display: grid; gap: 14px; }
.two-columns, .analytics-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); align-items: start; }
.analytics-grid { display: grid; gap: 14px; }
.large-card { grid-column: 1 / -1; }
.analytics-kpis { display: grid; grid-template-columns: repeat(6, minmax(0, 1fr)); gap: 10px; }
.analytics-kpis article { min-height: 100px; padding: 14px; }
.analytics-kpis span { color: var(--muted); font-size: 12px; font-weight: 900; }
.analytics-kpis strong { display: block; margin-top: 12px; color: var(--ink); font-size: 28px; line-height: 1; }
.analytics-card { padding: 15px; }
.analytics-card header { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; color: var(--primary); }
.analytics-card h3 { margin: 0; color: var(--ink); font-size: 16px; }
.trend-chart { height: 190px; display: flex; align-items: end; gap: 5px; padding-top: 10px; border-bottom: 1px solid var(--line); }
.trend-chart span { flex: 1; min-width: 4px; min-height: 7px; border-radius: 999px 999px 0 0; background: linear-gradient(180deg, #13796b, #e3a536); }
.funnel-list, .bar-list, .event-list, .insight-list { display: grid; gap: 10px; }
.funnel-list > div, .bar-row { display: grid; gap: 7px; }
.funnel-list div div, .bar-row div, .insight-list div {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}
.funnel-list span, .bar-row span, .insight-list span { color: var(--ink); font-size: 13px; font-weight: 900; overflow-wrap: anywhere; }
.funnel-list strong, .bar-row strong, .insight-list strong { color: var(--primary); white-space: nowrap; }
.funnel-list i, .bar-row i { display: block; height: 7px; border-radius: 999px; background: linear-gradient(90deg, #13796b, #e3a536); }
.event-list div { display: grid; grid-template-columns: 1fr auto; gap: 3px 10px; padding-bottom: 9px; border-bottom: 1px solid var(--line); }
.event-list strong { color: var(--ink); font-size: 13px; }
.event-list span, .event-list small { color: var(--muted); font-size: 12px; font-weight: 800; }
.connection-state { display: grid; gap: 6px; padding: 18px; border-radius: 8px; background: #fff8e8; color: #9a6400; }
.connection-state.live { background: #e4f4f0; color: var(--primary); }
.connection-state strong { font-size: 24px; }
.empty-state { margin: 0; color: var(--muted); }
.muted { opacity: .62; }
.spin { animation: spin .8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 1180px) {
  .analytics-kpis { grid-template-columns: repeat(3, minmax(0, 1fr)); }
}
@media (max-width: 860px) {
  .analytics-head { align-items: stretch; flex-direction: column; }
  .two-columns, .analytics-grid { grid-template-columns: 1fr; }
}
@media (max-width: 560px) {
  .analytics-kpis { grid-template-columns: 1fr; }
  .analytics-filters { display: grid; grid-template-columns: 1fr; }
  .analytics-filters input, .analytics-filters select, .analytics-filters button { width: 100%; min-width: 0; }
}
</style>
