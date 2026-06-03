<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { MessageSquareText, RefreshCw, ShieldAlert, Star } from '@lucide/vue'
import AppPagination from '../components/AppPagination.vue'
import api from '../services/api'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, DoctorReviews } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const notifications = useNotificationsStore()
const loading = ref(false)
const reviewsData = ref<DoctorReviews>()
const page = ref(1)
const pageSize = 5
const reviews = computed(() => reviewsData.value?.reviews ?? [])
const totalPages = computed(() => Math.max(1, Math.ceil(reviews.value.length / pageSize)))
const paginatedReviews = computed(() => reviews.value.slice((page.value - 1) * pageSize, page.value * pageSize))
const average = computed(() => reviewsData.value?.averageRating?.toFixed(1) ?? '-')

async function loadReviews() {
  loading.value = true
  try {
    const response = await api.get<ApiResponse<DoctorReviews>>('/Review/doctor/my')
    reviewsData.value = response.data.data
    if (page.value > totalPages.value) page.value = totalPages.value
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    loading.value = false
  }
}

function isFilled(rating: number, star: number) {
  return star <= rating
}

function changePage(newPage: number) {
  page.value = newPage
}

onMounted(loadReviews)
</script>

<template>
  <div>
    <div class="page-heading">
      <div><span class="section-kicker">آراء المراجعين</span><h2>تقييمات الطبيب</h2><p>تابع متوسط التقييم والتعليقات المضافة بعد اكتمال الحجوزات.</p></div>
      <button class="secondary-button" type="button" :disabled="loading" @click="loadReviews"><RefreshCw :size="17" /> تحديث</button>
    </div>

    <div v-if="loading && !reviewsData" class="empty-panel">جارِ تحميل التقييمات...</div>
    <template v-else-if="reviewsData">
      <div v-if="!reviewsData.isEnabled" class="feature-notice">
        <ShieldAlert :size="21" />
        <div><strong>عرض التقييمات للمرضى غير مفعّل حالياً</strong><p>يمكنك مراجعة التقييمات السابقة هنا، لكنها لن تظهر في الدليل الطبي حتى تفعيل الميزة ضمن الباقة والإعدادات.</p></div>
      </div>

      <section class="review-summary">
        <article class="rating-card">
          <span class="rating-icon"><Star :size="22" /></span>
          <div><small>متوسط التقييم</small><strong>{{ average }}</strong><span>من 5 نجوم</span></div>
        </article>
        <article class="rating-card">
          <span class="rating-icon comments-icon"><MessageSquareText :size="22" /></span>
          <div><small>عدد التقييمات</small><strong>{{ reviewsData.reviewCount }}</strong><span>تقييم موثّق بحجز مكتمل</span></div>
        </article>
      </section>

      <div v-if="!reviews.length" class="empty-panel review-empty">
        <MessageSquareText :size="32" /><h3>لا توجد تقييمات بعد</h3><p>ستظهر تقييمات المراجعين هنا بعد إكمال حجوزاتهم وإرسال آرائهم.</p>
      </div>
      <section v-else class="reviews-list">
        <article v-for="review in paginatedReviews" :key="review.id" class="review-card">
          <div class="review-card-header">
            <div><strong>{{ review.user.name || 'مراجع' }}</strong><small v-if="review.appoinmentId">حجز رقم #{{ review.appoinmentId }}</small></div>
            <div class="stars" :aria-label="`${review.rating} من 5 نجوم`">
              <Star v-for="star in 5" :key="star" :size="17" :class="{ filled: isFilled(review.rating, star) }" />
            </div>
          </div>
          <p>{{ review.comment || 'لم يضف المراجع تعليقاً نصياً.' }}</p>
        </article>
      </section>
      <AppPagination :page="page" :total-pages="totalPages" @change="changePage" />
    </template>
  </div>
</template>
