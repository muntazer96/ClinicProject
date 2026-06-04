import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../core/api_client.dart';
import '../../../core/app_theme.dart';
import '../../../widgets/app_scaffold.dart';
import '../directory_service.dart';
import '../models/directory_models.dart';
import '../widgets/doctor_avatar.dart';

class SearchScreen extends StatefulWidget {
  const SearchScreen({super.key, this.initialSpecialization});

  final int? initialSpecialization;

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
  String _sort = 'default';
  int _page = 1;
  int _totalPages = 1;
  int _totalItems = 0;
  bool _loading = true;
  bool _loadingMore = false;
  String? _error;

  List<DoctorSummary> get _sortedDoctors {
    final items = [..._doctors];
    if (_sort == 'rating') {
      items.sort(
        (a, b) => (b.averageRating ?? 0).compareTo(a.averageRating ?? 0),
      );
    } else if (_sort == 'reviews') {
      items.sort((a, b) => b.reviewCount.compareTo(a.reviewCount));
    } else if (_sort == 'booking') {
      items.sort(
        (a, b) => (b.canBookOnline ? 1 : 0).compareTo(a.canBookOnline ? 1 : 0),
      );
    }
    return items;
  }

  bool get _hasMore => _page < _totalPages;

  @override
  void initState() {
    super.initState();
    _specialization = widget.initialSpecialization;
    _loadInitialData();
  }

  @override
  void didUpdateWidget(covariant SearchScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.initialSpecialization != widget.initialSpecialization) {
      _specialization = widget.initialSpecialization;
      _search();
    }
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

  Future<void> _search({bool reset = true}) async {
    final nextPage = reset ? 1 : _page + 1;
    setState(() {
      if (reset) {
        _loading = true;
      } else {
        _loadingMore = true;
      }
      _error = null;
    });
    try {
      final result = await _service.searchDoctors(
        name: _name.text,
        province: _province,
        specialization: _specialization,
        page: nextPage,
      );
      if (!mounted) return;
      setState(() {
        _page = result.currentPage;
        _totalPages = result.totalPages;
        _totalItems = result.totalItems;
        _doctors = reset ? result.items : [..._doctors, ...result.items];
      });
    } catch (error) {
      if (!mounted) return;
      setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) {
        setState(() {
          _loading = false;
          _loadingMore = false;
        });
      }
    }
  }

  void _resetFilters() {
    _name.clear();
    setState(() {
      _province = null;
      _specialization = null;
      _sort = 'default';
    });
    _search();
  }

  @override
  Widget build(BuildContext context) => AppScaffold(
    title: 'ابحث عن طبيب',
    child: ListView(
      padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
      children: [
        const Text(
          'دليلك الطبي',
          style: TextStyle(fontSize: 25, fontWeight: FontWeight.w900),
        ),
        const SizedBox(height: 5),
        const Text(
          'اختر ما يناسبك واعرض الأطباء المتاحين للحجز.',
          style: TextStyle(color: AppColors.muted),
        ),
        const SizedBox(height: 14),
        _FiltersCard(
          name: _name,
          province: _province,
          specialization: _specialization,
          sort: _sort,
          specializations: _specializations,
          onProvinceChanged: (value) => setState(() => _province = value),
          onSpecializationChanged: (value) =>
              setState(() => _specialization = value),
          onSortChanged: (value) => setState(() => _sort = value ?? 'default'),
          onSearch: () => _search(),
          onReset: _resetFilters,
        ),
        const SizedBox(height: 18),
        Text(
          'النتائج (${_doctors.length} من $_totalItems)',
          style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w800),
        ),
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
            action: () => _search(),
          )
        else if (_doctors.isEmpty)
          const _MessageCard(
            icon: Icons.search_off_outlined,
            title: 'لا توجد نتائج',
            text: 'جرب تغيير المحافظة أو الاختصاص أو اسم الطبيب.',
          )
        else ...[
          ..._sortedDoctors.map(
            (doctor) => Padding(
              padding: const EdgeInsets.only(bottom: 10),
              child: _DoctorCard(
                doctor: doctor,
                onTap: () => context.push('/doctors/${doctor.id}'),
              ),
            ),
          ),
          if (_hasMore) ...[
            const SizedBox(height: 4),
            OutlinedButton.icon(
              onPressed: _loadingMore ? null : () => _search(reset: false),
              icon: _loadingMore
                  ? const SizedBox(
                      width: 17,
                      height: 17,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.expand_more_rounded),
              label: Text(
                _loadingMore ? 'جاري تحميل المزيد...' : 'تحميل المزيد',
              ),
            ),
          ],
        ],
      ],
    ),
  );
}

class _FiltersCard extends StatelessWidget {
  const _FiltersCard({
    required this.name,
    required this.province,
    required this.specialization,
    required this.sort,
    required this.specializations,
    required this.onProvinceChanged,
    required this.onSpecializationChanged,
    required this.onSortChanged,
    required this.onSearch,
    required this.onReset,
  });

