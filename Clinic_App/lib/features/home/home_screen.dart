import 'dart:async';

import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../core/app_theme.dart';
import '../../widgets/app_scaffold.dart';
import '../auth/auth_controller.dart';
import '../directory/directory_service.dart';
import '../directory/models/directory_models.dart';
import '../directory/specialization_icons.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final _service = DirectoryService();
  static const _initialOfferPage = 1000;
  final _offersController = PageController(
    viewportFraction: .92,
    initialPage: _initialOfferPage,
  );
  List<Specialization> _specializations = [];
  List<DoctorOffer> _offers = [];
  bool _loadingSpecializations = true;
  bool _loadingOffers = true;
  int _currentOfferPage = _initialOfferPage;
  Timer? _offersTimer;

  @override
  void initState() {
    super.initState();
    _loadSpecializations();
    _loadOffers();
  }

  @override
  void dispose() {
    _offersTimer?.cancel();
    _offersController.dispose();
    super.dispose();
  }

  Future<void> _loadSpecializations() async {
    try {
      final items = await _service.getSpecializations();
      if (mounted) setState(() => _specializations = items);
    } catch (_) {
      // The home page stays usable even if the shortcut lookup fails.
    } finally {
      if (mounted) setState(() => _loadingSpecializations = false);
    }
  }

  Future<void> _loadOffers() async {
    try {
      final result = await _service.getOffers(pageSize: 5);
      if (!mounted) return;
      setState(() {
        _offers = result.items;
        _currentOfferPage = _initialOfferPage;
      });
      if (_offersController.hasClients && result.items.isNotEmpty) {
        _offersController.jumpToPage(_initialOfferPage);
      }
      _startOfferAutoScroll();
    } catch (_) {
      // Offers are promotional content, so the home page remains usable.
    } finally {
      if (mounted) setState(() => _loadingOffers = false);
    }
  }

  void _startOfferAutoScroll() {
    _offersTimer?.cancel();
    if (_offers.length < 2) return;
    _offersTimer = Timer.periodic(const Duration(seconds: 4), (_) {
      if (!mounted || !_offersController.hasClients || _offers.isEmpty) return;
      final nextPage = _currentOfferPage + 1;
      _offersController.animateToPage(
        nextPage,
        duration: const Duration(milliseconds: 520),
        curve: Curves.easeOutCubic,
      );
    });
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthController>();

    return AppScaffold(
      child: RefreshIndicator(
        onRefresh: () async {
          await auth.refreshProfile(silent: true);
          await Future.wait([_loadSpecializations(), _loadOffers()]);
        },
        child: ListView(
          padding: const EdgeInsets.fromLTRB(16, 12, 16, 28),
          children: [
            _HeroPanel(auth: auth),
            const SizedBox(height: 14),
            FilledButton.icon(
              onPressed: () => context.go('/search'),
              icon: const Icon(Icons.event_available_rounded),
              label: const Text('ابدأ بالحجز الآن'),
            ),
            const SizedBox(height: 14),
            _SearchCard(onTap: () => context.go('/search')),
            const SizedBox(height: 22),
            _OffersBanner(
              loading: _loadingOffers,
              offers: _offers,
              controller: _offersController,
              onPageChanged: (index) => _currentOfferPage = index,
              onTap: () => context.go('/offers'),
            ),
            const SizedBox(height: 22),
            _SectionTitle(
              title: 'الاختصاصات المتوفرة',
              action: 'عرض المزيد',
              onAction: () => context.go('/specializations'),
            ),
            const SizedBox(height: 10),
            _SpecializationPreview(
              loading: _loadingSpecializations,
              items: _specializations.take(3).toList(),
            ),
            const SizedBox(height: 22),
            const _SectionTitle(title: 'خدمات سريعة'),
            const SizedBox(height: 10),
            const _ServiceList(),
          ],
        ),
      ),
    );
  }
}

class _HeroPanel extends StatelessWidget {
  const _HeroPanel({required this.auth});
  final AuthController auth;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(18),
    decoration: BoxDecoration(
      color: AppColors.primaryDark,
      borderRadius: BorderRadius.circular(8),
      boxShadow: const [
        BoxShadow(
          color: Color(0x1F155E75),
          blurRadius: 22,
          offset: Offset(0, 10),
        ),
      ],
    ),
    child: Row(
      children: [
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                auth.isAuthenticated
                    ? 'أهلاً، ${auth.displayName}'
                    : 'أهلاً بك في عيادتي',
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 23,
                  fontWeight: FontWeight.w900,
                ),
              ),
              const SizedBox(height: 6),
              const Text(
                'احجز، تابع مواعيدك، وعدّل بياناتك من مكان واحد.',
                style: TextStyle(
                  color: Color(0xFFD9F2EE),
                  fontSize: 13,
                  height: 1.6,
                ),
              ),
            ],
          ),
        ),
        const SizedBox(width: 12),
        Container(
          width: 54,
          height: 54,
          decoration: BoxDecoration(
            color: Colors.white.withValues(alpha: .14),
            borderRadius: BorderRadius.circular(8),
            border: Border.all(color: Colors.white24),
          ),
          child: const Icon(
            Icons.health_and_safety_rounded,
            color: Colors.white,
            size: 30,
          ),
        ),
      ],
    ),
  );
}

