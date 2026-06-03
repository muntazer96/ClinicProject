import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../../core/app_theme.dart';
import '../models/booking_models.dart';

class BookingSuccessArgs {
  const BookingSuccessArgs({
    required this.result,
    required this.doctorName,
    required this.clinicName,
    required this.date,
  });

  final BookingResult result;
  final String doctorName;
  final String clinicName;
  final DateTime date;
}

class BookingSuccessScreen extends StatelessWidget {
  const BookingSuccessScreen({super.key, required this.args});
  final BookingSuccessArgs args;

  Future<void> _copyCode(BuildContext context) async {
    await Clipboard.setData(ClipboardData(text: args.result.code));
    if (!context.mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('تم نسخ كود الحجز.')),
    );
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('تم الحجز')),
    body: ListView(
      padding: const EdgeInsets.all(20),
      children: [
        const Icon(Icons.check_circle, size: 74, color: AppColors.primary),
        const SizedBox(height: 12),
        const Text(
          'تم تثبيت حجزك بنجاح',
          textAlign: TextAlign.center,
          style: TextStyle(fontSize: 24, fontWeight: FontWeight.w900),
        ),
        const SizedBox(height: 16),
        Card(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              children: [
                _Row('رقم الدور', '#${args.result.queueNumber}'),
                Container(
                  width: double.infinity,
                  margin: const EdgeInsets.symmetric(vertical: 8),
                  padding: const EdgeInsets.all(13),
                  decoration: BoxDecoration(
                    color: AppColors.softBlue,
                    borderRadius: BorderRadius.circular(14),
                  ),
                  child: Column(
                    children: [
                      const Text(
                        'كود الحجز',
                        style: TextStyle(color: AppColors.muted),
                      ),
                      const SizedBox(height: 5),
                      SelectableText(
                        args.result.code,
                        style: const TextStyle(
                          fontSize: 22,
                          fontWeight: FontWeight.w900,
                          color: AppColors.primaryDark,
                        ),
                      ),
                      TextButton.icon(
                        onPressed: () => _copyCode(context),
                        icon: const Icon(Icons.copy_rounded),
                        label: const Text('نسخ الكود'),
                      ),
                    ],
                  ),
                ),
                _Row('الطبيب', args.doctorName),
                _Row('العيادة', args.clinicName),
                _Row('التاريخ', DateFormat('yyyy/MM/dd').format(args.date)),
              ],
            ),
          ),
        ),
        const SizedBox(height: 12),
        const Text(
          'احتفظ بكود الحجز. ستحتاجه لمتابعة الحجز أو إلغائه إذا كان الحجز كزائر.',
          textAlign: TextAlign.center,
          style: TextStyle(color: AppColors.muted),
        ),
        const SizedBox(height: 16),
        FilledButton(
          onPressed: () => context.go('/guest-booking'),
          child: const Text('متابعة الحجز'),
        ),
        OutlinedButton(
          onPressed: () => context.go('/search'),
          child: const Text('البحث عن طبيب آخر'),
        ),
        TextButton(
          onPressed: () => context.go('/'),
          child: const Text('العودة إلى الرئيسية'),
        ),
      ],
    ),
  );
}

class _Row extends StatelessWidget {
  const _Row(this.label, this.value);
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 7),
    child: Row(
      children: [
        Expanded(
          child: Text(label, style: const TextStyle(color: AppColors.muted)),
        ),
        Text(value, style: const TextStyle(fontWeight: FontWeight.w900)),
      ],
    ),
  );
}
