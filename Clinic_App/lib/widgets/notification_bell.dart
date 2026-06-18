import 'dart:async';

import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../core/app_theme.dart';
import '../core/push_notification_service.dart';
import '../features/auth/auth_controller.dart';
import '../features/messages/message_hub_service.dart';
import '../features/notifications/notification_service.dart';

class NotificationBell extends StatefulWidget {
  const NotificationBell({super.key, required this.doctor});

  final bool doctor;

  @override
  State<NotificationBell> createState() => _NotificationBellState();
}

class _NotificationBellState extends State<NotificationBell> {
  late final NotificationService _service;
  StreamSubscription<ForegroundAppNotification>? _hubSub;
  StreamSubscription<ForegroundAppNotification>? _pushSub;
  int _unreadCount = 0;

  @override
  void initState() {
    super.initState();
    _service = NotificationService(context.read<AuthController>().api);
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
    final hub = context.read<MessageHubService>();
    _hubSub = hub.onAppNotification.listen((_) => _load());
    _pushSub = PushNotificationService.onForegroundAppNotification.listen(
      (_) => _load(),
    );
  }

  @override
  void dispose() {
    _hubSub?.cancel();
    _pushSub?.cancel();
    super.dispose();
  }

  Future<void> _load() async {
    final auth = context.read<AuthController>();
    if (!auth.isAuthenticated) return;
    try {
      final count = await _service.getUnreadCount(doctor: widget.doctor);
      if (mounted) setState(() => _unreadCount = count);
    } catch (_) {}
  }

  @override
  Widget build(BuildContext context) {
    final icon = IconButton.filledTonal(
      tooltip: 'الإشعارات',
      style: IconButton.styleFrom(
        backgroundColor: AppColors.primary.withValues(alpha: .10),
        foregroundColor: AppColors.primary,
      ),
      onPressed: () async {
        await context.push(widget.doctor ? '/doctor/notifications' : '/notifications');
        await _load();
      },
      icon: const Icon(Icons.notifications_rounded),
    );

    if (_unreadCount <= 0) return icon;

    return Badge(
      label: Text(_unreadCount > 99 ? '99+' : '$_unreadCount'),
      child: icon,
    );
  }
}
