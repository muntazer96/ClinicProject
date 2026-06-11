import 'package:flutter/foundation.dart';

import '../models/doctor_models.dart';
import '../services/doctor_service.dart';

class DoctorController extends ChangeNotifier {
  DoctorController(this.service);

  final DoctorService service;
  bool loading = false;
  String? error;

  DoctorProfile? profile;
  List<DoctorAppointment> appointments = [];
  List<DoctorClinic> clinics = [];
  List<DoctorOfferManage> offers = [];
  List<SubscriptionPackage> packages = [];
  List<DoctorFeatureItem> features = [];
  List<DoctorSubscriptionInfo> subscriptions = [];

  Future<void> _run(Future<void> Function() action) async {
    loading = true;
    error = null;
    notifyListeners();
    try {
      await action();
    } catch (e) {
      error = e.toString();
      rethrow;
    } finally {
      loading = false;
      notifyListeners();
    }
  }

  Future<void> loadDashboard() => _run(() async {
    final result = await Future.wait([
      service.getProfile(),
      service.getAppointments(),
    ]);
    profile = result[0] as DoctorProfile;
    appointments = result[1] as List<DoctorAppointment>;
  });

  Future<void> loadAppointments({int? status, DateTime? from, DateTime? to}) =>
      _run(() async {
        appointments = await service.getAppointments(
          status: status,
          fromDate: from,
          toDate: to,
        );
      });

  Future<void> loadClinics() => _run(() async {
    clinics = await service.getClinics();
  });

  Future<void> loadOffers() => _run(() async {
    offers = await service.getOffers();
  });

  Future<void> loadSubscriptionData() => _run(() async {
    profile ??= await service.getProfile();
    packages = await service.getSubscriptionPackages();
    if (profile != null) {
      try {
        final current = await service.getCurrentSubscription();
        subscriptions = current == null ? const [] : [current];
        features = current?.features ?? await service.getDoctorFeatures(profile!.id);
      } catch (_) {
        subscriptions = const [];
        features = const [];
      }
    }
  });
}
