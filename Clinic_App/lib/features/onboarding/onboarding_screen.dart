import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:go_router/go_router.dart';

import '../../core/app_theme.dart';
import '../../widgets/app_logo.dart';
import 'startup_splash_screen.dart';

class OnboardingScreen extends StatefulWidget {
  const OnboardingScreen({super.key});

  @override
  State<OnboardingScreen> createState() => _OnboardingScreenState();
}

class _OnboardingScreenState extends State<OnboardingScreen> {
  final _controller = PageController();
  final _storage = const FlutterSecureStorage();
  int _page = 0;

  final _items = const [
    _OnboardingItem(
      icon: Icons.medical_services_outlined,
      title: 'اختار الطبيب المناسب',
      text: 'ابحث حسب الاختصاص والمحافظة وشوف تفاصيل العيادات بسرعة.',
    ),
    _OnboardingItem(
      icon: Icons.event_available_rounded,
      title: 'احجز دورك بسهولة',
      text: 'حدد اليوم المناسب وتابع رقم الدور من داخل التطبيق.',
    ),
    _OnboardingItem(
      icon: Icons.verified_user_outlined,
      title: 'حساب موثق وآمن',
      text: 'أكد الهاتف والبريد حتى تقدر تحجز وتدير مواعيدك بثقة.',
    ),
  ];

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  Future<void> _finish() async {
    await _storage.write(
      key: StartupSplashScreen.onboardingSeenKey,
      value: 'true',
    );
    if (mounted) context.go('/');
  }

  void _next() {
    if (_page == _items.length - 1) {
      _finish();
      return;
    }
    _controller.nextPage(
      duration: const Duration(milliseconds: 260),
      curve: Curves.easeOut,
    );
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    body: SafeArea(
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 10, 16, 0),
            child: Row(
              children: [
                const AppLogo(size: 42),
                const SizedBox(width: 8),
                const Expanded(
                  child: Text(
                    'عيادتي',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.w900),
                  ),
                ),
                TextButton(onPressed: _finish, child: const Text('تخطي')),
              ],
            ),
          ),
          Expanded(
            child: PageView.builder(
              controller: _controller,
              itemCount: _items.length,
              onPageChanged: (value) => setState(() => _page = value),
              itemBuilder: (context, index) =>
                  _OnboardingPage(item: _items[index]),
            ),
          ),
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 0, 16, 22),
            child: Column(
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: List.generate(
                    _items.length,
                    (index) => AnimatedContainer(
                      duration: const Duration(milliseconds: 180),
                      width: _page == index ? 28 : 8,
                      height: 8,
                      margin: const EdgeInsets.symmetric(horizontal: 3),
                      decoration: BoxDecoration(
                        color: _page == index
                            ? AppColors.primary
                            : AppColors.border,
                        borderRadius: BorderRadius.circular(8),
                      ),
                    ),
                  ),
                ),
                const SizedBox(height: 18),
                FilledButton.icon(
                  onPressed: _next,
                  icon: Icon(
                    _page == _items.length - 1
                        ? Icons.check_rounded
                        : Icons.arrow_forward_rounded,
                  ),
                  label: Text(
                    _page == _items.length - 1 ? 'ابدأ الآن' : 'التالي',
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    ),
  );
}

class _OnboardingItem {
  const _OnboardingItem({
    required this.icon,
    required this.title,
    required this.text,
  });

  final IconData icon;
  final String title;
  final String text;
}

class _OnboardingPage extends StatelessWidget {
  const _OnboardingPage({required this.item});
  final _OnboardingItem item;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(horizontal: 26),
    child: Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Container(
          width: 146,
          height: 146,
          decoration: BoxDecoration(
            color: AppColors.softBlue,
            borderRadius: BorderRadius.circular(8),
          ),
          child: Icon(item.icon, size: 68, color: AppColors.primary),
        ),
        const SizedBox(height: 26),
        Text(
          item.title,
          textAlign: TextAlign.center,
          style: const TextStyle(fontSize: 25, fontWeight: FontWeight.w900),
        ),
        const SizedBox(height: 10),
        Text(
          item.text,
          textAlign: TextAlign.center,
          style: const TextStyle(
            color: AppColors.muted,
            fontSize: 14,
            height: 1.7,
          ),
        ),
      ],
    ),
  );
}
