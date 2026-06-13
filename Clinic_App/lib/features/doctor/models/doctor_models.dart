import '../../../core/api_client.dart';

class DoctorItem {
  const DoctorItem({required this.id, required this.name});

  final int id;
  final String name;

  factory DoctorItem.fromJson(Map<String, dynamic>? json) => DoctorItem(
    id: json?['id'] as int? ?? 0,
    name: json?['name'] as String? ?? '',
  );
}

class ProvinceItem {
  const ProvinceItem({required this.id, required this.name});

  final int id;
  final String name;

  factory ProvinceItem.fromJson(Map<String, dynamic> json) => ProvinceItem(
    id: json['id'] as int? ?? 0,
    name: json['name'] as String? ?? '',
  );
}

class DoctorManageProfile {
  const DoctorManageProfile({
    required this.id,
    required this.name,
    required this.normalizedName,
    required this.specialization,
    required this.specializationId,
    required this.description,
    required this.iraqiProvince,
    required this.iraqiProvinceName,
    required this.imageName,
    required this.birthDay,
    required this.phoneNumber,
    required this.location,
    required this.isPubliclyVisible,
    required this.subscriptionRank,
  });

  final int id;
  final String name;
  final String normalizedName;
  final String specialization;
  final int specializationId;
  final String description;
  final int iraqiProvince;
  final String iraqiProvinceName;
  final String imageName;
  final DateTime? birthDay;
  final String phoneNumber;
  final String location;
  final bool isPubliclyVisible;
  final int subscriptionRank;

  String? get imageUrl =>
      imageName.trim().isEmpty ? null : ApiClient.doctorImageUrl(imageName);

  factory DoctorManageProfile.fromJson(Map<String, dynamic> json) {
    final specialization = json['specialization'] as Map<String, dynamic>?;
    return DoctorManageProfile(
      id: json['id'] as int? ?? 0,
      name: json['name'] as String? ?? '',
      normalizedName: json['normalizedName'] as String? ?? '',
      specialization: specialization?['name'] as String? ?? '',
      specializationId: specialization?['id'] as int? ?? 0,
      description: json['description'] as String? ?? '',
      iraqiProvince: json['iraqiProvince'] as int? ?? 0,
      iraqiProvinceName: json['iraqiProvinceName'] as String? ?? '',
      imageName: json['imageName'] as String? ?? '',
      birthDay: json['birthDay'] == null
          ? null
          : DateTime.tryParse(json['birthDay'] as String),
      phoneNumber: json['phoneNumber'] as String? ?? '',
      location: json['location'] as String? ?? '',
      isPubliclyVisible: json['isPubliclyVisible'] as bool? ?? false,
      subscriptionRank: json['subscriptionRank'] as int? ?? 0,
    );
  }
}

class DoctorClinic {
  const DoctorClinic({
    required this.id,
    required this.doctorId,
    required this.name,
    required this.iraqiProvince,
    required this.iraqiProvinceName,
    required this.address,
    required this.phoneNumber,
    required this.consultationPrice,
    required this.showConsultationPrice,
    required this.bookingWindowDays,
    required this.isVisible,
    this.mapUrl,
    this.latitude,
    this.longitude,
  });

  final int id;
  final int doctorId;
  final String name;
  final int iraqiProvince;
  final String iraqiProvinceName;
  final String address;
  final String? phoneNumber;
  final double? consultationPrice;
  final bool showConsultationPrice;
  final int bookingWindowDays;
  final bool isVisible;
  final String? mapUrl;
  final double? latitude;
  final double? longitude;

  factory DoctorClinic.fromJson(Map<String, dynamic> json) => DoctorClinic(
    id: json['id'] as int? ?? 0,
    doctorId: json['doctorId'] as int? ?? 0,
    name: json['name'] as String? ?? '',
    iraqiProvince: json['iraqiProvince'] as int? ?? 0,
    iraqiProvinceName: json['iraqiProvinceName'] as String? ?? '',
    address: json['address'] as String? ?? '',
    phoneNumber: json['phoneNumber'] as String?,
    consultationPrice: (json['consultationPrice'] as num?)?.toDouble(),
    showConsultationPrice: json['showConsultationPrice'] as bool? ?? false,
    bookingWindowDays: json['bookingWindowDays'] as int? ?? 7,
    isVisible: json['isVisible'] as bool? ?? true,
    mapUrl: json['mapUrl'] as String?,
    latitude: (json['latitude'] as num?)?.toDouble(),
    longitude: (json['longitude'] as num?)?.toDouble(),
  );

