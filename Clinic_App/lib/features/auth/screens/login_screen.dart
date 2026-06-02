import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../widgets/auth_shell.dart';
import '../auth_controller.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key, this.redirect});
  final String? redirect;
  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final phone = TextEditingController(), password = TextEditingController();
  String? error;
  Future<void> submit() async {
    setState(() => error = null);
    try {
      await context.read<AuthController>().login(
        phone.text.trim(),
        password.text,
      );
      if (mounted) context.go(widget.redirect ?? '/');
    } catch (e) {
      setState(() => error = ApiClient.errorMessage(e));
    }
  }

  @override
  Widget build(BuildContext context) {
    final loading = context.watch<AuthController>().loading;
    return AuthShell(
      title: 'تسجيل الدخول',
      subtitle: 'أدخل رقم الهاتف وكلمة المرور للوصول إلى حجوزاتك.',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          if (error != null) ErrorText(error!),
          TextField(
            controller: phone,
            keyboardType: TextInputType.phone,
            decoration: const InputDecoration(labelText: 'رقم الهاتف'),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: password,
            obscureText: true,
            decoration: const InputDecoration(labelText: 'كلمة المرور'),
          ),
          Align(
            alignment: Alignment.centerLeft,
            child: TextButton(
              onPressed: () => context.go('/forgot-password'),
              child: const Text('نسيت كلمة المرور؟'),
            ),
          ),
          FilledButton(
            onPressed: loading ? null : submit,
            child: Text(loading ? 'جارِ الدخول...' : 'تسجيل الدخول'),
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
