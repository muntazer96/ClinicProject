<script setup lang="ts">
import { computed, defineComponent, h, onMounted, ref } from 'vue'
import type { PropType } from 'vue'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'
import { useNotifications } from '../composables/useNotifications'
import type { AnalyticsLabelValue, AnalyticsMetric, AnalyticsSummary, ApiResponse, DoctorItem, PageResult } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import PageHeader from '../components/common/Pageheader.vue'
import EmptyState from '../components/common/Emptystate.vue'

type TabKey = 'overview' | 'offers' | 'doctors' | 'bookings' | 'search' | 'activity'

const auth = useAuthStore()
const { error: showError } = useNotifications()

const isAdmin = computed(() => auth.hasAnyRole(['SuperAdmin']) && !auth.hasAnyRole(['DoctorUser']))
const loading = ref(false)
const summary = ref<AnalyticsSummary | null>(null)
const doctors = ref<DoctorItem[]>([])
const selectedDoctorId = ref('')
const activeTab = ref<TabKey>('overview')
const doctorOptions = computed(() => [
  { value: '', label: 'كل النظام' },
  ...doctors.value.map((doctor) => ({ value: doctor.id, label: doctor.name })),
])

const today = new Date()
const fromDate = ref(toInputDate(new Date(today.getFullYear(), today.getMonth(), today.getDate() - 30)))
const toDate = ref(toInputDate(today))

const tabs = computed(() => [
  { key: 'overview'  as TabKey, label: 'الملخص',              icon: 'mdi-chart-bar'         },
  { key: 'offers'    as TabKey, label: 'الإعلانات والعروض',   icon: 'mdi-percent'            },
  ...( isAdmin.value && !selectedDoctorId.value
    ? [{ key: 'doctors' as TabKey, label: 'الأطباء',          icon: 'mdi-stethoscope'        }]
    : []),
  { key: 'bookings'  as TabKey, label: 'الحجوزات',            icon: 'mdi-calendar'           },
  { key: 'search'    as TabKey, label: 'البحث والوصول',       icon: 'mdi-magnify'            },
  { key: 'activity'  as TabKey, label: 'النشاط',              icon: 'mdi-lightning-bolt'     },
])

// BarList render component
const BarList = defineComponent({
  props: {
    rows: { type: Array as PropType<AnalyticsLabelValue[]>, required: true },
    formatFn: { type: Function as PropType<(v: number) => string>, required: true },
  },
  setup(props) {
    return () => {
      if (!props.rows.length) {
        return h('p', { class: 'empty-text' }, 'لا توجد بيانات')
      }
      const max = Math.max(...props.rows.map((r) => Number(r.value)), 1)
      return h('div', { class: 'bar-list' },
        props.rows.map((row) =>
          h('div', { class: 'bar-row', key: row.label }, [
            h('div', { class: 'bar-row-header' }, [
              h('span', { class: 'bar-label' }, row.label),
              h('strong', { class: 'bar-value' }, props.formatFn(Number(row.value))),
            ]),
            h('div', { class: 'bar-track' }, [
              h('div', {
                class: 'bar-fill',
                style: { width: `${Math.max(5, (Number(row.value) / max) * 100)}%` },
              }),
            ]),
          ])
        )
      )
    }
  },
})

const doctorMode = computed(() => !isAdmin.value || selectedDoctorId.value !== '')
const hasEvents  = computed(() => (summary.value?.recentEvents.length ?? 0) > 0)

const kpiKeys = computed(() =>
  isAdmin.value && !selectedDoctorId.value
    ? ['appointmentsInRange', 'searches', 'profileViews', 'bookingClicks', 'createdBookingsFromEvents', 'usersInRange']
    : ['searchAppearances', 'profileViews', 'bookingClicks', 'createdBookingsFromEvents', 'todayBookings', 'averageRating']
)

const kpis = computed(() =>
  kpiKeys.value.map((key) => findMetric(key)).filter(Boolean) as AnalyticsMetric[]
)

const funnelRows = computed<AnalyticsLabelValue[]>(() => {
  const searches = findMetric('searches')?.value ?? findMetric('searchAppearances')?.value ?? 0
  return [
    { label: 'بحث/ظهور',   value: searches },
    { label: 'فتح بروفايل', value: findMetric('profileViews')?.value ?? 0 },
    { label: 'ضغط حجز',    value: findMetric('bookingClicks')?.value ?? 0 },
    { label: 'حجز مسجل',   value: findMetric('createdBookingsFromEvents')?.value ?? 0 },
  ]
})

