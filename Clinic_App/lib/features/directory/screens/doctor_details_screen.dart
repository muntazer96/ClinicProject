import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
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
            padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
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
                _ReviewsSection(reviews: _reviews!),
              ],
            ],
          ),
  );
}

class _ReviewsSection extends StatelessWidget {
  const _ReviewsSection({required this.reviews});
  final DoctorReviews reviews;

  @override
  Widget build(BuildContext context) => Column(
    crossAxisAlignment: CrossAxisAlignment.start,
    children: [
      const _Title('تقييمات المراجعين'),
      const SizedBox(height: 8),
      Card(
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Column(
            children: [
              Row(
                children: [
                  const Icon(Icons.star_rounded, color: Color(0xFFFFB54A)),
                  const SizedBox(width: 5),
                  Text(
                    reviews.averageRating?.toStringAsFixed(1) ?? '-',
                    style: const TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.w900,
                    ),
                  ),
                  const SizedBox(width: 7),
                  Text(
                    'من 5 (${reviews.reviewCount} تقييم)',
                    style: const TextStyle(color: Color(0xFF78908D)),
                  ),
                ],
              ),
              if (reviews.reviews.isEmpty) ...[
                const SizedBox(height: 12),
                const Text(
                  'لا توجد تقييمات حتى الآن.',
                  style: TextStyle(color: Color(0xFF78908D)),
                ),
              ] else
                ...reviews.reviews.map(
                  (review) => Padding(
                    padding: const EdgeInsets.only(top: 12),
                    child: Container(
                      width: double.infinity,
                      padding: const EdgeInsets.all(11),
                      decoration: BoxDecoration(
                        color: const Color(0xFFF7FBFA),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            children: [
                              Expanded(
                                child: Text(
                                  review.userName,
                                  style: const TextStyle(
                                    fontWeight: FontWeight.w800,
                                  ),
                                ),
                              ),
                              Text(
                                '${review.rating}/5',
                                style: const TextStyle(
                                  color: Color(0xFFB16A2B),
                                  fontWeight: FontWeight.w800,
                                ),
                              ),
                            ],
                          ),
                          if (review.comment.isNotEmpty) ...[
                            const SizedBox(height: 5),
                            Text(review.comment),
                          ],
                        ],
                      ),
                    ),
                  ),
                ),
            ],
          ),
        ),
      ),
    ],
  );
}

class _ProfileCard extends StatelessWidget {
  const _ProfileCard({required this.doctor});
  final DoctorProfile doctor;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(18),
    decoration: BoxDecoration(
      gradient: const LinearGradient(
        colors: [Color(0xFF126E65), Color(0xFF26988D)],
      ),
      borderRadius: BorderRadius.circular(22),
    ),
    child: Row(
      children: [
        DoctorAvatar(
          imageName: doctor.imageName,
          size: 76,
          foreground: Colors.white,
          background: Colors.white.withValues(alpha: .16),
        ),
        const SizedBox(width: 14),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                doctor.name,
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 21,
                  fontWeight: FontWeight.w900,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                doctor.specializationName,
                style: const TextStyle(
                  color: Color(0xFFD8F1EE),
                  fontWeight: FontWeight.w700,
                ),
              ),
              if (doctor.averageRating != null) ...[
                const SizedBox(height: 9),
                Row(
                  children: [
                    const Icon(
                      Icons.star_rounded,
                      color: Color(0xFFFFCE77),
                      size: 19,
                    ),
                    const SizedBox(width: 4),
                    Text(
                      '${doctor.averageRating!.toStringAsFixed(1)} من 5 (${doctor.reviewCount} تقييم)',
                      style: const TextStyle(color: Colors.white, fontSize: 12),
                    ),
                  ],
                ),
              ],
            ],
          ),
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
          if (clinic.phoneNumber?.isNotEmpty == true)
            _InfoRow(
              icon: Icons.phone_outlined,
              text: clinic.phoneNumber!,
              onTap: () => openPhone(context, clinic.phoneNumber!),
            ),
          if (clinic.mapUrl?.isNotEmpty == true)
            _InfoRow(
              icon: Icons.map_outlined,
              text: 'فتح موقع العيادة على الخارطة',
              onTap: () => openMap(context, clinic.mapUrl!),
            ),
          const Divider(height: 22),
          const Text(
            'أوقات الدوام',
            style: TextStyle(fontWeight: FontWeight.w800),
          ),
          const SizedBox(height: 8),
          if (clinic.availabilities.isEmpty)
            const Text(
              'لم يتم تحديد دوام لهذه العيادة.',
              style: TextStyle(color: Color(0xFF78908D)),
            )
          else
            ...clinic.availabilities.map(
              (item) => Padding(
                padding: const EdgeInsets.only(bottom: 7),
                child: Row(
                  children: [
                    Expanded(
                      child: Text(
                        item.dayName,
                        style: const TextStyle(fontWeight: FontWeight.w700),
                      ),
                    ),
                    Text(
                      '${_shortTime(item.startTime)} - ${_shortTime(item.endTime)}',
                      style: const TextStyle(color: Color(0xFF147D72)),
                    ),
                    const SizedBox(width: 10),
                    Text(
                      '${item.maxAppointments} دور',
                      style: const TextStyle(
                        color: Color(0xFF78908D),
                        fontSize: 12,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          if (clinic.availabilities.isNotEmpty && canBookOnline) ...[
            const SizedBox(height: 7),
            FilledButton.icon(
              onPressed: () => context.push(
                '/book/$doctorId/${clinic.id}'
                '?doctorName=${Uri.encodeComponent(doctorName)}'
                '&clinicName=${Uri.encodeComponent(clinic.name)}',
              ),
              icon: const Icon(Icons.calendar_month_outlined),
              label: const Text('احجز دورك الآن'),
            ),
          ],
        ],
      ),
    ),
  );

  static String _shortTime(String value) =>
      value.length >= 5 ? value.substring(0, 5) : value;
}

class _InfoRow extends StatelessWidget {
  const _InfoRow({required this.icon, required this.text, this.onTap});
  final IconData icon;
  final String text;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) => InkWell(
    onTap: onTap,
    borderRadius: BorderRadius.circular(8),
    child: Padding(
      padding: const EdgeInsets.only(bottom: 7),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, size: 18, color: const Color(0xFF147D72)),
          const SizedBox(width: 7),
          Expanded(
            child: Text(
              text,
              style: TextStyle(
                color: onTap == null
                    ? const Color(0xFF5F7975)
                    : const Color(0xFF147D72),
                decoration: onTap == null ? null : TextDecoration.underline,
              ),
            ),
          ),
          if (onTap != null)
            const Icon(Icons.open_in_new, size: 15, color: Color(0xFF147D72)),
        ],
      ),
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
          const Icon(
            Icons.wifi_off_outlined,
            size: 44,
            color: Color(0xFF78908D),
          ),
          const SizedBox(height: 10),
          Text(text, textAlign: TextAlign.center),
          TextButton(onPressed: onRetry, child: const Text('إعادة المحاولة')),
        ],
      ),
    ),
  );
}
