import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../core/app_theme.dart';

class DoctorScaffold extends StatelessWidget {
  const DoctorScaffold({
    super.key,
    required this.title,
    required this.child,
    this.showBackButton = false,
    this.backRoute = '/doctor',
  });

  final String title;
  final Widget child;
  final bool showBackButton;
  final String backRoute;

  @override
  Widget build(BuildContext context) {
    final path = GoRouterState.of(context).uri.path;
    final selectedIndex = path == '/doctor/appointments'
        ? 1
        : path == '/doctor/clinics' || path.startsWith('/doctor/clinics/')
        ? 2
        : path == '/doctor/profile'
        ? 3
        : 0;

    return Scaffold(
      backgroundColor: AppColors.surface,
      appBar: AppBar(
        toolbarHeight: 64,
        leadingWidth: 58,
        titleSpacing: 0,
        centerTitle: true,
        leading: Padding(
          padding: const EdgeInsetsDirectional.only(start: 14),
          child: showBackButton
              ? _RoundIconButton(
                  tooltip: 'رجوع',
                  icon: Icons.arrow_back_rounded,
                  onPressed: () {
                    if (context.canPop()) {
                      context.pop();
                    } else {
                      context.go(backRoute);
                    }
                  },
                )
              : _RoundIconButton(
                  tooltip: 'الإشعارات',
                  icon: Icons.notifications_none_rounded,
                  onPressed: () => context.go('/doctor/notifications'),
                ),
        ),
        title: Text(
          title,
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
          textAlign: TextAlign.center,
          style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w900),
        ),
        actions: [
          Padding(
            padding: const EdgeInsetsDirectional.only(end: 14),
            child: showBackButton
                ? _RoundIconButton(
                    tooltip: 'الإشعارات',
                    icon: Icons.notifications_none_rounded,
                    onPressed: () => context.go('/doctor/notifications'),
                  )
                : Container(
                    width: 34,
                    height: 34,
                    padding: const EdgeInsets.all(6),
                    decoration: BoxDecoration(
                      gradient: const LinearGradient(
                        colors: [AppColors.primaryDark, AppColors.primary],
                      ),
                      borderRadius: BorderRadius.circular(9),
                      boxShadow: const [
                        BoxShadow(
                          color: Color(0x24155E75),
                          blurRadius: 14,
                          offset: Offset(0, 7),
                        ),
                      ],
                    ),
                    child: Image.asset('assets/app_logo.png'),
                  ),
          ),
        ],
      ),
      body: SafeArea(top: false, child: child),
      bottomNavigationBar: SafeArea(
        top: false,
        child: Container(
          margin: const EdgeInsets.fromLTRB(12, 0, 12, 10),
          padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 6),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(18),
            border: Border.all(color: AppColors.border),
            boxShadow: const [
              BoxShadow(
                color: Color(0x14155E75),
                blurRadius: 24,
                offset: Offset(0, 10),
              ),
            ],
          ),
          child: NavigationBar(
            height: 62,
            elevation: 0,
            backgroundColor: Colors.transparent,
            selectedIndex: selectedIndex,
            onDestinationSelected: (index) {
              if (index == 0) context.go('/doctor');
              if (index == 1) context.go('/doctor/appointments');
              if (index == 2) context.go('/doctor/clinics');
              if (index == 3) context.go('/doctor/profile');
            },
            destinations: const [
              NavigationDestination(
                icon: Icon(Icons.grid_view_outlined),
                selectedIcon: Icon(Icons.grid_view_rounded),
                label: 'الرئيسية',
              ),
              NavigationDestination(
                icon: Icon(Icons.event_note_outlined),
                selectedIcon: Icon(Icons.event_note_rounded),
                label: 'الحجوزات',
              ),
              NavigationDestination(
                icon: Icon(Icons.add_box_outlined),
                selectedIcon: Icon(Icons.add_box_rounded),
                label: 'عياداتي',
              ),
              NavigationDestination(
                icon: Icon(Icons.person_outline_rounded),
                selectedIcon: Icon(Icons.person_rounded),
                label: 'الملف',
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _RoundIconButton extends StatelessWidget {
  const _RoundIconButton({
    required this.tooltip,
    required this.icon,
    required this.onPressed,
  });

  final String tooltip;
  final IconData icon;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) => IconButton(
    tooltip: tooltip,
    onPressed: onPressed,
    icon: Icon(icon, size: 21),
    style: IconButton.styleFrom(
      foregroundColor: AppColors.text,
      backgroundColor: Colors.white,
      side: const BorderSide(color: AppColors.border),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
    ),
  );
}

class DoctorSectionCard extends StatelessWidget {
  const DoctorSectionCard({super.key, required this.child, this.padding});

  final Widget child;
  final EdgeInsetsGeometry? padding;

  @override
  Widget build(BuildContext context) => Container(
    width: double.infinity,
    padding: padding ?? const EdgeInsets.all(14),
    decoration: BoxDecoration(
      color: Colors.white,
      borderRadius: BorderRadius.circular(14),
      border: Border.all(color: AppColors.border),
      boxShadow: const [
        BoxShadow(
          color: Color(0x0F155E75),
          blurRadius: 18,
          offset: Offset(0, 8),
        ),
      ],
    ),
    child: child,
  );
}

class DoctorPage extends StatelessWidget {
  const DoctorPage({super.key, required this.children, this.padding});

  final List<Widget> children;
  final EdgeInsetsGeometry? padding;

  @override
  Widget build(BuildContext context) => SingleChildScrollView(
    physics: const AlwaysScrollableScrollPhysics(),
    padding: padding ?? const EdgeInsets.fromLTRB(16, 12, 16, 28),
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: children,
    ),
  );
}

class DoctorPrimaryButton extends StatelessWidget {
  const DoctorPrimaryButton({
    super.key,
    required this.label,
    required this.onPressed,
    this.icon,
  });

  final String label;
  final VoidCallback? onPressed;
  final IconData? icon;

  @override
  Widget build(BuildContext context) => FilledButton.icon(
    onPressed: onPressed,
    icon: Icon(icon ?? Icons.add_rounded, size: 19),
    label: Text(label),
    style: FilledButton.styleFrom(
      minimumSize: const Size.fromHeight(48),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(9)),
      backgroundColor: AppColors.primary,
      foregroundColor: Colors.white,
    ),
  );
}

class DoctorActionButton extends StatelessWidget {
  const DoctorActionButton({
    super.key,
    required this.label,
    required this.icon,
    required this.onPressed,
    this.danger = false,
  });

  final String label;
  final IconData icon;
  final VoidCallback? onPressed;
  final bool danger;

  @override
  Widget build(BuildContext context) => OutlinedButton.icon(
    onPressed: onPressed,
    icon: Icon(icon, size: 17),
    label: Text(label),
    style: OutlinedButton.styleFrom(
      foregroundColor: danger ? AppColors.danger : AppColors.primary,
      side: BorderSide(
        color: danger ? AppColors.softCoral : AppColors.border,
      ),
      backgroundColor: danger ? const Color(0xFFFFFBFB) : Colors.white,
      minimumSize: const Size(0, 42),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
    ),
  );
}

class DoctorInfoRow extends StatelessWidget {
  const DoctorInfoRow({
    super.key,
    required this.icon,
    required this.text,
    this.color = AppColors.primary,
  });

  final IconData icon;
  final String text;
  final Color color;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(top: 6),
    child: Row(
      children: [
        Icon(icon, size: 16, color: color),
        const SizedBox(width: 7),
        Expanded(
          child: Text(
            text,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(color: AppColors.muted, fontSize: 12),
          ),
        ),
      ],
    ),
  );
}

class DoctorSwitchTile extends StatelessWidget {
  const DoctorSwitchTile({
    super.key,
    required this.title,
    required this.value,
    required this.onChanged,
    this.subtitle,
  });

