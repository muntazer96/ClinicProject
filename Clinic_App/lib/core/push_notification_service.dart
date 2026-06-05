import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';

import 'api_client.dart';

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

    FirebaseMessaging.onMessage.listen(_showForegroundNotification);
    _foregroundNotificationsInitialized = true;
  }

  static Future<void> _showForegroundNotification(RemoteMessage message) async {
    final notification = message.notification;
    final title = notification?.title ?? message.data['title']?.toString();
    final body = notification?.body ?? message.data['body']?.toString();
    if ((title == null || title.trim().isEmpty) &&
        (body == null || body.trim().isEmpty)) {
      return;
    }

    await _localNotifications.show(
      id: message.hashCode,
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

  String get _platformName => switch (defaultTargetPlatform) {
    TargetPlatform.android => 'android',
    TargetPlatform.iOS => 'ios',
    TargetPlatform.macOS => 'macos',
    TargetPlatform.windows => 'windows',
    TargetPlatform.linux => 'linux',
    TargetPlatform.fuchsia => 'fuchsia',
  };
}
