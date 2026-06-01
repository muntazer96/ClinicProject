import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import api from '../services/api'

const TOKEN_KEY = 'clinic_admin_token'

interface LoginResponse {
  data?: {
    token?: string
  }
}

interface JwtPayload {
  exp?: number
  role?: string | string[]
  Role?: string | string[]
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[]
}

function decodePayload(token: string): JwtPayload | null {
  try {
    const [, payload] = token.split('.')
    if (!payload) return null
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/')
    return JSON.parse(decodeURIComponent(escape(atob(normalized)))) as JwtPayload
  } catch {
    return null
  }
}

function asArray(value?: string | string[]) {
  if (!value) return []
  return Array.isArray(value) ? value : [value]
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem(TOKEN_KEY) ?? '')
  const loading = ref(false)
  const error = ref('')

  const payload = computed(() => decodePayload(token.value))
  const roles = computed(() => {
    const claims = payload.value
    return Array.from(new Set([
      ...asArray(claims?.role),
      ...asArray(claims?.Role),
      ...asArray(claims?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']),
    ]))
  })
  const isAuthenticated = computed(() => {
    if (!token.value || !payload.value) return false
    return !payload.value.exp || payload.value.exp * 1000 > Date.now()
  })
  const primaryRole = computed(() => roles.value[0] ?? '')

  function hasAnyRole(allowedRoles: string[]) {
    return allowedRoles.length === 0 || allowedRoles.some((role) => roles.value.includes(role))
  }

  async function login(phoneNumber: string, password: string) {
    loading.value = true
    error.value = ''
    try {
      const response = await api.post<LoginResponse>('/User/signin', { phoneNumber, password })
      const newToken = response.data.data?.token
      if (!newToken) throw new Error('لم يرجع الخادم رمز دخول صالحاً.')
      token.value = newToken
      localStorage.setItem(TOKEN_KEY, newToken)
    } catch (requestError: any) {
      error.value = requestError.response?.data?.message ?? requestError.message ?? 'تعذر تسجيل الدخول.'
      throw requestError
    } finally {
      loading.value = false
    }
  }

  function logout() {
    token.value = ''
    localStorage.removeItem(TOKEN_KEY)
  }

  return { token, roles, primaryRole, isAuthenticated, loading, error, login, logout, hasAnyRole }
})
