import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/phone_number_validator.dart';
import '../../../widgets/auth_shell.dart';
import '../../../widgets/developer_credit.dart';
import '../auth_controller.dart';
import '../widgets/google_web_button.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key, this.redirect});
  final String? redirect;
  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final phone = TextEditingController(), password = TextEditingController();
  String? error;
  bool showPassword = false;
  bool _googleSetupStarted = false;

  bool get canSubmit =>
      phone.text.trim().isNotEmpty && password.text.isNotEmpty;

  @override
  void initState() {
    super.initState();
    phone.addListener(_refresh);
    password.addListener(_refresh);
  }

  @override
  void dispose() {
    phone.removeListener(_refresh);
    password.removeListener(_refresh);
    phone.dispose();
    password.dispose();
    super.dispose();
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (kIsWeb && !_googleSetupStarted) {
      _googleSetupStarted = true;
      unawaited(_initializeGoogleForWeb());
    }
  }

  void _refresh() {
    if (mounted) setState(() {});
  }

  Future<void> _initializeGoogleForWeb() async {
    try {
      await context.read<AuthController>().ensureGoogleSignInInitialized();
    } catch (e) {
      if (mounted) setState(() => error = ApiClient.errorMessage(e));
    }
  }

  Future<void> submit() async {
    if (!canSubmit) {
      setState(() => error = 'أدخل رقم الهاتف وكلمة المرور.');
      return;
    }
    if (!isValidIraqiPhone(phone.text)) {
      setState(() => error = iraqiPhoneError);
      return;
    }
    setState(() => error = null);
    try {
      await context.read<AuthController>().login(
        phone.text.trim(),
        password.text,
      );
      if (mounted) {
        final auth = context.read<AuthController>();
        context.go(widget.redirect ?? (auth.isDoctor ? '/doctor' : '/'));
      }
    } catch (e) {
      if (mounted) setState(() => error = ApiClient.errorMessage(e));
    }
  }

  Future<void> submitGoogle() async {
    setState(() => error = null);
    try {
      await context.read<AuthController>().loginWithGoogle();
      if (mounted) {
        final auth = context.read<AuthController>();
        context.go(widget.redirect ?? (auth.isDoctor ? '/doctor' : '/'));
      }
    } catch (e) {
      if (mounted) setState(() => error = ApiClient.errorMessage(e));
    }
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthController>();
    final loading = auth.loading;
    if (kIsWeb && auth.isAuthenticated) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        if (mounted) {
          context.go(widget.redirect ?? (auth.isDoctor ? '/doctor' : '/'));
        }
      });
    }
    return AuthShell(
      title: 'تسجيل الدخول',
      subtitle: 'أدخل رقم الهاتف وكلمة المرور للوصول إلى حجوزاتك.',
      footer: const DeveloperCredit(compact: true),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          if (error != null) ErrorText(error!),
          TextField(
            controller: phone,
            keyboardType: TextInputType.phone,
            inputFormatters: iraqiPhoneInputFormatters,
            textInputAction: TextInputAction.next,
            decoration: const InputDecoration(
              labelText: 'رقم الهاتف',
              prefixIcon: Icon(Icons.phone_outlined),
            ),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: password,
            obscureText: !showPassword,
            textInputAction: TextInputAction.done,
            onSubmitted: (_) => submit(),
            decoration: InputDecoration(
              labelText: 'كلمة المرور',
              prefixIcon: const Icon(Icons.lock_outline),
              suffixIcon: IconButton(
                tooltip: showPassword
                    ? 'إخفاء كلمة المرور'
                    : 'إظهار كلمة المرور',
                onPressed: () => setState(() => showPassword = !showPassword),
                icon: Icon(
                  showPassword
                      ? Icons.visibility_off_outlined
                      : Icons.visibility_outlined,
                ),
              ),
            ),
          ),
          Align(
            alignment: Alignment.centerLeft,
            child: TextButton(
              onPressed: () => context.go('/forgot-password'),
              child: const Text('نسيت كلمة المرور؟'),
            ),
          ),
          FilledButton(
            onPressed: loading || !canSubmit ? null : submit,
            child: Text(loading ? 'جارِ الدخول...' : 'تسجيل الدخول'),
          ),
          const SizedBox(height: 10),
          if (kIsWeb)
            Center(child: buildGoogleSignInWebButton())
          else
            OutlinedButton.icon(
              onPressed: loading ? null : submitGoogle,
              icon: const Text(
                'G',
                style: TextStyle(fontWeight: FontWeight.w900),
              ),
              label: const Text('المتابعة باستخدام Google'),
            ),
          TextButton(
            onPressed: () => context.go('/register'),
            child: const Text('ليس لديك حساب؟ أنشئ حساباً جديداً'),
          ),
        ],
      ),
    );
  }
}

class ErrorText extends StatelessWidget {
  const ErrorText(this.text, {super.key});
  final String text;
  @override
  Widget build(BuildContext context) => Container(
    margin: const EdgeInsets.only(bottom: 12),
    padding: const EdgeInsets.all(10),
    decoration: BoxDecoration(
      color: Colors.red.shade50,
      borderRadius: BorderRadius.circular(8),
    ),
    child: Text(text, style: TextStyle(color: Colors.red.shade800)),
  );
}
