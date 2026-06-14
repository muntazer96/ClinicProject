import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'core/app_router.dart';
import 'core/app_theme.dart';
import 'features/auth/auth_controller.dart';
import 'features/messages/message_hub_service.dart';
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
    return MaterialApp.router(
      title: 'عيادتي',
      debugShowCheckedModeBanner: false,
      routerConfig: router,
      locale: const Locale('ar'),
      builder: (context, child) {
        ErrorWidget.builder = (details) => Directionality(
          textDirection: TextDirection.rtl,
          child: Material(
            color: Colors.white,
            child: Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(Icons.error_outline, size: 48, color: AppColors.danger),
                    const SizedBox(height: 12),
                    const Text(
                      'حدث خطأ غير متوقع',
                      style: TextStyle(fontSize: 18, fontWeight: FontWeight.w900),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      details.exceptionAsString(),
                      textAlign: TextAlign.center,
                      style: const TextStyle(color: AppColors.muted, fontSize: 13),
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
            child: OfflineGate(
              child: AppVersionGate(child: child ?? const SizedBox.shrink()),
            ),
          ),
        );
      },
      theme: buildAppTheme(),
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
  Widget build(BuildContext context) {
    final auth = context.watch<AuthController>();
    final hub = context.read<MessageHubService>();

    if (auth.isAuthenticated && !hub.isConnected) {
      hub.connect();
    } else if (!auth.isAuthenticated && hub.isConnected) {
      hub.disconnect();
    }

    return widget.child;
  }
}
