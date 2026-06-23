import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';

class BookingConfirmArgs {
  const BookingConfirmArgs({
    required this.doctorName,
    required this.clinicName,
    required this.dayName,
    required this.date,
    this.guestName,
    this.guestPhone,
    this.notes,
  });

  final String doctorName;
  final String clinicName;
  final String dayName;
  final DateTime date;
  final String? guestName;
  final String? guestPhone;
  final String? notes;
}

class BookingConfirmScreen extends StatelessWidget {
  const BookingConfirmScreen({super.key, required this.args});
  final BookingConfirmArgs args;

  void _showLongPressHint(BuildContext context) {
    showAppSnackBar(
      context,
      'اضغط مطولاً على الزر لتثبيت الحجز.',
      type: AppSnackBarType.info,
    );
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('تثبيت الحجز')),
    body: ListView(
      padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
      children: [
        Container(
          padding: const EdgeInsets.all(18),
          decoration: BoxDecoration(
            color: AppColors.primaryDark,
            borderRadius: BorderRadius.circular(8),
          ),
          child: const Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Icon(
                Icons.event_available_rounded,
                color: Colors.white,
                size: 34,
              ),
              SizedBox(height: 12),
              Text(
                'راجع تفاصيل الحجز',
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 22,
                  fontWeight: FontWeight.w900,
                ),
              ),
              SizedBox(height: 6),
              Text(
                'تأكد من المعلومات، ثم اضغط مطولاً لتثبيت الدور.',
                style: TextStyle(color: Color(0xFFD7FFFA)),
              ),
            ],
          ),
        ),
        const SizedBox(height: 16),
        Card(
          child: Padding(
            padding: const EdgeInsets.all(14),
            child: Column(
              children: [
                _SummaryLine('الطبيب', args.doctorName),
                _SummaryLine('العيادة', args.clinicName),
                _SummaryLine(
                  'التاريخ',
                  '${args.dayName} - ${DateFormat('yyyy/MM/dd').format(args.date)}',
                ),
                if (args.guestName?.isNotEmpty == true)
                  _SummaryLine('المراجع', args.guestName!),
                if (args.guestPhone?.isNotEmpty == true)
                  _SummaryLine('الهاتف', args.guestPhone!),
                if (args.notes?.isNotEmpty == true)
                  _SummaryLine('الملاحظات', args.notes!),
              ],
            ),
          ),
        ),
        const SizedBox(height: 16),
        FilledButton.icon(
          onPressed: () => _showLongPressHint(context),
          onLongPress: () => context.pop(true),
          icon: const Icon(Icons.check_circle_outline),
          label: const Text('اضغط مطولاً لتثبيت الحجز'),
        ),
        const SizedBox(height: 10),
        OutlinedButton.icon(
          onPressed: () => context.pop(false),
          icon: const Icon(Icons.arrow_back_rounded),
          label: const Text('العودة'),
        ),
      ],
    ),
  );
}

class _SummaryLine extends StatelessWidget {
  const _SummaryLine(this.label, this.value);
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) => Container(
    margin: const EdgeInsets.only(bottom: 8),
    padding: const EdgeInsets.all(10),
    decoration: BoxDecoration(
      color: context.appSurfaceMuted,
      borderRadius: BorderRadius.circular(8),
    ),
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: 82,
          child: Text(label, style: TextStyle(color: context.appMuted)),
        ),
        Expanded(
          child: Text(
            value,
            style: const TextStyle(fontWeight: FontWeight.w800),
          ),
        ),
      ],
    ),
  );
}
