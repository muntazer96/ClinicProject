import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'core/app_router.dart';
import 'features/auth/auth_controller.dart';

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
      builder: (context, child) =>
          Directionality(textDirection: TextDirection.rtl, child: child!),
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFF147D72),
          primary: const Color(0xFF147D72),
          surface: Colors.white,
        ),
        scaffoldBackgroundColor: const Color(0xFFF6FAF9),
        useMaterial3: true,
        appBarTheme: const AppBarTheme(
          backgroundColor: Colors.white,
          foregroundColor: Color(0xFF183B38),
          surfaceTintColor: Colors.transparent,
          elevation: 0,
        ),
        cardTheme: CardThemeData(
          color: Colors.white,
          surfaceTintColor: Colors.transparent,
          elevation: 0,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(18),
            side: const BorderSide(color: Color(0xFFE3EEEC)),
          ),
        ),
        filledButtonTheme: FilledButtonThemeData(
          style: FilledButton.styleFrom(
            minimumSize: const Size.fromHeight(52),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(14),
            ),
          ),
        ),
        inputDecorationTheme: InputDecorationTheme(
          border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
          enabledBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(14),
            borderSide: const BorderSide(color: Color(0xFFE0ECEA)),
          ),
          filled: true,
          fillColor: Colors.white,
          contentPadding: const EdgeInsets.symmetric(
            horizontal: 15,
            vertical: 16,
          ),
        ),
      ),
    );
  }
}
