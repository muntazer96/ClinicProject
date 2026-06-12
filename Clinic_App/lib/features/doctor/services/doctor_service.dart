import 'dart:typed_data';

import 'package:dio/dio.dart';

import '../../../core/api_client.dart';
import '../models/doctor_models.dart';

class DoctorService {
  DoctorService(this._client);

  final ApiClient _client;

  List<dynamic> _listData(dynamic data) {
    final payload = data is Map ? data['data'] : data;
    if (payload is List) return payload;
    if (payload is Map && payload['items'] is List) {
      return payload['items'] as List;
    }
    return const [];
  }

  Map<String, dynamic> _mapData(dynamic data) =>
      ((data is Map ? data['data'] : data) as Map).cast<String, dynamic>();

  Future<List<ProvinceItem>> getProvinces() async {
    final response = await _client.dio.get('/IraqiProvince');
    return _listData(response.data)
        .map((item) => ProvinceItem.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<DoctorProfile> getProfile() async {
    final response = await _client.dio.get('/Doctor/my');
    return DoctorProfile.fromJson(_mapData(response.data));
  }

  Future<void> updateProfile(
    DoctorProfile profile, {
    required String name,
    required String description,
    required String phoneNumber,
    required String location,
  }) async {
    final form = FormData.fromMap({
      'Name': name.trim(),
      'NormalizedName': name.trim(),
      'SpecializationId': profile.specializationId,
      'Description': description.trim(),
      'IraqiProvince': profile.iraqiProvince,
      'BirthDay': _dateOnly(profile.birthDay ?? DateTime(1980)),
      'PhoneNumber': phoneNumber.trim(),
      'Location': location.trim(),
    });
    await _client.dio.put('/Doctor/my', data: form);
  }

  Future<void> updateProfileImage({
    required String fileName,
    required Uint8List bytes,
  }) async {
    await _client.dio.put(
      '/Doctor/image',
      data: FormData.fromMap({
        'file': MultipartFile.fromBytes(bytes, filename: fileName),
      }),
    );
  }

  Future<List<DoctorClinic>> getClinics() async {
    final response = await _client.dio.get('/Clinic/my');
    return _listData(response.data)
        .map((item) => DoctorClinic.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<void> addClinic(Map<String, dynamic> data) async {
    await _client.dio.post('/Clinic/my', data: data);
  }

  Future<void> updateClinic(Map<String, dynamic> data) async {
    await _client.dio.put('/Clinic/my', data: data);
  }

  Future<void> updateClinicPrice(DoctorClinic clinic, double? price) async {
    await updateClinic(clinic.toUpdateJson(price: price));
  }

  Future<void> deleteClinic(int id) async {
    await _client.dio.delete('/Clinic/my/$id');
  }

  Future<List<DoctorAppointment>> getAppointments({
    int? status,
    DateTime? fromDate,
    DateTime? toDate,
  }) async {
    final response = await _client.dio.get(
      '/Appointment/doctor/my',
      queryParameters: {
        if (status != null) 'status': status,
        if (fromDate != null) 'fromDate': _dateOnly(fromDate),
        if (toDate != null) 'toDate': _dateOnly(toDate),
      },
    );
    return _listData(response.data)
        .map((item) => DoctorAppointment.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<void> createManualAppointment({
    required int clinicId,
    required DateTime appointmentDate,
    required String patientName,
    required String patientPhoneNumber,
    String? notes,
  }) async {
    await _client.dio.post(
      '/Appointment/manual',
      data: {
        'clinicId': clinicId,
        'appointmentDate': appointmentDate.toIso8601String(),
        'patientName': patientName.trim(),
        'patientPhoneNumber': patientPhoneNumber.trim(),
        if (notes?.trim().isNotEmpty == true) 'notes': notes!.trim(),
      },
    );
  }

  Future<void> toggleAppointment(int appointmentId) async {
    await _client.dio.post(
      '/Appointment/toggle-status',
      queryParameters: {'appointmentId': appointmentId},
    );
  }

  Future<void> completeAppointment(int appointmentId) async {
    await _client.dio.post(
      '/Appointment/complete',
      queryParameters: {'appointmentId': appointmentId},
    );
  }

  Future<List<DoctorAvailability>> getAvailability(int clinicId) async {
    final response = await _client.dio.get('/DoctorAvailability/$clinicId');
    return _listData(response.data)
        .map(
          (item) => DoctorAvailability.fromJson(item as Map<String, dynamic>),
        )
        .toList();
  }

  Future<void> updateAvailability({
    required int clinicId,
    required int dayId,
    required String startTime,
    required String endTime,
    required int maxAppointments,
    required bool isAvailable,
  }) async {
    await _client.dio.put(
      '/DoctorAvailability/single-day',
      data: {
        'clinicId': clinicId,
        'dayId': dayId,
        'startTime': startTime,
        'endTime': endTime,
        'maxAppointments': maxAppointments,
        'isAvailable': isAvailable,
      },
    );
  }

  Future<List<ClinicExceptionDay>> getExceptions(int clinicId) async {
    final now = DateTime.now();
    final response = await _client.dio.get(
      '/ClinicException/my/$clinicId',
      queryParameters: {
        'fromDate': _dateOnly(now),
        'toDate': _dateOnly(now.add(const Duration(days: 45))),
      },
    );
    return _listData(response.data)
        .map(
          (item) => ClinicExceptionDay.fromJson(item as Map<String, dynamic>),
        )
        .toList();
  }

  Future<void> saveException({
    int? id,
    required int clinicId,
    required DateTime date,
    required bool isClosed,
    String? reason,
    String? startTime,
    String? endTime,
    int? maxAppointments,
  }) async {
    await _client.dio.post(
      '/ClinicException/my',
      data: {
        if (id != null && id > 0) 'id': id,
        'clinicId': clinicId,
        'exceptionDate': _dateOnly(date),
        'isClosed': isClosed,
        if (reason?.trim().isNotEmpty == true) 'closureReason': reason!.trim(),
        if (startTime?.trim().isNotEmpty == true)
          'startTime': startTime!.trim(),
        if (endTime?.trim().isNotEmpty == true) 'endTime': endTime!.trim(),
        if (maxAppointments != null) 'maxAppointments': maxAppointments,
      },
    );
  }

  Future<void> deleteException(int id) async {
    await _client.dio.delete('/ClinicException/my/$id');
  }

  Future<List<DoctorOfferManage>> getOffers() async {
    final response = await _client.dio.get('/DoctorOffer/my');
    return _listData(response.data)
        .map((item) => DoctorOfferManage.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<void> addOffer(Map<String, dynamic> data) async {
    await _client.dio.post('/DoctorOffer/my', data: data);
  }

  Future<void> updateOffer(Map<String, dynamic> data) async {
    await _client.dio.put('/DoctorOffer/my', data: data);
  }

  Future<void> deleteOffer(int id) async {
    await _client.dio.delete('/DoctorOffer/my/$id');
  }

  Future<DoctorReviewsSummary> getReviews() async {
    final response = await _client.dio.get('/Review/doctor/my');
    return DoctorReviewsSummary.fromJson(_mapData(response.data));
  }

  Future<List<SubscriptionPackage>> getSubscriptionPackages() async {
    final response = await _client.dio.get(
      '/SubscriptionPackages',
      queryParameters: {'pageSize': 50},
    );
    return _listData(response.data)
        .map(
          (item) => SubscriptionPackage.fromJson(item as Map<String, dynamic>),
        )
        .toList();
  }

  Future<List<DoctorFeatureItem>> getDoctorFeatures(int doctorId) async {
    final response = await _client.dio.get(
      '/DoctorFeature',
      queryParameters: {'doctorId': doctorId, 'pageSize': 50},
    );
    return _listData(response.data)
        .map((item) => DoctorFeatureItem.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<void> toggleFeature(int id) async {
    await _client.dio.post('/DoctorFeature/$id/toggle');
  }

  Future<List<DoctorSubscriptionInfo>> getDoctorSubscriptions(
    int doctorId,
  ) async {
    final response = await _client.dio.get(
      '/DoctorSubscription',
      queryParameters: {'doctorId': doctorId, 'isActive': true, 'pageSize': 5},
    );
    return _listData(response.data)
        .map(
          (item) =>
              DoctorSubscriptionInfo.fromJson(item as Map<String, dynamic>),
        )
        .toList();
  }

  Future<DoctorSubscriptionInfo?> getCurrentSubscription() async {
    try {
      final response = await _client.dio.get('/DoctorSubscription/my/current');
      return DoctorSubscriptionInfo.fromJson(_mapData(response.data));
    } on DioException catch (error) {
      if (error.response?.statusCode == 404) return null;
      rethrow;
    }
  }

  static String _dateOnly(DateTime date) =>
      '${date.year.toString().padLeft(4, '0')}-'
      '${date.month.toString().padLeft(2, '0')}-'
      '${date.day.toString().padLeft(2, '0')}';
}
