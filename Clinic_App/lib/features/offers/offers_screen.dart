import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../core/analytics_service.dart';
import '../../core/api_client.dart';
import '../../core/app_theme.dart';
import '../../widgets/app_scaffold.dart';
import '../directory/directory_service.dart';
import '../directory/models/directory_models.dart';
import '../auth/auth_controller.dart';

class OffersScreen extends StatefulWidget {
  const OffersScreen({super.key});

  @override
  State<OffersScreen> createState() => _OffersScreenState();
}

class _OffersScreenState extends State<OffersScreen> {
  late final DirectoryService _service;
  final _scrollController = ScrollController();
  late final AnalyticsService _analytics;
  final List<DoctorOffer> _offers = [];
  int _page = 1;
  int _totalPages = 1;
  int _totalItems = 0;
  bool _loading = true;
  bool _loadingMore = false;
  String? _error;

  bool get _hasMore => _page < _totalPages;

  @override
  void initState() {
    super.initState();
    final api = context.read<AuthController>().api;
    _service = DirectoryService(api);
    _analytics = AnalyticsService(api);
    _analytics.trackLater(eventType: 'page_viewed', page: 'offers');
    _scrollController.addListener(_onScroll);
    _loadOffers();
  }

  @override
  void dispose() {
    _scrollController.removeListener(_onScroll);
    _scrollController.dispose();
    super.dispose();
  }

  void _onScroll() {
    if (!_scrollController.hasClients ||
        _loading ||
        _loadingMore ||
        !_hasMore) {
      return;
    }
    final position = _scrollController.position;
    if (position.pixels >= position.maxScrollExtent - 260) {
      _loadOffers(reset: false);
    }
  }

  Future<void> _loadOffers({bool reset = true}) async {
    final nextPage = reset ? 1 : _page + 1;
    setState(() {
      if (reset) {
        _loading = true;
      } else {
        _loadingMore = true;
      }
      _error = null;
    });

    try {
      final result = await _service.getOffers(page: nextPage);
      if (!mounted) return;
      setState(() {
        _page = result.currentPage;
        _totalPages = result.totalPages;
        _totalItems = result.totalItems;
        if (reset) {
          _offers
            ..clear()
            ..addAll(result.items);
        } else {
          _offers.addAll(result.items);
        }
      });
      _trackOfferViews(result.items);
    } catch (error) {
      if (!mounted) return;
      setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) {
        setState(() {
          _loading = false;
          _loadingMore = false;
        });
      }
    }
  }

  void _trackOfferViews(List<DoctorOffer> items) {
    for (final offer in items) {
      _analytics.trackOnce(
        key: 'offers-list-${offer.id}',
        eventType: 'offer_viewed',
        doctorId: offer.doctorId,
        offerId: offer.id,
        source: 'offers_page',
        page: 'offers',
      );
    }
  }

  @override
  Widget build(BuildContext context) => AppScaffold(
    title: 'العروض',
    showBackButton: true,
    backRoute: '/',
    child: RefreshIndicator(
      onRefresh: () => _loadOffers(),
      child: ListView(
        controller: _scrollController,
        padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
        children: [
          const Text(
            'عروض الأطباء',
            style: TextStyle(fontSize: 25, fontWeight: FontWeight.w900),
          ),
          const SizedBox(height: 5),
          Text(
            _totalItems > 0
                ? 'تم العثور على $_totalItems عرض فعال لفترة محدودة.'
                : 'تابع العروض الفعالة واحجز مع الطبيب المناسب.',
            style: const TextStyle(color: AppColors.muted),
          ),
          const SizedBox(height: 16),
          if (_loading)
            const Padding(
              padding: EdgeInsets.all(36),
              child: Center(child: CircularProgressIndicator()),
            )
          else if (_error != null)
            _OfferMessage(
              icon: Icons.wifi_off_outlined,
              title: 'تعذر تحميل العروض',
              text: _error!,
              action: () => _loadOffers(),
            )
          else if (_offers.isEmpty)
            const _OfferMessage(
              icon: Icons.local_offer_outlined,
              title: 'لا توجد عروض حالياً',
              text: 'ستظهر هنا العروض الفعالة من الأطباء فور توفرها.',
            )
          else ...[
            ..._offers.map(
              (offer) => Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: OfferCard(
                  offer: offer,
                  onTap: () {
                    _analytics.trackLater(
                      eventType: 'offer_clicked',
                      doctorId: offer.doctorId,
                      offerId: offer.id,
                      source: 'offers_page',
                      page: 'offers',
                    );
                    context.push('/doctors/${offer.doctorId}?source=offer&offerId=${offer.id}');
                  },
                ),
              ),
            ),
            if (_loadingMore)
              const Padding(
                padding: EdgeInsets.all(14),
                child: Center(child: CircularProgressIndicator()),
              ),
          ],
        ],
      ),
    ),
  );
}

