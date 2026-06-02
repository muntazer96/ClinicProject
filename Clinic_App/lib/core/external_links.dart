import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';

Future<void> openPhone(BuildContext context, String phoneNumber) async {
  await _open(context, Uri(scheme: 'tel', path: phoneNumber.trim()));
}

Future<void> openMap(BuildContext context, String url) async {
  final uri = Uri.tryParse(url.trim());
  if (uri == null) {
    _showError(context);
    return;
  }
  await _open(context, uri);
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
  ScaffoldMessenger.of(context).showSnackBar(
    const SnackBar(content: Text('تعذر فتح الرابط على هذا الجهاز.')),
  );
}
