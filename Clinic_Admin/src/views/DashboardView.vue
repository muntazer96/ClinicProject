<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'
import { useNotifications } from '../composables/useNotifications'
import type { AnalyticsSummary, ApiResponse, CurrentDoctorSubscription } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import PageHeader from '../components/common/Pageheader.vue'
import EmptyState from '../components/common/Emptystate.vue'

const auth = useAuthStore()
const router = useRouter()
const { error: showError } = useNotifications()

const isAdmin = computed(() => auth.hasAnyRole(['SuperAdmin']) && !auth.hasAnyRole(['DoctorUser']))
const loading = ref(false)
const summary = ref<AnalyticsSummary | null>(null)
const currentSubscription = ref<CurrentDoctorSubscription | null>(null)

const today = new Date()
const fromDate = toInputDate(new Date(today.getFullYear(), today.getMonth(), today.getDate() - 7))
const toDate = toInputDate(today)

// Stat cards config
const statCards = computed(() => {
  if (isAdmin.value) {
    return [
      { key: 'appointmentsInRange', icon: 'mdi-calendar-month', color: 'success' },
      { key: 'profileViews',        icon: 'mdi-eye',            color: 'info'    },
      { key: 'bookingClicks',       icon: 'mdi-cursor-default-click', color: 'warning' },
      { key: 'usersInRange',        icon: 'mdi-account-group',  color: 'primary' },
    ]
  }
  return [
    { key: 'todayBookings',  icon: 'mdi-calendar-today',  color: 'success' },
    { key: 'profileViews',   icon: 'mdi-eye',              color: 'info'    },
    { key: 'bookingClicks',  icon: 'mdi-cursor-default-click', color: 'warning' },
    { key: 'averageRating',  icon: 'mdi-star',             color: 'primary' },
  ]
})

const quickRows = computed(() => isAdmin.value ? [
  { label: 'الأطباء الأكثر زيارة',  icon: 'mdi-eye-outline',      rows: summary.value?.topDoctorsByViews    ?? [] },
  { label: 'الأطباء الأكثر حجزاً',  icon: 'mdi-calendar-check',   rows: summary.value?.topDoctorsByBookings ?? [] },
  { label: 'أكثر كلمات البحث',      icon: 'mdi-magnify',          rows: summary.value?.topSearchTerms       ?? [] },
] : [
  { label: 'مصادر الحجز',           icon: 'mdi-source-branch',    rows: summary.value?.appointmentSources   ?? [] },
  { label: 'أكثر أيام الحجز',       icon: 'mdi-calendar-week',    rows: summary.value?.topBookingDays       ?? [] },
  { label: 'أوقات الذروة',          icon: 'mdi-clock-fast',       rows: summary.value?.peakBookingHours     ?? [] },
])

const offerStats = computed(() => [
  { label: 'مشاهدات العروض',    value: summary.value?.offers.views             ?? 0 },
  { label: 'ضغطات العروض',      value: summary.value?.offers.clicks            ?? 0 },
  { label: 'حجوزات من العروض',  value: summary.value?.offers.bookingsFromOffers ?? 0 },
])

const trend = computed(() =>
  isAdmin.value ? summary.value?.userGrowth ?? [] : summary.value?.appointmentTrend ?? []
)

const daysRemaining = computed(() => currentSubscription.value?.daysRemaining ?? null)

const subscriptionColor = computed(() => {
  const days = daysRemaining.value
  if (days == null) return 'success'
  if (days <= 1)    return 'error'
  if (days <= 3)    return 'warning'
  return 'success'
})

const subscriptionBg = computed(() => {
  const days = daysRemaining.value
  if (days == null) return 'var(--color-success-light)'
  if (days <= 1)    return 'var(--color-error-light)'
  if (days <= 3)    return 'var(--color-warning-light)'
  return 'var(--color-success-light)'
})

const subscriptionIconColor = computed(() => {
  const days = daysRemaining.value
  if (days == null) return 'var(--color-success)'
  if (days <= 1)    return 'var(--color-error)'
  if (days <= 3)    return 'var(--color-warning)'
  return 'var(--color-success)'
})

// Helpers
function toInputDate(date: Date) {
  return date.toLocaleDateString('en-CA')
}

function metric(key: string) {
  return summary.value?.metrics.find((m) => m.key === key)
}

function formatNumber(value = 0) {
  return new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 }).format(value)
}

function barHeight(value: number) {
  const max = Math.max(...trend.value.map((item) => Number(item.value)), 1)
  return `${Math.max(8, (Number(value) / max) * 100)}%`
}

// API calls
async function loadSummary() {
  loading.value = true
  try {
    const endpoint = isAdmin.value ? '/Analytics/admin/summary' : '/Analytics/doctor/summary'
    const response = await api.get<ApiResponse<AnalyticsSummary>>(endpoint, {
      params: { fromDate, toDate },
    })
    summary.value = response.data.data
  } catch (error) {
    showError(getErrorMessage(error))
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
    if (error.response?.status !== 404) showError(getErrorMessage(error))
  }
}