class OfferCard extends StatelessWidget {
  const OfferCard({super.key, required this.offer, required this.onTap});

  final DoctorOffer offer;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final featured = offer.isFeatured;
    const premiumColor = Color(0xFFD49A00);
    final accent = featured ? premiumColor : AppColors.primary;
    final iconBackground =
        featured ? const Color(0xFFFFF5D8) : const Color(0xFFEAF6F8);

    return Card(
    color: Colors.white,
    elevation: featured ? 2 : 1,
    shadowColor: (featured ? premiumColor : AppColors.primary)
        .withValues(alpha: featured ? .18 : .08),
    shape: RoundedRectangleBorder(
      borderRadius: BorderRadius.circular(8),
      side: BorderSide(
        color: featured ? const Color(0xFFE8C76D) : AppColors.border,
        width: featured ? 1.2 : 1,
      ),
    ),
    child: InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(8),
      child: Padding(
        padding: const EdgeInsets.all(15),
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
                children: [
                  Row(
                    children: [
                      if (featured) ...[
                        const _OfferBadge(text: 'طبيب مميز', featured: true),
                        const SizedBox(width: 7),
                      ] else if (offer.badgeText?.isNotEmpty == true) ...[
                        _OfferBadge(text: offer.badgeText!),
                        const SizedBox(width: 7),
                      ],
                      Expanded(
                        child: Text(
                          offer.priceText,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: TextStyle(
                            color: accent,
                            fontWeight: FontWeight.w900,
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 5),
                  Text(
                    offer.title,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      fontSize: 16,
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
                  const SizedBox(height: 7),
                  Wrap(
                    spacing: 8,
                    runSpacing: 5,
                    children: [
                      _MiniMeta(icon: Icons.apartment_rounded, text: offer.scope),
                      _MiniMeta(
                        icon: Icons.schedule_rounded,
                        text: '${offer.remainingDays} يوم متبقي',
                      ),
                    ],
                  ),
                ],
              ),
            ),
            const SizedBox(width: 8),
            Icon(
              Icons.arrow_back_ios_new_rounded,
              size: 17,
              color: featured ? premiumColor : AppColors.text,
            ),
          ],
        ),
      ),
    ),
  );
}

}

class _OfferBadge extends StatelessWidget {
  const _OfferBadge({required this.text, this.featured = false});

  final String text;
  final bool featured;

  @override
  Widget build(BuildContext context) {
    final color = featured ? const Color(0xFFD49A00) : AppColors.primary;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: featured ? const Color(0xFFFFFBF0) : const Color(0xFFEAF6F8),
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

class _MiniMeta extends StatelessWidget {
  const _MiniMeta({required this.icon, required this.text});
  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) => Row(
    mainAxisSize: MainAxisSize.min,
    children: [
      Icon(icon, size: 15, color: AppColors.muted),
      const SizedBox(width: 3),
      Text(
        text,
        style: const TextStyle(color: AppColors.muted, fontSize: 12),
      ),
    ],
  );
}

class _OfferMessage extends StatelessWidget {
  const _OfferMessage({
    required this.icon,
    required this.title,
    required this.text,
    this.action,
  });

  final IconData icon;
  final String title;
  final String text;
  final VoidCallback? action;

  @override
  Widget build(BuildContext context) => Card(
    child: Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        children: [
          Icon(icon, size: 44, color: AppColors.muted),
          const SizedBox(height: 10),
          Text(
            title,
            style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w900),
          ),
          const SizedBox(height: 5),
          Text(
            text,
            textAlign: TextAlign.center,
            style: const TextStyle(color: AppColors.muted),
          ),
          if (action != null) ...[
            const SizedBox(height: 12),
            TextButton(onPressed: action, child: const Text('إعادة المحاولة')),
          ],
        ],
      ),
    ),
  );
}
