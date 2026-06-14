import 'package:flutter/material.dart';

class AppLogo extends StatelessWidget {
  const AppLogo({
    super.key,
    required this.size,
    this.light = false,
  });

  final double size;
  final bool light;

  @override
  Widget build(BuildContext context) {
    return Image.asset(
      'assets/app_logo.png',
      width: size,
      height: size,
      fit: BoxFit.contain,
      color: light ? Colors.white : null,
      colorBlendMode: light ? BlendMode.srcIn : null,
    );
  }
}
