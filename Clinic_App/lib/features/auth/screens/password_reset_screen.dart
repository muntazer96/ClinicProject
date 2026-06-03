import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../core/api_client.dart';
import '../../../widgets/auth_shell.dart';
import 'login_screen.dart';
import 'register_screen.dart';

class PasswordResetScreen extends StatefulWidget {
  const PasswordResetScreen({super.key, this.userId, this.token});
  final String? userId, token;
  @override
  State<PasswordResetScreen> createState() => _PasswordResetScreenState();
}

class _PasswordResetScreenState extends State<PasswordResetScreen> {
  final password = TextEditingController(), confirm = TextEditingController();
  bool loading = false, done = false;
  bool showPassword = false, showConfirm = false;
  String? error;

  bool get canSubmit =>
      password.text.isNotEmpty && confirm.text.isNotEmpty && !loading;

  @override
  void initState() {
    super.initState();
    password.addListener(_refresh);
    confirm.addListener(_refresh);
  }

  @override
  void dispose() {
    password.removeListener(_refresh);
    confirm.removeListener(_refresh);
    password.dispose();
    confirm.dispose();
    super.dispose();
  }

  void _refresh() {
    if (mounted) setState(() {});
  }

  Future<void> submit() async {
    if (!canSubmit) return;
    if (widget.userId == null || widget.token == null) {
      setState(() => error = 'رابط إعادة التعيين غير صالح.');
      return;
    }
    if (password.text.length < 6) {
      setState(() => error = 'كلمة المرور يجب أن لا تقل عن ستة أحرف.');
      return;
    }
    if (password.text != confirm.text) {
      setState(() => error = 'كلمتا المرور غير متطابقتين.');
      return;
    }
    setState(() {
      loading = true;
      error = null;
    });
    try {
      await ApiClient().dio.post(
        '/User/password/reset',
        data: {
          'userId': widget.userId,
          'token': widget.token,
          'newPassword': password.text,
        },
      );
      if (mounted) setState(() => done = true);
    } catch (e) {
      if (mounted) setState(() => error = ApiClient.errorMessage(e));
    } finally {
      if (mounted) setState(() => loading = false);
    }
  }

  @override
  Widget build(BuildContext context) => AuthShell(
    title: 'كلمة مرور جديدة',
    subtitle: 'اختر كلمة مرور جديدة لا تقل عن ستة أحرف.',
    child: done
        ? FilledButton(
            onPressed: () => context.go('/login'),
            child: const Text('تم الحفظ، سجّل الدخول'),
          )
        : Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              if (error != null) ErrorText(error!),
              TextField(
                controller: password,
                obscureText: !showPassword,
                textInputAction: TextInputAction.next,
                decoration: InputDecoration(
                  labelText: 'كلمة المرور الجديدة',
                  prefixIcon: const Icon(Icons.lock_outline),
                  suffixIcon: IconButton(
                    tooltip: showPassword
                        ? 'إخفاء كلمة المرور'
                        : 'إظهار كلمة المرور',
                    onPressed: () =>
                        setState(() => showPassword = !showPassword),
                    icon: Icon(
                      showPassword
                          ? Icons.visibility_off_outlined
                          : Icons.visibility_outlined,
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 8),
              PasswordStrength(password.text),
              const SizedBox(height: 10),
              TextField(
                controller: confirm,
                obscureText: !showConfirm,
                textInputAction: TextInputAction.done,
                onSubmitted: (_) => submit(),
                decoration: InputDecoration(
                  labelText: 'تأكيد كلمة المرور',
                  prefixIcon: const Icon(Icons.lock_outline),
                  suffixIcon: IconButton(
                    tooltip: showConfirm
                        ? 'إخفاء كلمة المرور'
                        : 'إظهار كلمة المرور',
                    onPressed: () =>
                        setState(() => showConfirm = !showConfirm),
                    icon: Icon(
                      showConfirm
                          ? Icons.visibility_off_outlined
                          : Icons.visibility_outlined,
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 14),
              FilledButton(
                onPressed: canSubmit ? submit : null,
                child: Text(
                  loading ? 'جارِ الحفظ...' : 'حفظ كلمة المرور',
                ),
              ),
            ],
          ),
  );
}
