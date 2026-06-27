import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_theme.dart';
import '../../../widgets/app_scaffold.dart';
import '../directory_service.dart';
import '../models/directory_models.dart';
import '../widgets/specialty_card.dart';

class SpecializationsScreen extends StatefulWidget {
  const SpecializationsScreen({super.key});

  @override
  State<SpecializationsScreen> createState() => _SpecializationsScreenState();
}

class _SpecializationsScreenState extends State<SpecializationsScreen> {
  late final DirectoryService _service;

  List<Specialization> _items = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _service = DirectoryService(context.read<ApiClient>());
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final items = await _service.getSpecializations();
      if (mounted) setState(() => _items = items);
    } catch (error) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) => AppScaffold(
    title: 'الاختصاصات',
    showBackButton: true,
    backRoute: '/',
    child: RefreshIndicator(
      onRefresh: _load,
      child: CustomScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        slivers: [
          SliverPadding(
            padding: const EdgeInsets.fromLTRB(16, 14, 16, 0),
            sliver: SliverToBoxAdapter(
              child: _Header(
                loading: _loading,
                count: _items.length,
                onRefresh: _load,
              ),
            ),
          ),
          if (_loading)
            const SliverFillRemaining(
              hasScrollBody: false,
              child: Center(child: CircularProgressIndicator()),
            )
          else if (_error != null)
            SliverPadding(
              padding: const EdgeInsets.all(16),
              sliver: SliverFillRemaining(
                hasScrollBody: false,
                child: _Message(
                  text: 'حدث خطأ أثناء تحميل الاختصاصات.',
                  onRetry: _load,
                ),
              ),
            )
          else if (_items.isEmpty)
            const SliverPadding(
              padding: EdgeInsets.all(16),
              sliver: SliverFillRemaining(
                hasScrollBody: false,
                child: _Message(text: 'لا توجد اختصاصات متاحة حالياً.'),
              ),
            )
          else
            SliverPadding(
              padding: const EdgeInsets.fromLTRB(16, 16, 16, 24),
              sliver: SliverLayoutBuilder(
                builder: (context, constraints) {
                  final width = constraints.crossAxisExtent;
                  final columns = width >= 900
                      ? 5
                      : width >= 680
                      ? 4
                      : width >= 430
                      ? 3
                      : 2;

                  return SliverGrid(
                    delegate: SliverChildBuilderDelegate((context, index) {
                      final item = _items[index];
                      return SpecialtyCard(item: item, index: index);
                    }, childCount: _items.length),
                    gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                      crossAxisCount: columns,
                      crossAxisSpacing: 12,
                      mainAxisSpacing: 12,
                      childAspectRatio: columns == 2 ? 1.18 : .95,
                    ),
                  );
                },
              ),
            ),
        ],
      ),
    ),
  );
}

class _Header extends StatelessWidget {
  const _Header({
    required this.loading,
    required this.count,
    required this.onRefresh,
  });

  final bool loading;
  final int count;
  final VoidCallback onRefresh;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(18),
    decoration: BoxDecoration(
      gradient: LinearGradient(
        colors: [AppColors.primary, AppColors.primary.withOpacity(.82)],
      ),
      borderRadius: BorderRadius.circular(8),
      boxShadow: [
        BoxShadow(
          color: AppColors.primary.withOpacity(.18),
          blurRadius: 22,
          offset: const Offset(0, 10),
        ),
      ],
    ),
    child: Row(
      children: [
        IconButton(
          tooltip: 'تحديث',
          onPressed: loading ? null : onRefresh,
          icon: const Icon(Icons.refresh_rounded),
          color: Colors.white,
          style: IconButton.styleFrom(
            backgroundColor: Colors.white.withOpacity(.16),
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              const Text(
                'كل الاختصاصات',
                textAlign: TextAlign.end,
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 23,
                  fontWeight: FontWeight.w900,
                ),
              ),
              const SizedBox(height: 5),
              Text(
                count == 0
                    ? 'اختر الاختصاص المناسب لحجز موعدك'
                    : '$count اختصاص متاح للحجز',
                textAlign: TextAlign.end,
                style: TextStyle(
                  color: Colors.white.withOpacity(.84),
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
        ),
        const SizedBox(width: 12),
        Container(
          width: 56,
          height: 56,
          decoration: BoxDecoration(
            color: Colors.white.withOpacity(.16),
            borderRadius: BorderRadius.circular(8),
            border: Border.all(color: Colors.white.withOpacity(.25)),
          ),
          child: const Icon(
            Icons.local_hospital_outlined,
            color: Colors.white,
            size: 30,
          ),
        ),
      ],
    ),
  );
}

class _Message extends StatelessWidget {
  const _Message({required this.text, this.onRetry});

  final String text;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(24),
    decoration: BoxDecoration(
      color: context.appSurface,
      borderRadius: BorderRadius.circular(24),
      border: Border.all(color: context.appBorder),
    ),
    child: Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        const Icon(
          Icons.medical_services_outlined,
          size: 44,
          color: AppColors.muted,
        ),
        const SizedBox(height: 10),
        Text(
          text,
          textAlign: TextAlign.center,
          style: const TextStyle(fontWeight: FontWeight.w700),
        ),
        if (onRetry != null) ...[
          const SizedBox(height: 10),
          TextButton.icon(
            onPressed: onRetry,
            icon: const Icon(Icons.refresh_rounded),
            label: const Text('إعادة المحاولة'),
          ),
        ],
      ],
    ),
  );
}
