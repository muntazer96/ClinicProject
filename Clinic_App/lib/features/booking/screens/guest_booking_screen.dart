import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
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
  static const _lastPhoneKey = 'clinic_guest_booking_phone';
  static const _lastCodeKey = 'clinic_guest_booking_code';

  late final BookingService _service;
  final _storage = const FlutterSecureStorage();
  final _phone = TextEditingController(), _code = TextEditingController();
  BookingDetails? _booking;
  bool _loading = false;
  bool _hasSavedLookup = false;

  @override
  void initState() {
    super.initState();
    _service = BookingService(context.read<AuthController>().api);
    _restoreLastLookup();
  }

  @override
  void dispose() {
    _phone.dispose();
    _code.dispose();
    super.dispose();
  }

  Future<void> _restoreLastLookup() async {
    final phone = await _storage.read(key: _lastPhoneKey);
    final code = await _storage.read(key: _lastCodeKey);
    if (!mounted) return;
    if ((phone?.isNotEmpty == true) || (code?.isNotEmpty == true)) {
      _phone.text = phone ?? '';
      _code.text = code ?? '';
      setState(() => _hasSavedLookup = true);
    }
  }

  Future<void> _saveLastLookup() async {
    await _storage.write(key: _lastPhoneKey, value: _phone.text.trim());
    await _storage.write(key: _lastCodeKey, value: _code.text.trim());
    if (mounted) setState(() => _hasSavedLookup = true);
  }

  Future<void> _clearSavedLookup() async {
    await _storage.delete(key: _lastPhoneKey);
    await _storage.delete(key: _lastCodeKey);
    if (!mounted) return;
    setState(() {
      _hasSavedLookup = false;
      _booking = null;
      _phone.clear();
      _code.clear();
    });
  }

  Future<void> _search() async {
    if (_phone.text.trim().isEmpty || _code.text.trim().isEmpty) {
      showAppSnackBar(
        context,
        'أدخل رقم الهاتف وكود الحجز.',
        type: AppSnackBarType.warning,
      );
      return;
    }
    setState(() {
      _loading = true;
      _booking = null;
    });
    try {
      final booking = await _service.getGuestBooking(
        phoneNumber: _phone.text,
        bookingCode: _code.text,
      );
      await _saveLastLookup();
      if (mounted) setState(() => _booking = booking);
    } catch (error) {
      if (mounted) {
        showAppSnackBar(
          context,
          ApiClient.errorMessage(error),
          type: AppSnackBarType.error,
        );
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _cancel() async {
    try {
      await _service.cancelGuestBooking(
        phoneNumber: _phone.text,
        bookingCode: _code.text,
      );
      await _search();
      if (mounted) {
        showAppSnackBar(
          context,
          'تم إلغاء الحجز بنجاح.',
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
          style: TextStyle(color: AppColors.muted),
        ),
        if (_hasSavedLookup) ...[
          const SizedBox(height: 12),
          _SavedLookupNotice(
            onSearch: _loading ? null : _search,
            onClear: _clearSavedLookup,
          ),
        ],
        const SizedBox(height: 14),
        TextField(
          controller: _phone,
          keyboardType: TextInputType.phone,
          decoration: const InputDecoration(
            labelText: 'رقم الهاتف',
            prefixIcon: Icon(Icons.phone_outlined),
          ),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _code,
          textCapitalization: TextCapitalization.characters,
          decoration: const InputDecoration(
            labelText: 'كود الحجز',
            prefixIcon: Icon(Icons.confirmation_number_outlined),
          ),
          onSubmitted: (_) => _search(),
        ),
        const SizedBox(height: 10),
        FilledButton.icon(
          onPressed: _loading ? null : _search,
          icon: const Icon(Icons.search),
          label: Text(_loading ? 'جاري البحث...' : 'عرض الحجز'),
        ),
        if (_booking != null) ...[
          const SizedBox(height: 14),
          BookingCard(
            booking: _booking!,
            onCancel: null,
          ),
        ],
      ],
    ),
  );
}

class _SavedLookupNotice extends StatelessWidget {
  const _SavedLookupNotice({required this.onSearch, required this.onClear});

  final VoidCallback? onSearch;
  final VoidCallback onClear;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(12),
    decoration: BoxDecoration(
      color: AppColors.softBlue,
      borderRadius: BorderRadius.circular(8),
      border: Border.all(color: AppColors.border),
    ),
    child: Row(
      children: [
        const Icon(Icons.history_rounded, color: AppColors.primary),
        const SizedBox(width: 10),
        const Expanded(
          child: Text(
            'تمت تعبئة آخر حجز زائر محفوظ.',
            style: TextStyle(fontWeight: FontWeight.w800),
          ),
        ),
        IconButton(
          onPressed: onSearch,
          icon: const Icon(Icons.refresh_rounded),
          tooltip: 'عرض الحجز',
        ),
        IconButton(
          onPressed: onClear,
          icon: const Icon(Icons.close_rounded),
          tooltip: 'مسح',
        ),
      ],
    ),
  );
}
