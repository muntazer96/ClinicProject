import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_theme.dart';
import '../../../widgets/app_scaffold.dart';
import '../../auth/auth_controller.dart';
import '../directory_service.dart';
import '../models/directory_models.dart';
import '../widgets/doctor_avatar.dart';

class FavoriteDoctorsScreen extends StatefulWidget {
  const FavoriteDoctorsScreen({super.key});

  @override
  State<FavoriteDoctorsScreen> createState() => _FavoriteDoctorsScreenState();
}

class _FavoriteDoctorsScreenState extends State<FavoriteDoctorsScreen> {
  late final DirectoryService _service;
  List<DoctorSummary> _items = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _service = DirectoryService(context.read<AuthController>().api);
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      _items = await _service.getFavoriteDoctors();
    } catch (error) {
      _error = ApiClient.errorMessage(error);
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) => AppScaffold(
        title: 'الأطباء المفضلون',
        child: RefreshIndicator(
          onRefresh: _load,
          child: _loading
              ? const Center(child: CircularProgressIndicator())
              : _error != null
                  ? ListView(
                      padding: const EdgeInsets.all(24),
                      children: [
                        Text(_error!, textAlign: TextAlign.center),
                        TextButton(
                          onPressed: _load,
                          child: const Text('إعادة المحاولة'),
                        ),
                      ],
                    )
                  : _items.isEmpty
                      ? ListView(
                          padding: const EdgeInsets.all(28),
                          children: const [
                            Icon(
                              Icons.favorite_border_rounded,
                              size: 52,
                              color: AppColors.muted,
                            ),
                            SizedBox(height: 10),
                            Text(
                              'لا توجد أطباء في المفضلة حالياً.',
                              textAlign: TextAlign.center,
                              style: TextStyle(color: AppColors.muted),
                            ),
                          ],
                        )
                      : ListView.builder(
                          padding: const EdgeInsets.fromLTRB(16, 12, 16, 28),
                          itemCount: _items.length,
                          itemBuilder: (context, index) =>
                              _FavoriteDoctorCard(doctor: _items[index]),
                        ),
        ),
      );
}

class _FavoriteDoctorCard extends StatelessWidget {
  const _FavoriteDoctorCard({required this.doctor});

  final DoctorSummary doctor;

  @override
  Widget build(BuildContext context) => Padding(
        padding: const EdgeInsets.only(bottom: 10),
        child: Card(
          child: InkWell(
            borderRadius: BorderRadius.circular(8),
            onTap: () => context.push('/doctors/${doctor.id}'),
            child: Padding(
              padding: const EdgeInsets.all(12),
              child: Row(
                children: [
                  DoctorAvatar(
                    imageName: doctor.imageName,
                    size: 58,
                    foreground: AppColors.primary,
                    background: const Color(0xFFEAF7F5),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          doctor.name,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(
                            fontWeight: FontWeight.w900,
                            fontSize: 16,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          doctor.specializationName,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(
                            color: AppColors.primary,
                            fontWeight: FontWeight.w800,
                          ),
                        ),
                        if (doctor.clinics.isNotEmpty) ...[
                          const SizedBox(height: 4),
                          Text(
                            doctor.clinics.first.provinceName,
                            style: const TextStyle(color: AppColors.muted),
                          ),
                        ],
                      ],
                    ),
                  ),
                  const Icon(Icons.chevron_right_rounded),
                ],
              ),
            ),
          ),
        ),
      );
}
