<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import api from '../services/api'
import type { ApiResponse, AppReleaseResponse } from '../types/api'
import { getErrorMessage } from '../utils/errors'

const iosAppStoreUrl = ""

type DeviceType = 'android' | 'ios' | 'desktop'

const device = ref<DeviceType>('desktop')
const showIosMessage = ref(false)
const showScreenshots = ref(false)
const latestRelease = ref<AppReleaseResponse | null>(null)
const loading = ref(true)
const error = ref('')

const appName = "عيادتي"
const appTagline = "تطبيق إدارة العيادات الطبية"
const appDescription = "احجز مواعيدك، تابع ملفاتك الطبية، وتواصل مع أطبائك بكل سهولة وأمان"

const detected = ref(false)

const deviceLabel = computed(() => {
  switch (device.value) {
    case 'android': return 'جهاز أندرويد'
    case 'ios': return 'جهاز آيفون / آيباد'
    default: return 'جهاز كمبيوتر'
  }
})

const deviceIconComponent = computed(() => {
  switch (device.value) {
    case 'android': return 'mdi-cellphone'
    case 'ios': return 'mdi-apple'
    default: return 'mdi-monitor'
  }
})

const downloadUrl = computed(() => {
  if (!latestRelease.value) return ''
  return latestRelease.value.downloadUrl
})

function detectDevice() {
  const ua = navigator.userAgent.toLowerCase()
  if (/android/.test(ua)) { device.value = 'android' }
  else if (/iphone|ipad|ipod/.test(ua)) { device.value = 'ios' }
  else { device.value = 'desktop' }
  detected.value = true
}

function trackDownload(platform: string) {
  console.log(`[Download Tracking] Platform: ${platform}, Version: ${latestRelease.value?.versionName ?? '?'}, Timestamp: ${new Date().toISOString()}`)
}

async function fetchLatestRelease() {
  loading.value = true
  error.value = ''
  try {
    const res = await api.get<ApiResponse<AppReleaseResponse>>('/app-release/latest')
    latestRelease.value = res.data.data ?? null
  } catch (err) {
    error.value = getErrorMessage(err)
    latestRelease.value = null
  } finally {
    loading.value = false
  }
}

function handleDownload() {
  if (!downloadUrl.value) return

  if (device.value === 'android') {
    trackDownload('android')
    window.location.href = downloadUrl.value
  } else if (device.value === 'ios') {
    if (iosAppStoreUrl) {
      trackDownload('ios')
      window.location.href = iosAppStoreUrl
    } else {
      showIosMessage.value = true
    }
  }
}

function downloadAndroid() {
  if (!downloadUrl.value) return
  trackDownload('android')
  window.location.href = downloadUrl.value
}

function downloadIos() {
  if (iosAppStoreUrl) {
    trackDownload('ios')
    window.location.href = iosAppStoreUrl
  } else {
    showIosMessage.value = true
  }
}

onMounted(async () => {
  detectDevice()
  await fetchLatestRelease()
  document.title = `تحميل ${appName} - ${appTagline}`
  let metaDesc = document.querySelector<HTMLMetaElement>('meta[name="description"]')
  if (!metaDesc) {
    metaDesc = document.createElement('meta')
    metaDesc.name = 'description'
    document.head.appendChild(metaDesc)
  }
  metaDesc.content = appDescription
})
</script>

