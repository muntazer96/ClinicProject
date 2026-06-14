class Specialization {
  const Specialization({
    required this.id,
    required this.name,
    required this.iconName,
  });

  final int id;
  final String name;
  final String iconName;

  factory Specialization.fromJson(Map<String, dynamic> json) => Specialization(
    id: json['id'] as int,
    name: json['name'] as String,
    iconName: json['iconName'] as String? ?? 'medical-services',
  );
}

class Province {
  const Province(this.id, this.name);

  final int id;
  final String name;
}

const provinces = [
  Province(0, 'بغداد'),
  Province(1, 'البصرة'),
  Province(2, 'نينوى'),
  Province(3, 'الأنبار'),
  Province(4, 'أربيل'),
  Province(5, 'السليمانية'),
  Province(6, 'دهوك'),
  Province(7, 'كركوك'),
  Province(8, 'ذي قار'),
  Province(9, 'النجف'),
  Province(10, 'كربلاء'),
  Province(11, 'ديالى'),
  Province(12, 'بابل'),
  Province(13, 'واسط'),
  Province(14, 'ميسان'),
  Province(15, 'صلاح الدين'),
  Province(16, 'القادسية'),
  Province(17, 'المثنى'),
];

class DoctorSummary {
  const DoctorSummary({
    required this.id,
    required this.name,
    required this.specializationName,
    required this.specializationIconName,
    required this.description,
    required this.imageName,
    required this.canBookOnline,
    required this.averageRating,
    required this.reviewCount,
    required this.isFeatured,
    required this.activeSubscriptionName,
    required this.activeSubscriptionNormalizedName,
    required this.activeSubscriptionWeight,
    required this.clinics,
  });

  final int id;
  final String name;
  final String specializationName;
  final String specializationIconName;
  final String description;
  final String imageName;
  final bool canBookOnline;
  final double? averageRating;
  final int reviewCount;
  final bool isFeatured;
  final String? activeSubscriptionName;
  final String? activeSubscriptionNormalizedName;
  final double activeSubscriptionWeight;
  final List<ClinicSummary> clinics;

  factory DoctorSummary.fromJson(Map<String, dynamic> json) => DoctorSummary(
    id: json['id'] as int,
    name: json['name'] as String? ?? '',
    specializationName: json['specializationName'] as String? ?? '',
    specializationIconName:
        json['specializationIconName'] as String? ?? 'medical-services',
    description: json['description'] as String? ?? '',
    imageName: json['imageName'] as String? ?? '',
    canBookOnline: json['canBookOnline'] as bool? ?? false,
    averageRating: (json['averageRating'] as num?)?.toDouble(),
    reviewCount: json['reviewCount'] as int? ?? 0,
    isFeatured: json['isFeatured'] as bool? ?? false,
    activeSubscriptionName: json['activeSubscriptionName'] as String?,
    activeSubscriptionNormalizedName:
        json['activeSubscriptionNormalizedName'] as String?,
    activeSubscriptionWeight:
        (json['activeSubscriptionWeight'] as num?)?.toDouble() ?? 0,
    clinics: (json['clinics'] as List? ?? const [])
        .map((item) => ClinicSummary.fromJson(item as Map<String, dynamic>))
        .toList(),
  );
}

class DoctorProfile extends DoctorSummary {
  const DoctorProfile({
    required super.id,
    required super.name,
    required super.specializationName,
    required super.specializationIconName,
    required super.description,
    required super.imageName,
    required super.canBookOnline,
    required super.averageRating,
    required super.reviewCount,
    required super.isFeatured,
    required super.activeSubscriptionName,
    required super.activeSubscriptionNormalizedName,
    required super.activeSubscriptionWeight,
    required super.clinics,
    this.userId,
    this.canMessage = true,
    required this.clinicDetails,
  });

  final String? userId;
  final bool canMessage;
  final List<ClinicDetails> clinicDetails;

