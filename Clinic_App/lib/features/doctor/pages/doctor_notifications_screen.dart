import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorNotificationsScreen extends StatefulWidget {
  const DoctorNotificationsScreen({super.key});

  @override
  State<DoctorNotificationsScreen> createState() =>
      _DoctorNotificationsScreenState();
}

class _DoctorNotificationsScreenState extends State<DoctorNotificationsScreen> {
  late final DoctorService _service;
  List<DoctorNotificationItem> _items = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      _items = await _service.getNotifications();
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _markRead(DoctorNotificationItem item) async {
    if (item.isRead) return;
    try {
      await _service.markNotificationRead(item.id);
      await _load();
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    }
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
        title: 'الإشعارات',
        child: RefreshIndicator(
          onRefresh: _load,
          child: _loading
              ? const Center(child: CircularProgressIndicator())
              : _items.isEmpty
                  ? const DoctorEmptyState(
                      icon: Icons.notifications_none_rounded,
                      message: 'لا توجد إشعارات حالياً.',
                    )
                  : ListView.builder(
                      padding: const EdgeInsets.fromLTRB(16, 12, 16, 28),
                      itemCount: _items.length,
                      itemBuilder: (context, index) => _NotificationCard(
                        item: _items[index],
                        onTap: () => _markRead(_items[index]),
                      ),
                    ),
        ),
      );
}

class _NotificationCard extends StatelessWidget {
  const _NotificationCard({required this.item, required this.onTap});

  final DoctorNotificationItem item;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final color = item.isRead ? AppColors.muted : AppColors.primary;

    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: Material(
        color: Colors.white,
        borderRadius: BorderRadius.circular(8),
        child: InkWell(
          borderRadius: BorderRadius.circular(8),
          onTap: onTap,
          child: Container(
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(8),
              border: Border.all(
                color: item.isRead
                    ? const Color(0xFFE3ECEA)
                    : AppColors.primary.withOpacity(.26),
              ),
            ),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Icon(
                  item.isRead
                      ? Icons.notifications_none_rounded
                      : Icons.notifications_active_rounded,
                  color: color,
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        item.message,
                        style: TextStyle(
                          fontWeight:
                              item.isRead ? FontWeight.w700 : FontWeight.w900,
                          color: AppColors.text,
                          height: 1.45,
                        ),
                      ),
                      const SizedBox(height: 6),
                      Text(
                        DateFormat('yyyy/MM/dd - hh:mm a')
                            .format(item.createdAt),
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