<template>
  <main class="download-page">
    <div class="dl-container">
      <!-- Loading State -->
      <div v-if="loading" class="dl-centered-stack dl-spinner-box">
        <v-icon icon="mdi-loading" size="36" class="spin-icon" />
        <span class="dl-text-muted dl-bold">جاري تحميل معلومات التطبيق...</span>
      </div>

      <!-- Error State -->
      <div v-else-if="error && !latestRelease" class="dl-centered-stack dl-spinner-box">
        <div class="dl-icon-box dl-icon-box--error">
          <v-icon icon="mdi-close" size="32" />
        </div>
        <p class="dl-text-muted dl-text-small">{{ error }}</p>
      </div>

      <!-- Main Card -->
      <div v-else class="dl-card">
        <!-- Device Badge -->
        <div v-if="detected" class="dl-badge">
          <v-icon :icon="deviceIconComponent" size="14" />
          {{ deviceLabel }}
        </div>

        <!-- Brand -->
        <div class="dl-brand">
          <div class="dl-logo-circle">
            <v-icon icon="mdi-heart-pulse" size="40" />
          </div>
          <h1 class="dl-title">{{ appName }}</h1>
          <p class="dl-tagline">{{ appTagline }}</p>
          <p class="dl-description">{{ appDescription }}</p>

          <!-- Version Info -->
          <div v-if="latestRelease" class="dl-version-badge">
            <span class="dl-version-text">الإصدار {{ latestRelease.versionName }}</span>
            <span class="dl-divider-vert"></span>
            <span class="dl-text-muted">{{ latestRelease.fileSize }}</span>
            <span class="dl-divider-vert"></span>
            <span class="dl-text-muted">{{ latestRelease.downloadCount }} تحميل</span>
          </div>
        </div>

        <!-- Main Download Button (Android / iOS) -->
        <div v-if="device !== 'desktop'" class="dl-section-center">
          <button
            class="dl-btn-primary dl-btn-lg"
            :disabled="!downloadUrl"
            @click="handleDownload"
          >
            <v-icon icon="mdi-download" size="22" />
            {{ downloadUrl ? 'تحميل التطبيق' : 'لا يوجد إصدار متاح' }}
          </button>

          <Transition name="ios-msg">
            <div v-if="showIosMessage" class="dl-msg-ios">
              <span>إصدار آيفون قريباً جداً — تابعنا للحصول على الإشعار!</span>
              <button class="dl-btn-close" @click="showIosMessage = false">
                <v-icon icon="mdi-close" size="16" />
              </button>
            </div>
          </Transition>
        </div>

        <!-- Desktop: Both Options -->
        <div v-else class="dl-section-center">
          <p class="dl-prompt">اختر النظام الذي تريد تحميل التطبيق له:</p>

          <button
            class="dl-btn-primary dl-btn-block"
            :disabled="!downloadUrl"
            @click="downloadAndroid"
          >
            <v-icon icon="mdi-cellphone" size="20" />
            {{ downloadUrl ? 'تحميل نسخة أندرويد (APK)' : 'لا يوجد إصدار متاح' }}
          </button>

          <button
            class="dl-btn-outline dl-btn-block"
            @click="downloadIos"
          >
            <v-icon icon="mdi-apple" size="20" />
            <span v-if="iosAppStoreUrl">تحميل من App Store</span>
            <span v-else class="dl-inline-group">
              إصدار آيفون — قريباً
              <span class="dl-chip-soon">قريباً</span>
            </span>
          </button>

          <Transition name="ios-msg">
            <div v-if="showIosMessage" class="dl-msg-ios">
              <span>إصدار آيفون قريباً جداً — تابعنا للحصول على الإشعار!</span>
              <button class="dl-btn-close" @click="showIosMessage = false">
                <v-icon icon="mdi-close" size="16" />
              </button>
            </div>
          </Transition>
        </div>

        <!-- Release Notes -->
        <div v-if="latestRelease?.releaseNotes.length" class="dl-section">
          <div class="dl-divider-line">
            <span class="dl-divider-label">ملاحظات الإصدار {{ latestRelease.versionName }}</span>
          </div>
          <ul class="dl-list">
            <li v-for="(note, i) in latestRelease.releaseNotes" :key="i" class="dl-list-item" :style="{ animationDelay: `${i * 0.06}s` }">
              <span class="dl-bullet"></span>
              {{ note }}
            </li>
          </ul>
        </div>

        <!-- Doctor Request CTA -->
        <div class="dl-divider-line">
          <span class="dl-divider-label">هل أنت مريض وتريد أن تصبح طبيباً؟</span>
        </div>
        <div class="dl-section-center">
          <p class="dl-prompt">يمكنك الآن تقديم طلب لتحويل حسابك إلى حساب طبيب والاستفادة من خدمات عيادتي</p>
          <RouterLink to="/doctor-request" class="dl-btn-primary dl-btn-block" style="text-decoration: none;">
            <v-icon icon="mdi-heart-pulse" size="20" />
            تقديم طلب التحويل إلى طبيب
          </RouterLink>
          <RouterLink to="/doctor-request/status" class="dl-btn-outline dl-btn-block" style="text-decoration: none;">
            متابعة طلب موجود
          </RouterLink>
        </div>

        <!-- Divider -->
        <div class="dl-divider-line">
          <span class="dl-divider-label">مميزات التطبيق</span>
        </div>

        <!-- Features Grid -->
        <div class="dl-grid-4">
          <div v-for="(feature, i) in ['حجز مواعيد', 'ملفات طبية', 'تواصل مباشر', 'تذكيرات ذكية']" :key="i"
            class="dl-feature-card"
            :style="{ animationDelay: `${0.3 + i * 0.1}s` }"
          >
            <div class="dl-feature-icon">
              <v-icon icon="mdi-tablet-cellphone" size="18" />
            </div>
            <span class="dl-feature-label">{{ feature }}</span>
          </div>
        </div>

        <!-- Screenshots Placeholder -->
        <div class="dl-section">
          <button class="dl-trigger" @click="showScreenshots = !showScreenshots">
            <span>لقطات من التطبيق</span>
            <span class="dl-arrow" :class="{ 'dl-arrow--open': showScreenshots }">▼</span>
          </button>

          <Transition name="screenshots">
            <div v-if="showScreenshots" class="dl-grid-4">
              <div v-for="i in 4" :key="i"
                class="dl-screenshot-placeholder"
                :style="{ animationDelay: `${i * 0.08}s` }"
              >
                لقطة شاشة {{ i }}
              </div>
            </div>
          </Transition>
        </div>

        <!-- Footer -->
        <div class="dl-footer">
          <p>جميع الحقوق محفوظة &copy; {{ new Date().getFullYear() }} — {{ appName }}</p>
        </div>
      </div>
    </div>
  </main>
