import 'package:dio/dio.dart';

class ApiClient {
  static const _baseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'https://localhost:7136/api',
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
        onError: (error, handler) async {
          if (error.response?.statusCode == 401) {
            await onUnauthorized?.call();
          }
          handler.next(error);
        },
      ),
    );
  }

  final Dio dio;
  Future<void> Function()? onUnauthorized;

  static String doctorImageUrl(String imageName) {
    final apiUri = Uri.parse(_baseUrl);
    return apiUri
        .replace(path: '/DoctorImage/$imageName', query: null, fragment: null)
        .toString();
  }

  void setToken(String? token) {
    if (token == null || token.isEmpty) {
      dio.options.headers.remove('Authorization');
    } else {
      dio.options.headers['Authorization'] = 'Bearer $token';
    }
  }

  static String errorMessage(Object error) {
    if (error is DioException) {
      final data = error.response?.data;
      if (data is Map && data['message'] is String) return data['message'];
      return error.message ?? 'تعذر الاتصال بالخادم.';
    }
    return 'تعذر تنفيذ العملية. حاول مرة أخرى.';
  }
}
