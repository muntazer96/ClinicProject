import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';

import 'app_snack_bar.dart';

Future<void> openPhone(BuildContext context, String phoneNumber) async {
  await _open(context, Uri(scheme: 'tel', path: phoneNumber.trim()));
}

Future<void> openMap(
  BuildContext context,
  String value, {
  String? query,
}) async {
  final trimmed = value.trim();
  final uri = Uri.tryParse(trimmed);
  final mapUri = uri != null && uri.hasScheme
      ? uri
      : Uri.https('www.google.com', '/maps/search/', {
          'api': '1',
          'query': (query?.trim().isNotEmpty == true ? query : trimmed)!,
        });
  await _open(context, mapUri);
}

Future<void> _open(BuildContext context, Uri uri) async {
  try {
    final launched = await launchUrl(uri, mode: LaunchMode.externalApplication);
    if (!launched && context.mounted) _showError(context);
  } catch (_) {
    if (context.mounted) _showError(context);
  }
}

void _showError(BuildContext context) {
  showAppSnackBar(
    context,
    'تعذر فتح الرابط على هذا الجهاز.',
    type: AppSnackBarType.error,
  );
}
