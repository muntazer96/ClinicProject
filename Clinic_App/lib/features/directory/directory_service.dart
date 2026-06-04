import '../../core/api_client.dart';
import 'models/directory_models.dart';

class DirectoryService {
  DirectoryService([ApiClient? client]) : _client = client ?? ApiClient();

  final ApiClient _client;
  static const int pageSize = 15;

  Future<List<Specialization>> getSpecializations() async {
    final response = await _client.dio.get('/Specialization');
    final data = response.data['data'] as List? ?? const [];
    return data
        .map((item) => Specialization.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<DoctorSearchResult> searchDoctors({
    String? name,
    int? province,
    int? specialization,
    int page = 1,
  }) async {
    final response = await _client.dio.get(
      '/Doctor/public',
      queryParameters: {
        if (name != null && name.trim().isNotEmpty) 'name': name.trim(),
        if (province != null) 'iraqiProvince': province,
        if (specialization != null) 'specialization': specialization,
        'page': page,
        'pageSize': pageSize,
      },
    );
    final data = response.data['data'] as Map<String, dynamic>? ?? const {};
    final items = data['items'] as List? ?? const [];
    return DoctorSearchResult(
      items: items
          .map((item) => DoctorSummary.fromJson(item as Map<String, dynamic>))
          .toList(),
      totalItems: data['totalItems'] as int? ?? items.length,
      totalPages: data['totalPages'] as int? ?? 1,
      currentPage: data['currentPage'] as int? ?? page,
    );
  }

  Future<DoctorProfile> getDoctor(int id) async {
    final response = await _client.dio.get('/Doctor/public/$id');
    return DoctorProfile.fromJson(
      response.data['data'] as Map<String, dynamic>,
    );
  }
}

class DoctorSearchResult {
  const DoctorSearchResult({
    required this.items,
    required this.totalItems,
    required this.totalPages,
    required this.currentPage,
  });

  final List<DoctorSummary> items;
  final int totalItems;
  final int totalPages;
  final int currentPage;
}
