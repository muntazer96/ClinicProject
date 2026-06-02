import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../core/api_client.dart';
import '../../../widgets/app_scaffold.dart';
import '../directory_service.dart';
import '../models/directory_models.dart';
import '../widgets/doctor_avatar.dart';

class SearchScreen extends StatefulWidget {
  const SearchScreen({super.key});

  @override
  State<SearchScreen> createState() => _SearchScreenState();
}

class _SearchScreenState extends State<SearchScreen> {
  final _service = DirectoryService();
  final _name = TextEditingController();
  List<Specialization> _specializations = [];
  List<DoctorSummary> _doctors = [];
  int? _province;
  int? _specialization;
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadInitialData();
  }

  @override
  void dispose() {
    _name.dispose();
    super.dispose();
  }

  Future<void> _loadInitialData() async {
    try {
      final specializations = await _service.getSpecializations();
      if (!mounted) return;
      setState(() => _specializations = specializations);
    } catch (_) {
      // Search remains usable if the specialization lookup is unavailable.
    }
    await _search();
  }

  Future<void> _search() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final doctors = await _service.searchDoctors(
        name: _name.text,
        province: _province,
        specialization: _specialization,
      );
      if (!mounted) return;
      setState(() => _doctors = doctors);
    } catch (error) {
      if (!mounted) return;
      setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) => AppScaffold(
        title: 'ابحث عن طبيب',
        child: ListView(
          padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
          children: [
            const Text('دليلك الطبي',
                style: TextStyle(fontSize: 25, fontWeight: FontWeight.w900)),
            const SizedBox(height: 5),
            const Text('اختر ما يناسبك واعرض الأطباء المتاحين للحجز.',
                style: TextStyle(color: Color(0xFF78908D))),
            const SizedBox(height: 14),
            _FiltersCard(
              name: _name,
              province: _province,
              specialization: _specialization,
              specializations: _specializations,
              onProvinceChanged: (value) => setState(() => _province = value),
              onSpecializationChanged: (value) =>
                  setState(() => _specialization = value),
              onSearch: _search,
            ),
            const SizedBox(height: 18),
            Text('النتائج (${_doctors.length})',
                style:
                    const TextStyle(fontSize: 18, fontWeight: FontWeight.w800)),
            const SizedBox(height: 10),
            if (_loading)
              const Padding(
                padding: EdgeInsets.all(36),
                child: Center(child: CircularProgressIndicator()),
              )
            else if (_error != null)
              _MessageCard(
                icon: Icons.wifi_off_outlined,
                title: 'تعذر تحميل الأطباء',
                text: _error!,
                action: _search,
              )
            else if (_doctors.isEmpty)
              const _MessageCard(
                icon: Icons.search_off_outlined,
                title: 'لا توجد نتائج',
                text: 'جرّب تغيير المحافظة أو الاختصاص أو اسم الطبيب.',
              )
            else
              ..._doctors.map((doctor) => Padding(
                    padding: const EdgeInsets.only(bottom: 10),
                    child: _DoctorCard(
                      doctor: doctor,
                      onTap: () => context.push('/doctors/${doctor.id}'),
                    ),
                  )),
          ],
        ),
      );
}

class _FiltersCard extends StatelessWidget {
  const _FiltersCard({
    required this.name,
    required this.province,
    required this.specialization,
    required this.specializations,
    required this.onProvinceChanged,
    required this.onSpecializationChanged,
    required this.onSearch,
  });

  final TextEditingController name;
  final int? province;
  final int? specialization;
  final List<Specialization> specializations;
  final ValueChanged<int?> onProvinceChanged;
  final ValueChanged<int?> onSpecializationChanged;
  final VoidCallback onSearch;

