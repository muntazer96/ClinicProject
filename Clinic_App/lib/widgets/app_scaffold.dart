import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../core/app_theme.dart';
import '../features/auth/auth_controller.dart';

class AppScaffold extends StatelessWidget {
  const AppScaffold({super.key, required this.child, this.title = 'عيادتي'});
  final Widget child;
  final String title;

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthController>();
    final path = GoRouterState.of(context).uri.path;
    return Scaffold(
      appBar: AppBar(
        toolbarHeight: 76,
        title: Row(
          children: [
            Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                gradient: const LinearGradient(
                  colors: [AppColors.primary, AppColors.primaryDark],
                ),
                borderRadius: BorderRadius.circular(15),
                boxShadow: const [
                  BoxShadow(
                    color: Color(0x334267F5),
                    blurRadius: 14,
                    offset: Offset(0, 6),
                  ),
                ],
              ),
              child: const Icon(
                Icons.medical_services_rounded,
                color: Colors.white,
                size: 23,
              ),
            ),
            const SizedBox(width: 10),
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: const TextStyle(
                    fontSize: 17,
                    fontWeight: FontWeight.w900,
                  ),
                ),
                const Text(
                  'دليلك الطبي',
                  style: TextStyle(fontSize: 11, color: AppColors.muted),
                ),
              ],
            ),
          ],
        ),
        actions: [
          if (!auth.isAuthenticated)
            IconButton(
              tooltip: 'متابعة حجز زائر',
              onPressed: () => context.go('/guest-booking'),
              icon: const Icon(Icons.manage_search_rounded),
            ),
          if (auth.isAuthenticated)
            IconButton(
              tooltip: 'حجوزاتي',
              onPressed: () => context.go('/bookings'),
              icon: const Icon(Icons.calendar_month_rounded),
            ),
          if (auth.isAuthenticated)
            IconButton(
              tooltip: 'تسجيل الخروج',
              onPressed: () async {
                await auth.logout();
                if (context.mounted) context.go('/');
              },
              icon: const Icon(Icons.logout_rounded),
            )
          else
            Padding(
              padding: const EdgeInsets.only(left: 8),
              child: TextButton(
                onPressed: () => context.go('/login'),
                child: const Text(
                  'دخول',
                  style: TextStyle(fontWeight: FontWeight.w700),
                ),
              ),
            ),
        ],
      ),
      body: child,
      bottomNavigationBar: DecoratedBox(
        decoration: const BoxDecoration(
          color: Colors.white,
          boxShadow: [
            BoxShadow(
              color: Color(0x120F1F4B),
              blurRadius: 22,
              offset: Offset(0, -7),
            ),
          ],
        ),
        child: NavigationBar(
          selectedIndex: path == '/bookings'
              ? 2
              : path == '/search' || path.startsWith('/doctors/')
              ? 1
              : 0,
          onDestinationSelected: (index) {
            if (index == 0) context.go('/');
            if (index == 1) context.go('/search');
            if (index == 2) {
              context.go(auth.isAuthenticated ? '/bookings' : '/login');
            }
          },
          destinations: const [
            NavigationDestination(
              icon: Icon(Icons.home_outlined),
              selectedIcon: Icon(Icons.home_rounded),
              label: 'الرئيسية',
            ),
            NavigationDestination(
              icon: Icon(Icons.search_rounded),
              label: 'البحث',
            ),
            NavigationDestination(
              icon: Icon(Icons.calendar_month_outlined),
              selectedIcon: Icon(Icons.calendar_month_rounded),
              label: 'حجوزاتي',
            ),
          ],
        ),
      ),
    );
  }
}
