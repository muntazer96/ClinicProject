import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import '../../core/app_snack_bar.dart';
import '../../core/app_theme.dart';
import '../auth/auth_controller.dart';
import 'profile_service.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  late final ProfileService _service;
  UserProfile? _profile;
  bool _loading = true;
  bool _savingName = false;
  bool _sendingEmailConfirmation = false;
  bool _sendingPhoneConfirmation = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _service = ProfileService(context.read<AuthController>().api);
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final profile = await _service.getProfile();
      if (!mounted) return;
      setState(() => _profile = profile);
      context.read<AuthController>().setProfile(profile);
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _editName() async {
    final profile = _profile;
    if (profile == null) return;
    final name = await context.push<String>(
      '/profile/edit-name?name=${Uri.encodeComponent(profile.name)}',
    );
    if (name == null || name.isEmpty || name == profile.name) return;

    setState(() {
      _savingName = true;
      _error = null;
    });
    try {
      final updated = await _service.updateName(current: profile, name: name);
      if (!mounted) return;
      setState(() {
        _profile = updated;
      });
      context.read<AuthController>().setProfile(updated);
      showAppSnackBar(
        context,
        'تم تحديث الاسم بنجاح.',
        type: AppSnackBarType.success,
      );
    } catch (error) {
      if (mounted) {
        showAppSnackBar(
          context,
          ApiClient.errorMessage(error),
          type: AppSnackBarType.error,
        );
      }
    } finally {
      if (mounted) setState(() => _savingName = false);
    }
  }

  Future<void> _sendEmailConfirmation() async {
    final profile = _profile;
    if (profile == null || profile.email.isEmpty) return;
    setState(() {
      _sendingEmailConfirmation = true;
      _error = null;
    });
    try {
      await _service.sendEmailConfirmation(profile.email);
      if (mounted) {
        showAppSnackBar(
          context,
          'تم إرسال رابط تأكيد البريد الإلكتروني.',
          type: AppSnackBarType.success,
        );
      }
    } catch (error) {
      if (mounted) {
        showAppSnackBar(
          context,
          ApiClient.errorMessage(error),
          type: AppSnackBarType.error,
        );
      }
    } finally {
      if (mounted) setState(() => _sendingEmailConfirmation = false);
    }
  }

  Future<void> _sendPhoneConfirmation() async {
    setState(() {
      _sendingPhoneConfirmation = true;
      _error = null;
    });
    try {
      await _service.sendPhoneConfirmation();
      if (!mounted) return;
      setState(() {
        _sendingPhoneConfirmation = false;
      });
      showAppSnackBar(
        context,
        'تم إرسال رمز تأكيد الهاتف.',
        type: AppSnackBarType.success,
      );
      await _openPhoneConfirmationPage();
    } catch (error) {
      final message = ApiClient.errorMessage(error);
      if (message.contains('رمز جديد') && mounted) {
        setState(() {
          _sendingPhoneConfirmation = false;
        });
        showAppSnackBar(context, message, type: AppSnackBarType.warning);
        await _openPhoneConfirmationPage();
      } else if (mounted) {
        showAppSnackBar(context, message, type: AppSnackBarType.error);
      }
    } finally {
      if (mounted) setState(() => _sendingPhoneConfirmation = false);
    }
  }

  Future<void> _openPhoneConfirmationPage() async {
    final confirmed = await context.push<bool>('/profile/confirm-phone');
    if (confirmed == true) {
      if (mounted) setState(() => _sendingPhoneConfirmation = false);
      await _load();
    }
  }

  Future<void> _logout() async {
    await context.read<AuthController>().logout();
    if (mounted) context.go('/');
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) return const Center(child: CircularProgressIndicator());

    if (_profile == null) {
      return _ProfileMessage(
        icon: Icons.account_circle_outlined,
        title: 'سجل دخولك',
        text: _error ?? 'سجل الدخول حتى تدير ملفك الشخصي وحجوزاتك.',
        action: () => context.go('/login?redirect=/profile'),
        actionText: 'تسجيل الدخول',
      );
    }

    final profile = _profile!;
    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.fromLTRB(16, 12, 16, 28),
        children: [
          _ProfileHero(
            profile: profile,
            savingName: _savingName,
            onEditName: _editName,
          ),
          const SizedBox(height: 12),
          if (_error != null) _Notice(text: _error!, error: true),
          if (_error != null) const SizedBox(height: 12),
          _SectionCard(
            title: 'معلومات الحساب',
            icon: Icons.badge_outlined,
            child: Column(
              children: [
                _InfoTile(
                  icon: Icons.phone_outlined,
                  label: 'رقم الهاتف',
                  value: profile.phoneNumber,
                ),
                _InfoTile(
                  icon: Icons.email_outlined,
                  label: 'البريد الإلكتروني',
                  value: profile.email,
                ),
              ],
            ),
          ),
          if (!profile.phoneConfirmed || !profile.emailConfirmed) ...[
            const SizedBox(height: 12),
            _SectionCard(
              title: 'تأكيد الحساب',
              icon: Icons.verified_outlined,
              child: Column(
                children: [
                  if (!profile.phoneConfirmed)
                    _FullWidthButton(
                      child: FilledButton.icon(
                        onPressed: _sendingPhoneConfirmation
                            ? null
                            : _sendPhoneConfirmation,
                        icon: const Icon(Icons.sms_outlined),
                        label: Text(
                          _sendingPhoneConfirmation
                              ? 'جاري الإرسال...'
                              : 'تأكيد رقم الهاتف',
                        ),
                      ),
                    ),
                  if (!profile.phoneConfirmed && !profile.emailConfirmed)
                    const SizedBox(height: 10),
                  if (!profile.emailConfirmed)
                    _FullWidthButton(
                      child: OutlinedButton.icon(
                        onPressed: _sendingEmailConfirmation
                            ? null
                            : _sendEmailConfirmation,
                        icon: const Icon(Icons.email_outlined),
                        label: Text(
                          _sendingEmailConfirmation
                              ? 'جاري الإرسال...'
                              : 'تأكيد البريد الإلكتروني',
                        ),
                      ),
                    ),
                ],
              ),
            ),
          ],
          const SizedBox(height: 12),
          _SectionCard(
            title: 'الأمان',
            icon: Icons.lock_outline,
            child: _FullWidthButton(
              child: FilledButton.icon(
                onPressed: () => context.go('/profile/change-password'),
                icon: const Icon(Icons.lock_reset_rounded),
                label: const Text('تغيير كلمة المرور'),
              ),
            ),
          ),
          const SizedBox(height: 12),
          _SectionCard(
            title: 'اختصارات',
            icon: Icons.grid_view_rounded,
            child: Column(
              children: [
                _ActionTile(
                  icon: Icons.calendar_month_outlined,
                  title: 'حجوزاتي',
                  subtitle: 'تابع الحجوزات النشطة والسابقة',
                  onTap: () => context.go('/bookings'),
                ),
                _ActionTile(
                  icon: Icons.search_rounded,
                  title: 'البحث عن طبيب',
                  subtitle: 'اختر الاختصاص والمحافظة واحجز دورك',
                  onTap: () => context.go('/search'),
                ),
              ],
            ),
          ),
          const SizedBox(height: 12),
          _FullWidthButton(
            child: OutlinedButton.icon(
              onPressed: _logout,
              icon: const Icon(Icons.logout_rounded),
              label: const Text('تسجيل الخروج'),
            ),
          ),
        ],
      ),
    );
  }
}

