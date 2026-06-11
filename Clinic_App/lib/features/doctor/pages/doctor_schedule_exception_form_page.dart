import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
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
  DateTime? _date;
  bool _closed = true;
  bool _saving = false;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);
    _date = DateTime.now();
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
      initialDate: _date ?? DateTime.now(),
    );
    if (picked != null) setState(() => _date = picked);
  }

  Future<void> _save() async {
    if (_date == null) return;
    setState(() => _saving = true);
    try {
      await _service.saveException(
        clinicId: widget.clinic.id,
        date: _date!,
        isClosed: _closed,
        reason: _reason.text,
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
        OutlinedButton.icon(
          onPressed: _pickDate,
          icon: const Icon(Icons.calendar_month_rounded),
          label: Text('${_date!.year}/${_date!.month}/${_date!.day}'),
        ),
        const SizedBox(height: 10),
        SwitchListTile(
          value: _closed,
          onChanged: (value) => setState(() => _closed = value),
          title: const Text('إغلاق العيادة في هذا اليوم'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _reason,
          decoration: const InputDecoration(labelText: 'السبب'),
        ),
        const SizedBox(height: 16),
        FilledButton.icon(
          onPressed: _saving ? null : _save,
          icon: const Icon(Icons.save_rounded),
          label: Text(_saving ? 'جاري الحفظ...' : 'حفظ الاستثناء'),
        ),
      ],
    ),
  );
}
