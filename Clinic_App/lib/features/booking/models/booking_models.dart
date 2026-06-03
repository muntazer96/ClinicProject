class QueueAvailability {
  const QueueAvailability({
    required this.date,
    required this.dayName,
    required this.remainingAppointments,
    required this.isAvailable,
    this.startTime,
    this.endTime,
    this.closureReason,
  });

  final DateTime date;
  final String dayName;
  final String? startTime;
  final String? endTime;
  final int remainingAppointments;
  final bool isAvailable;
  final String? closureReason;

  factory QueueAvailability.fromJson(Map<String, dynamic> json) =>
      QueueAvailability(
        date: DateTime.parse(json['date'] as String),
        dayName: _arabicDayName(
          json['dayName'] as String?,
          json['dayNormalizedName'] as String?,
        ),
        startTime: json['startTime'] as String?,
        endTime: json['endTime'] as String?,
        remainingAppointments: json['remainingAppointments'] as int? ?? 0,
        isAvailable: json['isAvailable'] as bool? ?? false,
        closureReason: json['closureReason'] as String?,
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

class BookingResult {
  const BookingResult({
    required this.appointmentId,
    required this.code,
    required this.queueNumber,
    required this.requiresOtp,
  });

  final int appointmentId;
  final String code;
  final int queueNumber;
  final bool requiresOtp;

  factory BookingResult.fromJson(Map<String, dynamic> json) => BookingResult(
    appointmentId:
        (json['appointmentId'] ?? json['id'] ?? json['AppointmentId']) as int,
    code: (json['code'] ?? json['Code']) as String,
    queueNumber: (json['queueNumber'] ?? json['QueueNumber']) as int,
    requiresOtp: json['requiresOtp'] as bool? ?? false,
  );
}

class BookingDetails {
  const BookingDetails({
    required this.id,
    required this.code,
    required this.patientName,
    required this.appointmentDate,
    required this.queueNumber,
    required this.status,
    required this.doctorId,
    required this.doctorName,
    required this.clinicName,
    required this.clinicAddress,
    this.clinicPhoneNumber,
    this.mapUrl,
    this.cancellationReason,
    required this.hasReview,
  });

  final int id;
  final String code;
  final String patientName;
  final DateTime appointmentDate;
  final int queueNumber;
  final int status;
  final int doctorId;
  final String doctorName;
  final String clinicName;
  final String clinicAddress;
  final String? clinicPhoneNumber;
  final String? mapUrl;
  final String? cancellationReason;
  final bool hasReview;

  bool get canCancel => status == 0 || status == 1;
  bool get canReview => status == 3 && !hasReview;

  String get statusLabel => switch (status) {
    0 => 'بانتظار التأكيد',
    1 => 'مؤكد',
    2 => 'ملغي',
    3 => 'مكتمل',
    _ => 'غير معروف',
  };

  factory BookingDetails.fromJson(Map<String, dynamic> json) => BookingDetails(
    id: json['id'] as int,
    code: json['code'] as String? ?? '',
    patientName: json['patientName'] as String? ?? '',
    appointmentDate: DateTime.parse(json['appointmentDate'] as String),
    queueNumber: json['queueNumber'] as int? ?? 0,
    status: json['status'] as int? ?? 0,
    doctorId: json['doctorId'] as int? ?? 0,
    doctorName: json['doctorName'] as String? ?? '',
    clinicName: json['clinicName'] as String? ?? '',
    clinicAddress: json['clinicAddress'] as String? ?? '',
    clinicPhoneNumber: json['clinicPhoneNumber'] as String?,
    mapUrl: json['mapUrl'] as String?,
    cancellationReason: json['cancellationReason'] as String?,
    hasReview: json['hasReview'] as bool? ?? false,
  );
}
