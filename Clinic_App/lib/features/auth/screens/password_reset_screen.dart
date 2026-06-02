import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../../core/api_client.dart';
import '../../../widgets/auth_shell.dart';
import 'login_screen.dart';

class PasswordResetScreen extends StatefulWidget {
  const PasswordResetScreen({super.key, this.userId, this.token});
  final String? userId, token;
  @override
  State<PasswordResetScreen> createState() => _PasswordResetScreenState();
}
class _PasswordResetScreenState extends State<PasswordResetScreen> {
  final password = TextEditingController(), confirm = TextEditingController(); bool loading = false, done = false; String? error;
  Future<void> submit() async {
    if (widget.userId == null || widget.token == null) { setState(() => error = 'رابط إعادة التعيين غير صالح.'); return; }
    if (password.text != confirm.text) { setState(() => error = 'كلمتا المرور غير متطابقتين.'); return; }
    setState(() { loading = true; error = null; });
    try { await ApiClient().dio.post('/User/password/reset', data: {'userId': widget.userId, 'token': widget.token, 'newPassword': password.text}); setState(() => done = true); }
    catch (e) { setState(() => error = ApiClient.errorMessage(e)); }
    finally { if (mounted) setState(() => loading = false); }
  }
  @override
  Widget build(BuildContext context) => AuthShell(title: 'كلمة مرور جديدة', subtitle: 'اختر كلمة مرور جديدة لا تقل عن ستة أحرف.', child: done
      ? FilledButton(onPressed: () => context.go('/login'), child: const Text('تم الحفظ، سجّل الدخول'))
      : Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [if (error != null) ErrorText(error!), TextField(controller: password, obscureText: true, decoration: const InputDecoration(labelText: 'كلمة المرور الجديدة')), const SizedBox(height: 10), TextField(controller: confirm, obscureText: true, decoration: const InputDecoration(labelText: 'تأكيد كلمة المرور')), const SizedBox(height: 14), FilledButton(onPressed: loading ? null : submit, child: Text(loading ? 'جارِ الحفظ...' : 'حفظ كلمة المرور'))]));
}
