import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../core/app_theme.dart';
import '../../../core/push_notification_service.dart';
import '../../auth/auth_controller.dart';
import '../message_hub_service.dart';
import '../models/message_models.dart';

class InAppMessageNotificationListener extends StatefulWidget {
  const InAppMessageNotificationListener({
    super.key,
    required this.child,
    required this.onOpenConversation,
    required this.onOpenBookingNotification,
  });

  final Widget child;
  final void Function(String senderId, String senderName) onOpenConversation;
  final VoidCallback onOpenBookingNotification;

  @override
  State<InAppMessageNotificationListener> createState() =>
      _InAppMessageNotificationListenerState();
}

class _InAppMessageNotificationListenerState
    extends State<InAppMessageNotificationListener> {
  StreamSubscription<MessageDto>? _messageSub;
  StreamSubscription<ForegroundMessageNotification>? _messagePushSub;
  StreamSubscription<ForegroundAppNotification>? _appPushSub;
  StreamSubscription<ForegroundAppNotification>? _appHubSub;
  Timer? _dismissTimer;
  _ActiveInAppNotification? _notification;
  bool _listening = false;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_listening) return;

    _listening = true;
    final hub = context.read<MessageHubService>();
    _messageSub = hub.onMessage.listen(_showFromSignalR);
    _appHubSub = hub.onAppNotification.listen(_showFromAppPush);
    _messagePushSub =
        PushNotificationService.onForegroundMessageNotification.listen(
      _showFromMessagePush,
    );
    _appPushSub = PushNotificationService.onForegroundAppNotification.listen(
      _showFromAppPush,
    );
  }

  @override
  void dispose() {
    _messageSub?.cancel();
    _messagePushSub?.cancel();
    _appPushSub?.cancel();
    _appHubSub?.cancel();
    _dismissTimer?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final notification = _notification;

    return Stack(
      fit: StackFit.expand,
      children: [
        widget.child,
        if (notification != null)
          _InAppNotificationToast(
            title: notification.title,
            body: notification.body,
            icon: notification.icon,
            onClose: _dismiss,
            onTap: () {
              final action = notification.onTap;
              _dismiss();
              action();
            },
          ),
      ],
    );
  }

  void _showFromSignalR(MessageDto message) {
    _showMessage(
      senderId: message.senderId,
      senderName: message.senderName,
      body: message.content,
      hasImage: message.imageName?.isNotEmpty == true,
    );
  }

  void _showFromMessagePush(ForegroundMessageNotification notification) {
    _showMessage(
      senderId: notification.senderId,
      senderName: notification.senderName,
      body: notification.body,
      hasImage: notification.hasImage,
    );
  }

  void _showFromAppPush(ForegroundAppNotification notification) {
    if (notification.type != 'booking') return;

    _show(
      title: notification.title.trim().isNotEmpty
          ? notification.title.trim()
          : 'إشعار حجز',
      body: notification.body.trim().isNotEmpty
          ? notification.body.trim()
          : 'يوجد تحديث جديد على الحجز.',
      icon: Icons.calendar_month_rounded,
      onTap: widget.onOpenBookingNotification,
    );
  }

  void _showMessage({
    required String senderId,
    required String senderName,
    required String body,
    required bool hasImage,
  }) {
    if (!mounted || senderId.isEmpty) return;

    final auth = context.read<AuthController>();
    if (!auth.isAuthenticated || senderId == auth.profile?.id) return;

    final displayName =
        senderName.trim().isNotEmpty ? senderName.trim() : 'مستخدم';
    final preview = body.trim().isNotEmpty
        ? body.trim()
        : hasImage
            ? 'أرسل صورة'
            : 'رسالة جديدة';

    _show(
      title: displayName,
      body: preview,
      icon: Icons.chat_bubble_rounded,
      onTap: () => widget.onOpenConversation(senderId, displayName),
    );
  }

  void _show({
    required String title,
    required String body,
    required IconData icon,
    required VoidCallback onTap,
  }) {
    if (!mounted) return;

    _dismissTimer?.cancel();
    setState(() {
      _notification = _ActiveInAppNotification(
        title: title,
        body: body,
        icon: icon,
        onTap: onTap,
      );
    });

    _dismissTimer = Timer(const Duration(seconds: 5), _dismiss);
  }

  void _dismiss() {
    _dismissTimer?.cancel();
    _dismissTimer = null;
    if (mounted && _notification != null) {
      setState(() => _notification = null);
    }
  }
}

class _ActiveInAppNotification {
  const _ActiveInAppNotification({
    required this.title,
    required this.body,
    required this.icon,
    required this.onTap,
  });

  final String title;
  final String body;
  final IconData icon;
  final VoidCallback onTap;
}

class _InAppNotificationToast extends StatelessWidget {
  const _InAppNotificationToast({
    required this.title,
    required this.body,
    required this.icon,
    required this.onTap,
    required this.onClose,
  });

  final String title;
  final String body;
  final IconData icon;
  final VoidCallback onTap;
  final VoidCallback onClose;

  @override
  Widget build(BuildContext context) {
    final topPadding = MediaQuery.of(context).padding.top + 12;

    return PositionedDirectional(
      top: topPadding,
      start: 16,
      end: 16,
      child: SafeArea(
        bottom: false,
        child: Align(
          alignment: Alignment.topCenter,
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 560),
            child: TweenAnimationBuilder<double>(
              tween: Tween(begin: 0, end: 1),
              duration: const Duration(milliseconds: 220),
              curve: Curves.easeOutCubic,
              builder: (context, value, child) {
                return Opacity(
                  opacity: value,
                  child: Transform.translate(
                    offset: Offset(0, -12 * (1 - value)),
                    child: child,
                  ),
                );
              },
              child: Directionality(
                textDirection: TextDirection.rtl,
                child: Material(
                  color: Colors.transparent,
                  child: InkWell(
                    borderRadius: BorderRadius.circular(8),
                    onTap: onTap,
                    child: Container(
                      width: double.infinity,
                      padding: const EdgeInsets.symmetric(
                        horizontal: 14,
                        vertical: 12,
                      ),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(8),
                        border: Border.all(color: AppColors.border),
                        boxShadow: [
                          BoxShadow(
                            color: Colors.black.withOpacity(0.16),
                            blurRadius: 22,
                            offset: const Offset(0, 10),
                          ),
                        ],
                      ),
                      child: Row(
                        children: [
                          Container(
                            width: 42,
                            height: 42,
                            decoration: BoxDecoration(
                              color: AppColors.primary.withOpacity(0.1),
                              borderRadius: BorderRadius.circular(8),
                            ),
                            child: Icon(
                              icon,
                              color: AppColors.primary,
                              size: 22,
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Column(
                              mainAxisSize: MainAxisSize.min,
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  title,
                                  maxLines: 1,
                                  overflow: TextOverflow.ellipsis,
                                  style: const TextStyle(
                                    fontSize: 14,
                                    fontWeight: FontWeight.w900,
                                    color: AppColors.text,
                                  ),
                                ),
                                const SizedBox(height: 3),
                                Text(
                                  body,
                                  maxLines: 2,
                                  overflow: TextOverflow.ellipsis,
                                  style: const TextStyle(
                                    fontSize: 13,
                                    height: 1.35,
                                    color: AppColors.muted,
                                  ),
                                ),
                              ],
                            ),
                          ),
                          IconButton(
                            onPressed: onClose,
                            icon: const Icon(Icons.close_rounded),
                            color: AppColors.muted,
                            visualDensity: VisualDensity.compact,
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
