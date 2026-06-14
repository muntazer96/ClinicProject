import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../features/account/change_password_screen.dart';
import '../features/account/edit_name_screen.dart';
import '../features/account/profile_screen.dart';
import '../features/auth/auth_controller.dart';
import '../features/auth/screens/email_confirm_screen.dart';
import '../features/auth/screens/forgot_password_screen.dart';
import '../features/auth/screens/login_screen.dart';
import '../features/auth/screens/password_reset_screen.dart';
import '../features/auth/screens/register_screen.dart';
import '../features/booking/models/booking_models.dart';
import '../features/booking/screens/booking_confirm_screen.dart';
import '../features/booking/screens/booking_screen.dart';
import '../features/booking/screens/guest_booking_screen.dart';
import '../features/booking/screens/my_bookings_screen.dart';
import '../features/booking/screens/otp_screen.dart';
import '../features/booking/screens/review_booking_screen.dart';
import '../features/booking/screens/success_screen.dart';
import '../features/directory/screens/doctor_details_screen.dart';
import '../features/directory/screens/favorite_doctors_screen.dart';
import '../features/directory/screens/search_screen.dart';
import '../features/directory/screens/specializations_screen.dart';
import '../features/doctor/models/doctor_models.dart';
import '../features/doctor/pages/doctor_appointments_screen.dart';
import '../features/doctor/pages/doctor_clinic_form_page.dart';
import '../features/doctor/pages/doctor_clinics_screen.dart';
import '../features/doctor/pages/doctor_features_page.dart';
import '../features/doctor/pages/doctor_home_screen.dart';
import '../features/doctor/pages/doctor_manual_appointment_page.dart';
import '../features/doctor/pages/doctor_notifications_screen.dart';
import '../features/doctor/pages/doctor_offer_form_page.dart';
import '../features/doctor/pages/doctor_offers_screen.dart';
import '../features/doctor/pages/doctor_profile_edit_page.dart';
import '../features/doctor/pages/doctor_profile_screen.dart';
import '../features/doctor/pages/doctor_reviews_screen.dart';
import '../features/doctor/pages/doctor_schedule_day_form_page.dart';
import '../features/doctor/pages/doctor_schedule_exception_form_page.dart';
import '../features/doctor/pages/doctor_schedule_screen.dart';
import '../features/doctor/pages/doctor_subscription_screen.dart';
import '../features/doctor/widgets/doctor_scaffold.dart';
import '../features/home/home_screen.dart';
import '../features/offers/offers_screen.dart';
import '../features/onboarding/onboarding_screen.dart';
import '../features/onboarding/startup_splash_screen.dart';
import '../features/messages/screens/chat_screen.dart';
import '../features/messages/screens/conversations_screen.dart';
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
    GoRoute(path: '/doctor', builder: (_, __) => const DoctorHomeScreen()),
    GoRoute(
      path: '/doctor/appointments',
      builder: (_, __) => const DoctorAppointmentsScreen(),
    ),
    GoRoute(
      path: '/doctor/appointments/manual',
      builder: (_, __) => const DoctorManualAppointmentPage(),
    ),
    GoRoute(
      path: '/doctor/schedule',
      builder: (_, __) => const DoctorScheduleScreen(),
    ),
    GoRoute(
      path: '/doctor/schedule/day',
      builder: (_, state) => state.extra is DoctorAvailability
          ? DoctorScheduleDayFormPage(day: state.extra! as DoctorAvailability)
          : const _MissingBookingData(),
    ),
    GoRoute(
      path: '/doctor/schedule/exception',
      builder: (_, state) {
        final extra = state.extra;
        if (extra is DoctorScheduleExceptionFormArgs) {
          return DoctorScheduleExceptionFormPage(
            clinic: extra.clinic,
            exceptions: extra.exceptions,
          );
        }
        if (extra is DoctorClinic) {
          return DoctorScheduleExceptionFormPage(clinic: extra);
        }
        return const _MissingBookingData();
      },
    ),
    GoRoute(
      path: '/doctor/profile',
      builder: (_, __) => const DoctorProfileScreen(),
    ),
    GoRoute(
      path: '/doctor/profile/edit',
      builder: (_, state) => state.extra is DoctorManageProfile
          ? DoctorProfileEditPage(profile: state.extra! as DoctorManageProfile)
          : const _MissingBookingData(),
    ),
    GoRoute(
      path: '/doctor/profile/edit-name',
      builder: (_, state) => DoctorScaffold(
        title: 'تعديل الاسم',
        showBackButton: true,
        backRoute: '/doctor/profile',
        child: EditNameScreen(
          initialName: state.uri.queryParameters['name'] ?? '',
        ),
      ),
    ),
    GoRoute(
      path: '/doctor/features',
      builder: (_, state) => state.extra is DoctorManageProfile
          ? DoctorFeaturesPage(profile: state.extra! as DoctorManageProfile)
          : const _MissingBookingData(),
    ),
    GoRoute(
      path: '/doctor/clinics',
      builder: (_, __) => const DoctorClinicsScreen(),
    ),
    GoRoute(
      path: '/doctor/clinics/form',
      builder: (_, state) => DoctorClinicFormPage(
        clinic: state.extra is DoctorClinic
            ? state.extra! as DoctorClinic
            : null,
      ),
    ),
    GoRoute(
      path: '/doctor/clinics/:clinicId/schedule',
      builder: (_, state) => DoctorScheduleScreen(
        clinic: state.extra is DoctorClinic
            ? state.extra! as DoctorClinic
            : null,
      ),
    ),
    GoRoute(
      path: '/doctor/offers',
      builder: (_, __) => const DoctorOffersScreen(),
    ),
    GoRoute(
      path: '/doctor/offers/form',
      builder: (_, state) => DoctorOfferFormPage(
        offer: state.extra is DoctorOfferManage
            ? state.extra! as DoctorOfferManage
            : null,
      ),
    ),
    GoRoute(
      path: '/doctor/reviews',
      builder: (_, __) => const DoctorManageReviewsScreen(),
    ),
    GoRoute(
      path: '/doctor/subscription',
      builder: (_, __) => const DoctorSubscriptionScreen(),
    ),
    GoRoute(
      path: '/doctor/messages',
      builder: (_, __) => const DoctorScaffold(
        title: 'الرسائل',
        showBackButton: true,
        child: ConversationsScreen(),
      ),
    ),
    GoRoute(
      path: '/doctor/messages/:otherUserId',
      builder: (_, state) {
        final otherUserId = state.pathParameters['otherUserId'] ?? '';
        final otherUserName = state.extra as String? ?? 'المستخدم';
        return ChatScreen(
          otherUserId: otherUserId,
          otherUserName: otherUserName,
        );
      },
    ),
    GoRoute(
      path: '/doctor/notifications',
      builder: (_, __) => const DoctorNotificationsScreen(),
    ),
    GoRoute(path: '/offers', builder: (_, __) => const OffersScreen()),
    GoRoute(
      path: '/messages',
      builder: (_, __) => const AppScaffold(
        title: 'الرسائل',
        child: ConversationsScreen(),
      ),
    ),
    GoRoute(
      path: '/messages/:otherUserId',
      builder: (_, state) {
        final otherUserId = state.pathParameters['otherUserId'] ?? '';
        final otherUserName = state.extra as String? ?? 'المستخدم';
        return ChatScreen(
          otherUserId: otherUserId,
          otherUserName: otherUserName,
        );
      },
    ),
    GoRoute(
      path: '/favorites',
      builder: (_, __) => const FavoriteDoctorsScreen(),
    ),
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
      builder: (_, state) {
        final doctorId = _tryParsePathParam(state, 'doctorId');
        if (doctorId == null) return const _MissingBookingData();
        return DoctorDetailsScreen(
          doctorId: doctorId,
          source: state.uri.queryParameters['source'],
          offerId: int.tryParse(state.uri.queryParameters['offerId'] ?? ''),
        );
      },
    ),
    GoRoute(
      path: '/doctors/:doctorId/reviews',
      builder: (_, state) {
        final doctorId = _tryParsePathParam(state, 'doctorId');
        if (doctorId == null) return const _MissingBookingData();
        return DoctorReviewsScreen(
          doctorId: doctorId,
          doctorName: state.uri.queryParameters['doctorName'] ?? 'الطبيب',
        );
      },
    ),
    GoRoute(
      path: '/book/:doctorId/:clinicId',
      builder: (_, state) {
        final doctorId = _tryParsePathParam(state, 'doctorId');
        final clinicId = _tryParsePathParam(state, 'clinicId');
        if (doctorId == null || clinicId == null) {
          return const _MissingBookingData();
        }
        return BookingScreen(
          doctorId: doctorId,
          clinicId: clinicId,
          doctorName: state.uri.queryParameters['doctorName'] ?? 'الطبيب',
          clinicName: state.uri.queryParameters['clinicName'] ?? 'العيادة',
          source: state.uri.queryParameters['source'] ?? 'profile',
          offerId: int.tryParse(state.uri.queryParameters['offerId'] ?? ''),
        );
      },
    ),
    GoRoute(
      path: '/booking/otp',
      builder: (_, state) => state.extra is OtpScreenArgs
          ? OtpScreen(args: state.extra! as OtpScreenArgs)
          : const _MissingBookingData(),
    ),
    GoRoute(
      path: '/booking/confirm',
      builder: (_, state) => state.extra is BookingConfirmArgs
          ? BookingConfirmScreen(args: state.extra! as BookingConfirmArgs)
          : const _MissingBookingData(),
    ),
    GoRoute(
      path: '/booking/review',
      builder: (_, state) => state.extra is BookingDetails
          ? ReviewBookingScreen(booking: state.extra! as BookingDetails)
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
        showBackButton: true,
        backRoute: '/profile',
        child: ChangePasswordScreen(),
      ),
    ),
    GoRoute(
      path: '/profile/edit-name',
      builder: (_, state) => AppScaffold(
        title: 'تعديل الاسم',
        child: EditNameScreen(
          initialName: state.uri.queryParameters['name'] ?? '',
        ),
      ),
    ),
    GoRoute(
      path: '/profile/confirm-phone',
      builder: (_, __) => auth.isDoctor
          ? const DoctorScaffold(
              title: 'تأكيد الهاتف',
              showBackButton: true,
              backRoute: '/doctor/profile',
              child: ConfirmPhoneScreen(),
            )
          : const AppScaffold(
              title: 'تأكيد الهاتف',
              child: ConfirmPhoneScreen(),
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
      '/favorites',
      '/profile',
      '/profile/change-password',
      '/profile/edit-name',
      '/profile/confirm-phone',
      '/messages',
    };
    final doctorPage =
        state.uri.path == '/doctor' || state.uri.path.startsWith('/doctor/');
    final phoneConfirmationPage = state.uri.path == '/profile/confirm-phone';
    if ((protectedPages.contains(state.uri.path) || doctorPage) &&
        !auth.isAuthenticated) {
      return '/login?redirect=${Uri.encodeComponent(state.uri.toString())}';
    }
    if (doctorPage && !auth.isDoctor) return '/';
    if (auth.isDoctor &&
        (state.uri.path == '/' ||
            (protectedPages.contains(state.uri.path) &&
                !phoneConfirmationPage) ||
            authPages.contains(state.uri.path))) {
      return '/doctor';
    }
    if (auth.isAuthenticated && authPages.contains(state.uri.path)) return '/';
    return null;
  },
);

int? _tryParsePathParam(GoRouterState state, String key) =>
    int.tryParse(state.pathParameters[key] ?? '');

class _MissingBookingData extends StatelessWidget {
  const _MissingBookingData({this.redirectTo = '/search'});

  final String redirectTo;

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
              onPressed: () => context.go(redirectTo),
              child: const Text('عودة'),
            ),
          ],
        ),
      ),
    ),
  );
}
