import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
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
  late final TextEditingController _start;
  late final TextEditingController _end;
  late final TextEditingController _max;
  bool _saving = false;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);
    _available = widget.day.isAvailable;
    _start = TextEditingController(text: widget.day.startTime ?? '09:00:00');
    _end = TextEditingController(text: widget.day.endTime ?? '17:00:00');
    _max = TextEditingController(text: '${widget.day.maxAppointments ?? 10}');
  }

  @override
  void dispose() {
    _start.dispose();
    _end.dispose();
    _max.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    setState(() => _saving = true);
    try {
      await _service.updateAvailability(
        clinicId: widget.day.clinicId,
        dayId: widget.day.dayId,
        startTime: _start.text.trim(),
        endTime: _end.text.trim(),
        maxAppointments: int.tryParse(_max.text.trim()) ?? 10,
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
        SwitchListTile(
          value: _available,
          onChanged: (value) => setState(() => _available = value),
          title: Text(widget.day.dayName),
        ),
        TextField(
          controller: _start,
          decoration: const InputDecoration(labelText: 'وقت البداية HH:mm:ss'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _end,
          decoration: const InputDecoration(labelText: 'وقت النهاية HH:mm:ss'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _max,
          keyboardType: TextInputType.number,
          decoration: const InputDecoration(labelText: 'أقصى عدد حجوزات'),
        ),
        const SizedBox(height: 16),
        FilledButton.icon(
          onPressed: _saving ? null : _save,
          icon: const Icon(Icons.save_rounded),
          label: Text(_saving ? 'جاري الحفظ...' : 'حفظ'),
        ),
      ],
    ),
  );
}
