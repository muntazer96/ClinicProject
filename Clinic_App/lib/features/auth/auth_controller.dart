import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:google_sign_in/google_sign_in.dart';

import '../../core/api_client.dart';
import '../../core/push_notification_service.dart';
import '../account/profile_service.dart';

class AuthController extends ChangeNotifier {
  AuthController(this.api) {
    api.onUnauthorized = logout;
    api.getRefreshToken = () => _storage.read(key: _refreshTokenKey);
    api.onTokensRefreshed = _storeTokens;
  }

  static const _tokenKey = 'clinic_app_token';
  static const _refreshTokenKey = 'clinic_app_refresh_token';
  static const _phoneKey = 'clinic_app_phone';
  static const _googleServerClientId = String.fromEnvironment(
    'GOOGLE_SERVER_CLIENT_ID',
    defaultValue:
        '376580756051-kkide42qu74rqvo3fis1p1n28qeh3g50.apps.googleusercontent.com',
  );
  final ApiClient api;
  final FlutterSecureStorage _storage = const FlutterSecureStorage();
  String? _token;
  String? _refreshToken;
  String? _phoneNumber;
  UserProfile? _profile;
  bool loading = false;
  String? profileError;
  bool _restoring = false;
  bool _googleInitialized = false;
  StreamSubscription<GoogleSignInAuthenticationEvent>? _googleAuthSubscription;

  bool get isAuthenticated => _token?.isNotEmpty == true;
  bool get isDoctor => _profile?.roleName == 'DoctorUser';
  bool get isRestoring => _restoring;
  bool get needsPhoneSetup =>
      isAuthenticated && (_profile?.phoneNumber.trim().isEmpty ?? false);
  bool get needsPhoneConfirmation =>
      isAuthenticated &&
      (_profile?.phoneNumber.trim().isNotEmpty ?? false) &&
      _profile?.phoneConfirmed != true;
  String? get phoneNumber => _phoneNumber;
  UserProfile? get profile => _profile;
  String get displayName => _profile?.name.isNotEmpty == true
      ? _profile!.name
      : _phoneNumber ?? 'زائر';

  Future<void> restoreSession() async {
    _restoring = true;
    _token = await _storage.read(key: _tokenKey);
    _refreshToken = await _storage.read(key: _refreshTokenKey);
    _phoneNumber = await _storage.read(key: _phoneKey);
    api.setToken(_token);
    if (isAuthenticated) {
      await refreshProfile(silent: true);
      await _registerPushToken();
    }
    _restoring = false;
  }

  Future<void> login(String phoneNumber, String password) async {
    if (loading) return;
    loading = true;
    profileError = null;
    notifyListeners();
    try {
      final response = await api.dio.post(
        '/User/signin',
        data: {'phoneNumber': phoneNumber, 'password': password},
      );
      final token = response.data['data']?['token'] as String?;
      final refreshToken = response.data['data']?['refreshToken'] as String?;
      if (token == null || token.isEmpty) {
        throw Exception('لم يرجع الخادم رمز دخول صالحاً.');
      }
      await _storeTokens(token, refreshToken);
      _phoneNumber = phoneNumber.trim();
      await _storage.write(key: _phoneKey, value: _phoneNumber);
      await refreshProfile(silent: true);
      await _registerPushToken();
    } finally {
      loading = false;
      notifyListeners();
    }
  }

  Future<void> completeLoginFromTokens({
    required String phoneNumber,
    required String token,
    String? refreshToken,
  }) async {
    if (token.isEmpty) {
      throw Exception('لم يرجع الخادم رمز دخول صالحاً.');
    }
    loading = true;
    profileError = null;
    notifyListeners();
    try {
      await _storeTokens(token, refreshToken);
      _phoneNumber = phoneNumber.trim();
      await _storage.write(key: _phoneKey, value: _phoneNumber);
      await refreshProfile(silent: true);
      await _registerPushToken();
    } finally {
      loading = false;
      notifyListeners();
    }
  }

  Future<void> loginWithGoogle() async {
    if (loading) return;
    loading = true;
    profileError = null;
    notifyListeners();
    try {
      final googleSignIn = await ensureGoogleSignInInitialized();

      if (!googleSignIn.supportsAuthenticate()) {
        throw Exception('تسجيل الدخول بواسطة Google غير مدعوم على هذا الجهاز.');
      }

      final googleAccount = await googleSignIn.authenticate();
      final idToken = googleAccount.authentication.idToken;
      await _completeGoogleLogin(idToken);
    } finally {
      loading = false;
      notifyListeners();
    }
  }