  @override
  Widget build(BuildContext context) => Card(
        child: Padding(
          padding: const EdgeInsets.all(13),
          child: Column(children: [
            TextField(
              controller: name,
              textInputAction: TextInputAction.search,
              onSubmitted: (_) => onSearch(),
              decoration: const InputDecoration(
                prefixIcon: Icon(Icons.search),
                labelText: 'اسم الطبيب',
              ),
            ),
            const SizedBox(height: 10),
            DropdownButtonFormField<int>(
              value: province,
              decoration: const InputDecoration(
                prefixIcon: Icon(Icons.location_on_outlined),
                labelText: 'المحافظة',
              ),
              items: [
                const DropdownMenuItem(value: null, child: Text('كل المحافظات')),
                ...provinces.map((item) =>
                    DropdownMenuItem(value: item.id, child: Text(item.name))),
              ],
              onChanged: onProvinceChanged,
            ),
            const SizedBox(height: 10),
            DropdownButtonFormField<int>(
              value: specialization,
              decoration: const InputDecoration(
                prefixIcon: Icon(Icons.medical_services_outlined),
                labelText: 'الاختصاص',
              ),
              items: [
                const DropdownMenuItem(value: null, child: Text('كل الاختصاصات')),
                ...specializations.map((item) =>
                    DropdownMenuItem(value: item.id, child: Text(item.name))),
              ],
              onChanged: onSpecializationChanged,
            ),
            const SizedBox(height: 12),
            FilledButton.icon(
              onPressed: onSearch,
              icon: const Icon(Icons.search),
              label: const Text('عرض الأطباء'),
            ),
          ]),
        ),
      );
}

class _DoctorCard extends StatelessWidget {
  const _DoctorCard({required this.doctor, required this.onTap});

  final DoctorSummary doctor;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => Card(
        child: InkWell(
          borderRadius: BorderRadius.circular(18),
          onTap: onTap,
          child: Padding(
            padding: const EdgeInsets.all(13),
            child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
              DoctorAvatar(imageName: doctor.imageName),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(doctor.name,
                        style: const TextStyle(
                            fontSize: 16, fontWeight: FontWeight.w900)),
                    const SizedBox(height: 3),
                    Text(doctor.specializationName,
                        style: const TextStyle(
                            color: Color(0xFF147D72),
                            fontWeight: FontWeight.w700)),
                    const SizedBox(height: 8),
                    Wrap(spacing: 7, runSpacing: 6, children: [
                      if (doctor.clinics.isNotEmpty)
                        _Tag(
                            icon: Icons.location_on_outlined,
                            text: doctor.clinics.first.provinceName),
                      if (doctor.averageRating != null)
                        _Tag(
                            icon: Icons.star_rounded,
                            text:
                                '${doctor.averageRating!.toStringAsFixed(1)} (${doctor.reviewCount})',
                            color: const Color(0xFFB16A2B)),
                      if (doctor.canBookOnline)
                        const _Tag(
                            icon: Icons.calendar_month_outlined,
                            text: 'حجز إلكتروني'),
                    ]),
                  ],
                ),
              ),
              const Icon(Icons.chevron_left, color: Color(0xFF78908D)),
            ]),
          ),
        ),
      );
}

class _Tag extends StatelessWidget {
  const _Tag({required this.icon, required this.text, this.color});
  final IconData icon;
  final String text;
  final Color? color;

  @override
  Widget build(BuildContext context) => Row(mainAxisSize: MainAxisSize.min, children: [
        Icon(icon, size: 15, color: color ?? const Color(0xFF78908D)),
        const SizedBox(width: 3),
        Text(text,
            style: TextStyle(
                fontSize: 12, color: color ?? const Color(0xFF78908D))),
      ]);
}

class _MessageCard extends StatelessWidget {
  const _MessageCard({
    required this.icon,
    required this.title,
    required this.text,
    this.action,
  });
  final IconData icon;
  final String title;
  final String text;
  final VoidCallback? action;

  @override
  Widget build(BuildContext context) => Card(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(children: [
            Icon(icon, size: 42, color: const Color(0xFF78908D)),
            const SizedBox(height: 10),
            Text(title,
                style:
                    const TextStyle(fontSize: 17, fontWeight: FontWeight.w800)),
            const SizedBox(height: 5),
            Text(text,
                textAlign: TextAlign.center,
                style: const TextStyle(color: Color(0xFF78908D))),
            if (action != null) ...[
              const SizedBox(height: 12),
              TextButton(onPressed: action, child: const Text('إعادة المحاولة')),
            ],
          ]),
        ),
      );
}
