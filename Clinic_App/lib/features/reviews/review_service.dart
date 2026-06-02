import 'package:dio/dio.dart';

import '../../core/api_client.dart';
import 'models/review_models.dart';

class ReviewService {
  ReviewService(this._client);

  final ApiClient _client;

  Future<DoctorReviews?> getDoctorReviews(int doctorId) async {
    try {
      final response = await _client.dio.get('/Review/doctor/$doctorId');
      return DoctorReviews.fromJson(
        response.data['data'] as Map<String, dynamic>,
      );
    } on DioException catch (error) {
      if (error.response?.statusCode == 400) return null;
      rethrow;
    }
  }

  Future<void> addReview({
    required int doctorId,
    required int appointmentId,
    required int rating,
    required String comment,
  }) async {
    await _client.dio.post(
      '/Review',
      data: {
        'doctorId': doctorId,
        'appointmentId': appointmentId,
        'rating': rating,
        'comment': comment.trim(),
      },
    );
  }
}
