import 'dart:math';

import 'api_client.dart';

class AnalyticsService {
  AnalyticsService(this._client);

  static const int _maxTrackedKeys = 500;
  static bool consentGiven = false;

  final ApiClient _client;
  static final String _sessionId = _createSessionId();
  static final Set<String> _viewedOnce = <String>{};

  Future<void> track({
    required String eventType,
    int? doctorId,
    int? clinicId,
    int? specializationId,
    int? appointmentId,
    int? offerId,
    String? source,
    String? page,
    String? province,
    String? searchText,
  }) async {
    if (!consentGiven) return;
    try {
      await _client.dio.post(
        '/Analytics/track',
        data: {
          'eventType': eventType,
          'platform': 'mobile',
          'sessionId': _sessionId,
          if (doctorId != null) 'doctorId': doctorId,
          if (clinicId != null) 'clinicId': clinicId,
          if (specializationId != null) 'specializationId': specializationId,
          if (appointmentId != null) 'appointmentId': appointmentId,
          if (offerId != null) 'offerId': offerId,
          if (source?.trim().isNotEmpty == true) 'source': source!.trim(),
          if (page?.trim().isNotEmpty == true) 'page': page!.trim(),
          if (province?.trim().isNotEmpty == true) 'province': province!.trim(),
          if (searchText?.trim().isNotEmpty == true) 'searchText': searchText!.trim(),
        },
      );
    } catch (_) {
      // Analytics must never block the patient flow.
    }
  }

  void trackLater({
    required String eventType,
    int? doctorId,
    int? clinicId,
    int? specializationId,
    int? appointmentId,
    int? offerId,
    String? source,
    String? page,
    String? province,
    String? searchText,
  }) {
    Future<void>.microtask(
      () => track(
        eventType: eventType,
        doctorId: doctorId,
        clinicId: clinicId,
        specializationId: specializationId,
        appointmentId: appointmentId,
        offerId: offerId,
        source: source,
        page: page,
        province: province,
        searchText: searchText,
      ),
    );
  }

  void trackOnce({
    required String key,
    required String eventType,
    int? doctorId,
    int? clinicId,
    int? specializationId,
    int? appointmentId,
    int? offerId,
    String? source,
    String? page,
    String? province,
    String? searchText,
  }) {
    if (!_viewedOnce.add(key)) return;
    if (_viewedOnce.length > _maxTrackedKeys) {
      _viewedOnce.remove(_viewedOnce.first);
    }
    trackLater(
      eventType: eventType,
      doctorId: doctorId,
      clinicId: clinicId,
      specializationId: specializationId,
      appointmentId: appointmentId,
      offerId: offerId,
      source: source,
      page: page,
      province: province,
      searchText: searchText,
    );
  }

  static String _createSessionId() {
    final random = Random.secure();
    final value = List<int>.generate(12, (_) => random.nextInt(256))
        .map((part) => part.toRadixString(16).padLeft(2, '0'))
        .join();
    return '${DateTime.now().millisecondsSinceEpoch}-$value';
  }
}
