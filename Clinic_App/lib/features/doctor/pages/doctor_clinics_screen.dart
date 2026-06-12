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
    child: Stack(
      children: [
        RefreshIndicator(
            onRefresh: _load,
            child: _loading
                ? const Center(child: CircularProgressIndicator())
                : _items.isEmpty
                ? const DoctorEmptyState(
                    icon: Icons.local_hospital_outlined,
                    message: 'لا توجد عيادات مرتبطة بحساب الطبيب.',
                  )
                : ListView.builder(
                    padding: const EdgeInsets.fromLTRB(16, 14, 16, 86),
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
        PositionedDirectional(
          start: 18,
          bottom: 16,
          child: FloatingActionButton.small(
            heroTag: 'doctor-add-clinic',
            backgroundColor: AppColors.primary,
            foregroundColor: Colors.white,
            onPressed: () async {
              await context.push('/doctor/clinics/form');
              await _load();
            },
            child: const Icon(Icons.add_rounded),
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
  Widget build(BuildContext context) {
    final visibleColor = clinic.isVisible ? AppColors.primary : AppColors.muted;

    return Container(
      margin: const EdgeInsets.only(bottom: 14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: const Color(0xFFDDE9E7)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(.035),
            blurRadius: 16,
            offset: const Offset(0, 8),
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
                  width: 52,
                  height: 52,
                  decoration: BoxDecoration(
                    color: const Color(0xFFEAF7F5),
                    borderRadius: BorderRadius.circular(16),
                  ),
                  child: const Icon(
                    Icons.local_hospital_rounded,
                    color: AppColors.primary,
                    size: 28,
                  ),
                ),
                const SizedBox(width: 12),

                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        clinic.name,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: const TextStyle(
                          fontSize: 17,
                          fontWeight: FontWeight.w900,
                        ),
                      ),
                      const SizedBox(height: 5),
                      Text(
                        '${clinic.iraqiProvinceName} - ${clinic.address}',
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                        style: TextStyle(
                          fontSize: 12.5,
                          height: 1.4,
                          color: Colors.grey.shade700,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ],
                  ),
                ),

                const SizedBox(width: 8),

                DoctorStatusPill(
                  label: clinic.isVisible ? 'ظاهرة' : 'مخفية',
                  color: visibleColor,
                ),
              ],
            ),

            const SizedBox(height: 14),

            Row(
              children: [
                Expanded(
                  child: _ClinicInfoBox(
                    icon: Icons.payments_outlined,
                    title: 'سعر الكشف',
                    value: clinic.consultationPrice == null
                        ? 'بدون سعر'
                        : '${clinic.consultationPrice!.toStringAsFixed(0)} د.ع',
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: _ClinicInfoBox(
                    icon: Icons.phone_outlined,
                    title: 'الهاتف',
                    value: clinic.phoneNumber?.isNotEmpty == true
                        ? clinic.phoneNumber!
                        : 'غير مضاف',
                  ),
                ),
              ],
            ),

            const SizedBox(height: 12),

            SizedBox(
              height: 46,
              child: ElevatedButton.icon(
                onPressed: onSchedule,
                icon: const Icon(Icons.schedule_rounded, size: 19),
                label: const Text(
                  'إدارة أوقات الدوام',
                  style: TextStyle(fontWeight: FontWeight.w900),
                ),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: Colors.white,
                  elevation: 0,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
                  ),
                ),
              ),
            ),

            const SizedBox(height: 9),

            Row(
              children: [
                Expanded(
                  child: OutlinedButton.icon(
                    onPressed: onEdit,
                    icon: const Icon(Icons.edit_outlined, size: 18),
                    label: const Text(
                      'تعديل',
                      style: TextStyle(fontWeight: FontWeight.w900),
                    ),
                    style: OutlinedButton.styleFrom(
                      foregroundColor: AppColors.primary,
                      side: const BorderSide(color: Color(0xFFDDE9E7)),
                      padding: const EdgeInsets.symmetric(vertical: 12),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(13),
                      ),
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: OutlinedButton.icon(
                    onPressed: () =>
                        showAppSnackBar(context, 'اضغط مطولاً للحذف.'),
                    onLongPress: onDelete,
                    icon: const Icon(Icons.delete_outline, size: 18),
                    label: const Text(
                      'حذف',
                      style: TextStyle(fontWeight: FontWeight.w900),
                    ),
                    style: OutlinedButton.styleFrom(
                      foregroundColor: AppColors.danger,
                      side: BorderSide(color: AppColors.danger.withOpacity(.22)),
                      padding: const EdgeInsets.symmetric(vertical: 12),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(13),
                      ),
                    ),
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

class _ClinicInfoBox extends StatelessWidget {
  const _ClinicInfoBox({
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
                const SizedBox(height: 3),
                Text(
                  value,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontSize: 12.5,
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
