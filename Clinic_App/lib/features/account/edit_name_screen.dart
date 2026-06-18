import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import '../../core/app_theme.dart';
import '../auth/auth_controller.dart';
import 'profile_service.dart';

class EditNameScreen extends StatefulWidget {
  const EditNameScreen({super.key, required this.initialName});
  final String initialName;

  @override
  State<EditNameScreen> createState() => _EditNameScreenState();
}

class _EditNameScreenState extends State<EditNameScreen> {
  late final TextEditingController _controller;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: widget.initialName);
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _save() {
    final name = _controller.text.trim();
    if (name.isEmpty) return;
    context.pop(name);
  }

  @override
  Widget build(BuildContext context) => ListView(
    padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
    children: [
      const _PageHeader(
        icon: Icons.edit_outlined,
        title: 'تعديل الاسم',
        text: 'اكتب الاسم الذي تريد ظهوره داخل حسابك وحجوزاتك.',
      ),
      const SizedBox(height: 16),
      TextField(
        controller: _controller,
        autofocus: true,
        textInputAction: TextInputAction.done,
        onSubmitted: (_) => _save(),
        decoration: const InputDecoration(labelText: 'الاسم الكامل'),
      ),
      const SizedBox(height: 16),
      FilledButton.icon(
        onPressed: _save,
        icon: const Icon(Icons.check_rounded),
        label: const Text('حفظ الاسم'),
      ),
      const SizedBox(height: 10),
      OutlinedButton.icon(
        onPressed: () => context.pop(),
        icon: const Icon(Icons.arrow_back_rounded),
        label: const Text('العودة'),
      ),
    ],
  );
}

class ConfirmPhoneScreen extends StatefulWidget {
  const ConfirmPhoneScreen({super.key});

  @override
  State<ConfirmPhoneScreen> createState() => _ConfirmPhoneScreenState();
}

class _ConfirmPhoneScreenState extends State<ConfirmPhoneScreen> {
  static const _otpLength = 6;
  static const _resendSeconds = 120;

  late final ProfileService _service;
  late final List<TextEditingController> _controllers;
  late final List<FocusNode> _focusNodes;
  Timer? _timer;
  int _remainingSeconds = _resendSeconds;
  bool _resending = false;
  bool _confirming = false;
  bool _confirmed = false;
  String? _error;
  String? _notice;

  @override
  void initState() {
    super.initState();
    _service = ProfileService(context.read<AuthController>().api);
    _controllers = List.generate(_otpLength, (_) => TextEditingController());
    _focusNodes = List.generate(_otpLength, (_) => FocusNode());
    _startTimer(_resendSeconds);
  }

  @override
  void dispose() {
    _timer?.cancel();
    for (final controller in _controllers) {
      controller.dispose();
    }
    for (final node in _focusNodes) {
      node.dispose();
    }
    super.dispose();
  }

