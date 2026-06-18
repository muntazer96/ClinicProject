import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
import '../../../core/external_links.dart';
import '../../../widgets/long_press_button.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorAppointmentsScreen extends StatefulWidget {
  const DoctorAppointmentsScreen({super.key});

  @override
  State<DoctorAppointmentsScreen> createState() =>
      _DoctorAppointmentsScreenState();
}

class _DoctorAppointmentsScreenState extends State<DoctorAppointmentsScreen> {
  late final DoctorService _service;
  List<DoctorAppointment> _items = [];
  List<DoctorClinic> _clinics = [];
  bool _loading = true;
  int? _status;
  int? _clinicId;
  DateTime? _fromDate;
  DateTime? _toDate;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);
    final today = DateTime.now();
    _fromDate = DateTime(today.year, today.month, today.day);
    _toDate = _fromDate;
    _loadInitial();
  }

  Future<void> _loadInitial() async {
    setState(() => _loading = true);
    try {
      _clinics = await _service.getClinics();
      await _load(setLoading: false);
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _load({bool setLoading = true}) async {
    if (setLoading) setState(() => _loading = true);
    try {
      _items = await _service.getAppointments(
        status: _status,
        clinicId: _clinicId,
        fromDate: _fromDate,
        toDate: _toDate,
      );
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted && setLoading) setState(() => _loading = false);
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

  Future<void> _reject(DoctorAppointment item) async {
    try {
      await _service.rejectPendingAppointment(item.id);
      await _load();
      if (mounted) showAppSnackBar(context, 'Booking rejected.');
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
              child: SizedBox(
                width: double.infinity,
                height: 48,
                child: OutlinedButton.icon(
                  onPressed: _pickDateRange,
                  icon: const Icon(Icons.calendar_month_rounded),
                  label: Text(
                    _fromDate == null
                        ? 'فلترة حسب التاريخ'
                        : '${DateFormat('yyyy/MM/dd').format(_fromDate!)} - ${DateFormat('yyyy/MM/dd').format(_toDate ?? _fromDate!)}',
                    overflow: TextOverflow.ellipsis,
                  ),
                  style: OutlinedButton.styleFrom(
                    foregroundColor: AppColors.primary,
                    side: const BorderSide(color: Color(0xFFDDE9E7)),
                    backgroundColor: Colors.white,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(14),
                    ),
                  ),
                ),
              ),
            ),

            if (_clinics.length > 1)
              Padding(
                padding: const EdgeInsets.fromLTRB(16, 10, 16, 0),
                child: SingleChildScrollView(
                  scrollDirection: Axis.horizontal,
                  child: Row(
                    children: [
                      _ClinicChip('الكل', null, _clinicId, _setClinic),
                      const SizedBox(width: 8),
                      ..._clinics.expand(
                        (clinic) => [
                          _ClinicChip(
                            clinic.name,
                            clinic.id,
                            _clinicId,
                            _setClinic,
                          ),
                          const SizedBox(width: 8),
                        ],
                      ),
                    ],
                  ),
                ),
              ),

            Padding(
              padding: const EdgeInsets.fromLTRB(16, 10, 16, 8),
              child: Wrap(
                spacing: 8,
                runSpacing: 8,
                alignment: WrapAlignment.center,
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
                        padding: const EdgeInsets.fromLTRB(16, 4, 16, 88),
                        itemCount: _items.length,
                        itemBuilder: (context, index) => _AppointmentCard(
                          item: _items[index],
                          onToggle: _toggle,
                          onReject: _reject,
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
          child: FloatingActionButton(
            heroTag: 'doctor-add-appointment',
            backgroundColor: AppColors.primary,
            foregroundColor: Colors.white,
            tooltip: 'إضافة حجز',
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

  void _setClinic(int? value) {
    setState(() => _clinicId = value);
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
    required this.onReject,
    required this.onComplete,
  });

  final DoctorAppointment item;
  final ValueChanged<DoctorAppointment> onToggle;
  final ValueChanged<DoctorAppointment> onReject;
  final ValueChanged<DoctorAppointment> onComplete;

  @override
  Widget build(BuildContext context) {
    final statusColor = item.status == 2
        ? AppColors.danger
        : item.status == 3
        ? AppColors.success
        : item.status == 1
        ? AppColors.primary
        : const Color(0xFFB7791F);

    final isGuest = item.isGuestBooking;
    final sourceColor = isGuest ? const Color(0xFFD6A20B) : AppColors.primary;
    final sourceBg = isGuest
        ? const Color(0xFFFFF7DF)
        : const Color(0xFFEAF7F5);

    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(
          color: isGuest ? const Color(0xFFE8CF83) : const Color(0xFFDDE9E7),
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(.035),
            blurRadius: 14,
            offset: const Offset(0, 7),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                Container(
                  width: 46,
                  height: 46,
                  decoration: BoxDecoration(
                    color: sourceBg,
                    borderRadius: BorderRadius.circular(14),
                  ),
                  child: Icon(
                    isGuest
                        ? Icons.person_add_alt_1_rounded
                        : Icons.verified_user_rounded,
                    color: sourceColor,
                  ),
                ),
                const SizedBox(width: 10),

                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        item.patientName,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: const TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.w900,
                        ),
                      ),
                      const SizedBox(height: 4),
                      Row(
                        children: [
                          Icon(
                            Icons.phone_rounded,
                            size: 14,
                            color: Colors.grey.shade600,
                          ),
                          const SizedBox(width: 4),
                          Expanded(
                            child: Text(
                              item.patientPhoneNumber,
                              overflow: TextOverflow.ellipsis,
                              style: TextStyle(
                                fontSize: 12,
                                color: Colors.grey.shade700,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),

                const SizedBox(width: 8),
                DoctorStatusPill(label: item.statusLabel, color: statusColor),
              ],
            ),

            const SizedBox(height: 12),

            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: [
                _SmallSourceBadge(
                  text: isGuest ? 'حجز زائر' : 'مستخدم مسجل',
                  icon: isGuest
                      ? Icons.person_outline_rounded
                      : Icons.account_circle_outlined,
                  color: sourceColor,
                  bg: sourceBg,
                ),
                if (item.isPhoneConfirmed)
                  const _SmallSourceBadge(
                    text: 'الهاتف مؤكد',
                    icon: Icons.verified_rounded,
                    color: AppColors.success,
                    bg: Color(0xFFEAF8EF),
                  ),
                if (item.hasReview)
                  const _SmallSourceBadge(
                    text: 'قيّم الطبيب',
                    icon: Icons.star_rounded,
                    color: Color(0xFFD6A20B),
                    bg: Color(0xFFFFF7DF),
                  ),
              ],
            ),

            const SizedBox(height: 12),
            const Divider(height: 1),
            const SizedBox(height: 12),

            Row(
              children: [
                Expanded(
                  child: _InfoTile(
                    icon: Icons.calendar_month_rounded,
                    title: 'التاريخ',
                    value: DateFormat(
                      'yyyy/MM/dd',
                    ).format(item.appointmentDate),
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: _InfoTile(
                    icon: Icons.confirmation_number_rounded,
                    title: 'رقم الدور',
                    value: '#${item.queueNumber}',
                  ),
                ),
              ],
            ),

            const SizedBox(height: 8),

            _InfoLine(
              icon: Icons.local_hospital_rounded,
              text: item.clinicName,
            ),

            if (item.clinicAddress.isNotEmpty)
              _InfoLine(
                icon: Icons.location_on_outlined,
                text: item.clinicAddress,
              ),

            if (item.code.isNotEmpty)
              _InfoLine(
                icon: Icons.qr_code_rounded,
                text: 'كود الحجز: ${item.code}',
              ),

            if (item.status == 2 && item.cancelledAt != null) ...[
              const SizedBox(height: 8),
              _WarningBox(
                text:
                    'تم الإلغاء بتاريخ ${DateFormat('yyyy/MM/dd - hh:mm a').format(item.cancelledAt!)}',
              ),
            ],

            if (item.canToggle || item.canComplete) ...[
              const SizedBox(height: 14),
              Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  if (item.patientPhoneNumber.trim().isNotEmpty)
                    DoctorActionButton(
                      label: 'اتصال',
                      icon: Icons.phone_rounded,
                      onPressed: () =>
                          openPhone(context, item.patientPhoneNumber),
                    ),
                  if (!item.isGuestBooking && item.patientUserId != null) ...[
                    if (item.patientPhoneNumber.trim().isNotEmpty)
                      const SizedBox(height: 8),
                    DoctorActionButton(
                      label: 'إرسال رسالة',
                      icon: Icons.chat_outlined,
                      onPressed: () => context.push(
                        '/doctor/messages/${item.patientUserId}'
                        '?otherUserName=${Uri.encodeComponent(item.patientName)}',
                      ),
                    ),
                  ],
                  if (item.patientPhoneNumber.trim().isNotEmpty &&
                      (item.canToggle || item.canComplete))
                    const SizedBox(height: 8),
                  if (item.status == 0) ...[
                    DoctorActionButton(
                      label: 'قبول / تأكيد',
                      icon: Icons.check_circle_outline_rounded,
                      onPressed: () => onToggle(item),
                    ),
                    const SizedBox(height: 8),
                    LongPressButton(
                      danger: true,
                      onLongPress: () => onReject(item),
                      icon: const Icon(Icons.cancel_outlined),
                      label: const Text('رفض / إلغاء'),
                    ),
                  ] else if (item.status == 1)
                    LongPressButton(
                      danger: true,
                      onLongPress: () => onToggle(item),
                      icon: const Icon(Icons.cancel_outlined),
                      label: const Text('رفض / إلغاء'),
                    ),
                  if (item.canToggle && item.canComplete)
                    const SizedBox(height: 8),
                  if (item.canComplete)
                    DoctorActionButton(
                      label: 'إكمال',
                      icon: Icons.task_alt_rounded,
                      onPressed: () => onComplete(item),
                    ),
                ],
              ),
            ],
          ],
        ),
      ),
    );
  }
}

