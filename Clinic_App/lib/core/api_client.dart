import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';

class ApiClient {
  static const _baseUrl = String.fromEnvironment(
    'API_BASE_URL',

    //defaultValue: 'https://localhost:7136/api',
    defaultValue: 'http://localhost:8082/api',
    //defaultValue: 'http://192.168.1.102:8082/api',
    //defaultValue: 'http://192.168.100.7:8082/api',
    //defaultValue: 'http://192.174.0.120:81/api',
  );

  ApiClient()
    : dio = Dio(
        BaseOptions(
          baseUrl: _baseUrl,
          connectTimeout: const Duration(seconds: 15),
          receiveTimeout: const Duration(seconds: 15),
        ),
      ) {
    dio.interceptors.add(
      InterceptorsWrapper(
        onResponse: (response, handler) {
          markServerAvailable();
          handler.next(response);
        },
        onError: (error, handler) async {
          final request = error.requestOptions;
          final isConnError = _isConnectivityError(error);
          final alreadyRetriedConn = request.extra['retriedConn'] == true;
          if (isConnError && !alreadyRetriedConn) {
            debugPrint(
              '[ApiClient] Retrying ${request.method} ${request.uri} due to ${error.type}',
            );
            request.extra['retriedConn'] = true;
            try {
              final response = await Dio(dio.options).fetch(request);
              markServerAvailable();
              handler.resolve(response);
              return;
            } catch (_) {}
            markServerUnavailable();
          }
          final alreadyRetried = request.extra['retriedAfterRefresh'] == true;
          if (error.response?.statusCode == 401 && !alreadyRetried) {
            final refreshed = await _refreshAccessToken();
            if (refreshed != null) {
              request.extra['retriedAfterRefresh'] = true;
              request.headers['Authorization'] = 'Bearer $refreshed';
              try {
                final response = await dio.fetch(request);
                handler.resolve(response);
                return;
              } catch (_) {}
            }
          }
          if (error.response?.statusCode == 401) {
            await onUnauthorized?.call();
          }
          handler.next(error);
        },
      ),
    );
  }

  final Dio dio;
  static final ValueNotifier<bool> connectionAvailable = ValueNotifier(true);
  Future<void> Function()? onUnauthorized;
  Future<String?> Function()? getRefreshToken;
  Future<void> Function(String token, String refreshToken)? onTokensRefreshed;

  static void markServerAvailable() {
    if (!connectionAvailable.value) connectionAvailable.value = true;
  }

  static void markServerUnavailable() {
    if (connectionAvailable.value) connectionAvailable.value = false;
  }

  static Future<bool> checkServerAvailability() async {
    try {
      final response = await Dio(
        BaseOptions(
          baseUrl: _baseUrl,
          connectTimeout: const Duration(seconds: 8),
          receiveTimeout: const Duration(seconds: 8),
          sendTimeout: const Duration(seconds: 8),
        ),
      ).get('/Health');
      final available = (response.statusCode ?? 500) < 500;
      if (available) {
        markServerAvailable();
      } else {
        markServerUnavailable();
      }
      return available;
    } catch (error) {
      if (error is DioException) {
        debugPrint('[HealthCheck] ${error.type}: ${error.requestOptions.uri}');
        if (_isConnectivityError(error)) {
          markServerUnavailable();
        }
      }
      return false;
    }
  }

  static String doctorImageUrl(String imageName) {
    final apiUri = Uri.parse(_baseUrl);
    return apiUri
        .replace(path: '/DoctorImage/$imageName', query: null, fragment: null)
        .toString();
  }

  static String userImageUrl(String imageName) {
    final apiUri = Uri.parse(_baseUrl);
    return apiUri
        .replace(
          path: '/UserImgProfile/$imageName',
          query: null,
          fragment: null,
        )
        .toString();
  }

  void setToken(String? token) {
    if (token == null || token.isEmpty) {
      dio.options.headers.remove('Authorization');
    } else {
      dio.options.headers['Authorization'] = 'Bearer $token';
    }
  }

  Future<String?> _refreshAccessToken() async {
    final refreshToken = await getRefreshToken?.call();
    if (refreshToken == null || refreshToken.isEmpty) return null;
    try {
      final response = await dio.post(
        '/User/refresh',
        data: {'refreshToken': refreshToken},
        options: Options(extra: {'retriedAfterRefresh': true}),
      );
      final data = response.data['data'];
      final token = data?['token'] as String?;
      final newRefreshToken = data?['refreshToken'] as String?;
      if (token == null ||
          token.isEmpty ||
          newRefreshToken == null ||
          newRefreshToken.isEmpty) {
        return null;
      }
      setToken(token);
      await onTokensRefreshed?.call(token, newRefreshToken);
      return token;
    } catch (_) {
      return null;
    }
  }

  static bool _isConnectivityError(DioException error) {
    return error.type == DioExceptionType.connectionTimeout ||
        error.type == DioExceptionType.receiveTimeout ||
        error.type == DioExceptionType.sendTimeout ||
        error.type == DioExceptionType.connectionError;
  }

  static String errorMessage(Object error) {
    if (error is DioException) {
      final data = error.response?.data;
      if (data is Map && data['message'] is String) return data['message'];
      final statusCode = error.response?.statusCode;
      if (statusCode == 400) {
        return 'البيانات غير مكتملة أو غير صحيحة.';
      }
      if (statusCode == 401) {
        return 'انتهت الجلسة. سجل الدخول مرة أخرى.';
      }
      if (statusCode == 403) {
        return 'لا تملك صلاحية تنفيذ هذه العملية.';
      }
      if (statusCode == 404) {
        return 'لم يتم العثور على البيانات المطلوبة.';
      }
      if (statusCode != null && statusCode >= 500) {
        return 'حدث خلل في الخادم. حاول مرة أخرى بعد قليل.';
      }
      if (error.type == DioExceptionType.connectionTimeout ||
          error.type == DioExceptionType.receiveTimeout ||
          error.type == DioExceptionType.sendTimeout) {
        return 'الاتصال بطيء. تحقق من الإنترنت وحاول مرة أخرى.';
      }
      if (error.type == DioExceptionType.connectionError) {
        return 'تعذر الاتصال بالخادم. تحقق من الإنترنت أو عنوان الخادم.';
      }
      return error.message ?? 'تعذر الاتصال بالخادم.';
    }
    return 'تعذر تنفيذ العملية. حاول مرة أخرى.';
  }
}
