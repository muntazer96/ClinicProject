import 'package:go_router/go_router.dart';

import '../features/auth/auth_controller.dart';
import '../features/auth/screens/email_confirm_screen.dart';
import '../features/auth/screens/forgot_password_screen.dart';
import '../features/auth/screens/login_screen.dart';
import '../features/auth/screens/password_reset_screen.dart';
import '../features/auth/screens/register_screen.dart';
import '../features/directory/screens/doctor_details_screen.dart';
import '../features/directory/screens/search_screen.dart';
import '../features/home/home_screen.dart';
import '../features/home/my_bookings_placeholder.dart';

GoRouter createRouter(AuthController auth) => GoRouter(
      refreshListenable: auth,
      routes: [
        GoRoute(path: '/', builder: (_, __) => const HomeScreen()),
        GoRoute(path: '/search', builder: (_, __) => const SearchScreen()),
        GoRoute(
          path: '/doctors/:doctorId',
          builder: (_, state) => DoctorDetailsScreen(
            doctorId: int.parse(state.pathParameters['doctorId']!),
          ),
        ),
        GoRoute(path: '/login', builder: (_, state) => LoginScreen(redirect: state.uri.queryParameters['redirect'])),
        GoRoute(path: '/register', builder: (_, __) => const RegisterScreen()),
        GoRoute(path: '/forgot-password', builder: (_, __) => const ForgotPasswordScreen()),
        GoRoute(path: '/password-reset', builder: (_, state) => PasswordResetScreen(userId: state.uri.queryParameters['userId'], token: state.uri.queryParameters['token'])),
        GoRoute(path: '/email-confirm', builder: (_, state) => EmailConfirmScreen(userId: state.uri.queryParameters['userId'], token: state.uri.queryParameters['token'], identifier: state.uri.queryParameters['identifier'])),
        GoRoute(path: '/bookings', builder: (_, __) => const MyBookingsPlaceholder()),
      ],
      redirect: (_, state) {
        final authPages = {'/login', '/register', '/forgot-password'};
        if (state.uri.path == '/bookings' && !auth.isAuthenticated) {
          return '/login?redirect=${Uri.encodeComponent(state.uri.toString())}';
        }
        if (auth.isAuthenticated && authPages.contains(state.uri.path)) return '/';
        return null;
      },
    );
