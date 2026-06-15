import 'package:dio/dio.dart';
import 'package:image_picker/image_picker.dart';

import '../../core/api_client.dart';
import 'models/message_models.dart';

class MessageService {
  MessageService(this._client);

  final ApiClient _client;

  Future<MessageDto> send(SendMessageDto form) async {
    final response = await _client.dio.post('/Message/send', data: form.toJson());
    return MessageDto.fromJson(response.data['data'] as Map<String, dynamic>);
  }

  Future<MessageDto> sendImage({
    required String receiverId,
    required XFile image,
    String content = '',
  }) async {
    final bytes = await image.readAsBytes();
    final data = FormData.fromMap({
      'receiverId': receiverId,
      'content': content,
      'file': MultipartFile.fromBytes(bytes, filename: image.name),
    });
    final response = await _client.dio.post('/Message/send-image', data: data);
    return MessageDto.fromJson(response.data['data'] as Map<String, dynamic>);
  }

  Future<List<ConversationDto>> getConversations() async {
    final response = await _client.dio.get('/Message/conversations');
    final data = response.data['data'] as List? ?? [];
    return data
        .map((item) => ConversationDto.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<ConversationResult> getConversation(
    String otherUserId, {
    int page = 1,
    int pageSize = 10,
  }) async {
    final response = await _client.dio.get(
      '/Message/conversation/$otherUserId',
      queryParameters: {'page': page, 'pageSize': pageSize},
    );
    final data = response.data['data'] as Map<String, dynamic>;
    return ConversationResult.fromJson(data);
  }

  Future<void> markAsRead(String otherUserId) async {
    await _client.dio.put('/Message/read/$otherUserId');
  }

  Future<bool> canSend(String userId) async {
    try {
      final response = await _client.dio.get('/Message/can-send/$userId');
      final data = response.data is Map
          ? response.data as Map<String, dynamic>
          : <String, dynamic>{};
      return data['canSend'] as bool? ??
          (data['data'] is Map<String, dynamic>
              ? (data['data']['canSend'] as bool? ?? false)
              : false);
    } catch (_) {
      return false;
    }
  }

  Future<int> getUnreadCount() async {
    try {
      final response = await _client.dio.get('/Message/unread-count');
      final data = response.data is Map
          ? response.data as Map<String, dynamic>
          : <String, dynamic>{};
      final responseData = data['data'];
      return responseData is Map<String, dynamic>
          ? (responseData['unreadCount'] as num?)?.toInt() ?? 0
          : (responseData as num?)?.toInt() ?? 0;
    } catch (_) {
      return 0;
    }
  }
}

class ConversationResult {
  ConversationResult({
    required this.messages,
    required this.totalCount,
    required this.page,
    required this.pageSize,
    required this.hasMore,
  });

  factory ConversationResult.fromJson(Map<String, dynamic> json) =>
      ConversationResult(
        messages: (json['messages'] as List?)
                ?.map(
                  (item) => MessageDto.fromJson(item as Map<String, dynamic>),
                )
                .toList() ??
            [],
        totalCount: json['totalCount'] as int? ?? 0,
        page: json['page'] as int? ?? 1,
        pageSize: json['pageSize'] as int? ?? 50,
        hasMore: json['hasMore'] as bool? ?? false,
      );

  final List<MessageDto> messages;
  final int totalCount;
  final int page;
  final int pageSize;
  final bool hasMore;
}
