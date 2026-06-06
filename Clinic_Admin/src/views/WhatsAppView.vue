<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { CheckCircle2, Copy, MessageCircle, PlugZap, QrCode, RefreshCw, Send, ShieldCheck } from '@lucide/vue'
import { useNotificationsStore } from '../stores/notifications'

type BridgeStatus = {
  ready?: boolean
  authenticated?: boolean
  phone?: string
  qrDataUrl?: string
  message?: string
}

const storageKey = 'clinic_whatsapp_bridge_settings'
const notifications = useNotificationsStore()
const loading = ref(false)
const sending = ref(false)
const endpoint = ref(defaultBridgeEndpoint())
const token = ref('clinic-whatsapp-secret')
const phone = ref('')
const message = ref('رمز التحقق الخاص بحجزك هو: 123456')
const status = ref<BridgeStatus | null>(null)
const lastError = ref('')

const normalizedEndpoint = computed(() => endpoint.value.trim().replace(/\/+$/, ''))
const isReady = computed(() => status.value?.ready === true || status.value?.authenticated === true)
const qrImage = computed(() => status.value?.qrDataUrl ?? '')
const appsettingsSnippet = computed(() => JSON.stringify({
  WhatsAppMessages: {
    Enabled: true,
    Provider: 'CustomHttp',
    Endpoint: `${normalizedEndpoint.value}/send-message`,
    Token: token.value,
    PhoneFieldName: 'phone',
    MessageFieldName: 'message',
  },
}, null, 2))

function defaultBridgeEndpoint() {
  const protocol = window.location.protocol || 'http:'
  const host = window.location.hostname || '127.0.0.1'
  return `${protocol}//${host}:3001`
}

function headers(): Record<string, string> {
  const result: Record<string, string> = { 'Content-Type': 'application/json' }
  if (token.value.trim()) {
    result.Authorization = `Bearer ${token.value.trim()}`
  }
  return result
}

function saveSettings() {
  localStorage.setItem(storageKey, JSON.stringify({ endpoint: endpoint.value, token: token.value }))
  notifications.show('تم حفظ إعدادات الواتساب')
}

function loadSettings() {
  const raw = localStorage.getItem(storageKey)
  if (!raw) return
  try {
    const saved = JSON.parse(raw) as { endpoint?: string; token?: string }
    endpoint.value = saved.endpoint || endpoint.value
    token.value = saved.token || token.value
  } catch {
    localStorage.removeItem(storageKey)
  }
}

async function checkStatus() {
  loading.value = true
  lastError.value = ''
  try {
    const response = await fetch(`${normalizedEndpoint.value}/status`, { headers: headers() })
    if (!response.ok) throw new Error(`HTTP ${response.status}`)
    status.value = await response.json()
  } catch (error) {
    status.value = null
    lastError.value = error instanceof Error ? error.message : 'Connection failed'
    notifications.show('تعذر الاتصال بخدمة الواتساب', 'error')
  } finally {
    loading.value = false
  }
}

async function sendTest() {
  sending.value = true
  lastError.value = ''
  try {
    const response = await fetch(`${normalizedEndpoint.value}/send-message`, {
      method: 'POST',
      headers: headers(),
      body: JSON.stringify({ phone: phone.value, message: message.value }),
    })
    if (!response.ok) throw new Error(`HTTP ${response.status}`)
    notifications.show('تم إرسال رسالة الاختبار')
    await checkStatus()
  } catch (error) {
    lastError.value = error instanceof Error ? error.message : 'Send failed'
    notifications.show('فشل إرسال رسالة الاختبار', 'error')
  } finally {
    sending.value = false
  }
}

async function copySnippet() {
  await navigator.clipboard.writeText(appsettingsSnippet.value)
  notifications.show('تم نسخ إعدادات الباك')
}

onMounted(() => {
  loadSettings()
  checkStatus()
})
</script>

