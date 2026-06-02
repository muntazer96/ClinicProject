import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

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
        toolbarHeight: 70,
        title: Row(
          children: [
            Container(
              width: 42,
              height: 42,
              decoration: BoxDecoration(
                color: Theme.of(context).colorScheme.primary,
                borderRadius: BorderRadius.circular(14),
              ),
              child: const Icon(
                Icons.local_hospital_outlined,
                color: Colors.white,
              ),
            ),
            const SizedBox(width: 10),
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: const TextStyle(
                    fontSize: 18,
                    fontWeight: FontWeight.w800,
                  ),
                ),
                const Text(
                  'دليلك الطبي',
                  style: TextStyle(fontSize: 11, color: Color(0xFF7A918E)),
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
              icon: const Icon(Icons.manage_search_outlined),
            ),
          if (auth.isAuthenticated)
            IconButton(
              tooltip: 'حجوزاتي',
              onPressed: () => context.go('/bookings'),
              icon: const Icon(Icons.calendar_month_outlined),
            ),
          if (auth.isAuthenticated)
            IconButton(
              tooltip: 'تسجيل الخروج',
              onPressed: () async {
                await auth.logout();
                if (context.mounted) context.go('/');
              },
              icon: const Icon(Icons.logout),
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
      bottomNavigationBar: NavigationBar(
        height: 66,
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
            selectedIcon: Icon(Icons.home),
            label: 'الرئيسية',
          ),
          NavigationDestination(icon: Icon(Icons.search), label: 'البحث'),
          NavigationDestination(
            icon: Icon(Icons.calendar_month_outlined),
            label: 'حجوزاتي',
          ),
        ],
      ),
    );
  }
}
