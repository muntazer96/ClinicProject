<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '../services/api'
import { useNotifications } from '../composables/useNotifications'
import type { ApiResponse, ClinicItem, DoctorItem, PageResult } from '../types/api'
import { getErrorMessage } from '../utils/errors'
import EmptyState from '../components/common/Emptystate.vue'

const route  = useRoute()
const router = useRouter()
const { error: showError } = useNotifications()

const doctor   = ref<DoctorItem>()
const clinics  = ref<ClinicItem[]>([])
const loading  = ref(false)
const doctorId = Number(route.params.doctorId)
const doctorImage = ref('')
let doctorImageBlobUrl = ''

function cleanupDoctorImage() {
  if (doctorImageBlobUrl) {
    URL.revokeObjectURL(doctorImageBlobUrl)
    doctorImageBlobUrl = ''
  }
  doctorImage.value = ''
}

async function loadDoctorImage(path?: string): Promise<string> {
  if (!path) return ''
  const fileName = path.replace('/DoctorImage/', '')
  const normalizedFileName = (() => {
    try {
      return decodeURIComponent(fileName)
    } catch {
      return fileName
    }
  })()

  try {
    const res = await api.get(`/Files/doctor-image/${encodeURIComponent(normalizedFileName)}`, {
      responseType: 'blob',
    })
    doctorImageBlobUrl = URL.createObjectURL(res.data)
    return doctorImageBlobUrl
  } catch {
    return ''
  }
}

async function loadDetails() {
  loading.value = true
  cleanupDoctorImage()
  try {
    const [doctorRes, clinicRes] = await Promise.all([
      api.get<ApiResponse<PageResult<DoctorItem>>>('/Doctor', { params: { id: doctorId, page: 1, pageSize: 1 } }),
      api.get<ApiResponse<ClinicItem[]>>(`/Clinic/doctor/${doctorId}/admin`),
    ])
    doctor.value  = doctorRes.data.data.items[0]
    clinics.value = clinicRes.data.data
    doctorImage.value = await loadDoctorImage(doctor.value?.imageName)
    if (!doctor.value) showError('لم يتم العثور على ملف الطبيب.')
  } catch (e) {
    showError(getErrorMessage(e))
  } finally {
    loading.value = false
  }
}

onMounted(loadDetails)
onUnmounted(cleanupDoctorImage)
</script>

