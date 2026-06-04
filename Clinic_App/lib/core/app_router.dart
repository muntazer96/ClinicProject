import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../features/account/change_password_screen.dart';
import '../features/account/profile_screen.dart';
import '../features/auth/auth_controller.dart';
import '../features/auth/screens/email_confirm_screen.dart';
import '../features/auth/screens/forgot_password_screen.dart';
import '../features/auth/screens/login_screen.dart';
import '../features/auth/screens/password_reset_screen.dart';
import '../features/auth/screens/register_screen.dart';
import '../features/booking/screens/booking_screen.dart';
import '../features/booking/screens/guest_booking_screen.dart';
import '../features/booking/screens/my_bookings_screen.dart';
import '../features/booking/screens/otp_screen.dart';
import '../features/booking/screens/success_screen.dart';
import '../features/directory/screens/doctor_details_screen.dart';
import '../features/directory/screens/search_screen.dart';
import '../features/directory/screens/specializations_screen.dart';
import '../features/home/home_screen.dart';
import '../features/onboarding/onboarding_screen.dart';
import '../features/onboarding/startup_splash_screen.dart';
import '../features/reviews/screens/doctor_reviews_screen.dart';
import '../widgets/app_scaffold.dart';
import 'app_theme.dart';

GoRouter createRouter(AuthController auth) => GoRouter(
  initialLocation: '/splash',
  refreshListenable: auth,
  routes: [
    GoRoute(path: '/splash', builder: (_, __) => const StartupSplashScreen()),
    GoRoute(path: '/onboarding', builder: (_, __) => const OnboardingScreen()),
    GoRoute(path: '/', builder: (_, __) => const HomeScreen()),
    GoRoute(
      path: '/search',
      builder: (_, state) => SearchScreen(
        initialSpecialization: int.tryParse(
          state.uri.queryParameters['specialization'] ?? '',
        ),
      ),
    ),
    GoRoute(
      path: '/specializations',
      builder: (_, __) => const SpecializationsScreen(),
    ),
    GoRoute(
      path: '/doctors/:doctorId',
      builder: (_, state) => DoctorDetailsScreen(
        doctorId: int.parse(state.pathParameters['doctorId']!),
      ),
    ),
    GoRoute(
      path: '/doctors/:doctorId/reviews',
      builder: (_, state) => DoctorReviewsScreen(
        doctorId: int.parse(state.pathParameters['doctorId']!),
        doctorName: state.uri.queryParameters['doctorName'] ?? 'الطبيب',
      ),
    ),
    GoRoute(
      path: '/book/:doctorId/:clinicId',
      builder: (_, state) => BookingScreen(
        doctorId: int.parse(state.pathParameters['doctorId']!),
        clinicId: int.parse(state.pathParameters['clinicId']!),
        doctorName: state.uri.queryParameters['doctorName'] ?? 'الطبيب',
        clinicName: state.uri.queryParameters['clinicName'] ?? 'العيادة',
      ),
    ),
    GoRoute(
      path: '/booking/otp',
      builder: (_, state) => state.extra is OtpScreenArgs
          ? OtpScreen(args: state.extra! as OtpScreenArgs)
          : const _MissingBookingData(),
    ),
    GoRoute(
      path: '/booking/success',
      builder: (_, state) => state.extra is BookingSuccessArgs
          ? BookingSuccessScreen(args: state.extra! as BookingSuccessArgs)
          : const _MissingBookingData(),
    ),
    GoRoute(
      path: '/login',
      builder: (_, state) =>
          LoginScreen(redirect: state.uri.queryParameters['redirect']),
    ),
    GoRoute(path: '/register', builder: (_, __) => const RegisterScreen()),
    GoRoute(
      path: '/forgot-password',
      builder: (_, __) => const ForgotPasswordScreen(),
    ),
    GoRoute(
      path: '/password-reset',
      builder: (_, state) => PasswordResetScreen(
        userId: state.uri.queryParameters['userId'],
        token: state.uri.queryParameters['token'],
      ),
    ),
    GoRoute(
      path: '/email-confirm',
      builder: (_, state) => EmailConfirmScreen(
        userId: state.uri.queryParameters['userId'],
        token: state.uri.queryParameters['token'],
        identifier: state.uri.queryParameters['identifier'],
      ),
    ),
    GoRoute(path: '/bookings', builder: (_, __) => const MyBookingsScreen()),
    GoRoute(
      path: '/profile',
      builder: (_, __) =>
          const AppScaffold(title: 'حسابي', child: ProfileScreen()),
    ),
    GoRoute(
      path: '/profile/change-password',
      builder: (_, __) => const AppScaffold(
        title: 'تغيير كلمة المرور',
        child: ChangePasswordScreen(),
      ),
    ),
    GoRoute(
      path: '/guest-booking',
      builder: (_, __) => const GuestBookingScreen(),
    ),
  ],
  redirect: (_, state) {
    final authPages = {'/login', '/register', '/forgot-password'};
    if (state.uri.path == '/splash' || state.uri.path == '/onboarding') {
      return null;
    }
    final protectedPages = {
      '/bookings',
      '/profile',
      '/profile/change-password',
    };
    if (protectedPages.contains(state.uri.path) && !auth.isAuthenticated) {
      return '/login?redirect=${Uri.encodeComponent(state.uri.toString())}';
    }
    if (auth.isAuthenticated && authPages.contains(state.uri.path)) return '/';
    return null;
  },
);

class _MissingBookingData extends StatelessWidget {
  const _MissingBookingData();

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('الحجز')),
    body: Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.info_outline, size: 48, color: AppColors.muted),
            const SizedBox(height: 10),
            const Text('لا توجد بيانات حجز لعرضها.'),
            TextButton(
              onPressed: () => context.go('/search'),
              child: const Text('العودة إلى البحث'),
            ),
          ],
        ),
      ),
    ),
  );
}
