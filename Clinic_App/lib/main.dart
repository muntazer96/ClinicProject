import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'app.dart';
import 'core/api_client.dart';
import 'core/push_notification_service.dart';
import 'features/auth/auth_controller.dart';

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
  final auth = AuthController(ApiClient());
  await auth.restoreSession();
  runApp(ChangeNotifierProvider.value(value: auth, child: const ClinicApp()));
}
