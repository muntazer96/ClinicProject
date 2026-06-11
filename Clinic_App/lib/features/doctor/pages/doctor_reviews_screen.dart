import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorManageReviewsScreen extends StatefulWidget {
  const DoctorManageReviewsScreen({super.key});

  @override
  State<DoctorManageReviewsScreen> createState() =>
      _DoctorManageReviewsScreenState();
}

class _DoctorManageReviewsScreenState extends State<DoctorManageReviewsScreen> {
  late final DoctorService _service;
  DoctorReviewsSummary? _summary;
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
      _summary = await _service.getReviews();
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
        title: 'التقييمات',
        child: RefreshIndicator(
          onRefresh: _load,
          child: _loading
              ? const Center(child: CircularProgressIndicator())
              : (_summary == null || _summary!.reviews.isEmpty)
                  ? const DoctorEmptyState(
                      icon: Icons.star_border_rounded,
                      message: 'لا توجد تقييمات لعرضها حالياً.',
                    )
                  : ListView(
                      padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
                      children: [
                        _ReviewsSummaryCard(summary: _summary!),
                        const SizedBox(height: 14),
                        const _SectionTitle(title: 'آراء المرضى'),
                        const SizedBox(height: 10),
                        ..._summary!.reviews.map(
                          (item) => _ReviewCard(item: item),
                        ),
                      ],
                    ),
        ),
      );
}

class _ReviewsSummaryCard extends StatelessWidget {
  const _ReviewsSummaryCard({required this.summary});

  final DoctorReviewsSummary summary;

  @override
  Widget build(BuildContext context) {
    final rating = summary.averageRating ?? 0;
    final count = summary.reviewCount;

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
            width: 66,
            height: 66,
            decoration: BoxDecoration(
              color: Colors.white.withOpacity(.16),
              borderRadius: BorderRadius.circular(20),
            ),
            child: const Icon(
              Icons.star_rounded,
              color: Color(0xFFFFD166),
              size: 40,
            ),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'متوسط تقييم الطبيب',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 14,
                    fontWeight: FontWeight.w900,
                  ),
                ),
                const SizedBox(height: 6),
                Row(
                  children: [
                    Text(
                      rating == 0 ? '-' : rating.toStringAsFixed(1),
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 32,
                        fontWeight: FontWeight.w900,
                      ),
                    ),
                    const SizedBox(width: 8),
                    _Stars(rating: rating),
                  ],
                ),
                const SizedBox(height: 4),
                Text(
                  '$count تقييم من المرضى',
                  style: TextStyle(
                    color: Colors.white.withOpacity(.82),
                    fontSize: 12.5,
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

class _ReviewCard extends StatelessWidget {
  const _ReviewCard({required this.item});

final DoctorReview item;

  @override
  Widget build(BuildContext context) {
    final rating = item.rating.toDouble();

    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: const Color(0xFFDDE9E7)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(.025),
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
              CircleAvatar(
                radius: 22,
                backgroundColor: const Color(0xFFEAF7F5),
                child: Text(
                  item.userName.isEmpty ? '?' : item.userName.characters.first,
                  style: const TextStyle(
                    color: AppColors.primary,
                    fontWeight: FontWeight.w900,
                  ),
                ),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: Text(
                  item.userName,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontSize: 15,
                    fontWeight: FontWeight.w900,
                  ),
                ),
              ),
              _RatingBadge(rating: item.rating),
            ],
          ),
          const SizedBox(height: 12),
          _Stars(rating: rating, small: true),
          if (item.comment.trim().isNotEmpty) ...[
            const SizedBox(height: 10),
            Text(
              item.comment,
              style: TextStyle(
                fontSize: 13,
                height: 1.55,
                color: Colors.grey.shade800,
                fontWeight: FontWeight.w600,
              ),
            ),
          ],
          const SizedBox(height: 12),
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: const Color(0xFFF7FAFA),
              borderRadius: BorderRadius.circular(13),
              border: Border.all(color: const Color(0xFFE3ECEA)),
            ),
            child: Row(
              children: [
                const Icon(
                  Icons.info_outline_rounded,
                  color: AppColors.muted,
                  size: 17,
                ),
                const SizedBox(width: 7),
                Expanded(
                  child: Text(
                    'الرد على التقييم غير متاح من الـ API الحالي.',
                    style: TextStyle(
                      color: Colors.grey.shade600,
                      fontSize: 11.5,
                      fontWeight: FontWeight.w700,
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

class _Stars extends StatelessWidget {
  const _Stars({
    required this.rating,
    this.small = false,
  });

  final double rating;
  final bool small;

  @override
  Widget build(BuildContext context) {
    final fullStars = rating.floor().clamp(0, 5);
    final size = small ? 17.0 : 19.0;

    return Row(
      mainAxisSize: MainAxisSize.min,
      children: List.generate(5, (index) {
        final active = index < fullStars;

        return Icon(
          Icons.star_rounded,
          size: size,
          color: active ? const Color(0xFFFFC857) : Colors.white.withOpacity(.38),
        );
      }),
    );
  }
}

class _RatingBadge extends StatelessWidget {
  const _RatingBadge({required this.rating});

  final int rating;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 6),
      decoration: BoxDecoration(
        color: const Color(0xFFFFF7DF),
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: const Color(0xFFE8CF83)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Icon(Icons.star_rounded, color: Color(0xFFD6A20B), size: 15),
          const SizedBox(width: 3),
          Text(
            '$rating',
            style: const TextStyle(
              color: Color(0xFF9A6A08),
              fontSize: 12,
              fontWeight: FontWeight.w900,
            ),
          ),
        ],
      ),
    );
  }
}

class _SectionTitle extends StatelessWidget {
  const _SectionTitle({required this.title});

  final String title;

  @override
  Widget build(BuildContext context) => Text(
        title,
        style: const TextStyle(
          fontSize: 16,
          fontWeight: FontWeight.w900,
        ),
      );
}