class _SearchCard extends StatelessWidget {
  const _SearchCard({required this.onTap});
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => InkWell(
    onTap: onTap,
    borderRadius: BorderRadius.circular(8),
    child: Container(
      height: 58,
      padding: const EdgeInsets.symmetric(horizontal: 14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: AppColors.border),
      ),
      child: const Row(
        children: [
          Icon(Icons.search_rounded, color: AppColors.primary),
          SizedBox(width: 10),
          Expanded(
            child: Text(
              'ابحث عن طبيب',
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: TextStyle(
                color: AppColors.muted,
                fontWeight: FontWeight.w800,
              ),
            ),
          ),
          Icon(Icons.arrow_back_rounded, color: AppColors.text),
        ],
      ),
    ),
  );
}

class _OffersBanner extends StatelessWidget {
  const _OffersBanner({
    required this.loading,
    required this.offers,
    required this.controller,
    required this.onPageChanged,
    required this.onTap,
  });

  final bool loading;
  final List<DoctorOffer> offers;
  final PageController controller;
  final ValueChanged<int> onPageChanged;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    if (loading) {
      return Container(
        height: 154,
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(8),
          border: Border.all(color: AppColors.border),
        ),
        child: const Center(child: CircularProgressIndicator()),
      );
    }

    if (offers.isEmpty) return const SizedBox.shrink();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _SectionTitle(
          title: 'العروض الفعالة',
          action: 'عرض الكل',
          onAction: onTap,
        ),
        const SizedBox(height: 10),
        SizedBox(
          height: 166,
          child: Directionality(
            textDirection: TextDirection.rtl,
            child: PageView.builder(
              controller: controller,
              itemCount: offers.length > 1 ? null : offers.length,
              onPageChanged: onPageChanged,
              itemBuilder: (context, index) => Padding(
                padding: const EdgeInsetsDirectional.only(end: 8),
                child: _HomeOfferSlide(
                  offer: offers[index % offers.length],
                  onTap: onTap,
                ),
              ),
            ),
          ),
        ),
      ],
    );
  }
}

class _HomeOfferSlide extends StatelessWidget {
  const _HomeOfferSlide({required this.offer, required this.onTap});

  final DoctorOffer offer;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final featured = offer.isFeatured;
    const premiumColor = Color(0xFFD49A00);
    final accent = featured ? premiumColor : AppColors.primary;
    final iconBackground =
        featured ? const Color(0xFFFFF4D8) : const Color(0xFFEAF6F8);
    final borderColor =
        featured ? const Color(0xFFE4B23C) : AppColors.border;

    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(8),
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          gradient: featured
              ? const LinearGradient(
                  begin: Alignment.topRight,
                  end: Alignment.bottomLeft,
                  colors: [Color(0xFFFFF8DE), Color(0xFFFFFFFF)],
                )
              : null,
          color: featured ? null : Colors.white,
          borderRadius: BorderRadius.circular(8),
          border: Border.all(color: borderColor, width: featured ? 1.1 : 1),
          boxShadow: [
            BoxShadow(
              color: (featured ? premiumColor : AppColors.primary)
                  .withValues(alpha: featured ? .13 : .07),
              blurRadius: featured ? 22 : 16,
              offset: const Offset(0, 10),
            ),
          ],
        ),
        child: Row(
          children: [
            Container(
              width: 58,
              height: 58,
              decoration: BoxDecoration(
                color: iconBackground,
                borderRadius: BorderRadius.circular(8),
                border: Border.all(
                  color: featured ? const Color(0xFFE8C76D) : AppColors.border,
                ),
              ),
              child: Icon(
                featured
                    ? Icons.workspace_premium_rounded
                    : Icons.local_offer_rounded,
                color: accent,
                size: 31,
              ),
            ),
            const SizedBox(width: 13),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          offer.priceText,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: TextStyle(
                            color: accent,
                            fontSize: 18,
                            fontWeight: FontWeight.w900,
                          ),
                        ),
                      ),
                      if (featured)
                        const _OfferSlideBadge(
                          text: 'طبيب مميز',
                          featured: true,
                        )
                      else if (offer.badgeText?.isNotEmpty == true)
                        _OfferSlideBadge(text: offer.badgeText!),
                    ],
                  ),
                  const SizedBox(height: 6),
                  Text(
                    offer.title,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: AppColors.text,
                      fontWeight: FontWeight.w900,
                    ),
                  ),
                  const SizedBox(height: 5),
                  Text(
                    offer.doctorName,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: TextStyle(
                      color: accent,
                      fontWeight: FontWeight.w800,
                    ),
                  ),
                  const SizedBox(height: 9),
                  Row(
                    children: [
                      const Icon(
                        Icons.schedule_rounded,
                        size: 16,
                        color: AppColors.muted,
                      ),
                      const SizedBox(width: 4),
                      Expanded(
                        child: Text(
                          '${offer.remainingDays} يوم متبقي - ${offer.scope}',
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(
                            color: AppColors.muted,
                            fontSize: 12,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _OfferSlideBadge extends StatelessWidget {
  const _OfferSlideBadge({required this.text, this.featured = false});

  final String text;
  final bool featured;

  @override
  Widget build(BuildContext context) {
    final color = featured ? const Color(0xFFD49A00) : AppColors.primary;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: color.withValues(alpha: .45)),
      ),
      child: Text(
        text,
        style: TextStyle(
          color: color,
          fontSize: 11,
          fontWeight: FontWeight.w900,
        ),
      ),
    );
  }
}

class _SpecializationPreview extends StatelessWidget {
  const _SpecializationPreview({required this.loading, required this.items});

  final bool loading;
  final List<Specialization> items;

  @override
  Widget build(BuildContext context) {
    if (loading) {
      return const Padding(
        padding: EdgeInsets.all(24),
        child: Center(child: CircularProgressIndicator()),
      );
    }
    if (items.isEmpty) {
      return const _EmptySpecializations();
    }
    return Column(
      children: items
          .map(
            (item) => Padding(
              padding: const EdgeInsets.only(bottom: 10),
              child: _SpecializationTile(item: item),
            ),
          )
          .toList(),
    );
  }
}

class _SpecializationTile extends StatelessWidget {
  const _SpecializationTile({required this.item});
  final Specialization item;

  @override
  Widget build(BuildContext context) => InkWell(
    onTap: () => context.go('/search?specialization=${item.id}'),
    borderRadius: BorderRadius.circular(8),
    child: Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: AppColors.border),
      ),
      child: Row(
        children: [
          Container(
            width: 42,
            height: 42,
            decoration: BoxDecoration(
              color: AppColors.softBlue,
              borderRadius: BorderRadius.circular(8),
            ),
            child: Icon(
              specializationIconFor(item.iconName),
              color: AppColors.primary,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              item.name,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: const TextStyle(fontWeight: FontWeight.w900),
            ),
          ),
          const Icon(Icons.arrow_back_ios_new_rounded, size: 16),
        ],
      ),
    ),
  );
}