  Map<String, dynamic> toUpdateJson({double? price}) => {
    'id': id,
    'name': name,
    'iraqiProvince': iraqiProvince,
    'address': address,
    'latitude': latitude,
    'longitude': longitude,
    'mapUrl': mapUrl,
    'phoneNumber': phoneNumber,
    'consultationPrice': price ?? consultationPrice,
    'showConsultationPrice': showConsultationPrice,
    'bookingWindowDays': bookingWindowDays,
    'isVisible': isVisible,
  };

  Map<String, dynamic> toFormJson() => {
    'name': name,
    'iraqiProvince': iraqiProvince,
    'address': address,
    'latitude': latitude,
    'longitude': longitude,
    'mapUrl': mapUrl,
    'phoneNumber': phoneNumber,
    'consultationPrice': consultationPrice,
    'showConsultationPrice': showConsultationPrice,
    'bookingWindowDays': bookingWindowDays,
    'isVisible': isVisible,
  };
}

class DoctorAppointment {
  const DoctorAppointment({
    required this.id,
    required this.code,
    required this.patientName,
    required this.patientPhone,
    required this.appointmentDate,
    required this.queueNumber,
    required this.status,
    required this.doctorName,
    required this.clinicName,
    required this.clinicAddress,
    required this.isGuestBooking,
    required this.isPhoneConfirmed,
    required this.patientPhoneNumber,
required this.hasReview,
this.cancelledAt,
  });

  final int id;
  final String code;
  final String patientName;
  final String patientPhone;
  final DateTime appointmentDate;
  final int queueNumber;
  final int status;
  final String doctorName;
  final String clinicName;
  final String clinicAddress;
  final bool isGuestBooking;
  final bool isPhoneConfirmed;
  final String patientPhoneNumber;
final bool hasReview;
final DateTime? cancelledAt;

  bool get canToggle => status == 0 || status == 1;
  bool get canComplete => status == 1;

  String get statusLabel => switch (status) {
    0 => 'قيد الانتظار',
    1 => 'مؤكد',
    2 => 'ملغي',
    3 => 'مكتمل',
    _ => 'غير معروف',
  };

  factory DoctorAppointment.fromJson(Map<String, dynamic> json) {
    final clinic = json['clinic'] as Map<String, dynamic>?;
    final user = json['user'] as Map<String, dynamic>?;
    final doctor = json['doctor'] as Map<String, dynamic>?;
    return DoctorAppointment(
      id: json['id'] as int? ?? 0,
      code: json['code'] as String? ?? '',
      patientName:
          json['patientName'] as String? ??
          json['guestName'] as String? ??
          user?['name'] as String? ??
          'زائر',
      patientPhone: json['guestPhoneNumber'] as String? ?? '',
      appointmentDate: DateTime.parse(json['appointmentDate'] as String),
      queueNumber: json['queueNumber'] as int? ?? 0,
      status: json['status'] as int? ?? 0,
      doctorName: doctor?['name'] as String? ?? '',
      clinicName: clinic?['name'] as String? ?? json['clinicName'] as String? ?? '',
      clinicAddress:
          clinic?['address'] as String? ?? json['clinicAddress'] as String? ?? '',
      isGuestBooking: json['isGuestBooking'] as bool? ?? false,
      isPhoneConfirmed: json['isPhoneConfirmed'] as bool? ?? false,
      patientPhoneNumber: json['patientPhoneNumber'] ?? '',
hasReview: json['hasReview'] ?? false,
cancelledAt: json['cancelledAt'] == null
    ? null
    : DateTime.parse(json['cancelledAt']),
    );
  }
}

class DoctorAvailability {
  const DoctorAvailability({
    required this.id,
    required this.clinicId,
    required this.dayId,
    required this.dayName,
    required this.isAvailable,
    this.startTime,
    this.endTime,
    this.maxAppointments,
  });

  final int id;
  final int clinicId;
  final int dayId;
  final String dayName;
  final bool isAvailable;
  final String? startTime;
  final String? endTime;
  final int? maxAppointments;

