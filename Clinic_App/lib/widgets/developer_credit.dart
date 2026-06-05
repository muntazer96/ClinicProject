import 'package:flutter/material.dart';

import '../core/app_theme.dart';

class DeveloperCredit extends StatelessWidget {
  const DeveloperCredit({super.key, this.compact = false, this.light = false});

  final bool compact;
  final bool light;

  @override
  Widget build(BuildContext context) {
    final foreground = light ? Colors.white : AppColors.text;

    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            'Powered by',
            style: TextStyle(
              color: foreground.withValues(alpha: .62),
              fontSize: compact ? 9 : 10,
              fontWeight: FontWeight.w800,
              letterSpacing: 0,
            ),
          ),
          const SizedBox(height: 3),
          Image.asset(
            'assets/godev_logo.png',
            width: compact ? 42 : 52,
            fit: BoxFit.contain,
          ),
        ],
      ),
    );
  }
}
