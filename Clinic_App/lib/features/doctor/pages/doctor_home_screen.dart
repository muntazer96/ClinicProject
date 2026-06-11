import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorHomeScreen extends StatefulWidget {
  const DoctorHomeScreen({super.key});

  @override
  State<DoctorHomeScreen> createState() => _DoctorHomeScreenState();
}

class _DoctorHomeScreenState extends State<DoctorHomeScreen> {
  late final DoctorService _service;
  DoctorProfile? _profile;
  List<DoctorAppointment> _appointments = [];
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
      final result = await Future.wait([
        _service.getProfile(),
        _service.getAppointments(),
      ]);
      if (!mounted) return;
      _profile = result[0] as DoctorProfile;
      _appointments = result[1] as List<DoctorAppointment>;
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final now = DateTime.now();
    final todayItems = _appointments.where((item) {
      final date = item.appointmentDate;
      return date.year == now.year &&
          date.month == now.month &&
          date.day == now.day;
    }).toList();
    final upcoming = _appointments
        .where((item) => item.appointmentDate.isAfter(now) && item.status != 2)
        .toList()
      ..sort((a, b) => a.appointmentDate.compareTo(b.appointmentDate));
    final nextAppointment = upcoming.isEmpty ? null : upcoming.first;
    final completed = todayItems.where((item) => item.status == 3).length;
    final cancelled = todayItems.where((item) => item.status == 2).length;

    return DoctorScaffold(
      title: 'لوحة الطبيب',
      child: RefreshIndicator(
        onRefresh: _load,
        child: _loading
            ? const Center(child: CircularProgressIndicator())
            : DoctorPage(
                children: [
                  _DoctorHero(profile: _profile),
                  const SizedBox(height: 12),
                  GridView.count(
                    crossAxisCount: 2,
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    crossAxisSpacing: 10,
                    mainAxisSpacing: 10,
                    childAspectRatio: 1.18,
                    children: [
                      _StatTile('حجوزات اليوم', todayItems.length, Icons.event_available_rounded),
                      _StatTile('القادمة اليوم', todayItems.where((item) => item.status == 0 || item.status == 1).length, Icons.calendar_month_rounded),
                      _StatTile('المكتملة', completed, Icons.verified_rounded),
                      _StatTile('الملغاة', cancelled, Icons.cancel_outlined),
                    ],
                  ),
                  const SizedBox(height: 12),
                  _QuickLinks(),
                  const SizedBox(height: 18),
                  Text(
                    'أقرب حجز قادم',
                    style: Theme.of(context)
                        .textTheme
                        .titleMedium
                        ?.copyWith(fontWeight: FontWeight.w900),
                  ),
                  const SizedBox(height: 10),
                  if (nextAppointment == null)
                    const DoctorSectionCard(
                      child: Text(
                        'لا يوجد حجز قادم حالياً.',
                        style: TextStyle(color: AppColors.muted),
                      ),
                    )
                  else
                    _NextAppointmentCard(item: nextAppointment),
                ],
              ),
      ),
    );
  }
}

class _DoctorHero extends StatelessWidget {
  const _DoctorHero({required this.profile});

  final DoctorProfile? profile;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(14),
    decoration: BoxDecoration(
      gradient: const LinearGradient(
        colors: [AppColors.primaryDark, AppColors.primary],
      ),
      borderRadius: BorderRadius.circular(14),
      boxShadow: const [
        BoxShadow(
          color: Color(0x24155E75),
          blurRadius: 20,
          offset: Offset(0, 10),
        ),
      ],
    ),
    child: Row(
      children: [
        Container(
          width: 58,
          height: 58,
          decoration: const BoxDecoration(
            color: Color(0xFFFFF4DB),
            shape: BoxShape.circle,
          ),
          child: const Icon(
            Icons.workspace_premium_rounded,
            color: AppColors.accent,
            size: 34,
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                profile?.name.isNotEmpty == true ? profile!.name : 'أهلاً دكتور',
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 17,
                  fontWeight: FontWeight.w900,
                ),
              ),
              const SizedBox(height: 3),
              Text(
                profile?.specialization ?? 'إدارة حجوزاتك اليومية',
                style: const TextStyle(
                  color: Color(0xFFE4F7F4),
                  fontSize: 12,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ],
          ),
        ),
        CircleAvatar(
          radius: 28,
          backgroundColor: Colors.white,
          backgroundImage:
              profile?.imageUrl == null ? null : NetworkImage(profile!.imageUrl!),
          child: profile?.imageUrl == null
              ? const Icon(Icons.person_rounded, color: AppColors.primary)
              : null,
        ),
      ],
    ),
  );
}

class _StatTile extends StatelessWidget {
  const _StatTile(this.label, this.value, this.icon);

  final String label;
  final int value;
  final IconData icon;

  @override
  Widget build(BuildContext context) => DoctorSectionCard(
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Align(
          alignment: AlignmentDirectional.centerEnd,
          child: Icon(icon, color: AppColors.primary, size: 20),
        ),
        const Spacer(),
        Text(
          '$value',
          style: const TextStyle(fontSize: 24, fontWeight: FontWeight.w900),
        ),
        Text(
          label,
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
          style: const TextStyle(color: AppColors.muted, fontSize: 12),
        ),
      ],
    ),
  );
}

class _QuickLinks extends StatelessWidget {
  @override
  Widget build(BuildContext context) => Row(
    children: [
      Expanded(child: _LinkTile('العيادات', Icons.add_box_rounded, '/doctor/clinics')),
      const SizedBox(width: 8),
      Expanded(child: _LinkTile('العروض', Icons.local_offer_rounded, '/doctor/offers')),
      const SizedBox(width: 8),
      Expanded(child: _LinkTile('التقييمات', Icons.star_rounded, '/doctor/reviews')),
      const SizedBox(width: 8),
      Expanded(child: _LinkTile('المميزات', Icons.tune_rounded, '/doctor/profile')),
    ],
  );
}

class _LinkTile extends StatelessWidget {
  const _LinkTile(this.label, this.icon, this.route);

  final String label;
  final IconData icon;
  final String route;

  @override
  Widget build(BuildContext context) => InkWell(
    borderRadius: BorderRadius.circular(10),
    onTap: () => context.go(route),
    child: Container(
      height: 42,
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(10),
        border: Border.all(color: AppColors.border),
      ),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(icon, size: 17, color: AppColors.primary),
          const SizedBox(height: 2),
          Text(
            label,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(fontSize: 10, fontWeight: FontWeight.w800),
          ),
        ],
      ),
    ),
  );
}

class _NextAppointmentCard extends StatelessWidget {
  const _NextAppointmentCard({required this.item});

  final DoctorAppointment item;

  @override
  Widget build(BuildContext context) => DoctorSectionCard(
    child: Row(
      children: [
        DoctorStatusPill(label: '#${item.queueNumber}'),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                item.patientName,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(fontWeight: FontWeight.w900),
              ),
              const SizedBox(height: 2),
              Text(
                '${DateFormat('yyyy/MM/dd').format(item.appointmentDate)} - ${item.clinicName}',
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(color: AppColors.muted, fontSize: 12),
              ),
            ],
          ),
        ),
        const Icon(Icons.calendar_month_rounded, color: AppColors.primary),
      ],
    ),
  );
}
