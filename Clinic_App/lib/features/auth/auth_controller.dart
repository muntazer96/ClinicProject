import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../../core/api_client.dart';

class AuthController extends ChangeNotifier {
  AuthController(this.api) {
    api.onUnauthorized = logout;
  }

  static const _tokenKey = 'clinic_app_token';
  static const _phoneKey = 'clinic_app_phone';
  final ApiClient api;
  final FlutterSecureStorage _storage = const FlutterSecureStorage();
  String? _token;
  String? _phoneNumber;
  bool loading = false;

  bool get isAuthenticated => _token?.isNotEmpty == true;
  String? get phoneNumber => _phoneNumber;

  Future<void> restoreSession() async {
    _token = await _storage.read(key: _tokenKey);
    _phoneNumber = await _storage.read(key: _phoneKey);
    api.setToken(_token);
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
      if (token == null || token.isEmpty) {
        throw Exception('لم يرجع الخادم رمز دخول صالحاً.');
      }
      _token = token;
      _phoneNumber = phoneNumber.trim();
      api.setToken(token);
      await _storage.write(key: _tokenKey, value: token);
      await _storage.write(key: _phoneKey, value: _phoneNumber);
    } finally {
      loading = false;
      notifyListeners();
    }
  }

  Future<void> logout() async {
    _token = null;
    _phoneNumber = null;
    api.setToken(null);
    await _storage.delete(key: _tokenKey);
    await _storage.delete(key: _phoneKey);
    notifyListeners();
  }
}
