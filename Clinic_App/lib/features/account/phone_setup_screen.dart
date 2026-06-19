import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import '../../core/app_theme.dart';
import '../../core/phone_number_validator.dart';
import '../auth/auth_controller.dart';
import 'profile_service.dart';

class PhoneSetupScreen extends StatefulWidget {
  const PhoneSetupScreen({super.key});

  @override
  State<PhoneSetupScreen> createState() => _PhoneSetupScreenState();
}

class _PhoneSetupScreenState extends State<PhoneSetupScreen> {
  late final ProfileService _service;
  final _phoneController = TextEditingController();
  bool _saving = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _service = ProfileService(context.read<AuthController>().api);
    _phoneController.text =
        context.read<AuthController>().profile?.phoneNumber ?? '';
  }

  @override
  void dispose() {
    _phoneController.dispose();
    super.dispose();
  }

  Future<void> _saveAndSendOtp() async {
    final phone = _phoneController.text.trim();
    if (phone.isEmpty) {
      setState(() => _error = 'أدخل رقم الهاتف حتى نرسل رمز التأكيد.');
      return;
    }
    if (!isValidIraqiPhone(phone)) {
      setState(() => _error = iraqiPhoneError);
      return;
    }

    setState(() {
      _saving = true;
      _error = null;
    });
    try {
      final profile = await _service.setPhoneNumber(phone);
      if (!mounted) return;
      context.read<AuthController>().setProfile(profile);
      await _service.sendPhoneConfirmation();
      if (!mounted) return;
      context.go('/profile/confirm-phone');
    } catch (error) {
      final message = ApiClient.errorMessage(error);
      if (message.contains('رمز جديد') && mounted) {
        context.go('/profile/confirm-phone');
      } else if (mounted) {
        setState(() => _error = message);
      }
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) => ListView(
    padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
    children: [
      Container(
        padding: const EdgeInsets.all(18),
        decoration: BoxDecoration(
          color: AppColors.primaryDark,
          borderRadius: BorderRadius.circular(8),
        ),
        child: const Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Icon(Icons.phone_iphone_rounded, color: Colors.white, size: 34),
            SizedBox(height: 12),
            Text(
              'إضافة رقم الهاتف',
              style: TextStyle(
                color: Colors.white,
                fontSize: 22,
                fontWeight: FontWeight.w900,
              ),
            ),
            SizedBox(height: 6),
            Text(
              'أدخل رقم هاتفك حتى نرسل رمز التأكيد ونكمل تفعيل الحساب.',
              style: TextStyle(color: Color(0xFFD7FFFA)),
            ),
          ],
        ),
      ),
      const SizedBox(height: 16),
      if (_error != null) ...[
        _Notice(text: _error!, error: true),
        const SizedBox(height: 12),
      ],
      TextField(
        controller: _phoneController,
        autofocus: true,
        keyboardType: TextInputType.phone,
        inputFormatters: iraqiPhoneInputFormatters,
        textInputAction: TextInputAction.done,
        onSubmitted: (_) => _saveAndSendOtp(),
        decoration: const InputDecoration(
          labelText: 'رقم الهاتف',
          prefixIcon: Icon(Icons.phone_outlined),
        ),
      ),
      const SizedBox(height: 16),
      FilledButton.icon(
        onPressed: _saving ? null : _saveAndSendOtp,
        icon: _saving
            ? const SizedBox(
                width: 18,
                height: 18,
                child: CircularProgressIndicator(strokeWidth: 2),
              )
            : const Icon(Icons.sms_outlined),
        label: Text(_saving ? 'جاري الإرسال...' : 'إرسال رمز التأكيد'),
      ),
    ],
  );
}

class _Notice extends StatelessWidget {
  const _Notice({required this.text, this.error = false});
  final String text;
  final bool error;

  @override
  Widget build(BuildContext context) {
    final isDark = context.isDark;
    final background = error
        ? (isDark ? const Color(0xFF3F1518) : Colors.red.shade50)
        : context.appSoftBlue;
    final border = error
        ? (isDark ? const Color(0xFF7F1D1D) : Colors.red.shade100)
        : context.appBorder;
    final foreground = error
        ? (isDark ? const Color(0xFFFCA5A5) : Colors.red.shade800)
        : Theme.of(context).colorScheme.primary;
    return Container(
      padding: const EdgeInsets.all(11),
      decoration: BoxDecoration(
        color: background,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: border),
      ),
      child: Text(
        text,
        style: TextStyle(color: foreground, fontWeight: FontWeight.w700),
      ),
    );
  }
}
