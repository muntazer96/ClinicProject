import 'dart:async';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../message_hub_service.dart';
import '../message_service.dart';
import '../models/message_models.dart';

class ChatScreen extends StatefulWidget {
  const ChatScreen({
    super.key,
    required this.otherUserId,
    required this.otherUserName,
  });

  final String otherUserId;
  final String otherUserName;

  @override
  State<ChatScreen> createState() => _ChatScreenState();
}

class _ChatScreenState extends State<ChatScreen> {
  late final MessageService _service;
  late final MessageHubService _hub;
  final TextEditingController _textController = TextEditingController();
  final ScrollController _scrollController = ScrollController();
  List<MessageDto> _messages = [];
  String? _myUserId;
  bool _loading = true;
  bool _loadingMore = false;
  bool _hasMore = true;
  int _page = 1;
  bool _sending = false;
  bool _canSend = true;
  bool _canSendLoaded = false;
  Timer? _typingTimer;
  String? _typingUserId;

  @override
  void initState() {
    super.initState();
    final auth = context.read<AuthController>();
    _service = MessageService(auth.api);
    _hub = context.read<MessageHubService>();
    _myUserId = auth.profile?.id;
    _loadMessages();
    _checkCanSend();
    _scrollController.addListener(_onScroll);

    _hub.onMessageReceived = (msg) {
      if (msg.senderId == widget.otherUserId ||
          msg.receiverId == widget.otherUserId) {
        setState(() {
          if (!_messages.any((m) => m.id == msg.id)) {
            _messages.insert(0, msg);
          }
        });
        _scrollToBottom();
        if (msg.senderId == widget.otherUserId) {
          _hub.markRead(widget.otherUserId);
          _service.markAsRead(widget.otherUserId);
        }
      }
    };

    _hub.onMessagesRead = (otherUserId) {
      if (otherUserId == widget.otherUserId) {
        setState(() {
          _messages = _messages.map((msg) {
            if (msg.senderId == _myUserId && !msg.isRead) {
              return MessageDto(
                id: msg.id,
                senderId: msg.senderId,
                senderName: msg.senderName,
                senderImage: msg.senderImage,
                receiverId: msg.receiverId,
                receiverName: msg.receiverName,
                receiverImage: msg.receiverImage,
                content: msg.content,
                sentAt: msg.sentAt,
                isRead: true,
                readAt: DateTime.now(),
                type: msg.type,
              );
            }
            return msg;
          }).toList();
        });
      }
    };

    _hub.onUserTyping = (userId) {
      if (userId == widget.otherUserId) {
        setState(() => _typingUserId = userId);
      }
    };

    _hub.onUserStopTyping = (userId) {
      if (userId == widget.otherUserId) {
        setState(() => _typingUserId = null);
      }
    };

    _hub.markRead(widget.otherUserId);
    _service.markAsRead(widget.otherUserId);
  }

  @override
  void dispose() {
    _hub.signalConversationsChanged();
    _textController.dispose();
    _scrollController.dispose();
    _typingTimer?.cancel();
    super.dispose();
  }

