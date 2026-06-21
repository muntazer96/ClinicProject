<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { Apple, Download, HeartPulse, LoaderCircle, Monitor, Smartphone, TabletSmartphone, X } from '@lucide/vue'
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
    case 'android': return Smartphone
    case 'ios': return Apple
    default: return Monitor
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
  <main class="min-h-screen bg-gradient-to-br from-[#f6f9f8] via-[#eef8f5] to-[#e4f4f0] flex items-center justify-center p-4 sm:p-6">
    <div class="w-full max-w-2xl">
      <!-- Loading State -->
      <div v-if="loading" class="flex flex-col items-center justify-center py-20 text-[var(--muted)] animate-[fadeInUp_0.4s_ease_both]">
        <LoaderCircle :size="36" class="spin-icon mb-3" />
        <span class="text-sm font-bold">جاري تحميل معلومات التطبيق...</span>
      </div>

      <!-- Error State -->
      <div v-else-if="error && !latestRelease" class="flex flex-col items-center justify-center py-20 text-center animate-[fadeInUp_0.4s_ease_both]">
        <div class="flex items-center justify-center w-16 h-16 rounded-2xl bg-red-50 text-red-500 mb-4">
          <X :size="32" />
        </div>
        <p class="text-sm text-[var(--muted)] max-w-xs">{{ error }}</p>
      </div>

      <!-- Main Card -->
      <div v-else class="relative bg-white/80 backdrop-blur-xl border border-[var(--line)] rounded-3xl shadow-[var(--shadow)] p-6 sm:p-10 animate-[fadeInUp_0.5s_ease_both]">
        <!-- Device Badge -->
        <div v-if="detected" class="absolute -top-3 left-1/2 -translate-x-1/2 inline-flex items-center gap-1.5 px-4 py-1.5 bg-[var(--primary)] text-white text-xs font-bold rounded-full shadow-lg whitespace-nowrap">
          <component :is="deviceIconComponent" :size="14" />
          {{ deviceLabel }}
        </div>

        <!-- Brand -->
        <div class="flex flex-col items-center text-center pt-2">
          <div class="flex items-center justify-center w-20 h-20 rounded-2xl bg-[var(--primary-soft)] text-[var(--primary)] mb-5 shadow-sm">
            <HeartPulse :size="40" />
          </div>

          <h1 class="text-3xl sm:text-4xl font-extrabold text-[var(--ink)] mb-2">
            {{ appName }}
          </h1>
          <p class="text-[var(--muted)] text-base sm:text-lg font-medium mb-1">
            {{ appTagline }}
          </p>
          <p class="text-[var(--muted)] text-sm max-w-md leading-relaxed">
            {{ appDescription }}
          </p>

          <!-- Version Info -->
          <div v-if="latestRelease" class="mt-4 inline-flex items-center gap-3 px-4 py-2 rounded-full bg-[var(--primary-soft)] border border-[var(--primary)]/20">
            <span class="text-xs font-bold text-[var(--primary)]">الإصدار {{ latestRelease.versionName }}</span>
            <span class="w-px h-3 bg-[var(--primary)]/20"></span>
            <span class="text-xs text-[var(--muted)]">{{ latestRelease.fileSize }}</span>
            <span class="w-px h-3 bg-[var(--primary)]/20"></span>
            <span class="text-xs text-[var(--muted)]">{{ latestRelease.downloadCount }} تحميل</span>
          </div>
        </div>

        <!-- Main Download Button (Android / iOS) -->
        <div v-if="device !== 'desktop'" class="mt-8 text-center animate-[fadeInUp_0.5s_ease_0.2s_both]">
          <button
            class="w-full sm:w-auto inline-flex items-center justify-center gap-3 px-10 py-4 bg-[var(--primary)] hover:bg-[var(--primary-dark)] text-white text-lg font-bold rounded-2xl shadow-lg shadow-[var(--primary)]/20 transition-all duration-300 hover:scale-[1.02] hover:shadow-xl active:scale-[0.98] cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
            :disabled="!downloadUrl"
            @click="handleDownload"
          >
            <Download :size="22" />
            {{ downloadUrl ? 'تحميل التطبيق' : 'لا يوجد إصدار متاح' }}
          </button>

          <Transition name="ios-msg">
            <div v-if="showIosMessage" class="mt-4 flex items-center justify-center gap-2 p-3 bg-amber-50 border border-amber-200 text-amber-800 text-sm rounded-xl animate-[fadeInUp_0.3s_ease_both]">
              <span>إصدار آيفون قريباً جداً — تابعنا للحصول على الإشعار!</span>
              <button class="flex-shrink-0 p-1 rounded-lg hover:bg-amber-100 transition-colors cursor-pointer" @click="showIosMessage = false">
                <X :size="16" />
              </button>
            </div>
          </Transition>
        </div>

        <!-- Desktop: Both Options -->
        <div v-else class="mt-8 space-y-3 animate-[fadeInUp_0.5s_ease_0.2s_both]">
          <p class="text-center text-sm text-[var(--muted)] mb-4">
            اختر النظام الذي تريد تحميل التطبيق له:
          </p>

          <button
            class="w-full flex items-center justify-center gap-3 px-6 py-4 bg-[var(--primary)] hover:bg-[var(--primary-dark)] text-white font-bold rounded-2xl shadow-lg shadow-[var(--primary)]/20 transition-all duration-300 hover:scale-[1.01] hover:shadow-xl active:scale-[0.99] cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
            :disabled="!downloadUrl"
            @click="downloadAndroid"
          >
            <Smartphone :size="20" />
            {{ downloadUrl ? 'تحميل نسخة أندرويد (APK)' : 'لا يوجد إصدار متاح' }}
          </button>

          <button
            class="w-full flex items-center justify-center gap-3 px-6 py-4 bg-white hover:bg-gray-50 text-[var(--ink)] font-bold border-2 border-[var(--line)] rounded-2xl transition-all duration-300 hover:border-[var(--primary)]/30 hover:scale-[1.01] active:scale-[0.99] cursor-pointer"
            @click="downloadIos"
          >
            <Apple :size="20" />
            <span v-if="iosAppStoreUrl">تحميل من App Store</span>
            <span v-else class="flex items-center gap-2">
              إصدار آيفون — قريباً
              <span class="px-2 py-0.5 text-[11px] bg-amber-100 text-amber-700 rounded-full font-bold">قريباً</span>
            </span>
          </button>

          <Transition name="ios-msg">
            <div v-if="showIosMessage" class="flex items-center justify-center gap-2 p-3 bg-amber-50 border border-amber-200 text-amber-800 text-sm rounded-xl animate-[fadeInUp_0.3s_ease_both]">
              <span>إصدار آيفون قريباً جداً — تابعنا للحصول على الإشعار!</span>
              <button class="flex-shrink-0 p-1 rounded-lg hover:bg-amber-100 transition-colors cursor-pointer" @click="showIosMessage = false">
                <X :size="16" />
              </button>
            </div>
          </Transition>
        </div>

        <!-- Release Notes -->
        <div v-if="latestRelease?.releaseNotes.length" class="mt-8">
          <div class="flex items-center gap-3 my-6">
            <span class="flex-1 h-px bg-[var(--line)]"></span>
            <span class="text-xs text-[var(--muted)] font-bold">ملاحظات الإصدار {{ latestRelease.versionName }}</span>
            <span class="flex-1 h-px bg-[var(--line)]"></span>
          </div>
          <ul class="space-y-2">
            <li v-for="(note, i) in latestRelease.releaseNotes" :key="i"
              class="flex items-start gap-2.5 text-sm text-[var(--ink)] leading-relaxed animate-[fadeInUp_0.3s_ease_both]"
              :style="{ animationDelay: `${i * 0.06}s` }"
            >
              <span class="mt-1.5 w-1.5 h-1.5 rounded-full bg-[var(--primary)] flex-shrink-0"></span>
              {{ note }}
            </li>
          </ul>
        </div>

        <!-- Divider -->
        <div class="flex items-center gap-3 my-8">
          <span class="flex-1 h-px bg-[var(--line)]"></span>
          <span class="text-xs text-[var(--muted)] font-bold">مميزات التطبيق</span>
          <span class="flex-1 h-px bg-[var(--line)]"></span>
        </div>

        <!-- Features Grid -->
        <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
          <div v-for="(feature, i) in ['حجز مواعيد', 'ملفات طبية', 'تواصل مباشر', 'تذكيرات ذكية']" :key="i"
            class="text-center p-3 rounded-xl bg-[var(--primary-soft)]/50 border border-[var(--line)]/50 transition-all duration-300 hover:bg-[var(--primary-soft)] hover:border-[var(--primary)]/20 hover:-translate-y-0.5"
            :style="{ animationDelay: `${0.3 + i * 0.1}s` }"
          >
            <div class="flex items-center justify-center w-10 h-10 mx-auto mb-2 rounded-xl bg-[var(--primary-soft)] text-[var(--primary)]">
              <TabletSmartphone :size="18" />
            </div>
            <span class="text-xs font-bold text-[var(--ink)]">{{ feature }}</span>
          </div>
        </div>

        <!-- Screenshots Placeholder -->
        <div class="mt-8">
          <button
            class="w-full flex items-center justify-between gap-3 px-5 py-3 rounded-xl bg-white border border-[var(--line)] transition-all duration-300 hover:border-[var(--primary)]/30 hover:bg-[var(--primary-soft)]/30 cursor-pointer"
            @click="showScreenshots = !showScreenshots"
          >
            <span class="text-sm font-bold text-[var(--ink)]">لقطات من التطبيق</span>
            <span class="text-[var(--muted)] text-xs transition-transform duration-300" :class="showScreenshots ? 'rotate-180' : ''">▼</span>
          </button>

          <Transition name="screenshots">
            <div v-if="showScreenshots" class="mt-4 grid grid-cols-2 sm:grid-cols-4 gap-3">
              <div v-for="i in 4" :key="i"
                class="aspect-[9/16] rounded-2xl bg-gradient-to-br from-[var(--primary-soft)] to-gray-100 border border-[var(--line)] flex items-center justify-center text-[var(--muted)] text-xs font-bold animate-[fadeInUp_0.4s_ease_both]"
                :style="{ animationDelay: `${i * 0.08}s` }"
              >
                لقطة شاشة {{ i }}
              </div>
            </div>
          </Transition>
        </div>

        <!-- Footer -->
        <div class="mt-8 text-center">
          <p class="text-xs text-[var(--muted)]">
            جميع الحقوق محفوظة &copy; {{ new Date().getFullYear() }} — {{ appName }}
          </p>
        </div>
      </div>
    </div>
  </main>
</template>

<style scoped>
.spin-icon {
  animation: spin 1s linear infinite;
}
@keyframes spin {
  to { transform: rotate(360deg); }
}
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
