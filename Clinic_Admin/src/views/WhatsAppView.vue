<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useNotifications } from '../composables/useNotifications'

type BridgeStatus = {
  ready?: boolean
  authenticated?: boolean
  phone?: string
  qrDataUrl?: string
  message?: string
}

const storageKey = 'clinic_whatsapp_bridge_settings'
const { success: showSuccess, error: showError } = useNotifications()

const loading  = ref(false)
const sending  = ref(false)
const endpoint = ref(defaultBridgeEndpoint())
const token    = ref('clinic-whatsapp-secret')
const phone    = ref('')
const message  = ref('رمز التحقق الخاص بحجزك هو: 123456')
const status   = ref<BridgeStatus | null>(null)
const lastError = ref('')

const normalizedEndpoint = computed(() => endpoint.value.trim().replace(/\/+$/, ''))
const isReady  = computed(() => status.value?.ready === true || status.value?.authenticated === true)
const qrImage  = computed(() => status.value?.qrDataUrl ?? '')

const appsettingsSnippet = computed(() =>
  JSON.stringify({
    WhatsAppMessages: {
      Enabled: true,
      Provider: 'CustomHttp',
      Endpoint: `${normalizedEndpoint.value}/send-message`,
      Token: token.value,
      PhoneFieldName: 'phone',
      MessageFieldName: 'message',
    },
  }, null, 2)
)

function defaultBridgeEndpoint() {
  const protocol = window.location.protocol || 'http:'
  const host = window.location.hostname || '127.0.0.1'
  return `${protocol}//${host}/whatsapp-bridge`
}

function headers(): Record<string, string> {
  const result: Record<string, string> = { 'Content-Type': 'application/json' }
  if (token.value.trim()) result.Authorization = `Bearer ${token.value.trim()}`
  return result
}

function saveSettings() {
  localStorage.setItem(storageKey, JSON.stringify({ endpoint: endpoint.value, token: token.value }))
  showSuccess('تم حفظ إعدادات الواتساب')
}

function loadSettings() {
  const raw = localStorage.getItem(storageKey)
  if (!raw) return
  try {
    const saved = JSON.parse(raw) as { endpoint?: string; token?: string }
    endpoint.value = saved.endpoint || endpoint.value
    token.value    = saved.token    || token.value
  } catch { localStorage.removeItem(storageKey) }
}

async function checkStatus() {
  loading.value = true
  lastError.value = ''
  try {
    const r = await fetch(`${normalizedEndpoint.value}/status`, { headers: headers() })
    if (!r.ok) throw new Error(`HTTP ${r.status}`)
    status.value = await r.json()
  } catch (e) {
    status.value = null
    lastError.value = e instanceof Error ? e.message : 'Connection failed'
    showError('تعذر الاتصال بخدمة الواتساب')
  } finally { loading.value = false }
}

async function logoutWhatsapp() {
  loading.value = true
  lastError.value = ''
  try {
    const r = await fetch(`${normalizedEndpoint.value}/logout`, { method: 'POST', headers: headers() })
    if (!r.ok) throw new Error(`HTTP ${r.status}`)
    status.value = null
    showSuccess('تم تسجيل الخروج من واتساب')
    await checkStatus()
  } catch (e) {
    lastError.value = e instanceof Error ? e.message : 'Logout failed'
    showError('فشل تسجيل الخروج من واتساب')
  } finally { loading.value = false }
}

async function sendTest() {
  sending.value = true
  lastError.value = ''
  try {
    const r = await fetch(`${normalizedEndpoint.value}/send-message`, {
      method: 'POST',
      headers: headers(),
      body: JSON.stringify({ phone: phone.value, message: message.value }),
    })
    if (!r.ok) throw new Error(`HTTP ${r.status}`)
    showSuccess('تم إرسال رسالة الاختبار')
    await checkStatus()
  } catch (e) {
    lastError.value = e instanceof Error ? e.message : 'Send failed'
    showError('فشل إرسال رسالة الاختبار')
  } finally { sending.value = false }
}

