import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
import '../models/booking_models.dart';

class ReviewSubmission {
  const ReviewSubmission({required this.rating, required this.comment});
  final int rating;
  final String comment;
}

class ReviewBookingScreen extends StatefulWidget {
  const ReviewBookingScreen({super.key, required this.booking});
  final BookingDetails booking;

  @override
  State<ReviewBookingScreen> createState() => _ReviewBookingScreenState();
}

class _ReviewBookingScreenState extends State<ReviewBookingScreen> {
  final _comment = TextEditingController();
  int _rating = 5;

  @override
  void dispose() {
    _comment.dispose();
    super.dispose();
  }

  void _submit() {
    final comment = _comment.text.trim();
    if (comment.isEmpty) {
      showAppSnackBar(
        context,
        'اكتب تعليقاً قبل إرسال التقييم.',
        type: AppSnackBarType.warning,
      );
      return;
    }
    context.pop(ReviewSubmission(rating: _rating, comment: comment));
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('تقييم الطبيب')),
    body: ListView(
      padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
      children: [
        Container(
          padding: const EdgeInsets.all(18),
          decoration: BoxDecoration(
            color: AppColors.primaryDark,
            borderRadius: BorderRadius.circular(8),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Icon(Icons.star_rounded, color: Colors.white, size: 34),
              const SizedBox(height: 12),
              Text(
                widget.booking.doctorName,
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 22,
                  fontWeight: FontWeight.w900,
                ),
              ),
              const SizedBox(height: 6),
              Text(
                widget.booking.clinicName,
                style: const TextStyle(color: Color(0xFFD7FFFA)),
              ),
            ],
          ),
        ),
        const SizedBox(height: 16),
        Card(
          child: Padding(
            padding: const EdgeInsets.all(14),
            child: Column(
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: List.generate(
                    5,
                    (index) => IconButton(
                      onPressed: () => setState(() => _rating = index + 1),
                      icon: Icon(
                        index < _rating
                            ? Icons.star_rounded
                            : Icons.star_outline,
                        color: const Color(0xFFFFB54A),
                      ),
                    ),
                  ),
                ),
                TextField(
                  controller: _comment,
                  maxLength: 1000,
                  maxLines: 4,
                  decoration: const InputDecoration(labelText: 'اكتب تعليقك'),
                ),
              ],
            ),
          ),
        ),
        const SizedBox(height: 16),
        FilledButton.icon(
          onPressed: _submit,
          icon: const Icon(Icons.send_outlined),
          label: const Text('إرسال التقييم'),
        ),
        const SizedBox(height: 10),
        OutlinedButton.icon(
          onPressed: () => context.pop(),
          icon: const Icon(Icons.arrow_back_rounded),
          label: const Text('العودة'),
        ),
      ],
    ),
  );
}