</template>

<style scoped>
/* ── Layout ── */
.download-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #f6f9f8 0%, #eef8f5 50%, #e4f4f0 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
}

@media (min-width: 640px) {
  .download-page { padding: 24px; }
}

.dl-container {
  width: 100%;
  max-width: 672px;
}

.dl-centered-stack {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.dl-spinner-box {
  padding: 80px 0;
  text-align: center;
}

.dl-bold { font-weight: 700; }
.dl-text-muted { color: var(--color-text-muted); }
.dl-text-small { font-size: 14px; }
.dl-inline-group { display: inline-flex; align-items: center; gap: 8px; }

/* ── Card ── */
.dl-card {
  position: relative;
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border: 1px solid var(--color-border);
  border-radius: 24px;
  box-shadow: var(--shadow-md);
  padding: 24px;
  animation: fadeInUp 0.5s ease both;
}

@media (min-width: 640px) {
  .dl-card { padding: 40px; }
}

/* ── Device Badge ── */
.dl-badge {
  position: absolute;
  top: -12px;
  left: 50%;
  transform: translateX(-50%);
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 16px;
  background: var(--color-primary);
  color: #ffffff;
  font-size: 12px;
  font-weight: 700;
  border-radius: 999px;
  box-shadow: var(--shadow-md);
  white-space: nowrap;
}

/* ── Brand ── */
.dl-brand {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding-top: 8px;
}

.dl-logo-circle {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 80px;
  height: 80px;
  border-radius: 16px;
  background: var(--color-primary-soft);
  color: var(--color-primary);
  margin-bottom: 20px;
  box-shadow: var(--shadow-sm);
}

.dl-title {
  margin: 0 0 8px;
  font-size: clamp(30px, 5vw, 36px);
  font-weight: 800;
  color: var(--color-text);
}

.dl-tagline {
  margin: 0 0 4px;
  font-size: 16px;
  font-weight: 500;
  color: var(--color-text-muted);
}

@media (min-width: 640px) {
  .dl-tagline { font-size: 18px; }
}

.dl-description {
  margin: 0;
  font-size: 14px;
  color: var(--color-text-muted);
  max-width: 448px;
  line-height: 1.625;
}

/* ── Version Badge ── */
.dl-version-badge {
  display: inline-flex;
  align-items: center;
  gap: 12px;
  margin-top: 16px;
  padding: 8px 16px;
  border-radius: 999px;
  background: var(--color-primary-soft);
  border: 1px solid rgba(19, 121, 107, 0.2);
}

.dl-version-text {
  font-size: 12px;
  font-weight: 700;
  color: var(--color-primary);
}

.dl-divider-vert {
  width: 1px;
  height: 12px;
  background: rgba(19, 121, 107, 0.2);
}

/* ── Primary Button ── */
.dl-btn-primary {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  border: none;
  background: var(--color-primary);
  color: #ffffff;
  font-family: var(--font-family-primary);
  font-weight: 700;
  border-radius: 16px;
  box-shadow: 0 8px 24px rgba(19, 121, 107, 0.2);
  cursor: pointer;
  transition: background 0.15s ease, transform 0.15s ease, box-shadow 0.15s ease;
  user-select: none;
}

.dl-btn-primary:hover {
  background: var(--color-primary-dark);
  color: #ffffff;
  transform: scale(1.02);
  box-shadow: 0 12px 32px rgba(19, 121, 107, 0.3);
}

.dl-btn-primary:active {
  transform: scale(0.98);
}

.dl-btn-primary:focus-visible {
  outline: 3px solid rgba(19, 121, 107, 0.3);
  outline-offset: 2px;
}

.dl-btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  pointer-events: none;
}

