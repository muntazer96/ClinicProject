import 'package:flutter/foundation.dart';
import 'package:package_info_plus/package_info_plus.dart';

import 'api_client.dart';

class AppVersionInfo {
  const AppVersionInfo({
    required this.platform,
    required this.currentVersion,
    required this.currentBuildNumber,
    required this.latestVersion,
    required this.latestBuildNumber,
    required this.minimumSupportedVersion,
    required this.minimumSupportedBuildNumber,
    required this.updateAvailable,
    required this.updateRequired,
    required this.forceUpdate,
    required this.title,
    required this.message,
    required this.updateUrl,
  });

  final String platform;
  final String currentVersion;
  final int currentBuildNumber;
  final String latestVersion;
  final int latestBuildNumber;
  final String minimumSupportedVersion;
  final int minimumSupportedBuildNumber;
  final bool updateAvailable;
  final bool updateRequired;
  final bool forceUpdate;
  final String title;
  final String message;
  final String? updateUrl;

  factory AppVersionInfo.fromJson(Map<String, dynamic> json) => AppVersionInfo(
    platform: json['platform'] as String? ?? '',
    currentVersion: json['currentVersion'] as String? ?? '',
    currentBuildNumber: json['currentBuildNumber'] as int? ?? 0,
    latestVersion: json['latestVersion'] as String? ?? '',
    latestBuildNumber: json['latestBuildNumber'] as int? ?? 0,
    minimumSupportedVersion: json['minimumSupportedVersion'] as String? ?? '',
    minimumSupportedBuildNumber:
        json['minimumSupportedBuildNumber'] as int? ?? 0,
    updateAvailable: json['updateAvailable'] as bool? ?? false,
    updateRequired: json['updateRequired'] as bool? ?? false,
    forceUpdate: json['forceUpdate'] as bool? ?? false,
    title: json['title'] as String? ?? 'تحديث جديد متوفر',
    message:
        json['message'] as String? ??
        'تتوفر نسخة أحدث من التطبيق. يرجى التحديث للحصول على أفضل تجربة.',
    updateUrl: json['updateUrl'] as String?,
  );
}

class AppVersionService {
  AppVersionService(this._client);

  final ApiClient _client;

  Future<AppVersionInfo?> checkForUpdate() async {
    final packageInfo = await PackageInfo.fromPlatform();
    final buildNumber = int.tryParse(packageInfo.buildNumber) ?? 1;
    final response = await _client.dio.get(
      '/AppVersion/check',
      queryParameters: {
        'platform': _platformName,
        'currentVersion': packageInfo.version,
        'currentBuildNumber': buildNumber,
      },
    );
    final data = response.data['data'];
    if (data is! Map) return null;
    return AppVersionInfo.fromJson(Map<String, dynamic>.from(data));
  }

  String get _platformName {
    if (kIsWeb) return 'web';
    return switch (defaultTargetPlatform) {
      TargetPlatform.android => 'android',
      TargetPlatform.iOS => 'ios',
      TargetPlatform.macOS ||
      TargetPlatform.windows ||
      TargetPlatform.linux ||
      TargetPlatform.fuchsia => 'web',
    };
  }
}
