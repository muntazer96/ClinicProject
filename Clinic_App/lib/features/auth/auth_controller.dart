import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../../core/api_client.dart';
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
  final ApiClient api;
  final FlutterSecureStorage _storage = const FlutterSecureStorage();
  String? _token;
  String? _refreshToken;
  String? _phoneNumber;
  UserProfile? _profile;
  bool loading = false;

  bool get isAuthenticated => _token?.isNotEmpty == true;
  String? get phoneNumber => _phoneNumber;
  UserProfile? get profile => _profile;
  String get displayName => _profile?.name.isNotEmpty == true
      ? _profile!.name
      : _phoneNumber ?? 'زائر';

  Future<void> restoreSession() async {
    _token = await _storage.read(key: _tokenKey);
    _refreshToken = await _storage.read(key: _refreshTokenKey);
    _phoneNumber = await _storage.read(key: _phoneKey);
    api.setToken(_token);
    if (isAuthenticated) {
      await refreshProfile(silent: true);
    }
  }

  Future<void> login(String phoneNumber, String password) async {
    loading = true;
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
    } finally {
      loading = false;
      notifyListeners();
    }
  }

  Future<void> logout() async {
    final currentRefreshToken = _refreshToken;
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
    } catch (_) {
      // Keep the session usable even if profile details fail to load.
    } finally {
      if (!silent) loading = false;
      notifyListeners();
    }
  }

  void setProfile(UserProfile profile) {
    _profile = profile;
    _phoneNumber = profile.phoneNumber;
    _storage.write(key: _phoneKey, value: profile.phoneNumber);
    notifyListeners();
  }
}