async function copySnippet() {
  await navigator.clipboard.writeText(appsettingsSnippet.value)
  showSuccess('تم نسخ إعدادات الباك')
}

onMounted(() => { loadSettings(); checkStatus() })
</script>

<template>
  <div class="whatsapp-page">

    <!-- Header -->
    <div class="page-top">
      <div>
        <p class="page-kicker">Super Admin</p>
        <h1 class="page-title">ربط WhatsApp OTP</h1>
      </div>
      <div class="page-actions">
        <!-- Status Badge -->
        <div class="status-badge" :class="{ 'status-badge--live': isReady }">
          <v-icon :icon="isReady ? 'mdi-check-circle' : 'mdi-lightning-bolt'" size="18" />
          <span>{{ isReady ? 'Connected' : 'Waiting QR' }}</span>
        </div>
        <!-- Logout Button -->
        <v-btn
          v-if="isReady"
          color="error"
          variant="outlined"
          prepend-icon="mdi-logout"
          :loading="loading"
          @click="logoutWhatsapp"
        >
          تسجيل خروج
        </v-btn>
      </div>
    </div>

    <!-- Main Grid -->
    <div class="wa-grid">

      <!-- Settings Card -->
      <div class="wa-card">
        <div class="wa-card-header">
          <v-icon icon="mdi-shield-check" color="primary" size="20" />
          <h3>إعدادات الخدمة</h3>
        </div>
        <div class="wa-form">
          <div class="form-field">
            <label class="form-label">رابط الخدمة</label>
            <input v-model="endpoint" class="form-input ltr" placeholder="http://127.0.0.1:3001" dir="ltr" />
          </div>
          <div class="form-field">
            <label class="form-label">Token</label>
            <input v-model="token" class="form-input ltr" placeholder="secret-token" dir="ltr" />
          </div>
          <div class="form-actions">
            <v-btn variant="outlined" color="primary" @click="saveSettings">حفظ</v-btn>
            <v-btn color="primary" prepend-icon="mdi-refresh" :loading="loading" @click="checkStatus">فحص</v-btn>
          </div>
        </div>
      </div>

      <!-- QR Card -->
      <div class="wa-card wa-card--qr">
        <div class="wa-card-header">
          <v-icon icon="mdi-qrcode" color="primary" size="20" />
          <h3>QR Code</h3>
        </div>

        <!-- QR Image -->
        <div v-if="qrImage && !isReady" class="qr-box">
          <img :src="qrImage" alt="WhatsApp QR" />
        </div>

        <!-- Connected State -->
        <div v-else-if="isReady" class="qr-connected">
          <v-icon icon="mdi-whatsapp" color="success" size="48" />
          <strong>الحساب مربوط</strong>
          <span>{{ status?.phone || 'WhatsApp متصل بنجاح' }}</span>
        </div>

        <!-- Waiting State -->
        <div v-else class="qr-waiting">
          <v-icon icon="mdi-message-processing" color="var(--color-text-muted)" size="48" />
          <strong>بانتظار QR</strong>
          <span>{{ status?.message || 'شغّل خدمة WhatsApp_Bridge ثم اضغط فحص' }}</span>
        </div>
      </div>

    </div>

    <!-- Second Grid -->
    <div class="wa-grid">

      <!-- Test Message Card -->
      <div class="wa-card">
        <div class="wa-card-header">
          <v-icon icon="mdi-send" color="primary" size="20" />
          <h3>رسالة اختبار</h3>
        </div>
        <div class="wa-form">
          <div class="form-field">
            <label class="form-label">رقم الهاتف</label>
            <input v-model="phone" class="form-input ltr" placeholder="07701234567" dir="ltr" />
          </div>
          <div class="form-field">
            <label class="form-label">الرسالة</label>
            <textarea v-model="message" class="form-textarea" rows="4" />
          </div>
          <v-btn
            color="primary"
            prepend-icon="mdi-send"
            :loading="sending"
            :disabled="!phone.trim()"
            @click="sendTest"
          >
            إرسال
          </v-btn>
        </div>
      </div>

      <!-- Backend Config Card -->
      <div class="wa-card">
        <div class="wa-card-header">
          <v-icon icon="mdi-code-json" color="primary" size="20" />
          <h3>إعدادات الباك</h3>
        </div>
        <pre class="code-block" dir="ltr">{{ appsettingsSnippet }}</pre>
        <v-btn variant="outlined" color="primary" prepend-icon="mdi-content-copy" @click="copySnippet">
          نسخ
        </v-btn>
      </div>

    </div>

    <!-- Error Banner -->
    <v-alert v-if="lastError" type="error" variant="tonal" icon="mdi-alert-circle">
      {{ lastError }}
    </v-alert>

  </div>