class _ProfileHero extends StatelessWidget {
  const _ProfileHero({
    required this.profile,
    required this.savingName,
    required this.onEditName,
  });
  final UserProfile profile;
  final bool savingName;
  final VoidCallback onEditName;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(18),
    decoration: BoxDecoration(
      color: AppColors.primaryDark,
      borderRadius: BorderRadius.circular(8),
    ),
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            CircleAvatar(
              radius: 34,
              backgroundColor: Colors.white.withValues(alpha: .18),
              child: Text(
                profile.initials,
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 22,
                  fontWeight: FontWeight.w900,
                ),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    profile.name.isEmpty ? 'مستخدم عيادتي' : profile.name,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 20,
                      fontWeight: FontWeight.w900,
                    ),
                  ),
                  const SizedBox(height: 3),
                  Text(
                    profile.phoneNumber,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(color: Color(0xFFD7FFFA)),
                  ),
                ],
              ),
            ),
            IconButton.filledTonal(
              tooltip: 'تعديل الاسم',
              onPressed: savingName ? null : onEditName,
              icon: savingName
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.edit_outlined),
            ),
          ],
        ),
        const SizedBox(height: 16),
        Wrap(
          spacing: 8,
          runSpacing: 8,
          children: [
            _StatusPill(
              label: profile.emailConfirmed ? 'البريد مؤكد' : 'البريد غير مؤكد',
              active: profile.emailConfirmed,
            ),
            _StatusPill(
              label: profile.phoneConfirmed ? 'الهاتف مؤكد' : 'الهاتف غير مؤكد',
              active: profile.phoneConfirmed,
            ),
          ],
        ),
      ],
    ),
  );
}