const offerRows = computed<AnalyticsLabelValue[]>(() => [
  { label: 'وصول الإعلانات',    value: summary.value?.offers.views ?? 0 },
  { label: 'مشاهدات العروض',    value: summary.value?.offers.views ?? 0 },
  { label: 'ضغطات العروض',      value: summary.value?.offers.clicks ?? 0 },
  { label: 'حجوزات من العروض',  value: summary.value?.offers.bookingsFromOffers ?? 0 },
])

const subscriptionRows = computed<AnalyticsLabelValue[]>(() => [
  { label: 'مشتركين',       value: summary.value?.subscriptions.activeSubscribers  ?? 0 },
  { label: 'Premium',       value: summary.value?.subscriptions.premiumSubscribers ?? 0 },
  { label: 'Basic',         value: summary.value?.subscriptions.basicSubscribers   ?? 0 },
  { label: 'منتهية',        value: summary.value?.subscriptions.expiredSubscriptions ?? 0 },
  { label: 'قريبة الانتهاء', value: summary.value?.subscriptions.expiringSoon      ?? 0 },
])

const trend = computed(() =>
  doctorMode.value ? summary.value?.appointmentTrend ?? [] : summary.value?.userGrowth ?? []
)

const insightRows = computed(() => {
  const rows: { label: string; value: string }[] = []
  const profileRate = summary.value?.conversions.searchToProfileRate ?? 0
  const bookingRate = summary.value?.conversions.profileToBookingRate ?? 0
  const topProvince = summary.value?.topProvinces[0]?.label
  const topSearch   = summary.value?.topSearchTerms[0]?.label
  const topDoctor   = summary.value?.topDoctorsByBookings[0]?.label

  rows.push({ label: 'تحويل البحث إلى بروفايل',    value: `${formatNumber(profileRate)}%` })
  rows.push({ label: 'تحويل البروفايل إلى حجز',    value: `${formatNumber(bookingRate)}%` })
  if (topProvince) rows.push({ label: 'أقوى محافظة',     value: topProvince })
  if (topSearch)   rows.push({ label: 'أقوى كلمة بحث',   value: topSearch })
  if (topDoctor && isAdmin.value && !selectedDoctorId.value)
    rows.push({ label: 'أقوى طبيب بالحجز', value: topDoctor })
  return rows
})

// Helpers
function toInputDate(d: Date) { return d.toLocaleDateString('en-CA') }

function findMetric(key: string) {
  return summary.value?.metrics.find((m) => m.key === key)
}

function formatNumber(value = 0) {
  return new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 }).format(value)
}

function formatTime(value: string) {
  return new Intl.DateTimeFormat('en-US', {
    month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit',
  }).format(new Date(value))
}

function trendHeight(value: number) {
  const max = Math.max(...trend.value.map((r) => Number(r.value)), 1)
  return `${Math.max(7, (Number(value) / max) * 100)}%`
}

// API
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
    showError(getErrorMessage(error))
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

onMounted(() => Promise.all([loadDoctors(), loadAnalytics()]))
</script>

