import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../../core/api_client.dart';
import '../../../widgets/auth_shell.dart';
import 'login_screen.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});
  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}
class _RegisterScreenState extends State<RegisterScreen> {
  final name = TextEditingController(), phone = TextEditingController(), email = TextEditingController(), password = TextEditingController(), confirm = TextEditingController();
  bool loading = false; String? error;
  Future<void> submit() async {
    if (password.text != confirm.text) { setState(() => error = 'كلمتا المرور غير متطابقتين.'); return; }
    setState(() { loading = true; error = null; });
    try {
      await ApiClient().dio.post('/User/signup', data: {'name': name.text.trim(), 'phoneNumber': phone.text.trim(), 'email': email.text.trim(), 'password': password.text});
      if (mounted) context.go('/email-confirm?identifier=${Uri.encodeComponent(email.text.trim())}');
    } catch (e) { setState(() => error = ApiClient.errorMessage(e)); }
    finally { if (mounted) setState(() => loading = false); }
  }
  @override
  Widget build(BuildContext context) => AuthShell(title: 'إنشاء حساب جديد', subtitle: 'سجّل بياناتك مرة واحدة لتتابع حجوزاتك بسهولة.', child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
    if (error != null) ErrorText(error!),
    TextField(controller: name, decoration: const InputDecoration(labelText: 'الاسم الكامل')),
    const SizedBox(height: 10), TextField(controller: phone, keyboardType: TextInputType.phone, decoration: const InputDecoration(labelText: 'رقم الهاتف')),
    const SizedBox(height: 10), TextField(controller: email, keyboardType: TextInputType.emailAddress, decoration: const InputDecoration(labelText: 'البريد الإلكتروني')),
    const SizedBox(height: 10), TextField(controller: password, obscureText: true, decoration: const InputDecoration(labelText: 'كلمة المرور')),
    const SizedBox(height: 10), TextField(controller: confirm, obscureText: true, decoration: const InputDecoration(labelText: 'تأكيد كلمة المرور')),
    const SizedBox(height: 14), FilledButton(onPressed: loading ? null : submit, child: Text(loading ? 'جارِ الإنشاء...' : 'إنشاء الحساب')),
    TextButton(onPressed: () => context.go('/login'), child: const Text('لديك حساب؟ سجّل دخولك')),
  ]));
}