  factory DoctorAvailability.fromJson(Map<String, dynamic> json) =>
      DoctorAvailability(
        id: json['id'] as int? ?? 0,
        clinicId: json['clinicId'] as int? ?? 0,
        dayId: json['dayId'] as int? ?? 0,
        dayName: _dayName(json['dayNormailzedName'] as String? ?? json['dayName'] as String?),
        isAvailable: json['isAvailable'] as bool? ?? false,
        startTime: json['startTime'] as String?,
        endTime: json['endTime'] as String?,
        maxAppointments: json['maxAppointments'] as int?,
      );
}

class ClinicExceptionDay {
  const ClinicExceptionDay({
    required this.id,
    required this.clinicId,
    required this.exceptionDate,
    required this.isClosed,
    this.closureReason,
    this.maxAppointments,
    this.startTime,
    this.endTime,
  });

  final int id;
  final int clinicId;
  final DateTime exceptionDate;
  final bool isClosed;
  final String? closureReason;
  final int? maxAppointments;
  final String? startTime;
  final String? endTime;

  factory ClinicExceptionDay.fromJson(Map<String, dynamic> json) =>
      ClinicExceptionDay(
        id: json['id'] as int? ?? 0,
        clinicId: json['clinicId'] as int? ?? 0,
        exceptionDate: DateTime.parse(json['exceptionDate'] as String),
        isClosed: json['isClosed'] as bool? ?? false,
        closureReason: json['closureReason'] as String?,
        maxAppointments: json['maxAppointments'] as int?,
        startTime: json['startTime'] as String?,
        endTime: json['endTime'] as String?,
      );
}

class DoctorOfferManage {
  const DoctorOfferManage({
    required this.id,
    required this.title,
    required this.description,
    required this.clinicName,
    required this.offerTypeName,
    required this.originalPrice,
    required this.offerPrice,
    required this.discountPercent,
    required this.startsAt,
    required this.endsAt,
    required this.remainingDays,
    required this.isActive,
    required this.isCurrentlyVisible,
  });

  final int id;
  final String title;
  final String? description;
  final String? clinicName;
  final String offerTypeName;
  final double? originalPrice;
  final double? offerPrice;
  final double? discountPercent;
  final DateTime startsAt;
  final DateTime endsAt;
  final int remainingDays;
  final bool isActive;
  final bool isCurrentlyVisible;

  factory DoctorOfferManage.fromJson(Map<String, dynamic> json) =>
      DoctorOfferManage(
        id: json['id'] as int? ?? 0,
        title: json['title'] as String? ?? '',
        description: json['description'] as String?,
        clinicName: json['clinicName'] as String?,
        offerTypeName: json['offerTypeName'] as String? ?? '',
        originalPrice: (json['originalPrice'] as num?)?.toDouble(),
        offerPrice: (json['offerPrice'] as num?)?.toDouble(),
        discountPercent: (json['discountPercent'] as num?)?.toDouble(),
        startsAt: DateTime.parse(json['startsAt'] as String),
        endsAt: DateTime.parse(json['endsAt'] as String),
        remainingDays: json['remainingDays'] as int? ?? 0,
        isActive: json['isActive'] as bool? ?? false,
        isCurrentlyVisible: json['isCurrentlyVisible'] as bool? ?? false,
      );
}

class DoctorNotificationItem {
  const DoctorNotificationItem({
    required this.id,
    required this.message,
    required this.createdAt,
    required this.status,
  });

  final int id;
  final String message;
  final DateTime createdAt;
  final int status;

  bool get isRead => status == 1;

  factory DoctorNotificationItem.fromJson(Map<String, dynamic> json) =>
      DoctorNotificationItem(
        id: json['id'] as int? ?? 0,
        message: json['message'] as String? ?? '',
        createdAt: DateTime.parse(json['createdAt'] as String),
        status: json['status'] as int? ?? 0,
      );
}

class DoctorReview {
  const DoctorReview({
    required this.id,
    required this.userName,
    required this.rating,
    required this.comment,
  });

  final int id;
  final String userName;
  final int rating;
  final String comment;