  Future<void> _loadMessages() async {
    setState(() => _loading = true);
    _page = 1;
    try {
      final result = await _service.getConversation(
        widget.otherUserId,
        page: _page,
        pageSize: 10,
      );
      setState(() {
        _messages = result.messages;
        _hasMore = result.hasMore;
      });
      _scrollToBottom();
      _page++;
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _loadMore() async {
    if (_loadingMore || !_hasMore) return;
    setState(() => _loadingMore = true);
    try {
      final result = await _service.getConversation(
        widget.otherUserId,
        page: _page,
        pageSize: 10,
      );
      setState(() {
        _messages.addAll(result.messages);
        _hasMore = result.hasMore;
      });
      _page++;
    } catch (_) {
    } finally {
      if (mounted) setState(() => _loadingMore = false);
    }
  }

  void _onScroll() {
    if (!_scrollController.hasClients || _loadingMore || !_hasMore) return;
    final maxScroll = _scrollController.position.maxScrollExtent;
    final current = _scrollController.position.pixels;
    if (current >= maxScroll - 200) {
      _loadMore();
    }
  }

  Future<void> _checkCanSend() async {
    final can = await _service.canSend(widget.otherUserId);
    if (mounted) {
      setState(() {
        _canSend = can;
        _canSendLoaded = true;
      });
    }
  }

  Future<void> _sendMessage() async {
    final text = _textController.text.trim();
    if (text.isEmpty) return;

    setState(() => _sending = true);
    _textController.clear();

    try {
      final form = SendMessageDto(
        receiverId: widget.otherUserId,
        content: text,
      );

      if (_hub.isConnected) {
        await _hub.sendMessage(form);
      } else {
        final msg = await _service.send(form);
        setState(() => _messages.insert(0, msg));
        _scrollToBottom();
      }
    } catch (error) {
      if (mounted) {
        showAppSnackBar(context, ApiClient.errorMessage(error));
      }
    } finally {
      if (mounted) setState(() => _sending = false);
    }
  }

  void _onTyping(String value) {
    _typingTimer?.cancel();
    _hub.sendTyping(widget.otherUserId);
    _typingTimer = Timer(const Duration(seconds: 2), () {
      _hub.sendStopTyping(widget.otherUserId);
    });
  }

  void _scrollToBottom() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (_scrollController.hasClients) {
        _scrollController.animateTo(
          0,
          duration: const Duration(milliseconds: 200),
          curve: Curves.easeOut,
        );
      }
    });
  }

  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          toolbarHeight: 76,
          titleSpacing: 16,
          leading: IconButton(
            tooltip: 'رجوع',
            onPressed: () => context.pop(),
            icon: const Icon(Icons.arrow_back_rounded),
          ),
          title: Row(
            children: [
              CircleAvatar(
                radius: 18,
                backgroundColor: AppColors.softAmber,
                child: Text(
                  widget.otherUserName.isNotEmpty
                      ? widget.otherUserName[0]
                      : '?',
                  style: const TextStyle(
                    color: AppColors.primaryDark,
                    fontWeight: FontWeight.w900,
                    fontSize: 14,
                  ),
                ),
              ),
              const SizedBox(width: 10),
              Text(
                widget.otherUserName,
                style: const TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.w900,
                ),
              ),
            ],
          ),
        ),
        body: Column(
          children: [
            if (!_canSend && _canSendLoaded)
              Container(
                width: double.infinity,
                padding: const EdgeInsets.symmetric(
                  horizontal: 16,
                  vertical: 10,
                ),
                color: AppColors.softAmber,
                child: const Row(
                  children: [
                    Icon(Icons.info_outline, color: AppColors.primaryDark),
                    SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        'تم تعطيل إرسال الرسائل من قبل الطبيب.',
                        style: TextStyle(
                          color: AppColors.primaryDark,
                          fontWeight: FontWeight.w600,
                          fontSize: 13,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            Expanded(
              child: _loading
                  ? const Center(child: CircularProgressIndicator())
                  : GestureDetector(
                      onTap: () => FocusScope.of(context).unfocus(),
                      child: ListView.builder(
                        reverse: true,
                        controller: _scrollController,
                        padding: const EdgeInsets.symmetric(
                          horizontal: 12,
                          vertical: 8,
                        ),
                        itemCount: _messages.length +
                            (_typingUserId != null ? 1 : 0) +
                            (_loadingMore ? 1 : 0),
                        itemBuilder: (context, index) {
                          final lastIndex = _messages.length +
                              (_typingUserId != null ? 1 : 0) +
                              (_loadingMore ? 1 : 0) -
                              1;

                          if (_loadingMore && index == lastIndex) {
                            return const Padding(
                              padding: EdgeInsets.symmetric(vertical: 16),
                              child: Center(
                                child: SizedBox(
                                  width: 20,
                                  height: 20,
                                  child: CircularProgressIndicator(
                                    strokeWidth: 2,
                                  ),
                                ),
                              ),
                            );
                          }

                          if (_typingUserId != null &&
                              index == 0) {
                            return const Padding(
                              padding: EdgeInsets.all(8),
                              child: Row(
                                children: [
                                  SizedBox(width: 40),
                                  Text(
                                    'يكتب...',
                                    style: TextStyle(
                                      color: AppColors.muted,
                                      fontSize: 13,
                                      fontStyle: FontStyle.italic,
                                    ),
                                  ),
                                ],
                              ),
                            );
                          }

                          final msgOffset =
                              _typingUserId != null ? 1 : 0;
                          final msgIndex = index - msgOffset;
                          final msg = _messages[msgIndex];
                          final isMe = msg.senderId == _myUserId;

                          return _MessageBubble(
                            message: msg,
                            isMe: isMe,
                          );
                        },
                      ),
                    ),
            ),
            Container(
              decoration: const BoxDecoration(
                color: Colors.white,
                boxShadow: [
                  BoxShadow(
                    color: Color(0x0D000000),
                    blurRadius: 8,
                    offset: Offset(0, -2),
                  ),
                ],
              ),
              child: SafeArea(
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(12, 8, 12, 8),
                  child: Row(
                    children: [
                      Expanded(
                        child: TextField(
                          controller: _textController,
                          onChanged: !_canSend ? null : _onTyping,
                          enabled: _canSend,
                          decoration: InputDecoration(
                            hintText: !_canSend
                                ? 'المراسلة معطلة'
                                : 'اكتب رسالتك...',
                            filled: true,
                            fillColor: const Color(0xFFF5F7FA),
                            contentPadding: const EdgeInsets.symmetric(
                              horizontal: 16,
                              vertical: 12,
                            ),
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(24),
                              borderSide: BorderSide.none,
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(width: 8),
                      Material(
                        color: _canSend
                            ? AppColors.primary
                            : AppColors.muted,
                        borderRadius: BorderRadius.circular(24),
                        child: InkWell(
                          borderRadius: BorderRadius.circular(24),
                          onTap:
                              (_sending || !_canSend) ? null : _sendMessage,
                          child: Container(
                            width: 44,
                            height: 44,
                            alignment: Alignment.center,
                            child: _sending
                                ? const SizedBox(
                                    width: 20,
                                    height: 20,
                                    child: CircularProgressIndicator(
                                      strokeWidth: 2,
                                      color: Colors.white,
                                    ),
                                  )
                                : const Icon(
                                    Icons.send_rounded,
                                    color: Colors.white,
                                  ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ],
        ),
      );
}

class _MessageBubble extends StatelessWidget {
  const _MessageBubble({
    required this.message,
    required this.isMe,
  });

  final MessageDto message;
  final bool isMe;

  @override
  Widget build(BuildContext context) {
    final time = DateFormat('hh:mm a').format(message.sentAt);

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 3),
      child: Row(
        mainAxisAlignment:
            isMe ? MainAxisAlignment.end : MainAxisAlignment.start,
        crossAxisAlignment: CrossAxisAlignment.end,
        children: [
          if (!isMe) ...[
            CircleAvatar(
              radius: 14,
              backgroundColor: AppColors.softAmber,
              child: Text(
                message.senderName.isNotEmpty
                    ? message.senderName[0]
                    : '?',
                style: const TextStyle(
                  color: AppColors.primaryDark,
                  fontSize: 11,
                  fontWeight: FontWeight.w900,
                ),
              ),
            ),
            const SizedBox(width: 8),
          ],
          Flexible(
            child: Container(
              constraints: BoxConstraints(
                maxWidth: MediaQuery.of(context).size.width * 0.7,
              ),
              padding: const EdgeInsets.symmetric(
                horizontal: 14,
                vertical: 10,
              ),
              decoration: BoxDecoration(
                color: isMe ? AppColors.primary : const Color(0xFFF0F0F0),
                borderRadius: BorderRadius.only(
                  topLeft: const Radius.circular(16),
                  topRight: const Radius.circular(16),
                  bottomLeft: isMe
                      ? const Radius.circular(16)
                      : const Radius.circular(4),
                  bottomRight: isMe
                      ? const Radius.circular(4)
                      : const Radius.circular(16),
                ),
              ),
              child: Column(
                crossAxisAlignment: isMe
                    ? CrossAxisAlignment.end
                    : CrossAxisAlignment.start,
                children: [
                  Text(
                    message.content,
                    style: TextStyle(
                      color: isMe ? Colors.white : AppColors.text,
                      fontSize: 14,
                      height: 1.4,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        time,
                        style: TextStyle(
                          color: isMe
                              ? Colors.white.withOpacity(.7)
                              : AppColors.muted,
                          fontSize: 10,
                        ),
                      ),
                      if (isMe && message.isRead) ...[
                        const SizedBox(width: 4),
                        const Icon(
                          Icons.done_all_rounded,
                          size: 14,
                          color: Colors.white70,
                        ),
                      ],
                      if (isMe && !message.isRead) ...[
                        const SizedBox(width: 4),
                        const Icon(
                          Icons.done_rounded,
                          size: 14,
                          color: Colors.white54,
                        ),
                      ],
                    ],
                  ),
                ],
              ),
            ),
          ),
          if (isMe) const SizedBox(width: 8),
        ],
      ),
    );
  }
}
