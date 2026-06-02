import 'package:flutter/material.dart';

import '../../../core/api_client.dart';
import '../../../core/app_theme.dart';

class DoctorAvatar extends StatelessWidget {
  const DoctorAvatar({
    super.key,
    required this.imageName,
    this.size = 62,
    this.foreground = AppColors.primary,
    this.background = AppColors.softBlue,
  });

  final String imageName;
  final double size;
  final Color foreground;
  final Color background;

  @override
  Widget build(BuildContext context) => ClipRRect(
    borderRadius: BorderRadius.circular(size * .27),
    child: Container(
      width: size,
      height: size,
      color: background,
      child: imageName.isEmpty
          ? _fallback()
          : Image.network(
              ApiClient.doctorImageUrl(imageName),
              fit: BoxFit.cover,
              errorBuilder: (_, __, ___) => _fallback(),
            ),
    ),
  );

  Widget _fallback() =>
      Icon(Icons.person_outline, size: size * .58, color: foreground);
}
