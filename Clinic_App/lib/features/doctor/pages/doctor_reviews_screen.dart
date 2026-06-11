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
                    DoctorSectionCard(
                      child: Row(
                        children: [
                          const Icon(Icons.star_rounded, color: AppColors.accent),
                          const SizedBox(width: 8),
                          Text(
                            '${_summary!.averageRating?.toStringAsFixed(1) ?? '-'}',
                            style: const TextStyle(
                              fontSize: 24,
                              fontWeight: FontWeight.w900,
                            ),
                          ),
                          const SizedBox(width: 8),
                          Text('${_summary!.reviewCount} تقييم'),
                        ],
                      ),
                    ),
                    const SizedBox(height: 12),
                    ..._summary!.reviews.map(
                      (item) => Padding(
                        padding: const EdgeInsets.only(bottom: 10),
                        child: DoctorSectionCard(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                children: [
                                  Expanded(
                                    child: Text(
                                      item.userName,
                                      style: const TextStyle(
                                        fontWeight: FontWeight.w900,
                                      ),
                                    ),
                                  ),
                                  Text('★ ${item.rating}'),
                                ],
                              ),
                              const SizedBox(height: 6),
                              Text(item.comment),
                              const SizedBox(height: 8),
                              const Text(
                                'الرد على التقييم غير متاح من الـ API الحالي.',
                                style: TextStyle(
                                  color: AppColors.muted,
                                  fontSize: 12,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
    ),
  );
}