.dl-btn-primary:disabled:hover {
  transform: none;
}

.dl-btn-primary svg {
  color: #ffffff !important;
  flex-shrink: 0;
}

.dl-btn-lg {
  width: 100%;
  padding: 16px 40px;
  font-size: 18px;
}

@media (min-width: 640px) {
  .dl-btn-lg { width: auto; }
}

.dl-btn-block {
  width: 100%;
  padding: 16px 24px;
  font-size: 16px;
}

/* ── Outline Button ── */
.dl-btn-outline {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 16px 24px;
  background: #ffffff;
  color: var(--color-text);
  font-family: var(--font-family-primary);
  font-weight: 700;
  font-size: 16px;
  border: 2px solid var(--color-border);
  border-radius: 16px;
  cursor: pointer;
  transition: background 0.15s ease, border-color 0.15s ease, transform 0.15s ease;
  user-select: none;
}

.dl-btn-outline:hover {
  background: var(--color-background);
  border-color: var(--color-primary);
  transform: scale(1.01);
}

.dl-btn-outline:active {
  transform: scale(0.99);
}

.dl-btn-outline:focus-visible {
  outline: 3px solid rgba(19, 121, 107, 0.3);
  outline-offset: 2px;
}

.dl-btn-outline svg {
  flex-shrink: 0;
}

.dl-btn-outline:hover svg {
  color: var(--color-primary);
}

.dl-chip-soon {
  padding: 2px 8px;
  font-size: 11px;
  background: var(--color-warning-light);
  color: var(--color-warning);
  border-radius: 999px;
  font-weight: 700;
}

/* ── Sections ── */
.dl-section-center {
  margin-top: 32px;
  text-align: center;
  animation: fadeInUp 0.5s ease 0.2s both;
}

.dl-section-center > * + * {
  margin-top: 12px;
}

.dl-section {
  margin-top: 32px;
}

.dl-prompt {
  text-align: center;
  font-size: 14px;
  color: var(--color-text-muted);
  margin: 0 0 16px;
}

.dl-msg-ios {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  margin-top: 16px;
  padding: 12px;
  background: var(--color-warning-light);
  border: 1px solid #fde68a;
  color: #92400e;
  font-size: 14px;
  border-radius: 12px;
  animation: fadeInUp 0.3s ease both;
}

