<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '../../services/api'
import PageHeader from '../../components/common/Pageheader.vue'
import { getRequestById, acceptRequest, rejectRequest } from '../../services/doctorRequestService'
import { getErrorMessage } from '../../utils/errors'
import { useNotifications } from '../../composables/useNotifications'
import type { DoctorRequestDetails } from '../../types/api'

const { success: showSuccess, error: showError } = useNotifications()
const route = useRoute()
const router = useRouter()

const request = ref<DoctorRequestDetails | null>(null)
const loading = ref(true)
const processing = ref(false)

const rejectDialog = ref(false)
const rejectReason = ref('')
const imageDialog = ref(false)
const dialogImageUrl = ref('')
const dialogImageLabel = ref('')
const frontImageUrl = ref('')
const backImageUrl = ref('')
const blobUrls: string[] = []

async function loadIdentityImage(path: string): Promise<string> {
  const fileName = path.replace('/RequestIdentity/', '')
  try {
    const res = await api.get(`/Files/identity-image/${fileName}`, {
      responseType: 'blob',
    })
    const url = URL.createObjectURL(res.data)
    blobUrls.push(url)
    return url
  } catch {
    return ''
  }
}

function cleanupBlobs() {
  blobUrls.forEach(u => URL.revokeObjectURL(u))
  blobUrls.length = 0
}

onUnmounted(cleanupBlobs)

function statusLabel(s: string) {
  switch (s) {
    case 'Waiting': return 'قيد الانتظار'
    case 'Accepted': return 'مقبول'
    case 'Rejected': return 'مرفوض'
    default: return s
  }
}

function statusColor(s: string) {
  switch (s) {
    case 'Accepted': return 'success'
    case 'Waiting': return 'warning'
    case 'Rejected': return 'error'
    default: return 'default'
  }
}

async function fetchData() {
  loading.value = true
  cleanupBlobs()
  try {
    const id = Number(route.params.id)
    const res = await getRequestById(id)
    request.value = res.data!
    const r = res.data!
    frontImageUrl.value = await loadIdentityImage(r.identityFront)
    if (r.identityBack) {
      backImageUrl.value = await loadIdentityImage(r.identityBack)
    }
  } catch (e) {
    showError(getErrorMessage(e))
    router.push('/doctor-requests')
  } finally {
    loading.value = false
  }
}

async function handleAccept() {
  processing.value = true
  try {
    await acceptRequest(request.value!.id)
    showSuccess('تم قبول الطلب بنجاح')
    await fetchData()
  } catch (e) {
    showError(getErrorMessage(e))
  } finally {
    processing.value = false
  }
}

function openRejectDialog() {
  rejectReason.value = ''
  rejectDialog.value = true
}

async function handleReject() {
  if (!rejectReason.value.trim()) return
  processing.value = true
  try {
    await rejectRequest(request.value!.id, rejectReason.value.trim())
    showSuccess('تم رفض الطلب')
    rejectDialog.value = false
    await fetchData()
  } catch (e) {
    showError(getErrorMessage(e))
  } finally {
    processing.value = false
  }
}

onMounted(fetchData)
</script>