onMounted(() => Promise.all([loadSummary(), loadCurrentSubscription()]))
</script>

<template>
  <div class="dashboard">

    <!-- Header -->
    <PageHeader
      :title="isAdmin ? 'نظرة عامة على النظام' : 'نظرة عامة على عيادتك'"
      :subtitle="isAdmin ? 'Super Admin' : 'Doctor'"
    >
      <template #actions>
        <v-btn
          variant="outlined"
          color="primary"
          :loading="loading"
          prepend-icon="mdi-refresh"
          @click="loadSummary"
        >
          تحديث
        </v-btn>
        <v-btn
          color="primary"
          append-icon="mdi-arrow-left"
          @click="router.push('/analytics')"
        >
          الإحصائيات
        </v-btn>
      </template>
    </PageHeader>

    <!-- Subscription Banner (Doctor only) -->
    <div
      v-if="!isAdmin && currentSubscription"
      class="subscription-banner"
      :style="{ backgroundColor: subscriptionBg, borderColor: subscriptionIconColor }"
    >
      <div class="subscription-icon" :style="{ backgroundColor: subscriptionBg }">
        <v-icon icon="mdi-calendar-clock" size="24" :color="subscriptionIconColor" />
      </div>
      <div class="subscription-info">
        <p class="subscription-kicker">الاشتراك الحالي</p>
        <h3 class="subscription-name">
          {{ currentSubscription.packageArabicName || currentSubscription.packageName }}
        </h3>
        <p class="subscription-remaining">
          متبقي <strong>{{ daysRemaining }}</strong> يوم حتى انتهاء الاشتراك
        </p>
      </div>
      <v-chip :color="subscriptionColor" variant="tonal" size="small" class="subscription-chip">
        {{ daysRemaining }} يوم
      </v-chip>
    </div>

    <!-- Stat Cards -->
    <div class="stats-grid">
      <template v-if="loading">
        <v-skeleton-loader
          v-for="i in 4"
          :key="i"
          type="card"
          class="stat-skeleton"
        />
      </template>
      <template v-else>
        <div
          v-for="card in statCards"
          :key="card.key"
          class="stat-card"
        >
          <div class="stat-icon" :class="`stat-icon--${card.color}`">
            <v-icon :icon="card.icon" size="22" />
          </div>
          <div class="stat-info">
            <p class="stat-label">{{ metric(card.key)?.label ?? '-' }}</p>
            <h3 class="stat-value">{{ formatNumber(metric(card.key)?.value) }}</h3>
          </div>
        </div>
      </template>
    </div>

    <!-- Charts Row -->
    <div class="overview-grid">

      <!-- Trend Chart -->
      <div class="panel">
        <div class="panel-header">
          <v-icon icon="mdi-chart-bar" color="primary" size="20" />
          <h3>{{ isAdmin ? 'نمو المستخدمين' : 'حركة الحجوزات' }}</h3>
        </div>
        <div v-if="loading" class="chart-skeleton">
          <v-skeleton-loader type="image" height="170" />
        </div>
        <div v-else-if="trend.length" class="mini-chart">
          <div class="chart-bars">
            <div
              v-for="point in trend"
              :key="point.date"
              class="chart-bar-wrapper"
              :title="`${formatNumber(point.value)}`"
            >
              <div class="chart-bar" :style="{ height: barHeight(point.value) }" />
              <span class="chart-label">{{ point.label }}</span>
            </div>
          </div>
        </div>
        <EmptyState
          v-else
          icon="mdi-chart-bar"
          title="لا توجد بيانات"
          compact
        />
      </div>

      <!-- Offers Panel -->
      <div class="panel">
        <div class="panel-header">
          <v-icon icon="mdi-percent" color="primary" size="20" />
          <h3>العروض</h3>
        </div>
        <div v-if="loading">
          <v-skeleton-loader v-for="i in 3" :key="i" type="list-item" />
        </div>
        <div v-else class="summary-list">
          <div v-for="row in offerStats" :key="row.label" class="summary-row">
            <span class="summary-label">{{ row.label }}</span>
            <strong class="summary-value">{{ formatNumber(row.value) }}</strong>
          </div>
        </div>
      </div>
    </div>

    <!-- Quick Stats Rows -->
    <div class="overview-grid three-cols">
      <div v-for="group in quickRows" :key="group.label" class="panel">
        <div class="panel-header">
          <v-icon :icon="group.icon" color="primary" size="20" />
          <h3>{{ group.label }}</h3>
        </div>
        <div v-if="loading">
          <v-skeleton-loader v-for="i in 4" :key="i" type="list-item" />
        </div>
        <div v-else-if="group.rows.length" class="rank-list">
          <div
            v-for="(row, index) in group.rows.slice(0, 5)"
            :key="row.label"
            class="rank-row"
          >
            <div class="rank-left">
              <span class="rank-number">{{ index + 1 }}</span>
              <span class="rank-label">{{ row.label }}</span>
            </div>
            <strong class="rank-value">{{ formatNumber(row.value) }}</strong>
          </div>
        </div>
        <EmptyState v-else icon="mdi-database-off" title="لا توجد بيانات" compact />
      </div>
    </div>

    <!-- No Events Alert -->
    <v-alert
      v-if="!loading && !(summary?.recentEvents?.length)"
      type="info"
      variant="tonal"
      icon="mdi-information"
      class="no-events-alert"
    >
      لا توجد أحداث تطبيق مسجلة لهذه الفترة
    </v-alert>

  </div>
