import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
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
  String _filter = 'active';
  String? _error;

  List<BookingDetails> get _filteredBookings {
    final sorted = [..._bookings]
      ..sort((a, b) => b.appointmentDate.compareTo(a.appointmentDate));
    return switch (_filter) {
      'active' => sorted.where((booking) => booking.canCancel).toList(),
      'completed' => sorted.where((booking) => booking.status == 3).toList(),
      'cancelled' => sorted.where((booking) => booking.status == 2).toList(),
      _ => sorted,
    };
  }

  int get _activeCount =>
      _bookings.where((booking) => booking.canCancel).length;
  int get _completedCount =>
      _bookings.where((booking) => booking.status == 3).length;
  int get _cancelledCount =>
      _bookings.where((booking) => booking.status == 2).length;
  BookingDetails? get _nextBooking {
    final today = DateTime.now();
    final startOfToday = DateTime(today.year, today.month, today.day);
    final upcoming =
        _bookings
            .where(
              (booking) =>
                  booking.canCancel &&
                  !booking.appointmentDate.isBefore(startOfToday),
            )
            .toList()
          ..sort((a, b) => a.appointmentDate.compareTo(b.appointmentDate));
    return upcoming.isEmpty ? null : upcoming.first;
  }

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
          icon: const Icon(Icons.star_rounded, color: AppColors.warning),
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
            OutlinedButton(
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
    final accepted = await showCancelBookingDialog(
      context: context,
      queueNumber: booking.queueNumber,
      doctorName: booking.doctorName,
    );
    if (accepted != true) return;
    try {
      await _service.cancelMyBooking(booking.id);
      await _load();
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(const SnackBar(content: Text('تم إلغاء الحجز.')));
      }
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    }
  }

  @override
  Widget build(BuildContext context) {
    final filtered = _filteredBookings;
    return AppScaffold(
      title: 'حجوزاتي',
      child: RefreshIndicator(
        onRefresh: _load,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
          children: [
            Row(
              children: [
                const Expanded(
                  child: Text(
                    'حجوزاتي',
                    style: TextStyle(fontSize: 24, fontWeight: FontWeight.w900),
                  ),
                ),
                IconButton(
                  onPressed: _loading ? null : _load,
                  icon: const Icon(Icons.refresh_rounded),
                  tooltip: 'تحديث',
                ),
              ],
            ),
            const SizedBox(height: 5),
            const Text(
              'راجع أدوارك الحالية والسابقة أو ألغِ الحجز قبل موعده.',
              style: TextStyle(color: AppColors.muted),
            ),
            const SizedBox(height: 14),
            _BookingSummary(
              active: _activeCount,
              completed: _completedCount,
              cancelled: _cancelledCount,
            ),
            if (_nextBooking != null) ...[
              const SizedBox(height: 12),
              _NextBookingBanner(booking: _nextBooking!),
            ],
            const SizedBox(height: 12),
            _BookingFilterBar(
              value: _filter,
              onChanged: (value) => setState(() => _filter = value),
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
            else if (filtered.isEmpty)
              const BookingMessage(text: 'لا توجد حجوزات ضمن هذا الفلتر.')
            else
              ...filtered.map(
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
}

Future<bool?> showCancelBookingDialog({
  required BuildContext context,
  required int queueNumber,
  String? doctorName,
}) {
  return showDialog<bool>(
    context: context,
    builder: (context) => AlertDialog(
      icon: const Icon(
        Icons.cancel_outlined,
        color: AppColors.danger,
        size: 34,
      ),
      title: const Text('إلغاء الحجز'),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'هل تريد إلغاء حجز الدور #$queueNumber؟',
            style: const TextStyle(fontWeight: FontWeight.w900),
          ),
          if (doctorName?.isNotEmpty == true) ...[
            const SizedBox(height: 8),
            Text(doctorName!, style: const TextStyle(color: AppColors.muted)),
          ],
        ],
      ),
      actionsPadding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
      actions: [
        Row(
          children: [
            Expanded(
              child: OutlinedButton(
                onPressed: () => Navigator.pop(context, false),
                child: const Text('تراجع'),
              ),
            ),
            const SizedBox(width: 8),
            Expanded(
              child: FilledButton(
                style: FilledButton.styleFrom(
                  backgroundColor: AppColors.danger,
                ),
                onPressed: () => Navigator.pop(context, true),
                child: const Text('تأكيد الإلغاء'),
              ),
            ),
          ],
        ),
      ],
    ),
  );
}

class _BookingSummary extends StatelessWidget {
  const _BookingSummary({
    required this.active,
    required this.completed,
    required this.cancelled,
  });

  final int active;
  final int completed;
  final int cancelled;

  @override
  Widget build(BuildContext context) => Row(
    children: [
      Expanded(
        child: _SummaryTile(
          label: 'نشطة',
          value: active,
          color: AppColors.primary,
        ),
      ),
      const SizedBox(width: 8),
      Expanded(
        child: _SummaryTile(
          label: 'مكتملة',
          value: completed,
          color: AppColors.success,
        ),
      ),
      const SizedBox(width: 8),
      Expanded(
        child: _SummaryTile(
          label: 'ملغية',
          value: cancelled,
          color: AppColors.danger,
        ),
      ),
    ],
  );
}

class _SummaryTile extends StatelessWidget {
  const _SummaryTile({
    required this.label,
    required this.value,
    required this.color,
  });

  final String label;
  final int value;
  final Color color;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(12),
    decoration: BoxDecoration(
      color: Colors.white,
      borderRadius: BorderRadius.circular(8),
      border: Border.all(color: AppColors.border),
    ),
    child: Column(
      children: [
        Text(
          '$value',
          style: TextStyle(
            color: color,
            fontSize: 20,
            fontWeight: FontWeight.w900,
          ),
        ),
        Text(label, style: const TextStyle(color: AppColors.muted)),
      ],
    ),
  );
}

class _NextBookingBanner extends StatelessWidget {
  const _NextBookingBanner({required this.booking});
  final BookingDetails booking;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(14),
    decoration: BoxDecoration(
      color: AppColors.softBlue,
      borderRadius: BorderRadius.circular(8),
      border: Border.all(color: AppColors.border),
    ),
    child: Row(
      children: [
        const Icon(Icons.event_available_rounded, color: AppColors.primary),
        const SizedBox(width: 10),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'أقرب حجز قادم',
                style: TextStyle(fontWeight: FontWeight.w900),
              ),
              const SizedBox(height: 3),
              Text(
                '${booking.doctorName} - ${DateFormat('yyyy/MM/dd').format(booking.appointmentDate)} - الدور #${booking.queueNumber}',
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
            ],
          ),
        ),
      ],
    ),
  );
}

