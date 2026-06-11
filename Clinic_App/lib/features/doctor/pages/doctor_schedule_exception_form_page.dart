import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorScheduleExceptionFormPage extends StatefulWidget {
  const DoctorScheduleExceptionFormPage({super.key, required this.clinic});

  final DoctorClinic clinic;

  @override
  State<DoctorScheduleExceptionFormPage> createState() =>
      _DoctorScheduleExceptionFormPageState();
}

class _DoctorScheduleExceptionFormPageState
    extends State<DoctorScheduleExceptionFormPage> {
  late final DoctorService _service;
  final _reason = TextEditingController();

  DateTime _date = DateTime.now();
  bool _closed = true;
  bool _saving = false;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);
  }

  @override
  void dispose() {
    _reason.dispose();
    super.dispose();
  }

  Future<void> _pickDate() async {
    final picked = await showDatePicker(
      context: context,
      firstDate: DateTime.now(),
      lastDate: DateTime.now().add(const Duration(days: 180)),
      initialDate: _date,
    );

    if (picked != null) {
      setState(() => _date = picked);
    }
  }

  Future<void> _save() async {
    setState(() => _saving = true);

    try {
      await _service.saveException(
        clinicId: widget.clinic.id,
        date: _date,
        isClosed: _closed,
        reason: _reason.text.trim(),
      );

      if (!mounted) return;
      showAppSnackBar(context, 'تم حفظ الاستثناء.');
      context.pop();
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
        title: 'استثناء دوام',
        showBackButton: true,
        backRoute: '/doctor/schedule',
        child: ListView(
          padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
          children: [
            _HeroExceptionCard(
              clinicName: widget.clinic.name,
              closed: _closed,
            ),

            const SizedBox(height: 14),

            _SectionCard(
              title: 'تاريخ الاستثناء',
              icon: Icons.calendar_month_rounded,
              child: _DatePickerBox(
                date: _date,
                onTap: _pickDate,
              ),
            ),

            const SizedBox(height: 14),

            _SectionCard(
              title: 'نوع الاستثناء',
              icon: Icons.tune_rounded,
              child: _ClosedSwitchCard(
                value: _closed,
                onChanged: (value) => setState(() => _closed = value),
              ),
            ),

            const SizedBox(height: 14),

            _SectionCard(
              title: 'سبب الاستثناء',
              icon: Icons.notes_rounded,
              child: TextField(
                controller: _reason,
                maxLines: 4,
                minLines: 3,
                decoration: const InputDecoration(
                  labelText: 'مثال: عطلة، ظرف طارئ، صيانة العيادة...',
                  prefixIcon: Icon(Icons.edit_note_rounded),
                  alignLabelWithHint: true,
                ),
              ),
            ),

            const SizedBox(height: 18),

            SizedBox(
              height: 52,
              child: ElevatedButton.icon(
                onPressed: _saving ? null : _save,
                icon: _saving
                    ? const SizedBox(
                        width: 18,
                        height: 18,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: Colors.white,
                        ),
                      )
                    : const Icon(Icons.save_rounded),
                label: Text(
                  _saving ? 'جاري الحفظ...' : 'حفظ الاستثناء',
                  style: const TextStyle(
                    fontSize: 15,
                    fontWeight: FontWeight.w900,
                  ),
                ),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: Colors.white,
                  disabledBackgroundColor: AppColors.primary.withOpacity(.55),
                  elevation: 0,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(16),
                  ),
                ),
              ),
            ),
          ],
        ),
      );
}

class _HeroExceptionCard extends StatelessWidget {
  const _HeroExceptionCard({
    required this.clinicName,
    required this.closed,
  });

  final String clinicName;
  final bool closed;

  @override
  Widget build(BuildContext context) {
    return AnimatedContainer(
      duration: const Duration(milliseconds: 220),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: closed
              ? const [Color(0xFFB45309), Color(0xFFD97706)]
              : const [Color(0xFF0F7F73), Color(0xFF0D625C)],
          begin: Alignment.topRight,
          end: Alignment.bottomLeft,
        ),
        borderRadius: BorderRadius.circular(22),
        boxShadow: [
          BoxShadow(
            color: (closed ? const Color(0xFFD97706) : AppColors.primary)
                .withOpacity(.18),
            blurRadius: 18,
            offset: const Offset(0, 9),
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            width: 58,
            height: 58,
            decoration: BoxDecoration(
              color: Colors.white.withOpacity(.16),
              borderRadius: BorderRadius.circular(18),
            ),
            child: Icon(
              closed
                  ? Icons.event_busy_rounded
                  : Icons.event_available_rounded,
              color: Colors.white,
              size: 31,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  clinicName,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 18,
                    fontWeight: FontWeight.w900,
                  ),
                ),
                const SizedBox(height: 6),
                Text(
                  closed
                      ? 'سيتم إغلاق العيادة في التاريخ المحدد'
                      : 'سيتم فتح العيادة بشكل استثنائي',
                  style: TextStyle(
                    color: Colors.white.withOpacity(.88),
                    fontSize: 12.5,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _SectionCard extends StatelessWidget {
  const _SectionCard({
    required this.title,
    required this.icon,
    required this.child,
  });

  final String title;
  final IconData icon;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: const Color(0xFFDDE9E7)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(.025),
            blurRadius: 12,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              Icon(icon, color: AppColors.primary, size: 20),
              const SizedBox(width: 7),
              Text(
                title,
                style: const TextStyle(
                  fontSize: 15,
                  fontWeight: FontWeight.w900,
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),
          child,
        ],
      ),
    );
  }
}

class _DatePickerBox extends StatelessWidget {
  const _DatePickerBox({
    required this.date,
    required this.onTap,
  });

  final DateTime date;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: const Color(0xFFF7FAFA),
      borderRadius: BorderRadius.circular(16),
      child: InkWell(
        borderRadius: BorderRadius.circular(16),
        onTap: onTap,
        child: Container(
          padding: const EdgeInsets.all(14),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(16),
            border: Border.all(color: const Color(0xFFE3ECEA)),
          ),
          child: Row(
            children: [
              const Icon(
                Icons.today_rounded,
                color: AppColors.primary,
                size: 25,
              ),
              const SizedBox(width: 10),
              Expanded(
                child: Text(
                  DateFormat('yyyy/MM/dd').format(date),
                  style: const TextStyle(
                    fontSize: 18,
                    fontWeight: FontWeight.w900,
                  ),
                ),
              ),
              const Icon(
                Icons.keyboard_arrow_down_rounded,
                color: AppColors.primary,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _ClosedSwitchCard extends StatelessWidget {
  const _ClosedSwitchCard({
    required this.value,
    required this.onChanged,
  });

  final bool value;
  final ValueChanged<bool> onChanged;

  @override
  Widget build(BuildContext context) {
    final color = value ? const Color(0xFFD97706) : AppColors.primary;

    return AnimatedContainer(
      duration: const Duration(milliseconds: 200),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 12),
      decoration: BoxDecoration(
        color: color.withOpacity(.08),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: color.withOpacity(.18)),
      ),
      child: Row(
        children: [
          Icon(
            value ? Icons.lock_clock_rounded : Icons.event_available_rounded,
            color: color,
            size: 24,
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              value
                  ? 'إغلاق العيادة في هذا اليوم'
                  : 'فتح العيادة في هذا اليوم',
              style: TextStyle(
                fontSize: 14,
                fontWeight: FontWeight.w900,
                color: color,
              ),
            ),
          ),
          Switch(
            value: value,
            activeColor: color,
            onChanged: onChanged,
          ),
        ],
      ),
    );
  }
}