import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/app_theme.dart';
import '../../../widgets/app_logo.dart';
import '../../../widgets/notification_bell.dart';
import '../../auth/auth_controller.dart';
import '../../messages/message_hub_service.dart';
import '../services/doctor_service.dart';

class DoctorScaffold extends StatefulWidget {
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
  State<DoctorScaffold> createState() => _DoctorScaffoldState();
}

class _DoctorScaffoldState extends State<DoctorScaffold> {
  String? _doctorName;
  bool? _canMessage;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadDoctorProfile());
  }

  Future<void> _loadDoctorProfile() async {
    final auth = context.read<AuthController>();
    if (!auth.isDoctor) return;
    try {
      final service = DoctorService(auth.api);
      final profile = await service.getProfile();
      if (mounted) {
        setState(() {
          _doctorName = profile.name;
          _canMessage = profile.canMessage;
        });
      }
    } catch (_) {}
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthController>();
    final path = GoRouterState.of(context).uri.path;
    final selectedIndex =
        path == '/doctor/appointments' ||
            path.startsWith('/doctor/appointments/')
        ? 1
        : path == '/doctor/clinics' ||
              path.startsWith('/doctor/clinics/') ||
              path == '/doctor/schedule' ||
              path.startsWith('/doctor/schedule/')
        ? 2
        : path == '/doctor/offers' || path.startsWith('/doctor/offers/')
        ? 3
        : path == '/doctor/profile' ||
              path.startsWith('/doctor/profile/') ||
              path == '/doctor/features' ||
              path == '/doctor/subscription' ||
              path == '/doctor/reviews'
        ? 4
        : 0;
    final isMessagesPage =
        path == '/doctor/messages' || path.startsWith('/doctor/messages/');
    final hub = context.watch<MessageHubService>();
    final unreadCount = hub.unreadCount;
    final displayName = _doctorName?.isNotEmpty == true
        ? _doctorName!
        : auth.displayName;
    const premiumColor = AppColors.primary;

    return Scaffold(
      floatingActionButton: isMessagesPage || _canMessage != true
          ? null
          : _MessageFab(unreadCount: unreadCount),
      floatingActionButtonLocation: FloatingActionButtonLocation.endFloat,
      appBar: AppBar(
        foregroundColor: premiumColor,
        toolbarHeight: 76,
        titleSpacing: 16,
        leading: widget.showBackButton
            ? IconButton(
                tooltip: 'رجوع',
                onPressed: () {
                  if (context.canPop()) {
                    context.pop();
                  } else {
                    context.go(widget.backRoute);
                  }
                },
                icon: const Icon(Icons.arrow_back_rounded),
              )
            : null,
        title: Row(
          children: [
            const AppLogo(size: 44),
            const SizedBox(width: 10),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    widget.title,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      fontSize: 17,
                      fontWeight: FontWeight.w900,
                    ),
                  ),
                  Text(
                    displayName,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      fontSize: 11,
                      color: AppColors.muted,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
        actions: [
          const NotificationBell(doctor: true),
          const SizedBox(width: 4),
        ],
      ),
      body: widget.child,
      bottomNavigationBar: DecoratedBox(
        decoration: const BoxDecoration(
          color: Colors.white,
          boxShadow: [
            BoxShadow(
              color: Color(0x121D4A44),
              blurRadius: 22,
              offset: Offset(0, -7),
            ),
          ],
        ),
        child: NavigationBar(
          indicatorColor: premiumColor.withValues(alpha: .14),
          selectedIndex: selectedIndex,
          onDestinationSelected: (index) {
            if (index == 0) context.go('/doctor');
            if (index == 1) context.go('/doctor/appointments');
            if (index == 2) context.go('/doctor/clinics');
            if (index == 3) context.go('/doctor/offers');
            if (index == 4) context.go('/doctor/profile');
          },
          destinations: [
            const NavigationDestination(
              icon: Icon(Icons.home_outlined),
              selectedIcon: Icon(Icons.home_rounded),
              label: 'الرئيسية',
            ),
            const NavigationDestination(
              icon: Icon(Icons.calendar_month_outlined),
              selectedIcon: Icon(Icons.calendar_month_rounded),
              label: 'الحجوزات',
            ),
            const NavigationDestination(
              icon: Icon(Icons.local_hospital_outlined),
              selectedIcon: Icon(Icons.local_hospital_rounded),
              label: 'عياداتي',
            ),
            const NavigationDestination(
              icon: Icon(Icons.local_offer_outlined),
              selectedIcon: Icon(Icons.local_offer_rounded),
              label: 'العروض',
            ),
            const NavigationDestination(
              icon: Icon(Icons.person_outline_rounded),
              selectedIcon: Icon(Icons.person_rounded),
              label: 'الملف',
            ),
          ],
        ),
      ),
    );
  }
}

class _MessageFab extends StatefulWidget {
  const _MessageFab({required this.unreadCount});

  final int unreadCount;

  @override
  State<_MessageFab> createState() => _MessageFabState();
}

class _MessageFabState extends State<_MessageFab>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller;
  late final Animation<double> _scale;

  int _lastCount = 0;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 400),
    );
    _scale = Tween<double>(begin: 1, end: 1.15).animate(
      CurvedAnimation(parent: _controller, curve: Curves.easeInOut),
    );
  }

  @override
  void didUpdateWidget(_MessageFab oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.unreadCount > 0 && widget.unreadCount != _lastCount) {
      _lastCount = widget.unreadCount;
      _controller.forward().then((_) => _controller.reverse());
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _scale,
      builder: (context, child) => Transform.scale(
        scale: _scale.value,
        child: child,
      ),
      child: FloatingActionButton(
        heroTag: 'message_fab',
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        elevation: 4,
        onPressed: () => context.push('/doctor/messages'),
        tooltip: 'الرسائل',
        child: Badge(
          isLabelVisible: widget.unreadCount > 0,
          label: widget.unreadCount > 0
              ? Text(
                  widget.unreadCount > 99
                      ? '99+'
                      : '${widget.unreadCount}',
                  style: const TextStyle(fontSize: 10),
                )
              : null,
          child: const Icon(Icons.chat_rounded),
        ),
      ),
    );
  }
}

class DoctorSectionCard extends StatelessWidget {
  const DoctorSectionCard({super.key, required this.child, this.padding});

  final Widget child;
  final EdgeInsetsGeometry? padding;

  @override
  Widget build(BuildContext context) => Card(
    child: Padding(padding: padding ?? const EdgeInsets.all(14), child: child),
  );
}

class DoctorPage extends StatelessWidget {
  const DoctorPage({super.key, required this.children, this.padding});

  final List<Widget> children;
  final EdgeInsetsGeometry? padding;

  @override
  Widget build(BuildContext context) => SingleChildScrollView(
    physics: const AlwaysScrollableScrollPhysics(),
    padding: padding ?? const EdgeInsets.fromLTRB(16, 14, 16, 28),
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
    style: danger
        ? OutlinedButton.styleFrom(
            foregroundColor: AppColors.danger,
            side: const BorderSide(color: AppColors.softCoral),
          )
        : null,
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
      borderRadius: BorderRadius.circular(8),
    ),
    child: Text(
      label,
      style: TextStyle(color: color, fontSize: 11, fontWeight: FontWeight.w900),
    ),
  );
}

class DoctorEmptyState extends StatelessWidget {
  const DoctorEmptyState({
    super.key,
    required this.icon,
    required this.message,
  });

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
