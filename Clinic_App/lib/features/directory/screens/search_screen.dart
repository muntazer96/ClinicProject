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
  final _scrollController = ScrollController();
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

  bool get _hasMore => _page < _totalPages;

  @override
  void initState() {
    super.initState();
    _specialization = widget.initialSpecialization;
    _scrollController.addListener(_onScroll);
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
    _scrollController.removeListener(_onScroll);
    _scrollController.dispose();
    _name.dispose();
    super.dispose();
  }

  void _onScroll() {
    if (!_scrollController.hasClients ||
        _loading ||
        _loadingMore ||
        !_hasMore) {
      return;
    }
    final position = _scrollController.position;
    if (position.pixels >= position.maxScrollExtent - 260) {
      _search(reset: false);
    }
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
        sort: _sort,
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

  void _changeSort(String? value) {
    setState(() => _sort = value ?? 'default');
    _search();
  }

  @override
  Widget build(BuildContext context) => AppScaffold(
    title: 'ابحث عن طبيب',
    child: ListView(
      controller: _scrollController,
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
          onSortChanged: _changeSort,
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
          ..._doctors.map(
            (doctor) => Padding(
              padding: const EdgeInsets.only(bottom: 10),
              child: _DoctorCard(
                doctor: doctor,
                onTap: () => context.push('/doctors/${doctor.id}'),
              ),
            ),
          ),
          if (_loadingMore) ...[
            const SizedBox(height: 4),
            const Padding(
              padding: EdgeInsets.all(14),
              child: Center(child: CircularProgressIndicator()),
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
  Widget build(BuildContext context) {
    final featured = doctor.isFeatured;
    final accent = _featuredAccent(doctor);
    final subscriptionName = doctor.activeSubscriptionName?.trim();

    return Card(
    color: featured ? const Color(0xFFFFFCF1) : Colors.white,
    shape: RoundedRectangleBorder(
      borderRadius: BorderRadius.circular(8),
      side: BorderSide(
        color: featured ? accent.withOpacity(0.82) : AppColors.border,
        width: featured ? 1.4 : 1,
      ),
    ),
    child: InkWell(
      borderRadius: BorderRadius.circular(8),
      onTap: onTap,
      child: Stack(
        children: [
          Padding(
        padding: EdgeInsets.fromLTRB(13, featured ? 38 : 13, 13, 13),
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
                color: featured ? accent : AppColors.primary,
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
          if (featured)
            Positioned(
              top: 10,
              right: 12,
              child: _FeaturedBadge(
                color: accent,
                text: subscriptionName?.isNotEmpty == true
                    ? subscriptionName!
                    : 'Ù…Ù…ÙŠØ²',
              ),
            ),
        ],
      ),
    ),
  );
  }

  Color _featuredAccent(DoctorSummary doctor) {
    final name = doctor.activeSubscriptionNormalizedName?.toLowerCase() ?? '';
    if (name.contains('diamond') || name.contains('premium')) {
      return const Color(0xFF0E7490);
    }
    return const Color(0xFFB7791F);
  }
}

class _FeaturedBadge extends StatelessWidget {
  const _FeaturedBadge({required this.color, required this.text});

  final Color color;
  final String text;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 5),
    decoration: BoxDecoration(
      color: color,
      borderRadius: BorderRadius.circular(8),
      boxShadow: [
        BoxShadow(
          color: color.withOpacity(0.24),
          blurRadius: 12,
          offset: const Offset(0, 6),
        ),
      ],
    ),
    child: Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        const Icon(
          Icons.workspace_premium_rounded,
          size: 15,
          color: Colors.white,
        ),
        const SizedBox(width: 4),
        Text(
          text,
          style: const TextStyle(
            color: Colors.white,
            fontSize: 11,
            fontWeight: FontWeight.w900,
          ),
        ),
      ],
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
