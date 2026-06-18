import 'dart:typed_data';

import 'package:dio/dio.dart';

import '../../core/api_client.dart';

class UserProfile {
  const UserProfile({
    required this.id,
    required this.name,
    required this.phoneNumber,
    required this.phoneConfirmed,
    this.roleName,
    this.imageName,
    this.lastLoginDate,
  });

  final String id;
  final String name;
  final String phoneNumber;
  final bool phoneConfirmed;
  final String? roleName;
  final String? imageName;
  final DateTime? lastLoginDate;

  String? get imageUrl {
    final value = imageName?.trim();
    if (value == null || value.isEmpty || value == 'default.png') return null;
    return ApiClient.userImageUrl(value);
  }

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
    phoneConfirmed: json['phoneNumberConfirmed'] as bool? ?? false,
    roleName: json['roleName'] as String?,
    imageName: json['imageName'] as String?,
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
  }) async {
    await _client.dio.put(
      '/User/me',
      data: {
        'name': name.trim(),
        'phoneNumber': phoneNumber.trim(),
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
    );
  }

  Future<void> updateProfileImage({
    required String fileName,
    required Uint8List bytes,
  }) async {
    await _client.dio.put(
      '/Profile/image',
      data: FormData.fromMap({
        'file': MultipartFile.fromBytes(bytes, filename: fileName),
      }),
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