class _BookingFilterBar extends StatelessWidget {
  const _BookingFilterBar({required this.value, required this.onChanged});

  final String value;
  final ValueChanged<String> onChanged;

  @override
  Widget build(BuildContext context) => SingleChildScrollView(
    scrollDirection: Axis.horizontal,
    child: Row(
      children: [
        _FilterChip('active', 'النشطة', value, onChanged),
        _FilterChip('all', 'الكل', value, onChanged),
        _FilterChip('completed', 'المكتملة', value, onChanged),
        _FilterChip('cancelled', 'الملغية', value, onChanged),
      ],
    ),
  );
}

class _FilterChip extends StatelessWidget {
  const _FilterChip(this.id, this.label, this.value, this.onChanged);

  final String id;
  final String label;
  final String value;
  final ValueChanged<String> onChanged;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsetsDirectional.only(end: 8),
    child: ChoiceChip(
      selected: value == id,
      label: Text(label),
      onSelected: (_) => onChanged(id),
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

  Future<void> _copyCode(BuildContext context) async {
    await Clipboard.setData(ClipboardData(text: booking.code));
    if (!context.mounted) return;
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(const SnackBar(content: Text('تم نسخ كود الحجز.')));
  }

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
          _InfoRow(
            icon: Icons.event_outlined,
            text:
                'التاريخ: ${DateFormat('yyyy/MM/dd').format(booking.appointmentDate)}',
          ),
          _InfoRow(
            icon: Icons.confirmation_number_outlined,
            text: 'رقم الدور: #${booking.queueNumber}',
          ),
          Row(
            children: [
              Expanded(
                child: _InfoRow(
                  icon: Icons.key_rounded,
                  text: 'كود الحجز: ${booking.code}',
                ),
              ),
              IconButton(
                onPressed: booking.code.isEmpty
                    ? null
                    : () => _copyCode(context),
                icon: const Icon(Icons.copy_rounded),
                tooltip: 'نسخ الكود',
              ),
            ],
          ),
          if (booking.clinicAddress.isNotEmpty)
            _InfoRow(
              icon: Icons.place_outlined,
              text: 'العنوان: ${booking.clinicAddress}',
            ),
          if (booking.clinicPhoneNumber?.isNotEmpty == true ||
              booking.mapUrl?.isNotEmpty == true) ...[
            const SizedBox(height: 8),
            Row(
              children: [
                if (booking.clinicPhoneNumber?.isNotEmpty == true)
                  Expanded(
                    child: OutlinedButton.icon(
                      onPressed: () =>
                          openPhone(context, booking.clinicPhoneNumber!),
                      icon: const Icon(Icons.phone_outlined),
                      label: const Text('اتصال'),
                    ),
                  ),
                if (booking.clinicPhoneNumber?.isNotEmpty == true &&
                    booking.mapUrl?.isNotEmpty == true)
                  const SizedBox(width: 8),
                if (booking.mapUrl?.isNotEmpty == true)
                  Expanded(
                    child: OutlinedButton.icon(
                      onPressed: () => openMap(context, booking.mapUrl!),
                      icon: const Icon(Icons.map_outlined),
                      label: const Text('الموقع'),
                    ),
                  ),
              ],
            ),
          ],
          if (booking.cancellationReason?.isNotEmpty == true)
            _InfoRow(
              icon: Icons.info_outline,
              text: 'سبب الإلغاء: ${booking.cancellationReason}',
            ),
          if (onCancel != null || onReview != null) ...[
            const SizedBox(height: 10),
            Row(
              children: [
                if (onCancel != null)
                  Expanded(
                    child: OutlinedButton.icon(
                      onPressed: onCancel,
                      icon: const Icon(Icons.close),
                      label: const Text('إلغاء الحجز'),
                    ),
                  ),
                if (onCancel != null && onReview != null)
                  const SizedBox(width: 8),
                if (onReview != null)
                  Expanded(
                    child: FilledButton.icon(
                      onPressed: onReview,
                      icon: const Icon(Icons.star_outline),
                      label: const Text('تقييم الطبيب'),
                    ),
                  ),
              ],
            ),
          ],
        ],
      ),
    ),
  );
}

class _InfoRow extends StatelessWidget {
  const _InfoRow({required this.icon, required this.text});

  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: 7),
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 18, color: AppColors.primary),
        const SizedBox(width: 8),
        Expanded(child: Text(text)),
      ],
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
      2 => (const Color(0xFFFFECEC), AppColors.danger),
      3 => (const Color(0xFFE4F4F0), AppColors.success),
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
