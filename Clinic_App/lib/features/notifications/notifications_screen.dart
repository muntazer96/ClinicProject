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
  static const _pageSize = 20;

  late final NotificationService _service;
  final _scrollController = ScrollController();
  StreamSubscription<ForegroundAppNotification>? _hubSub;
  StreamSubscription<ForegroundAppNotification>? _pushSub;
  List<AppNotificationItem> _items = [];
  bool _loading = true;
  bool _loadingMore = false;
  bool _hasMore = true;
  int _page = 1;

  @override
  void initState() {
    super.initState();
    _service = NotificationService(context.read<AuthController>().api);
    _scrollController.addListener(_onScroll);
    _load();
    final hub = context.read<MessageHubService>();
    _hubSub = hub.onAppNotification.listen((_) => _load(silent: true));
    _pushSub = PushNotificationService.onForegroundAppNotification.listen(
      (_) => _load(silent: true),
    );
  }

  @override
  void dispose() {
    _scrollController.dispose();
    _hubSub?.cancel();
    _pushSub?.cancel();
    super.dispose();
  }

  Future<void> _load({bool silent = false}) async {
    if (!silent && mounted) setState(() => _loading = true);
    try {
      final items = await _service.getNotifications(
        doctor: widget.doctor,
        page: 1,
        pageSize: _pageSize,
      );
      if (mounted) {
        setState(() {
          _items = items;
          _page = 1;
          _hasMore = items.length == _pageSize;
        });
      }
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted && !silent) setState(() => _loading = false);
    }
  }

  Future<void> _loadMore() async {
    if (_loading || _loadingMore || !_hasMore) return;
    setState(() => _loadingMore = true);
    try {
      final nextPage = _page + 1;
      final items = await _service.getNotifications(
        doctor: widget.doctor,
        page: nextPage,
        pageSize: _pageSize,
      );
      if (mounted) {
        setState(() {
          _items = [..._items, ...items];
          _page = nextPage;
          _hasMore = items.length == _pageSize;
        });
      }
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loadingMore = false);
    }
  }

  void _onScroll() {
    if (!_scrollController.hasClients) return;
    final position = _scrollController.position;
    if (position.pixels >= position.maxScrollExtent - 220) {
      _loadMore();
    }
  }

  Future<void> _markRead(AppNotificationItem item) async {
    if (item.isRead) return;
    try {
      await _service.markRead(doctor: widget.doctor, id: item.id);
      if (!mounted) return;
      setState(() {
        _items = _items
            .map(
              (current) => current.id == item.id
                  ? AppNotificationItem(
                      id: current.id,
                      message: current.message,
                      createdAt: current.createdAt,
                      status: 1,
                      readAt: DateTime.now(),
                    )
                  : current,
            )
            .toList();
      });
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    }
  }

  Future<void> _markAllRead() async {
    try {
      await _service.markAllRead(doctor: widget.doctor);
      if (!mounted) return;
      setState(() {
        _items = _items
            .map(
              (item) => item.isRead
                  ? item
                  : AppNotificationItem(
                      id: item.id,
                      message: item.message,
                      createdAt: item.createdAt,
                      status: 1,
                      readAt: DateTime.now(),
                    ),
            )
            .toList();
      });
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    }
  }

  @override
  Widget build(BuildContext context) {
    final unreadCount = _items.where((item) => !item.isRead).length;
    final content = RefreshIndicator(
      onRefresh: _load,
      child: _loading
          ? const Center(child: CircularProgressIndicator())
          : _items.isEmpty
              ? const _EmptyNotifications()
              : ListView.builder(
                  controller: _scrollController,
                  padding: const EdgeInsets.fromLTRB(16, 12, 16, 28),
                  itemCount: _items.length + 2,
                  itemBuilder: (context, index) {
                    if (index == 0) {
                      return _NotificationsToolbar(
                        unreadCount: unreadCount,
                        onMarkAllRead: unreadCount == 0 ? null : _markAllRead,
                      );
                    }

                    if (index == _items.length + 1) {
                      if (_loadingMore) {
                        return const Padding(
                          padding: EdgeInsets.symmetric(vertical: 18),
                          child: Center(child: CircularProgressIndicator()),
                        );
                      }
                      if (!_hasMore) {
                        return const Padding(
                          padding: EdgeInsets.symmetric(vertical: 12),
                          child: Text(
                            'تم عرض كل الإشعارات',
                            textAlign: TextAlign.center,
                            style: TextStyle(
                              color: AppColors.muted,
                              fontWeight: FontWeight.w700,
                            ),
                          ),
                        );
                      }
                      return const SizedBox(height: 18);
                    }

                    final item = _items[index - 1];
                    return _NotificationCard(
                      item: item,
                      onTap: () => _markRead(item),
                    );
                  },
                ),
    );

    if (widget.doctor) {
      return DoctorScaffold(title: 'الإشعارات', child: content);
    }

    return AppScaffold(title: 'الإشعارات', showBackButton: true, child: content);
  }
}

class _NotificationsToolbar extends StatelessWidget {
  const _NotificationsToolbar({
    required this.unreadCount,
    required this.onMarkAllRead,
  });

  final int unreadCount;
  final VoidCallback? onMarkAllRead;

  @override
  Widget build(BuildContext context) => Padding(
        padding: const EdgeInsets.only(bottom: 12),
        child: Row(
          children: [
            Expanded(
              child: Text(
                unreadCount == 0
                    ? 'كل الإشعارات مقروءة'
                    : '$unreadCount إشعار غير مقروء',
                style: const TextStyle(
                  color: AppColors.text,
                  fontWeight: FontWeight.w900,
                ),
              ),
            ),
            TextButton.icon(
              onPressed: onMarkAllRead,
              icon: const Icon(Icons.done_all_rounded, size: 18),
              label: const Text('قراءة الكل'),
            ),
          ],
        ),
      );
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