</template>

<style scoped>
.whatsapp-page { display: flex; flex-direction: column; gap: var(--spacing-lg); }

/* Page Top */
.page-top { display: flex; align-items: center; justify-content: space-between; gap: var(--spacing-lg); flex-wrap: wrap; }
.page-kicker { margin: 0 0 4px 0; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--color-text-muted); }
.page-title { margin: 0; font-size: 28px; font-weight: 800; color: var(--color-text); }
.page-actions { display: flex; align-items: center; gap: var(--spacing-md); flex-wrap: wrap; }

/* Status Badge */
.status-badge {
  display: inline-flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: 10px 16px;
  border-radius: var(--radius-lg);
  background: var(--color-warning-light);
  color: var(--color-warning);
  font-weight: 700;
  font-size: 14px;
}
.status-badge--live {
  background: var(--color-success-light);
  color: var(--color-success);
}

/* Grid */
.wa-grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--spacing-lg); align-items: start; }

/* Card */
.wa-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  box-shadow: var(--shadow-sm);
}
.wa-card--qr { min-height: 320px; display: flex; flex-direction: column; }

.wa-card-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-lg);
  padding-bottom: var(--spacing-md);
  border-bottom: 1px solid var(--color-border);
}
.wa-card-header h3 { margin: 0; font-size: 15px; font-weight: 700; color: var(--color-text); }

/* Form */
.wa-form { display: flex; flex-direction: column; gap: var(--spacing-lg); }
.form-field { display: flex; flex-direction: column; gap: 6px; }
.form-label { font-size: 13px; font-weight: 600; color: var(--color-text); }
.form-input, .form-textarea {
  padding: 10px 12px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-size: 14px;
  outline: none;
  width: 100%;
  transition: border-color 0.2s;
}
.form-input:focus, .form-textarea:focus { border-color: var(--color-primary); }
.form-textarea { resize: vertical; }
.ltr { direction: ltr; }
.form-actions { display: flex; gap: var(--spacing-md); justify-content: flex-end; }

/* QR States */
.qr-box { display: flex; align-items: center; justify-content: center; padding: var(--spacing-xl); border: 2px dashed var(--color-primary-light); border-radius: var(--radius-lg); background: var(--color-primary-soft); flex: 1; }
.qr-box img { width: min(100%, 260px); aspect-ratio: 1; object-fit: contain; }

.qr-connected, .qr-waiting {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: var(--spacing-md); flex: 1;
  padding: var(--spacing-2xl);
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-background);
  text-align: center;
}
.qr-connected { border-color: var(--color-success); background: var(--color-success-light); }
.qr-connected strong { font-size: 18px; font-weight: 700; color: var(--color-success); }
.qr-connected span, .qr-waiting span { font-size: 13px; color: var(--color-text-muted); line-height: 1.5; }
.qr-waiting strong { font-size: 16px; font-weight: 700; color: var(--color-text); }

/* Code Block */
.code-block {
  overflow: auto;
  margin: 0 0 var(--spacing-lg) 0;
  padding: var(--spacing-lg);
  border-radius: var(--radius-md);
  background: #0d2420;
  color: #9de8d8;
  font-size: 12px;
  line-height: 1.7;
  direction: ltr;
  text-align: left;
}

/* Responsive */
@media (max-width: 768px) {
  .wa-grid { grid-template-columns: 1fr; }
  .page-top { flex-direction: column; align-items: flex-start; }
}
</style>