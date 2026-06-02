import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../widgets/app_scaffold.dart';
import '../../auth/auth_controller.dart';
import '../booking_service.dart';
import '../models/booking_models.dart';
import 'my_bookings_screen.dart';

class GuestBookingScreen extends StatefulWidget {
  const GuestBookingScreen({super.key});
  @override
  State<GuestBookingScreen> createState() => _GuestBookingScreenState();
}

class _GuestBookingScreenState extends State<GuestBookingScreen> {
  late final BookingService _service;
  final _phone = TextEditingController(), _code = TextEditingController();
  BookingDetails? _booking;
  bool _loading = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _service = BookingService(context.read<AuthController>().api);
  }

  @override
  void dispose() {
    _phone.dispose();
    _code.dispose();
    super.dispose();
  }

  Future<void> _search() async {
    if (_phone.text.trim().isEmpty || _code.text.trim().isEmpty) {
      setState(() => _error = 'أدخل رقم الهاتف وكود الحجز.');
      return;
    }
    setState(() {
      _loading = true;
      _error = null;
      _booking = null;
    });
    try {
      final booking = await _service.getGuestBooking(
        phoneNumber: _phone.text,
        bookingCode: _code.text,
      );
      if (mounted) setState(() => _booking = booking);
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _cancel() async {
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('إلغاء الحجز'),
        content: const Text('هل تريد إلغاء حجز الزائر؟'),
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
      await _service.cancelGuestBooking(
        phoneNumber: _phone.text,
        bookingCode: _code.text,
      );
      await _search();
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    }
  }

  @override
  Widget build(BuildContext context) => AppScaffold(
    title: 'حجز الزائر',
    child: ListView(
      padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
      children: [
        const Text(
          'متابعة حجز الزائر',
          style: TextStyle(fontSize: 24, fontWeight: FontWeight.w900),
        ),
        const SizedBox(height: 5),
        const Text(
          'استخدم الهاتف وكود الحجز لعرض دورك أو إلغائه.',
          style: TextStyle(color: Color(0xFF78908D)),
        ),
        const SizedBox(height: 14),
        TextField(
          controller: _phone,
          keyboardType: TextInputType.phone,
          decoration: const InputDecoration(labelText: 'رقم الهاتف'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _code,
          decoration: const InputDecoration(labelText: 'كود الحجز'),
        ),
        const SizedBox(height: 10),
        FilledButton.icon(
          onPressed: _loading ? null : _search,
          icon: const Icon(Icons.search),
          label: Text(_loading ? 'جارِ البحث...' : 'عرض الحجز'),
        ),
        if (_error != null) ...[
          const SizedBox(height: 10),
          Text(_error!, style: TextStyle(color: Colors.red.shade800)),
        ],
        if (_booking != null) ...[
          const SizedBox(height: 14),
          BookingCard(
            booking: _booking!,
            onCancel: _booking!.canCancel ? _cancel : null,
          ),
        ],
      ],
    ),
  );
}
