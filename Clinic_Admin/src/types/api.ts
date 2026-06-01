export interface ApiResponse<T> {
  status: string
  code: number
  message: string
  data: T
}

export interface PageResult<T> {
  items: T[]
  totalItems: number
  totalPages: number
  currentPage: number
  pageSize: number
}

export interface UserItem {
  id: string
  name?: string
  phoneNumber?: string
  userName?: string
  imageName?: string
  isLocked: boolean
  lastLoginDate?: string
  roleName?: string
}

export interface SpecializationItem {
  id: number
  name: string
  normalizedName: string
}

export interface DoctorItem {
  id: number
  name: string
  normalizedName: string
  specialization: SpecializationItem
  description: string
  subscriptionRank: number
  iraqiProvince: number
  iraqiProvinceName: string
  iraqiProvinceNormalizedName: string
  birthDay: string
  imageName: string
  phoneNumber: string
  location: string
  isPubliclyVisible: boolean
  userId?: string
}

export interface SubscriptionPackage {
  id: number
  name: string
  normalizedName: string
  price: number
  yearlyPrice: number
  maxClinics: number
  maxDailyAppointments: number
  maxWeeklyDays: number
  showReviews: boolean
  showMessages: boolean
  eBooking: boolean
  ePayments: boolean
  makeOffers: boolean
  maxActiveOffers: number
}

export interface DoctorSubscription {
  id: number
  doctor: DoctorItem
  package: SubscriptionPackage
  startDate: string
  endDate: string
  isActive: boolean
  status: number
  cancelledAt?: string
}

export interface FeatureItem {
  id: number
  name: string
  normalizedName: string
  description?: string
  isPremiumOnly: boolean
}

export interface DoctorFeature {
  id: number
  doctor: DoctorItem
  feature: FeatureItem
  isEnabled: boolean
}

export interface ClinicItem {
  id: number
  doctorId: number
  name: string
  iraqiProvince: number
  iraqiProvinceName: string
  address: string
  latitude?: number
  longitude?: number
  mapUrl?: string
  phoneNumber?: string
  isVisible: boolean
}

export interface DayItem {
  id: number
  name: string
  normalizedName: string
}

export interface ClinicAvailability {
  id: number
  clinicId: number
  dayId: number
  dayName: string
  dayNormailzedName: string
  isAvailable: boolean
  startTime?: string
  endTime?: string
  maxAppointments?: number
}
