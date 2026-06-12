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
  DoctorSubscriptionInfo? _subscription;
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
        _service.getCurrentSubscription(),
      ]);

      if (!mounted) return;
      _profile = result[0] as DoctorProfile;
      _appointments = result[1] as List<DoctorAppointment>;
      _subscription = result[2] as DoctorSubscriptionInfo?;
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
    final waitingToday =
        todayItems.where((item) => item.status == 0 || item.status == 1).length;
    final completed = todayItems.where((item) => item.status == 3).length;
    final cancelled = todayItems.where((item) => item.status == 2).length;

    return DoctorScaffold(
      title: 'لوحة الطبيب',
      child: RefreshIndicator(
        onRefresh: _load,
        child: _loading
            ? const Center(child: CircularProgressIndicator())
            : ListView(
                padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
                children: [
                  _DoctorHero(profile: _profile, todayCount: todayItems.length),

                  const SizedBox(height: 14),

                  Row(
                    children: [
                      Expanded(
                        child: _StatCard(
                          label: 'حجوزات اليوم',
                          value: todayItems.length,
                          icon: Icons.event_available_rounded,
                          color: AppColors.primary,
                        ),
                      ),
                      const SizedBox(width: 10),
                      Expanded(
                        child: _StatCard(
                          label: 'القادمة اليوم',
                          value: waitingToday,
                          icon: Icons.calendar_month_rounded,
                          color: const Color(0xFF2563EB),
                        ),
                      ),
                    ],
                  ),

                  const SizedBox(height: 10),

                  Row(
                    children: [
                      Expanded(
                        child: _StatCard(
                          label: 'المكتملة',
                          value: completed,
                          icon: Icons.verified_rounded,
                          color: AppColors.success,
                        ),
                      ),
                      const SizedBox(width: 10),
                      Expanded(
                        child: _StatCard(
                          label: 'الملغاة',
                          value: cancelled,
                          icon: Icons.cancel_outlined,
                          color: AppColors.danger,
                        ),
                      ),
                    ],
                  ),

                  const SizedBox(height: 14),

                  _PremiumButton(
                    onTap: () => context.go('/doctor/subscription'),
                  ),

                  if (_subscription?.endDate != null) ...[
                    const SizedBox(height: 10),
                    _SubscriptionCountdownCard(subscription: _subscription!),
                  ],

                  const SizedBox(height: 18),

                  _SectionHeader(
                    title: 'أقرب حجز قادم',
                    action: 'عرض الكل',
                    onActionTap: () => context.go('/doctor/appointments'),
                  ),

                  const SizedBox(height: 10),

                  if (nextAppointment == null)
                    const _EmptyNextAppointment()
                  else
                    _NextAppointmentCard(item: nextAppointment),
                ],
              ),
      ),
    );
  }
}

class _DoctorHero extends StatelessWidget {
  const _DoctorHero({
    required this.profile,
    required this.todayCount,
  });

  final DoctorProfile? profile;
  final int todayCount;