class _EmptySpecializations extends StatelessWidget {
  const _EmptySpecializations();

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(16),
    decoration: BoxDecoration(
      color: Colors.white,
      borderRadius: BorderRadius.circular(8),
      border: Border.all(color: AppColors.border),
    ),
    child: const Text(
      'لا توجد اختصاصات متاحة حالياً.',
      textAlign: TextAlign.center,
      style: TextStyle(color: AppColors.muted),
    ),
  );
}

class _SectionTitle extends StatelessWidget {
  const _SectionTitle({required this.title, this.action, this.onAction});
  final String title;
  final String? action;
  final VoidCallback? onAction;

  @override
  Widget build(BuildContext context) => Row(
    children: [
      Expanded(
        child: Text(
          title,
          style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w900),
        ),
      ),
      if (action != null)
        TextButton(
          onPressed: onAction,
          child: Text(action!, style: const TextStyle(fontSize: 12)),
        ),
    ],
  );
}

class _ServiceList extends StatelessWidget {
  const _ServiceList();

  @override
  Widget build(BuildContext context) => Column(
    children: [
      _ServiceRow(
        icon: Icons.calendar_month_rounded,
        title: 'حجوزاتي',
        subtitle: 'تابع مواعيدك الحالية والسابقة.',
        color: AppColors.softAmber,
        onTap: () => context.go('/bookings'),
      ),
      const SizedBox(height: 10),
      _ServiceRow(
        icon: Icons.manage_search_rounded,
        title: 'متابعة حجز زائر',
        subtitle: 'اعرض الحجز أو ألغِه باستخدام الهاتف والكود.',
        color: AppColors.softCoral,
        onTap: () => context.go('/guest-booking'),
      ),
      const SizedBox(height: 10),
      _ServiceRow(
        icon: Icons.account_circle_rounded,
        title: 'الملف الشخصي',
        subtitle: 'عرض بيانات الحساب وتعديل الاسم.',
        color: AppColors.surfaceMuted,
        onTap: () => context.go('/profile'),
      ),
    ],
  );
}

class _ServiceRow extends StatelessWidget {
  const _ServiceRow({
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
  Widget build(BuildContext context) => InkWell(
    onTap: onTap,
    borderRadius: BorderRadius.circular(8),
    child: Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: AppColors.border),
      ),
      child: Row(
        children: [
          Container(
            width: 42,
            height: 42,
            decoration: BoxDecoration(
              color: color,
              borderRadius: BorderRadius.circular(8),
            ),
            child: Icon(icon, color: AppColors.primaryDark),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: const TextStyle(fontWeight: FontWeight.w900),
                ),
                const SizedBox(height: 3),
                Text(
                  subtitle,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    color: AppColors.muted,
                    fontSize: 12,
                    height: 1.5,
                  ),
                ),
              ],
            ),
          ),
          const Icon(Icons.arrow_back_ios_new_rounded, size: 16),
        ],
      ),
    ),
  );
}
