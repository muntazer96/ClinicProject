import '../../core/api_client.dart';

class AppNotificationItem {
  const AppNotificationItem({
    required this.id,
    required this.message,
    required this.createdAt,
    required this.status,
    this.readAt,
  });

  final int id;
  final String message;
  final DateTime createdAt;
  final int status;
  final DateTime? readAt;

  bool get isRead => status == 1;

  factory AppNotificationItem.fromJson(Map<String, dynamic> json) =>
      AppNotificationItem(
        id: json['id'] as int? ?? 0,
        message: json['message'] as String? ?? '',
        createdAt: DateTime.parse(json['createdAt'] as String),
        status: json['status'] as int? ?? 0,
        readAt: json['readAt'] == null
            ? null
            : DateTime.tryParse(json['readAt'] as String),
      );
}

class NotificationService {
  NotificationService(this._client);

  final ApiClient _client;

  String _prefix(bool doctor) =>
      doctor ? '/Notification/doctor/my' : '/Notification/my';

  Future<List<AppNotificationItem>> getNotifications({
    required bool doctor,
    int page = 1,
    int pageSize = 20,
  }) async {
    final response = await _client.dio.get(
      _prefix(doctor),
      queryParameters: {'page': page, 'pageSize': pageSize},
    );
    final data = response.data['data'] as List? ?? const [];
    return data
        .map(
          (item) => AppNotificationItem.fromJson(item as Map<String, dynamic>),
        )
        .toList();
  }

  Future<int> getUnreadCount({required bool doctor}) async {
    final response = await _client.dio.get('${_prefix(doctor)}/unread-count');
    final data = response.data['data'];
    if (data is Map) {
      return (data['unreadCount'] as num?)?.toInt() ??
          (data['UnreadCount'] as num?)?.toInt() ??
          0;
    }
    return (data as num?)?.toInt() ?? 0;
  }

  Future<void> markRead({required bool doctor, required int id}) async {
    await _client.dio.post('${_prefix(doctor)}/$id/read');
  }

  Future<void> markAllRead({required bool doctor}) async {
    await _client.dio.post('${_prefix(doctor)}/read-all');
  }
}
