import 'package:flutter/material.dart';

import '../core/app_snack_bar.dart';

class LongPressButton extends StatelessWidget {
  const LongPressButton({
    super.key,
    required this.onLongPress,
    required this.icon,
    required this.label,
    this.danger = false,
    this.filled = false,
  });

  final VoidCallback? onLongPress;
  final Widget icon;
  final Widget label;
  final bool danger;
  final bool filled;

  @override
  Widget build(BuildContext context) {
    final foreground = danger ? Theme.of(context).colorScheme.error : null;
    final showHint = onLongPress == null
        ? null
        : () => showAppSnackBar(context, 'اضغط مطولاً لتنفيذ العملية.');

    if (filled) {
      return FilledButton.icon(
        onPressed: showHint,
        onLongPress: onLongPress,
        icon: icon,
        label: label,
      );
    }

    return OutlinedButton.icon(
      onPressed: showHint,
      onLongPress: onLongPress,
      icon: icon,
      label: label,
      style: foreground == null
          ? null
          : OutlinedButton.styleFrom(foregroundColor: foreground),
    );
  }
}
