import api from './api'
import type {
  ApiResponse,
  CheckPhoneResponse,
  VerifyOtpResponse,
  DoctorRequestResponse,
  DoctorRequestPagination,
  DoctorRequestDetails,
} from '../types/api'

export async function checkPhone(phoneNumber: string, captchaToken: string) {
  const res = await api.post<ApiResponse<CheckPhoneResponse>>('/DoctorRequest/check-phone', {
    captchaToken,
    phoneNumber,
  })
  return res.data
}

export async function sendOtp(userId: string, phoneNumber: string) {
  const res = await api.post<ApiResponse<null>>('/DoctorRequest/send-otp', {
    userId,
    phoneNumber,
  })
  return res.data
}

export async function verifyOtp(userId: string, phoneNumber: string, otpCode: string) {
  const res = await api.post<ApiResponse<VerifyOtpResponse>>('/DoctorRequest/verify-otp', {
    userId,
    phoneNumber,
    otpCode,
  })
  return res.data
}

export async function submitDoctorRequest(formData: FormData) {
  const res = await api.post<ApiResponse<DoctorRequestResponse>>('/DoctorRequest/submit', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return res.data
}

export async function getRequestStatus(code: string, phoneNumber: string) {
  const res = await api.get<ApiResponse<DoctorRequestDetails>>(`/DoctorRequest/by-code/${code}`, { params: { phoneNumber } })
  return res.data
}

export async function getRequests(params: {
  page?: number
  pageSize?: number
  status?: string
  search?: string
}) {
  const res = await api.get<ApiResponse<DoctorRequestPagination>>('/DoctorRequest', { params })
  return res.data
}

export async function getRequestById(id: number) {
  const res = await api.get<ApiResponse<DoctorRequestDetails>>(`/DoctorRequest/${id}`)
  return res.data
}

export async function acceptRequest(id: number) {
  const res = await api.post<ApiResponse<string>>(`/DoctorRequest/${id}/accept`)
  return res.data
}

export async function rejectRequest(id: number, rejectedReason: string) {
  const res = await api.post<ApiResponse<string>>(`/DoctorRequest/${id}/reject`, { rejectedReason })
  return res.data
}
