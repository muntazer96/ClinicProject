import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
import '../../../core/phone_number_validator.dart';
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
    if (!isValidIraqiPhone(_phone.text)) {
      showAppSnackBar(context, iraqiPhoneError, type: AppSnackBarType.warning);
      return;
    }

    setState(() => _saving = true);

    try {
      await _service.createManualAppointment(
        clinicId: _clinic!.id,
        appointmentDate: _date!,
        patientName: _name.text.trim(),
        patientPhoneNumber: _phone.text.trim(),
        notes: _notes.text.trim(),
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
      initialDate: _date ?? DateTime.now(),
    );

    if (date != null) {
      setState(() => _date = date);
    }
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
              const _HeroCard(),

              const SizedBox(height: 14),

              _SectionCard(
                title: 'اختيار العيادة',
                icon: Icons.local_hospital_rounded,
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
                    decoration: const InputDecoration(
                      labelText: 'العيادة',
                      prefixIcon: Icon(Icons.apartment_rounded),
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 14),

              _SectionCard(
                title: 'بيانات المريض',
                icon: Icons.person_rounded,
                children: [
                  _AppTextField(
                    controller: _name,
                    label: 'اسم المريض',
                    icon: Icons.badge_rounded,
                  ),
                  const SizedBox(height: 12),
                  _AppTextField(
                    controller: _phone,
                    label: 'رقم الهاتف',
                    icon: Icons.phone_rounded,
                    keyboardType: TextInputType.phone,
                    inputFormatters: iraqiPhoneInputFormatters,
                  ),
                ],
              ),

              const SizedBox(height: 14),

              _SectionCard(
                title: 'موعد الحجز',
                icon: Icons.calendar_month_rounded,
                children: [_DateBox(date: _date, onTap: _pickDate)],
              ),

              const SizedBox(height: 14),

              _SectionCard(
                title: 'ملاحظات إضافية',
                icon: Icons.notes_rounded,
                children: [
                  TextField(
                    controller: _notes,
                    minLines: 3,
                    maxLines: 5,
                    decoration: const InputDecoration(
                      labelText: 'ملاحظات',
                      prefixIcon: Icon(Icons.edit_note_rounded),
                      alignLabelWithHint: true,
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 18),

              SizedBox(
                height: 52,
                child: ElevatedButton.icon(
                  onPressed: _saving ? null : _submit,
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
                    _saving ? 'جاري الحفظ...' : 'حفظ الحجز',
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

class _HeroCard extends StatelessWidget {
  const _HeroCard();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [Color(0xFF0F7F73), Color(0xFF0D625C)],
          begin: Alignment.topRight,
          end: Alignment.bottomLeft,
        ),
        borderRadius: BorderRadius.circular(22),
        boxShadow: [
          BoxShadow(
            color: AppColors.primary.withOpacity(.18),
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
            child: const Icon(
              Icons.event_available_rounded,
              color: Colors.white,
              size: 31,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              'أضف حجزاً يدوياً للمريض داخل جدول العيادة',
              style: TextStyle(
                color: Colors.white.withOpacity(.96),
                fontSize: 15,
                height: 1.5,
                fontWeight: FontWeight.w900,
              ),
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
        color: context.appSurface,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: context.appBorder),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(context.isDark ? .18 : .025),
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

class _AppTextField extends StatelessWidget {
  const _AppTextField({
    required this.controller,
    required this.label,
    required this.icon,
    this.keyboardType,
    this.inputFormatters,
  });

  final TextEditingController controller;
  final String label;
  final IconData icon;
  final TextInputType? keyboardType;
  final List<TextInputFormatter>? inputFormatters;

  @override
  Widget build(BuildContext context) {
    return TextField(
      controller: controller,
      keyboardType: keyboardType,
      inputFormatters: inputFormatters,
      decoration: InputDecoration(labelText: label, prefixIcon: Icon(icon)),
    );
  }
}

class _DateBox extends StatelessWidget {
  const _DateBox({required this.date, required this.onTap});

  final DateTime? date;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final selected = date != null;

    return Material(
      color: selected ? context.appSoftBlue : context.appSurfaceMuted,
      borderRadius: BorderRadius.circular(16),
      child: InkWell(
        borderRadius: BorderRadius.circular(16),
        onTap: onTap,
        child: Container(
          padding: const EdgeInsets.all(14),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(16),
            border: Border.all(
              color: selected ? AppColors.primary : context.appBorder,
            ),
          ),
          child: Row(
            children: [
              Icon(
                Icons.today_rounded,
                color: selected ? AppColors.primary : context.appMuted,
                size: 25,
              ),
              const SizedBox(width: 10),
              Expanded(
                child: Text(
                  selected
                      ? DateFormat('yyyy/MM/dd').format(date!)
                      : 'اختيار تاريخ الحجز',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w900,
                    color: selected
                        ? Theme.of(context).colorScheme.primary
                        : context.appMuted,
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
