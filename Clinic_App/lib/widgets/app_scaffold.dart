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
    final fallbackInitial = auth.displayName.trim().isEmpty
        ? 'م'
        : String.fromCharCode(auth.displayName.trim().runes.first);
    final selectedIndex = path == '/profile'
        ? 3
        : path == '/bookings'
        ? 2
        : path == '/search' || path.startsWith('/doctors/')
        ? 1
        : 0;

    return Scaffold(
      appBar: AppBar(
        toolbarHeight: 76,
        titleSpacing: 16,
        title: Row(
          children: [
            Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: AppColors.primary,
                borderRadius: BorderRadius.circular(8),
                boxShadow: const [
                  BoxShadow(
                    color: Color(0x22155E75),
                    blurRadius: 16,
                    offset: Offset(0, 8),
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
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    title,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      fontSize: 17,
                      fontWeight: FontWeight.w900,
                    ),
                  ),
                  Text(
                    auth.isAuthenticated
                        ? auth.displayName
                        : 'دليلك الطبي الذكي',
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      fontSize: 11,
                      color: AppColors.muted,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
        actions: [
          IconButton(
            tooltip: 'متابعة حجز زائر',
            onPressed: () => context.go('/guest-booking'),
            icon: const Icon(Icons.manage_search_rounded),
          ),
          IconButton(
            tooltip: auth.isAuthenticated ? 'حسابي' : 'تسجيل الدخول',
            onPressed: () => context.go(
              auth.isAuthenticated ? '/profile' : '/login?redirect=/profile',
            ),
            icon: auth.isAuthenticated
                ? CircleAvatar(
                    radius: 15,
                    backgroundColor: AppColors.softAmber,
                    child: Text(
                      auth.profile?.initials ?? fallbackInitial,
                      style: const TextStyle(
                        color: AppColors.primaryDark,
                        fontSize: 12,
                        fontWeight: FontWeight.w900,
                      ),
                    ),
                  )
                : const Icon(Icons.person_outline_rounded),
          ),
          const SizedBox(width: 4),
        ],
      ),
      body: child,
      bottomNavigationBar: DecoratedBox(
        decoration: const BoxDecoration(
          color: Colors.white,
          boxShadow: [
            BoxShadow(
              color: Color(0x121D4A44),
              blurRadius: 22,
              offset: Offset(0, -7),
            ),
          ],
        ),
        child: NavigationBar(
          selectedIndex: selectedIndex,
          onDestinationSelected: (index) {
            if (index == 0) context.go('/');
            if (index == 1) context.go('/search');
            if (index == 2) {
              context.go(
                auth.isAuthenticated
                    ? '/bookings'
                    : '/login?redirect=/bookings',
              );
            }
            if (index == 3) {
              context.go(
                auth.isAuthenticated ? '/profile' : '/login?redirect=/profile',
              );
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
              label: 'الأطباء',
            ),
            NavigationDestination(
              icon: Icon(Icons.calendar_month_outlined),
              selectedIcon: Icon(Icons.calendar_month_rounded),
              label: 'حجوزاتي',
            ),
            NavigationDestination(
              icon: Icon(Icons.person_outline_rounded),
              selectedIcon: Icon(Icons.person_rounded),
              label: 'حسابي',
            ),
          ],
        ),
      ),
    );
  }
}
