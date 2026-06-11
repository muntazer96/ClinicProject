import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorClinicsScreen extends StatefulWidget {
  const DoctorClinicsScreen({super.key});

  @override
  State<DoctorClinicsScreen> createState() => _DoctorClinicsScreenState();
}

class _DoctorClinicsScreenState extends State<DoctorClinicsScreen> {
  late final DoctorService _service;
  List<DoctorClinic> _items = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      _items = await _service.getClinics();
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _delete(DoctorClinic clinic) async {
    await _service.deleteClinic(clinic.id);
    await _load();
    if (mounted) showAppSnackBar(context, 'تم حذف العيادة.');
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
    title: 'عياداتي',
    child: Column(
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(16, 10, 16, 0),
          child: DoctorPrimaryButton(
            label: 'إضافة عيادة جديدة',
            icon: Icons.add_rounded,
            onPressed: () async {
              await context.push('/doctor/clinics/form');
              await _load();
            },
          ),
        ),
        Expanded(
          child: RefreshIndicator(
            onRefresh: _load,
            child: _loading
                ? const Center(child: CircularProgressIndicator())
                : _items.isEmpty
                ? const DoctorEmptyState(
                    icon: Icons.local_hospital_outlined,
                    message: 'لا توجد عيادات مرتبطة بحساب الطبيب.',
                  )
                : ListView.builder(
                    padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
                    itemCount: _items.length,
                    itemBuilder: (context, index) => _ClinicCard(
                      clinic: _items[index],
                      onEdit: () async {
                        await context.push('/doctor/clinics/form', extra: _items[index]);
                        await _load();
                      },
                      onSchedule: () => context.push(
                        '/doctor/clinics/${_items[index].id}/schedule',
                        extra: _items[index],
                      ),
                      onDelete: () => _delete(_items[index]),
                    ),
                  ),
          ),
        ),
      ],
    ),
  );
}

class _ClinicCard extends StatelessWidget {
  const _ClinicCard({
    required this.clinic,
    required this.onEdit,
    required this.onSchedule,
    required this.onDelete,
  });

  final DoctorClinic clinic;
  final VoidCallback onEdit;
  final VoidCallback onSchedule;
  final VoidCallback onDelete;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: 12),
    child: DoctorSectionCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 38,
                height: 38,
                decoration: BoxDecoration(
                  color: AppColors.surfaceMuted,
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(Icons.local_hospital_rounded, color: AppColors.primary),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      clinic.name,
                      style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w900),
                    ),
                    DoctorInfoRow(
                      icon: Icons.place_outlined,
                      text: '${clinic.iraqiProvinceName} - ${clinic.address}',
                    ),
                    if (clinic.phoneNumber?.isNotEmpty == true)
                      DoctorInfoRow(icon: Icons.phone_outlined, text: clinic.phoneNumber!),
                    DoctorInfoRow(
                      icon: Icons.payments_outlined,
                      text: clinic.consultationPrice == null
                          ? 'بدون سعر'
                          : '${clinic.consultationPrice!.toStringAsFixed(0)} د.ع',
                    ),
                  ],
                ),
              ),
              DoctorStatusPill(
                label: clinic.isVisible ? 'ظاهرة' : 'مخفية',
                color: clinic.isVisible ? AppColors.primary : AppColors.muted,
              ),
            ],
          ),
          const SizedBox(height: 14),
          Row(
            children: [
              Expanded(
                child: DoctorActionButton(
                  label: 'تعديل',
                  icon: Icons.edit_outlined,
                  onPressed: onEdit,
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: DoctorActionButton(
                  label: 'أوقات الدوام',
                  icon: Icons.schedule_rounded,
                  onPressed: onSchedule,
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: DoctorActionButton(
                  label: 'حذف',
                  icon: Icons.delete_outline,
                  danger: true,
                  onPressed: onDelete,
                ),
              ),
            ],
          ),
        ],
      ),
    ),
  );
}
