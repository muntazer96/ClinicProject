import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_theme.dart';
import '../../../core/external_links.dart';
import '../../../widgets/app_scaffold.dart';
import '../../auth/auth_controller.dart';
import '../../reviews/review_service.dart';
import '../booking_service.dart';
import '../models/booking_models.dart';

class MyBookingsScreen extends StatefulWidget {
  const MyBookingsScreen({super.key});
  @override
  State<MyBookingsScreen> createState() => _MyBookingsScreenState();
}

class _MyBookingsScreenState extends State<MyBookingsScreen> {
  late final BookingService _service;
  late final ReviewService _reviewService;
  List<BookingDetails> _bookings = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _service = BookingService(context.read<AuthController>().api);
    _reviewService = ReviewService(context.read<AuthController>().api);
    _load();
  }

  Future<void> _review(BookingDetails booking) async {
    var rating = 5;
    final comment = TextEditingController();
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          title: const Text('تقييم الطبيب'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: List.generate(
                  5,
                  (index) => IconButton(
                    onPressed: () => setDialogState(() => rating = index + 1),
                    icon: Icon(
                      index < rating ? Icons.star_rounded : Icons.star_outline,
                      color: const Color(0xFFFFB54A),
                    ),
                  ),
                ),
              ),
              TextField(
                controller: comment,
                maxLength: 1000,
                maxLines: 3,
                decoration: const InputDecoration(labelText: 'اكتب تعليقك'),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context, false),
              child: const Text('تراجع'),
            ),
            FilledButton(
              onPressed: () => Navigator.pop(context, true),
              child: const Text('إرسال التقييم'),
            ),
          ],
        ),
      ),
    );
    if (accepted != true) {
      comment.dispose();
      return;
    }
    if (comment.text.trim().isEmpty) {
      comment.dispose();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('اكتب تعليقاً قبل إرسال التقييم.')),
        );
      }
      return;
    }
    try {
      await _reviewService.addReview(
        doctorId: booking.doctorId,
        appointmentId: booking.id,
        rating: rating,
        comment: comment.text,
      );
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('شكراً لك، تم إرسال تقييمك.')),
        );
      }
      await _load();
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      comment.dispose();
    }
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final bookings = await _service.getMyBookings();
      if (mounted) setState(() => _bookings = bookings);
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _cancel(BookingDetails booking) async {
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('إلغاء الحجز'),
        content: Text('هل تريد إلغاء حجز الدور #${booking.queueNumber}؟'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('تراجع'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('تأكيد الإلغاء'),
          ),
        ],
      ),
    );
    if (accepted != true) return;
    try {
      await _service.cancelMyBooking(booking.id);
      await _load();
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    }
  }

  @override
  Widget build(BuildContext context) => AppScaffold(
    title: 'حجوزاتي',
    child: RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
        children: [
          const Text(
            'حجوزاتي',
            style: TextStyle(fontSize: 24, fontWeight: FontWeight.w900),
          ),
          const SizedBox(height: 5),
          const Text(
            'راجع أدوارك الحالية والسابقة أو ألغِ الحجز قبل موعده.',
            style: TextStyle(color: AppColors.muted),
          ),
          const SizedBox(height: 14),
          if (_loading)
            const Padding(
              padding: EdgeInsets.all(34),
              child: Center(child: CircularProgressIndicator()),
            )
          else if (_error != null)
            BookingMessage(text: _error!, action: _load)
          else if (_bookings.isEmpty)
            const BookingMessage(text: 'لا توجد حجوزات في حسابك حتى الآن.')
          else
            ..._bookings.map(
              (booking) => Padding(
                padding: const EdgeInsets.only(bottom: 10),
                child: BookingCard(
                  booking: booking,
                  onCancel: booking.canCancel ? () => _cancel(booking) : null,
                  onReview: booking.canReview ? () => _review(booking) : null,
                ),
              ),
            ),
        ],
      ),
    ),
  );
}

class BookingCard extends StatelessWidget {
  const BookingCard({
    super.key,
    required this.booking,
    this.onCancel,
    this.onReview,
  });
  final BookingDetails booking;
  final VoidCallback? onCancel;
  final VoidCallback? onReview;

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
                  booking.doctorName,
                  style: const TextStyle(
                    fontSize: 17,
                    fontWeight: FontWeight.w900,
                  ),
                ),
              ),
              _StatusChip(booking: booking),
            ],
          ),
          Text(
            booking.clinicName,
            style: const TextStyle(color: AppColors.primary),
          ),
          const Divider(height: 20),
          Text(
            'التاريخ: ${DateFormat('yyyy/MM/dd').format(booking.appointmentDate)}',
          ),
          Text('رقم الدور: #${booking.queueNumber}'),
          Text('كود الحجز: ${booking.code}'),
          if (booking.clinicAddress.isNotEmpty)
            Text('العنوان: ${booking.clinicAddress}'),
          if (booking.clinicPhoneNumber?.isNotEmpty == true)
            TextButton.icon(
              onPressed: () => openPhone(context, booking.clinicPhoneNumber!),
              icon: const Icon(Icons.phone_outlined),
              label: Text('اتصال بالعيادة: ${booking.clinicPhoneNumber}'),
            ),
          if (booking.mapUrl?.isNotEmpty == true)
            TextButton.icon(
              onPressed: () => openMap(context, booking.mapUrl!),
              icon: const Icon(Icons.map_outlined),
              label: const Text('فتح موقع العيادة'),
            ),
          if (booking.cancellationReason?.isNotEmpty == true)
            Text('سبب الإلغاء: ${booking.cancellationReason}'),
          if (onCancel != null) ...[
            const SizedBox(height: 8),
            OutlinedButton.icon(
              onPressed: onCancel,
              icon: const Icon(Icons.close),
              label: const Text('إلغاء الحجز'),
            ),
          ],
          if (onReview != null) ...[
            const SizedBox(height: 8),
            FilledButton.icon(
              onPressed: onReview,
              icon: const Icon(Icons.star_outline),
              label: const Text('تقييم الطبيب'),
            ),
          ],
        ],
      ),
    ),
  );
}

class _StatusChip extends StatelessWidget {
  const _StatusChip({required this.booking});
  final BookingDetails booking;

  @override
  Widget build(BuildContext context) {
    final (background, foreground) = switch (booking.status) {
      1 => (AppColors.softBlue, AppColors.primary),
      2 => (const Color(0xFFFFECEC), const Color(0xFFB23A3A)),
      3 => (const Color(0xFFE4F4F0), const Color(0xFF13796B)),
      _ => (const Color(0xFFFFF0DF), const Color(0xFFB16A2B)),
    };
    return Chip(
      backgroundColor: background,
      side: BorderSide.none,
      label: Text(
        booking.statusLabel,
        style: TextStyle(color: foreground, fontWeight: FontWeight.w700),
      ),
    );
  }
}

class BookingMessage extends StatelessWidget {
  const BookingMessage({super.key, required this.text, this.action});
  final String text;
  final VoidCallback? action;
  @override
  Widget build(BuildContext context) => Card(
    child: Padding(
      padding: const EdgeInsets.all(22),
      child: Column(
        children: [
          const Icon(
            Icons.calendar_month_outlined,
            size: 42,
            color: AppColors.muted,
          ),
          const SizedBox(height: 9),
          Text(text, textAlign: TextAlign.center),
          if (action != null)
            TextButton(onPressed: action, child: const Text('إعادة المحاولة')),
        ],
      ),
    ),
  );
}
