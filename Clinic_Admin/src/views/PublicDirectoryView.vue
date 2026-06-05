<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { CalendarDays, MapPin, Search, Star, Stethoscope } from '@lucide/vue'
import AppPagination from '../components/AppPagination.vue'
import api from '../services/api'
import { provinces } from '../constants/provinces'
import type { ApiResponse, PageResult, PublicDoctorListItem, SpecializationItem } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const doctors = ref<PublicDoctorListItem[]>([])
const specializations = ref<SpecializationItem[]>([])
const loading = ref(false)
const message = ref('')
const page = ref(1)
const totalPages = ref(1)
const filters = reactive({ name: '', specialization: '', iraqiProvince: '', sort: 'default' })
const apiOrigin = new URL(api.defaults.baseURL ?? 'https://localhost:7136/api').origin

function doctorImage(imageName?: string) {
  return imageName ? `${apiOrigin}/DoctorImage/${imageName}` : ''
}

async function loadSpecializations() {
  const response = await api.get<ApiResponse<SpecializationItem[]>>('/Specialization')
  specializations.value = response.data.data
}

async function loadDoctors() {
  loading.value = true
  message.value = ''
  try {
    const response = await api.get<ApiResponse<PageResult<PublicDoctorListItem>>>('/Doctor/public', {
      params: {
        name: filters.name || undefined,
        specialization: filters.specialization || undefined,
        iraqiProvince: filters.iraqiProvince === '' ? undefined : filters.iraqiProvince,
        sort: filters.sort || undefined,
        page: page.value,
        pageSize: 9,
      },
    })
    doctors.value = response.data.data.items
    totalPages.value = response.data.data.totalPages
  } catch (error: any) {
    if (error.response?.status === 404) {
      doctors.value = []
      totalPages.value = 1
    } else message.value = getErrorMessage(error)
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  page.value = 1
  loadDoctors()
}

function changePage(next: number) {
  page.value = next
  loadDoctors()
}

onMounted(async () => {
  try {
    await Promise.all([loadSpecializations(), loadDoctors()])
  } catch (error) {
    message.value = getErrorMessage(error)
  }
})
</script>

<template>
  <main class="public-page">
    <header class="public-topbar">
      <RouterLink class="public-brand" to="/directory"><Stethoscope :size="25" /> عيادتي</RouterLink>
      <RouterLink class="secondary-button" to="/guest-booking">متابعة حجز</RouterLink>
    </header>

    <section class="public-heading">
      <span class="section-kicker">دليل الأطباء</span>
      <h1>اختر الطبيب والعيادة واحجز دورك</h1>
    </section>

    <form class="filter-card public-filters" @submit.prevent="applyFilters">
      <label class="search-box"><Search :size="18" /><input v-model="filters.name" placeholder="اسم الطبيب" /></label>
      <select v-model="filters.specialization"><option value="">كل الاختصاصات</option><option v-for="item in specializations" :key="item.id" :value="item.id">{{ item.name }}</option></select>
      <select v-model="filters.iraqiProvince"><option value="">كل المحافظات</option><option v-for="province in provinces" :key="province.value" :value="province.value">{{ province.name }}</option></select>
      <select v-model="filters.sort">
        <option value="default">الأفضلية</option>
        <option value="rating">الأعلى تقييماً</option>
        <option value="reviews">الأكثر تقييمات</option>
        <option value="booking">الحجز الإلكتروني</option>
      </select>
      <button class="compact-primary" type="submit">بحث</button>
    </form>

    <p v-if="message" class="form-error">{{ message }}</p>
    <div v-if="loading" class="empty-panel">جارِ تحميل الأطباء...</div>
    <div v-else-if="!doctors.length" class="empty-panel">لا توجد نتائج مطابقة.</div>

    <section v-else class="public-doctor-grid">
      <article v-for="doctor in doctors" :key="doctor.id" class="public-doctor-card">
        <div class="public-doctor-photo"><img v-if="doctorImage(doctor.imageName)" :src="doctorImage(doctor.imageName)" :alt="doctor.name" /><Stethoscope v-else :size="34" /></div>
        <div class="public-doctor-copy">
          <h2>{{ doctor.name }}</h2>
          <p>{{ doctor.specializationName }}</p>
          <span v-if="doctor.averageRating" class="rating-line"><Star :size="15" /> {{ doctor.averageRating.toFixed(1) }} · {{ doctor.reviewCount }} تقييم</span>
          <span v-if="doctor.clinics[0]" class="clinic-line"><MapPin :size="15" /> {{ doctor.clinics[0].iraqiProvinceName }}، {{ doctor.clinics[0].address }}</span>
        </div>
        <RouterLink class="compact-primary public-card-action" :to="`/directory/${doctor.id}`"><CalendarDays :size="16" /> عرض وحجز</RouterLink>
      </article>
    </section>

    <AppPagination :page="page" :total-pages="totalPages" @change="changePage" />
  </main>
</template>

<style scoped>
.public-page { min-height: 100vh; padding: 24px; background: #f6f9f8; }.public-topbar { display: flex; justify-content: space-between; align-items: center; gap: 12px; max-width: 1120px; margin: 0 auto 26px; }
.public-brand { display: flex; align-items: center; gap: 9px; color: var(--primary); text-decoration: none; font-size: 24px; font-weight: 800; }
.public-heading { max-width: 1120px; margin: 0 auto 16px; }.public-heading h1 { margin: 6px 0 0; color: var(--ink); font-size: 32px; }
.public-filters { max-width: 1120px; margin-inline: auto; }
.public-doctor-grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 14px; max-width: 1120px; margin: 0 auto; }
.public-doctor-card { display: grid; grid-template-columns: 78px 1fr; gap: 12px; padding: 15px; border: 1px solid var(--line); border-radius: 14px; background: #fff; box-shadow: var(--shadow); }
.public-doctor-photo { display: grid; place-items: center; width: 78px; height: 78px; overflow: hidden; color: var(--primary); border-radius: 20px; background: var(--primary-soft); }.public-doctor-photo img { width: 100%; height: 100%; object-fit: cover; }
.public-doctor-copy h2 { margin: 0 0 4px; font-size: 19px; }.public-doctor-copy p { margin: 0 0 9px; color: var(--muted); }.rating-line, .clinic-line { display: flex; align-items: center; gap: 5px; color: var(--muted); font-size: 12px; margin-top: 5px; }
.public-card-action { grid-column: 1 / -1; text-decoration: none; }
@media (max-width: 900px) { .public-doctor-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
@media (max-width: 620px) { .public-page { padding: 16px; }.public-doctor-grid { grid-template-columns: 1fr; }.public-topbar { align-items: flex-start; flex-direction: column; } }
</style>
