import 'dart:async';

import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../booking_service.dart';
import '../models/booking_models.dart';
import 'success_screen.dart';

class OtpScreenArgs {
  const OtpScreenArgs({
    required this.result,
    required this.phoneNumber,
    required this.doctorName,
    required this.clinicName,
    required this.date,
  });

  final BookingResult result;
  final String phoneNumber;
  final String doctorName;
  final String clinicName;
  final DateTime date;
}

class OtpScreen extends StatefulWidget {
  const OtpScreen({super.key, required this.args});
  final OtpScreenArgs args;

  @override
  State<OtpScreen> createState() => _OtpScreenState();
}

class _OtpScreenState extends State<OtpScreen> {
  final _code = TextEditingController();
  late final BookingService _service;
  bool _loading = false;
  bool _resending = false;
  int _resendSeconds = 60;
  String? _error;
  Timer? _timer;

  @override
  void initState() {
    super.initState();
    _service = BookingService(context.read<AuthController>().api);
    _code.addListener(_onCodeChanged);
    _startCooldown();
  }

  @override
  void dispose() {
    _code.removeListener(_onCodeChanged);
    _code.dispose();
    _timer?.cancel();
    super.dispose();
  }

  void _onCodeChanged() {
    if (mounted) setState(() {});
  }

  Future<void> _confirm() async {
    if (_code.text.trim().isEmpty) return;
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      await _service.confirmOtp(
        phoneNumber: widget.args.phoneNumber,
        bookingCode: widget.args.result.code,
        otpCode: _code.text,
      );
      if (!mounted) return;
      if (context.read<AuthController>().isAuthenticated) {
        context.go('/bookings');
      } else {
        context.go(
          '/booking/success',
          extra: BookingSuccessArgs(
            result: widget.args.result,
            doctorName: widget.args.doctorName,
            clinicName: widget.args.clinicName,
            date: widget.args.date,
          ),
        );
      }
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _resend() async {
    if (_resending || _resendSeconds > 0) return;
    setState(() => _error = null);
    try {
      setState(() => _resending = true);
      await _service.resendOtp(
        phoneNumber: widget.args.phoneNumber,
        bookingCode: widget.args.result.code,
      );
      if (mounted) {
        _startCooldown();
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(const SnackBar(content: Text('تم إرسال رمز جديد.')));
      }
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _resending = false);
    }
  }

  void _startCooldown() {
    _timer?.cancel();
    setState(() => _resendSeconds = 60);
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (!mounted) {
        timer.cancel();
        return;
      }
      if (_resendSeconds <= 1) {
        timer.cancel();
        setState(() => _resendSeconds = 0);
      } else {
        setState(() => _resendSeconds--);
      }
    });
  }

  String get _maskedPhone {
    final phone = widget.args.phoneNumber.trim();
    if (phone.length <= 4) return phone;
    return '******${phone.substring(phone.length - 4)}';
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('تأكيد رقم الهاتف')),
    body: ListView(
      padding: const EdgeInsets.all(22),
      children: [
        const Icon(Icons.sms_outlined, size: 58, color: AppColors.primary),
        const SizedBox(height: 14),
        const Text(
          'أدخل رمز التحقق',
          textAlign: TextAlign.center,
          style: TextStyle(fontSize: 23, fontWeight: FontWeight.w900),
        ),
        const SizedBox(height: 7),
        Text(
          'تم إرسال الرمز إلى $_maskedPhone',
          textAlign: TextAlign.center,
          style: const TextStyle(color: AppColors.muted),
        ),
        const SizedBox(height: 8),
        const Text(
          'لا تشارك الرمز مع أي شخص. يمكنك إعادة الإرسال بعد انتهاء العداد.',
          textAlign: TextAlign.center,
          style: TextStyle(color: AppColors.muted, fontSize: 12),
        ),
        const SizedBox(height: 20),
        TextField(
          controller: _code,
          keyboardType: TextInputType.number,
          textAlign: TextAlign.center,
          maxLength: 8,
          decoration: const InputDecoration(labelText: 'رمز OTP'),
          onSubmitted: (_) => _confirm(),
        ),
        if (_error != null)
          Text(
            _error!,
            style: TextStyle(color: Colors.red.shade800),
            textAlign: TextAlign.center,
          ),
        const SizedBox(height: 10),
        FilledButton(
          onPressed: _loading || _code.text.trim().isEmpty ? null : _confirm,
          child: Text(_loading ? 'جاري التحقق...' : 'تأكيد الرمز'),
        ),
        TextButton(
          onPressed: _resending || _resendSeconds > 0 ? null : _resend,
          child: Text(
            _resendSeconds > 0
                ? 'إعادة الإرسال بعد $_resendSeconds ثانية'
                : _resending
                ? 'جاري إعادة الإرسال...'
                : 'إعادة إرسال الرمز',
          ),
        ),
      ],
    ),
  );
}
