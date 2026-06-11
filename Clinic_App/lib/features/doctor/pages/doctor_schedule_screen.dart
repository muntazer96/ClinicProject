import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

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
        _availability = _completeWeek(
          clinic.id,
          result[0] as List<DoctorAvailability>,
        );
        _exceptions = result[1] as List<ClinicExceptionDay>;
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  List<DoctorAvailability> _completeWeek(
    int clinicId,
    List<DoctorAvailability> existing,
  ) {
    const names = {
      1: 'الأحد',
      2: 'الاثنين',
      3: 'الثلاثاء',
      4: 'الأربعاء',
      5: 'الخميس',
      6: 'الجمعة',
      7: 'السبت',
    };
    return List.generate(7, (index) {
      final dayId = index + 1;
      return existing.where((item) => item.dayId == dayId).firstOrNull ??
          DoctorAvailability(
            id: 0,
            clinicId: clinicId,
            dayId: dayId,
            dayName: names[dayId]!,
            isAvailable: false,
            startTime: '09:00:00',
            endTime: '17:00:00',
            maxAppointments: 10,
          );
    });
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
                      child: Text(
                        _clinic!.name,
                        style: const TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.w900,
                        ),
                      ),
                    ),
                    const SizedBox(height: 12),
                    ..._availability.map(
                      (item) => Padding(
                        padding: const EdgeInsets.only(bottom: 8),
                        child: DoctorSectionCard(
                          child: ListTile(
                            contentPadding: EdgeInsets.zero,
                            title: Text(item.dayName),
                            subtitle: Text(
                              item.isAvailable
                                  ? '${item.startTime ?? '--'} - ${item.endTime ?? '--'} | ${item.maxAppointments ?? 0} حجز'
                                  : 'غير متاح',
                            ),
                            trailing: IconButton(
                              tooltip: 'تعديل',
                              onPressed: () async {
                                await context.push(
                                  '/doctor/schedule/day',
                                  extra: item,
                                );
                                await _load();
                              },
                              icon: const Icon(Icons.edit_outlined),
                            ),
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(height: 10),
                    FilledButton.icon(
                      onPressed: () async {
                        await context.push(
                          '/doctor/schedule/exception',
                          extra: _clinic,
                        );
                        await _load();
                      },
                      icon: const Icon(Icons.event_busy_rounded),
                      label: const Text('إضافة استثناء دوام'),
                    ),
                    const SizedBox(height: 12),
                    ..._exceptions.map(
                      (item) => Padding(
                        padding: const EdgeInsets.only(bottom: 8),
                        child: DoctorSectionCard(
                          child: ListTile(
                            contentPadding: EdgeInsets.zero,
                            title: Text(
                              '${item.exceptionDate.year}/${item.exceptionDate.month}/${item.exceptionDate.day}',
                            ),
                            subtitle: Text(item.closureReason ?? 'استثناء'),
                            trailing: IconButton(
                              tooltip: 'حذف',
                              onPressed: () async {
                                await _service.deleteException(item.id);
                                await _load();
                              },
                              icon: const Icon(Icons.delete_outline_rounded),
                            ),
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
  );
}