class Specialization {
  const Specialization({required this.id, required this.name});

  final int id;
  final String name;

  factory Specialization.fromJson(Map<String, dynamic> json) =>
      Specialization(id: json['id'] as int, name: json['name'] as String);
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
    required this.clinicDetails,
  });

  final List<ClinicDetails> clinicDetails;

  factory DoctorProfile.fromJson(Map<String, dynamic> json) {
    final clinics = (json['clinics'] as List? ?? const [])
        .map((item) => ClinicDetails.fromJson(item as Map<String, dynamic>))
        .toList();
    return DoctorProfile(
      id: json['id'] as int,
      name: json['name'] as String? ?? '',
      specializationName: json['specializationName'] as String? ?? '',
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
  });

  final int id;
  final String name;
  final String provinceName;
  final String address;

  factory ClinicSummary.fromJson(Map<String, dynamic> json) => ClinicSummary(
    id: json['id'] as int,
    name: json['name'] as String? ?? '',
    provinceName: json['iraqiProvinceName'] as String? ?? '',
    address: json['address'] as String? ?? '',
  );
}

class ClinicDetails extends ClinicSummary {
  const ClinicDetails({
    required super.id,
    required super.name,
    required super.provinceName,
    required super.address,
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
