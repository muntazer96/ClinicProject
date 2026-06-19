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
  DoctorManageProfile? _profile;
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
        _features =
            _subscription?.features ??
            await _service.getDoctorFeatures(profile.id);
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
          Container(
            margin: const EdgeInsets.fromLTRB(16, 10, 16, 8),
            padding: const EdgeInsets.all(4),
            decoration: BoxDecoration(
              color: context.appSurfaceMuted,
              borderRadius: BorderRadius.circular(16),
              border: Border.all(color: context.appBorder),
            ),
            child: TabBar(
              indicator: BoxDecoration(
                color: AppColors.primary,
                borderRadius: BorderRadius.circular(13),
              ),
              labelColor: Colors.white,
              unselectedLabelColor: context.appText,
              labelStyle: Theme.of(
                context,
              ).textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w900),
              unselectedLabelStyle: Theme.of(
                context,
              ).textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w800),
              dividerColor: Colors.transparent,
              indicatorSize: TabBarIndicatorSize.tab,
              tabs: const [
                Tab(
                  child: Text(
                    'حسابي',
                    textAlign: TextAlign.center,
                    style: TextStyle(fontSize: 14, fontWeight: FontWeight.w800),
                  ),
                ),
                Tab(
                  child: Text(
                    'بيانات الطبيب',
                    textAlign: TextAlign.center,
                    style: TextStyle(fontSize: 14, fontWeight: FontWeight.w800),
                  ),
                ),
              ],
            ),
          ),
          Expanded(
            child: TabBarView(
              children: [
                ProfileScreen(onProfileChanged: _load),
                _loading
                    ? const Center(child: CircularProgressIndicator())
                    : RefreshIndicator(
                        onRefresh: _load,
                        child: ListView(
                          padding: const EdgeInsets.fromLTRB(16, 8, 16, 28),
                          children: [
                            _DoctorProfileCard(profile: _profile),

                            const SizedBox(height: 14),

                            SizedBox(
                              height: 48,
                              child: ElevatedButton.icon(
                                onPressed: () async {
                                  await context.push(
                                    '/doctor/profile/edit',
                                    extra: _profile,
                                  );
                                  await _load();
                                },
                                icon: const Icon(Icons.edit_outlined),
                                label: const Text(
                                  'تعديل بيانات الطبيب',
                                  style: TextStyle(fontWeight: FontWeight.w900),
                                ),
                                style: ElevatedButton.styleFrom(
                                  backgroundColor: AppColors.primary,
                                  foregroundColor: Colors.white,
                                  elevation: 0,
                                  shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(15),
                                  ),
                                ),
                              ),
                            ),

                            const SizedBox(height: 14),

                            _QuickTile(
                              icon: Icons.star_rounded,
                              title: 'التقييمات',
                              subtitle: 'عرض تقييمات المرضى والردود',
                              color: AppColors.warning,
                              onTap: () => context.push('/doctor/reviews'),
                            ),

                            const SizedBox(height: 14),

                            _QuickTile(
                              icon: Icons.chat_bubble_rounded,
                              title: 'الرسائل',
                              subtitle: 'عرض واستعراض الرسائل مع المرضى',
                              color: AppColors.primary,
                              onTap: () => context.push('/doctor/messages'),
                            ),

                            const SizedBox(height: 14),

                            _SubscriptionCard(subscription: _subscription),

                            const SizedBox(height: 14),

                            _FeaturesCard(
                              features: _features,
                              onManage: () => context.push(
                                '/doctor/features',
                                extra: _profile,
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

  final DoctorManageProfile? profile;

  @override
  Widget build(BuildContext context) {
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
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(3),
            decoration: BoxDecoration(
              color: Colors.white.withOpacity(.22),
              shape: BoxShape.circle,
            ),
            child: CircleAvatar(
              radius: 39,
              backgroundColor: Colors.white.withOpacity(.18),
              child: ClipOval(
                child: profile?.imageUrl == null
                    ? const _DoctorAvatarFallback(size: 78, iconSize: 34)
                    : Image.network(
                        profile!.imageUrl!,
                        width: 78,
                        height: 78,
                        fit: BoxFit.cover,
                        errorBuilder: (_, __, ___) =>
                            const _DoctorAvatarFallback(size: 78, iconSize: 34),
                      ),
              ),
            ),
          ),
          const SizedBox(width: 13),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  profile?.name ?? '-',
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 20,
                    fontWeight: FontWeight.w900,
                  ),
                ),
                const SizedBox(height: 5),
                _WhitePill(
                  icon: Icons.workspace_premium_rounded,
                  text: profile?.specialization ?? '-',
                ),
                if (profile?.description.isNotEmpty == true) ...[
                  const SizedBox(height: 8),
                  Text(
                    profile!.description,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: TextStyle(
                      color: Colors.white.withOpacity(.84),
                      fontSize: 12.5,
                      height: 1.4,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _WhitePill extends StatelessWidget {
  const _WhitePill({required this.icon, required this.text});

  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 6),
      decoration: BoxDecoration(
        color: Colors.white.withOpacity(.16),
        borderRadius: BorderRadius.circular(999),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, color: Colors.white, size: 15),
          const SizedBox(width: 5),
          Flexible(
            child: Text(
              text,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 11.5,
                fontWeight: FontWeight.w800,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _DoctorAvatarFallback extends StatelessWidget {
  const _DoctorAvatarFallback({required this.size, required this.iconSize});

  final double size;
  final double iconSize;

  @override
  Widget build(BuildContext context) => Container(
    width: size,
    height: size,
    alignment: Alignment.center,
    color: Colors.white.withOpacity(.12),
    child: Icon(
      Icons.medical_services_rounded,
      color: context.appSurface,
      size: iconSize,
    ),
  );
}

class _QuickTile extends StatelessWidget {
  const _QuickTile({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.color,
    required this.onTap,
  });

  final IconData icon;
  final String title;
  final String subtitle;
  final Color color;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: context.appSurface,
      borderRadius: BorderRadius.circular(18),
      child: InkWell(
        borderRadius: BorderRadius.circular(18),
        onTap: onTap,
        child: Container(
          padding: const EdgeInsets.all(14),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(18),
            border: Border.all(color: context.appBorder),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withOpacity(context.isDark ? .18 : .025),
                blurRadius: 12,
                offset: const Offset(0, 6),
              ),
            ],
          ),
          child: Row(
            children: [
              Container(
                width: 46,
                height: 46,
                decoration: BoxDecoration(
                  color: color.withOpacity(.12),
                  borderRadius: BorderRadius.circular(15),
                ),
                child: Icon(icon, color: color, size: 27),
              ),
              const SizedBox(width: 11),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      title,
                      style: const TextStyle(
                        fontWeight: FontWeight.w900,
                        fontSize: 15,
                      ),
                    ),
                    const SizedBox(height: 3),
                    Text(
                      subtitle,
                      style: TextStyle(
                        color: context.appMuted,
                        fontSize: 12,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
              ),
              Icon(Icons.chevron_right_rounded, color: context.appMuted),
            ],
          ),
        ),
      ),
    );
  }
}

class _SubscriptionCard extends StatelessWidget {
  const _SubscriptionCard({required this.subscription});

  final DoctorSubscriptionInfo? subscription;

  @override
  Widget build(BuildContext context) {
    final hasSub = subscription != null;
    final package = subscription?.package;
    final packageName = subscription?.packageArabicName.isNotEmpty == true
        ? subscription!.packageArabicName
        : subscription?.packageName ?? '-';
    final englishName = subscription?.packageEnglishName.isNotEmpty == true
        ? subscription!.packageEnglishName
        : subscription?.packageNormalizedName ?? '';

    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: context.appSurface,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: context.appBorder),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(context.isDark ? .18 : .025),
            blurRadius: 12,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              const Icon(
                Icons.workspace_premium_rounded,
                color: Color(0xFFD6A20B),
                size: 22,
              ),
              const SizedBox(width: 7),
              const Expanded(
                child: Text(
                  'الاشتراك الحالي',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.w900),
                ),
              ),
              _SmallBadge(
                text: hasSub ? 'فعال' : 'غير متاح',
                color: hasSub ? AppColors.primary : context.appMuted,
              ),
            ],
          ),
          const SizedBox(height: 13),
          if (!hasSub)
            Text(
              'لا توجد بيانات اشتراك متاحة لهذا الحساب.',
              style: TextStyle(color: context.appMuted),
            )
          else ...[
            Text(
              packageName,
              style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w900),
            ),
            if (englishName.isNotEmpty) ...[
              const SizedBox(height: 3),
              Text(
                englishName,
                style: TextStyle(
                  color: context.appMuted,
                  fontSize: 12.5,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ],
            const SizedBox(height: 10),
            Row(
              children: [
                Expanded(
                  child: _DateInfoBox(
                    title: 'التفعيل',
                    date: _format(subscription!.startDate),
                    icon: Icons.play_circle_outline_rounded,
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: _DateInfoBox(
                    title: 'الانتهاء',
                    date: _format(subscription!.endDate),
                    icon: Icons.event_busy_rounded,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 10),
            Row(
              children: [
                Expanded(
                  child: _SubscriptionMetric(
                    icon: Icons.payments_outlined,
                    title: 'السعر',
                    value: _money(package?.price ?? 0),
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: _SubscriptionMetric(
                    icon: Icons.workspace_premium_outlined,
                    title: 'السعر السنوي',
                    value: _money(package?.yearlyPrice ?? 0),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: [
                _FeaturePill(text: 'العيادات: ${package?.maxClinics ?? 0}'),
                _FeaturePill(
                  text: 'حجوزات يومية: ${package?.maxDailyAppointments ?? 0}',
                ),
                _FeaturePill(
                  text: 'أيام أسبوعية: ${package?.maxWeeklyDays ?? 0}',
                ),
                _FeaturePill(
                  text: 'العروض النشطة: ${package?.maxActiveOffers ?? 0}',
                ),
                if (package?.showReviews == true)
                  const _FeaturePill(text: 'إظهار التقييمات'),
                if (package?.showMessages == true)
                  const _FeaturePill(text: 'الرسائل'),
                if (package?.eBooking == true)
                  const _FeaturePill(text: 'الحجز الإلكتروني'),
                if (package?.ePayments == true)
                  const _FeaturePill(text: 'الدفع الإلكتروني'),
                if (package?.makeOffers == true)
                  const _FeaturePill(text: 'إنشاء العروض'),
              ],
            ),
          ],
        ],
      ),
    );
  }

  String _format(DateTime? value) =>
      value == null ? '-' : DateFormat('yyyy/MM/dd').format(value);

  String _money(double value) => value <= 0
      ? '-'
      : NumberFormat.currency(
          locale: 'ar_IQ',
          symbol: 'د.ع',
          decimalDigits: 0,
        ).format(value);
}

class _SubscriptionMetric extends StatelessWidget {
  const _SubscriptionMetric({
    required this.icon,
    required this.title,
    required this.value,
  });

  final IconData icon;
  final String title;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(11),
      decoration: BoxDecoration(
        color: context.appSurfaceMuted,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: context.appBorder),
      ),
      child: Row(
        children: [
          Icon(icon, color: AppColors.primary, size: 18),
          const SizedBox(width: 7),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: TextStyle(fontSize: 11, color: context.appMuted),
                ),
                const SizedBox(height: 2),
                Text(
                  value,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontSize: 12.5,
                    fontWeight: FontWeight.w900,
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

class _DateInfoBox extends StatelessWidget {
  const _DateInfoBox({
    required this.title,
    required this.date,
    required this.icon,
  });

  final String title;
  final String date;
  final IconData icon;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(11),
      decoration: BoxDecoration(
        color: context.appSurfaceMuted,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: context.appBorder),
      ),
      child: Row(
        children: [
          Icon(icon, color: AppColors.primary, size: 18),
          const SizedBox(width: 7),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: TextStyle(fontSize: 11, color: context.appMuted),
                ),
                const SizedBox(height: 2),
                Text(
                  date,
                  style: const TextStyle(
                    fontSize: 12.5,
                    fontWeight: FontWeight.w900,
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

class _FeaturesCard extends StatelessWidget {
  const _FeaturesCard({required this.features, required this.onManage});

  final List<DoctorFeatureItem> features;
  final VoidCallback onManage;

  @override
  Widget build(BuildContext context) {
    final enabledFeatures = features.where((item) => item.isEnabled).toList();

    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: context.appSurface,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: context.appBorder),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(context.isDark ? .18 : .025),
            blurRadius: 12,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              const Icon(
                Icons.auto_awesome_rounded,
                color: AppColors.primary,
                size: 21,
              ),
              const SizedBox(width: 7),
              const Expanded(
                child: Text(
                  'المميزات المتاحة',
                  style: TextStyle(fontWeight: FontWeight.w900, fontSize: 16),
                ),
              ),
              TextButton(onPressed: onManage, child: const Text('إدارة')),
            ],
          ),
          const SizedBox(height: 10),
          if (enabledFeatures.isEmpty)
            Text(
              'لا توجد بيانات مميزات متاحة من صلاحيات الـ API الحالية.',
              style: TextStyle(color: context.appMuted),
            )
          else
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: enabledFeatures
                  .map((item) => _FeaturePill(text: item.name))
                  .toList(),
            ),
        ],
      ),
    );
  }
}

class _FeaturePill extends StatelessWidget {
  const _FeaturePill({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 7),
      decoration: BoxDecoration(
        color: context.appSoftBlue,
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: AppColors.primary.withOpacity(.16)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Icon(
            Icons.check_circle_rounded,
            size: 15,
            color: AppColors.primary,
          ),
          const SizedBox(width: 5),
          Text(
            text,
            style: TextStyle(
              color: Theme.of(context).colorScheme.primary,
              fontSize: 12,
              fontWeight: FontWeight.w800,
            ),
          ),
        ],
      ),
    );
  }
}

class _SmallBadge extends StatelessWidget {
  const _SmallBadge({required this.text, required this.color});

  final String text;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: color.withOpacity(.09),
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: color.withOpacity(.18)),
      ),
      child: Text(
        text,
        style: TextStyle(
          color: color,
          fontWeight: FontWeight.w900,
          fontSize: 11.5,
        ),
      ),
    );
  }
}