.dl-btn-close {
  flex-shrink: 0;
  padding: 4px;
  border: none;
  background: transparent;
  color: inherit;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.15s ease;
}

.dl-btn-close:hover {
  background: rgba(251, 191, 36, 0.3);
}

/* ── Divider ── */
.dl-divider-line {
  display: flex;
  align-items: center;
  gap: 12px;
  margin: 24px 0;
}

.dl-divider-line::before,
.dl-divider-line::after {
  content: '';
  flex: 1;
  height: 1px;
  background: var(--color-border);
}

.dl-divider-label {
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-muted);
  white-space: nowrap;
}

/* ── Release Notes List ── */
.dl-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.dl-list-item {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  font-size: 14px;
  color: var(--color-text);
  line-height: 1.625;
  animation: fadeInUp 0.3s ease both;
}

.dl-bullet {
  margin-top: 6px;
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: var(--color-primary);
  flex-shrink: 0;
}

/* ── Features Grid ── */
.dl-grid-4 {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
}

@media (min-width: 640px) {
  .dl-grid-4 { grid-template-columns: repeat(4, 1fr); }
}

.dl-feature-card {
  text-align: center;
  padding: 12px;
  border-radius: 12px;
  background: rgba(228, 244, 240, 0.5);
  border: 1px solid rgba(226, 235, 233, 0.5);
  transition: background 0.3s ease, border-color 0.3s ease, transform 0.3s ease;
}

.dl-feature-card:hover {
  background: var(--color-primary-soft);
  border-color: rgba(19, 121, 107, 0.2);
  transform: translateY(-2px);
}

.dl-feature-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  margin: 0 auto 8px;
  border-radius: 12px;
  background: var(--color-primary-soft);
  color: var(--color-primary);
}

.dl-feature-label {
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text);
}

/* ── Screenshot Toggle ── */
.dl-trigger {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px 20px;
  border: 1px solid var(--color-border);
  border-radius: 12px;
  background: #ffffff;
  cursor: pointer;
  font-family: var(--font-family-primary);
  transition: border-color 0.3s ease, background 0.3s ease;
}

.dl-trigger:hover {
  border-color: var(--color-primary);
  background: rgba(228, 244, 240, 0.3);
}

.dl-trigger:focus-visible {
  outline: 3px solid rgba(19, 121, 107, 0.3);
  outline-offset: 2px;
}

.dl-trigger span:first-child {
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text);
}

.dl-arrow {
  font-size: 12px;
  color: var(--color-text-muted);
  transition: transform 0.3s ease;
}

.dl-arrow--open {
  transform: rotate(180deg);
}

.dl-screenshot-placeholder {
  aspect-ratio: 9/16;
  border-radius: 16px;
  background: linear-gradient(135deg, var(--color-primary-soft), #f3f4f6);
  border: 1px solid var(--color-border);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-muted);
  animation: fadeInUp 0.4s ease both;
}

/* ── Footer ── */
.dl-footer {
  margin-top: 32px;
  text-align: center;
}

.dl-footer p {
  margin: 0;
  font-size: 12px;
  color: var(--color-text-muted);
}

/* ── Helpers ── */
.spin-icon {
  animation: spin 1s linear infinite;
  margin-bottom: 12px;
}

.dl-icon-box {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 64px;
  height: 64px;
  border-radius: 16px;
  margin-bottom: 16px;
}

.dl-icon-box--error {
  background: #fef2f2;
  color: #ef4444;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(12px); }
  to { opacity: 1; transform: translateY(0); }
}

/* ── Transitions ── */
.ios-msg-enter-active,
.ios-msg-leave-active {
  transition: all 0.25s ease;
}
.ios-msg-enter-from,
.ios-msg-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}
.screenshots-enter-active,
.screenshots-leave-active {
  transition: all 0.3s ease;
}
.screenshots-enter-from,
.screenshots-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}
</style>
