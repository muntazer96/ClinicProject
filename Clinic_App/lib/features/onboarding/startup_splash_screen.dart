import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:go_router/go_router.dart';

import '../../core/app_theme.dart';
import '../../widgets/developer_credit.dart';

class StartupSplashScreen extends StatefulWidget {
  const StartupSplashScreen({super.key});

  static const onboardingSeenKey = 'clinic_onboarding_seen';

  @override
  State<StartupSplashScreen> createState() => _StartupSplashScreenState();
}

class _StartupSplashScreenState extends State<StartupSplashScreen>
    with SingleTickerProviderStateMixin {
  final _storage = const FlutterSecureStorage();
  late final AnimationController _controller;
  late final Animation<double> _scale;
  late final Animation<double> _fade;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 950),
    )..forward();
    _scale = CurvedAnimation(parent: _controller, curve: Curves.easeOutBack);
    _fade = CurvedAnimation(parent: _controller, curve: Curves.easeOut);
    _continue();
  }

  Future<void> _continue() async {
    await Future<void>.delayed(const Duration(milliseconds: 1450));
    final seen = await _storage.read(
      key: StartupSplashScreen.onboardingSeenKey,
    );
    if (!mounted) return;
    context.go(seen == 'true' ? '/' : '/onboarding');
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    backgroundColor: AppColors.primaryDark,
    body: SafeArea(
      child: Padding(
        padding: const EdgeInsets.fromLTRB(24, 36, 24, 22),
        child: Column(
          children: [
            const Spacer(),
            Center(
              child: FadeTransition(
                opacity: _fade,
                child: ScaleTransition(
                  scale: _scale,
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Container(
                        width: 118,
                        height: 118,
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: Colors.white,
                          borderRadius: BorderRadius.circular(28),
                          boxShadow: const [
                            BoxShadow(
                              color: Color(0x33000000),
                              blurRadius: 24,
                              offset: Offset(0, 12),
                            ),
                          ],
                        ),
                        child: Image.asset('assets/app_logo.png'),
                      ),
                      const SizedBox(height: 18),
                      const Text(
                        'عيادتي',
                        style: TextStyle(
                          color: Colors.white,
                          fontSize: 28,
                          fontWeight: FontWeight.w900,
                        ),
                      ),
                      const SizedBox(height: 6),
                      const Text(
                        'حجزك الطبي صار أسهل',
                        style: TextStyle(color: Color(0xFFD9F2EE)),
                      ),
                    ],
                  ),
                ),
              ),
            ),
            const Spacer(),
            const DeveloperCredit(light: true),
          ],
        ),
      ),
    ),
  );
}
