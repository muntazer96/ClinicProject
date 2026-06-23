import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../core/app_theme.dart';
import 'app_logo.dart';

class AuthShell extends StatelessWidget {
  const AuthShell({
    super.key,
    required this.title,
    required this.subtitle,
    required this.child,
    this.footer,
  });

  final String title;
  final String subtitle;
  final Widget child;
  final Widget? footer;

  @override
  Widget build(BuildContext context) {
    final surface = context.appSurface;
    final text = context.appText;
    final muted = context.appMuted;
    return Scaffold(
      body: SafeArea(
        child: Stack(
          children: [
            Container(
              height: 245,
              decoration: const BoxDecoration(
                gradient: LinearGradient(
                  colors: [AppColors.primary, AppColors.primaryDark],
                  begin: Alignment.topRight,
                  end: Alignment.bottomLeft,
                ),
                borderRadius: BorderRadius.only(
                  bottomLeft: Radius.circular(34),
                  bottomRight: Radius.circular(34),
                ),
              ),
            ),

            Column(
              children: [
                Expanded(
                  child: SingleChildScrollView(
                    padding: const EdgeInsets.fromLTRB(18, 16, 18, 20),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        Row(
                          children: [
                            IconButton(
                              style: IconButton.styleFrom(
                                backgroundColor: Colors.white.withValues(
                                  alpha: .14,
                                ),
                                foregroundColor: Colors.white,
                              ),
                              onPressed: () => context.canPop()
                                  ? context.pop()
                                  : context.go('/'),
                              icon: const Icon(Icons.arrow_back),
                            ),
                            const Spacer(),
                            const Text(
                              'عيادتي',
                              style: TextStyle(
                                color: Colors.white,
                                fontSize: 18,
                                fontWeight: FontWeight.w800,
                              ),
                            ),
                            const SizedBox(width: 8),
                            const AppLogo(size: 38, light: true),
                          ],
                        ),

                        const SizedBox(height: 25),

                        const Text(
                          'رعايتك تبدأ بخطوة بسيطة',
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 22,
                            fontWeight: FontWeight.w800,
                          ),
                        ),

                        const SizedBox(height: 7),

                        const Text(
                          'سجّل حسابك وتابع حجوزاتك بسهولة ووضوح.',
                          style: TextStyle(
                            color: Color(0xFFD6F2EE),
                            height: 1.5,
                          ),
                        ),

                        const SizedBox(height: 24),

                        Container(
                          constraints: const BoxConstraints(maxWidth: 520),
                          padding: const EdgeInsets.all(22),
                          decoration: BoxDecoration(
                            color: surface,
                            borderRadius: BorderRadius.circular(22),
                            border: Border.all(color: context.appBorder),
                            boxShadow: [
                              BoxShadow(
                                color: context.isDark
                                    ? Colors.black.withValues(alpha: .34)
                                    : const Color(0x180B514A),
                                blurRadius: 28,
                                offset: Offset(0, 12),
                              ),
                            ],
                          ),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              Text(
                                title,
                                style: TextStyle(
                                  color: text,
                                  fontSize: 25,
                                  fontWeight: FontWeight.w800,
                                ),
                              ),
                              const SizedBox(height: 7),
                              Text(
                                subtitle,
                                style: TextStyle(color: muted, height: 1.6),
                              ),
                              const SizedBox(height: 20),
                              child,
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ),

                if (footer != null)
                  Padding(
                    padding: const EdgeInsets.fromLTRB(18, 8, 18, 18),
                    child: footer!,
                  ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
