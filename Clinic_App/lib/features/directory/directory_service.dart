import '../../core/api_client.dart';
import 'models/directory_models.dart';

class DirectoryService {
  DirectoryService([ApiClient? client]) : _client = client ?? ApiClient();

  final ApiClient _client;

  Future<List<Specialization>> getSpecializations() async {
    final response = await _client.dio.get('/Specialization');
    final data = response.data['data'] as List? ?? const [];
    return data
        .map((item) => Specialization.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<List<DoctorSummary>> searchDoctors({
    String? name,
    int? province,
    int? specialization,
  }) async {
    final response = await _client.dio.get(
      '/Doctor/public',
      queryParameters: {
        if (name != null && name.trim().isNotEmpty) 'name': name.trim(),
        if (province != null) 'iraqiProvince': province,
        if (specialization != null) 'specialization': specialization,
        'page': 1,
        'pageSize': 100,
      },
    );
    final items = response.data['data']['items'] as List? ?? const [];
    return items
        .map((item) => DoctorSummary.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<DoctorProfile> getDoctor(int id) async {
    final response = await _client.dio.get('/Doctor/public/$id');
    return DoctorProfile.fromJson(
      response.data['data'] as Map<String, dynamic>,
    );
  }
}