  void _startTimer(int seconds) {
    _timer?.cancel();
    setState(() => _remainingSeconds = seconds);
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (!mounted) return;
      if (_remainingSeconds <= 1) {
        timer.cancel();
        setState(() => _remainingSeconds = 0);
      } else {
        setState(() => _remainingSeconds--);
      }
    });
  }

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
    if (_code.length == _otpLength) _confirm();
  }

  String get _code => _controllers.map((controller) => controller.text).join();

  Future<void> _confirm() async {
    if (_confirming || _confirmed) return;
    final code = _code.trim();
    if (code.length != _otpLength) {
      setState(() => _error = 'أدخل رمز التأكيد المكوّن من 6 أرقام.');
      return;
    }
    setState(() {
      _confirming = true;
      _error = null;
      _notice = null;
    });
    try {
      await _service.confirmPhone(code);
      final profile = await _service.getProfile();
      if (!mounted) return;
      context.read<AuthController>().setProfile(profile);
      _timer?.cancel();
      setState(() {
        _confirmed = true;
        _notice = 'تم تأكيد رقم الهاتف بنجاح.';
      });
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _confirming = false);
    }
  }

  Future<void> _resend() async {
    if (_remainingSeconds > 0 || _resending) return;
    setState(() {
      _resending = true;
      _error = null;
      _notice = null;
    });
    try {
      await _service.sendPhoneConfirmation();
      for (final controller in _controllers) {
        controller.clear();
      }
      _focusNodes.first.requestFocus();
      setState(() => _notice = 'تم إرسال رمز جديد.');
      _startTimer(_resendSeconds);
    } catch (error) {
      final message = ApiClient.errorMessage(error);
      setState(() => _error = message);
      final match = RegExp(r'(\d+)').firstMatch(message);
      final seconds = int.tryParse(match?.group(1) ?? '');
      if (seconds != null && seconds > 0) _startTimer(seconds);
    } finally {
      if (mounted) setState(() => _resending = false);
    }
  }

  @override
  Widget build(BuildContext context) => ListView(
    padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
    children: [
      _PageHeader(
        icon: _confirmed ? Icons.verified_rounded : Icons.sms_outlined,
        title: _confirmed ? 'تم تأكيد الهاتف' : 'تأكيد رقم الهاتف',
        text: _confirmed
            ? 'صار رقم هاتفك مؤكداً ويمكنك إكمال الحجوزات بشكل طبيعي.'
            : 'أدخل رمز التأكيد الذي وصل إلى رقم هاتفك.',
      ),
      const SizedBox(height: 16),
      if (_confirmed) ...[
        _Notice(text: _notice ?? 'تم تأكيد رقم الهاتف بنجاح.'),
        const SizedBox(height: 16),
        FilledButton.icon(
          onPressed: () {
            final auth = context.read<AuthController>();
            context.go(auth.isDoctor ? '/doctor' : '/');
          },
          icon: const Icon(Icons.arrow_back_rounded),
          label: const Text('العودة'),
        ),
      ] else ...[
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
                    enabled: !_confirming,
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
        if (_error != null) ...[
          const SizedBox(height: 10),
          _Notice(text: _error!, error: true),
        ],
        if (_notice != null) ...[
          const SizedBox(height: 10),
          _Notice(text: _notice!),
        ],
        const SizedBox(height: 12),
        Center(
          child: Text(
            _remainingSeconds > 0
                ? 'يمكنك طلب رمز جديد بعد ${_formatTime(_remainingSeconds)}'
                : 'يمكنك الآن طلب رمز جديد',
            style: const TextStyle(color: AppColors.muted),
          ),
        ),
        const SizedBox(height: 8),
        FilledButton.icon(
          onPressed: _confirming ? null : _confirm,
          icon: _confirming
              ? const SizedBox(
                  width: 18,
                  height: 18,
                  child: CircularProgressIndicator(strokeWidth: 2),
                )
              : const Icon(Icons.verified_outlined),
          label: Text(_confirming ? 'جاري التأكيد...' : 'تأكيد الرقم'),
        ),
        const SizedBox(height: 10),
        OutlinedButton.icon(
          onPressed: _remainingSeconds > 0 || _resending ? null : _resend,
          icon: const Icon(Icons.refresh_rounded),
          label: Text(_resending ? 'جاري الإرسال...' : 'طلب رمز جديد'),
        ),
        const SizedBox(height: 10),
        OutlinedButton.icon(
          onPressed: () => context.pop(false),
          icon: const Icon(Icons.arrow_back_rounded),
          label: const Text('العودة'),
        ),
      ],
    ],
  );

  static String _formatTime(int seconds) {
    final minutes = seconds ~/ 60;
    final remaining = seconds % 60;
    return '$minutes:${remaining.toString().padLeft(2, '0')}';
  }
}

class _PageHeader extends StatelessWidget {
  const _PageHeader({
    required this.icon,
    required this.title,
    required this.text,
  });

  final IconData icon;
  final String title;
  final String text;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(18),
    decoration: BoxDecoration(
      color: AppColors.primaryDark,
      borderRadius: BorderRadius.circular(8),
    ),
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, color: Colors.white, size: 34),
        const SizedBox(height: 12),
        Text(
          title,
          style: const TextStyle(
            color: Colors.white,
            fontSize: 22,
            fontWeight: FontWeight.w900,
          ),
        ),
        const SizedBox(height: 6),
        Text(text, style: const TextStyle(color: Color(0xFFD7FFFA))),
      ],
    ),
  );
}

class _Notice extends StatelessWidget {
  const _Notice({required this.text, this.error = false});
  final String text;
  final bool error;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(11),
    decoration: BoxDecoration(
      color: error ? Colors.red.shade50 : const Color(0xFFEAF7F3),
      borderRadius: BorderRadius.circular(8),
      border: Border.all(
        color: error ? Colors.red.shade100 : const Color(0xFFCDECE4),
      ),
    ),
    child: Text(
      text,
      style: TextStyle(
        color: error ? Colors.red.shade800 : AppColors.primaryDark,
      ),
    ),
  );
}