  final TextEditingController name;
  final int? province;
  final int? specialization;
  final String sort;
  final List<Specialization> specializations;
  final ValueChanged<int?> onProvinceChanged;
  final ValueChanged<int?> onSpecializationChanged;
  final ValueChanged<String?> onSortChanged;
  final VoidCallback onSearch;
  final VoidCallback onReset;

  @override
  Widget build(BuildContext context) {
    final selectedSpecialization =
        specializations.any((item) => item.id == specialization)
        ? specialization
        : null;

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(13),
        child: Column(
          children: [
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
                const DropdownMenuItem(
                  value: null,
                  child: Text('كل المحافظات'),
                ),
                ...provinces.map(
                  (item) =>
                      DropdownMenuItem(value: item.id, child: Text(item.name)),
                ),
              ],
              onChanged: onProvinceChanged,
            ),
            const SizedBox(height: 10),
            DropdownButtonFormField<int>(
              value: selectedSpecialization,
              decoration: const InputDecoration(
                prefixIcon: Icon(Icons.medical_services_outlined),
                labelText: 'الاختصاص',
              ),
              items: [
                const DropdownMenuItem(
                  value: null,
                  child: Text('كل الاختصاصات'),
                ),
                ...specializations.map(
                  (item) =>
                      DropdownMenuItem(value: item.id, child: Text(item.name)),
                ),
              ],
              onChanged: onSpecializationChanged,
            ),
            const SizedBox(height: 10),
            DropdownButtonFormField<String>(
              value: sort,
              decoration: const InputDecoration(
                prefixIcon: Icon(Icons.sort_rounded),
                labelText: 'ترتيب النتائج',
              ),
              items: const [
                DropdownMenuItem(
                  value: 'default',
                  child: Text('الترتيب الافتراضي'),
                ),
                DropdownMenuItem(
                  value: 'rating',
                  child: Text('الأعلى تقييماً'),
                ),
                DropdownMenuItem(
                  value: 'reviews',
                  child: Text('الأكثر مراجعات'),
                ),
                DropdownMenuItem(
                  value: 'booking',
                  child: Text('الحجز الإلكتروني أولاً'),
                ),
              ],
              onChanged: onSortChanged,
            ),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(
                  child: FilledButton.icon(
                    onPressed: onSearch,
                    icon: const Icon(Icons.search),
                    label: const Text('عرض الأطباء'),
                  ),
                ),
                const SizedBox(width: 8),
                IconButton.filledTonal(
                  tooltip: 'مسح الفلاتر',
                  onPressed: onReset,
                  icon: const Icon(Icons.filter_alt_off_rounded),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _DoctorCard extends StatelessWidget {
  const _DoctorCard({required this.doctor, required this.onTap});

  final DoctorSummary doctor;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => Card(
    child: InkWell(
      borderRadius: BorderRadius.circular(8),
      onTap: onTap,
      child: Padding(
        padding: const EdgeInsets.all(13),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            DoctorAvatar(imageName: doctor.imageName, size: 76),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    doctor.name,
                    style: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.w900,
                    ),
                  ),
                  const SizedBox(height: 3),
                  Text(
                    doctor.specializationName,
                    style: const TextStyle(
                      color: AppColors.primary,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Wrap(
                    spacing: 7,
                    runSpacing: 6,
                    children: [
                      if (doctor.clinics.isNotEmpty)
                        _Tag(
                          icon: Icons.location_on_outlined,
                          text: doctor.clinics.first.provinceName,
                        ),
                      if (doctor.averageRating != null)
                        _Tag(
                          icon: Icons.star_rounded,
                          text:
                              '${doctor.averageRating!.toStringAsFixed(1)} (${doctor.reviewCount})',
                          color: const Color(0xFFB16A2B),
                        ),
                      if (doctor.canBookOnline)
                        const _Tag(
                          icon: Icons.calendar_month_outlined,
                          text: 'حجز إلكتروني',
                        ),
                    ],
                  ),
                ],
              ),
            ),
            Container(
              width: 36,
              height: 36,
              decoration: BoxDecoration(
                color: AppColors.primary,
                borderRadius: BorderRadius.circular(8),
              ),
              child: const Icon(
                Icons.arrow_back_rounded,
                color: Colors.white,
                size: 19,
              ),
            ),
          ],
        ),
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
  Widget build(BuildContext context) => Row(
    mainAxisSize: MainAxisSize.min,
    children: [
      Icon(icon, size: 15, color: color ?? AppColors.muted),
      const SizedBox(width: 3),
      Text(
        text,
        style: TextStyle(fontSize: 12, color: color ?? AppColors.muted),
      ),
    ],
  );
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
      child: Column(
        children: [
          Icon(icon, size: 42, color: AppColors.muted),
          const SizedBox(height: 10),
          Text(
            title,
            style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w800),
          ),
          const SizedBox(height: 5),
          Text(
            text,
            textAlign: TextAlign.center,
            style: const TextStyle(color: AppColors.muted),
          ),
          if (action != null) ...[
            const SizedBox(height: 12),
            TextButton(onPressed: action, child: const Text('إعادة المحاولة')),
          ],
        ],
      ),
    ),
  );
}
