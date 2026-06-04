import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_theme.dart';
import '../../../core/external_links.dart';
import '../../../widgets/app_scaffold.dart';
import '../../auth/auth_controller.dart';
import '../../reviews/models/review_models.dart';
import '../../reviews/review_service.dart';
import '../directory_service.dart';
import '../models/directory_models.dart';
import '../widgets/doctor_avatar.dart';

class DoctorDetailsScreen extends StatefulWidget {
  const DoctorDetailsScreen({super.key, required this.doctorId});
  final int doctorId;

  @override
  State<DoctorDetailsScreen> createState() => _DoctorDetailsScreenState();
}

class _DoctorDetailsScreenState extends State<DoctorDetailsScreen> {
  final _service = DirectoryService();
  late final ReviewService _reviewService;
  DoctorProfile? _doctor;
  DoctorReviews? _reviews;
  String? _error;

  @override
  void initState() {
    super.initState();
    _reviewService = ReviewService(context.read<AuthController>().api);
    _load();
  }

  Future<void> _load() async {
    setState(() => _error = null);
    try {
      final results = await Future.wait([
        _service.getDoctor(widget.doctorId),
        _reviewService.getDoctorReviews(widget.doctorId),
      ]);
      if (mounted) {
        setState(() {
          _doctor = results[0] as DoctorProfile;
          _reviews = results[1] as DoctorReviews?;
        });
      }
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    }
  }

  @override
  Widget build(BuildContext context) => AppScaffold(
    title: 'صفحة الطبيب',
    child: _doctor == null
        ? _error == null
              ? const Center(child: CircularProgressIndicator())
              : _ErrorView(text: _error!, onRetry: _load)
        : ListView(
            primary: true,
            physics: const AlwaysScrollableScrollPhysics(
              parent: ClampingScrollPhysics(),
            ),
            keyboardDismissBehavior: ScrollViewKeyboardDismissBehavior.onDrag,
            padding: const EdgeInsets.fromLTRB(16, 10, 16, 28),
            children: [
              _ProfileCard(doctor: _doctor!),
              const SizedBox(height: 16),
              if (_doctor!.description.isNotEmpty) ...[
                const _Title('نبذة عن الطبيب'),
                const SizedBox(height: 8),
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(14),
                    child: Text(
                      _doctor!.description,
                      style: const TextStyle(height: 1.7),
                    ),
                  ),
                ),
                const SizedBox(height: 16),
              ],
              const _Title('العيادات ومواعيد الدوام'),
              const SizedBox(height: 8),
              if (_doctor!.clinicDetails.isEmpty)
                const Card(
                  child: Padding(
                    padding: EdgeInsets.all(16),
                    child: Text('لا توجد عيادات متاحة حالياً.'),
                  ),
                )
              else
                ..._doctor!.clinicDetails.map(
                  (clinic) => Padding(
                    padding: const EdgeInsets.only(bottom: 10),
                    child: _ClinicCard(
                      clinic: clinic,
                      doctorId: _doctor!.id,
                      doctorName: _doctor!.name,
                      canBookOnline: _doctor!.canBookOnline,
                    ),
                  ),
                ),
              if (_reviews?.isEnabled == true) ...[
                const SizedBox(height: 10),
                _ReviewsButton(doctor: _doctor!, reviews: _reviews!),
              ],
            ],
          ),
  );
}

class _ReviewsButton extends StatelessWidget {
  const _ReviewsButton({required this.doctor, required this.reviews});
  final DoctorProfile doctor;
  final DoctorReviews reviews;

  @override
  Widget build(BuildContext context) => Card(
    child: Padding(
      padding: const EdgeInsets.all(14),
      child: Row(
        children: [
          const Icon(Icons.star_rounded, color: AppColors.warning, size: 28),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              '${reviews.averageRating?.toStringAsFixed(1) ?? '-'} من 5 (${reviews.reviewCount} تقييم)',
              style: const TextStyle(fontWeight: FontWeight.w900),
            ),
          ),
          FilledButton.icon(
            onPressed: () => context.push(
              '/doctors/${doctor.id}/reviews?doctorName=${Uri.encodeComponent(doctor.name)}',
            ),
            icon: const Icon(Icons.rate_review_outlined),
            label: const Text('عرض التقييمات'),
          ),
        ],
      ),
    ),
  );
}

