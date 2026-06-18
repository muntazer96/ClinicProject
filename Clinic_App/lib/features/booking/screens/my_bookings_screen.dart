import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart' hide TextDirection;
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
import '../../../core/external_links.dart';
import '../../../widgets/app_scaffold.dart';
import '../../auth/auth_controller.dart';
import '../../reviews/review_service.dart';
import '../booking_service.dart';
import '../models/booking_models.dart';
import 'review_booking_screen.dart';

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
  late DateTime _fromDate;
  late DateTime _toDate;

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

  int get _activeCount => _bookings.where((b) => b.canCancel).length;
  int get _completedCount => _bookings.where((b) => b.status == 3).length;
  int get _cancelledCount => _bookings.where((b) => b.status == 2).length;

  BookingDetails? get _nextBooking {
    final now = DateTime.now();
    final today = DateTime(now.year, now.month, now.day);

    final upcoming =
        _bookings
            .where((b) => b.canCancel && !b.appointmentDate.isBefore(today))
            .toList()
          ..sort((a, b) => a.appointmentDate.compareTo(b.appointmentDate));

    return upcoming.isEmpty ? null : upcoming.first;
  }

  @override
  void initState() {
    super.initState();
    _service = BookingService(context.read<AuthController>().api);
    _reviewService = ReviewService(context.read<AuthController>().api);
    final now = DateTime.now();
    _fromDate = DateTime(now.year, now.month, now.day);
    _toDate = _fromDate.add(const Duration(days: 30));
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final bookings = await _service.getMyBookings(
        fromDate: _fromDate,
        toDate: _toDate,
      );
      if (mounted) setState(() => _bookings = bookings);
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _cancel(BookingDetails booking) async {
    try {
      await _service.cancelMyBooking(booking.id);
      await _load();

      if (mounted) {
        showAppSnackBar(
          context,
          'تم إلغاء الحجز.',
          type: AppSnackBarType.success,
        );
      }
    } catch (error) {
      if (mounted) {
        showAppSnackBar(
          context,
          ApiClient.errorMessage(error),
          type: AppSnackBarType.error,
        );
      }
    }
  }

  Future<void> _review(BookingDetails booking) async {
    final submission = await context.push<ReviewSubmission>(
      '/booking/review',
      extra: booking,
    );

    if (submission == null) return;

    try {
      await _reviewService.addReview(
        doctorId: booking.doctorId,
        appointmentId: booking.id,
        rating: submission.rating,
        comment: submission.comment,
      );

      if (mounted) {
        showAppSnackBar(
          context,
          'شكراً لك، تم إرسال تقييمك.',
          type: AppSnackBarType.success,
        );
      }

      await _load();
    } catch (error) {
      if (mounted) {
        showAppSnackBar(
          context,
          ApiClient.errorMessage(error),
          type: AppSnackBarType.error,
        );
      }
    }
  }

  Future<void> _pickDateRange() async {
    final picked = await showDateRangePicker(
      context: context,
      firstDate: DateTime(2020),
      lastDate: DateTime(DateTime.now().year + 2, 12, 31),
      initialDateRange: DateTimeRange(start: _fromDate, end: _toDate),
      helpText: 'اختيار مدة الحجوزات',
      cancelText: 'إلغاء',
      confirmText: 'تطبيق',
      saveText: 'تطبيق',
      builder: (context, child) => Directionality(
        textDirection: TextDirection.rtl,
        child: Theme(data: Theme.of(context), child: child!),
      ),
    );

    if (picked == null) return;

    setState(() {
      _fromDate = DateTime(
        picked.start.year,
        picked.start.month,
        picked.start.day,
      );
      _toDate = DateTime(picked.end.year, picked.end.month, picked.end.day);
    });
    await _load();
  }

  @override
  Widget build(BuildContext context) {
    final filtered = _filteredBookings;

    return AppScaffold(
      title: 'حجوزاتي',
      child: RefreshIndicator(
        onRefresh: _load,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(16, 14, 16, 26),
          children: [
            _PageHeader(loading: _loading, onRefresh: _load),
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
            const SizedBox(height: 14),
            _BookingFilterBar(
              value: _filter,
              onChanged: (value) => setState(() => _filter = value),
            ),
            const SizedBox(height: 10),
            _DateRangeFilter(
              fromDate: _fromDate,
              toDate: _toDate,
              onTap: _loading ? null : _pickDateRange,
            ),
            const SizedBox(height: 14),
            if (_loading)
              const Padding(
                padding: EdgeInsets.all(38),
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
                  padding: const EdgeInsets.only(bottom: 14),
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

class _DateRangeFilter extends StatelessWidget {
  const _DateRangeFilter({
    required this.fromDate,
    required this.toDate,
    required this.onTap,
  });

  final DateTime fromDate;
  final DateTime toDate;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final formatter = DateFormat('yyyy/MM/dd');

    return OutlinedButton.icon(
      onPressed: onTap,
      icon: const Icon(Icons.date_range_rounded),
      label: Text(
        'من ${formatter.format(fromDate)} إلى ${formatter.format(toDate)}',
        overflow: TextOverflow.ellipsis,
      ),
      style: OutlinedButton.styleFrom(
        alignment: Alignment.centerRight,
        foregroundColor: AppColors.primary,
        side: const BorderSide(color: AppColors.border),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 13),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      ),
    );
  }
}

class _PageHeader extends StatelessWidget {
  const _PageHeader({required this.loading, required this.onRefresh});

  final bool loading;
  final VoidCallback onRefresh;

  @override
  Widget build(BuildContext context) => Row(
    children: [
      const Expanded(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'حجوزاتي',
              style: TextStyle(fontSize: 25, fontWeight: FontWeight.w900),
            ),
            SizedBox(height: 4),
            Text(
              'تابع حجوزاتك الحالية والسابقة بسهولة.',
              style: TextStyle(color: AppColors.muted),
            ),
          ],
        ),
      ),
      IconButton.filledTonal(
        onPressed: loading ? null : onRefresh,
        icon: const Icon(Icons.refresh_rounded),
        tooltip: 'تحديث',
      ),
    ],
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
          icon: Icons.event_available_rounded,
          color: AppColors.primary,
        ),
      ),
      const SizedBox(width: 8),
      Expanded(
        child: _SummaryTile(
          label: 'مكتملة',
          value: completed,
          icon: Icons.verified_rounded,
          color: AppColors.success,
        ),
      ),
      const SizedBox(width: 8),
      Expanded(
        child: _SummaryTile(
          label: 'ملغية',
          value: cancelled,
          icon: Icons.cancel_rounded,
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
    required this.icon,
    required this.color,
  });

  final String label;
  final int value;
  final IconData icon;
  final Color color;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(12),
    decoration: BoxDecoration(
      color: Colors.white,
      borderRadius: BorderRadius.circular(18),
      border: Border.all(color: AppColors.border),
      boxShadow: [
        BoxShadow(
          color: Colors.black.withOpacity(.035),
          blurRadius: 14,
          offset: const Offset(0, 6),
        ),
      ],
    ),
    child: Column(
      children: [
        Icon(icon, color: color, size: 23),
        const SizedBox(height: 6),
        Text(
          '$value',
          style: TextStyle(
            color: color,
            fontSize: 22,
            fontWeight: FontWeight.w900,
          ),
        ),
        Text(
          label,
          style: const TextStyle(
            color: AppColors.muted,
            fontWeight: FontWeight.w600,
          ),
        ),
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
      borderRadius: BorderRadius.circular(20),
      border: Border.all(color: AppColors.border),
    ),
    child: Row(
      children: [
        Container(
          width: 46,
          height: 46,
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(16),
          ),
          child: const Icon(
            Icons.notifications_active_rounded,
            color: AppColors.primary,
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'أقرب حجز قادم',
                style: TextStyle(fontWeight: FontWeight.w900),
              ),
              const SizedBox(height: 4),
              Text(
                '${booking.doctorName} - ${DateFormat('yyyy/MM/dd').format(booking.appointmentDate)} - الدور #${booking.queueNumber}',
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(color: AppColors.muted),
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
  Widget build(BuildContext context) {
    final selected = value == id;

    return Padding(
      padding: const EdgeInsetsDirectional.only(end: 8),
      child: ChoiceChip(
        selected: selected,
        label: Text(label),
        onSelected: (_) => onChanged(id),
        selectedColor: AppColors.primary,
        backgroundColor: Colors.white,
        side: BorderSide(
          color: selected ? AppColors.primary : AppColors.border,
        ),
        labelStyle: TextStyle(
          color: selected ? Colors.white : AppColors.muted,
          fontWeight: FontWeight.w800,
        ),
      ),
    );
  }
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

    showAppSnackBar(
      context,
      'تم نسخ كود الحجز.',
      type: AppSnackBarType.success,
    );
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
        border: Border.all(color: AppColors.border),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(.06),
            blurRadius: 18,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(24),
        child: Column(
          children: [
            _BookingCardHeader(booking: booking),
            Padding(
              padding: const EdgeInsets.all(14),
              child: Column(
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: _BookingInfoTile(
                          icon: Icons.calendar_month_rounded,
                          title: 'التاريخ',
                          value: DateFormat(
                            'yyyy/MM/dd',
                          ).format(booking.appointmentDate),
                        ),
                      ),
                      const SizedBox(width: 8),
                      Expanded(
                        child: _BookingInfoTile(
                          icon: Icons.confirmation_number_rounded,
                          title: 'رقم الدور',
                          value: '#${booking.queueNumber}',
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  _BookingInfoTile(
                    icon: Icons.key_rounded,
                    title: 'كود الحجز',
                    value: booking.code.isEmpty ? 'لا يوجد كود' : booking.code,
                    trailing: booking.code.isEmpty
                        ? null
                        : IconButton(
                            onPressed: () => _copyCode(context),
                            icon: const Icon(Icons.copy_rounded, size: 19),
                            tooltip: 'نسخ الكود',
                          ),
                  ),
                  if (booking.clinicAddress.isNotEmpty) ...[
                    const SizedBox(height: 8),
                    _BookingInfoTile(
                      icon: Icons.place_outlined,
                      title: 'العنوان',
                      value: booking.clinicAddress,
                    ),
                  ],
                  if (booking.cancellationReason?.isNotEmpty == true) ...[
                    const SizedBox(height: 8),
                    _CancelReasonBox(reason: booking.cancellationReason!),
                  ],
                  if (booking.clinicPhoneNumber?.isNotEmpty == true ||
                      booking.mapUrl?.isNotEmpty == true) ...[
                    const SizedBox(height: 12),
                    Row(
                      children: [
                        if (booking.clinicPhoneNumber?.isNotEmpty == true)
                          Expanded(
                            child: OutlinedButton.icon(
                              onPressed: () => openPhone(
                                context,
                                booking.clinicPhoneNumber!,
                              ),
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
                              onPressed: () =>
                                  openMap(context, booking.mapUrl!),
                              icon: const Icon(Icons.map_outlined),
                              label: const Text('الموقع'),
                            ),
                          ),
                      ],
                    ),
                  ],
                  if (onCancel != null || onReview != null) ...[
                    const SizedBox(height: 10),
                    Row(
                      children: [
                        if (onReview != null)
                          Expanded(
                            child: FilledButton.icon(
                              onPressed: onReview,
                              icon: const Icon(Icons.star_outline_rounded),
                              label: const Text('تقييم الطبيب'),
                            ),
                          ),
                        if (onReview != null && onCancel != null)
                          const SizedBox(width: 8),
                        if (onCancel != null)
                          Expanded(
                            child: OutlinedButton.icon(
                              onPressed: () => _showLongPressHint(context),
                              onLongPress: onCancel,
                              icon: const Icon(Icons.close_rounded),
                              label: const Text('إلغاء'),
                              style: OutlinedButton.styleFrom(
                                foregroundColor: AppColors.danger,
                                side: const BorderSide(color: AppColors.danger),
                              ),
                            ),
                          ),
                      ],
                    ),
                    if (onCancel != null) ...[
                      const SizedBox(height: 6),
                      const Text(
                        'لإلغاء الحجز اضغط مطولاً على زر إلغاء.',
                        style: TextStyle(color: AppColors.muted, fontSize: 12),
                      ),
                    ],
                  ],
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _BookingCardHeader extends StatelessWidget {
  const _BookingCardHeader({required this.booking});

  final BookingDetails booking;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(15),
    decoration: BoxDecoration(
      gradient: LinearGradient(
        colors: [AppColors.primary, AppColors.primary.withOpacity(.82)],
      ),
    ),
    child: Row(
      children: [
        _StatusChip(booking: booking),
        const SizedBox(width: 10),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Text(
                booking.doctorName,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                textAlign: TextAlign.end,
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 18,
                  fontWeight: FontWeight.w900,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                booking.clinicName,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                textAlign: TextAlign.end,
                style: TextStyle(
                  color: Colors.white.withOpacity(.85),
                  fontWeight: FontWeight.w600,
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
            color: Colors.white.withOpacity(.16),
            borderRadius: BorderRadius.circular(18),
            border: Border.all(color: Colors.white.withOpacity(.25)),
          ),
          child: const Icon(
            Icons.medical_services_outlined,
            color: Colors.white,
            size: 28,
          ),
        ),
      ],
    ),
  );
}

class _BookingInfoTile extends StatelessWidget {
  const _BookingInfoTile({
    required this.icon,
    required this.title,
    required this.value,
    this.trailing,
  });

  final IconData icon;
  final String title;
  final String value;
  final Widget? trailing;

  @override
  Widget build(BuildContext context) => Container(
    width: double.infinity,
    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 11),
    decoration: BoxDecoration(
      color: const Color(0xFFF6FAF9),
      borderRadius: BorderRadius.circular(16),
      border: Border.all(color: AppColors.border),
    ),
    child: Row(
      children: [
        Icon(icon, color: AppColors.primary, size: 20),
        const SizedBox(width: 8),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                style: const TextStyle(
                  color: AppColors.muted,
                  fontSize: 12,
                  fontWeight: FontWeight.w600,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                value,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                  fontWeight: FontWeight.w900,
                  fontSize: 14,
                ),
              ),
            ],
          ),
        ),
        if (trailing != null) trailing!,
      ],
    ),
  );
}

class _CancelReasonBox extends StatelessWidget {
  const _CancelReasonBox({required this.reason});

  final String reason;

  @override
  Widget build(BuildContext context) => Container(
    width: double.infinity,
    padding: const EdgeInsets.all(12),
    decoration: BoxDecoration(
      color: const Color(0xFFFFECEC),
      borderRadius: BorderRadius.circular(16),
    ),
    child: Row(
      children: [
        const Icon(Icons.info_outline, color: AppColors.danger, size: 20),
        const SizedBox(width: 8),
        Expanded(
          child: Text(
            'سبب الإلغاء: $reason',
            style: const TextStyle(
              color: AppColors.danger,
              fontWeight: FontWeight.w700,
            ),
          ),
        ),
      ],
    ),
  );
}

class _StatusChip extends StatelessWidget {
  const _StatusChip({required this.booking});

  final BookingDetails booking;

  @override
  Widget build(BuildContext context) {
    final color = _statusColor(booking.status);
    final background = _statusBackground(booking.status);

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 11, vertical: 7),
      decoration: BoxDecoration(
        color: background,
        borderRadius: BorderRadius.circular(999),
      ),
      child: Text(
        booking.statusLabel,
        style: TextStyle(
          color: color,
          fontWeight: FontWeight.w900,
          fontSize: 12,
        ),
      ),
    );
  }
}

Color _statusColor(int status) {
  return switch (status) {
    1 => AppColors.primary,
    2 => AppColors.danger,
    3 => AppColors.success,
    _ => const Color(0xFFB16A2B),
  };
}

Color _statusBackground(int status) {
  return switch (status) {
    1 => const Color(0xFFE4F5F1),
    2 => const Color(0xFFFFECEC),
    3 => const Color(0xFFE4F4F0),
    _ => const Color(0xFFFFF0DF),
  };
}

void _showLongPressHint(BuildContext context) {
  showAppSnackBar(
    context,
    'اضغط مطولاً على الزر لإلغاء الحجز.',
    type: AppSnackBarType.info,
  );
}

class BookingMessage extends StatelessWidget {
  const BookingMessage({super.key, required this.text, this.action});

  final String text;
  final VoidCallback? action;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(24),
    decoration: BoxDecoration(
      color: Colors.white,
      borderRadius: BorderRadius.circular(24),
      border: Border.all(color: AppColors.border),
    ),
    child: Column(
      children: [
        const Icon(
          Icons.calendar_month_outlined,
          size: 44,
          color: AppColors.muted,
        ),
        const SizedBox(height: 10),
        Text(
          text,
          textAlign: TextAlign.center,
          style: const TextStyle(fontWeight: FontWeight.w700),
        ),
        if (action != null) ...[
          const SizedBox(height: 8),
          TextButton.icon(
            onPressed: action,
            icon: const Icon(Icons.refresh_rounded),
            label: const Text('إعادة المحاولة'),
          ),
        ],
      ],
    ),
  );
}
