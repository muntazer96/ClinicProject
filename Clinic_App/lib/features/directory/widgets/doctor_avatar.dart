import 'package:flutter/material.dart';

import '../../../core/api_client.dart';

class DoctorAvatar extends StatelessWidget {
  const DoctorAvatar({
    super.key,
    required this.imageName,
    this.size = 62,
    this.foreground = const Color(0xFF147D72),
    this.background = const Color(0xFFE4F5F1),
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
