class DoctorReviews {
  const DoctorReviews({
    required this.isEnabled,
    required this.averageRating,
    required this.reviewCount,
    required this.reviews,
  });

  final bool isEnabled;
  final double? averageRating;
  final int reviewCount;
  final List<DoctorReview> reviews;

  factory DoctorReviews.fromJson(Map<String, dynamic> json) => DoctorReviews(
    isEnabled: json['isEnabled'] as bool? ?? false,
    averageRating: (json['averageRating'] as num?)?.toDouble(),
    reviewCount: json['reviewCount'] as int? ?? 0,
    reviews: (json['reviews'] as List? ?? const [])
        .map((item) => DoctorReview.fromJson(item as Map<String, dynamic>))
        .toList(),
  );
}

class DoctorReview {
  const DoctorReview({
    required this.userName,
    required this.rating,
    required this.comment,
  });

  final String userName;
  final int rating;
  final String comment;

  factory DoctorReview.fromJson(Map<String, dynamic> json) => DoctorReview(
    userName:
        (json['user'] as Map<String, dynamic>?)?['name'] as String? ?? 'مستخدم',
    rating: json['rating'] as int? ?? 0,
    comment: json['comment'] as String? ?? '',
  );
}
