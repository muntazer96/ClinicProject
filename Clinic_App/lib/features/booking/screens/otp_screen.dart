import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
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
  static const _otpLength = 6;

  late final BookingService _service;
  late final List<TextEditingController> _controllers;
  late final List<FocusNode> _focusNodes;
  bool _loading = false;
  bool _resending = false;
  int _resendSeconds = 60;
  Timer? _timer;

  @override
  void initState() {
    super.initState();
    _service = BookingService(context.read<AuthController>().api);
    _controllers = List.generate(_otpLength, (_) => TextEditingController());
    _focusNodes = List.generate(_otpLength, (_) => FocusNode());
    _startCooldown();
  }

  @override
  void dispose() {
    for (final controller in _controllers) {
      controller.dispose();
    }
    for (final node in _focusNodes) {
      node.dispose();
    }
    _timer?.cancel();
    super.dispose();
  }

  String get _code => _controllers.map((controller) => controller.text).join();

  void _onDigitChanged(int index, String value) {
    final digits = value.replaceAll(RegExp(r'\D'), '');
    if (digits.length > 1) {
      _fillCode(digits);
      return;
    }
    if (digits != value) {
      _controllers[index].text = digits;
      _controllers[index].selection = TextSelection.collapsed(
        offset: digits.length,
      );
    }
    if (digits.isNotEmpty && index < _otpLength - 1) {
      _focusNodes[index + 1].requestFocus();
    }
    if (mounted) setState(() {});
    if (_code.length == _otpLength) _confirm();
  }

  void _fillCode(String value) {
    final digits = value.replaceAll(RegExp(r'\D'), '');
    for (var i = 0; i < _otpLength; i++) {
      _controllers[i].text = i < digits.length ? digits[i] : '';
    }
    final nextIndex = digits.length >= _otpLength
        ? _otpLength - 1
        : digits.length;
    _focusNodes[nextIndex.clamp(0, _otpLength - 1).toInt()].requestFocus();
    if (mounted) setState(() {});
    if (_code.length == _otpLength) _confirm();
  }

  Future<void> _confirm() async {
    final code = _code.trim();
    if (_loading || code.length != _otpLength) return;
    setState(() => _loading = true);
    try {
      await _service.confirmOtp(
        phoneNumber: widget.args.phoneNumber,
        bookingCode: widget.args.result.code,
        otpCode: code,
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

  Future<void> _resend() async {
    if (_resending || _resendSeconds > 0) return;
    try {
      setState(() => _resending = true);
      await _service.resendOtp(
        phoneNumber: widget.args.phoneNumber,
        bookingCode: widget.args.result.code,
      );
      if (mounted) {
        for (final controller in _controllers) {
          controller.clear();
        }
        _focusNodes.first.requestFocus();
        _startCooldown();
        showAppSnackBar(
          context,
          'تم إرسال رمز جديد.',
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
        Directionality(
          textDirection: TextDirection.ltr,
          child: Row(
            children: List.generate(
              _otpLength,
              (index) => Expanded(
                child: Padding(
                  padding: EdgeInsets.only(left: index == 0 ? 0 : 6),
                  child: TextField(
                    controller: _controllers[index],
                    focusNode: _focusNodes[index],
                    autofocus: index == 0,
                    enabled: !_loading,
                    keyboardType: TextInputType.number,
                    textAlign: TextAlign.center,
                    inputFormatters: [
                      FilteringTextInputFormatter.digitsOnly,
                      LengthLimitingTextInputFormatter(_otpLength),
                    ],
                    style: const TextStyle(
                      fontSize: 22,
                      fontWeight: FontWeight.w900,
                    ),
                    decoration: const InputDecoration(counterText: ''),
                    onChanged: (value) => _onDigitChanged(index, value),
                    onTap: () =>
                        _controllers[index].selection = TextSelection.collapsed(
                          offset: _controllers[index].text.length,
                        ),
                  ),
                ),
              ),
            ),
          ),
        ),
        const SizedBox(height: 10),
        FilledButton(
          onPressed: _loading || _code.length != _otpLength ? null : _confirm,
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
