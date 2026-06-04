import 'package:flutter/material.dart';

import 'app_theme.dart';

enum AppSnackBarType { success, error, warning, info }

void showAppSnackBar(
  BuildContext context,
  String message, {
  AppSnackBarType type = AppSnackBarType.info,
}) {
  final color = switch (type) {
    AppSnackBarType.success => AppColors.success,
    AppSnackBarType.error => AppColors.danger,
    AppSnackBarType.warning => AppColors.warning,
    AppSnackBarType.info => AppColors.primaryDark,
  };

  ScaffoldMessenger.of(context)
    ..hideCurrentSnackBar()
    ..showSnackBar(SnackBar(content: Text(message), backgroundColor: color));
}