  factory DoctorProfile.fromJson(Map<String, dynamic> json) {
    final clinics = (json['clinics'] as List? ?? const [])
        .map((item) => ClinicDetails.fromJson(item as Map<String, dynamic>))
        .toList();
    return DoctorProfile(
      id: json['id'] as int,
      name: json['name'] as String? ?? '',
      specializationName: json['specializationName'] as String? ?? '',
      specializationIconName:
          json['specializationIconName'] as String? ?? 'medical-services',
      description: json['description'] as String? ?? '',
      imageName: json['imageName'] as String? ?? '',
      canBookOnline: json['canBookOnline'] as bool? ?? false,
      averageRating: (json['averageRating'] as num?)?.toDouble(),
      reviewCount: json['reviewCount'] as int? ?? 0,
      isFeatured: json['isFeatured'] as bool? ?? false,
      activeSubscriptionName: json['activeSubscriptionName'] as String?,
      activeSubscriptionNormalizedName:
          json['activeSubscriptionNormalizedName'] as String?,
      activeSubscriptionWeight:
          (json['activeSubscriptionWeight'] as num?)?.toDouble() ?? 0,
      userId: json['userId'] as String?,
      canMessage: json['canMessage'] as bool? ?? true,
      clinics: clinics,
      clinicDetails: clinics,
    );
  }
}

class ClinicSummary {
  const ClinicSummary({
    required this.id,
    required this.name,
    required this.provinceName,
    required this.address,
    required this.consultationPrice,
    required this.showConsultationPrice,
  });

  final int id;
  final String name;
  final String provinceName;
  final String address;
  final double? consultationPrice;
  final bool showConsultationPrice;

  factory ClinicSummary.fromJson(Map<String, dynamic> json) => ClinicSummary(
    id: json['id'] as int,
    name: json['name'] as String? ?? '',
    provinceName: json['iraqiProvinceName'] as String? ?? '',
    address: json['address'] as String? ?? '',
    consultationPrice: (json['consultationPrice'] as num?)?.toDouble(),
    showConsultationPrice: json['showConsultationPrice'] as bool? ?? false,
  );
}

class ClinicDetails extends ClinicSummary {
  const ClinicDetails({
    required super.id,
    required super.name,
    required super.provinceName,
    required super.address,
    required super.consultationPrice,
    required super.showConsultationPrice,
    required this.mapUrl,
    required this.phoneNumber,
    required this.availabilities,
  });

  final String? mapUrl;
  final String? phoneNumber;
  final List<ClinicAvailability> availabilities;

  factory ClinicDetails.fromJson(Map<String, dynamic> json) => ClinicDetails(
    id: json['id'] as int,
    name: json['name'] as String? ?? '',
    provinceName: json['iraqiProvinceName'] as String? ?? '',
    address: json['address'] as String? ?? '',
    consultationPrice: (json['consultationPrice'] as num?)?.toDouble(),
    showConsultationPrice: json['showConsultationPrice'] as bool? ?? false,
    mapUrl: json['mapUrl'] as String?,
    phoneNumber: json['phoneNumber'] as String?,
    availabilities: (json['availabilities'] as List? ?? const [])
        .map(
          (item) => ClinicAvailability.fromJson(item as Map<String, dynamic>),
        )
        .toList(),
  );
}

class ClinicAvailability {
  const ClinicAvailability({
    required this.dayName,
    required this.startTime,
    required this.endTime,
    required this.maxAppointments,
  });

  final String dayName;
  final String startTime;
  final String endTime;
  final int maxAppointments;

  factory ClinicAvailability.fromJson(Map<String, dynamic> json) =>
      ClinicAvailability(
        dayName: _arabicDayName(
          json['dayName'] as String?,
          json['dayNormalizedName'] as String?,
        ),
        startTime: json['startTime'] as String? ?? '',
        endTime: json['endTime'] as String? ?? '',
        maxAppointments: json['maxAppointments'] as int? ?? 0,
      );
}