<template>
  <div class="doctor-details-page">

    <!-- Back Button -->
    <div class="page-top">
      <v-btn
        variant="outlined"
        color="primary"
        prepend-icon="mdi-arrow-right"
        @click="router.push('/doctors')"
      >
        العودة للأطباء
      </v-btn>
    </div>

    <!-- Loading -->
    <template v-if="loading">
      <v-skeleton-loader type="card" height="200" />
      <div class="skeletons-grid">
        <v-skeleton-loader v-for="i in 4" :key="i" type="card" height="140" />
      </div>
    </template>

    <template v-else-if="doctor">

      <!-- Hero Banner -->
      <div class="doctor-hero">
        <!-- Avatar -->
        <div class="doctor-avatar">
          <img v-if="doctorImage" :src="doctorImage" :alt="doctor.name" />
          <v-icon v-else icon="mdi-account" size="48" color="primary" />
        </div>

        <!-- Info -->
        <div class="doctor-hero-info">
          <p class="hero-kicker">ملف طبيب رقم #{{ doctor.id }}</p>
          <h1 class="hero-name">{{ doctor.name }}</h1>
          <p class="hero-sub">{{ doctor.specialization?.name }} · {{ doctor.iraqiProvinceName }}</p>

          <div class="hero-badges">
            <v-chip
              size="small"
              :color="doctor.isPubliclyVisible ? 'success' : 'warning'"
              variant="tonal"
            >
              <v-icon
                :icon="doctor.isPubliclyVisible ? 'mdi-eye' : 'mdi-eye-off'"
                size="14"
                start
              />
              {{ doctor.isPubliclyVisible ? 'ظاهر للعامة' : 'مخفي عن العامة' }}
            </v-chip>
            <v-chip
              size="small"
              :color="doctor.userId ? 'success' : 'default'"
              variant="tonal"
            >
              <v-icon
                :icon="doctor.userId ? 'mdi-account-check' : 'mdi-account-off'"
                size="14"
                start
              />
              {{ doctor.userId ? 'حساب الدخول مربوط' : 'بدون حساب دخول' }}
            </v-chip>
          </div>
        </div>

        <!-- Clinic Count Badge -->
        <div class="hero-stat">
          <v-icon icon="mdi-hospital-building" color="white" size="28" />
          <strong>{{ clinics.length }}</strong>
          <span>عيادة مسجلة</span>
        </div>
      </div>

      <!-- Info Cards -->
      <div class="info-grid">
        <!-- Contact Info -->
        <div class="info-card info-card--wide">
          <div class="info-card-header">
            <v-icon icon="mdi-card-account-details" color="primary" size="20" />
            <h3>معلومات الاتصال</h3>
          </div>
          <div class="info-lines">
            <div v-if="doctor.phoneNumber" class="info-line">
              <v-icon icon="mdi-phone" size="17" color="primary" />
              <span>{{ doctor.phoneNumber }}</span>
            </div>
            <div v-if="doctor.location" class="info-line">
              <v-icon icon="mdi-map-marker" size="17" color="primary" />
              <span>{{ doctor.location }}</span>
            </div>
            <div v-if="doctor.description" class="info-line">
              <v-icon icon="mdi-stethoscope" size="17" color="primary" />
              <span>{{ doctor.description }}</span>
            </div>
          </div>
        </div>

        <!-- Specialization -->
        <div class="info-card">
          <div class="info-card-header">
            <v-icon icon="mdi-stethoscope" color="primary" size="20" />
            <h3>التخصص</h3>
          </div>
          <p class="info-value">{{ doctor.specialization?.name ?? '-' }}</p>
        </div>

        <!-- Province -->
        <div class="info-card">
          <div class="info-card-header">
            <v-icon icon="mdi-map-marker" color="primary" size="20" />
            <h3>المحافظة</h3>
          </div>
          <p class="info-value">{{ doctor.iraqiProvinceName ?? '-' }}</p>
        </div>
      </div>

      <!-- Clinics Section -->
      <div class="section-header">
        <div>
          <p class="section-kicker">الفروع</p>
          <h2 class="section-title">عيادات الطبيب</h2>
        </div>
        <v-chip color="primary" variant="tonal">{{ clinics.length }} عيادة</v-chip>
      </div>

      <EmptyState
        v-if="!clinics.length"
        icon="mdi-hospital-building"
        title="لا توجد عيادات"
        description="لم يضف الطبيب أي عيادة إلى حسابه بعد"
      />

      <div v-else class="clinics-grid">
        <div v-for="clinic in clinics" :key="clinic.id" class="clinic-card">
          <!-- Header -->
          <div class="clinic-card-header">
            <div class="clinic-icon">
              <v-icon icon="mdi-hospital-building" color="primary" size="20" />
            </div>
            <div class="clinic-header-info">
              <h4 class="clinic-name">{{ clinic.name }}</h4>
              <v-chip
                size="x-small"
                :color="clinic.isVisible ? 'success' : 'default'"
                variant="tonal"
              >
                {{ clinic.isVisible ? 'ظاهرة للمرضى' : 'مخفية' }}
              </v-chip>
            </div>
          </div>

          <v-divider />

          <!-- Details -->
          <div class="clinic-details">
            <div class="clinic-detail-row">
              <v-icon icon="mdi-map-marker" size="15" color="primary" />
              <span><strong>{{ clinic.iraqiProvinceName }}</strong>، {{ clinic.address }}</span>
            </div>
            <div v-if="clinic.phoneNumber" class="clinic-detail-row">
              <v-icon icon="mdi-phone" size="15" color="primary" />
              <span>{{ clinic.phoneNumber }}</span>
            </div>
          </div>

          <!-- Map Link -->
          <div v-if="clinic.mapUrl" class="clinic-card-footer">
            <v-btn
              variant="text"
              size="small"
              color="info"
              prepend-icon="mdi-map"
              :href="clinic.mapUrl"
              target="_blank"
              tag="a"
            >
              فتح الخارطة
            </v-btn>
          </div>
        </div>
      </div>

    </template>

    <!-- Not Found -->
    <EmptyState
      v-else-if="!loading"
      icon="mdi-account-off"
      title="الطبيب غير موجود"
      description="لم يتم العثور على بيانات هذا الطبيب"
    >
      <template #action>
        <v-btn color="primary" @click="router.push('/doctors')">العودة للأطباء</v-btn>
      </template>
    </EmptyState>

  </div>
