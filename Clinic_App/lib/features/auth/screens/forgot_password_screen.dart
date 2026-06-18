import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../widgets/auth_shell.dart';
import 'login_screen.dart';

class ForgotPasswordScreen extends StatefulWidget {
  const ForgotPasswordScreen({super.key});

  @override
  State<ForgotPasswordScreen> createState() => _ForgotPasswordScreenState();
}

class _ForgotPasswordScreenState extends State<ForgotPasswordScreen> {
  static const _otpLength = 6;

  final phone = TextEditingController();
  late final List<TextEditingController> _otpControllers;
  late final List<FocusNode> _otpFocusNodes;
  bool codeSent = false;
  bool loading = false;
  String? error;
  String? notice;

  bool get canSend => phone.text.trim().isNotEmpty && !loading;
  bool get canVerify =>
      phone.text.trim().isNotEmpty && _otpCode.length == _otpLength && !loading;

  @override
  void initState() {
    super.initState();
    phone.addListener(_refresh);
    _otpControllers = List.generate(_otpLength, (_) => TextEditingController());
    _otpFocusNodes = List.generate(_otpLength, (_) => FocusNode());
  }

  @override
  void dispose() {
    phone.removeListener(_refresh);
    phone.dispose();
    for (final controller in _otpControllers) {
      controller.dispose();
    }
    for (final node in _otpFocusNodes) {
      node.dispose();
    }
    super.dispose();
  }

  void _refresh() {
    if (mounted) setState(() {});
  }

  String get _otpCode =>
      _otpControllers.map((controller) => controller.text).join();

  void _onOtpDigitChanged(int index, String value) {
    final digits = value.replaceAll(RegExp(r'\D'), '');
    if (digits.length > 1) {
      _fillOtpCode(digits);
      return;
    }
    if (digits != value) {
      _otpControllers[index].text = digits;
      _otpControllers[index].selection = TextSelection.collapsed(
        offset: digits.length,
      );
    }
    if (digits.isNotEmpty && index < _otpLength - 1) {
      _otpFocusNodes[index + 1].requestFocus();
    }
    if (_otpCode.length == _otpLength) _verifyOtp();
    _refresh();
  }

  void _fillOtpCode(String value) {
    final digits = value.replaceAll(RegExp(r'\D'), '');
    for (var i = 0; i < _otpLength; i++) {
      _otpControllers[i].text = i < digits.length ? digits[i] : '';
    }
    final nextIndex = digits.length >= _otpLength
        ? _otpLength - 1
        : digits.length;
    _otpFocusNodes[nextIndex.clamp(0, _otpLength - 1).toInt()].requestFocus();
    if (_otpCode.length == _otpLength) _verifyOtp();
    _refresh();
  }

  Future<void> _sendOtp() async {
    if (!canSend) return;
    setState(() {
      loading = true;
      error = null;
      notice = null;
    });
    try {
      await context.read<ApiClient>().dio.post(
        '/User/password/forgot/send-otp',
        data: {'phoneNumber': phone.text.trim()},
      );
      if (!mounted) return;
      setState(() {
        codeSent = true;
        notice = 'تم إرسال رمز التحقق إلى رقم الهاتف.';
      });
    } catch (e) {
      if (mounted) setState(() => error = ApiClient.errorMessage(e));
    } finally {
      if (mounted) setState(() => loading = false);
    }
  }

  Future<void> _verifyOtp() async {
    if (!canVerify) return;
    setState(() {
      loading = true;
      error = null;
      notice = null;
    });
    try {
      final response = await context.read<ApiClient>().dio.post(
        '/User/password/forgot/verify-otp',
        data: {
          'phoneNumber': phone.text.trim(),
          'otpCode': _otpCode,
        },
      );
      final data = response.data['data'] as Map<String, dynamic>? ?? {};
      final resetToken = data['resetToken'] as String? ?? '';
      final phoneNumber = data['phoneNumber'] as String? ?? phone.text.trim();
      if (resetToken.isEmpty) {
        throw Exception('تعذر إنشاء رمز إعادة تعيين كلمة المرور.');
      }
      if (mounted) {
        context.go(
          '/password-reset?phoneNumber=${Uri.encodeComponent(phoneNumber)}&resetToken=${Uri.encodeComponent(resetToken)}',
        );
      }
    } catch (e) {
      if (mounted) setState(() => error = ApiClient.errorMessage(e));
    } finally {
      if (mounted) setState(() => loading = false);
    }
  }

  @override
  Widget build(BuildContext context) => AuthShell(
    title: 'استعادة كلمة المرور',
    subtitle: codeSent
        ? 'أدخل رمز التحقق المرسل إلى رقم هاتفك.'
        : 'أدخل رقم الهاتف المرتبط بحسابك لإرسال رمز تحقق.',
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        if (error != null) ErrorText(error!),
        if (notice != null) ...[
          _Notice(text: notice!),
          const SizedBox(height: 12),
        ],
        TextField(
          controller: phone,
          enabled: !loading && !codeSent,
          keyboardType: TextInputType.phone,
          textInputAction: codeSent ? TextInputAction.next : TextInputAction.done,
          onSubmitted: (_) {
            if (!codeSent) _sendOtp();
          },
          decoration: const InputDecoration(
            labelText: 'رقم الهاتف',
            prefixIcon: Icon(Icons.phone_outlined),
          ),
        ),
        if (codeSent) ...[
          const SizedBox(height: 12),
          Directionality(
            textDirection: TextDirection.ltr,
            child: Row(
              children: List.generate(
                _otpLength,
                (index) => Expanded(
                  child: Padding(
                    padding: EdgeInsets.only(left: index == 0 ? 0 : 6),
                    child: TextField(
                      controller: _otpControllers[index],
                      focusNode: _otpFocusNodes[index],
                      autofocus: index == 0,
                      enabled: !loading,
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
                      onChanged: (value) => _onOtpDigitChanged(index, value),
                      onSubmitted: (_) => _verifyOtp(),
                      onTap: () => _otpControllers[index].selection =
                          TextSelection.collapsed(
                        offset: _otpControllers[index].text.length,
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ),
        ],
        const SizedBox(height: 14),
        FilledButton(
          onPressed: codeSent
              ? (canVerify ? _verifyOtp : null)
              : (canSend ? _sendOtp : null),
          child: Text(
            loading
                ? 'جاري المعالجة...'
                : codeSent
                    ? 'تأكيد الرمز'
                    : 'إرسال رمز التحقق',
          ),
        ),
        if (codeSent)
          TextButton(
            onPressed: loading
                ? null
                : () {
                    for (final controller in _otpControllers) {
                      controller.clear();
                    }
                    setState(() {
                      codeSent = false;
                      notice = null;
                      error = null;
                    });
                  },
            child: const Text('تغيير رقم الهاتف'),
          ),
        TextButton(
          onPressed: () => context.go('/login'),
          child: const Text('العودة إلى تسجيل الدخول'),
        ),
      ],
    ),
  );
}

class _Notice extends StatelessWidget {
  const _Notice({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(10),
    decoration: BoxDecoration(
      color: const Color(0xFFEAF7F3),
      borderRadius: BorderRadius.circular(8),
      border: Border.all(color: const Color(0xFFCDECE4)),
    ),
    child: Text(text, style: const TextStyle(color: Color(0xFF0F5F55))),
  );
}
