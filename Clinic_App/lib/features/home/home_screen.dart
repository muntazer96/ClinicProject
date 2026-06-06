import 'dart:async';

import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../core/analytics_service.dart';
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
  late final AnalyticsService _analytics;
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
    _analytics = AnalyticsService(context.read<AuthController>().api);
    _analytics.trackLater(eventType: 'page_viewed', page: 'home');
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
      _trackOfferViewed(_initialOfferPage);
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
              onPageChanged: (index) {
                _currentOfferPage = index;
                _trackOfferViewed(index);
              },
              onViewAll: () {
                _analytics.trackLater(
                  eventType: 'page_viewed',
                  source: 'home_offers_view_all',
                  page: 'offers',
                );
                context.go('/offers');
              },
              onOfferTap: (offer) {
                _analytics.trackLater(
                  eventType: 'offer_clicked',
                  doctorId: offer.doctorId,
                  offerId: offer.id,
                  source: 'home_banner',
                  page: 'home',
                );
                context.push('/doctors/${offer.doctorId}?source=offer&offerId=${offer.id}');
              },
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

  void _trackOfferViewed(int pageIndex) {
    if (_offers.isEmpty) return;
    final offer = _offers[pageIndex % _offers.length];
    _analytics.trackOnce(
      key: 'home-offer-${offer.id}',
      eventType: 'offer_viewed',
      doctorId: offer.doctorId,
      offerId: offer.id,
      source: 'home_banner',
      page: 'home',
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
    required this.onViewAll,
    required this.onOfferTap,
  });

  final bool loading;
  final List<DoctorOffer> offers;
  final PageController controller;
  final ValueChanged<int> onPageChanged;
  final VoidCallback onViewAll;
  final ValueChanged<DoctorOffer> onOfferTap;

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
          onAction: onViewAll,
        ),
        const SizedBox(height: 10),
        SizedBox(
          height: 210,
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
                  onTap: () => onOfferTap(offers[index % offers.length]),
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
    final palette = featured ? _OfferPalette.premium : _OfferPalette.standard;

    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(8),
      child: Container(
        clipBehavior: Clip.antiAlias,
        decoration: BoxDecoration(
          color: palette.surface,
          borderRadius: BorderRadius.circular(8),
          border: Border.all(color: palette.border, width: 1.1),
          boxShadow: [
            BoxShadow(
              color: palette.shadow,
              blurRadius: 18,
              offset: const Offset(0, 8),
            ),
          ],
        ),
        child: Stack(
          children: [
            Positioned.fill(
              child: CustomPaint(
                painter: _OfferBannerPainter(
                  accent: palette.accent,
                  featured: featured,
                ),
              ),
            ),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 13, vertical: 11),
              child: Row(
                children: [
                  _OfferArtTile(featured: featured, palette: palette),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Flexible(
                              child: _OfferRibbon(
                                text: featured
                                    ? 'طبيب مميز'
                                    : offer.badgeText ?? 'عرض محدود',
                                featured: featured,
                                palette: palette,
                              ),
                            ),
                            const SizedBox(width: 7),
                            Flexible(
                              child: _OfferMetaPill(
                                text: 'ينتهي ${offer.remainingDays} أيام',
                                palette: palette,
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 6),
                        _OfferPriceText(
                          text: offer.priceText,
                          featured: featured,
                          palette: palette,
                        ),
                        const SizedBox(height: 2),
                        Text(
                          offer.title,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(
                            color: AppColors.text,
                            fontSize: 14,
                            height: 1.35,
                            fontWeight: FontWeight.w900,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          offer.doctorName,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: TextStyle(
                            color: palette.darkAccent,
                            fontSize: 13,
                            fontWeight: FontWeight.w900,
                          ),
                        ),
                        const SizedBox(height: 2),
                        Text(
                          offer.scope,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(
                            color: AppColors.muted,
                            fontSize: 11,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                        const SizedBox(height: 6),
                        _OfferActionButton(palette: palette),
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
}

class _OfferPalette {
  const _OfferPalette({
    required this.accent,
    required this.darkAccent,
    required this.surface,
    required this.border,
    required this.shadow,
  });

  static const premium = _OfferPalette(
    accent: Color(0xFFD79A00),
    darkAccent: Color(0xFFB98200),
    surface: Color(0xFFFFFBED),
    border: Color(0xFFE8B33A),
    shadow: Color(0x33D79A00),
  );

  static const standard = _OfferPalette(
    accent: Color(0xFF0F8B83),
    darkAccent: Color(0xFF08746E),
    surface: Colors.white,
    border: Color(0xFF38A9A2),
    shadow: Color(0x260F8B83),
  );

  final Color accent;
  final Color darkAccent;
  final Color surface;
  final Color border;
  final Color shadow;
}

class _OfferRibbon extends StatelessWidget {
  const _OfferRibbon({
    required this.text,
    required this.featured,
    required this.palette,
  });

  final String text;
  final bool featured;
  final _OfferPalette palette;

  @override
  Widget build(BuildContext context) => Container(
    constraints: const BoxConstraints(maxWidth: 128),
    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
    decoration: BoxDecoration(
      color: Colors.white.withValues(alpha: .86),
      borderRadius: BorderRadius.circular(8),
      border: Border.all(color: palette.border),
    ),
    child: Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(
          featured ? Icons.workspace_premium_rounded : Icons.local_offer_rounded,
          size: 16,
          color: palette.accent,
        ),
        const SizedBox(width: 5),
        Flexible(
          child: Text(
            text,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: TextStyle(
              color: palette.darkAccent,
              fontSize: 12,
              fontWeight: FontWeight.w900,
            ),
          ),
        ),
      ],
    ),
  );
}

class _OfferMetaPill extends StatelessWidget {
  const _OfferMetaPill({required this.text, required this.palette});

  final String text;
  final _OfferPalette palette;

  @override
  Widget build(BuildContext context) => Container(
    constraints: const BoxConstraints(maxWidth: 120),
    padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 5),
    decoration: BoxDecoration(
      color: Colors.white.withValues(alpha: .74),
      borderRadius: BorderRadius.circular(8),
      border: Border.all(color: palette.border.withValues(alpha: .38)),
    ),
    child: Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(Icons.schedule_rounded, size: 15, color: palette.accent),
        const SizedBox(width: 4),
        Flexible(
          child: Text(
            text,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: TextStyle(
              color: palette.darkAccent,
              fontSize: 11,
              fontWeight: FontWeight.w800,
            ),
          ),
        ),
      ],
    ),
  );
}

class _OfferPriceText extends StatelessWidget {
  const _OfferPriceText({
    required this.text,
    required this.featured,
    required this.palette,
  });

  final String text;
  final bool featured;
  final _OfferPalette palette;

  @override
  Widget build(BuildContext context) => SizedBox(
    width: double.infinity,
    child: FittedBox(
      fit: BoxFit.scaleDown,
      alignment: AlignmentDirectional.centerStart,
      child: Text(
        text,
        maxLines: 1,
        style: TextStyle(
          color: palette.accent,
          fontSize: featured ? 31 : 26,
          height: 1.05,
          fontWeight: FontWeight.w900,
        ),
      ),
    ),
  );
}

class _OfferActionButton extends StatelessWidget {
  const _OfferActionButton({required this.palette});

  final _OfferPalette palette;

  @override
  Widget build(BuildContext context) => Container(
    height: 34,
    constraints: const BoxConstraints(maxWidth: 122),
    decoration: BoxDecoration(
      color: palette.accent,
      borderRadius: BorderRadius.circular(8),
      boxShadow: [
        BoxShadow(
          color: palette.shadow,
          blurRadius: 10,
          offset: const Offset(0, 5),
        ),
      ],
    ),
    child: Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Container(
          width: 22,
          height: 22,
          decoration: const BoxDecoration(
            color: Colors.white,
            shape: BoxShape.circle,
          ),
          child: Icon(
            Icons.arrow_back_rounded,
            color: palette.accent,
            size: 17,
          ),
        ),
        const SizedBox(width: 7),
        const Text(
          'احجز الآن',
          style: TextStyle(
            color: Colors.white,
            fontSize: 12,
            fontWeight: FontWeight.w900,
          ),
        ),
      ],
    ),
  );
}

class _OfferArtTile extends StatelessWidget {
  const _OfferArtTile({required this.featured, required this.palette});

  final bool featured;
  final _OfferPalette palette;

  @override
  Widget build(BuildContext context) => Container(
    width: 74,
    height: 74,
    decoration: BoxDecoration(
      color: Colors.white.withValues(alpha: .72),
      borderRadius: BorderRadius.circular(8),
      border: Border.all(color: Colors.white),
      boxShadow: [
        BoxShadow(
          color: palette.shadow,
          blurRadius: 14,
          offset: const Offset(0, 6),
        ),
      ],
    ),
    child: Stack(
      alignment: Alignment.center,
      children: [
        Icon(
          featured ? Icons.workspace_premium_rounded : Icons.sell_rounded,
          color: palette.accent,
          size: featured ? 46 : 44,
        ),
        if (!featured)
          const Text(
            '%',
            style: TextStyle(
              color: Colors.white,
              fontSize: 24,
              fontWeight: FontWeight.w900,
            ),
          ),
      ],
    ),
  );
}

class _OfferBannerPainter extends CustomPainter {
  const _OfferBannerPainter({required this.accent, required this.featured});

  final Color accent;
  final bool featured;

  @override
  void paint(Canvas canvas, Size size) {
    final softPaint = Paint()
      ..color = accent.withValues(alpha: featured ? .12 : .08)
      ..style = PaintingStyle.fill;
    final lightPaint = Paint()
      ..color = accent.withValues(alpha: featured ? .07 : .05)
      ..style = PaintingStyle.fill;

    final wave = Path()
      ..moveTo(size.width * .1, size.height)
      ..quadraticBezierTo(
        size.width * .34,
        size.height * .58,
        size.width * .55,
        size.height * .82,
      )
      ..quadraticBezierTo(
        size.width * .75,
        size.height * 1.04,
        size.width,
        size.height * .2,
      )
      ..lineTo(size.width, size.height)
      ..close();
    canvas.drawPath(wave, softPaint);

    final stripe = Path()
      ..moveTo(size.width * .78, 0)
      ..lineTo(size.width, 0)
      ..lineTo(size.width, size.height * .14)
      ..quadraticBezierTo(
        size.width * .88,
        size.height * .35,
        size.width * .78,
        size.height * .78,
      )
      ..close();
    canvas.drawPath(stripe, lightPaint);
  }

  @override
  bool shouldRepaint(covariant _OfferBannerPainter oldDelegate) =>
      oldDelegate.accent != accent || oldDelegate.featured != featured;
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
