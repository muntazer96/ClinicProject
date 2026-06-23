import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_theme.dart';
import '../../../core/phone_number_validator.dart';
import '../../../widgets/auth_shell.dart';
import '../../../widgets/developer_credit.dart';
import 'login_screen.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final firstName = TextEditingController();
  final secondName = TextEditingController();
  final phone = TextEditingController();
  final password = TextEditingController();
  final confirm = TextEditingController();

  bool loading = false;
  bool showPassword = false;
  bool showConfirm = false;
  String? error;

  bool get canSubmit =>
      firstName.text.trim().isNotEmpty &&
      secondName.text.trim().isNotEmpty &&
      phone.text.trim().isNotEmpty &&
      password.text.isNotEmpty &&
      confirm.text.isNotEmpty &&
      !loading;

  String get fullName => '${firstName.text.trim()} ${secondName.text.trim()}';

  @override
  void initState() {
    super.initState();

    for (final controller in [
      firstName,
      secondName,
      phone,
      password,
      confirm,
    ]) {
      controller.addListener(_refresh);
    }
  }

  @override
  void dispose() {
    for (final controller in [
      firstName,
      secondName,
      phone,
      password,
      confirm,
    ]) {
      controller.removeListener(_refresh);
      controller.dispose();
    }

    super.dispose();
  }

  void _refresh() {
    if (mounted) setState(() {});
  }

  Future<void> submit() async {
    if (!canSubmit) return;

    if (password.text.length < 6) {
      setState(() => error = 'كلمة المرور يجب أن لا تقل عن ستة أحرف.');
      return;
    }

    if (!isValidIraqiPhone(phone.text)) {
      setState(() => error = iraqiPhoneError);
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
      await context.read<ApiClient>().dio.post(
        '/User/signup',
        data: {
          'name': fullName,
          'phoneNumber': phone.text.trim(),
          'password': password.text,
        },
      );

      if (mounted) {
        context.go('/login');
      }
    } catch (e) {
      if (mounted) setState(() => error = ApiClient.errorMessage(e));
    } finally {
      if (mounted) setState(() => loading = false);
    }
  }

  @override
  Widget build(BuildContext context) => AuthShell(
    title: 'إنشاء حساب جديد',
    subtitle: 'سجّل بياناتك مرة واحدة لتتابع حجوزاتك بسهولة.',
    footer: const DeveloperCredit(compact: true),
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        if (error != null) ErrorText(error!),

        TextField(
          controller: firstName,
          textInputAction: TextInputAction.next,
          decoration: const InputDecoration(
            labelText: 'الاسم الأول',
            prefixIcon: Icon(Icons.person_outline),
          ),
        ),

        const SizedBox(height: 10),

        TextField(
          controller: secondName,
          textInputAction: TextInputAction.next,
          decoration: const InputDecoration(
            labelText: 'الاسم الثاني',
            prefixIcon: Icon(Icons.badge_outlined),
          ),
        ),

        const SizedBox(height: 10),

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

        const SizedBox(height: 10),

        TextField(
          controller: password,
          obscureText: !showPassword,
          textInputAction: TextInputAction.next,
          decoration: InputDecoration(
            labelText: 'كلمة المرور',
            prefixIcon: const Icon(Icons.lock_outline),
            suffixIcon: IconButton(
              tooltip: showPassword ? 'إخفاء كلمة المرور' : 'إظهار كلمة المرور',
              onPressed: () {
                setState(() => showPassword = !showPassword);
              },
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
              tooltip: showConfirm ? 'إخفاء كلمة المرور' : 'إظهار كلمة المرور',
              onPressed: () {
                setState(() => showConfirm = !showConfirm);
              },
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
          child: Text(loading ? 'جارِ الإنشاء...' : 'إنشاء الحساب'),
        ),

        TextButton(
          onPressed: () => context.go('/login'),
          child: const Text('لديك حساب؟ سجّل دخولك'),
        ),
      ],
    ),
  );
}

class PasswordStrength extends StatelessWidget {
  const PasswordStrength(this.password, {super.key});

  final String password;

  int get _score {
    var score = 0;

    if (password.length >= 6) score++;
    if (password.length >= 10) score++;
    if (RegExp(r'[A-Z]').hasMatch(password)) score++;
    if (RegExp(r'[0-9]').hasMatch(password)) score++;
    if (RegExp(r'[^A-Za-z0-9]').hasMatch(password)) score++;

    return score.clamp(0, 5);
  }

  @override
  Widget build(BuildContext context) {
    final score = _score;

    final color = score <= 2
        ? Colors.red.shade700
        : score <= 4
        ? AppColors.warning
        : AppColors.primary;

    final label = password.isEmpty
        ? 'أدخل كلمة مرور لا تقل عن ستة أحرف.'
        : score <= 2
        ? 'كلمة المرور ضعيفة'
        : score <= 4
        ? 'كلمة المرور متوسطة'
        : 'كلمة المرور قوية';

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        ClipRRect(
          borderRadius: BorderRadius.circular(999),
          child: LinearProgressIndicator(
            value: password.isEmpty ? .05 : score / 5,
            minHeight: 6,
            color: color,
            backgroundColor: AppColors.border,
          ),
        ),
        const SizedBox(height: 6),
        Text(label, style: TextStyle(color: color, fontSize: 12)),
      ],
    );
  }
}