  final String title;
  final String? subtitle;
  final bool value;
  final ValueChanged<bool> onChanged;

  @override
  Widget build(BuildContext context) => DoctorSectionCard(
    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
    child: SwitchListTile(
      contentPadding: EdgeInsets.zero,
      value: value,
      activeColor: Colors.white,
      activeTrackColor: AppColors.primary,
      inactiveThumbColor: Colors.white,
      inactiveTrackColor: AppColors.border,
      onChanged: onChanged,
      title: Text(title, style: const TextStyle(fontWeight: FontWeight.w800)),
      subtitle: subtitle == null
          ? null
          : Text(subtitle!, style: const TextStyle(color: AppColors.muted)),
    ),
  );
}

class DoctorStatusPill extends StatelessWidget {
  const DoctorStatusPill({
    super.key,
    required this.label,
    this.color = AppColors.primary,
  });

  final String label;
  final Color color;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 4),
    decoration: BoxDecoration(
      color: color.withValues(alpha: .10),
      borderRadius: BorderRadius.circular(7),
    ),
    child: Text(
      label,
      style: TextStyle(color: color, fontSize: 11, fontWeight: FontWeight.w900),
    ),
  );
}

class DoctorEmptyState extends StatelessWidget {
  const DoctorEmptyState({super.key, required this.icon, required this.message});

  final IconData icon;
  final String message;

  @override
  Widget build(BuildContext context) => Center(
    child: Padding(
      padding: const EdgeInsets.all(28),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 48, color: AppColors.muted),
          const SizedBox(height: 10),
          Text(
            message,
            textAlign: TextAlign.center,
            style: const TextStyle(
              color: AppColors.muted,
              fontWeight: FontWeight.w700,
            ),
          ),
        ],
      ),
    ),
  );
}