</template>

<style scoped>
.doctor-details-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.page-top { display: flex; }

.skeletons-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--spacing-lg);
}

/* Hero */
.doctor-hero {
  display: flex;
  align-items: center;
  gap: var(--spacing-xl);
  padding: var(--spacing-2xl);
  border-radius: var(--radius-lg);
  background: linear-gradient(125deg, var(--color-primary-dark), #299789);
  color: white;
  box-shadow: var(--shadow-md);
}

.doctor-avatar {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 96px;
  height: 96px;
  border-radius: var(--radius-lg);
  border: 3px solid rgba(255, 255, 255, 0.8);
  background: var(--color-primary-soft);
  overflow: hidden;
  flex-shrink: 0;
}

.doctor-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.doctor-hero-info { flex: 1; }

.hero-kicker {
  margin: 0 0 6px 0;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: rgba(255, 255, 255, 0.7);
}

.hero-name {
  margin: 0 0 4px 0;
  font-size: 28px;
  font-weight: 800;
  color: white;
}

.hero-sub {
  margin: 0 0 var(--spacing-lg) 0;
  font-size: 14px;
  color: rgba(255, 255, 255, 0.8);
}

.hero-badges { display: flex; gap: var(--spacing-sm); flex-wrap: wrap; }

.hero-stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-xl);
  border-radius: var(--radius-lg);
  background: rgba(255, 255, 255, 0.15);
  min-width: 120px;
  text-align: center;
  flex-shrink: 0;
}

.hero-stat strong {
  font-size: 36px;
  font-weight: 800;
  color: white;
  line-height: 1;
}

.hero-stat span {
  font-size: 12px;
  color: rgba(255, 255, 255, 0.8);
  font-weight: 600;
}

/* Info Grid */
.info-grid {
  display: grid;
  grid-template-columns: 1.5fr 1fr 1fr;
  gap: var(--spacing-lg);
}

.info-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  box-shadow: var(--shadow-sm);
}

.info-card--wide { grid-column: span 1; }

.info-card-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-lg);
  padding-bottom: var(--spacing-md);
  border-bottom: 1px solid var(--color-border);
}

.info-card-header h3 {
  margin: 0;
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text);
}

.info-lines { display: flex; flex-direction: column; gap: var(--spacing-md); }

.info-line {
  display: flex;
  align-items: flex-start;
  gap: var(--spacing-md);
  font-size: 13px;
  color: var(--color-text);
  line-height: 1.5;
}

.info-value {
  margin: 0;
  font-size: 18px;
  font-weight: 700;
  color: var(--color-text);
}

/* Section Header */
.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-lg);
}

.section-kicker {
  margin: 0 0 4px 0;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-muted);
}

.section-title {
  margin: 0;
  font-size: 20px;
  font-weight: 700;
  color: var(--color-text);
}

/* Clinics Grid */
.clinics-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--spacing-lg);
}

.clinic-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  transition: box-shadow 0.2s ease;
}

.clinic-card:hover { box-shadow: var(--shadow-md); }

.clinic-card-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-lg);
}

.clinic-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  background: var(--color-primary-soft);
  flex-shrink: 0;
}

.clinic-header-info { flex: 1; min-width: 0; }

.clinic-name {
  margin: 0 0 4px 0;
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.clinic-details {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  padding: var(--spacing-lg);
  flex: 1;
}

.clinic-detail-row {
  display: flex;
  align-items: flex-start;
  gap: var(--spacing-sm);
  font-size: 12px;
  color: var(--color-text);
  line-height: 1.5;
}

.clinic-card-footer {
  padding: var(--spacing-sm) var(--spacing-md);
  border-top: 1px solid var(--color-border);
}

/* Responsive */
@media (max-width: 1100px) {
  .info-grid { grid-template-columns: 1fr 1fr; }
  .info-card--wide { grid-column: 1 / -1; }
  .clinics-grid { grid-template-columns: repeat(2, 1fr); }
  .skeletons-grid { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 768px) {
  .doctor-hero { flex-direction: column; align-items: flex-start; }
  .hero-stat { width: 100%; }
  .info-grid { grid-template-columns: 1fr; }
  .clinics-grid { grid-template-columns: 1fr; }
  .skeletons-grid { grid-template-columns: 1fr; }
}
</style>
