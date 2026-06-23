import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../widgets/auth_shell.dart';
import '../auth_controller.dart';
import 'login_screen.dart';
import 'register_screen.dart';

class PasswordResetScreen extends StatefulWidget {
  const PasswordResetScreen({super.key, this.phoneNumber, this.resetToken});
  final String? phoneNumber, resetToken;
  @override
  State<PasswordResetScreen> createState() => _PasswordResetScreenState();
}

class _PasswordResetScreenState extends State<PasswordResetScreen> {
  final password = TextEditingController(), confirm = TextEditingController();
  bool loading = false;
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
    if (widget.phoneNumber == null || widget.resetToken == null) {
      setState(() => error = 'رمز إعادة التعيين غير صالح.');
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
      final response = await context.read<ApiClient>().dio.post(
        '/User/password/reset',
        data: {
          'phoneNumber': widget.phoneNumber,
          'resetToken': widget.resetToken,
          'newPassword': password.text,
        },
      );
      final data = response.data['data'] as Map<String, dynamic>? ?? {};
      final token = data['token'] as String? ?? '';
      final refreshToken = data['refreshToken'] as String?;
      if (!mounted) return;
      await context.read<AuthController>().completeLoginFromTokens(
        phoneNumber: widget.phoneNumber!,
        token: token,
        refreshToken: refreshToken,
      );
      if (!mounted) return;
      final auth = context.read<AuthController>();
      context.go(auth.isDoctor ? '/doctor' : '/');
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
    child: Column(
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
