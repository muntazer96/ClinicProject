import 'dart:async';

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import '../../core/app_snack_bar.dart';
import '../../core/app_theme.dart';
import '../../core/push_notification_service.dart';
import '../../widgets/app_scaffold.dart';
import '../auth/auth_controller.dart';
import '../doctor/widgets/doctor_scaffold.dart';
import '../messages/message_hub_service.dart';
import 'notification_service.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({super.key, required this.doctor});

  final bool doctor;

  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  late final NotificationService _service;
  StreamSubscription<ForegroundAppNotification>? _hubSub;
  StreamSubscription<ForegroundAppNotification>? _pushSub;
  List<AppNotificationItem> _items = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _service = NotificationService(context.read<AuthController>().api);
    _load();
    final hub = context.read<MessageHubService>();
    _hubSub = hub.onAppNotification.listen((_) => _load(silent: true));
    _pushSub = PushNotificationService.onForegroundAppNotification.listen(
      (_) => _load(silent: true),
    );
  }

  @override
  void dispose() {
    _hubSub?.cancel();
    _pushSub?.cancel();
    super.dispose();
  }

  Future<void> _load({bool silent = false}) async {
    if (!silent && mounted) setState(() => _loading = true);
    try {
      final items = await _service.getNotifications(doctor: widget.doctor);
      if (mounted) setState(() => _items = items);
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted && !silent) setState(() => _loading = false);
    }
  }

  Future<void> _markRead(AppNotificationItem item) async {
    if (item.isRead) return;
    try {
      await _service.markRead(doctor: widget.doctor, id: item.id);
      await _load();
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    }
  }

  @override
  Widget build(BuildContext context) {
    final content = RefreshIndicator(
      onRefresh: _load,
      child: _loading
          ? const Center(child: CircularProgressIndicator())
          : _items.isEmpty
              ? const _EmptyNotifications()
              : ListView.builder(
                  padding: const EdgeInsets.fromLTRB(16, 12, 16, 28),
                  itemCount: _items.length,
                  itemBuilder: (context, index) => _NotificationCard(
                    item: _items[index],
                    onTap: () => _markRead(_items[index]),
                  ),
                ),
    );

    if (widget.doctor) {
      return DoctorScaffold(title: 'الإشعارات', child: content);
    }

    return AppScaffold(title: 'الإشعارات', showBackButton: true, child: content);
  }
}

class _EmptyNotifications extends StatelessWidget {
  const _EmptyNotifications();

  @override
  Widget build(BuildContext context) => ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        children: const [
          SizedBox(height: 110),
          Icon(Icons.notifications_none_rounded, size: 54, color: AppColors.muted),
          SizedBox(height: 12),
          Text(
            'لا توجد إشعارات حالياً.',
            textAlign: TextAlign.center,
            style: TextStyle(color: AppColors.muted, fontWeight: FontWeight.w800),
          ),
        ],
      );
}

class _NotificationCard extends StatelessWidget {
  const _NotificationCard({required this.item, required this.onTap});

  final AppNotificationItem item;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final color = item.isRead ? AppColors.muted : AppColors.primary;

    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: Material(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        child: InkWell(
          borderRadius: BorderRadius.circular(14),
          onTap: onTap,
          child: Container(
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(14),
              border: Border.all(
                color: item.isRead
                    ? const Color(0xFFE3ECEA)
                    : AppColors.primary.withValues(alpha: .28),
              ),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withValues(alpha: .025),
                  blurRadius: 12,
                  offset: const Offset(0, 6),
                ),
              ],
            ),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Container(
                  width: 42,
                  height: 42,
                  decoration: BoxDecoration(
                    color: color.withValues(alpha: .10),
                    borderRadius: BorderRadius.circular(13),
                  ),
                  child: Icon(
                    item.isRead
                        ? Icons.notifications_none_rounded
                        : Icons.notifications_active_rounded,
                    color: color,
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        item.message,
                        style: TextStyle(
                          fontWeight: item.isRead ? FontWeight.w700 : FontWeight.w900,
                          color: AppColors.text,
                          height: 1.45,
                        ),
                      ),
                      const SizedBox(height: 6),
                      Text(
                        DateFormat('yyyy/MM/dd - hh:mm a').format(item.createdAt),
                        style: const TextStyle(
                          color: AppColors.muted,
                          fontSize: 12,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
