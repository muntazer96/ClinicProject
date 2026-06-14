import 'dart:async';
import 'package:flutter/foundation.dart';
import 'package:signalr_netcore/signalr_client.dart';
import '../../core/api_client.dart';
import 'models/message_models.dart';

class MessageHubService extends ChangeNotifier {
  MessageHubService(this._client);

  final ApiClient _client;
  HubConnection? _hubConnection;
  bool _isConnected = false;
  int _unreadCount = 0;

  bool get isConnected => _isConnected;
  int get unreadCount => _unreadCount;

  final _messageController = StreamController<MessageDto>.broadcast();
  Stream<MessageDto> get onMessage => _messageController.stream;

  void Function(MessageDto message)? onMessageReceived;
  void Function(String otherUserId)? onMessagesRead;
  void Function(String userId)? onUserTyping;
  void Function(String userId)? onUserStopTyping;

  String _buildHubUrl() {
    final dioBase = _client.dio.options.baseUrl;
    final uri = Uri.parse(dioBase);
    return uri.replace(path: '/hubs/message').toString();
  }

  Future<void> connect() async {
    if (_isConnected) return;

    final token = _client.dio.options.headers['Authorization'] as String?;
    if (token == null || token.isEmpty) return;

    _hubConnection = HubConnectionBuilder()
        .withUrl(
          _buildHubUrl(),
          options: HttpConnectionOptions(
            accessTokenFactory: () async => token.replaceFirst('Bearer ', ''),
          ),
        )
        .withAutomaticReconnect()
        .build();

    _hubConnection!.on('ReceiveMessage', (args) {
      if (args == null || args.isEmpty) return;
      final dto = MessageDto.fromJson(args[0] as Map<String, dynamic>);
      _messageController.add(dto);
      onMessageReceived?.call(dto);
      notifyListeners();
    });

    _hubConnection!.on('MessageSent', (args) {
      if (args == null || args.isEmpty) return;
      final dto = MessageDto.fromJson(args[0] as Map<String, dynamic>);
      _messageController.add(dto);
      onMessageReceived?.call(dto);
      notifyListeners();
    });

    _hubConnection!.on('MessagesRead', (args) {
      if (args == null || args.isEmpty) return;
      final otherUserId = args[0] as String;
      onMessagesRead?.call(otherUserId);
      notifyListeners();
    });

    _hubConnection!.on('UnreadCount', (args) {
      if (args == null || args.isEmpty) return;
      _unreadCount = (args[0] as num).toInt();
      notifyListeners();
    });

    _hubConnection!.on('UserTyping', (args) {
      if (args == null || args.isEmpty) return;
      onUserTyping?.call(args[0] as String);
    });

    _hubConnection!.on('UserStopTyping', (args) {
      if (args == null || args.isEmpty) return;
      onUserStopTyping?.call(args[0] as String);
    });

    _hubConnection!.on('Error', (args) {
      if (args == null || args.isEmpty) return;
      debugPrint('SignalR error: ${args[0]}');
    });

    _hubConnection!.onclose(({error}) {
      _isConnected = false;
      debugPrint('SignalR connection closed: $error');
      notifyListeners();
    });

    _hubConnection!.onreconnected(({connectionId}) {
      _isConnected = true;
      debugPrint('SignalR reconnected: $connectionId');
      notifyListeners();
    });

    _hubConnection!.onreconnecting(({error}) {
      _isConnected = false;
      debugPrint('SignalR reconnecting: $error');
      notifyListeners();
    });

    try {
      await _hubConnection!.start();
      _isConnected = true;
      debugPrint('SignalR connected');
      notifyListeners();
    } catch (e) {
      _isConnected = false;
      debugPrint('SignalR connection failed: $e');
      notifyListeners();
    }
  }

  Future<void> sendMessage(SendMessageDto form) async {
    if (_hubConnection == null || !_isConnected) return;
    try {
      await _hubConnection!.invoke('SendMessage', args: [form.toJson()]);
    } catch (e) {
      debugPrint('SignalR send failed: $e');
      rethrow;
    }
  }

  Future<void> markRead(String otherUserId) async {
    if (_hubConnection == null || !_isConnected) return;
    try {
      await _hubConnection!.invoke('MarkRead', args: [otherUserId]);
    } catch (e) {
      debugPrint('SignalR markRead failed: $e');
    }
  }

  Future<void> sendTyping(String otherUserId) async {
    if (_hubConnection == null || !_isConnected) return;
    try {
      await _hubConnection!.invoke('Typing', args: [otherUserId]);
    } catch (_) {}
  }

  Future<void> sendStopTyping(String otherUserId) async {
    if (_hubConnection == null || !_isConnected) return;
    try {
      await _hubConnection!.invoke('StopTyping', args: [otherUserId]);
    } catch (_) {}
  }

  void disconnect() {
    _hubConnection?.stop();
    _hubConnection = null;
    _isConnected = false;
    _unreadCount = 0;
    notifyListeners();
  }

  @override
  void dispose() {
    disconnect();
    _messageController.close();
    super.dispose();
  }
}
