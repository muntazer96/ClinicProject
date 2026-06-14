import axios from 'axios'

export const ACCESS_TOKEN_KEY = 'clinic_admin_token'
export const REFRESH_TOKEN_KEY = 'clinic_admin_refresh_token'

const api = axios.create({
    //baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://192.168.100.7:8082/api',

    baseURL: import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7136/api',

    //baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:81/api',


  timeout: 15000,
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem(ACCESS_TOKEN_KEY)
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as any
    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY)
      if (refreshToken) {
        originalRequest._retry = true
        try {
          const response = await axios.post(`${api.defaults.baseURL}/User/refresh`, { refreshToken })
          const data = response.data?.data
          if (data?.token && data?.refreshToken) {
            localStorage.setItem(ACCESS_TOKEN_KEY, data.token)
            localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken)
            window.dispatchEvent(new CustomEvent('clinic-auth-refreshed', { detail: data }))
            originalRequest.headers.Authorization = `Bearer ${data.token}`
            return api(originalRequest)
          }
        } catch {
          localStorage.removeItem(ACCESS_TOKEN_KEY)
          localStorage.removeItem(REFRESH_TOKEN_KEY)
        }
      }
    }

    if (error.response?.status === 401) {
      localStorage.removeItem(ACCESS_TOKEN_KEY)
      localStorage.removeItem(REFRESH_TOKEN_KEY)
      if (window.location.pathname !== '/login') window.location.assign('/login')
    }
    return Promise.reject(error)
  },
)

export default api