  factory DoctorReview.fromJson(Map<String, dynamic> json) {
    final user = json['user'] as Map<String, dynamic>?;
    return DoctorReview(
      id: json['id'] as int? ?? 0,
      userName: user?['name'] as String? ?? 'مستخدم',
      rating: json['rating'] as int? ?? 0,
      comment: json['comment'] as String? ?? '',
    );
  }
}

class DoctorReviewsSummary {
  const DoctorReviewsSummary({
    required this.isEnabled,
    required this.averageRating,
    required this.reviewCount,
    required this.reviews,
  });

  final bool isEnabled;
  final double? averageRating;
  final int reviewCount;
  final List<DoctorReview> reviews;

  factory DoctorReviewsSummary.fromJson(Map<String, dynamic> json) =>
      DoctorReviewsSummary(
        isEnabled: json['isEnabled'] as bool? ?? false,
        averageRating: (json['averageRating'] as num?)?.toDouble(),
        reviewCount: json['reviewCount'] as int? ?? 0,
        reviews: ((json['reviews'] as List?) ?? const [])
            .map((item) => DoctorReview.fromJson(item as Map<String, dynamic>))
            .toList(),
      );
}

class DoctorFeatureItem {
  const DoctorFeatureItem({
    required this.id,
    required this.name,
    required this.normalizedName,
    required this.description,
    required this.isPremiumOnly,
    required this.isEnabled,
  });

  final int id;
  final String name;
  final String normalizedName;
  final String? description;
  final bool isPremiumOnly;
  final bool isEnabled;

  factory DoctorFeatureItem.fromJson(Map<String, dynamic> json) {
    final feature = json['feature'] as Map<String, dynamic>? ?? json;
    return DoctorFeatureItem(
      id: json['id'] as int? ?? 0,
      name: feature['name'] as String? ?? '',
      normalizedName: feature['normalizedName'] as String? ?? '',
      description: feature['description'] as String?,
      isPremiumOnly: feature['isPremiumOnly'] as bool? ?? false,
      isEnabled: json['isEnabled'] as bool? ?? false,
    );
  }
}

class SubscriptionPackage {
  const SubscriptionPackage({
    required this.id,
    required this.name,
    required this.normalizedName,
    required this.price,
    required this.yearlyPrice,
    required this.maxClinics,
    required this.maxDailyAppointments,
    required this.maxWeeklyDays,
    required this.showReviews,
    required this.showMessages,
    required this.eBooking,
    required this.ePayments,
    required this.makeOffers,
    required this.maxActiveOffers,
  });

  final int id;
  final String name;
  final String normalizedName;
  final double price;
  final double yearlyPrice;
  final int maxClinics;
  final int maxDailyAppointments;
  final int maxWeeklyDays;
  final bool showReviews;
  final bool showMessages;
  final bool eBooking;
  final bool ePayments;
  final bool makeOffers;
  final int maxActiveOffers;

  factory SubscriptionPackage.fromJson(Map<String, dynamic> json) =>
      SubscriptionPackage(
        id: json['id'] as int? ?? 0,
        name: json['name'] as String? ?? '',
        normalizedName: json['normalizedName'] as String? ?? '',
        price: (json['price'] as num?)?.toDouble() ?? 0,
        yearlyPrice: (json['yearlyPrice'] as num?)?.toDouble() ?? 0,
        maxClinics: json['maxClinics'] as int? ?? 0,
        maxDailyAppointments: json['maxDailyAppointments'] as int? ?? 0,
        maxWeeklyDays: json['maxWeeklyDays'] as int? ?? 0,
        showReviews: json['showReviews'] as bool? ?? false,
        showMessages: json['showMessages'] as bool? ?? false,
        eBooking: json['eBooking'] as bool? ?? false,
        ePayments: json['ePayments'] as bool? ?? false,
        makeOffers: json['makeOffers'] as bool? ?? false,
        maxActiveOffers: json['maxActiveOffers'] as int? ?? 0,
      );

  List<String> get featureLabels => [
    'عدد العيادات: $maxClinics',
    'حجوزات يومية: $maxDailyAppointments',
    'أيام دوام أسبوعية: $maxWeeklyDays',
    if (showReviews) 'إظهار التقييمات',
    if (showMessages) 'الرسائل',
    if (eBooking) 'الحجز الإلكتروني',
    if (ePayments) 'الدفع الإلكتروني',
    if (makeOffers) 'إنشاء عروض',
    'العروض النشطة: $maxActiveOffers',
  ];
}

