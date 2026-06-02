import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../../core/api_client.dart';
import '../../../widgets/auth_shell.dart';
import 'login_screen.dart';

class ForgotPasswordScreen extends StatefulWidget {
  const ForgotPasswordScreen({super.key});
  @override
  State<ForgotPasswordScreen> createState() => _ForgotPasswordScreenState();
}

class _ForgotPasswordScreenState extends State<ForgotPasswordScreen> {
  final identifier = TextEditingController();
  bool loading = false, sent = false;
  String? error;
  Future<void> submit() async {
    setState(() {
      loading = true;
      error = null;
    });
    try {
      await ApiClient().dio.post(
        '/User/password/reset-link',
        queryParameters: {'identifier': identifier.text.trim()},
      );
      setState(() => sent = true);
    } catch (e) {
      setState(() => error = ApiClient.errorMessage(e));
    } finally {
      if (mounted) setState(() => loading = false);
    }
  }

  @override
  Widget build(BuildContext context) => AuthShell(
    title: 'استعادة كلمة المرور',
    subtitle:
        'أدخل بريدك الإلكتروني أو رقم الهاتف وسنرسل رابط إعادة التعيين إلى بريدك.',
    child: sent
        ? Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Text(
                'تم إرسال الرابط. راجع بريدك الإلكتروني لإكمال العملية.',
              ),
              const SizedBox(height: 14),
              FilledButton(
                onPressed: () => context.go('/login'),
                child: const Text('العودة لتسجيل الدخول'),
              ),
            ],
          )
        : Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              if (error != null) ErrorText(error!),
              TextField(
                controller: identifier,
                decoration: const InputDecoration(
                  labelText: 'البريد الإلكتروني أو رقم الهاتف',
                ),
              ),
              const SizedBox(height: 14),
              FilledButton(
                onPressed: loading ? null : submit,
                child: Text(
                  loading ? 'جارِ الإرسال...' : 'إرسال رابط الاستعادة',
                ),
              ),
            ],
          ),
  );
}