class _ProfileCard extends StatelessWidget {
  const _ProfileCard({required this.doctor});
  final DoctorProfile doctor;

  @override
  Widget build(BuildContext context) => Card(
    child: Column(
      children: [
        Container(
          width: double.infinity,
          padding: const EdgeInsets.only(top: 18),
          decoration: const BoxDecoration(
            color: AppColors.softBlue,
            borderRadius: BorderRadius.vertical(top: Radius.circular(8)),
          ),
          child: Center(
            child: DoctorAvatar(
              imageName: doctor.imageName,
              size: 174,
              foreground: AppColors.primary,
              background: Color(0xFFD7FFFA),
            ),
          ),
        ),
        Padding(
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 17),
          child: Column(
            children: [
              Text(
                doctor.name,
                textAlign: TextAlign.center,
                style: const TextStyle(
                  fontSize: 23,
                  fontWeight: FontWeight.w900,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                doctor.specializationName,
                style: const TextStyle(
                  color: AppColors.primary,
                  fontWeight: FontWeight.w800,
                ),
              ),
              const SizedBox(height: 15),
              Row(
                children: [
                  _DoctorMetric(
                    icon: Icons.star_rounded,
                    value: doctor.averageRating?.toStringAsFixed(1) ?? '-',
                    label: 'التقييم',
                    iconColor: AppColors.warning,
                  ),
                  _DoctorMetric(
                    icon: Icons.rate_review_rounded,
                    value: '${doctor.reviewCount}+',
                    label: 'تقييم',
                  ),
                  _DoctorMetric(
                    icon: Icons.local_hospital_rounded,
                    value: '${doctor.clinicDetails.length}',
                    label: 'عيادات',
                  ),
                ],
              ),
            ],
          ),
        ),
      ],
    ),
  );
}

class _DoctorMetric extends StatelessWidget {
  const _DoctorMetric({
    required this.icon,
    required this.value,
    required this.label,
    this.iconColor = AppColors.primary,
  });

  final IconData icon;
  final String value;
  final String label;
  final Color iconColor;

  @override
  Widget build(BuildContext context) => Expanded(
    child: Column(
      children: [
        Container(
          width: 38,
          height: 38,
          decoration: BoxDecoration(
            color: iconColor.withValues(alpha: .12),
            borderRadius: BorderRadius.circular(8),
          ),
          child: Icon(icon, color: iconColor, size: 20),
        ),
        const SizedBox(height: 6),
        Text(value, style: const TextStyle(fontWeight: FontWeight.w900)),
        Text(
          label,
          style: const TextStyle(color: AppColors.muted, fontSize: 11),
        ),
      ],
    ),
  );
}

class _ClinicCard extends StatelessWidget {
  const _ClinicCard({
    required this.clinic,
    required this.doctorId,
    required this.doctorName,
    required this.canBookOnline,
  });
  final ClinicDetails clinic;
  final int doctorId;
  final String doctorName;
  final bool canBookOnline;

