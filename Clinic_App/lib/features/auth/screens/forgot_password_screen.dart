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

  bool get canSubmit => identifier.text.trim().isNotEmpty && !loading;

  @override
  void initState() {
    super.initState();
    identifier.addListener(_refresh);
  }

  @override
  void dispose() {
    identifier.removeListener(_refresh);
    identifier.dispose();
    super.dispose();
  }

  void _refresh() {
    if (mounted) setState(() {});
  }

  Future<void> submit() async {
    if (!canSubmit) {
      setState(() => error = 'أدخل البريد الإلكتروني أو رقم الهاتف.');
      return;
    }
    setState(() {
      loading = true;
      error = null;
    });
    try {
      await ApiClient().dio.post(
        '/User/password/reset-link',
        queryParameters: {'identifier': identifier.text.trim()},
      );
      if (mounted) setState(() => sent = true);
    } catch (e) {
      if (mounted) setState(() => error = ApiClient.errorMessage(e));
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
              const _SuccessNotice(
                text:
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
                keyboardType: TextInputType.emailAddress,
                textInputAction: TextInputAction.done,
                onSubmitted: (_) => submit(),
                decoration: const InputDecoration(
                  labelText: 'البريد الإلكتروني أو رقم الهاتف',
                  prefixIcon: Icon(Icons.alternate_email_outlined),
                ),
              ),
              const SizedBox(height: 14),
              FilledButton(
                onPressed: canSubmit ? submit : null,
                child: Text(
                  loading ? 'جارِ الإرسال...' : 'إرسال رابط الاستعادة',
                ),
              ),
            ],
          ),
  );
}

class _SuccessNotice extends StatelessWidget {
  const _SuccessNotice({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(12),
    decoration: BoxDecoration(
      color: const Color(0xFFE4F4F0),
      borderRadius: BorderRadius.circular(12),
    ),
    child: Text(text, textAlign: TextAlign.center),
  );
}
