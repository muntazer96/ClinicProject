import 'dart:async';

import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../core/analytics_service.dart';
import '../../core/api_client.dart';
import '../../core/app_theme.dart';
import '../../widgets/app_scaffold.dart';
import '../auth/auth_controller.dart';
import '../directory/directory_service.dart';
import '../directory/models/directory_models.dart';
import '../directory/widgets/specialty_card.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  late final DirectoryService _service;
  late final AnalyticsService _analytics;

  final _offersController = PageController(viewportFraction: .90);

  List<Specialization> _specializations = [];
  List<DoctorOffer> _offers = [];

  bool _loadingSpecializations = true;
  bool _loadingOffers = true;
  String? _specializationsError;
  String? _offersError;

  int _currentOfferPage = 0;
  Timer? _offersTimer;

  @override
  void initState() {
    super.initState();
    final api = context.read<AuthController>().api;
    _service = DirectoryService(api);
    _analytics = AnalyticsService(api);
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
    setState(() => _specializationsError = null);
    try {
      final items = await _service.getSpecializations();
      if (mounted) setState(() => _specializations = items);
    } catch (error) {
      if (mounted) {
        setState(() => _specializationsError = ApiClient.errorMessage(error));
      }
    } finally {
      if (mounted) setState(() => _loadingSpecializations = false);
    }
  }

  Future<void> _loadOffers() async {
    setState(() => _offersError = null);
    try {
      final result = await _service.getOffers(pageSize: 5);
      if (!mounted) return;

      setState(() {
        _offers = result.items;
        _currentOfferPage = 0;
      });

      if (_offersController.hasClients && result.items.isNotEmpty) {
        _offersController.jumpToPage(0);
      }

      _trackOfferViewed(0);
      _startOfferAutoScroll();
    } catch (error) {
      if (mounted) setState(() => _offersError = ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loadingOffers = false);
    }
  }

  void _startOfferAutoScroll() {
    _offersTimer?.cancel();

    if (_offers.length < 2) return;

    _offersTimer = Timer.periodic(const Duration(seconds: 4), (_) {
      if (!mounted || !_offersController.hasClients || _offers.isEmpty) return;

      final nextPage = (_currentOfferPage + 1) % _offers.length;

      _offersController.animateToPage(
        nextPage,
        duration: const Duration(milliseconds: 520),
        curve: Curves.easeOutCubic,
      );
    });
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
          padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
          children: [
            _HeroSection(auth: auth),
            const SizedBox(height: 14),
            _SearchCard(onTap: () => context.go('/search')),
            const SizedBox(height: 18),
            _QuickBookingButton(onTap: () => context.go('/search')),
            const SizedBox(height: 24),
            _OffersSection(
              loading: _loadingOffers,
              error: _offersError,
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
                context.push(
                  '/doctors/${offer.doctorId}?source=offer&offerId=${offer.id}',
                );
              },
            ),
            const SizedBox(height: 24),
            _SectionTitle(
              title: 'الاختصاصات الطبية',
              action: 'عرض الكل',
              onAction: () => context.push('/specializations'),
            ),
            const SizedBox(height: 12),
            _SpecializationPreview(
              loading: _loadingSpecializations,
              error: _specializationsError,
              items: _specializations.take(4).toList(),
              onRetry: _loadSpecializations,
            ),
            const SizedBox(height: 24),
            const _SectionTitle(title: 'خدمات سريعة'),
            const SizedBox(height: 12),
            const _ServiceList(),
          ],
        ),
      ),
    );
  }
}

class _HeroSection extends StatelessWidget {
  const _HeroSection({required this.auth});

  final AuthController auth;