</template>

<style scoped>
.dashboard {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

/* Subscription Banner */
.subscription-banner {
  display: flex;
  align-items: center;
  gap: var(--spacing-lg);
  padding: var(--spacing-lg);
  border: 1px solid;
  border-radius: var(--radius-lg);
}

.subscription-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: var(--radius-md);
  flex-shrink: 0;
}

.subscription-info {
  flex: 1;
}

.subscription-kicker {
  margin: 0;
  padding: 0;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-muted);
}

.subscription-name {
  margin: 4px 0;
  padding: 0;
  font-size: 16px;
  font-weight: 700;
  color: var(--color-text);
}

.subscription-remaining {
  margin: 0;
  padding: 0;
  font-size: 13px;
  color: var(--color-text-muted);
}

.subscription-remaining strong {
  font-weight: 800;
  color: var(--color-text);
}

.subscription-chip {
  flex-shrink: 0;
}

/* Stats Grid */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--spacing-lg);
}

.stat-skeleton {
  border-radius: var(--radius-lg);
}

.stat-card {
  display: flex;
  align-items: center;
  gap: var(--spacing-lg);
  padding: var(--spacing-lg);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  transition: box-shadow 0.2s ease;
}

.stat-card:hover {
  box-shadow: var(--shadow-md);
}

.stat-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: var(--radius-md);
  flex-shrink: 0;
}

.stat-icon--success { background: var(--color-success-light); color: var(--color-success); }
.stat-icon--info    { background: var(--color-info-light);    color: var(--color-info);    }
.stat-icon--warning { background: var(--color-warning-light); color: var(--color-warning); }
.stat-icon--primary { background: var(--color-primary-soft);  color: var(--color-primary); }
.stat-icon--error   { background: var(--color-error-light);   color: var(--color-error);   }

.stat-info {
  flex: 1;
  min-width: 0;
}

.stat-label {
  margin: 0 0 4px 0;
  padding: 0;
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text-muted);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.stat-value {
  margin: 0;
  padding: 0;
  font-size: 24px;
  font-weight: 800;
  color: var(--color-text);
  line-height: 1.2;
}

/* Panels */
.overview-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--spacing-lg);
}

.overview-grid.three-cols {
  grid-template-columns: repeat(3, 1fr);
}

.panel {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  box-shadow: var(--shadow-sm);
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
  padding: 0;
  font-size: 15px;
  font-weight: 700;
  color: var(--color-text);
}

/* Mini Chart */
.mini-chart {
  height: 170px;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
}

.chart-bars {
  display: flex;
  align-items: flex-end;
  gap: 4px;
  height: 140px;
  padding-bottom: 8px;
  border-bottom: 2px solid var(--color-border);
}

.chart-bar-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-end;
  gap: 4px;
  height: 100%;
}

.chart-bar {
  width: 100%;
  min-height: 8px;
  border-radius: 4px 4px 0 0;
  background: linear-gradient(180deg, var(--color-primary) 0%, var(--color-primary-light) 100%);
  transition: height 0.3s ease;
}

.chart-label {
  font-size: 9px;
  color: var(--color-text-muted);
  text-align: center;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 100%;
}

/* Summary List */
.summary-list {
  display: flex;
  flex-direction: column;
  gap: 0;
}

.summary-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-md) 0;
  border-bottom: 1px solid var(--color-border-light);
}

.summary-row:last-child {
  border-bottom: none;
}

.summary-label {
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text);
}

.summary-value {
  font-size: 16px;
  font-weight: 800;
  color: var(--color-primary);
}

/* Rank List */
.rank-list {
  display: flex;
  flex-direction: column;
  gap: 0;
}

.rank-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-md) 0;
  border-bottom: 1px solid var(--color-border-light);
  gap: var(--spacing-md);
}

.rank-row:last-child {
  border-bottom: none;
}

.rank-left {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  min-width: 0;
  flex: 1;
}

.rank-number {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: var(--color-primary-soft);
  color: var(--color-primary);
  font-size: 11px;
  font-weight: 800;
  flex-shrink: 0;
}

.rank-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.rank-value {
  font-size: 14px;
  font-weight: 800;
  color: var(--color-primary);
  white-space: nowrap;
  flex-shrink: 0;
}

.no-events-alert {
  margin-top: var(--spacing-sm);
}

/* Responsive */
@media (max-width: 1200px) {
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .overview-grid.three-cols {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 768px) {
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .overview-grid,
  .overview-grid.three-cols {
    grid-template-columns: 1fr;
  }

  .subscription-banner {
    flex-wrap: wrap;
  }
}

@media (max-width: 600px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }

  .stat-value {
    font-size: 20px;
  }
}
</style>