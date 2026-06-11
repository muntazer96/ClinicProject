import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../../core/app_theme.dart';
import '../../account/profile_screen.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorProfileScreen extends StatefulWidget {
  const DoctorProfileScreen({super.key});

  @override
  State<DoctorProfileScreen> createState() => _DoctorProfileScreenState();
}

class _DoctorProfileScreenState extends State<DoctorProfileScreen> {
  late final DoctorService _service;
  DoctorProfile? _profile;
  DoctorSubscriptionInfo? _subscription;
  List<DoctorFeatureItem> _features = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final profile = await _service.getProfile();
      _profile = profile;
      try {
        _subscription = await _service.getCurrentSubscription();
        _features = _subscription?.features ?? await _service.getDoctorFeatures(profile.id);
      } catch (_) {
        _subscription = null;
        _features = const [];
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) => DefaultTabController(
    length: 2,
    child: DoctorScaffold(
      title: 'الملف الشخصي',
      child: Column(
        children: [
          const TabBar(
            tabs: [
              Tab(text: 'حسابي'),
              Tab(text: 'بيانات الطبيب'),
            ],
          ),
          Expanded(
            child: TabBarView(
              children: [
                const ProfileScreen(),
                _loading
                    ? const Center(child: CircularProgressIndicator())
                    : RefreshIndicator(
                        onRefresh: _load,
                        child: ListView(
                          padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
                          children: [
                            _DoctorProfileCard(profile: _profile),
                            const SizedBox(height: 12),
                            FilledButton.icon(
                              onPressed: () async {
                                await context.push(
                                  '/doctor/profile/edit',
                                  extra: _profile,
                                );
                                await _load();
                              },
                              icon: const Icon(Icons.edit_outlined),
                              label: const Text('تعديل بيانات الطبيب'),
                            ),
                            const SizedBox(height: 12),
                            _SubscriptionCard(
                              subscription: _subscription,
                            ),
                            const SizedBox(height: 12),
                            DoctorSectionCard(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Row(
                                    children: [
                                      const Expanded(
                                        child: Text(
                                          'المميزات المتاحة',
                                          style: TextStyle(
                                            fontWeight: FontWeight.w900,
                                            fontSize: 16,
                                          ),
                                        ),
                                      ),
                                      TextButton(
                                        onPressed: () => context.push(
                                          '/doctor/features',
                                          extra: _profile,
                                        ),
                                        child: const Text('إدارة'),
                                      ),
                                    ],
                                  ),
                                  const SizedBox(height: 8),
                                  if (_features.isEmpty)
                                    const Text(
                                      'لا توجد بيانات مميزات متاحة من صلاحيات الـ API الحالية.',
                                      style: TextStyle(color: AppColors.muted),
                                    )
                                  else
                                    Wrap(
                                      spacing: 8,
                                      runSpacing: 8,
                                      children: _features
                                          .where((item) => item.isEnabled)
                                          .map(
                                            (item) => Chip(label: Text(item.name)),
                                          )
                                          .toList(),
                                    ),
                                ],
                              ),
                            ),
                          ],
                        ),
                      ),
              ],
            ),
          ),
        ],
      ),
    ),
  );
}

class _DoctorProfileCard extends StatelessWidget {
  const _DoctorProfileCard({required this.profile});

  final DoctorProfile? profile;

  @override
  Widget build(BuildContext context) => DoctorSectionCard(
    child: Row(
      children: [
        CircleAvatar(
          radius: 34,
          backgroundColor: AppColors.softBlue,
          backgroundImage:
              profile?.imageUrl == null ? null : NetworkImage(profile!.imageUrl!),
          child: profile?.imageUrl == null
              ? const Icon(Icons.medical_services_rounded)
              : null,
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                profile?.name ?? '-',
                style: const TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.w900,
                ),
              ),
              Text(
                profile?.specialization ?? '-',
                style: const TextStyle(color: AppColors.muted),
              ),
              Text(
                profile?.description ?? '',
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
            ],
          ),
        ),
      ],
    ),
  );
}

class _SubscriptionCard extends StatelessWidget {
  const _SubscriptionCard({required this.subscription});

  final DoctorSubscriptionInfo? subscription;

  @override
  Widget build(BuildContext context) => DoctorSectionCard(
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'الاشتراك الحالي',
          style: TextStyle(fontSize: 16, fontWeight: FontWeight.w900),
        ),
        const SizedBox(height: 8),
        if (subscription == null)
          const Text(
            'لا توجد بيانات اشتراك متاحة لهذا الحساب.',
            style: TextStyle(color: AppColors.muted),
          )
        else ...[
          Text(subscription!.packageName),
          Text(
            'التفعيل: ${_format(subscription!.startDate)}',
            style: const TextStyle(color: AppColors.muted),
          ),
          Text(
            'الانتهاء: ${_format(subscription!.endDate)}',
            style: const TextStyle(color: AppColors.muted),
          ),
        ],
      ],
    ),
  );

  String _format(DateTime? value) =>
      value == null ? '-' : DateFormat('yyyy/MM/dd').format(value);
}