class _StatusPill extends StatelessWidget {
  const _StatusPill({required this.label, required this.active});
  final String label;
  final bool active;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 7),
    decoration: BoxDecoration(
      color: Colors.white.withValues(alpha: active ? .18 : .12),
      borderRadius: BorderRadius.circular(8),
    ),
    child: Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(
          active ? Icons.check_circle_rounded : Icons.info_outline_rounded,
          color: Colors.white,
          size: 15,
        ),
        const SizedBox(width: 5),
        Text(label, style: const TextStyle(color: Colors.white, fontSize: 12)),
      ],
    ),
  );
}

class _FullWidthButton extends StatelessWidget {
  const _FullWidthButton({required this.child});
  final Widget child;

  @override
  Widget build(BuildContext context) =>
      SizedBox(width: double.infinity, child: child);
}

class _SectionCard extends StatelessWidget {
  const _SectionCard({
    required this.title,
    required this.icon,
    required this.child,
  });

  final String title;
  final IconData icon;
  final Widget child;

  @override
  Widget build(BuildContext context) => Card(
    child: Padding(
      padding: const EdgeInsets.all(14),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(icon, color: AppColors.primary),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  title,
                  style: const TextStyle(
                    fontSize: 17,
                    fontWeight: FontWeight.w900,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          child,
        ],
      ),
    ),
  );
}

class _InfoTile extends StatelessWidget {
  const _InfoTile({
    required this.icon,
    required this.label,
    required this.value,
  });

  final IconData icon;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) => Container(
    margin: const EdgeInsets.only(bottom: 10),
    padding: const EdgeInsets.all(12),
    decoration: BoxDecoration(
      color: AppColors.surfaceMuted,
      borderRadius: BorderRadius.circular(8),
    ),
    child: Row(
      children: [
        Icon(icon, color: AppColors.primary),
        const SizedBox(width: 10),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                label,
                style: const TextStyle(color: AppColors.muted, fontSize: 12),
              ),
              Text(
                value.isEmpty ? '-' : value,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(fontWeight: FontWeight.w900),
              ),
            ],
          ),
        ),
      ],
    ),
  );
}

class _ActionTile extends StatelessWidget {
  const _ActionTile({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.onTap,
  });

  final IconData icon;
  final String title;
  final String subtitle;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => ListTile(
    contentPadding: EdgeInsets.zero,
    leading: CircleAvatar(
      backgroundColor: AppColors.softBlue,
      child: Icon(icon, color: AppColors.primary),
    ),
    title: Text(title, style: const TextStyle(fontWeight: FontWeight.w900)),
    subtitle: Text(subtitle, maxLines: 1, overflow: TextOverflow.ellipsis),
    trailing: const Icon(Icons.arrow_back_ios_new_rounded, size: 16),
    onTap: onTap,
  );
}

class _Notice extends StatelessWidget {
  const _Notice({required this.text, this.error = false});
  final String text;
  final bool error;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(11),
    decoration: BoxDecoration(
      color: error ? Colors.red.shade50 : const Color(0xFFEAF7F3),
      borderRadius: BorderRadius.circular(8),
      border: Border.all(
        color: error ? Colors.red.shade100 : const Color(0xFFCDECE4),
      ),
    ),
    child: Text(
      text,
      style: TextStyle(
        color: error ? Colors.red.shade800 : AppColors.primaryDark,
      ),
    ),
  );
}

class _ProfileMessage extends StatelessWidget {
  const _ProfileMessage({
    required this.icon,
    required this.title,
    required this.text,
    required this.action,
    required this.actionText,
  });
  final IconData icon;
  final String title;
  final String text;
  final VoidCallback action;
  final String actionText;

  @override
  Widget build(BuildContext context) => Center(
    child: Padding(
      padding: const EdgeInsets.all(24),
      child: Card(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(icon, size: 48, color: AppColors.primary),
              const SizedBox(height: 10),
              Text(
                title,
                style: const TextStyle(
                  fontSize: 19,
                  fontWeight: FontWeight.w900,
                ),
              ),
              const SizedBox(height: 6),
              Text(
                text,
                textAlign: TextAlign.center,
                style: const TextStyle(color: AppColors.muted),
              ),
              const SizedBox(height: 16),
              _FullWidthButton(
                child: FilledButton(onPressed: action, child: Text(actionText)),
              ),
            ],
          ),
        ),
      ),
    ),
  );
}