  Future<GoogleSignIn> ensureGoogleSignInInitialized() async {
    if (_googleServerClientId.isEmpty) {
      throw Exception(
        'تسجيل الدخول بواسطة Google غير مهيأ في التطبيق. شغل التطبيق مع GOOGLE_SERVER_CLIENT_ID.',
      );
    }

    final googleSignIn = GoogleSignIn.instance;
    if (!_googleInitialized) {
      await googleSignIn.initialize(
        clientId: kIsWeb ? _googleServerClientId : null,
        serverClientId: kIsWeb ? null : _googleServerClientId,
      );
      _googleInitialized = true;
    }

    if (kIsWeb && _googleAuthSubscription == null) {
      _googleAuthSubscription = googleSignIn.authenticationEvents.listen(
        (event) async {
          if (event is GoogleSignInAuthenticationEventSignIn) {
            loading = true;
            profileError = null;
            notifyListeners();
            try {
              await _completeGoogleLogin(event.user.authentication.idToken);
            } catch (error) {
              profileError = ApiClient.errorMessage(error);
            } finally {
              loading = false;
              notifyListeners();
            }
          }
        },
        onError: (Object error) {
          profileError = ApiClient.errorMessage(error);
          notifyListeners();
        },
      );
    }

    return googleSignIn;
  }

  Future<void> _completeGoogleLogin(String? idToken) async {
    if (idToken == null || idToken.isEmpty) {
      throw Exception('تعذر الحصول على رمز Google صالح.');
    }

    final response = await api.dio.post(
      '/User/google-signin',
      data: {'idToken': idToken},
    );
    final token = response.data['data']?['token'] as String?;
    final refreshToken = response.data['data']?['refreshToken'] as String?;
    if (token == null || token.isEmpty) {
      throw Exception('لم يرجع الخادم رمز دخول صالحاً.');
    }

    await _storeTokens(token, refreshToken);
    await _storage.delete(key: _phoneKey);
    await refreshProfile(silent: true);
    await _registerPushToken();
  }

  Future<void> logout() async {
    final currentRefreshToken = _refreshToken;
    if (_googleInitialized) {
      try {
        await GoogleSignIn.instance.signOut();
      } catch (_) {}
    }
    _token = null;
    _refreshToken = null;
    _phoneNumber = null;
    _profile = null;
    api.setToken(null);
    await _storage.delete(key: _tokenKey);
    await _storage.delete(key: _refreshTokenKey);
    await _storage.delete(key: _phoneKey);
    if (currentRefreshToken?.isNotEmpty == true) {
      try {
        await PushNotificationService(api).unregisterCurrentDevice();
        await api.dio.post(
          '/User/logout',
          data: {'refreshToken': currentRefreshToken},
        );
      } catch (_) {}
    }
    notifyListeners();
  }

  Future<void> _storeTokens(String token, String? refreshToken) async {
    _token = token;
    _refreshToken = refreshToken;
    api.setToken(token);
    await _storage.write(key: _tokenKey, value: token);
    if (refreshToken?.isNotEmpty == true) {
      await _storage.write(key: _refreshTokenKey, value: refreshToken);
    }
    notifyListeners();
  }

  Future<void> refreshProfile({bool silent = false}) async {
    if (!isAuthenticated) return;
    if (!silent) {
      loading = true;
      notifyListeners();
    }
    try {
      _profile = await ProfileService(api).getProfile();
      _phoneNumber = _profile?.phoneNumber ?? _phoneNumber;
      if (_phoneNumber?.isNotEmpty == true) {
        await _storage.write(key: _phoneKey, value: _phoneNumber);
      }
      profileError = null;
    } catch (e) {
      profileError = 'تعذر تحميل بيانات الملف الشخصي.';
    } finally {
      if (!silent) loading = false;
      notifyListeners();
    }
  }

  Future<void> _registerPushToken() async {
    try {
      await PushNotificationService(api).registerCurrentDevice();
    } catch (_) {}
  }

  void setProfile(UserProfile profile) {
    _profile = profile;
    _phoneNumber = profile.phoneNumber;
    _storage.write(key: _phoneKey, value: profile.phoneNumber);
    notifyListeners();
  }

  @override
  void dispose() {
    _googleAuthSubscription?.cancel();
    super.dispose();
  }
}
