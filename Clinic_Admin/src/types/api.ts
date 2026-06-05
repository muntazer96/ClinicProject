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
  phoneNumberConfirmed: boolean
  email?: string
  emailConfirmed: boolean
  userName?: string
  imageName?: string
  isLocked: boolean
  isFirstLogin: boolean
  lastLoginDate?: string
  roleId?: string
  roleName?: string
  linkedDoctor?: {
    id: number
    name: string
    normalizedName: string
    specializationName: string
  }
}

export interface SpecializationItem {
  id: number
  name: string
  normalizedName: string
  iconName: string
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
  linkedUser?: {
    id: string
    name?: string
    userName?: string
    phoneNumber?: string
    email?: string
  }
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

export interface DoctorOfferItem {
  id: number
  doctorId: number
  doctorName: string
  clinicId?: number
  clinicName?: string
  appliesToAllClinics: boolean
  title: string
  description?: string
  offerType: number
  offerTypeName: string
  originalPrice?: number
  offerPrice?: number
  discountPercent?: number
  badgeText?: string
  terms?: string
  startsAt: string
  endsAt: string
  durationDays: number
  remainingDays: number
  isActive: boolean
  isCurrentlyVisible: boolean
}

export interface DoctorOfferQuota {
  doctorId: number
  canMakeOffers: boolean
  maxActiveOffers: number
  activeOffers: number
  remainingOffers: number
  packageName?: string
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

export interface AppointmentItem {
  id: number
  code: string
  patientName: string
  patientPhoneNumber?: string
  appointmentDate: string
  queueNumber: number
  status: number
  isPhoneConfirmed: boolean
  cancellationReason?: string
  clinicId: number
  clinicName: string
}

export interface QueueAvailabilityItem {
  clinicId: number
  date: string
  dayName: string
  dayNormalizedName: string
  startTime?: string
  endTime?: string
  maxAppointments: number
  bookedAppointments: number
  remainingAppointments: number
  isAvailable: boolean
  hasException: boolean
  closureReason?: string
}

export interface ClinicExceptionItem {
  id: number
  clinicId: number
  exceptionDate: string
  isClosed: boolean
  closureReason?: string
  maxAppointments?: number
  startTime?: string
  endTime?: string
}

export interface ReviewItem {
  id: number
  user: {
    id: string
    name: string
    normalizedName: string
  }
  rating: number
  comment: string
  appoinmentId?: number
}

export interface DoctorReviews {
  doctorId: number
  isEnabled: boolean
  averageRating?: number
  reviewCount: number
  reviews: ReviewItem[]
}

export interface PublicDoctorListItem {
  id: number
  name: string
  normalizedName: string
  specializationId: number
  specializationName: string
  specializationNormalizedName: string
  specializationIconName: string
  description: string
  imageName: string
  canBookOnline: boolean
  averageRating?: number
  reviewCount: number
  clinics: Array<{
    id: number
    name: string
    iraqiProvince: number
    iraqiProvinceName: string
    address: string
  }>
}

export interface PublicDoctorProfile extends PublicDoctorListItem {
  clinics: PublicClinic[]
}

export interface PublicClinic {
  id: number
  name: string
  iraqiProvince: number
  iraqiProvinceName: string
  address: string
  latitude?: number
  longitude?: number
  mapUrl?: string
  phoneNumber?: string
  availabilities: Array<{
    dayId: number
    dayName: string
    dayNormalizedName: string
    startTime: string
    endTime: string
    maxAppointments: number
  }>
}

export interface BookingDetails {
  id: number
  code: string
  patientName: string
  patientPhoneNumber?: string
  appointmentDate: string
  queueNumber: number
  status: number
  isPhoneConfirmed: boolean
  hasReview: boolean
  cancellationReason?: string
  cancelledAt?: string
  doctorId: number
  doctorName: string
  clinicId: number
  clinicName: string
  clinicAddress: string
  clinicPhoneNumber?: string
  mapUrl?: string
  latitude?: number
  longitude?: number
}

export interface AppVersionPolicy {
  id: number
  platform: string
  latestVersion: string
  latestBuildNumber: number
  minimumSupportedVersion: string
  minimumSupportedBuildNumber: number
  forceUpdate: boolean
  isEnabled: boolean
  title: string
  message: string
  updateUrl?: string
}

export interface AppVersionCheck {
  platform: string
  currentVersion: string
  currentBuildNumber: number
  latestVersion: string
  latestBuildNumber: number
  minimumSupportedVersion: string
  minimumSupportedBuildNumber: number
  updateAvailable: boolean
  updateRequired: boolean
  forceUpdate: boolean
  title: string
  message: string
  updateUrl?: string
}