<template>
  <div class="whatsapp-page">
    <section class="whatsapp-head">
      <div>
        <span class="section-kicker">Super Admin</span>
        <h2>ربط WhatsApp OTP</h2>
      </div>
      <div class="status-chip" :class="{ live: isReady }">
        <CheckCircle2 v-if="isReady" :size="18" />
        <PlugZap v-else :size="18" />
        <span>{{ isReady ? 'Connected' : 'Waiting QR' }}</span>
      </div>
    </section>

    <section class="whatsapp-grid">
      <article class="panel-card settings-card">
        <header><ShieldCheck :size="18" /><h3>إعدادات الخدمة</h3></header>
        <form class="whatsapp-form" @submit.prevent="checkStatus">
          <label>
            رابط الخدمة
            <input v-model="endpoint" dir="ltr" placeholder="http://127.0.0.1:3001">
          </label>
          <label>
            Token
            <input v-model="token" dir="ltr" placeholder="secret-token">
          </label>
          <div class="form-actions">
            <button class="secondary-button" type="button" @click="saveSettings">حفظ</button>
            <button class="compact-primary" type="submit" :disabled="loading">
              <RefreshCw :size="16" :class="{ spin: loading }" />
              فحص
            </button>
          </div>
        </form>
      </article>

      <article class="panel-card qr-card">
        <header><QrCode :size="18" /><h3>QR</h3></header>
        <div v-if="qrImage && !isReady" class="qr-box">
          <img :src="qrImage" alt="WhatsApp QR">
        </div>
        <div v-else class="qr-placeholder">
          <MessageCircle :size="42" />
          <strong>{{ isReady ? 'الحساب مربوط' : 'بانتظار QR' }}</strong>
          <span>{{ status?.phone || status?.message || 'شغل خدمة WhatsApp_Bridge ثم اضغط فحص' }}</span>
        </div>
      </article>
    </section>

    <section class="whatsapp-grid">
      <article class="panel-card">
        <header><Send :size="18" /><h3>رسالة اختبار</h3></header>
        <form class="whatsapp-form" @submit.prevent="sendTest">
          <label>
            رقم الهاتف
            <input v-model="phone" dir="ltr" placeholder="07701234567">
          </label>
          <label>
            الرسالة
            <textarea v-model="message" rows="4"></textarea>
          </label>
          <button class="compact-primary" type="submit" :disabled="sending || !phone.trim()">
            <Send :size="16" />
            إرسال
          </button>
        </form>
      </article>

      <article class="panel-card">
        <header><Copy :size="18" /><h3>إعدادات الباك</h3></header>
        <pre dir="ltr">{{ appsettingsSnippet }}</pre>
        <button class="secondary-button copy-button" type="button" @click="copySnippet">
          <Copy :size="16" />
          نسخ
        </button>
      </article>
    </section>

    <section v-if="lastError" class="whatsapp-error">
      {{ lastError }}
    </section>
  </div>
</template>

<style scoped>
.whatsapp-page { display: flex; flex-direction: column; gap: 15px; }
.whatsapp-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 20px;
  border: 1px solid var(--line);
  border-radius: 8px;
  background: #fff;
  box-shadow: var(--shadow);
}
.whatsapp-head h2 { margin: 6px 0 0; color: var(--ink); font-size: 25px; }
.status-chip {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 9px 12px;
  border-radius: 999px;
  color: #9a6400;
  background: #fff4d8;
  font-weight: 900;
}
.status-chip.live { color: var(--primary); background: var(--primary-soft); }
.whatsapp-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 14px; align-items: start; }
.panel-card header { display: flex; align-items: center; gap: 8px; color: var(--primary); }
.panel-card header h3 { margin: 0; }
.whatsapp-form { display: grid; gap: 13px; margin-top: 14px; }
.whatsapp-form label { display: grid; gap: 7px; color: var(--ink); font-size: 13px; font-weight: 900; }
.whatsapp-form input, .whatsapp-form textarea {
  width: 100%;
  padding: 11px;
  border: 1px solid var(--line);
  border-radius: 8px;
  background: #fff;
  color: var(--ink);
  outline: 0;
}
.form-actions { display: flex; flex-wrap: wrap; gap: 8px; justify-content: flex-end; }
.qr-card { min-height: 320px; }
.qr-box { display: grid; place-items: center; padding: 18px; border: 1px dashed #91c7bf; border-radius: 8px; background: #f7fcfb; }
.qr-box img { width: min(100%, 260px); aspect-ratio: 1; object-fit: contain; }
.qr-placeholder {
  min-height: 230px;
  display: grid;
  place-items: center;
  align-content: center;
  gap: 9px;
  color: var(--muted);
  border: 1px dashed var(--line);
  border-radius: 8px;
  background: #fbfdfc;
  text-align: center;
}
.qr-placeholder strong { color: var(--ink); }
pre {
  overflow: auto;
  margin: 14px 0;
  padding: 14px;
  border-radius: 8px;
  background: #102a27;
  color: #e6fffa;
  font-size: 12px;
  line-height: 1.7;
}
.copy-button { margin-right: auto; }
.whatsapp-error {
  padding: 11px 13px;
  border: 1px solid #ffd6d6;
  border-radius: 8px;
  color: #a33c3c;
  background: #fff0f0;
  font-weight: 800;
}
.spin { animation: spin .8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 860px) {
  .whatsapp-head { align-items: stretch; flex-direction: column; }
  .whatsapp-grid { grid-template-columns: 1fr; }
}
</style>
