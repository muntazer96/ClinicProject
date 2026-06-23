import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'core/app_router.dart';
import 'core/app_theme.dart';
import 'core/theme_controller.dart';
import 'features/auth/auth_controller.dart';
import 'features/messages/message_hub_service.dart';
import 'features/messages/widgets/in_app_message_notification_listener.dart';
import 'widgets/app_version_gate.dart';
import 'widgets/offline_gate.dart';

class ClinicApp extends StatefulWidget {
  const ClinicApp({super.key});

  @override
  State<ClinicApp> createState() => _ClinicAppState();
}

class _ClinicAppState extends State<ClinicApp> {
  late final router = createRouter(context.read<AuthController>());

  @override
  Widget build(BuildContext context) {
    final theme = context.watch<ThemeController>();
    return MaterialApp.router(
      title: 'عيادتي',
      debugShowCheckedModeBanner: false,
      routerConfig: router,
      locale: const Locale('ar'),
      builder: (context, child) {
        ErrorWidget.builder = (details) => Directionality(
          textDirection: TextDirection.rtl,
          child: Material(
            color: Theme.of(context).scaffoldBackgroundColor,
            child: Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(
                      Icons.error_outline,
                      size: 48,
                      color: AppColors.danger,
                    ),
                    const SizedBox(height: 12),
                    const Text(
                      'حدث خطأ غير متوقع',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.w900,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      details.exceptionAsString(),
                      textAlign: TextAlign.center,
                      style: TextStyle(color: context.appMuted, fontSize: 13),
                    ),
                  ],
                ),
              ),
            ),
          ),
        );
        return Directionality(
          textDirection: TextDirection.rtl,
          child: _SignalRConnector(
            child: InAppMessageNotificationListener(
              onOpenConversation: (senderId, senderName) {
                final auth = context.read<AuthController>();
                final path = auth.isDoctor
                    ? '/doctor/messages/$senderId'
                    : '/messages/$senderId';
                router.push(path, extra: senderName);
              },
              onOpenBookingNotification: () {
                final auth = context.read<AuthController>();
                router.push(
                  auth.isDoctor ? '/doctor/appointments' : '/bookings',
                );
              },
              child: OfflineGate(
                child: AppVersionGate(child: child ?? const SizedBox.shrink()),
              ),
            ),
          ),
        );
      },
      theme: buildAppTheme(),
      darkTheme: buildAppTheme(brightness: Brightness.dark),
      themeMode: theme.mode,
    );
  }
}

class _SignalRConnector extends StatefulWidget {
  const _SignalRConnector({required this.child});

  final Widget child;

  @override
  State<_SignalRConnector> createState() => _SignalRConnectorState();
}

class _SignalRConnectorState extends State<_SignalRConnector> {
  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    WidgetsBinding.instance.addPostFrameCallback((_) => _syncConnection());
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthController>();
    final hub = context.watch<MessageHubService>();

    if (auth.isAuthenticated && !hub.isConnected && !hub.isConnecting) {
      WidgetsBinding.instance.addPostFrameCallback((_) => _syncConnection());
    } else if (!auth.isAuthenticated && hub.isConnected) {
      WidgetsBinding.instance.addPostFrameCallback((_) => _syncConnection());
    }

    return widget.child;
  }

  void _syncConnection() {
    if (!mounted) return;
    final auth = context.read<AuthController>();
    final hub = context.read<MessageHubService>();

    if (auth.isAuthenticated && !hub.isConnected && !hub.isConnecting) {
      hub.connect();
    } else if (!auth.isAuthenticated && hub.isConnected) {
      hub.disconnect();
    }
  }
}
