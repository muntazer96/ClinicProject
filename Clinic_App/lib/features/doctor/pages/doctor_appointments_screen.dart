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

class DoctorAppointmentsScreen extends StatefulWidget {
  const DoctorAppointmentsScreen({super.key});

  @override
  State<DoctorAppointmentsScreen> createState() => _DoctorAppointmentsScreenState();
}

class _DoctorAppointmentsScreenState extends State<DoctorAppointmentsScreen> {
  late final DoctorService _service;
  List<DoctorAppointment> _items = [];
  bool _loading = true;
  int? _status;
  DateTime? _fromDate;
  DateTime? _toDate;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      _items = await _service.getAppointments(
        status: _status,
        fromDate: _fromDate,
        toDate: _toDate,
      );
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _toggle(DoctorAppointment item) async {
    try {
      await _service.toggleAppointment(item.id);
      await _load();
      if (mounted) showAppSnackBar(context, 'تم تحديث حالة الحجز.');
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    }
  }

  Future<void> _complete(DoctorAppointment item) async {
    try {
      await _service.completeAppointment(item.id);
      await _load();
      if (mounted) showAppSnackBar(context, 'تم إكمال الحجز.');
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    }
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
    title: 'إدارة الحجوزات',
    child: Stack(
      children: [
        Column(
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 10, 16, 0),
              child: OutlinedButton.icon(
                onPressed: _pickDateRange,
                icon: const Icon(Icons.calendar_month_rounded),
                label: Text(
                  _fromDate == null
                      ? 'فلترة حسب التاريخ'
                      : '${DateFormat('yyyy/MM/dd').format(_fromDate!)} - ${DateFormat('yyyy/MM/dd').format(_toDate ?? _fromDate!)}',
                ),
              ),
            ),
            SizedBox(
              height: 56,
              child: ListView(
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 9),
                scrollDirection: Axis.horizontal,
                children: [
                  _StatusChip('الكل', null, _status, _setStatus),
                  _StatusChip('قيد الانتظار', 0, _status, _setStatus),
                  _StatusChip('مؤكد', 1, _status, _setStatus),
                  _StatusChip('ملغي', 2, _status, _setStatus),
                  _StatusChip('مكتمل', 3, _status, _setStatus),
                ],
              ),
            ),
            Expanded(
              child: RefreshIndicator(
                onRefresh: _load,
                child: _loading
                    ? const Center(child: CircularProgressIndicator())
                    : _items.isEmpty
                    ? const DoctorEmptyState(
                        icon: Icons.event_busy_rounded,
                        message: 'لا توجد حجوزات بهذه الحالة.',
                      )
                    : ListView.builder(
                        padding: const EdgeInsets.fromLTRB(16, 6, 16, 86),
                        itemCount: _items.length,
                        itemBuilder: (context, index) => _AppointmentCard(
                          item: _items[index],
                          onToggle: _toggle,
                          onComplete: _complete,
                        ),
                      ),
              ),
            ),
          ],
        ),
        PositionedDirectional(
          start: 18,
          bottom: 16,
          child: FloatingActionButton.small(
            heroTag: 'doctor-add-appointment',
            backgroundColor: AppColors.primary,
            foregroundColor: Colors.white,
            onPressed: () async {
              await context.push('/doctor/appointments/manual');
              await _load();
            },
            child: const Icon(Icons.add_rounded),
          ),
        ),
      ],
    ),
  );

  void _setStatus(int? value) {
    setState(() => _status = value);
    _load();
  }

  Future<void> _pickDateRange() async {
    final range = await showDateRangePicker(
      context: context,
      firstDate: DateTime(2020),
      lastDate: DateTime.now().add(const Duration(days: 365)),
      initialDateRange: _fromDate == null
          ? null
          : DateTimeRange(start: _fromDate!, end: _toDate ?? _fromDate!),
    );
    if (range == null) return;
    setState(() {
      _fromDate = range.start;
      _toDate = range.end;
    });
    await _load();
  }
}

class _AppointmentCard extends StatelessWidget {
  const _AppointmentCard({
    required this.item,
    required this.onToggle,
    required this.onComplete,
  });

  final DoctorAppointment item;
  final ValueChanged<DoctorAppointment> onToggle;
  final ValueChanged<DoctorAppointment> onComplete;

  @override
  Widget build(BuildContext context) {
    final statusColor = item.status == 2
        ? AppColors.danger
        : item.status == 3
        ? AppColors.success
        : item.status == 1
        ? AppColors.primary
        : AppColors.primaryDark;

    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: DoctorSectionCard(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                DoctorStatusPill(label: item.statusLabel, color: statusColor),
                const Spacer(),
                Text(
                  item.patientName,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w900),
                ),
              ],
            ),
            const SizedBox(height: 8),
            DoctorInfoRow(
              icon: Icons.local_hospital_rounded,
              text: item.clinicName,
            ),
            DoctorInfoRow(
              icon: Icons.calendar_month_rounded,
              text:
                  '${DateFormat('yyyy/MM/dd').format(item.appointmentDate)} - رقم الدور ${item.queueNumber}',
            ),
            const SizedBox(height: 12),
            Row(
              children: [
                if (item.canToggle)
                  Expanded(
                    child: DoctorActionButton(
                      label: item.status == 0 ? 'قبول / تأكيد' : 'رفض / إلغاء',
                      icon: item.status == 0
                          ? Icons.check_circle_outline_rounded
                          : Icons.cancel_outlined,
                      danger: item.status != 0,
                      onPressed: () => onToggle(item),
                    ),
                  ),
                if (item.canToggle && item.canComplete) const SizedBox(width: 8),
                if (item.canComplete)
                  Expanded(
                    child: DoctorActionButton(
                      label: 'إكمال',
                      icon: Icons.task_alt_rounded,
                      onPressed: () => onComplete(item),
                    ),
                  ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _StatusChip extends StatelessWidget {
  const _StatusChip(this.label, this.value, this.selected, this.onSelected);

  final String label;
  final int? value;
  final int? selected;
  final ValueChanged<int?> onSelected;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsetsDirectional.only(end: 8),
    child: ChoiceChip(
      label: Text(label),
      selected: selected == value,
      onSelected: (_) => onSelected(value),
    ),
  );
}
