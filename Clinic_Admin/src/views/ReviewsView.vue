<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import api from '../services/api'
import { useNotifications } from '../composables/useNotifications'
import type { ApiResponse, DoctorReviewsPageResult, ReviewItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import EmptyState from '../components/common/Emptystate.vue'

const { error: showError } = useNotifications()

const loading     = ref(false)
const data        = ref<DoctorReviewsPageResult>()
const page        = ref(1)
const pageSize    = 10

const reviews = computed<ReviewItem[]>(() => data.value?.items ?? data.value?.reviews ?? [])

const totalItems = computed(() => {
  const d = data.value
  return d?.totalItems ?? d?.reviewCount ?? reviews.value.length
})

const totalPages = computed(() => {
  const d = data.value
  if (d?.totalPages) return d.totalPages
  return Math.max(1, Math.ceil(totalItems.value / pageSize))
})

const average = computed(() => data.value?.averageRating?.toFixed(1) ?? '-')

function isFilled(rating: number, star: number) { return star <= rating }

const displayed = computed(() => {
  if (data.value?.items) return reviews.value
  return reviews.value.slice((page.value - 1) * pageSize, page.value * pageSize)
})

async function loadReviews() {
  loading.value = true
  try {
    const r = await api.get<ApiResponse<DoctorReviewsPageResult>>('/Review/doctor/my', {
      params: { page: page.value, pageSize },
    })
    data.value = r.data.data
  } catch (e) { showError(getErrorMessage(e)) }
  finally { loading.value = false }
}

watch(page, () => {
  if (!data.value?.items) {
    data.value = undefined
    loadReviews()
  }
})

onMounted(loadReviews)
</script>

<template>
  <div class="reviews-page">

    <!-- Header -->
    <div class="page-top">
      <div>
        <p class="page-kicker">آراء المراجعين</p>
        <h1 class="page-title">تقييمات الطبيب</h1>
      </div>
      <v-btn variant="outlined" color="primary" prepend-icon="mdi-refresh" :loading="loading" @click="loadReviews">
        تحديث
      </v-btn>
    </div>

    <!-- Loading -->
    <template v-if="loading && !data">
      <div class="summary-grid">
        <v-skeleton-loader v-for="i in 2" :key="i" type="card" height="120" />
      </div>
      <v-skeleton-loader v-for="i in 3" :key="i" type="list-item-two-line" class="mb-2" />
    </template>

    <template v-else-if="data">

      <!-- Feature Disabled Alert -->
      <v-alert
        v-if="!data.isEnabled"
        type="warning"
        variant="tonal"
        icon="mdi-shield-alert"
      >
        <strong>عرض التقييمات للمرضى غير مفعّل حالياً</strong>
        <p style="margin: 4px 0 0; font-size: 13px; opacity: 0.85;">
          يمكنك مراجعة التقييمات السابقة هنا، لكنها لن تظهر في الدليل الطبي حتى تفعيل الميزة ضمن الباقة والإعدادات.
        </p>
      </v-alert>

      <!-- Summary Cards -->
      <div class="summary-grid">
        <!-- Average Rating -->
        <div class="summary-card summary-card--star">
          <div class="summary-icon">
            <v-icon icon="mdi-star" color="warning" size="26" />
          </div>
          <div class="summary-info">
            <p class="summary-label">متوسط التقييم</p>
            <strong class="summary-value">{{ average }}</strong>
            <span class="summary-sub">من 5 نجوم</span>
          </div>
          <!-- Star Display -->
          <div class="stars-display">
            <v-icon
              v-for="star in 5"
              :key="star"
              :icon="star <= Math.round(Number(average)) ? 'mdi-star' : 'mdi-star-outline'"
              :color="star <= Math.round(Number(average)) ? 'warning' : 'var(--color-border)'"
              size="20"
            />
          </div>
        </div>

        <!-- Review Count -->
        <div class="summary-card">
          <div class="summary-icon">
            <v-icon icon="mdi-comment-text" color="primary" size="26" />
          </div>
          <div class="summary-info">
            <p class="summary-label">عدد التقييمات</p>
            <strong class="summary-value">{{ totalItems }}</strong>
            <span class="summary-sub">تقييم موثّق بحجز مكتمل</span>
          </div>
        </div>
      </div>

      <!-- Empty -->
      <EmptyState
        v-if="!reviews.length"
        icon="mdi-comment-off"
        title="لا توجد تقييمات بعد"
        description="ستظهر تقييمات المراجعين هنا بعد إكمال حجوزاتهم وإرسال آرائهم"
      />

      <!-- Reviews List -->
      <div v-else class="reviews-list">
        <div v-for="review in displayed" :key="review.id" class="review-card">
          <!-- Header -->
          <div class="review-card-header">
            <div class="reviewer-info">
              <div class="reviewer-avatar">
                <v-icon icon="mdi-account" size="18" color="primary" />
              </div>
              <div>
                <strong class="reviewer-name">{{ review.user.name || 'مراجع' }}</strong>
                <p v-if="review.appointmentId" class="reviewer-sub">حجز رقم #{{ review.appointmentId }}</p>
              </div>
            </div>

            <!-- Stars -->
            <div class="stars-row" :aria-label="`${review.rating} من 5 نجوم`">
              <v-icon
                v-for="star in 5"
                :key="star"
                :icon="isFilled(review.rating, star) ? 'mdi-star' : 'mdi-star-outline'"
                :color="isFilled(review.rating, star) ? 'warning' : 'var(--color-border)'"
                size="18"
              />
              <span class="rating-number">{{ review.rating }}/5</span>
            </div>
          </div>

          <!-- Comment -->
          <p class="review-comment">
            {{ review.comment || 'لم يضف المراجع تعليقاً نصياً.' }}
          </p>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="pagination-bar">
        <v-pagination
          v-model="page"
          :length="totalPages"
          :total-visible="5"
          density="compact"
          color="primary"
        />
      </div>

    </template>

  </div>
</template>

<style scoped>
.reviews-page { display: flex; flex-direction: column; gap: var(--spacing-lg); }

/* Page Top */
.page-top { display: flex; align-items: center; justify-content: space-between; gap: var(--spacing-lg); flex-wrap: wrap; }
.page-kicker { margin: 0 0 4px 0; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--color-text-muted); }
.page-title { margin: 0; font-size: 28px; font-weight: 800; color: var(--color-text); }

