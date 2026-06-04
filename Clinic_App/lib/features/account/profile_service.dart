import '../../core/api_client.dart';

class UserProfile {
  const UserProfile({
    required this.id,
    required this.name,
    required this.phoneNumber,
    required this.email,
    required this.phoneConfirmed,
    required this.emailConfirmed,
    this.roleName,
    this.lastLoginDate,
  });

  final String id;
  final String name;
  final String phoneNumber;
  final String email;
  final bool phoneConfirmed;
  final bool emailConfirmed;
  final String? roleName;
  final DateTime? lastLoginDate;

  String get initials {
    final parts = name.trim().split(RegExp(r'\s+')).where((p) => p.isNotEmpty);
    final letters = parts
        .take(2)
        .map((p) => String.fromCharCode(p.runes.first))
        .join();
    return letters.isEmpty ? 'م' : letters;
  }

  factory UserProfile.fromJson(Map<String, dynamic> json) => UserProfile(
    id: json['id'] as String? ?? '',
    name: json['name'] as String? ?? '',
    phoneNumber: json['phoneNumber'] as String? ?? '',
    email: json['email'] as String? ?? '',
    phoneConfirmed: json['phoneNumberConfirmed'] as bool? ?? false,
    emailConfirmed: json['emailConfirmed'] as bool? ?? false,
    roleName: json['roleName'] as String?,
    lastLoginDate: json['lastLoginDate'] == null
        ? null
        : DateTime.tryParse(json['lastLoginDate'] as String),
  );
}

class ProfileService {
  ProfileService(this._client);

  final ApiClient _client;

  Future<UserProfile> getProfile() async {
    final response = await _client.dio.get('/User/me');
    return UserProfile.fromJson(response.data['data'] as Map<String, dynamic>);
  }

  Future<UserProfile> updateProfile({
    required String name,
    required String phoneNumber,
    required String email,
  }) async {
    await _client.dio.put(
      '/User/me',
      data: {
        'name': name.trim(),
        'phoneNumber': phoneNumber.trim(),
        'email': email.trim(),
      },
    );
    return getProfile();
  }

  Future<UserProfile> updateName({
    required UserProfile current,
    required String name,
  }) {
    return updateProfile(
      name: name,
      phoneNumber: current.phoneNumber,
      email: current.email,
    );
  }

  Future<void> changePassword({
    required String currentPassword,
    required String newPassword,
  }) async {
    await _client.dio.post(
      '/User/password/change',
      data: {'currentPassword': currentPassword, 'newPassword': newPassword},
    );
  }

  Future<void> sendEmailConfirmation(String identifier) async {
    await _client.dio.post(
      '/User/email-confirmation',
      queryParameters: {'identifier': identifier},
    );
  }

  Future<void> sendPhoneConfirmation() async {
    await _client.dio.post('/User/phone-confirmation');
  }

  Future<void> confirmPhone(String otpCode) async {
    await _client.dio.post(
      '/User/phone-confirm',
      data: {'otpCode': otpCode.trim()},
    );
  }
}