<template>
  <div class="analytics-page">

    <!-- Header -->
    <PageHeader
      :title="doctorMode ? 'تحليل أداء الطبيب' : 'تحليل نمو التطبيق'"
      :subtitle="doctorMode ? 'Doctor Analytics' : 'System Analytics'"
    >
      <template #actions>
        <v-btn
          color="primary"
          :loading="loading"
          prepend-icon="mdi-refresh"
          @click="loadAnalytics"
        >
          تحديث
        </v-btn>
      </template>
    </PageHeader>

    <!-- Filters -->
    <div class="filters-bar">
      <!-- Doctor Select (Admin only) -->
      <div v-if="isAdmin" class="filter-field">
        <label class="filter-label">الطبيب</label>
        <v-autocomplete
          v-model="selectedDoctorId"
          :items="doctorOptions"
          item-title="label"
          item-value="value"
          class="doctor-select"
          density="compact"
          variant="outlined"
          hide-details
          @update:model-value="loadAnalytics"
        />
      </div>

      <!-- From Date -->
      <div class="filter-field">
        <label class="filter-label">من</label>
        <input v-model="fromDate" type="date" class="filter-input" />
      </div>

      <!-- To Date -->
      <div class="filter-field">
        <label class="filter-label">إلى</label>
        <input v-model="toDate" type="date" class="filter-input" />
      </div>
    </div>

    <!-- Tabs -->
    <div class="tabs-bar">
      <button
        v-for="tab in tabs"
        :key="tab.key"
        type="button"
        class="tab-btn"
        :class="{ 'tab-active': activeTab === tab.key }"
        @click="activeTab = tab.key"
      >
        <v-icon :icon="tab.icon" size="17" />
        {{ tab.label }}
      </button>
    </div>

    <!-- KPI Cards -->
    <div class="kpi-grid" :class="{ 'kpi-loading': loading }">
      <template v-if="loading">
        <v-skeleton-loader v-for="i in 6" :key="i" type="card" height="100" />
      </template>
      <template v-else>
        <div v-for="item in kpis" :key="item.key" class="kpi-card">
          <p class="kpi-label">{{ item.label }}</p>
          <h3 class="kpi-value">{{ formatNumber(item.value) }}</h3>
        </div>
      </template>
    </div>

    <!-- ── Overview Tab ── -->
    <template v-if="activeTab === 'overview'">
      <div class="analytics-grid">

        <!-- Trend Chart -->
        <div class="panel panel-full">
          <div class="panel-header">
            <v-icon icon="mdi-trending-up" color="primary" size="20" />
            <h3>{{ doctorMode ? 'ترند الحجوزات' : 'ترند النمو' }}</h3>
          </div>
          <div v-if="trend.length" class="trend-chart">
            <div
              v-for="point in trend"
              :key="point.date"
              class="trend-bar-wrap"
              :title="`${formatNumber(point.value)}`"
            >
              <div class="trend-bar" :style="{ height: trendHeight(point.value) }" />
              <span class="trend-label">{{ point.label }}</span>
            </div>
          </div>
          <EmptyState v-else icon="mdi-chart-line" title="لا توجد بيانات" compact />
        </div>

        <!-- Funnel -->
        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-filter" color="primary" size="20" />
            <h3>مسار التحويل</h3>
          </div>
          <BarList :rows="funnelRows" :format-fn="formatNumber" />
        </div>

        <!-- Insights -->
        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-lightbulb" color="primary" size="20" />
            <h3>قراءة سريعة</h3>
          </div>
          <div class="insight-list">
            <div v-for="row in insightRows" :key="row.label" class="insight-row">
              <span class="insight-label">{{ row.label }}</span>
              <strong class="insight-value">{{ row.value }}</strong>
            </div>
          </div>
        </div>

      </div>
    </template>

    <!-- ── Offers Tab ── -->
    <template v-else-if="activeTab === 'offers'">
      <div class="analytics-grid">
        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-percent" color="primary" size="20" />
            <h3>أداء الإعلانات والعروض</h3>
          </div>
          <BarList :rows="offerRows" :format-fn="formatNumber" />
        </div>
        <div v-if="isAdmin && !selectedDoctorId" class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-account-group" color="primary" size="20" />
            <h3>الاشتراكات</h3>
          </div>
          <BarList :rows="subscriptionRows" :format-fn="formatNumber" />
        </div>
      </div>
    </template>

    <!-- ── Doctors Tab ── -->
    <template v-else-if="activeTab === 'doctors'">
      <div class="analytics-grid">
        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-eye" color="primary" size="20" />
            <h3>الأطباء الأكثر زيارة</h3>
          </div>
          <BarList :rows="summary?.topDoctorsByViews ?? []" :format-fn="formatNumber" />
        </div>
        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-calendar-check" color="primary" size="20" />
            <h3>الأطباء الأكثر حجزاً</h3>
          </div>
          <BarList :rows="summary?.topDoctorsByBookings ?? []" :format-fn="formatNumber" />
        </div>
      </div>
    </template>

    <!-- ── Bookings Tab ── -->
    <template v-else-if="activeTab === 'bookings'">
      <div class="analytics-grid">
        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-calendar" color="primary" size="20" />
            <h3>الحجوزات حسب الحالة</h3>
          </div>
          <BarList :rows="summary?.appointmentStatus ?? []" :format-fn="formatNumber" />
        </div>
        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-source-branch" color="primary" size="20" />
            <h3>الحجوزات حسب المصدر</h3>
          </div>
          <BarList :rows="summary?.appointmentSources ?? []" :format-fn="formatNumber" />
        </div>
        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-stethoscope" color="primary" size="20" />
            <h3>{{ doctorMode ? 'العيادات الأكثر حجزاً' : 'الاختصاصات الأكثر حجزاً' }}</h3>
          </div>
          <BarList
            :rows="doctorMode ? summary?.topClinicsByBookings ?? [] : summary?.topSpecializationsByBookings ?? []"
            :format-fn="formatNumber"
          />
        </div>
        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-clock-fast" color="primary" size="20" />
            <h3>أوقات الذروة</h3>
          </div>
          <BarList :rows="summary?.peakBookingHours ?? []" :format-fn="formatNumber" />
        </div>
      </div>
    </template>

    <!-- ── Search Tab ── -->
    <template v-else-if="activeTab === 'search'">
      <div class="analytics-grid">
        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-magnify" color="primary" size="20" />
            <h3>كلمات البحث</h3>
          </div>
          <BarList :rows="summary?.topSearchTerms ?? []" :format-fn="formatNumber" />
        </div>
        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-stethoscope" color="primary" size="20" />
            <h3>الاختصاصات الأكثر بحثاً</h3>
          </div>
          <BarList :rows="summary?.topSpecializationsBySearch ?? []" :format-fn="formatNumber" />
        </div>
        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-map-marker" color="primary" size="20" />
            <h3>المحافظات النشطة</h3>
          </div>
          <BarList :rows="summary?.topProvinces ?? []" :format-fn="formatNumber" />
        </div>
        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-eye" color="primary" size="20" />
            <h3>الصفحات الأكثر فتحاً</h3>
          </div>
          <BarList :rows="summary?.topPages ?? []" :format-fn="formatNumber" />
        </div>
      </div>
    </template>

    <!-- ── Activity Tab ── -->
    <template v-else>
      <div class="analytics-grid">
        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-lightning-bolt" color="primary" size="20" />
            <h3>آخر الأحداث</h3>
          </div>
          <div v-if="summary?.recentEvents.length" class="event-list">
            <div
              v-for="event in summary.recentEvents"
              :key="`${event.eventType}-${event.occurredAt}`"
              class="event-row"
            >
              <div class="event-left">
                <v-icon icon="mdi-circle-small" color="primary" size="16" />
                <strong class="event-label">{{ event.label }}</strong>
              </div>
              <div class="event-right">
                <span class="event-time">{{ formatTime(event.occurredAt) }}</span>
                <v-chip size="x-small" variant="tonal">
                  {{ event.source || event.page || 'mobile' }}
                </v-chip>
              </div>
            </div>
          </div>
          <EmptyState v-else icon="mdi-lightning-bolt-off" title="لا توجد أحداث" compact />
        </div>

        <div class="panel">
          <div class="panel-header">
            <v-icon icon="mdi-wifi" color="primary" size="20" />
            <h3>حالة الربط</h3>
          </div>
          <div class="connection-state" :class="{ 'connection-live': hasEvents }">
            <v-icon
              :icon="hasEvents ? 'mdi-check-circle' : 'mdi-alert-circle'"
              :color="hasEvents ? 'success' : 'warning'"
              size="40"
            />
            <div>
              <strong>{{ hasEvents ? 'Live' : 'No Events' }}</strong>
              <p>{{ hasEvents ? 'الأحداث تصل للنظام' : 'لا توجد أحداث ضمن الفترة المحددة' }}</p>
            </div>
          </div>
        </div>
      </div>
    </template>

  </div>
