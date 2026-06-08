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
    viewportFraction: .90,
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
              onAction: () => context.go('/specializations'),
            ),
            const SizedBox(height: 12),
            _SpecializationPreview(
              loading: _loadingSpecializations,
              items: _specializations.take(4).toList(),
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
        color: Colors.white,
        borderRadius: BorderRadius.circular(22),
        border: Border.all(color: AppColors.border),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(.045),
            blurRadius: 18,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: const Row(
        children: [
          Icon(Icons.search_rounded, color: AppColors.primary),
          SizedBox(width: 10),
          Expanded(
            child: Text(
              'ابحث عن طبيب، اختصاص أو عيادة',
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

class _OffersSection extends StatelessWidget {
  const _OffersSection({
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
        height: 170,
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(24),
          border: Border.all(color: AppColors.border),
        ),
        child: const Center(child: CircularProgressIndicator()),
      );
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
    final surface = featured ? const Color(0xFFFFFBED) : Colors.white;

    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(26),
      child: Container(
        clipBehavior: Clip.antiAlias,
        decoration: BoxDecoration(
          color: surface,
          borderRadius: BorderRadius.circular(26),
          border: Border.all(
            color: featured ? const Color(0xFFE8B33A) : AppColors.border,
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
                            color: AppColors.text,
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
                            color: AppColors.muted,
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

    return GridView.builder(
      itemCount: items.length,
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 2,
        crossAxisSpacing: 12,
        mainAxisSpacing: 12,
        childAspectRatio: 1.08,
      ),
      itemBuilder: (context, index) {
        return _SpecializationCard(item: items[index]);
      },
    );
  }
}

class _SpecializationCard extends StatelessWidget {
  const _SpecializationCard({required this.item});

  final Specialization item;

  @override
  Widget build(BuildContext context) {
    final icon = specializationIconFor(item.iconName);

    return InkWell(
      onTap: () => context.go('/search?specialization=${item.id}'),
      borderRadius: BorderRadius.circular(24),
      child: Ink(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(24),
          border: Border.all(color: AppColors.border),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(.045),
              blurRadius: 18,
              offset: const Offset(0, 8),
            ),
          ],
        ),
        child: Stack(
          children: [
            PositionedDirectional(
              top: -18,
              end: -22,
              child: Icon(
                icon,
                size: 115,
                color: AppColors.primary.withOpacity(.07),
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(15),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Container(
                    width: 44,
                    height: 44,
                    decoration: BoxDecoration(
                      color: AppColors.primary.withOpacity(.10),
                      borderRadius: BorderRadius.circular(15),
                    ),
                    child: Icon(icon, color: AppColors.primary, size: 24),
                  ),
                  const Spacer(),
                  Text(
                    item.name,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.w900,
                    ),
                  ),
                  const SizedBox(height: 8),
                  const Row(
                    children: [
                      Text(
                        'عرض الأطباء',
                        style: TextStyle(
                          color: AppColors.primary,
                          fontSize: 12,
                          fontWeight: FontWeight.w900,
                        ),
                      ),
                      SizedBox(width: 5),
                      Icon(
                        Icons.arrow_back_rounded,
                        size: 15,
                        color: AppColors.primary,
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

class _EmptySpecializations extends StatelessWidget {
  const _EmptySpecializations();

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(18),
    decoration: BoxDecoration(
      color: Colors.white,
      borderRadius: BorderRadius.circular(22),
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
    borderRadius: BorderRadius.circular(22),
    child: Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(22),
        border: Border.all(color: AppColors.border),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(.035),
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