  @override
  Widget build(BuildContext context) {
    final name =
        profile?.name.isNotEmpty == true ? profile!.name : 'أهلاً دكتور';

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [Color(0xFF0F7F73), Color(0xFF0D625C)],
          begin: Alignment.topRight,
          end: Alignment.bottomLeft,
        ),
        borderRadius: BorderRadius.circular(24),
        boxShadow: [
          BoxShadow(
            color: AppColors.primary.withOpacity(.18),
            blurRadius: 18,
            offset: const Offset(0, 9),
          ),
        ],
      ),
      child: Column(
        children: [
          Row(
            children: [
              CircleAvatar(
                radius: 35,
                backgroundColor: Colors.white.withOpacity(.20),
                backgroundImage: profile?.imageUrl == null
                    ? null
                    : NetworkImage(profile!.imageUrl!),
                child: profile?.imageUrl == null
                    ? const Icon(
                        Icons.person_rounded,
                        color: Colors.white,
                        size: 34,
                      )
                    : null,
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      name,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 20,
                        fontWeight: FontWeight.w900,
                      ),
                    ),
                    const SizedBox(height: 5),
                    Text(
                      profile?.specialization ?? 'إدارة حجوزاتك اليومية',
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: TextStyle(
                        color: Colors.white.withOpacity(.85),
                        fontSize: 12.5,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ],
                ),
              ),
              Container(
                width: 46,
                height: 46,
                decoration: BoxDecoration(
                  color: Colors.white.withOpacity(.16),
                  borderRadius: BorderRadius.circular(15),
                ),
                child: const Icon(
                  Icons.workspace_premium_rounded,
                  color: Color(0xFFFFD166),
                  size: 27,
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: Colors.white.withOpacity(.13),
              borderRadius: BorderRadius.circular(17),
              border: Border.all(color: Colors.white.withOpacity(.16)),
            ),
            child: Row(
              children: [
                const Icon(
                  Icons.today_rounded,
                  color: Colors.white,
                  size: 21,
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    'لديك $todayCount حجز لهذا اليوم',
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 13,
                      fontWeight: FontWeight.w900,
                    ),
                  ),
                ),
                Text(
                  DateFormat('yyyy/MM/dd').format(DateTime.now()),
                  style: TextStyle(
                    color: Colors.white.withOpacity(.82),
                    fontSize: 12,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _StatCard extends StatelessWidget {
  const _StatCard({
    required this.label,
    required this.value,
    required this.icon,
    required this.color,
  });

  final String label;
  final int value;
  final IconData icon;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 128,
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(19),
        border: Border.all(
          color: const Color(0xFFDDE9E7),
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(.025),
            blurRadius: 12,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            width: 38,
            height: 38,
            decoration: BoxDecoration(
              color: color.withOpacity(.10),
              borderRadius: BorderRadius.circular(13),
            ),
            child: Icon(
              icon,
              color: color,
              size: 22,
            ),
          ),

          const SizedBox(height: 10),

          Text(
            '$value',
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: TextStyle(
              fontSize: 25,
              fontWeight: FontWeight.w900,
              color: color,
            ),
          ),

          const SizedBox(height: 3),

          Expanded(
            child: Align(
              alignment: AlignmentDirectional.bottomStart,
              child: Text(
                label,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: TextStyle(
                  color: Colors.grey.shade600,
                  fontSize: 11.5,
                  fontWeight: FontWeight.w800,
                  height: 1.2,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _PremiumButton extends StatelessWidget {
  const _PremiumButton({required this.onTap});

  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: const Color(0xFFFFF8E7),
      borderRadius: BorderRadius.circular(18),
      child: InkWell(
        borderRadius: BorderRadius.circular(18),
        onTap: onTap,
        child: Container(
          padding: const EdgeInsets.all(14),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(18),
            border: Border.all(color: const Color(0xFFE8CF83)),
          ),
          child: Row(
            children: [
              Container(
                width: 46,
                height: 46,
                decoration: BoxDecoration(
                  color: const Color(0xFFFFE8A8),
                  borderRadius: BorderRadius.circular(15),
                ),
                child: const Icon(
                  Icons.workspace_premium_rounded,
                  color: Color(0xFFD6A20B),
                  size: 28,
                ),
              ),
              const SizedBox(width: 11),
              const Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'أنواع الاشتراكات',
                      style: TextStyle(
                        fontSize: 15,
                        fontWeight: FontWeight.w900,
                        color: Color(0xFF4A3510),
                      ),
                    ),
                    SizedBox(height: 3),
                    Text(
                      'اطّلع على الباقات والمميزات المتاحة',
                      style: TextStyle(
                        fontSize: 12,
                        fontWeight: FontWeight.w700,
                        color: Color(0xFF8A6518),
                      ),
                    ),
                  ],
                ),
              ),
              const Icon(
                Icons.chevron_right_rounded,
                color: Color(0xFFD6A20B),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _SubscriptionCountdownCard extends StatelessWidget {
  const _SubscriptionCountdownCard({required this.subscription});

  final DoctorSubscriptionInfo subscription;

  @override
  Widget build(BuildContext context) {
    final endDate = subscription.endDate!;
    final today = DateTime.now();
    final daysLeft = DateTime(endDate.year, endDate.month, endDate.day)
        .difference(DateTime(today.year, today.month, today.day))
        .inDays;
    final color = daysLeft <= 1
        ? AppColors.danger
        : daysLeft <= 3
            ? AppColors.warning
            : AppColors.success;

    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: color.withOpacity(.08),
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: color.withOpacity(.22)),
      ),
      child: Row(
        children: [
          Icon(Icons.hourglass_bottom_rounded, color: color),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              daysLeft < 0
                  ? 'انتهى الاشتراك'
                  : 'متبقي $daysLeft يوم على انتهاء الاشتراك',
              style: TextStyle(
                color: color,
                fontWeight: FontWeight.w900,
              ),
            ),
          ),
          Text(
            DateFormat('yyyy/MM/dd').format(endDate),
            style: TextStyle(color: color, fontWeight: FontWeight.w800),
          ),
        ],
      ),
    );
  }
}

class _SectionHeader extends StatelessWidget {
  const _SectionHeader({
    required this.title,
    required this.action,
    required this.onActionTap,
  });

  final String title;
  final String action;
  final VoidCallback onActionTap;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: Text(
            title,
            style: const TextStyle(
              fontSize: 17,
              fontWeight: FontWeight.w900,
            ),
          ),
        ),
        TextButton(
          onPressed: onActionTap,
          child: Text(action),
        ),
      ],
    );
  }
}

class _NextAppointmentCard extends StatelessWidget {
  const _NextAppointmentCard({required this.item});

  final DoctorAppointment item;

  @override
  Widget build(BuildContext context) {
    final isGuest = item.isGuestBooking;
    final sourceColor = isGuest ? const Color(0xFFD6A20B) : AppColors.primary;

    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(19),
        border: Border.all(color: const Color(0xFFDDE9E7)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(.025),
            blurRadius: 12,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            width: 54,
            height: 54,
            decoration: BoxDecoration(
              color: sourceColor.withOpacity(.10),
              borderRadius: BorderRadius.circular(17),
            ),
            child: Center(
              child: Text(
                '#${item.queueNumber}',
                style: TextStyle(
                  color: sourceColor,
                  fontWeight: FontWeight.w900,
                  fontSize: 16,
                ),
              ),
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  item.patientName,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontWeight: FontWeight.w900,
                    fontSize: 15.5,
                  ),
                ),
                const SizedBox(height: 5),
                Text(
                  '${DateFormat('yyyy/MM/dd').format(item.appointmentDate)} - ${item.clinicName}',
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    color: Colors.grey.shade600,
                    fontSize: 12,
                    fontWeight: FontWeight.w700,
                  ),
                ),
                const SizedBox(height: 7),
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 9, vertical: 5),
                  decoration: BoxDecoration(
                    color: sourceColor.withOpacity(.09),
                    borderRadius: BorderRadius.circular(999),
                  ),
                  child: Text(
                    isGuest ? 'حجز زائر' : 'مستخدم مسجل',
                    style: TextStyle(
                      color: sourceColor,
                      fontSize: 11,
                      fontWeight: FontWeight.w900,
                    ),
                  ),
                ),
              ],
            ),
          ),
          const Icon(
            Icons.calendar_month_rounded,
            color: AppColors.primary,
          ),
        ],
      ),
    );
  }
}

class _EmptyNextAppointment extends StatelessWidget {
  const _EmptyNextAppointment();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: const Color(0xFFDDE9E7)),
      ),
      child: Row(
        children: [
          const Icon(Icons.event_busy_rounded, color: AppColors.muted),
          const SizedBox(width: 8),
          Text(
            'لا يوجد حجز قادم حالياً.',
            style: TextStyle(
              color: Colors.grey.shade600,
              fontWeight: FontWeight.w700,
            ),
          ),
        ],
      ),
    );
  }
}
