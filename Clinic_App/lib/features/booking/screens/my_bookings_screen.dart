import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../widgets/app_scaffold.dart';
import '../../auth/auth_controller.dart';
import '../booking_service.dart';
import '../models/booking_models.dart';

class MyBookingsScreen extends StatefulWidget {
  const MyBookingsScreen({super.key});
  @override
  State<MyBookingsScreen> createState() => _MyBookingsScreenState();
}

class _MyBookingsScreenState extends State<MyBookingsScreen> {
  late final BookingService _service;
  List<BookingDetails> _bookings = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _service = BookingService(context.read<AuthController>().api);
    _load();
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
            style: TextStyle(color: Color(0xFF78908D)),
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
                ),
              ),
            ),
        ],
      ),
    ),
  );
}

class BookingCard extends StatelessWidget {
  const BookingCard({super.key, required this.booking, this.onCancel});
  final BookingDetails booking;
  final VoidCallback? onCancel;

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
              Chip(label: Text(booking.statusLabel)),
            ],
          ),
          Text(
            booking.clinicName,
            style: const TextStyle(color: Color(0xFF147D72)),
          ),
          const Divider(height: 20),
          Text(
            'التاريخ: ${DateFormat('yyyy/MM/dd').format(booking.appointmentDate)}',
          ),
          Text('رقم الدور: #${booking.queueNumber}'),
          Text('كود الحجز: ${booking.code}'),
          if (onCancel != null) ...[
            const SizedBox(height: 8),
            OutlinedButton.icon(
              onPressed: onCancel,
              icon: const Icon(Icons.close),
              label: const Text('إلغاء الحجز'),
            ),
          ],
        ],
      ),
    ),
  );
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
            color: Color(0xFF78908D),
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