class _SmallSourceBadge extends StatelessWidget {
  const _SmallSourceBadge({
    required this.text,
    required this.icon,
    required this.color,
    required this.bg,
  });

  final String text;
  final IconData icon;
  final Color color;
  final Color bg;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 6),
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: color.withOpacity(.18)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: color),
          const SizedBox(width: 4),
          Text(
            text,
            style: TextStyle(
              fontSize: 11,
              fontWeight: FontWeight.w800,
              color: color,
            ),
          ),
        ],
      ),
    );
  }
}

class _InfoTile extends StatelessWidget {
  const _InfoTile({
    required this.icon,
    required this.title,
    required this.value,
  });

  final IconData icon;
  final String title;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(11),
      decoration: BoxDecoration(
        color: const Color(0xFFF7FAFA),
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: const Color(0xFFE3ECEA)),
      ),
      child: Row(
        children: [
          Icon(icon, size: 18, color: AppColors.primary),
          const SizedBox(width: 7),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: TextStyle(fontSize: 11, color: Colors.grey.shade600),
                ),
                const SizedBox(height: 2),
                Text(
                  value,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontSize: 13,
                    fontWeight: FontWeight.w900,
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

class _InfoLine extends StatelessWidget {
  const _InfoLine({required this.icon, required this.text});

  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) {
    if (text.trim().isEmpty) return const SizedBox.shrink();

    return Padding(
      padding: const EdgeInsets.only(top: 7),
      child: Row(
        children: [
          Icon(icon, size: 17, color: AppColors.primary),
          const SizedBox(width: 7),
          Expanded(
            child: Text(
              text,
              style: TextStyle(
                fontSize: 12.5,
                color: Colors.grey.shade800,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _WarningBox extends StatelessWidget {
  const _WarningBox({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(10),
      decoration: BoxDecoration(
        color: AppColors.danger.withOpacity(.08),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        children: [
          Icon(Icons.info_outline_rounded, size: 17, color: AppColors.danger),
          const SizedBox(width: 7),
          Expanded(
            child: Text(
              text,
              style: TextStyle(
                color: AppColors.danger,
                fontSize: 12,
                fontWeight: FontWeight.w700,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _ClinicChip extends StatelessWidget {
  const _ClinicChip(this.label, this.value, this.selected, this.onSelected);

  final String label;
  final int? value;
  final int? selected;
  final ValueChanged<int?> onSelected;

  @override
  Widget build(BuildContext context) {
    final isSelected = selected == value;

    return InkWell(
      borderRadius: BorderRadius.circular(14),
      onTap: () => onSelected(value),
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 180),
        constraints: const BoxConstraints(minWidth: 76, maxWidth: 170),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
        decoration: BoxDecoration(
          color: isSelected ? AppColors.primaryDark : Colors.white,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(
            color: isSelected ? AppColors.primaryDark : const Color(0xFFDDE9E7),
          ),
        ),
        child: Text(
          label,
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
          textAlign: TextAlign.center,
          style: TextStyle(
            fontSize: 12,
            fontWeight: FontWeight.w800,
            color: isSelected ? Colors.white : AppColors.primaryDark,
          ),
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
  Widget build(BuildContext context) {
    final isSelected = selected == value;

    return InkWell(
      borderRadius: BorderRadius.circular(14),
      onTap: () => onSelected(value),
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 180),
        constraints: const BoxConstraints(minWidth: 74),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
        decoration: BoxDecoration(
          color: isSelected ? AppColors.primary : Colors.white,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(
            color: isSelected ? AppColors.primary : const Color(0xFFDDE9E7),
          ),
        ),
        child: Text(
          label,
          textAlign: TextAlign.center,
          softWrap: false,
          style: TextStyle(
            fontSize: 12,
            fontWeight: FontWeight.w800,
            color: isSelected ? Colors.white : AppColors.primaryDark,
          ),
        ),
      ),
    );
  }
}