class DoctorSubscriptionInfo {
  const DoctorSubscriptionInfo({
    required this.id,
    required this.packageId,
    required this.packageName,
    required this.packageArabicName,
    required this.packageEnglishName,
    required this.packageNormalizedName,
    required this.package,
    required this.statusName,
    required this.startDate,
    required this.endDate,
    required this.daysRemaining,
    required this.isTopPackage,
    required this.features,
  });

  final int id;
  final int packageId;
  final String packageName;
  final String packageArabicName;
  final String packageEnglishName;
  final String packageNormalizedName;
  final SubscriptionPackage package;
  final String statusName;
  final DateTime? startDate;
  final DateTime? endDate;
  final int daysRemaining;
  final bool isTopPackage;
  final List<DoctorFeatureItem> features;

  factory DoctorSubscriptionInfo.fromJson(Map<String, dynamic> json) {
    final package = json['package'] as Map<String, dynamic>?;
    final packageJson = <String, dynamic>{
      'id': json['packageId'] ?? package?['id'],
      'name':
          json['packageArabicName'] ??
          json['packageName'] ??
          package?['arabicName'] ??
          package?['name'],
      'normalizedName':
          json['packageEnglishName'] ??
          json['packageNormalizedName'] ??
          package?['englishName'] ??
          package?['normalizedName'],
      'price': json['price'] ?? package?['price'],
      'yearlyPrice': json['yearlyPrice'] ?? package?['yearlyPrice'],
      'maxClinics': json['maxClinics'] ?? package?['maxClinics'],
      'maxDailyAppointments':
          json['maxDailyAppointments'] ?? package?['maxDailyAppointments'],
      'maxWeeklyDays': json['maxWeeklyDays'] ?? package?['maxWeeklyDays'],
      'showReviews': json['showReviews'] ?? package?['showReviews'],
      'showMessages': json['showMessages'] ?? package?['showMessages'],
      'eBooking': json['eBooking'] ?? package?['eBooking'],
      'ePayments': json['ePayments'] ?? package?['ePayments'],
      'makeOffers': json['makeOffers'] ?? package?['makeOffers'],
      'maxActiveOffers': json['maxActiveOffers'] ?? package?['maxActiveOffers'],
    };
    final parsedPackage = SubscriptionPackage.fromJson(packageJson);

    return DoctorSubscriptionInfo(
      id: json['id'] as int? ?? 0,
      packageId: json['packageId'] as int? ?? parsedPackage.id,
      packageName:
          json['packageName'] as String? ??
          package?['name'] as String? ??
          '',
      packageArabicName:
          json['packageArabicName'] as String? ??
          package?['arabicName'] as String? ??
          package?['name'] as String? ??
          json['packageName'] as String? ??
          '',
      packageEnglishName:
          json['packageEnglishName'] as String? ??
          json['packageNormalizedName'] as String? ??
          package?['englishName'] as String? ??
          package?['normalizedName'] as String? ??
          '',
      packageNormalizedName:
          json['packageNormalizedName'] as String? ??
          package?['normalizedName'] as String? ??
          '',
      package: parsedPackage,
      statusName: json['statusName'] as String? ?? json['status'] as String? ?? '',
      startDate: json['startDate'] == null
          ? null
          : DateTime.tryParse(json['startDate'] as String),
      endDate: json['endDate'] == null
          ? null
          : DateTime.tryParse(json['endDate'] as String),
      daysRemaining: json['daysRemaining'] as int? ?? 0,
      isTopPackage: json['isTopPackage'] as bool? ?? false,
      features: ((json['features'] as List?) ?? const [])
          .map((item) => DoctorFeatureItem.fromJson(item as Map<String, dynamic>))
          .toList(),
    );
  }
}

String _dayName(String? value) => switch (value?.toLowerCase()) {
  'sunday' => 'الأحد',
  'monday' => 'الاثنين',
  'tuesday' => 'الثلاثاء',
  'wednesday' => 'الأربعاء',
  'thursday' => 'الخميس',
  'friday' => 'الجمعة',
  'saturday' => 'السبت',
  _ => value ?? '',
};
