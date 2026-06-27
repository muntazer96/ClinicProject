import 'dart:async';

import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'app.dart';
import 'core/api_client.dart';
import 'core/push_notification_service.dart';
import 'core/theme_controller.dart';
import 'features/auth/auth_controller.dart';
import 'features/messages/message_hub_service.dart';
import 'features/notifications/notification_center.dart';

@pragma('vm:entry-point')
Future<void> _firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  if (!PushNotificationService.isSupported) return;
  await Firebase.initializeApp();
}

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  if (PushNotificationService.isSupported) {
    await Firebase.initializeApp();
    FirebaseMessaging.onBackgroundMessage(_firebaseMessagingBackgroundHandler);
    await PushNotificationService.initializeForegroundNotifications();
  }
  final api = ApiClient();
  final auth = AuthController(api);
  final themeController = ThemeController();
  final messageHub = MessageHubService(api);

  await themeController.restore();

  await auth.restoreSession().timeout(
    const Duration(seconds: 10),
    onTimeout: () {},
  );

  if (auth.isAuthenticated) {
    messageHub.connect();
  }

  runApp(
    MultiProvider(
      providers: [
        Provider<ApiClient>.value(value: api),
        ChangeNotifierProvider<ThemeController>.value(value: themeController),
        ChangeNotifierProvider<AuthController>.value(value: auth),
        ChangeNotifierProvider<MessageHubService>.value(value: messageHub),
        ChangeNotifierProvider<NotificationCenter>(
          create: (_) => NotificationCenter(),
        ),
      ],
      child: const ClinicApp(),
    ),
  );
}
