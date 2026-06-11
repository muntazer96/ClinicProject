import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorScheduleScreen extends StatefulWidget {
  const DoctorScheduleScreen({super.key, this.clinic});

  final DoctorClinic? clinic;

  @override
  State<DoctorScheduleScreen> createState() => _DoctorScheduleScreenState();
}

class _DoctorScheduleScreenState extends State<DoctorScheduleScreen> {
  late final DoctorService _service;
  DoctorClinic? _clinic;
  List<DoctorAvailability> _availability = [];
  List<ClinicExceptionDay> _exceptions = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);
    _clinic = widget.clinic;
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      if (_clinic == null) {
        final clinics = await _service.getClinics();
        _clinic = clinics.isEmpty ? null : clinics.first;
      }
      final clinic = _clinic;
      if (clinic != null) {
        final result = await Future.wait([
          _service.getAvailability(clinic.id),
          _service.getExceptions(clinic.id),
        ]);
        _availability = result[0] as List<DoctorAvailability>;
        _exceptions = result[1] as List<ClinicExceptionDay>;
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
    title: 'أوقات الدوام',
    showBackButton: widget.clinic != null,
    backRoute: '/doctor/clinics',
    child: _loading
        ? const Center(child: CircularProgressIndicator())
        : _clinic == null
        ? const DoctorEmptyState(
            icon: Icons.schedule_outlined,
            message: 'لا توجد عيادة لإدارة الدوام.',
          )
        : RefreshIndicator(
            onRefresh: _load,
            child: ListView(
              padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
              children: [
                DoctorSectionCard(
                  child: Row(
                    children: [
                      const Icon(Icons.local_hospital_rounded, color: AppColors.primary),
                      const SizedBox(width: 10),
                      Expanded(
                        child: Text(
                          _clinic!.name,
                          style: const TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.w900,
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 12),
                ..._availability.map(
                  (item) => _ScheduleDayCard(
                    item: item,
                    onEdit: () async {
                      await context.push('/doctor/schedule/day', extra: item);
                      await _load();
                    },
                  ),
                ),
                const SizedBox(height: 10),
                DoctorPrimaryButton(
                  label: 'إضافة استثناء دوام',
                  icon: Icons.event_busy_rounded,
                  onPressed: () async {
                    await context.push('/doctor/schedule/exception', extra: _clinic);
                    await _load();
                  },
                ),
                if (_exceptions.isNotEmpty) ...[
                  const SizedBox(height: 16),
                  const Text(
                    'استثناءات الدوام',
                    style: TextStyle(fontWeight: FontWeight.w900, fontSize: 16),
                  ),
                  const SizedBox(height: 8),
                  ..._exceptions.map(
                    (item) => Padding(
                      padding: const EdgeInsets.only(bottom: 8),
                      child: DoctorSectionCard(
                        child: Row(
                          children: [
                            const Icon(Icons.event_busy_rounded, color: AppColors.primary),
                            const SizedBox(width: 10),
                            Expanded(
                              child: Text(
                                '${item.exceptionDate.year}/${item.exceptionDate.month}/${item.exceptionDate.day} - ${item.closureReason ?? 'استثناء'}',
                                style: const TextStyle(fontWeight: FontWeight.w800),
                              ),
                            ),
                            IconButton(
                              tooltip: 'حذف',
                              onPressed: () async {
                                await _service.deleteException(item.id);
                                await _load();
                              },
                              icon: const Icon(Icons.delete_outline_rounded),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                ],
              ],
            ),
          ),
  );
}

class _ScheduleDayCard extends StatelessWidget {
  const _ScheduleDayCard({required this.item, required this.onEdit});

  final DoctorAvailability item;
  final VoidCallback onEdit;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: 8),
    child: DoctorSectionCard(
      child: Row(
        children: [
          DoctorStatusPill(
            label: item.isAvailable ? 'متاح' : 'غير متاح',
            color: item.isAvailable ? AppColors.primary : AppColors.muted,
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  item.dayName,
                  style: const TextStyle(fontWeight: FontWeight.w900),
                ),
                const SizedBox(height: 2),
                Text(
                  item.isAvailable
                      ? '${item.startTime ?? '--'} - ${item.endTime ?? '--'} | ${item.maxAppointments ?? 0} حجز'
                      : 'يمكنك تفعيل هذا اليوم وإضافة أوقات الدوام',
                  style: const TextStyle(color: AppColors.muted, fontSize: 12),
                ),
              ],
            ),
          ),
          IconButton(
            tooltip: 'تعديل',
            onPressed: onEdit,
            icon: const Icon(Icons.edit_outlined),
          ),
        ],
      ),
    ),
  );
}
