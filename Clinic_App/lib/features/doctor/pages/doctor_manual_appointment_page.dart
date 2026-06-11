import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorManualAppointmentPage extends StatefulWidget {
  const DoctorManualAppointmentPage({super.key});

  @override
  State<DoctorManualAppointmentPage> createState() =>
      _DoctorManualAppointmentPageState();
}

class _DoctorManualAppointmentPageState
    extends State<DoctorManualAppointmentPage> {
  late final DoctorService _service;
  final _name = TextEditingController();
  final _phone = TextEditingController();
  final _notes = TextEditingController();
  List<DoctorClinic> _clinics = [];
  DoctorClinic? _clinic;
  DateTime? _date;
  bool _loading = true;
  bool _saving = false;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);
    _load();
  }

  @override
  void dispose() {
    _name.dispose();
    _phone.dispose();
    _notes.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    try {
      _clinics = await _service.getClinics();
      _clinic = _clinics.isEmpty ? null : _clinics.first;
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _submit() async {
    if (_clinic == null || _date == null || _name.text.trim().isEmpty) {
      showAppSnackBar(context, 'أكمل بيانات الحجز.');
      return;
    }
    setState(() => _saving = true);
    try {
      await _service.createManualAppointment(
        clinicId: _clinic!.id,
        appointmentDate: _date!,
        patientName: _name.text,
        patientPhoneNumber: _phone.text,
        notes: _notes.text,
      );
      if (!mounted) return;
      showAppSnackBar(context, 'تم إضافة الحجز.');
      context.pop();
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  Future<void> _pickDate() async {
    final date = await showDatePicker(
      context: context,
      firstDate: DateTime.now(),
      lastDate: DateTime.now().add(const Duration(days: 90)),
      initialDate: DateTime.now(),
    );
    if (date != null) setState(() => _date = date);
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
    title: 'إضافة حجز يدوي',
    showBackButton: true,
    backRoute: '/doctor/appointments',
    child: _loading
        ? const Center(child: CircularProgressIndicator())
        : ListView(
            padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
            children: [
              DropdownButtonFormField<DoctorClinic>(
                value: _clinic,
                items: _clinics
                    .map(
                      (clinic) => DropdownMenuItem(
                        value: clinic,
                        child: Text(clinic.name),
                      ),
                    )
                    .toList(),
                onChanged: (value) => setState(() => _clinic = value),
                decoration: const InputDecoration(labelText: 'العيادة'),
              ),
              const SizedBox(height: 10),
              TextField(
                controller: _name,
                decoration: const InputDecoration(labelText: 'اسم المريض'),
              ),
              const SizedBox(height: 10),
              TextField(
                controller: _phone,
                keyboardType: TextInputType.phone,
                decoration: const InputDecoration(labelText: 'رقم الهاتف'),
              ),
              const SizedBox(height: 10),
              OutlinedButton.icon(
                onPressed: _pickDate,
                icon: const Icon(Icons.calendar_month_rounded),
                label: Text(
                  _date == null
                      ? 'اختيار تاريخ الحجز'
                      : '${_date!.year}/${_date!.month}/${_date!.day}',
                ),
              ),
              const SizedBox(height: 10),
              TextField(
                controller: _notes,
                minLines: 3,
                maxLines: 5,
                decoration: const InputDecoration(labelText: 'ملاحظات'),
              ),
              const SizedBox(height: 16),
              FilledButton.icon(
                onPressed: _saving ? null : _submit,
                icon: const Icon(Icons.save_rounded),
                label: Text(_saving ? 'جاري الحفظ...' : 'حفظ الحجز'),
              ),
            ],
          ),
  );
}
