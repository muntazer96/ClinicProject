import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/analytics_service.dart';
import '../../../core/api_client.dart';
import '../../../core/app_theme.dart';
import '../../../widgets/app_scaffold.dart';
import '../../auth/auth_controller.dart';
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
  late final DirectoryService _service;
  final _name = TextEditingController();
  final _scrollController = ScrollController();
  late final AnalyticsService _analytics;
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
    final api = context.read<AuthController>().api;
    _service = DirectoryService(api);
    _analytics = AnalyticsService(api);
    _analytics.trackLater(eventType: 'page_viewed', page: 'search');
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
      _trackSearchResult(result.items, reset: reset);
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

  void _trackSearchResult(List<DoctorSummary> items, {required bool reset}) {
    final searchText = _name.text.trim();
    final provinceName = _selectedProvinceName();
    if (reset) {
      _analytics.trackLater(
        eventType: 'doctor_search_performed',
        specializationId: _specialization,
        province: provinceName,
        searchText: searchText,
        page: 'search',
      );
    }
    for (final doctor in items) {
      _analytics.trackOnce(
        key:
            'search-${doctor.id}-$_page-${searchText}_${_province}_${_specialization}_$_sort',
        eventType: 'doctor_shown_in_search',
        doctorId: doctor.id,
        specializationId: _specialization,
        province: doctor.clinics.isNotEmpty
            ? doctor.clinics.first.provinceName
            : provinceName,
        searchText: searchText,
        source: 'search',
        page: 'search',
      );
    }
  }

  String? _selectedProvinceName() {
    final selected = _province;
    if (selected == null) return null;
    return provinces
        .where((province) => province.id == selected)
        .map((province) => province.name)
        .firstOrNull;
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
        Text(
          'اختر ما يناسبك واعرض الأطباء المتاحين للحجز.',
          style: TextStyle(color: context.appMuted),
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
                onTap: () =>
                    context.push('/doctors/${doctor.id}?source=search'),
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
    const premiumColor = Color(0xFFD49A00);

    return Card(
      color: context.appSurface,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(8),
        side: BorderSide(
          color: featured ? premiumColor : context.appBorder,
          width: featured ? 1.5 : 1,
        ),
      ),
      child: InkWell(
        borderRadius: BorderRadius.circular(8),
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              DoctorAvatar(
                imageName: doctor.imageName,
                size: 76,
                foreground: featured ? premiumColor : AppColors.primary,
                background: featured
                    ? (context.isDark
                          ? const Color(0xFF33270F)
                          : const Color(0xFFFFF4D8))
                    : context.appSoftBlue,
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Row(
                      children: [
                        if (featured) ...[
                          const Icon(
                            Icons.workspace_premium_rounded,
                            color: premiumColor,
                            size: 18,
                          ),
                          const SizedBox(width: 6),
                        ],
                        Expanded(
                          child: Text(
                            doctor.name,
                            style: const TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.w900,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 5),
                    Text(
                      doctor.specializationName,
                      style: TextStyle(
                        color: featured ? premiumColor : AppColors.primary,
                        fontWeight: FontWeight.w800,
                      ),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 8),
                    if (doctor.clinics.isNotEmpty)
                      _InlineLocation(text: doctor.clinics.first.provinceName),
                    if (featured) ...[
                      const SizedBox(height: 10),
                      const _PremiumDoctorBadge(),
                    ] else ...[
                      const SizedBox(height: 8),
                      Wrap(
                        spacing: 7,
                        runSpacing: 6,
                        children: [
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
                  ],
                ),
              ),
              const SizedBox(width: 12),
              Container(
                width: 52,
                height: 52,
                decoration: BoxDecoration(
                  color: featured ? premiumColor : AppColors.primary,
                  borderRadius: BorderRadius.circular(8),
                  boxShadow: featured
                      ? [
                          BoxShadow(
                            color: premiumColor.withValues(alpha: .22),
                            blurRadius: 16,
                            offset: const Offset(0, 8),
                          ),
                        ]
                      : null,
                ),
                child: const Icon(
                  Icons.arrow_back_rounded,
                  color: Colors.white,
                  size: 25,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _InlineLocation extends StatelessWidget {
  const _InlineLocation({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) => Row(
    mainAxisSize: MainAxisSize.min,
    children: [
      Icon(Icons.location_on_outlined, color: context.appMuted, size: 18),
      const SizedBox(width: 4),
      Flexible(
        child: Text(
          text,
          style: TextStyle(
            color: context.appMuted,
            fontWeight: FontWeight.w700,
          ),
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
      ),
    ],
  );
}

class _PremiumDoctorBadge extends StatelessWidget {
  const _PremiumDoctorBadge();

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.symmetric(horizontal: 11, vertical: 6),
    decoration: BoxDecoration(
      color: context.isDark ? const Color(0xFF33270F) : const Color(0xFFFFFBF0),
      borderRadius: BorderRadius.circular(8),
      border: Border.all(color: const Color(0xFFE4B23C), width: 1.1),
    ),
    child: const Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(Icons.diamond_outlined, color: Color(0xFFD49A00), size: 16),
        SizedBox(width: 5),
        Text(
          'طبيب مميز',
          style: TextStyle(
            color: Color(0xFFD49A00),
            fontSize: 12,
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
      Icon(icon, size: 15, color: color ?? context.appMuted),
      const SizedBox(width: 3),
      Text(
        text,
        style: TextStyle(fontSize: 12, color: color ?? context.appMuted),
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
          Icon(icon, size: 42, color: context.appMuted),
          const SizedBox(height: 10),
          Text(
            title,
            style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w800),
          ),
          const SizedBox(height: 5),
          Text(
            text,
            textAlign: TextAlign.center,
            style: TextStyle(color: context.appMuted),
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