  @override
  Widget build(BuildContext context) {
    final name = auth.isAuthenticated ? auth.displayName : 'زائرنا العزيز';

    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          begin: AlignmentDirectional.topStart,
          end: AlignmentDirectional.bottomEnd,
          colors: [AppColors.primaryDark, AppColors.primary],
        ),
        borderRadius: BorderRadius.circular(28),
        boxShadow: [
          BoxShadow(
            color: AppColors.primary.withOpacity(.22),
            blurRadius: 26,
            offset: const Offset(0, 12),
          ),
        ],
      ),
      child: Stack(
        children: [
          PositionedDirectional(
            top: -28,
            start: -22,
            child: Icon(
              Icons.health_and_safety_rounded,
              size: 145,
              color: Colors.white.withOpacity(.06),
            ),
          ),
          Row(
            children: [
              Container(
                width: 62,
                height: 62,
                decoration: BoxDecoration(
                  color: Colors.white.withOpacity(.15),
                  borderRadius: BorderRadius.circular(22),
                  border: Border.all(color: Colors.white24),
                ),
                child: const Icon(
                  Icons.health_and_safety_rounded,
                  color: Colors.white,
                  size: 34,
                ),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'أهلاً، $name',
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 23,
                        fontWeight: FontWeight.w900,
                      ),
                    ),
                    const SizedBox(height: 7),
                    const Text(
                      'احجز موعدك مع الطبيب المناسب وتابع حجوزاتك بسهولة.',
                      style: TextStyle(
                        color: Color(0xFFE4F7F4),
                        fontSize: 13,
                        height: 1.6,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _QuickBookingButton extends StatelessWidget {
  const _QuickBookingButton({required this.onTap});

  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => FilledButton.icon(
    onPressed: onTap,
    icon: const Icon(Icons.event_available_rounded),
    label: const Text('ابدأ بالحجز الآن'),
    style: FilledButton.styleFrom(
      minimumSize: const Size.fromHeight(52),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(18)),
    ),
  );
}

class _SearchCard extends StatelessWidget {
  const _SearchCard({required this.onTap});

  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => InkWell(
    onTap: onTap,
    borderRadius: BorderRadius.circular(22),
    child: Container(
      height: 60,
      padding: const EdgeInsets.symmetric(horizontal: 14),
      decoration: BoxDecoration(
        color: context.appSurface,
        borderRadius: BorderRadius.circular(22),
        border: Border.all(color: context.appBorder),
        boxShadow: [
          BoxShadow(
            color: context.isDark
                ? Colors.black.withValues(alpha: .20)
                : Colors.black.withOpacity(.045),
            blurRadius: 18,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Row(
        children: [
          Icon(
            Icons.search_rounded,
            color: Theme.of(context).colorScheme.primary,
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              'ابحث عن طبيب، اختصاص أو عيادة',
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: TextStyle(
                color: context.appMuted,
                fontWeight: FontWeight.w800,
              ),
            ),
          ),
          Icon(Icons.arrow_back_rounded, color: context.appText),
        ],
      ),
    ),
  );
}

class _OffersSection extends StatelessWidget {
  const _OffersSection({
    required this.loading,
    this.error,
    required this.offers,
    required this.controller,
    required this.onPageChanged,
    required this.onViewAll,
    required this.onOfferTap,
  });

  final bool loading;
  final String? error;
  final List<DoctorOffer> offers;
  final PageController controller;
  final ValueChanged<int> onPageChanged;
  final VoidCallback onViewAll;
  final ValueChanged<DoctorOffer> onOfferTap;

  @override
  Widget build(BuildContext context) {
    if (loading) {
      return Container(
        height: 170,
        decoration: BoxDecoration(
          color: context.appSurface,
          borderRadius: BorderRadius.circular(24),
          border: Border.all(color: context.appBorder),
        ),
        child: const Center(child: CircularProgressIndicator()),
      );
    }

    if (error != null) {
      return _InlineErrorMessage(text: error!);
    }

    if (offers.isEmpty) return const SizedBox.shrink();

    return Column(
      children: [
        _SectionTitle(
          title: 'العروض الفعالة',
          action: 'عرض الكل',
          onAction: onViewAll,
        ),
        const SizedBox(height: 12),
        SizedBox(
          height: 190,
          child: Directionality(
            textDirection: TextDirection.rtl,
            child: PageView.builder(
              controller: controller,
              itemCount: offers.length > 1 ? null : offers.length,
              onPageChanged: onPageChanged,
              itemBuilder: (context, index) {
                final offer = offers[index % offers.length];

                return Padding(
                  padding: const EdgeInsetsDirectional.only(end: 10),
                  child: _OfferCard(
                    offer: offer,
                    onTap: () => onOfferTap(offer),
                  ),
                );
              },
            ),
          ),
        ),
      ],
    );
  }
}

class _OfferCard extends StatelessWidget {
  const _OfferCard({required this.offer, required this.onTap});

  final DoctorOffer offer;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final featured = offer.isFeatured;
    final accent = featured ? const Color(0xFFD79A00) : AppColors.primary;
    final surface = featured
        ? (context.isDark ? const Color(0xFF2F2817) : const Color(0xFFFFFBED))
        : context.appSurface;

    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(26),
      child: Container(
        clipBehavior: Clip.antiAlias,
        decoration: BoxDecoration(
          color: surface,
          borderRadius: BorderRadius.circular(26),
          border: Border.all(
            color: featured ? const Color(0xFFE8B33A) : context.appBorder,
          ),
          boxShadow: [
            BoxShadow(
              color: accent.withOpacity(.16),
              blurRadius: 22,
              offset: const Offset(0, 10),
            ),
          ],
        ),
        child: Stack(
          children: [
            PositionedDirectional(
              top: -34,
              start: -28,
              child: Icon(
                featured
                    ? Icons.workspace_premium_rounded
                    : Icons.local_offer_rounded,
                size: 155,
                color: accent.withOpacity(.08),
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(15),
              child: Row(
                children: [
                  Container(
                    width: 76,
                    height: 76,
                    decoration: BoxDecoration(
                      color: accent.withOpacity(.12),
                      borderRadius: BorderRadius.circular(24),
                    ),
                    child: Icon(
                      featured
                          ? Icons.workspace_premium_rounded
                          : Icons.sell_rounded,
                      color: accent,
                      size: 42,
                    ),
                  ),
                  const SizedBox(width: 13),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Wrap(
                          spacing: 6,
                          runSpacing: 6,
                          children: [
                            _OfferBadge(
                              text: featured
                                  ? 'طبيب مميز'
                                  : offer.badgeText ?? 'عرض محدود',
                              color: accent,
                            ),
                            _OfferBadge(
                              text: 'ينتهي ${offer.remainingDays} أيام',
                              color: AppColors.primary,
                              icon: Icons.schedule_rounded,
                            ),
                          ],
                        ),
                        const Spacer(),
                        Text(
                          offer.priceText,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: TextStyle(
                            color: accent,
                            fontSize: featured ? 28 : 25,
                            fontWeight: FontWeight.w900,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          offer.title,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(
                            fontWeight: FontWeight.w900,
                            fontSize: 14,
                          ),
                        ),
                        const SizedBox(height: 3),
                        Text(
                          offer.doctorName,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: TextStyle(
                            color: accent,
                            fontWeight: FontWeight.w900,
                            fontSize: 13,
                          ),
                        ),
                        const SizedBox(height: 3),
                        Text(
                          offer.scope,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(
                            fontWeight: FontWeight.w700,
                            fontSize: 11,
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
}

class _OfferBadge extends StatelessWidget {
  const _OfferBadge({required this.text, required this.color, this.icon});

  final String text;
  final Color color;
  final IconData? icon;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 5),
    decoration: BoxDecoration(
      color: color.withOpacity(.10),
      borderRadius: BorderRadius.circular(999),
    ),
    child: Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon ?? Icons.local_offer_rounded, size: 14, color: color),
        const SizedBox(width: 4),
        Text(
          text,
          style: TextStyle(
            color: color,
            fontSize: 11,
            fontWeight: FontWeight.w900,
          ),
        ),
      ],
    ),
  );
}

class _SpecializationPreview extends StatelessWidget {
  const _SpecializationPreview({
    required this.loading,
    this.error,
    required this.items,
    this.onRetry,
  });

  final bool loading;
  final String? error;
  final List<Specialization> items;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) {
    if (loading) {
      return const Padding(
        padding: EdgeInsets.all(24),
        child: Center(child: CircularProgressIndicator()),
      );
    }

    if (error != null) {
      return _InlineErrorMessage(text: error!, onRetry: onRetry);
    }

    if (items.isEmpty) {
      return const _EmptySpecializations();
    }

    return LayoutBuilder(
      builder: (context, constraints) {
        final width = constraints.maxWidth;
        final columns = width >= 680
            ? 4
            : width >= 430
            ? 3
            : 2;

        return GridView.builder(
          itemCount: items.length,
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
            crossAxisCount: columns,
            crossAxisSpacing: 10,
            mainAxisSpacing: 10,
            childAspectRatio: columns == 2 ? 1.55 : 1.28,
          ),
          itemBuilder: (context, index) {
            return SpecialtyCard(
              item: items[index],
              index: index,
              compact: true,
            );
          },
        );
      },
    );
  }
}

class _EmptySpecializations extends StatelessWidget {
  const _EmptySpecializations();

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(18),
    decoration: BoxDecoration(
      color: context.appSurface,
      borderRadius: BorderRadius.circular(22),
      border: Border.all(color: context.appBorder),
    ),
    child: Text(
      'لا توجد اختصاصات متاحة حالياً.',
      textAlign: TextAlign.center,
      style: TextStyle(color: context.appMuted),
    ),
  );
}

class _InlineErrorMessage extends StatelessWidget {
  const _InlineErrorMessage({required this.text, this.onRetry});

  final String text;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) => Container(
    width: double.infinity,
    padding: const EdgeInsets.all(18),
    decoration: BoxDecoration(
      color: context.appSurface,
      borderRadius: BorderRadius.circular(22),
      border: Border.all(color: context.appBorder),
    ),
    child: Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(
          text,
          textAlign: TextAlign.center,
          style: const TextStyle(
            color: AppColors.danger,
            fontWeight: FontWeight.w700,
          ),
        ),
        if (onRetry != null) ...[
          const SizedBox(height: 8),
          TextButton.icon(
            onPressed: onRetry,
            icon: const Icon(Icons.refresh_rounded),
            label: const Text('إعادة المحاولة'),
          ),
        ],
      ],
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
          style: const TextStyle(fontSize: 19, fontWeight: FontWeight.w900),
        ),
      ),
      if (action != null)
        TextButton(
          onPressed: onAction,
          child: Text(
            action!,
            style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w900),
          ),
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
        color: context.appSoftAmber,
        onTap: () => context.go('/bookings'),
      ),
      const SizedBox(height: 10),
      _ServiceRow(
        icon: Icons.favorite_rounded,
        title: 'الأطباء المفضلون',
        subtitle: 'وصول سريع للأطباء الذين تتابعهم.',
        color: context.isDark
            ? const Color(0xFF3A2330)
            : const Color(0xFFFFE4EA),
        onTap: () => context.go('/favorites'),
      ),
      const SizedBox(height: 10),
      _ServiceRow(
        icon: Icons.manage_search_rounded,
        title: 'متابعة حجز زائر',
        subtitle: 'اعرض الحجز أو ألغِه باستخدام الهاتف والكود.',
        color: context.appSoftCoral,
        onTap: () => context.go('/guest-booking'),
      ),
      const SizedBox(height: 10),
      _ServiceRow(
        icon: Icons.account_circle_rounded,
        title: 'الملف الشخصي',
        subtitle: 'عرض بيانات الحساب وتعديل الاسم.',
        color: context.appSurfaceMuted,
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
    borderRadius: BorderRadius.circular(22),
    child: Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: context.appSurface,
        borderRadius: BorderRadius.circular(22),
        border: Border.all(color: context.appBorder),
        boxShadow: [
          BoxShadow(
            color: context.isDark
                ? Colors.black.withValues(alpha: .18)
                : Colors.black.withOpacity(.035),
            blurRadius: 16,
            offset: const Offset(0, 7),
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            width: 46,
            height: 46,
            decoration: BoxDecoration(
              color: color,
              borderRadius: BorderRadius.circular(16),
            ),
            child: Icon(icon, color: Theme.of(context).colorScheme.primary),
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
                  style: TextStyle(
                    color: context.appMuted,
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
// import 'dart:async';
// import 'dart:math' as math;

// import 'package:flutter/material.dart';
// import 'package:go_router/go_router.dart';
// import 'package:provider/provider.dart';

// import '../../core/analytics_service.dart';
// import '../../core/api_client.dart';
// import '../../core/app_theme.dart';
// import '../../widgets/app_scaffold.dart';
// import '../auth/auth_controller.dart';
// import '../directory/directory_service.dart';
// import '../directory/models/directory_models.dart';
// import '../directory/specialization_icons.dart';

// // ─── Design Tokens ────────────────────────────────────────────────────────────

// class _Clr {
//   static const navy = Color(0xFF0B1628);
//   static const navyLight = Color(0xFF152035);
//   static const teal = Color(0xFF00C9A7);
//   static const tealDim = Color(0xFF009E83);
//   static const amber = Color(0xFFFFB347);
//   static const surface = Color(0xFFF5F9FF);
//   static const surfaceDark = Color(0xFF1A2840);
//   static const muted = Color(0xFF8FA3BF);
//   static const danger = Color(0xFFFF6B6B);
//   static const cardBorder = Color(0xFFE4EDF8);
//   static const cardBorderDark = Color(0xFF243550);
// }

// // ─── Home Screen ──────────────────────────────────────────────────────────────

// class HomeScreen extends StatefulWidget {
//   const HomeScreen({super.key});

//   @override
//   State<HomeScreen> createState() => _HomeScreenState();
// }

// class _HomeScreenState extends State<HomeScreen> {
//   late final DirectoryService _service;
//   late final AnalyticsService _analytics;

//   final _offersController = PageController(viewportFraction: .88);

//   List<Specialization> _specializations = [];
//   List<DoctorOffer> _offers = [];

//   bool _loadingSpecializations = true;
//   bool _loadingOffers = true;
//   String? _specializationsError;
//   String? _offersError;

//   int _currentOfferPage = 0;
//   Timer? _offersTimer;

//   @override
//   void initState() {
//     super.initState();
//     final api = context.read<AuthController>().api;
//     _service = DirectoryService(api);
//     _analytics = AnalyticsService(api);
//     _analytics.trackLater(eventType: 'page_viewed', page: 'home');
//     _loadSpecializations();
//     _loadOffers();
//   }

//   @override
//   void dispose() {
//     _offersTimer?.cancel();
//     _offersController.dispose();
//     super.dispose();
//   }

//   Future<void> _loadSpecializations() async {
//     setState(() => _specializationsError = null);
//     try {
//       final items = await _service.getSpecializations();
//       if (mounted) setState(() => _specializations = items);
//     } catch (error) {
//       if (mounted) {
//         setState(() => _specializationsError = ApiClient.errorMessage(error));
//       }
//     } finally {
//       if (mounted) setState(() => _loadingSpecializations = false);
//     }
//   }

//   Future<void> _loadOffers() async {
//     setState(() => _offersError = null);
//     try {
//       final result = await _service.getOffers(pageSize: 5);
//       if (!mounted) return;
//       setState(() {
//         _offers = result.items;
//         _currentOfferPage = 0;
//       });
//       if (_offersController.hasClients && result.items.isNotEmpty) {
//         _offersController.jumpToPage(0);
//       }
//       _trackOfferViewed(0);
//       _startOfferAutoScroll();
//     } catch (error) {
//       if (mounted) setState(() => _offersError = ApiClient.errorMessage(error));
//     } finally {
//       if (mounted) setState(() => _loadingOffers = false);
//     }
//   }

//   void _startOfferAutoScroll() {
//     _offersTimer?.cancel();
//     if (_offers.length < 2) return;
//     _offersTimer = Timer.periodic(const Duration(seconds: 4), (_) {
//       if (!mounted || !_offersController.hasClients || _offers.isEmpty) return;
//       final nextPage = (_currentOfferPage + 1) % _offers.length;
//       _offersController.animateToPage(
//         nextPage,
//         duration: const Duration(milliseconds: 520),
//         curve: Curves.easeOutCubic,
//       );
//     });
//   }

//   void _trackOfferViewed(int pageIndex) {
//     if (_offers.isEmpty) return;
//     final offer = _offers[pageIndex % _offers.length];
//     _analytics.trackOnce(
//       key: 'home-offer-${offer.id}',
//       eventType: 'offer_viewed',
//       doctorId: offer.doctorId,
//       offerId: offer.id,
//       source: 'home_banner',
//       page: 'home',
//     );
//   }

//   @override
//   Widget build(BuildContext context) {
//     final auth = context.watch<AuthController>();
//     final isDark = context.isDark;

//     return AppScaffold(
//       child: RefreshIndicator(
//         color: _Clr.teal,
//         onRefresh: () async {
//           await auth.refreshProfile(silent: true);
//           await Future.wait([_loadSpecializations(), _loadOffers()]);
//         },
//         child: ListView(
//           padding: EdgeInsets.zero,
//           children: [
//             // ── Hero ──────────────────────────────────────────────────────────
//             _HeroSection(auth: auth),

//             // ── Body ─────────────────────────────────────────────────────────
//             Padding(
//               padding: const EdgeInsets.fromLTRB(16, 20, 16, 28),
//               child: Column(
//                 crossAxisAlignment: CrossAxisAlignment.stretch,
//                 children: [
//                   // Search
//                   _SearchBar(onTap: () => context.go('/search')),
//                   const SizedBox(height: 20),

//                   // Quick actions strip
//                   _QuickActionsRow(
//                     onBook: () => context.go('/search'),
//                     onBookings: () => context.go('/bookings'),
//                     onFavorites: () => context.go('/favorites'),
//                     onProfile: () => context.go('/profile'),
//                   ),
//                   const SizedBox(height: 28),

//                   // Offers
//                   _OffersSection(
//                     loading: _loadingOffers,
//                     error: _offersError,
//                     offers: _offers,
//                     controller: _offersController,
//                     onPageChanged: (index) {
//                       _currentOfferPage = index;
//                       _trackOfferViewed(index);
//                     },
//                     onViewAll: () {
//                       _analytics.trackLater(
//                         eventType: 'page_viewed',
//                         source: 'home_offers_view_all',
//                         page: 'offers',
//                       );
//                       context.go('/offers');
//                     },
//                     onOfferTap: (offer) {
//                       _analytics.trackLater(
//                         eventType: 'offer_clicked',
//                         doctorId: offer.doctorId,
//                         offerId: offer.id,
//                         source: 'home_banner',
//                         page: 'home',
//                       );
//                       context.push(
//                         '/doctors/${offer.doctorId}?source=offer&offerId=${offer.id}',
//                       );
//                     },
//                   ),
//                   const SizedBox(height: 28),

//                   // Specializations – horizontal scroll
//                   _SectionHeader(
//                     title: 'الاختصاصات الطبية',
//                     action: 'عرض الكل',
//                     onAction: () => context.go('/specializations'),
//                   ),
//                   const SizedBox(height: 12),
//                   _SpecializationRow(
//                     loading: _loadingSpecializations,
//                     error: _specializationsError,
//                     items: _specializations.take(8).toList(),
//                     onRetry: _loadSpecializations,
//                   ),
//                   const SizedBox(height: 28),

//                   // Guest lookup
//                   _GuestLookupBanner(
//                     onTap: () => context.go('/guest-booking'),
//                   ),
//                 ],
//               ),
//             ),
//           ],
//         ),
//       ),
//     );
//   }
// }

// // ─── Hero ─────────────────────────────────────────────────────────────────────

// class _HeroSection extends StatefulWidget {
//   const _HeroSection({required this.auth});
//   final AuthController auth;

//   @override
//   State<_HeroSection> createState() => _HeroSectionState();
// }

// class _HeroSectionState extends State<_HeroSection>
//     with SingleTickerProviderStateMixin {
//   late final AnimationController _pulse;

//   @override
//   void initState() {
//     super.initState();
//     _pulse = AnimationController(
//       vsync: this,
//       duration: const Duration(seconds: 3),
//     )..repeat();
//   }

//   @override
//   void dispose() {
//     _pulse.dispose();
//     super.dispose();
//   }

//   @override
//   Widget build(BuildContext context) {
//     final name =
//         widget.auth.isAuthenticated ? widget.auth.displayName : 'زائرنا العزيز';
//     final topPad = MediaQuery.of(context).padding.top;

//     return Container(
//       width: double.infinity,
//       padding: EdgeInsets.fromLTRB(20, topPad + 20, 20, 28),
//       decoration: const BoxDecoration(
//         gradient: LinearGradient(
//           begin: Alignment.topLeft,
//           end: Alignment.bottomRight,
//           colors: [_Clr.navy, Color(0xFF12233E)],
//         ),
//         borderRadius: BorderRadius.vertical(bottom: Radius.circular(36)),
//       ),
//       child: Stack(
//         clipBehavior: Clip.none,
//         children: [
//           // Pulse rings (signature element)
//           PositionedDirectional(
//             top: -10,
//             end: -10,
//             child: AnimatedBuilder(
//               animation: _pulse,
//               builder: (context, _) {
//                 return CustomPaint(
//                   size: const Size(130, 130),
//                   painter: _PulseRingPainter(_pulse.value),
//                 );
//               },
//             ),
//           ),

//           Column(
//             crossAxisAlignment: CrossAxisAlignment.start,
//             children: [
//               // Top row: greeting + notification icon
//               Row(
//                 crossAxisAlignment: CrossAxisAlignment.start,
//                 children: [
//                   Expanded(
//                     child: Column(
//                       crossAxisAlignment: CrossAxisAlignment.start,
//                       children: [
//                         Text(
//                           'مرحباً، $name 👋',
//                           maxLines: 1,
//                           overflow: TextOverflow.ellipsis,
//                           style: const TextStyle(
//                             color: Colors.white,
//                             fontSize: 26,
//                             fontWeight: FontWeight.w900,
//                             height: 1.2,
//                           ),
//                         ),
//                         const SizedBox(height: 6),
//                         const Text(
//                           'كيف يمكننا مساعدتك اليوم؟',
//                           style: TextStyle(
//                             color: _Clr.muted,
//                             fontSize: 14,
//                             fontWeight: FontWeight.w600,
//                           ),
//                         ),
//                       ],
//                     ),
//                   ),
//                   const SizedBox(width: 60), // space for pulse rings
//                 ],
//               ),

//               const SizedBox(height: 24),

//               // Stat chips row
//               Row(
//                 children: const [
//                   _StatChip(
//                     icon: Icons.medical_services_rounded,
//                     value: '+٢٠٠',
//                     label: 'طبيب',
//                   ),
//                   SizedBox(width: 10),
//                   _StatChip(
//                     icon: Icons.business_rounded,
//                     value: '+٥٠',
//                     label: 'عيادة',
//                   ),
//                   SizedBox(width: 10),
//                   _StatChip(
//                     icon: Icons.category_rounded,
//                     value: '+٣٠',
//                     label: 'اختصاص',
//                   ),
//                 ],
//               ),
//             ],
//           ),
//         ],
//       ),
//     );
//   }
// }

// class _StatChip extends StatelessWidget {
//   const _StatChip({
//     required this.icon,
//     required this.value,
//     required this.label,
//   });

//   final IconData icon;
//   final String value;
//   final String label;

//   @override
//   Widget build(BuildContext context) => Container(
//         padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
//         decoration: BoxDecoration(
//           color: Colors.white.withOpacity(.07),
//           borderRadius: BorderRadius.circular(14),
//           border: Border.all(color: Colors.white.withOpacity(.10)),
//         ),
//         child: Row(
//           mainAxisSize: MainAxisSize.min,
//           children: [
//             Icon(icon, size: 14, color: _Clr.teal),
//             const SizedBox(width: 5),
//             Text(
//               '$value $label',
//               style: const TextStyle(
//                 color: Colors.white,
//                 fontSize: 12,
//                 fontWeight: FontWeight.w800,
//               ),
//             ),
//           ],
//         ),
//       );
// }

// // Pulse ring painter – the signature element
// class _PulseRingPainter extends CustomPainter {
//   _PulseRingPainter(this.t);
//   final double t;

//   @override
//   void paint(Canvas canvas, Size size) {
//     final center = Offset(size.width / 2, size.height / 2);

//     for (var i = 0; i < 3; i++) {
//       final phase = (t + i / 3) % 1.0;
//       final radius = 20.0 + phase * 50;
//       final opacity = (1.0 - phase) * 0.25;

//       final paint = Paint()
//         ..color = _Clr.teal.withOpacity(opacity)
//         ..style = PaintingStyle.stroke
//         ..strokeWidth = 1.5;

//       canvas.drawCircle(center, radius, paint);
//     }

//     // Center glow dot
//     final glowPaint = Paint()
//       ..color = _Clr.teal.withOpacity(.18)
//       ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 8);
//     canvas.drawCircle(center, 18, glowPaint);

//     final dotPaint = Paint()..color = _Clr.teal;
//     canvas.drawCircle(center, 10, dotPaint);

//     final iconPaint = Paint()..color = _Clr.navy;
//     // Small cross (health symbol) inside dot
//     final crossPaint = Paint()
//       ..color = _Clr.navy
//       ..strokeWidth = 2
//       ..strokeCap = StrokeCap.round;
//     canvas.drawLine(
//       Offset(center.dx - 5, center.dy),
//       Offset(center.dx + 5, center.dy),
//       crossPaint,
//     );
//     canvas.drawLine(
//       Offset(center.dx, center.dy - 5),
//       Offset(center.dx, center.dy + 5),
//       crossPaint,
//     );
//   }

//   @override
//   bool shouldRepaint(_PulseRingPainter old) => old.t != t;
// }

// // ─── Search Bar ───────────────────────────────────────────────────────────────

// class _SearchBar extends StatelessWidget {
//   const _SearchBar({required this.onTap});
//   final VoidCallback onTap;

//   @override
//   Widget build(BuildContext context) {
//     final isDark = context.isDark;

//     return GestureDetector(
//       onTap: onTap,
//       child: Container(
//         height: 54,
//         padding: const EdgeInsets.symmetric(horizontal: 16),
//         decoration: BoxDecoration(
//           color: isDark ? _Clr.surfaceDark : Colors.white,
//           borderRadius: BorderRadius.circular(18),
//           border: Border.all(
//             color: isDark ? _Clr.cardBorderDark : _Clr.cardBorder,
//           ),
//           boxShadow: [
//             BoxShadow(
//               color: _Clr.teal.withOpacity(.08),
//               blurRadius: 20,
//               offset: const Offset(0, 6),
//             ),
//           ],
//         ),
//         child: Row(
//           children: [
//             const Icon(Icons.search_rounded, color: _Clr.teal, size: 22),
//             const SizedBox(width: 10),
//             Expanded(
//               child: Text(
//                 'ابحث عن طبيب أو اختصاص ...',
//                 style: TextStyle(
//                   color: isDark ? _Clr.muted : const Color(0xFFABC0D8),
//                   fontSize: 14,
//                   fontWeight: FontWeight.w700,
//                 ),
//               ),
//             ),
//             Container(
//               padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
//               decoration: BoxDecoration(
//                 color: _Clr.teal.withOpacity(.12),
//                 borderRadius: BorderRadius.circular(10),
//               ),
//               child: const Text(
//                 'بحث',
//                 style: TextStyle(
//                   color: _Clr.teal,
//                   fontSize: 12,
//                   fontWeight: FontWeight.w900,
//                 ),
//               ),
//             ),
//           ],
//         ),
//       ),
//     );
//   }
// }

// // ─── Quick Actions Row ────────────────────────────────────────────────────────

// class _QuickActionsRow extends StatelessWidget {
//   const _QuickActionsRow({
//     required this.onBook,
//     required this.onBookings,
//     required this.onFavorites,
//     required this.onProfile,
//   });

//   final VoidCallback onBook;
//   final VoidCallback onBookings;
//   final VoidCallback onFavorites;
//   final VoidCallback onProfile;

//   @override
//   Widget build(BuildContext context) {
//     return Column(
//       children: [
//         // Primary CTA
//         _PrimaryBookButton(onTap: onBook),
//         const SizedBox(height: 14),
//         // Secondary row
//         Row(
//           children: [
//             Expanded(
//               child: _MiniActionCard(
//                 icon: Icons.calendar_today_rounded,
//                 label: 'حجوزاتي',
//                 color: const Color(0xFFFFF3E0),
//                 darkColor: const Color(0xFF2E2416),
//                 iconColor: _Clr.amber,
//                 onTap: onBookings,
//               ),
//             ),
//             const SizedBox(width: 10),
//             Expanded(
//               child: _MiniActionCard(
//                 icon: Icons.favorite_rounded,
//                 label: 'المفضلون',
//                 color: const Color(0xFFFFEEF0),
//                 darkColor: const Color(0xFF2E1A1E),
//                 iconColor: const Color(0xFFFF6B8A),
//                 onTap: onFavorites,
//               ),
//             ),
//             const SizedBox(width: 10),
//             Expanded(
//               child: _MiniActionCard(
//                 icon: Icons.account_circle_rounded,
//                 label: 'حسابي',
//                 color: const Color(0xFFEEF4FF),
//                 darkColor: const Color(0xFF1A2235),
//                 iconColor: const Color(0xFF6B9FFF),
//                 onTap: onProfile,
//               ),
//             ),
//           ],
//         ),
//       ],
//     );
//   }
// }

// class _PrimaryBookButton extends StatelessWidget {
//   const _PrimaryBookButton({required this.onTap});
//   final VoidCallback onTap;

//   @override
//   Widget build(BuildContext context) => GestureDetector(
//         onTap: onTap,
//         child: Container(
//           height: 56,
//           decoration: BoxDecoration(
//             gradient: const LinearGradient(
//               colors: [_Clr.teal, _Clr.tealDim],
//             ),
//             borderRadius: BorderRadius.circular(18),
//             boxShadow: [
//               BoxShadow(
//                 color: _Clr.teal.withOpacity(.30),
//                 blurRadius: 18,
//                 offset: const Offset(0, 8),
//               ),
//             ],
//           ),
//           child: const Row(
//             mainAxisAlignment: MainAxisAlignment.center,
//             children: [
//               Icon(Icons.event_available_rounded, color: Colors.white, size: 20),
//               SizedBox(width: 8),
//               Text(
//                 'احجز موعدك الآن',
//                 style: TextStyle(
//                   color: Colors.white,
//                   fontSize: 16,
//                   fontWeight: FontWeight.w900,
//                 ),
//               ),
//             ],
//           ),
//         ),
//       );
// }

// class _MiniActionCard extends StatelessWidget {
//   const _MiniActionCard({
//     required this.icon,
//     required this.label,
//     required this.color,
//     required this.darkColor,
//     required this.iconColor,
//     required this.onTap,
//   });

//   final IconData icon;
//   final String label;
//   final Color color;
//   final Color darkColor;
//   final Color iconColor;
//   final VoidCallback onTap;

//   @override
//   Widget build(BuildContext context) {
//     final isDark = context.isDark;
//     return InkWell(
//       onTap: onTap,
//       borderRadius: BorderRadius.circular(16),
//       child: Container(
//         padding: const EdgeInsets.symmetric(vertical: 14),
//         decoration: BoxDecoration(
//           color: isDark ? darkColor : color,
//           borderRadius: BorderRadius.circular(16),
//           border: Border.all(
//             color: isDark ? _Clr.cardBorderDark : _Clr.cardBorder,
//           ),
//         ),
//         child: Column(
//           mainAxisSize: MainAxisSize.min,
//           children: [
//             Icon(icon, color: iconColor, size: 22),
//             const SizedBox(height: 6),
//             Text(
//               label,
//               style: TextStyle(
//                 fontSize: 11,
//                 fontWeight: FontWeight.w900,
//                 color: isDark ? Colors.white : const Color(0xFF2A3A4E),
//               ),
//             ),
//           ],
//         ),
//       ),
//     );
//   }
// }

// // ─── Offers Section ───────────────────────────────────────────────────────────

// class _OffersSection extends StatelessWidget {
//   const _OffersSection({
//     required this.loading,
//     this.error,
//     required this.offers,
//     required this.controller,
//     required this.onPageChanged,
//     required this.onViewAll,
//     required this.onOfferTap,
//   });

//   final bool loading;
//   final String? error;
//   final List<DoctorOffer> offers;
//   final PageController controller;
//   final ValueChanged<int> onPageChanged;
//   final VoidCallback onViewAll;
//   final ValueChanged<DoctorOffer> onOfferTap;

//   @override
//   Widget build(BuildContext context) {
//     if (loading) {
//       return Column(
//         crossAxisAlignment: CrossAxisAlignment.start,
//         children: [
//           _SectionHeader(title: 'العروض الفعالة'),
//           const SizedBox(height: 12),
//           Container(
//             height: 160,
//             decoration: BoxDecoration(
//               color: context.isDark ? _Clr.surfaceDark : _Clr.surface,
//               borderRadius: BorderRadius.circular(24),
//               border: Border.all(
//                 color:
//                     context.isDark ? _Clr.cardBorderDark : _Clr.cardBorder,
//               ),
//             ),
//             child: const Center(
//               child: CircularProgressIndicator(color: _Clr.teal),
//             ),
//           ),
//         ],
//       );
//     }

//     if (error != null) return _InlineError(text: error!);
//     if (offers.isEmpty) return const SizedBox.shrink();

//     return Column(
//       crossAxisAlignment: CrossAxisAlignment.start,
//       children: [
//         _SectionHeader(
//           title: 'العروض الفعالة',
//           action: 'عرض الكل',
//           onAction: onViewAll,
//         ),
//         const SizedBox(height: 12),
//         SizedBox(
//           height: 170,
//           child: Directionality(
//             textDirection: TextDirection.rtl,
//             child: PageView.builder(
//               controller: controller,
//               itemCount: offers.length > 1 ? null : offers.length,
//               onPageChanged: onPageChanged,
//               itemBuilder: (context, index) {
//                 final offer = offers[index % offers.length];
//                 return Padding(
//                   padding: const EdgeInsetsDirectional.only(end: 12),
//                   child: _OfferCard(offer: offer, onTap: () => onOfferTap(offer)),
//                 );
//               },
//             ),
//           ),
//         ),
//       ],
//     );
//   }
// }

// class _OfferCard extends StatelessWidget {
//   const _OfferCard({required this.offer, required this.onTap});
//   final DoctorOffer offer;
//   final VoidCallback onTap;

//   @override
//   Widget build(BuildContext context) {
//     final featured = offer.isFeatured;
//     final accent = featured ? _Clr.amber : _Clr.teal;

//     return GestureDetector(
//       onTap: onTap,
//       child: Container(
//         decoration: BoxDecoration(
//           gradient: LinearGradient(
//             begin: Alignment.topLeft,
//             end: Alignment.bottomRight,
//             colors: featured
//                 ? [const Color(0xFF2A1F0A), const Color(0xFF1E1608)]
//                 : [_Clr.navyLight, _Clr.navy],
//           ),
//           borderRadius: BorderRadius.circular(24),
//           border: Border.all(
//             color: accent.withOpacity(.30),
//             width: 1.2,
//           ),
//           boxShadow: [
//             BoxShadow(
//               color: accent.withOpacity(.15),
//               blurRadius: 20,
//               offset: const Offset(0, 8),
//             ),
//           ],
//         ),
//         child: Stack(
//           children: [
//             // Background icon
//             PositionedDirectional(
//               bottom: -20,
//               end: -20,
//               child: Icon(
//                 featured
//                     ? Icons.workspace_premium_rounded
//                     : Icons.local_offer_rounded,
//                 size: 130,
//                 color: accent.withOpacity(.07),
//               ),
//             ),
//             Padding(
//               padding: const EdgeInsets.all(16),
//               child: Row(
//                 children: [
//                   // Icon box
//                   Container(
//                     width: 68,
//                     height: 68,
//                     decoration: BoxDecoration(
//                       color: accent.withOpacity(.15),
//                       borderRadius: BorderRadius.circular(20),
//                       border: Border.all(color: accent.withOpacity(.25)),
//                     ),
//                     child: Icon(
//                       featured
//                           ? Icons.workspace_premium_rounded
//                           : Icons.sell_rounded,
//                       color: accent,
//                       size: 36,
//                     ),
//                   ),
//                   const SizedBox(width: 14),
//                   Expanded(
//                     child: Column(
//                       crossAxisAlignment: CrossAxisAlignment.start,
//                       mainAxisAlignment: MainAxisAlignment.center,
//                       children: [
//                         // Badge row
//                         Row(
//                           children: [
//                             _TinyBadge(
//                               text: featured
//                                   ? 'طبيب مميز'
//                                   : offer.badgeText ?? 'عرض محدود',
//                               color: accent,
//                             ),
//                             const SizedBox(width: 6),
//                             _TinyBadge(
//                               text: '${offer.remainingDays} أيام',
//                               color: Colors.white54,
//                               icon: Icons.schedule_rounded,
//                             ),
//                           ],
//                         ),
//                         const SizedBox(height: 8),
//                         Text(
//                           offer.priceText,
//                           style: TextStyle(
//                             color: accent,
//                             fontSize: 26,
//                             fontWeight: FontWeight.w900,
//                             height: 1,
//                           ),
//                         ),
//                         const SizedBox(height: 4),
//                         Text(
//                           offer.title,
//                           maxLines: 1,
//                           overflow: TextOverflow.ellipsis,
//                           style: const TextStyle(
//                             color: Colors.white,
//                             fontWeight: FontWeight.w800,
//                             fontSize: 13,
//                           ),
//                         ),
//                         const SizedBox(height: 2),
//                         Text(
//                           '${offer.doctorName} — ${offer.scope}',
//                           maxLines: 1,
//                           overflow: TextOverflow.ellipsis,
//                           style: TextStyle(
//                             color: accent.withOpacity(.80),
//                             fontWeight: FontWeight.w700,
//                             fontSize: 11,
//                           ),
//                         ),
//                       ],
//                     ),
//                   ),
//                 ],
//               ),
//             ),
//           ],
//         ),
//       ),
//     );
//   }
// }

// class _TinyBadge extends StatelessWidget {
//   const _TinyBadge({required this.text, required this.color, this.icon});
//   final String text;
//   final Color color;
//   final IconData? icon;

//   @override
//   Widget build(BuildContext context) => Container(
//         padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
//         decoration: BoxDecoration(
//           color: color.withOpacity(.12),
//           borderRadius: BorderRadius.circular(999),
//           border: Border.all(color: color.withOpacity(.25)),
//         ),
//         child: Row(
//           mainAxisSize: MainAxisSize.min,
//           children: [
//             if (icon != null) ...[
//               Icon(icon, size: 10, color: color),
//               const SizedBox(width: 3),
//             ],
//             Text(
//               text,
//               style: TextStyle(
//                 color: color,
//                 fontSize: 10,
//                 fontWeight: FontWeight.w900,
//               ),
//             ),
//           ],
//         ),
//       );
// }

// // ─── Specialization Row (horizontal scroll) ───────────────────────────────────

// class _SpecializationRow extends StatelessWidget {
//   const _SpecializationRow({
//     required this.loading,
//     this.error,
//     required this.items,
//     this.onRetry,
//   });

//   final bool loading;
//   final String? error;
//   final List<Specialization> items;
//   final VoidCallback? onRetry;

//   @override
//   Widget build(BuildContext context) {
//     if (loading) {
//       return SizedBox(
//         height: 100,
//         child: Center(
//           child: CircularProgressIndicator(color: _Clr.teal),
//         ),
//       );
//     }

//     if (error != null) return _InlineError(text: error!, onRetry: onRetry);
//     if (items.isEmpty) {
//       return Container(
//         padding: const EdgeInsets.all(16),
//         decoration: BoxDecoration(
//           color: context.appSurface,
//           borderRadius: BorderRadius.circular(20),
//         ),
//         child: Text(
//           'لا توجد اختصاصات متاحة حالياً.',
//           textAlign: TextAlign.center,
//           style: TextStyle(color: context.appMuted),
//         ),
//       );
//     }

//     return SizedBox(
//       height: 100,
//       child: ListView.separated(
//         scrollDirection: Axis.horizontal,
//         reverse: true, // RTL
//         padding: const EdgeInsets.symmetric(horizontal: 2),
//         itemCount: items.length,
//         separatorBuilder: (_, __) => const SizedBox(width: 10),
//         itemBuilder: (context, i) => _SpecCard(item: items[i]),
//       ),
//     );
//   }
// }

// class _SpecCard extends StatelessWidget {
//   const _SpecCard({required this.item});
//   final Specialization item;

//   @override
//   Widget build(BuildContext context) {
//     final icon = specializationIconFor(item.iconName);
//     final isDark = context.isDark;

//     return InkWell(
//       onTap: () => context.go('/search?specialization=${item.id}'),
//       borderRadius: BorderRadius.circular(18),
//       child: Container(
//         width: 82,
//         decoration: BoxDecoration(
//           color: isDark ? _Clr.surfaceDark : Colors.white,
//           borderRadius: BorderRadius.circular(18),
//           border: Border.all(
//             color: isDark ? _Clr.cardBorderDark : _Clr.cardBorder,
//           ),
//           boxShadow: [
//             BoxShadow(
//               color: Colors.black.withOpacity(.05),
//               blurRadius: 10,
//               offset: const Offset(0, 4),
//             ),
//           ],
//         ),
//         child: Column(
//           mainAxisAlignment: MainAxisAlignment.center,
//           children: [
//             Container(
//               width: 42,
//               height: 42,
//               decoration: BoxDecoration(
//                 color: _Clr.teal.withOpacity(.10),
//                 borderRadius: BorderRadius.circular(14),
//               ),
//               child: Icon(icon, color: _Clr.teal, size: 22),
//             ),
//             const SizedBox(height: 6),
//             Padding(
//               padding: const EdgeInsets.symmetric(horizontal: 6),
//               child: Text(
//                 item.name,
//                 maxLines: 2,
//                 overflow: TextOverflow.ellipsis,
//                 textAlign: TextAlign.center,
//                 style: TextStyle(
//                   fontSize: 10,
//                   fontWeight: FontWeight.w800,
//                   height: 1.3,
//                   color: isDark ? Colors.white : const Color(0xFF1E2D3D),
//                 ),
//               ),
//             ),
//           ],
//         ),
//       ),
//     );
//   }
// }

// // ─── Guest Lookup Banner ──────────────────────────────────────────────────────

// class _GuestLookupBanner extends StatelessWidget {
//   const _GuestLookupBanner({required this.onTap});
//   final VoidCallback onTap;

//   @override
//   Widget build(BuildContext context) => GestureDetector(
//         onTap: onTap,
//         child: Container(
//           padding: const EdgeInsets.all(18),
//           decoration: BoxDecoration(
//             color: context.isDark ? _Clr.surfaceDark : const Color(0xFFF0FAFA),
//             borderRadius: BorderRadius.circular(22),
//             border: Border.all(
//               color: _Clr.teal.withOpacity(.20),
//             ),
//           ),
//           child: Row(
//             children: [
//               Container(
//                 width: 48,
//                 height: 48,
//                 decoration: BoxDecoration(
//                   color: _Clr.teal.withOpacity(.12),
//                   borderRadius: BorderRadius.circular(15),
//                 ),
//                 child: const Icon(
//                   Icons.manage_search_rounded,
//                   color: _Clr.teal,
//                   size: 26,
//                 ),
//               ),
//               const SizedBox(width: 14),
//               const Expanded(
//                 child: Column(
//                   crossAxisAlignment: CrossAxisAlignment.start,
//                   children: [
//                     Text(
//                       'متابعة حجز بدون حساب',
//                       style: TextStyle(
//                         fontWeight: FontWeight.w900,
//                         fontSize: 14,
//                       ),
//                     ),
//                     SizedBox(height: 3),
//                     Text(
//                       'أدخل رقم هاتفك والكود لعرض حجزك أو إلغائه.',
//                       style: TextStyle(
//                         color: _Clr.muted,
//                         fontSize: 12,
//                         height: 1.5,
//                       ),
//                     ),
//                   ],
//                 ),
//               ),
//               const Icon(
//                 Icons.arrow_back_ios_new_rounded,
//                 size: 14,
//                 color: _Clr.teal,
//               ),
//             ],
//           ),
//         ),
//       );
// }

// // ─── Shared Widgets ───────────────────────────────────────────────────────────

// class _SectionHeader extends StatelessWidget {
//   const _SectionHeader({required this.title, this.action, this.onAction});

//   final String title;
//   final String? action;
//   final VoidCallback? onAction;

//   @override
//   Widget build(BuildContext context) => Row(
//         children: [
//           // Teal accent line
//           Container(
//             width: 4,
//             height: 20,
//             decoration: BoxDecoration(
//               color: _Clr.teal,
//               borderRadius: BorderRadius.circular(4),
//             ),
//           ),
//           const SizedBox(width: 8),
//           Expanded(
//             child: Text(
//               title,
//               style: const TextStyle(
//                 fontSize: 17,
//                 fontWeight: FontWeight.w900,
//               ),
//             ),
//           ),
//           if (action != null)
//             GestureDetector(
//               onTap: onAction,
//               child: Container(
//                 padding:
//                     const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
//                 decoration: BoxDecoration(
//                   color: _Clr.teal.withOpacity(.10),
//                   borderRadius: BorderRadius.circular(10),
//                 ),
//                 child: Text(
//                   action!,
//                   style: const TextStyle(
//                     color: _Clr.teal,
//                     fontSize: 11,
//                     fontWeight: FontWeight.w900,
//                   ),
//                 ),
//               ),
//             ),
//         ],
//       );
// }

// class _InlineError extends StatelessWidget {
//   const _InlineError({required this.text, this.onRetry});

//   final String text;
//   final VoidCallback? onRetry;

//   @override
//   Widget build(BuildContext context) => Container(
//         padding: const EdgeInsets.all(16),
//         decoration: BoxDecoration(
//           color: _Clr.danger.withOpacity(.06),
//           borderRadius: BorderRadius.circular(18),
//           border: Border.all(color: _Clr.danger.withOpacity(.20)),
//         ),
//         child: Column(
//           mainAxisSize: MainAxisSize.min,
//           children: [
//             Text(
//               text,
//               textAlign: TextAlign.center,
//               style: const TextStyle(
//                 color: _Clr.danger,
//                 fontWeight: FontWeight.w700,
//                 fontSize: 13,
//               ),
//             ),
//             if (onRetry != null) ...[
//               const SizedBox(height: 8),
//               TextButton.icon(
//                 onPressed: onRetry,
//                 icon: const Icon(Icons.refresh_rounded, color: _Clr.teal),
//                 label: const Text(
//                   'إعادة المحاولة',
//                   style: TextStyle(color: _Clr.teal),
//                 ),
//               ),
//             ],
//           ],
//         ),
//       );
// }
