import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorScheduleDayFormPage extends StatefulWidget {
  const DoctorScheduleDayFormPage({super.key, required this.day});

  final DoctorAvailability day;

  @override
  State<DoctorScheduleDayFormPage> createState() =>
      _DoctorScheduleDayFormPageState();
}

class _DoctorScheduleDayFormPageState extends State<DoctorScheduleDayFormPage> {
  late final DoctorService _service;
  late bool _available;
  late TimeOfDay _startTime;
  late TimeOfDay _endTime;
  late final TextEditingController _max;
  bool _saving = false;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);
    _available = widget.day.isAvailable;
    _startTime = _parseTime(widget.day.startTime ?? '09:00:00');
    _endTime = _parseTime(widget.day.endTime ?? '17:00:00');
    _max = TextEditingController(text: '${widget.day.maxAppointments ?? 10}');
  }

  @override
  void dispose() {
    _max.dispose();
    super.dispose();
  }

  TimeOfDay _parseTime(String value) {
    final parts = value.split(':');
    return TimeOfDay(
      hour: int.tryParse(parts.isNotEmpty ? parts[0] : '') ?? 9,
      minute: int.tryParse(parts.length > 1 ? parts[1] : '') ?? 0,
    );
  }

  String _toApiTime(TimeOfDay time) {
    final h = time.hour.toString().padLeft(2, '0');
    final m = time.minute.toString().padLeft(2, '0');
    return '$h:$m:00';
  }

  String _displayTime(TimeOfDay time) {
    final h = time.hour.toString().padLeft(2, '0');
    final m = time.minute.toString().padLeft(2, '0');
    return '$h:$m';
  }

  Future<void> _pickTime({
    required TimeOfDay initial,
    required ValueChanged<TimeOfDay> onPicked,
  }) async {
    final picked = await showTimePicker(
      context: context,
      initialTime: initial,
      builder: (context, child) {
        return Directionality(
          textDirection: TextDirection.rtl,
          child: child!,
        );
      },
    );

    if (picked != null) {
      setState(() => onPicked(picked));
    }
  }

  Future<void> _save() async {
    final maxAppointments = int.tryParse(_max.text.trim()) ?? 10;

    if (maxAppointments <= 0) {
      showAppSnackBar(context, 'أدخل عدد حجوزات صحيح.');
      return;
    }

    setState(() => _saving = true);

    try {
      await _service.updateAvailability(
        clinicId: widget.day.clinicId,
        dayId: widget.day.dayId,
        startTime: _toApiTime(_startTime),
        endTime: _toApiTime(_endTime),
        maxAppointments: maxAppointments,
        isAvailable: _available,
      );

      if (!mounted) return;
      showAppSnackBar(context, 'تم حفظ وقت الدوام.');
      context.pop();
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
        title: 'تعديل الدوام',
        showBackButton: true,
        backRoute: '/doctor/schedule',
        child: ListView(
          padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
          children: [
            _DayHeroCard(
              dayName: widget.day.dayName,
              available: _available,
              onChanged: (value) => setState(() => _available = value),
            ),

            const SizedBox(height: 14),

            AnimatedOpacity(
              duration: const Duration(milliseconds: 200),
              opacity: _available ? 1 : .45,
              child: IgnorePointer(
                ignoring: !_available,
                child: Column(
                  children: [
                    _SectionCard(
                      title: 'وقت الدوام',
                      icon: Icons.access_time_rounded,
                      children: [
                        Row(
                          children: [
                            Expanded(
                              child: _TimeBox(
                                title: 'بداية الدوام',
                                time: _displayTime(_startTime),
                                icon: Icons.play_arrow_rounded,
                                onTap: () => _pickTime(
                                  initial: _startTime,
                                  onPicked: (value) => _startTime = value,
                                ),
                              ),
                            ),
                            const SizedBox(width: 10),
                            Expanded(
                              child: _TimeBox(
                                title: 'نهاية الدوام',
                                time: _displayTime(_endTime),
                                icon: Icons.stop_rounded,
                                onTap: () => _pickTime(
                                  initial: _endTime,
                                  onPicked: (value) => _endTime = value,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),

                    const SizedBox(height: 14),

                    _SectionCard(
                      title: 'سعة الحجوزات',
                      icon: Icons.groups_rounded,
                      children: [
                        TextField(
                          controller: _max,
                          keyboardType: TextInputType.number,
                          decoration: const InputDecoration(
                            labelText: 'أقصى عدد حجوزات لهذا اليوم',
                            prefixIcon: Icon(Icons.confirmation_number_rounded),
                            suffixText: 'حجز',
                          ),
                        ),
                        const SizedBox(height: 10),
                        Text(
                          'هذا العدد يحدد أعلى رقم دور يمكن حجزه في هذا اليوم.',
                          style: TextStyle(
                            fontSize: 12,
                            color: Colors.grey.shade600,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ],
                    ),
                  ],
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
                  _saving ? 'جاري الحفظ...' : 'حفظ وقت الدوام',
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

class _DayHeroCard extends StatelessWidget {
  const _DayHeroCard({
    required this.dayName,
    required this.available,
    required this.onChanged,
  });

  final String dayName;
  final bool available;
  final ValueChanged<bool> onChanged;

  @override
  Widget build(BuildContext context) {
    final color = available ? AppColors.primary : AppColors.muted;

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: available
              ? const [Color(0xFF0F7F73), Color(0xFF0D625C)]
              : const [Color(0xFF7B8583), Color(0xFF5F6967)],
          begin: Alignment.topRight,
          end: Alignment.bottomLeft,
        ),
        borderRadius: BorderRadius.circular(22),
        boxShadow: [
          BoxShadow(
            color: color.withOpacity(.18),
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
              available ? Icons.event_available_rounded : Icons.event_busy_rounded,
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
                  dayName,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 22,
                    fontWeight: FontWeight.w900,
                  ),
                ),
                const SizedBox(height: 5),
                Text(
                  available
                      ? 'العيادة تستقبل الحجوزات بهذا اليوم'
                      : 'هذا اليوم غير متاح للحجز',
                  style: TextStyle(
                    color: Colors.white.withOpacity(.86),
                    fontSize: 12.5,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ],
            ),
          ),
          Switch(
            value: available,
            activeColor: Colors.white,
            activeTrackColor: Colors.white.withOpacity(.35),
            inactiveThumbColor: Colors.white,
            inactiveTrackColor: Colors.white.withOpacity(.25),
            onChanged: onChanged,
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
    required this.children,
  });

  final String title;
  final IconData icon;
  final List<Widget> children;

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
          ...children,
        ],
      ),
    );
  }
}

class _TimeBox extends StatelessWidget {
  const _TimeBox({
    required this.title,
    required this.time,
    required this.icon,
    required this.onTap,
  });

  final String title;
  final String time;
  final IconData icon;
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
          padding: const EdgeInsets.all(13),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(16),
            border: Border.all(color: const Color(0xFFE3ECEA)),
          ),
          child: Column(
            children: [
              Icon(icon, color: AppColors.primary, size: 24),
              const SizedBox(height: 8),
              Text(
                title,
                style: TextStyle(
                  fontSize: 12,
                  color: Colors.grey.shade600,
                  fontWeight: FontWeight.w700,
                ),
              ),
              const SizedBox(height: 5),
              Text(
                time,
                style: const TextStyle(
                  fontSize: 21,
                  fontWeight: FontWeight.w900,
                  letterSpacing: .4,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}