<template>
  <div class="dr-detail-page">
    <PageHeader
      :title="request ? `طلب #${request.code}` : 'تفاصيل الطلب'"
      subtitle="تفاصيل طلب التحويل إلى حساب طبيب"
    >
      <template #actions>
        <v-btn
          variant="outlined"
          color="primary"
          prepend-icon="mdi-arrow-right"
          @click="router.push('/doctor-requests')"
        >
          عودة
        </v-btn>
      </template>
    </PageHeader>

    <v-skeleton-loader v-if="loading" type="article@3" />

    <template v-if="request">
      <!-- Status Banner -->
      <v-card elevation="0" class="status-banner" :class="`status-${statusColor(request.status)}`">
        <v-icon
          :icon="request.status === 'Accepted' ? 'mdi-check-circle' : request.status === 'Rejected' ? 'mdi-close-circle' : 'mdi-clock-outline'"
          size="28"
        />
        <div class="status-info">
          <span class="status-label">{{ statusLabel(request.status) }}</span>
          <span v-if="request.rejectedReason" class="status-reason">سبب الرفض: {{ request.rejectedReason }}</span>
        </div>
      </v-card>

      <!-- Actions (only for Waiting) -->
      <div v-if="request.status === 'Waiting'" class="action-bar">
        <v-btn
          color="success"
          variant="elevated"
          size="large"
          prepend-icon="mdi-check"
          :loading="processing"
          @click="handleAccept"
        >
          قبول الطلب
        </v-btn>
        <v-btn
          color="error"
          variant="outlined"
          size="large"
          prepend-icon="mdi-close"
          :disabled="processing"
          @click="openRejectDialog"
        >
          رفض الطلب
        </v-btn>
      </div>

      <!-- Info Grid -->
      <div class="info-grid">
        <v-card elevation="0" class="info-card">
          <div class="card-header">
            <v-icon icon="mdi-account" color="primary" size="20" />
            <h3>معلومات الشخصية</h3>
          </div>
          <v-divider />
          <div class="card-body">
            <div class="info-row">
              <span class="info-label">الاسم الكامل</span>
              <span class="info-value">{{ request.fullName }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">الاسم المعروف به</span>
              <span class="info-value">{{ request.knownName }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">رقم الهاتف</span>
              <span class="info-value" dir="ltr">{{ request.phoneNumber }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">تاريخ الميلاد</span>
              <span class="info-value">{{ request.birthDay }}</span>
            </div>
          </div>
        </v-card>

        <v-card elevation="0" class="info-card">
          <div class="card-header">
            <v-icon icon="mdi-stethoscope" color="primary" size="20" />
            <h3>معلومات المهنية</h3>
          </div>
          <v-divider />
          <div class="card-body">
            <div class="info-row">
              <span class="info-label">التخصص</span>
              <span class="info-value">{{ request.specializationName }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">المحافظة</span>
              <span class="info-value">{{ request.province }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">كود المتابعة</span>
              <span class="info-value code-value" dir="ltr">{{ request.code }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">تاريخ الطلب</span>
              <span class="info-value">{{ new Date(request.createdAt).toLocaleDateString('ar-IQ', { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' }) }}</span>
            </div>
          </div>
        </v-card>
      </div>

      <!-- Identity Attachments -->
      <v-card elevation="0" class="images-card">
        <div class="card-header">
          <v-icon icon="mdi-card-account-details" color="primary" size="20" />
          <h3>المرفقات</h3>
        </div>
        <v-divider />
        <div class="attachments-row">
          <v-btn
            v-if="frontImageUrl"
            variant="outlined"
            color="primary"
            prepend-icon="mdi-file-image"
            @click="dialogImageUrl = frontImageUrl; dialogImageLabel = 'الوجه الأمامي للهوية'; imageDialog = true"
          >
            عرض الهوية (أمامي)
          </v-btn>
          <v-btn
            v-if="backImageUrl"
            variant="outlined"
            color="primary"
            prepend-icon="mdi-file-image"
            @click="dialogImageUrl = backImageUrl; dialogImageLabel = 'الوجه الخلفي للهوية'; imageDialog = true"
          >
            عرض الهوية (خلفي)
          </v-btn>
          <v-btn
            v-if="!frontImageUrl && !backImageUrl"
            disabled
            variant="outlined"
            prepend-icon="mdi-file-image"
          >
            لا توجد مرفقات
          </v-btn>
        </div>
      </v-card>

      <!-- Audit Info -->
      <v-card elevation="0" class="info-card">
        <div class="card-header">
          <v-icon icon="mdi-information" color="primary" size="20" />
          <h3>معلومات إضافية</h3>
        </div>
        <v-divider />
        <div class="card-body">
          <div class="info-row">
            <span class="info-label">الحالة</span>
            <v-chip size="small" :color="statusColor(request.status)" variant="tonal">
              {{ statusLabel(request.status) }}
            </v-chip>
          </div>
          <div v-if="request.rejectedReason" class="info-row">
            <span class="info-label">سبب الرفض</span>
            <span class="info-value info-value--error">{{ request.rejectedReason }}</span>
          </div>
          <div v-if="request.modifiedAt" class="info-row">
            <span class="info-label">آخر تحديث</span>
            <span class="info-value">{{ new Date(request.modifiedAt).toLocaleDateString('ar-IQ', { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' }) }}</span>
          </div>
        </div>
      </v-card>
    </template>

    <!-- Image Preview Dialog -->
    <v-dialog v-model="imageDialog" max-width="700">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-file-image" color="primary" size="22" />
          {{ dialogImageLabel }}
        </v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          <img v-if="dialogImageUrl" :src="dialogImageUrl" class="dialog-image" alt="identity" />
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="imageDialog = false">إغلاق</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Reject Dialog -->
    <v-dialog v-model="rejectDialog" max-width="500">
      <v-card>
        <v-card-title class="dialog-title">
          <v-icon icon="mdi-alert" color="error" size="22" />
          رفض الطلب
        </v-card-title>
        <v-divider />
        <v-card-text class="dialog-body">
          <p class="dialog-desc">سيتم إرسال سبب الرفض إلى مقدم الطلب عبر واتساب.</p>
          <v-textarea
            v-model="rejectReason"
            variant="outlined"
            label="سبب الرفض"
            placeholder="أدخل سبب الرفض..."
            :rules="[v => !!v?.trim() || 'يرجى إدخال سبب الرفض']"
            auto-grow
            rows="3"
            hide-details="auto"
          />
        </v-card-text>
        <v-divider />
        <v-card-actions class="dialog-actions">
          <v-btn variant="outlined" @click="rejectDialog = false">تراجع</v-btn>
          <v-btn
            color="error"
            :disabled="!rejectReason.trim()"
            :loading="processing"
            @click="handleReject"
          >
            تأكيد الرفض
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<style scoped>
.dr-detail-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

/* Status Banner */
.status-banner {
  display: flex;
  align-items: center;
  gap: var(--spacing-lg);
  padding: var(--spacing-lg);
  border-radius: var(--radius-lg) !important;
  border: 1px solid !important;
}

.status-success {
  background: linear-gradient(135deg, #f0fdf4, #dcfce7) !important;
  border-color: #bbf7d0 !important;
  color: #16a34a;
}

.status-warning {
  background: linear-gradient(135deg, #fffbeb, #fef3c7) !important;
  border-color: #fde68a !important;
  color: #d97706;
}

.status-error {
  background: linear-gradient(135deg, #fef2f2, #fee2e2) !important;
  border-color: #fecaca !important;
  color: #dc2626;
}

.status-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.status-label {
  font-size: 16px;
  font-weight: 700;
}

.status-reason {
  font-size: 13px;
  opacity: 0.8;
}

/* Action Bar */
.action-bar {
  display: flex;
  gap: var(--spacing-md);
}

/* Info Grid */
.info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-lg);
}

.info-card,
.images-card {
  border: 1px solid var(--color-border) !important;
  border-radius: var(--radius-lg) !important;
  overflow: hidden;
}

.card-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-lg);
  background: var(--color-surface);
}

.card-header h3 {
  margin: 0;
  font-size: 15px;
  font-weight: 700;
  color: var(--color-text);
}

.card-body {
  padding: var(--spacing-lg);
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 6px 0;
  border-bottom: 1px solid var(--color-border-light);
}

.info-row:last-child {
  border-bottom: none;
}

.info-label {
  font-size: 13px;
  color: var(--color-text-muted);
}

.info-value {
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text);
}

.info-value--error {
  color: #dc2626;
}

.code-value {
  color: var(--color-primary);
  font-family: monospace;
  font-weight: 700;
}

/* Attachments */
.attachments-row {
  display: flex;
  gap: var(--spacing-md);
  padding: var(--spacing-lg);
  flex-wrap: wrap;
}

/* Dialog Image */
.dialog-image {
  width: 100%;
  max-height: 70vh;
  object-fit: contain;
  display: block;
  border-radius: var(--radius-md);
}

/* Dialog */
.dialog-title {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-lg) !important;
  font-size: 16px !important;
  font-weight: 700;
}

.dialog-body {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
  padding: var(--spacing-lg) !important;
}

.dialog-desc {
  margin: 0;
  font-size: 14px;
  color: var(--color-text-muted);
}

.dialog-actions {
  padding: var(--spacing-lg) !important;
  gap: var(--spacing-md);
  justify-content: flex-end;
}

@media (max-width: 768px) {
  .info-grid {
    grid-template-columns: 1fr;
  }
  .action-bar {
    flex-direction: column;
  }
}
</style>
