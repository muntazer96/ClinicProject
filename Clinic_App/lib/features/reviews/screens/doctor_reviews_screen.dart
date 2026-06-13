import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_theme.dart';
import '../../../widgets/app_scaffold.dart';
import '../models/review_models.dart';
import '../review_service.dart';

class DoctorReviewsScreen extends StatefulWidget {
  const DoctorReviewsScreen({
    super.key,
    required this.doctorId,
    required this.doctorName,
  });

  final int doctorId;
  final String doctorName;

  @override
  State<DoctorReviewsScreen> createState() => _DoctorReviewsScreenState();
}

class _DoctorReviewsScreenState extends State<DoctorReviewsScreen> {
  late final ReviewService _service;
  DoctorReviews? _reviews;
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _service = ReviewService(context.read<ApiClient>());
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final reviews = await _service.getDoctorReviews(widget.doctorId);
      if (mounted) setState(() => _reviews = reviews);
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) => AppScaffold(
    title: 'التقييمات',
    child: RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
        children: [
          Text(
            widget.doctorName,
            style: const TextStyle(fontSize: 23, fontWeight: FontWeight.w900),
          ),
          const SizedBox(height: 10),
          if (_loading)
            const Padding(
              padding: EdgeInsets.all(34),
              child: Center(child: CircularProgressIndicator()),
            )
          else if (_error != null)
            _Message(text: _error!, onRetry: _load)
          else if (_reviews == null || _reviews?.isEnabled != true)
            const _Message(text: 'التقييمات غير متاحة لهذا الطبيب حالياً.')
          else ...[
            _ReviewSummary(reviews: _reviews!),
            const SizedBox(height: 12),
            if (_reviews!.reviews.isEmpty)
              const _Message(text: 'لا توجد تقييمات حتى الآن.')
            else
              ..._reviews!.reviews.map(
                (review) => Padding(
                  padding: const EdgeInsets.only(bottom: 10),
                  child: _ReviewCard(review: review),
                ),
              ),
          ],
        ],
      ),
    ),
  );
}

class _ReviewSummary extends StatelessWidget {
  const _ReviewSummary({required this.reviews});
  final DoctorReviews reviews;

  @override
  Widget build(BuildContext context) => Card(
    child: Padding(
      padding: const EdgeInsets.all(14),
      child: Row(
        children: [
          const Icon(Icons.star_rounded, color: AppColors.warning, size: 30),
          const SizedBox(width: 8),
          Text(
            reviews.averageRating?.toStringAsFixed(1) ?? '-',
            style: const TextStyle(fontSize: 24, fontWeight: FontWeight.w900),
          ),
          const SizedBox(width: 8),
          Text(
            'من 5 (${reviews.reviewCount} تقييم)',
            style: const TextStyle(color: AppColors.muted),
          ),
        ],
      ),
    ),
  );
}

class _ReviewCard extends StatelessWidget {
  const _ReviewCard({required this.review});
  final DoctorReview review;

  @override
  Widget build(BuildContext context) => Card(
    child: Padding(
      padding: const EdgeInsets.all(14),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Expanded(
                child: Text(
                  review.userName,
                  style: const TextStyle(fontWeight: FontWeight.w900),
                ),
              ),
              Row(
                children: List.generate(
                  5,
                  (index) => Icon(
                    index < review.rating
                        ? Icons.star_rounded
                        : Icons.star_outline_rounded,
                    color: AppColors.warning,
                    size: 17,
                  ),
                ),
              ),
            ],
          ),
          if (review.comment.isNotEmpty) ...[
            const SizedBox(height: 8),
            Text(review.comment, style: const TextStyle(height: 1.6)),
          ],
        ],
      ),
    ),
  );
}

class _Message extends StatelessWidget {
  const _Message({required this.text, this.onRetry});
  final String text;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) => Card(
    child: Padding(
      padding: const EdgeInsets.all(22),
      child: Column(
        children: [
          const Icon(
            Icons.star_outline_rounded,
            size: 42,
            color: AppColors.muted,
          ),
          const SizedBox(height: 10),
          Text(text, textAlign: TextAlign.center),
          if (onRetry != null)
            TextButton(onPressed: onRetry, child: const Text('إعادة المحاولة')),
        ],
      ),
    ),
  );
}
