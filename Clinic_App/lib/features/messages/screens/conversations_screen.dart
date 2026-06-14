import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import 'dart:async';
import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
import '../../../core/push_notification_service.dart';
import '../../auth/auth_controller.dart';
import '../message_hub_service.dart';
import '../message_service.dart';
import '../models/message_models.dart';

class ConversationsScreen extends StatefulWidget {
  const ConversationsScreen({super.key});

  @override
  State<ConversationsScreen> createState() => _ConversationsScreenState();
}

class _ConversationsScreenState extends State<ConversationsScreen>
    with WidgetsBindingObserver {
  late final MessageService _service;
  late final MessageHubService _hub;
  StreamSubscription<MessageDto>? _messageSub;
  StreamSubscription<String>? _pushSub;
  List<ConversationDto> _items = [];
  bool _loading = true;
  String? _myUserId;
  int _lastVersion = 0;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    final auth = context.read<AuthController>();
    _service = MessageService(auth.api);
    _myUserId = auth.profile?.id;
    _hub = context.read<MessageHubService>();
    _lastVersion = _hub.conversationsVersion;
    _messageSub = _hub.onMessage.listen(_onNewMessage);
    _pushSub = PushNotificationService.onNewMessageNotification.listen((_) {
      _loadSilent();
      _hub.refreshUnreadCount();
    });
    _load();
  }

  void _onNewMessage(MessageDto msg) {
    try {
      _loadSilent();
      if (msg.senderId != _myUserId) {
        PushNotificationService.showLocalNotification(
          title: msg.senderName,
          body: msg.content,
        );
      }
    } catch (_) {}
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _messageSub?.cancel();
    _pushSub?.cancel();
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      _loadSilent();
    }
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      _items = await _service.getConversations();
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _loadSilent() async {
    try {
      _items = await _service.getConversations();
      if (mounted) setState(() {});
    } catch (_) {}
  }

  @override
  Widget build(BuildContext context) {
    final version = context.watch<MessageHubService>().conversationsVersion;
    if (version != _lastVersion) {
      _lastVersion = version;
      WidgetsBinding.instance.addPostFrameCallback((_) => _loadSilent());
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: _loading
          ? const Center(child: CircularProgressIndicator())
          : _items.isEmpty
              ? ListView(
                  children: const [
                    SizedBox(height: 120),
                    Center(
                      child: Column(
                        children: [
                          Icon(
                            Icons.chat_bubble_outline_rounded,
                            size: 64,
                            color: AppColors.muted,
                          ),
                          SizedBox(height: 16),
                          Text(
                            'لا توجد رسائل حالياً.',
                            style: TextStyle(
                              color: AppColors.muted,
                              fontSize: 16,
                            ),
                          ),
                          SizedBox(height: 8),
                          Text(
                            'يمكنك مراسلة الأطباء من صفحة ملفهم.',
                            style: TextStyle(
                              color: AppColors.muted,
                              fontSize: 13,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                )
              : ListView.builder(
                  padding: const EdgeInsets.fromLTRB(16, 12, 16, 28),
                  itemCount: _items.length,
                  itemBuilder: (context, index) =>
                      _ConversationCard(item: _items[index]),
                ),
    );
  }
}

class _ConversationCard extends StatelessWidget {
  const _ConversationCard({required this.item});

  final ConversationDto item;

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthController>();
    final isDoctor = auth.isDoctor;
    final time =
        DateFormat('MM/dd').format(item.lastMessageAt);

    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: Material(
        color: Colors.white,
        borderRadius: BorderRadius.circular(8),
        child: InkWell(
          borderRadius: BorderRadius.circular(8),
          onTap: () => context.push(
            isDoctor
                ? '/doctor/messages/${item.otherUserId}'
                : '/messages/${item.otherUserId}',
            extra: item.otherUserName,
          ),
          child: Container(
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(8),
              border: Border.all(
                color: item.unreadCount > 0
                    ? AppColors.primary.withOpacity(.26)
                    : const Color(0xFFE3ECEA),
              ),
            ),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                CircleAvatar(
                  radius: 22,
                  backgroundColor: AppColors.softAmber,
                  child: Text(
                    item.otherUserName.isNotEmpty
                        ? item.otherUserName[0]
                        : '?',
                    style: const TextStyle(
                      color: AppColors.primaryDark,
                      fontWeight: FontWeight.w900,
                    ),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            item.otherUserName,
                            style: TextStyle(
                              fontWeight: item.unreadCount > 0
                                  ? FontWeight.w900
                                  : FontWeight.w700,
                              color: AppColors.text,
                            ),
                          ),
                          Text(
                            time,
                            style: const TextStyle(
                              color: AppColors.muted,
                              fontSize: 12,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 6),
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              item.lastMessage,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: TextStyle(
                                color: item.unreadCount > 0
                                    ? AppColors.text
                                    : AppColors.muted,
                                fontSize: 13,
                                fontWeight: item.unreadCount > 0
                                    ? FontWeight.w700
                                    : FontWeight.w500,
                              ),
                            ),
                          ),
                          if (item.unreadCount > 0) ...[
                            const SizedBox(width: 8),
                            Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 8,
                                vertical: 2,
                              ),
                              decoration: BoxDecoration(
                                color: AppColors.primary,
                                borderRadius: BorderRadius.circular(12),
                              ),
                              child: Text(
                                '${item.unreadCount}',
                                style: const TextStyle(
                                  color: Colors.white,
                                  fontSize: 11,
                                  fontWeight: FontWeight.w900,
                                ),
                              ),
                            ),
                          ],
                        ],
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