</template>

<style scoped>
.analytics-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

/* Filters */
.filters-bar {
  display: flex;
  align-items: flex-end;
  gap: var(--spacing-md);
  flex-wrap: wrap;
  padding: var(--spacing-lg);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}

.filter-field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.filter-label {
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-muted);
}

.filter-input {
  height: 40px;
  padding: 0 12px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s;
}

.filter-input { min-width: 150px; }
.doctor-select {
  min-width: 220px;
}

.filter-input:focus {
  border-color: var(--color-primary);
}

/* Tabs */
.tabs-bar {
  display: flex;
  gap: 6px;
  padding: 6px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  overflow-x: auto;
  flex-wrap: wrap;
}

.tab-btn {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  padding: 9px 14px;
  border: none;
  border-radius: var(--radius-md);
  background: transparent;
  color: var(--color-text-muted);
  font-family: var(--font-family-primary);
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  white-space: nowrap;
  transition: all 0.15s ease;
}

.tab-btn:hover {
  background: var(--color-primary-soft);
  color: var(--color-primary);
}

.tab-active {
  background: var(--color-primary-soft) !important;
  color: var(--color-primary) !important;
  font-weight: 700;
}

/* KPI Grid */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: var(--spacing-md);
  transition: opacity 0.2s;
}

.kpi-loading { opacity: 0.6; }