String _arabicDayName(String? dayName, String? normalizedName) {
  final value = (normalizedName?.isNotEmpty == true ? normalizedName : dayName)
      ?.toLowerCase();
  return switch (value) {
    'sunday' => 'الأحد',
    'monday' => 'الاثنين',
    'tuesday' => 'الثلاثاء',
    'wednesday' => 'الأربعاء',
    'thursday' => 'الخميس',
    'friday' => 'الجمعة',
    'saturday' => 'السبت',
    _ => dayName ?? '',
  };
}

class DoctorOffer {
  const DoctorOffer({
    required this.id,
    required this.doctorId,
    required this.doctorName,
    required this.isFeatured,
    required this.activeSubscriptionName,
    required this.activeSubscriptionNormalizedName,
    required this.activeSubscriptionWeight,
    required this.clinicName,
    required this.appliesToAllClinics,
    required this.title,
    required this.description,
    required this.offerType,
    required this.offerTypeName,
    required this.originalPrice,
    required this.offerPrice,
    required this.discountPercent,
    required this.badgeText,
    required this.terms,
    required this.startsAt,
    required this.endsAt,
    required this.remainingDays,
  });

  final int id;
  final int doctorId;
  final String doctorName;
  final bool isFeatured;
  final String? activeSubscriptionName;
  final String? activeSubscriptionNormalizedName;
  final double activeSubscriptionWeight;
  final String? clinicName;
  final bool appliesToAllClinics;
  final String title;
  final String? description;
  final int offerType;
  final String offerTypeName;
  final double? originalPrice;
  final double? offerPrice;
  final double? discountPercent;
  final String? badgeText;
  final String? terms;
  final DateTime startsAt;
  final DateTime endsAt;
  final int remainingDays;

  String get scope =>
      appliesToAllClinics ? 'جميع العيادات' : clinicName ?? 'عيادة محددة';

  String get priceText {
    if (offerType == 0 && discountPercent != null) {
      final value = discountPercent! % 1 == 0
          ? discountPercent!.toInt().toString()
          : discountPercent!.toStringAsFixed(1);
      return '$value% خصم';
    }
    if (offerType == 3) return 'استشارة مجانية';
    if (offerPrice != null) return '${offerPrice!.toStringAsFixed(0)} د.ع';
    return offerTypeName;
  }

  factory DoctorOffer.fromJson(Map<String, dynamic> json) => DoctorOffer(
    id: json['id'] as int? ?? 0,
    doctorId: json['doctorId'] as int? ?? 0,
    doctorName: json['doctorName'] as String? ?? '',
    isFeatured: json['isFeatured'] as bool? ?? false,
    activeSubscriptionName: json['activeSubscriptionName'] as String?,
    activeSubscriptionNormalizedName:
        json['activeSubscriptionNormalizedName'] as String?,
    activeSubscriptionWeight:
        (json['activeSubscriptionWeight'] as num?)?.toDouble() ?? 0,
    clinicName: json['clinicName'] as String?,
    appliesToAllClinics: json['appliesToAllClinics'] as bool? ?? false,
    title: json['title'] as String? ?? '',
    description: json['description'] as String?,
    offerType: json['offerType'] as int? ?? 0,
    offerTypeName: json['offerTypeName'] as String? ?? '',
    originalPrice: (json['originalPrice'] as num?)?.toDouble(),
    offerPrice: (json['offerPrice'] as num?)?.toDouble(),
    discountPercent: (json['discountPercent'] as num?)?.toDouble(),
    badgeText: json['badgeText'] as String?,
    terms: json['terms'] as String?,
    startsAt: DateTime.tryParse(json['startsAt'] as String? ?? '') ?? DateTime.now(),
    endsAt: DateTime.tryParse(json['endsAt'] as String? ?? '') ?? DateTime.now(),
    remainingDays: json['remainingDays'] as int? ?? 0,
  );
}

class DoctorOfferResult {
  const DoctorOfferResult({
    required this.items,
    required this.totalItems,
    required this.totalPages,
    required this.currentPage,
  });

  final List<DoctorOffer> items;
  final int totalItems;
  final int totalPages;
  final int currentPage;
}
