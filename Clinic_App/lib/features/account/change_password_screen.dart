import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import '../../core/app_theme.dart';
import '../auth/auth_controller.dart';
import 'profile_service.dart';

class ChangePasswordScreen extends StatefulWidget {
  const ChangePasswordScreen({super.key});

  @override
  State<ChangePasswordScreen> createState() => _ChangePasswordScreenState();
}

class _ChangePasswordScreenState extends State<ChangePasswordScreen> {
  late final ProfileService _service;
  final _currentPassword = TextEditingController();
  final _newPassword = TextEditingController();
  final _confirmPassword = TextEditingController();
  bool _loading = false;
  bool _showCurrent = false;
  bool _showNew = false;
  bool _showConfirm = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _service = ProfileService(context.read<AuthController>().api);
  }

  @override
  void dispose() {
    _currentPassword.dispose();
    _newPassword.dispose();
    _confirmPassword.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (_currentPassword.text.isEmpty || _newPassword.text.isEmpty) {
      setState(() => _error = 'أدخل كلمة السر الحالية والجديدة.');
      return;
    }
    if (_newPassword.text.length < 6) {
      setState(() => _error = 'كلمة السر الجديدة يجب أن لا تقل عن 6 أحرف.');
      return;
    }
    if (_newPassword.text != _confirmPassword.text) {
      setState(() => _error = 'تأكيد كلمة السر لا يطابق كلمة السر الجديدة.');
      return;
    }

    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      await _service.changePassword(
        currentPassword: _currentPassword.text,
        newPassword: _newPassword.text,
      );
      if (!mounted) return;
      await context.read<AuthController>().logout();
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('تم تغيير كلمة المرور. سجل الدخول مجدداً.'),
        ),
      );
      context.go('/login');
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) => ListView(
    padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
    children: [
      const Text(
        'تغيير كلمة المرور',
        style: TextStyle(fontSize: 23, fontWeight: FontWeight.w900),
      ),
      const SizedBox(height: 6),
      const Text(
        'بعد تغيير كلمة المرور سيتم تسجيل خروجك للحفاظ على أمان الحساب.',
        style: TextStyle(color: AppColors.muted),
      ),
      const SizedBox(height: 16),
      Card(
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Column(
            children: [
              _PasswordField(
                controller: _currentPassword,
                label: 'كلمة السر الحالية',
                visible: _showCurrent,
                onToggle: () => setState(() => _showCurrent = !_showCurrent),
              ),
              const SizedBox(height: 10),
              _PasswordField(
                controller: _newPassword,
                label: 'كلمة السر الجديدة',
                visible: _showNew,
                onToggle: () => setState(() => _showNew = !_showNew),
              ),
              const SizedBox(height: 10),
              _PasswordField(
                controller: _confirmPassword,
                label: 'تأكيد كلمة السر الجديدة',
                visible: _showConfirm,
                onToggle: () => setState(() => _showConfirm = !_showConfirm),
              ),
              if (_error != null) ...[
                const SizedBox(height: 10),
                Text(
                  _error!,
                  style: TextStyle(color: Colors.red.shade800),
                  textAlign: TextAlign.center,
                ),
              ],
              const SizedBox(height: 14),
              FilledButton.icon(
                onPressed: _loading ? null : _submit,
                icon: const Icon(Icons.lock_reset_rounded),
                label: Text(_loading ? 'جاري التغيير...' : 'تغيير كلمة المرور'),
              ),
            ],
          ),
        ),
      ),
    ],
  );
}

class _PasswordField extends StatelessWidget {
  const _PasswordField({
    required this.controller,
    required this.label,
    required this.visible,
    required this.onToggle,
  });

  final TextEditingController controller;
  final String label;
  final bool visible;
  final VoidCallback onToggle;

  @override
  Widget build(BuildContext context) => TextField(
    controller: controller,
    obscureText: !visible,
    decoration: InputDecoration(
      labelText: label,
      prefixIcon: const Icon(Icons.lock_outline),
      suffixIcon: IconButton(
        tooltip: visible ? 'إخفاء' : 'إظهار',
        onPressed: onToggle,
        icon: Icon(
          visible ? Icons.visibility_off_outlined : Icons.visibility_outlined,
        ),
      ),
    ),
  );
}