.kpi-card {
  padding: var(--spacing-lg);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}

.kpi-label {
  margin: 0 0 var(--spacing-md) 0;
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-muted);
  line-height: 1.4;
}

.kpi-value {
  margin: 0;
  font-size: 28px;
  font-weight: 800;
  color: var(--color-text);
  line-height: 1;
}

/* Panels */
.analytics-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--spacing-lg);
  align-items: start;
}

.panel {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  box-shadow: var(--shadow-sm);
}

.panel-full {
  grid-column: 1 / -1;
}

.panel-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-lg);
  padding-bottom: var(--spacing-md);
  border-bottom: 1px solid var(--color-border);
}

.panel-header h3 {
  margin: 0;
  font-size: 15px;
  font-weight: 700;
  color: var(--color-text);
}

/* Trend Chart */
.trend-chart {
  height: 190px;
  display: flex;
  align-items: flex-end;
  gap: 4px;
  padding-bottom: 24px;
  border-bottom: 2px solid var(--color-border);
  position: relative;
}

.trend-bar-wrap {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-end;
  gap: 4px;
  height: 100%;
}

.trend-bar {
  width: 100%;
  min-height: 7px;
  border-radius: 4px 4px 0 0;
  background: linear-gradient(180deg, var(--color-primary) 0%, var(--color-primary-light) 100%);
}

.trend-label {
  position: absolute;
  bottom: 4px;
  font-size: 9px;
  color: var(--color-text-muted);
  white-space: nowrap;
}

/* Bar List (from BarList component) */
:deep(.bar-list) {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

:deep(.bar-row) {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

:deep(.bar-row-header) {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--spacing-md);
}

:deep(.bar-label) {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text);
  overflow-wrap: anywhere;
}

:deep(.bar-value) {
  font-size: 13px;
  font-weight: 800;
  color: var(--color-primary);
  white-space: nowrap;
}

:deep(.bar-track) {
  height: 6px;
  background: var(--color-border);
  border-radius: 999px;
  overflow: hidden;
}

:deep(.bar-fill) {
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, var(--color-primary) 0%, var(--color-primary-light) 100%);
  transition: width 0.3s ease;
}

:deep(.empty-text) {
  margin: 0;
  color: var(--color-text-muted);
  font-size: 13px;
}

/* Insight List */
.insight-list {
  display: flex;
  flex-direction: column;
  gap: 0;
}

.insight-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-md) 0;
  border-bottom: 1px solid var(--color-border-light);
}

.insight-row:last-child { border-bottom: none; }

.insight-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text);
}

.insight-value {
  font-size: 14px;
  font-weight: 800;
  color: var(--color-primary);
}

/* Event List */
.event-list {
  display: flex;
  flex-direction: column;
  gap: 0;
}

.event-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-md) 0;
  border-bottom: 1px solid var(--color-border-light);
  flex-wrap: wrap;
}

.event-row:last-child { border-bottom: none; }

.event-left {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  flex: 1;
  min-width: 0;
}

.event-label {
  font-size: 13px;
  color: var(--color-text);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.event-right {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  flex-shrink: 0;
}

.event-time {
  font-size: 11px;
  color: var(--color-text-muted);
  font-weight: 600;
}

/* Connection State */
.connection-state {
  display: flex;
  align-items: center;
  gap: var(--spacing-lg);
  padding: var(--spacing-xl);
  border-radius: var(--radius-lg);
  background: var(--color-warning-light);
  color: var(--color-warning);
}

.connection-live {
  background: var(--color-success-light);
  color: var(--color-success);
}

.connection-state strong {
  display: block;
  font-size: 22px;
  font-weight: 800;
  margin-bottom: 4px;
}

.connection-state p {
  margin: 0;
  font-size: 13px;
  opacity: 0.8;
}

/* Responsive */
@media (max-width: 1200px) {
  .kpi-grid { grid-template-columns: repeat(3, 1fr); }
}

@media (max-width: 768px) {
  .analytics-grid { grid-template-columns: 1fr; }
  .panel-full { grid-column: 1; }
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 600px) {
  .kpi-grid { grid-template-columns: 1fr; }
  .filters-bar { flex-direction: column; }
  .filter-input, .doctor-select { width: 100%; }
}
</style>