/* Summary Grid */
.summary-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-lg);
}

.summary-card {
  display: flex;
  align-items: center;
  gap: var(--spacing-lg);
  padding: var(--spacing-xl);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  flex-wrap: wrap;
}

.summary-card--star {
  background: linear-gradient(135deg, #fffbeb, #fef9c3);
  border-color: #fde68a;
}

.summary-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 56px;
  height: 56px;
  border-radius: var(--radius-lg);
  background: white;
  flex-shrink: 0;
  box-shadow: var(--shadow-sm);
}

.summary-info { flex: 1; }

.summary-label {
  margin: 0 0 4px 0;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-muted);
}

.summary-value {
  display: block;
  font-size: 36px;
  font-weight: 800;
  color: var(--color-text);
  line-height: 1.1;
  margin-bottom: 2px;
}

.summary-sub {
  font-size: 12px;
  color: var(--color-text-muted);
}

.stars-display {
  display: flex;
  gap: 2px;
  flex-shrink: 0;
}

/* Reviews List */
.reviews-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.review-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  box-shadow: var(--shadow-sm);
  transition: box-shadow 0.2s ease;
}

.review-card:hover { box-shadow: var(--shadow-md); }

.review-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-lg);
  margin-bottom: var(--spacing-lg);
  padding-bottom: var(--spacing-md);
  border-bottom: 1px solid var(--color-border-light);
  flex-wrap: wrap;
}

.reviewer-info {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

.reviewer-avatar {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  background: var(--color-primary-soft);
  flex-shrink: 0;
}

.reviewer-name {
  font-size: 15px;
  font-weight: 700;
  color: var(--color-text);
}

.reviewer-sub {
  margin: 2px 0 0 0;
  font-size: 12px;
  color: var(--color-text-muted);
}

.stars-row {
  display: flex;
  align-items: center;
  gap: 2px;
}

.rating-number {
  margin-right: var(--spacing-sm);
  font-size: 13px;
  font-weight: 700;
  color: var(--color-text-muted);
}

[dir='rtl'] .rating-number {
  margin-right: 0;
  margin-left: var(--spacing-sm);
}

.review-comment {
  margin: 0;
  font-size: 14px;
  color: var(--color-text);
  line-height: 1.7;
  font-style: italic;
}

/* Pagination */
.pagination-bar { display: flex; justify-content: center; }

/* Responsive */
@media (max-width: 600px) {
  .summary-grid { grid-template-columns: 1fr; }
  .review-card-header { flex-direction: column; align-items: flex-start; }
}
</style>