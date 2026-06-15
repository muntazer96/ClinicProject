class SendMessageDto {
  SendMessageDto({
    required this.receiverId,
    required this.content,
    this.type = 'General',
  });

  final String receiverId;
  final String content;
  final String type;

  Map<String, dynamic> toJson() => {
    'receiverId': receiverId,
    'content': content,
    'type': type,
  };
}

class MessageDto {
  MessageDto({
    required this.id,
    required this.senderId,
    required this.senderName,
    this.senderImage,
    required this.receiverId,
    required this.receiverName,
    this.receiverImage,
    required this.content,
    this.imageName,
    required this.sentAt,
    required this.isRead,
    this.readAt,
    required this.type,
  });

  factory MessageDto.fromJson(Map<String, dynamic> json) => MessageDto(
    id: json['id'] as int,
    senderId: json['senderId'] as String,
    senderName: json['senderName'] as String? ?? '',
    senderImage: json['senderImage'] as String?,
    receiverId: json['receiverId'] as String,
    receiverName: json['receiverName'] as String? ?? '',
    receiverImage: json['receiverImage'] as String?,
    content: json['content'] as String? ?? '',
    imageName: json['imageName'] as String?,
    sentAt: DateTime.parse(json['sentAt'] as String),
    isRead: json['isRead'] as bool? ?? false,
    readAt: json['readAt'] == null
        ? null
        : DateTime.tryParse(json['readAt'] as String),
    type: json['type'] as String? ?? 'General',
  );

  final int id;
  final String senderId;
  final String senderName;
  final String? senderImage;
  final String receiverId;
  final String receiverName;
  final String? receiverImage;
  final String content;
  final String? imageName;
  final DateTime sentAt;
  final bool isRead;
  final DateTime? readAt;
  final String type;
}

class ConversationDto {
  ConversationDto({
    required this.otherUserId,
    required this.otherUserName,
    this.otherUserImage,
    required this.lastMessage,
    this.lastMessageImageName,
    required this.lastMessageAt,
    required this.unreadCount,
  });

  factory ConversationDto.fromJson(Map<String, dynamic> json) =>
      ConversationDto(
        otherUserId: json['otherUserId'] as String,
        otherUserName: json['otherUserName'] as String? ?? '',
        otherUserImage: json['otherUserImage'] as String?,
        lastMessage: json['lastMessage'] as String? ?? '',
        lastMessageImageName: json['lastMessageImageName'] as String?,
        lastMessageAt: DateTime.parse(json['lastMessageAt'] as String),
        unreadCount: json['unreadCount'] as int? ?? 0,
      );

  final String otherUserId;
  final String otherUserName;
  final String? otherUserImage;
  final String lastMessage;
  final String? lastMessageImageName;
  final DateTime lastMessageAt;
  final int unreadCount;
}
