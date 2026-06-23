import 'dart:async';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';

import 'api_client.dart';

class ForegroundMessageNotification {
  const ForegroundMessageNotification({
    required this.senderId,
    required this.senderName,
    required this.body,
    required this.hasImage,
  });

  final String senderId;
  final String senderName;
  final String body;
  final bool hasImage;
}

class ForegroundAppNotification {
  const ForegroundAppNotification({
    required this.type,
    required this.title,
    required this.body,
    required this.data,
  });

  final String type;
  final String title;
  final String body;
  final Map<String, String> data;
}

class PushNotificationService {
  PushNotificationService(this._client);

  static const _androidChannel = AndroidNotificationChannel(
    'clinic_app_notifications',
    'Clinic notifications',
    description: 'Booking updates and account notifications.',
    importance: Importance.high,
  );
  static final FlutterLocalNotificationsPlugin _localNotifications =
      FlutterLocalNotificationsPlugin();
  static bool _foregroundNotificationsInitialized = false;
  static final _newMessageController = StreamController<String>.broadcast();
  static final _foregroundMessageController =
      StreamController<ForegroundMessageNotification>.broadcast();
  static final _foregroundAppNotificationController =
      StreamController<ForegroundAppNotification>.broadcast();
  static Stream<String> get onNewMessageNotification =>
      _newMessageController.stream;
  static Stream<ForegroundMessageNotification>
      get onForegroundMessageNotification => _foregroundMessageController.stream;
  static Stream<ForegroundAppNotification>
      get onForegroundAppNotification =>
          _foregroundAppNotificationController.stream;

  static bool get isSupported =>
      !kIsWeb && defaultTargetPlatform == TargetPlatform.android;

  final ApiClient _client;
  final FirebaseMessaging _messaging = FirebaseMessaging.instance;

  static Future<void> initializeForegroundNotifications() async {
    if (!isSupported) return;
    if (_foregroundNotificationsInitialized) return;

    const initializationSettings = InitializationSettings(
      android: AndroidInitializationSettings('@mipmap/ic_launcher'),
      iOS: DarwinInitializationSettings(),
      macOS: DarwinInitializationSettings(),
    );
    await _localNotifications.initialize(settings: initializationSettings);

    final androidNotifications = _localNotifications
        .resolvePlatformSpecificImplementation<
          AndroidFlutterLocalNotificationsPlugin
        >();
    await androidNotifications?.createNotificationChannel(_androidChannel);

    FirebaseMessaging.onMessage.listen(_onFirebaseMessage);
    _foregroundNotificationsInitialized = true;
  }

  static Future<void> showLocalNotification({
    required String title,
    required String body,
  }) async {
    if (!isSupported) return;
    if (title.trim().isEmpty && body.trim().isEmpty) return;

    try {
      await _localNotifications.show(
        id: title.hashCode ^ body.hashCode,
        title: title,
        body: body,
        notificationDetails: const NotificationDetails(
          android: AndroidNotificationDetails(
            'clinic_app_notifications',
            'Clinic notifications',
            channelDescription: 'Booking updates and account notifications.',
            importance: Importance.high,
            priority: Priority.high,
            icon: '@mipmap/ic_launcher',
          ),
          iOS: DarwinNotificationDetails(),
          macOS: DarwinNotificationDetails(),
        ),
      );
    } catch (_) {}
  }

  static void _onFirebaseMessage(RemoteMessage message) {
    final type = message.data['type']?.toString();
    final notification = message.notification;
    final title = notification?.title ?? message.data['title']?.toString();
    final body = notification?.body ?? message.data['body']?.toString();

    if (type == 'new_message') {
      final senderId = message.data['senderId']?.toString() ?? '';
      _newMessageController.add(senderId);
      _foregroundMessageController.add(
        ForegroundMessageNotification(
          senderId: senderId,
          senderName:
              message.data['senderName']?.toString() ??
              message.notification?.title ??
              '',
          body:
              message.data['body']?.toString() ??
              message.notification?.body ??
              '',
          hasImage:
              message.data['hasImage']?.toString().toLowerCase() == 'true',
        ),
      );
      return;
    }

    if (type == 'booking') {
      _foregroundAppNotificationController.add(
        ForegroundAppNotification(
          type: type ?? '',
          title: title ?? '',
          body: body ?? '',
          data: message.data.map(
            (key, value) => MapEntry(key, value.toString()),
          ),
        ),
      );
      return;
    }

    if ((title == null || title.trim().isEmpty) &&
        (body == null || body.trim().isEmpty)) {
      return;
    }

    showLocalNotification(title: title ?? '', body: body ?? '');
  }

  Future<void> registerCurrentDevice() async {
    if (!isSupported) return;
    await _requestPermissionIfNeeded();
    final token = await _messaging.getToken();
    if (token == null || token.trim().isEmpty) return;

    await _registerToken(token);
    FirebaseMessaging.instance.onTokenRefresh.listen((newToken) {
      if (newToken.trim().isEmpty) return;
      _registerToken(newToken);
    });
  }

  Future<void> unregisterCurrentDevice() async {
    if (!isSupported) return;
    final token = await _messaging.getToken();
    if (token == null || token.trim().isEmpty) return;

    await _client.dio.delete(
      '/User/device-token',
      data: {
        'token': token,
        'platform': _platformName,
      },
    );
  }

  Future<void> _registerToken(String token) async {
    await _client.dio.post(
      '/User/device-token',
      data: {
        'token': token,
        'platform': _platformName,
      },
    );
  }

  Future<void> _requestPermissionIfNeeded() async {
    if (defaultTargetPlatform != TargetPlatform.iOS &&
        defaultTargetPlatform != TargetPlatform.macOS &&
        defaultTargetPlatform != TargetPlatform.android) {
      return;
    }

    await _messaging.requestPermission(
      alert: true,
      badge: true,
      sound: true,
    );
  }

  static void dispose() {
    _newMessageController.close();
    _foregroundMessageController.close();
    _foregroundAppNotificationController.close();
  }

  String get _platformName => switch (defaultTargetPlatform) {
    TargetPlatform.android => 'android',
    TargetPlatform.iOS => 'ios',
    TargetPlatform.macOS => 'macos',
    TargetPlatform.windows => 'windows',
    TargetPlatform.linux => 'linux',
    TargetPlatform.fuchsia => 'fuchsia',
  };
}
