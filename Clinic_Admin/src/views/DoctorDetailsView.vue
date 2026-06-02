<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { ArrowRight, Building2, ExternalLink, Eye, EyeOff, MapPin, Phone, Stethoscope, UserRound } from '@lucide/vue'
import api from '../services/api'
import { useNotificationsStore } from '../stores/notifications'
import type { ApiResponse, ClinicItem, DoctorItem, PageResult } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const route = useRoute()
const notifications = useNotificationsStore()
const doctor = ref<DoctorItem>()
const clinics = ref<ClinicItem[]>([])
const loading = ref(false)
const doctorId = Number(route.params.doctorId)
const apiOrigin = computed(() => new URL(api.defaults.baseURL ?? 'https://localhost:7136/api').origin)
const doctorImage = computed(() => doctor.value?.imageName ? `${apiOrigin.value}/DoctorImage/${doctor.value.imageName}` : '')

async function loadDetails() {
  loading.value = true
  try {
    const [doctorResponse, clinicResponse] = await Promise.all([
      api.get<ApiResponse<PageResult<DoctorItem>>>('/Doctor', { params: { id: doctorId, page: 1, pageSize: 1 } }),
      api.get<ApiResponse<ClinicItem[]>>(`/Clinic/doctor/${doctorId}/admin`),
    ])
    doctor.value = doctorResponse.data.data.items[0]
    clinics.value = clinicResponse.data.data
    if (!doctor.value) notifications.show('لم يتم العثور على ملف الطبيب.', 'error')
  } catch (error) {
    notifications.show(getErrorMessage(error), 'error')
  } finally {
    loading.value = false
  }
}

onMounted(loadDetails)
</script>

<template>
  <div>
    <div class="page-heading">
      <div><span class="section-kicker">دليل الأطباء</span><h2>تفاصيل الطبيب</h2><p>راجع بيانات الطبيب والعيادات التابعة له وحالة ظهورها للمرضى.</p></div>
      <RouterLink class="secondary-button details-back" to="/doctors"><ArrowRight :size="17" /> العودة للأطباء</RouterLink>
    </div>

    <div v-if="loading" class="empty-panel">جارِ تحميل تفاصيل الطبيب...</div>
    <template v-else-if="doctor">
      <section class="details-hero">
        <div class="details-photo">
          <img v-if="doctorImage" :src="doctorImage" :alt="doctor.name" />
          <UserRound v-else :size="48" />
        </div>
        <div class="details-copy">
          <span class="section-kicker">ملف طبيب رقم #{{ doctor.id }}</span>
          <h2>{{ doctor.name }}</h2>
          <p>{{ doctor.specialization.name }} · {{ doctor.iraqiProvinceName }}</p>
          <div class="details-badges">
            <span class="status-badge" :class="doctor.isPubliclyVisible ? 'status-success' : 'status-warning'"><Eye v-if="doctor.isPubliclyVisible" :size="14" /><EyeOff v-else :size="14" /> {{ doctor.isPubliclyVisible ? 'ظاهر للعامة' : 'مخفي عن العامة' }}</span>
            <span class="status-badge" :class="doctor.userId ? 'status-success' : 'status-neutral'">{{ doctor.userId ? 'حساب الدخول مربوط' : 'بدون حساب دخول' }}</span>
          </div>
        </div>
      </section>

      <section class="details-grid">
        <article class="panel-card">
          <span class="section-kicker">معلومات الاتصال</span>
          <div class="details-lines">
            <span><Phone :size="17" /> {{ doctor.phoneNumber }}</span>
            <span><MapPin :size="17" /> {{ doctor.location }}</span>
            <span><Stethoscope :size="17" /> {{ doctor.description }}</span>
          </div>
        </article>
        <article class="panel-card details-stat">
          <Building2 :size="24" />
          <strong>{{ clinics.length }}</strong>
          <span>عيادة مسجلة للطبيب</span>
        </article>
      </section>

      <div class="details-section-title"><div><span class="section-kicker">الفروع</span><h3>عيادات الطبيب</h3></div></div>
      <div v-if="!clinics.length" class="empty-panel"><Building2 :size="30" /><h3>لا توجد عيادات</h3><p>لم يضف الطبيب أي عيادة إلى حسابه بعد.</p></div>
      <section v-else class="clinic-grid">
        <article v-for="clinic in clinics" :key="clinic.id" class="clinic-card">
          <div class="clinic-card-header"><span class="clinic-icon"><Building2 :size="21" /></span><div><h3>{{ clinic.name }}</h3><span class="status-badge" :class="clinic.isVisible ? 'status-success' : 'status-neutral'">{{ clinic.isVisible ? 'ظاهرة للمرضى' : 'مخفية' }}</span></div></div>
          <div class="clinic-details">
            <span><MapPin :size="16" /><b>{{ clinic.iraqiProvinceName }}</b>، {{ clinic.address }}</span>
            <span v-if="clinic.phoneNumber"><Phone :size="16" />{{ clinic.phoneNumber }}</span>
          </div>
          <div v-if="clinic.mapUrl" class="clinic-card-actions"><a class="clinic-link" :href="clinic.mapUrl" target="_blank" rel="noopener"><ExternalLink :size="15" /> فتح الخارطة</a></div>
        </article>
      </section>
    </template>
  </div>
</template>

<style scoped>
.details-back { text-decoration: none; }.details-hero { display: flex; align-items: center; gap: 16px; padding: 20px; color: #fff; border-radius: 16px; background: linear-gradient(125deg, var(--primary-dark), #299789); box-shadow: var(--shadow); }
.details-photo { display: grid; place-items: center; width: 88px; height: 88px; overflow: hidden; color: var(--primary); border: 4px solid rgba(255,255,255,.8); border-radius: 24px; background: var(--primary-soft); }.details-photo img { width: 100%; height: 100%; object-fit: cover; }
.details-copy h2 { margin: 5px 0 4px; font-size: 25px; }.details-copy p { margin: 0; color: #d4f0ec; }.details-copy .section-kicker { color: #ccf3ee; }.details-badges { display: flex; flex-wrap: wrap; gap: 7px; margin-top: 13px; }.details-badges span { display: flex; align-items: center; gap: 4px; }
.details-grid { display: grid; grid-template-columns: 1.5fr .5fr; gap: 14px; margin-top: 15px; }.details-lines { display: grid; gap: 10px; margin-top: 14px; color: #58706c; line-height: 1.7; }.details-lines span { display: flex; gap: 7px; }.details-lines svg { flex: 0 0 auto; color: var(--primary); }
.details-stat { display: grid; place-items: center; color: var(--primary); text-align: center; }.details-stat strong { margin: 8px 0 3px; color: var(--ink); font-size: 32px; }.details-stat span { color: var(--muted); font-size: 12px; }
.details-section-title { margin: 21px 0 11px; }.details-section-title h3 { margin: 5px 0 0; font-size: 20px; }
@media (max-width: 760px) { .details-grid { grid-template-columns: 1fr; }.details-hero { align-items: flex-start; }.details-photo { width: 70px; height: 70px; } }
</style>
