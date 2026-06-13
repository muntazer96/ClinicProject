import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../../core/api_client.dart';
import '../../../widgets/auth_shell.dart';
import 'login_screen.dart';

class EmailConfirmScreen extends StatefulWidget {
  const EmailConfirmScreen({
    super.key,
    this.userId,
    this.token,
    this.identifier,
  });
  final String? userId, token, identifier;
  @override
  State<EmailConfirmScreen> createState() => _EmailConfirmScreenState();
}

class _EmailConfirmScreenState extends State<EmailConfirmScreen> {
  bool loading = false, done = false, sent = false;
  String? error;
  @override
  void initState() {
    super.initState();
    if (widget.userId != null && widget.token != null) confirm();
  }

  Future<void> confirm() async {
    setState(() => loading = true);
    try {
      await context.read<ApiClient>().dio.get(
        '/User/email-confirm',
        queryParameters: {'userId': widget.userId, 'token': widget.token},
      );
      setState(() => done = true);
    } catch (e) {
      setState(() => error = ApiClient.errorMessage(e));
    } finally {
      if (mounted) setState(() => loading = false);
    }
  }

  Future<void> resend() async {
    setState(() {
      loading = true;
      error = null;
    });
    try {
      await context.read<ApiClient>().dio.post(
        '/User/email-confirmation',
        queryParameters: {'identifier': widget.identifier},
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
    title: 'تأكيد البريد الإلكتروني',
    subtitle: 'فعّل بريدك لحماية حسابك واستعادة كلمة المرور عند الحاجة.',
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        if (error != null) ErrorText(error!),
        Text(
          loading
              ? 'جارِ تنفيذ الطلب...'
              : done
              ? 'تم تفعيل بريدك بنجاح.'
              : sent
              ? 'تم إرسال رسالة التفعيل. راجع بريدك الإلكتروني.'
              : 'أرسل رسالة التفعيل إلى بريدك الإلكتروني.',
        ),
        const SizedBox(height: 14),
        if (!done && widget.identifier != null)
          FilledButton(
            onPressed: loading ? null : resend,
            child: const Text('إرسال رابط التفعيل'),
          ),
        TextButton(
          onPressed: () => context.go('/login'),
          child: const Text('المتابعة إلى تسجيل الدخول'),
        ),
      ],
    ),
  );
}
