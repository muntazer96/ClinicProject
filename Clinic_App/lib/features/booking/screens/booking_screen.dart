import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_theme.dart';
import '../../../widgets/app_scaffold.dart';
import '../../auth/auth_controller.dart';
import '../booking_service.dart';
import '../models/booking_models.dart';
import 'otp_screen.dart';
import 'success_screen.dart';

class BookingScreen extends StatefulWidget {
  const BookingScreen({
    super.key,
    required this.doctorId,
    required this.clinicId,
    required this.doctorName,
    required this.clinicName,
  });

  final int doctorId;
  final int clinicId;
  final String doctorName;
  final String clinicName;

  @override
  State<BookingScreen> createState() => _BookingScreenState();
}

class _BookingScreenState extends State<BookingScreen> {
  late final BookingService _service;
  final _name = TextEditingController();
  final _phone = TextEditingController();
  final _notes = TextEditingController();
  List<QueueAvailability> _days = [];
  QueueAvailability? _selected;
  bool _loading = true;
  bool _saving = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _service = BookingService(context.read<AuthController>().api);
    _loadAvailability();
  }

  @override
  void dispose() {
    _name.dispose();
    _phone.dispose();
    _notes.dispose();
    super.dispose();
  }

  Future<void> _loadAvailability() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final days = await _service.getAvailability(widget.clinicId);
      if (!mounted) return;
      setState(() {
        _days = days;
        _selected = null;
        for (final day in days) {
          if (day.isAvailable) {
            _selected = day;
            break;
          }
        }
      });
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _submit() async {
    final auth = context.read<AuthController>();
    if (_selected == null) return;
    if (!auth.isAuthenticated &&
        (_name.text.trim().isEmpty || _phone.text.trim().isEmpty)) {
      setState(() => _error = 'أدخل اسم المراجع ورقم الهاتف لإكمال الحجز.');
      return;
    }
    if (auth.isAuthenticated && auth.phoneNumber == null) {
      setState(
        () => _error =
            'سجل الخروج ثم ادخل إلى حسابك مرة أخرى قبل الحجز لتأكيد رقم الهاتف.',
      );
      return;
    }
    final accepted = await _confirmBookingSummary(auth.isAuthenticated);
    if (accepted != true) return;
    setState(() {
      _saving = true;
      _error = null;
    });
    try {
      final result = await _service.createBooking(
        doctorId: widget.doctorId,
        clinicId: widget.clinicId,
        date: _selected!.date,
        guestName: auth.isAuthenticated ? null : _name.text,
        guestPhoneNumber: auth.isAuthenticated ? null : _phone.text,
        notes: _notes.text,
      );
      if (!mounted) return;
      final phoneNumber = auth.isAuthenticated
          ? auth.phoneNumber
          : _phone.text.trim();
      if (result.requiresOtp) {
        context.go(
          '/booking/otp',
          extra: OtpScreenArgs(
            result: result,
            phoneNumber: phoneNumber ?? '',
            doctorName: widget.doctorName,
            clinicName: widget.clinicName,
            date: _selected!.date,
          ),
        );
      } else {
        context.go(
          '/booking/success',
          extra: BookingSuccessArgs(
            result: result,
            doctorName: widget.doctorName,
            clinicName: widget.clinicName,
            date: _selected!.date,
          ),
        );
      }
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  Future<bool?> _confirmBookingSummary(bool authenticated) {
    final selected = _selected;
    if (selected == null) return Future.value(false);
    return showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('تأكيد الحجز'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _SummaryLine('الطبيب', widget.doctorName),
            _SummaryLine('العيادة', widget.clinicName),
            _SummaryLine(
              'التاريخ',
              '${selected.dayName} - ${DateFormat('yyyy/MM/dd').format(selected.date)}',
            ),
            if (!authenticated) ...[
              _SummaryLine('المراجع', _name.text.trim()),
              _SummaryLine('الهاتف', _phone.text.trim()),
            ],
            if (_notes.text.trim().isNotEmpty)
              _SummaryLine('الملاحظات', _notes.text.trim()),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('تراجع'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('تثبيت الحجز'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final authenticated = context.watch<AuthController>().isAuthenticated;
    return AppScaffold(
      title: 'حجز دور',
      child: ListView(
        padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
        children: [
          _DoctorHeader(
            doctorName: widget.doctorName,
            clinicName: widget.clinicName,
          ),
          const SizedBox(height: 22),
          const Text(
            'اختر اليوم المناسب',
            style: TextStyle(fontSize: 18, fontWeight: FontWeight.w800),
          ),
          const SizedBox(height: 10),
          if (_loading)
            const Center(child: CircularProgressIndicator())
          else if (_days.isEmpty)
            const _Notice(text: 'لا توجد أيام متاحة للحجز حالياً.')
          else
            SizedBox(
              height: 142,
              child: ListView.separated(
                scrollDirection: Axis.horizontal,
                itemCount: _days.length,
                separatorBuilder: (_, __) => const SizedBox(width: 8),
                itemBuilder: (_, index) {
                  final day = _days[index];
                  return _DayCard(
                    day: day,
                    selected: _selected == day,
                    onTap: day.isAvailable
                        ? () => setState(() => _selected = day)
                        : null,
                  );
                },
              ),
            ),
          if (_selected != null) ...[
            const SizedBox(height: 10),
            _SelectedDaySummary(day: _selected!),
          ],
          const SizedBox(height: 16),
          if (!authenticated) ...[
            const _Notice(
              text:
                  'يمكنك الحجز كزائر. احتفظ بكود الحجز لمراجعته أو إلغائه لاحقاً.',
            ),
            const SizedBox(height: 10),
            TextField(
              controller: _name,
              decoration: const InputDecoration(labelText: 'اسم المراجع'),
            ),
            const SizedBox(height: 10),
            TextField(
              controller: _phone,
              keyboardType: TextInputType.phone,
              decoration: const InputDecoration(labelText: 'رقم الهاتف'),
            ),
            const SizedBox(height: 8),
            TextButton(
              onPressed: () => context.push(
                '/login?redirect=${Uri.encodeComponent(GoRouterState.of(context).uri.toString())}',
              ),
              child: const Text('لديك حساب؟ سجل الدخول قبل الحجز'),
            ),
          ],
          TextField(
            controller: _notes,
            maxLength: 1000,
            maxLines: 3,
            decoration: const InputDecoration(
              labelText: 'ملاحظات اختيارية للمراجعة',
            ),
          ),
          if (_error != null) ...[
            const SizedBox(height: 8),
            _Notice(text: _error!, error: true),
          ],
          const SizedBox(height: 12),
          FilledButton.icon(
            onPressed: _selected == null || _saving ? null : _submit,
            icon: const Icon(Icons.check_circle_outline),
            label: Text(_saving ? 'جارِ تثبيت الحجز...' : 'تأكيد حجز الدور'),
          ),
        ],
      ),
    );
  }
}

class _DayCard extends StatelessWidget {
  const _DayCard({required this.day, required this.selected, this.onTap});

  final QueueAvailability day;
  final bool selected;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) => InkWell(
    borderRadius: BorderRadius.circular(16),
    onTap: onTap,
    child: Container(
      width: 132,
      padding: const EdgeInsets.all(10),
      decoration: BoxDecoration(
        color: selected ? AppColors.primary : Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: selected ? AppColors.primary : AppColors.border,
        ),
        boxShadow: selected
            ? const [
                BoxShadow(
                  color: Color(0x3313796B),
                  blurRadius: 14,
                  offset: Offset(0, 7),
                ),
              ]
            : null,
      ),
      child: Column(
        children: [
          Text(
            day.dayName,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: TextStyle(
              color: selected ? Colors.white : AppColors.text,
              fontWeight: FontWeight.w800,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            DateFormat('d/M').format(day.date),
            style: TextStyle(color: selected ? Colors.white : AppColors.muted),
          ),
          const SizedBox(height: 5),
          if (day.startTime?.isNotEmpty == true && day.endTime?.isNotEmpty == true)
            Text(
              '${_shortTime(day.startTime!)} - ${_shortTime(day.endTime!)}',
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: TextStyle(
                color: selected ? const Color(0xFFD7FFFA) : AppColors.primary,
                fontSize: 11,
                fontWeight: FontWeight.w700,
              ),
            ),
          const Spacer(),
          Text(
            day.isAvailable
                ? '${day.remainingAppointments} دور متبقٍ'
                : 'غير متاح',
            style: TextStyle(
              fontSize: 11,
              color: selected ? const Color(0xFFD7FFFA) : AppColors.muted,
            ),
          ),
        ],
      ),
    ),
  );

  static String _shortTime(String value) =>
      value.length >= 5 ? value.substring(0, 5) : value;
}

class _SelectedDaySummary extends StatelessWidget {
  const _SelectedDaySummary({required this.day});

  final QueueAvailability day;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(12),
    decoration: BoxDecoration(
      color: AppColors.softBlue,
      borderRadius: BorderRadius.circular(14),
      border: Border.all(color: AppColors.border),
    ),
    child: Row(
      children: [
        const Icon(Icons.event_available_rounded, color: AppColors.primary),
        const SizedBox(width: 9),
        Expanded(
          child: Text(
            '${day.dayName} ${DateFormat('yyyy/MM/dd').format(day.date)} - ${day.remainingAppointments} دور متبقٍ',
            style: const TextStyle(fontWeight: FontWeight.w800),
          ),
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
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: 8),
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: 76,
          child: Text(label, style: const TextStyle(color: AppColors.muted)),
        ),
        Expanded(
          child: Text(value, style: const TextStyle(fontWeight: FontWeight.w800)),
        ),
      ],
    ),
  );
}

class _Notice extends StatelessWidget {
  const _Notice({required this.text, this.error = false});
  final String text;
  final bool error;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(11),
    decoration: BoxDecoration(
      color: error ? Colors.red.shade50 : AppColors.softBlue,
      borderRadius: BorderRadius.circular(12),
    ),
    child: Text(
      text,
      style: TextStyle(
        color: error ? Colors.red.shade800 : AppColors.primaryDark,
      ),
    ),
  );
}

class _DoctorHeader extends StatelessWidget {
  const _DoctorHeader({required this.doctorName, required this.clinicName});

  final String doctorName;
  final String clinicName;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(17),
    decoration: BoxDecoration(
      gradient: const LinearGradient(
        colors: [AppColors.primary, AppColors.primaryDark],
      ),
      borderRadius: BorderRadius.circular(24),
      boxShadow: const [
        BoxShadow(
          color: Color(0x3313796B),
          blurRadius: 18,
          offset: Offset(0, 9),
        ),
      ],
    ),
    child: Row(
      children: [
        Container(
          width: 51,
          height: 51,
          decoration: BoxDecoration(
            color: Colors.white.withValues(alpha: .17),
            borderRadius: BorderRadius.circular(16),
          ),
          child: const Icon(Icons.calendar_month_rounded, color: Colors.white),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                doctorName,
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 18,
                  fontWeight: FontWeight.w900,
                ),
              ),
              const SizedBox(height: 3),
              Text(
                clinicName,
                style: const TextStyle(color: Color(0xFFD7FFFA)),
              ),
            ],
          ),
        ),
      ],
    ),
  );
}
