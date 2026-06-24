import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

class ExitGuard extends StatelessWidget {
  const ExitGuard({super.key, required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) => PopScope(
    canPop: false,
    onPopInvokedWithResult: (didPop, _) async {
      if (didPop) return;
      final navigator = Navigator.of(context);
      if (navigator.canPop()) {
        navigator.pop();
      } else {
        final confirmed = await showDialog<bool>(
          context: context,
          builder: (ctx) => AlertDialog(
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(16),
            ),
            title: const Text('خروج'),
            content: const Text('هل أنت متأكد من الخروج من التطبيق؟'),
            actionsAlignment: MainAxisAlignment.center,
            actions: [
              TextButton(
                onPressed: () => Navigator.of(ctx).pop(false),
                child: const Text(
                  'إلغاء',
                  style: TextStyle(fontWeight: FontWeight.w800),
                ),
              ),
              FilledButton(
                onPressed: () => Navigator.of(ctx).pop(true),
                style: FilledButton.styleFrom(minimumSize: const Size(0, 40)),
                child: const Text(
                  'خروج',
                  style: TextStyle(fontWeight: FontWeight.w800),
                ),
              ),
            ],
          ),
        );
        if (confirmed == true && context.mounted) {
          SystemNavigator.pop();
        }
      }
    },
    child: child,
  );
}
