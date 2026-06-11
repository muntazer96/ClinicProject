import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorSubscriptionScreen extends StatefulWidget {
  const DoctorSubscriptionScreen({super.key});

  @override
  State<DoctorSubscriptionScreen> createState() =>
      _DoctorSubscriptionScreenState();
}

class _DoctorSubscriptionScreenState extends State<DoctorSubscriptionScreen> {
  late final DoctorService _service;
  List<SubscriptionPackage> _items = [];
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
      _items = await _service.getSubscriptionPackages();
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
        title: 'الاشتراكات',
        child: RefreshIndicator(
          onRefresh: _load,
          child: _loading
              ? const Center(child: CircularProgressIndicator())
              : ListView.separated(
                  padding: const EdgeInsets.fromLTRB(14, 14, 14, 28),
                  itemCount: _items.length,
                  separatorBuilder: (_, __) => const SizedBox(height: 14),
                  itemBuilder: (context, index) {
                    return _SubscriptionBanner(item: _items[index]);
                  },
                ),
        ),
      );
}

class _SubscriptionBanner extends StatelessWidget {
  const _SubscriptionBanner({required this.item});

  final SubscriptionPackage item;

  @override
  Widget build(BuildContext context) {
    final style = _PackageStyle.from(item.normalizedName);

    return Container(
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: style.gradient,
          begin: Alignment.topRight,
          end: Alignment.bottomLeft,
        ),
        borderRadius: BorderRadius.circular(22),
        border: Border.all(color: style.borderColor),
        boxShadow: [
          BoxShadow(
            color: style.shadowColor.withOpacity(.16),
            blurRadius: 18,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Stack(
        children: [
          Positioned(
            left: -18,
            top: -18,
            child: Icon(
              style.icon,
              size: 120,
              color: style.accent.withOpacity(.08),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Row(
                  children: [
                    _IconBox(style: style),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            item.name,
                            style: TextStyle(
                              fontSize: 22,
                              fontWeight: FontWeight.w900,
                              color: style.titleColor,
                            ),
                          ),
                          const SizedBox(height: 4),
                          Text(
                            style.subtitle,
                            style: TextStyle(
                              fontSize: 13,
                              color: style.titleColor.withOpacity(.72),
                              fontWeight: FontWeight.w700,
                            ),
                          ),
                        ],
                      ),
                    ),
                    _Badge(text: style.badge, color: style.accent),
                  ],
                ),

                const SizedBox(height: 18),

                Row(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Text(
                      item.price == 0
                          ? 'مجاني'
                          : '${item.price.toStringAsFixed(0)} د.ع',
                      style: TextStyle(
                        fontSize: 25,
                        fontWeight: FontWeight.w900,
                        color: style.accent,
                      ),
                    ),
                    const SizedBox(width: 6),
                    Padding(
                      padding: const EdgeInsets.only(bottom: 4),
                      child: Text(
                        '/ شهرياً',
                        style: TextStyle(
                          fontSize: 12,
                          color: style.titleColor.withOpacity(.55),
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ),
                  ],
                ),

                if (item.yearlyPrice > 0) ...[
                  const SizedBox(height: 5),
                  Text(
                    'السعر السنوي: ${item.yearlyPrice.toStringAsFixed(0)} د.ع',
                    style: TextStyle(
                      fontSize: 12,
                      color: style.titleColor.withOpacity(.58),
                    ),
                  ),
                ],

                const SizedBox(height: 16),

                Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  children: [
                    _FeaturePill(
                      icon: Icons.local_hospital_outlined,
                      text: '${item.maxClinics} عيادة',
                      color: style.accent,
                    ),
                    _FeaturePill(
                      icon: Icons.event_available_outlined,
                      text: '${item.maxDailyAppointments} حجز يومي',
                      color: style.accent,
                    ),
                    _FeaturePill(
                      icon: Icons.calendar_month_outlined,
                      text: '${item.maxWeeklyDays} أيام دوام',
                      color: style.accent,
                    ),
                    if (item.showReviews)
                      _FeaturePill(
                        icon: Icons.star_rounded,
                        text: 'تقييمات',
                        color: style.accent,
                      ),
                    if (item.eBooking)
                      _FeaturePill(
                        icon: Icons.public,
                        text: 'حجز إلكتروني',
                        color: style.accent,
                      ),
                    if (item.makeOffers)
                      _FeaturePill(
                        icon: Icons.local_offer_rounded,
                        text: '${item.maxActiveOffers} عروض',
                        color: style.accent,
                      ),
                  ],
                ),

                const SizedBox(height: 16),

                SizedBox(
                  height: 46,
                  child: ElevatedButton.icon(
                    onPressed: () {
                      // TODO: افتح صفحة الدفع او طلب الاشتراك
                    },
                    icon: const Icon(Icons.arrow_back_rounded),
                    label: Text(
                      item.price == 0 ? 'الباقة الحالية' : 'اختيار الاشتراك',
                      style: const TextStyle(
                        fontWeight: FontWeight.w900,
                        fontSize: 15,
                      ),
                    ),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: style.accent,
                      foregroundColor: Colors.white,
                      elevation: 0,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(14),
                      ),
                    ),
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

class _PackageStyle {
  final List<Color> gradient;
  final Color accent;
  final Color borderColor;
  final Color shadowColor;
  final Color titleColor;
  final IconData icon;
  final String subtitle;
  final String badge;

  const _PackageStyle({
    required this.gradient,
    required this.accent,
    required this.borderColor,
    required this.shadowColor,
    required this.titleColor,
    required this.icon,
    required this.subtitle,
    required this.badge,
  });

  factory _PackageStyle.from(String normalizedName) {
    final key = normalizedName.trim().toLowerCase();

    if (key == 'basic') {
      return const _PackageStyle(
        gradient: [Color(0xFFFFFFFF), Color(0xFFF3F8F7)],
        accent: Color(0xFF607D7A),
        borderColor: Color(0xFFDDE8E6),
        shadowColor: Color(0xFF607D7A),
        titleColor: Color(0xFF172524),
        icon: Icons.spa_outlined,
        subtitle: 'بداية بسيطة لإدارة عيادتك',
        badge: 'أساسي',
      );
    }

    if (key == 'gold') {
      return const _PackageStyle(
        gradient: [Color(0xFFFFFCF3), Color(0xFFFFF1C7)],
        accent: Color(0xFFD6A20B),
        borderColor: Color(0xFFE8C866),
        shadowColor: Color(0xFFD6A20B),
        titleColor: Color(0xFF2B2415),
        icon: Icons.workspace_premium_rounded,
        subtitle: 'ظهور أفضل ومميزات أكثر',
        badge: 'ذهبي',
      );
    }

    if (key == 'diamond') {
      return const _PackageStyle(
        gradient: [Color(0xFFF8FDFF), Color(0xFFE8F8FF)],
        accent: Color(0xFF1697B7),
        borderColor: Color(0xFF9DDDEA),
        shadowColor: Color(0xFF1697B7),
        titleColor: Color(0xFF10282E),
        icon: Icons.diamond_outlined,
        subtitle: 'حجز إلكتروني وعروض وميزات متقدمة',
        badge: 'ألماس',
      );
    }

    return const _PackageStyle(
      gradient: [Color(0xFFFFFBF0), Color(0xFFFFE8A8)],
      accent: Color(0xFFC99500),
      borderColor: Color(0xFFE0B530),
      shadowColor: Color(0xFFC99500),
      titleColor: Color(0xFF1E1A10),
      icon: Icons.emoji_events_rounded,
      subtitle: 'أعلى ظهور وأقوى مميزات للطبيب',
      badge: 'فاخر',
    );
  }
}

class _IconBox extends StatelessWidget {
  const _IconBox({required this.style});

  final _PackageStyle style;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 54,
      height: 54,
      decoration: BoxDecoration(
        color: Colors.white.withOpacity(.78),
        borderRadius: BorderRadius.circular(17),
        border: Border.all(color: style.borderColor),
      ),
      child: Icon(style.icon, color: style.accent, size: 30),
    );
  }
}

class _Badge extends StatelessWidget {
  const _Badge({required this.text, required this.color});

  final String text;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 7),
      decoration: BoxDecoration(
        color: color.withOpacity(.12),
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: color.withOpacity(.35)),
      ),
      child: Text(
        text,
        style: TextStyle(
          color: color,
          fontWeight: FontWeight.w900,
          fontSize: 12,
        ),
      ),
    );
  }
}

class _FeaturePill extends StatelessWidget {
  const _FeaturePill({
    required this.icon,
    required this.text,
    required this.color,
  });

  final IconData icon;
  final String text;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 9),
      decoration: BoxDecoration(
        color: Colors.white.withOpacity(.72),
        borderRadius: BorderRadius.circular(13),
        border: Border.all(color: color.withOpacity(.16)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 16, color: color),
          const SizedBox(width: 5),
          Text(
            text,
            style: const TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w800,
            ),
          ),
        ],
      ),
    );
  }
}