  @override
  Widget build(BuildContext context) => Card(
    child: Padding(
      padding: const EdgeInsets.all(14),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            clinic.name,
            style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w900),
          ),
          const SizedBox(height: 8),
          _InfoRow(
            icon: Icons.location_on_outlined,
            text: '${clinic.provinceName} - ${clinic.address}',
          ),
          if (clinic.phoneNumber?.isNotEmpty == true ||
              clinic.mapUrl?.isNotEmpty == true) ...[
            const SizedBox(height: 8),
            Row(
              children: [
                if (clinic.phoneNumber?.isNotEmpty == true)
                  Expanded(
                    child: OutlinedButton.icon(
                      onPressed: () => openPhone(context, clinic.phoneNumber!),
                      icon: const Icon(Icons.phone_outlined),
                      label: const Text('اتصال'),
                    ),
                  ),
                if (clinic.phoneNumber?.isNotEmpty == true &&
                    clinic.mapUrl?.isNotEmpty == true)
                  const SizedBox(width: 8),
                if (clinic.mapUrl?.isNotEmpty == true)
                  Expanded(
                    child: OutlinedButton.icon(
                      onPressed: () => openMap(
                        context,
                        clinic.mapUrl!,
                        query:
                            '${clinic.name} ${clinic.provinceName} ${clinic.address}',
                      ),
                      icon: const Icon(Icons.map_outlined),
                      label: const Text('الموقع'),
                    ),
                  ),
              ],
            ),
          ],
          const Divider(height: 22),
          const Text(
            'أوقات الدوام',
            style: TextStyle(fontWeight: FontWeight.w800),
          ),
          const SizedBox(height: 8),
          if (clinic.availabilities.isEmpty)
            const Text(
              'لم يتم تحديد دوام لهذه العيادة.',
              style: TextStyle(color: AppColors.muted),
            )
          else
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: clinic.availabilities
                  .map((item) => _AvailabilityChip(item: item))
                  .toList(),
            ),
          if (clinic.availabilities.isNotEmpty && canBookOnline) ...[
            const SizedBox(height: 10),
            SizedBox(
              width: double.infinity,
              child: FilledButton.icon(
                onPressed: () => context.push(
                  '/book/$doctorId/${clinic.id}'
                  '?doctorName=${Uri.encodeComponent(doctorName)}'
                  '&clinicName=${Uri.encodeComponent(clinic.name)}',
                ),
                icon: const Icon(Icons.calendar_month_rounded),
                label: const Text('احجز دورك الآن'),
              ),
            ),
          ],
        ],
      ),
    ),
  );
}

class _AvailabilityChip extends StatelessWidget {
  const _AvailabilityChip({required this.item});
  final ClinicAvailability item;

  @override
  Widget build(BuildContext context) => Container(
    width: double.infinity,
    padding: const EdgeInsets.all(10),
    decoration: BoxDecoration(
      color: AppColors.surfaceMuted,
      borderRadius: BorderRadius.circular(8),
      border: Border.all(color: AppColors.border),
    ),
    child: Row(
      children: [
        Expanded(
          child: Text(
            item.dayName,
            style: const TextStyle(fontWeight: FontWeight.w800),
          ),
        ),
        Text(
          '${_shortTime(item.startTime)} - ${_shortTime(item.endTime)}',
          style: const TextStyle(
            color: AppColors.primary,
            fontWeight: FontWeight.w800,
          ),
        ),
        const SizedBox(width: 9),
        Text(
          '${item.maxAppointments} دور',
          style: const TextStyle(color: AppColors.muted, fontSize: 12),
        ),
      ],
    ),
  );

  static String _shortTime(String value) =>
      value.length >= 5 ? value.substring(0, 5) : value;
}

class _InfoRow extends StatelessWidget {
  const _InfoRow({required this.icon, required this.text});
  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: 7),
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 18, color: AppColors.primary),
        const SizedBox(width: 7),
        Expanded(
          child: Text(text, style: const TextStyle(color: AppColors.muted)),
        ),
      ],
    ),
  );
}

class _Title extends StatelessWidget {
  const _Title(this.text);
  final String text;

  @override
  Widget build(BuildContext context) => Text(
    text,
    style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w900),
  );
}

class _ErrorView extends StatelessWidget {
  const _ErrorView({required this.text, required this.onRetry});
  final String text;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) => Center(
    child: Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Icon(Icons.wifi_off_outlined, size: 44, color: AppColors.muted),
          const SizedBox(height: 10),
          Text(text, textAlign: TextAlign.center),
          TextButton(onPressed: onRetry, child: const Text('إعادة المحاولة')),
        ],
      ),
    ),
  );
}
