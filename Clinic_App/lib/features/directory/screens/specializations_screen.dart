import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../core/api_client.dart';
import '../../../core/app_theme.dart';
import '../../../widgets/app_scaffold.dart';
import '../directory_service.dart';
import '../models/directory_models.dart';

class SpecializationsScreen extends StatefulWidget {
  const SpecializationsScreen({super.key});

  @override
  State<SpecializationsScreen> createState() => _SpecializationsScreenState();
}

class _SpecializationsScreenState extends State<SpecializationsScreen> {
  final _service = DirectoryService();
  List<Specialization> _items = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
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
    child: RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
        children: [
          Row(
            children: [
              const Expanded(
                child: Text(
                  'كل الاختصاصات',
                  style: TextStyle(fontSize: 23, fontWeight: FontWeight.w900),
                ),
              ),
              IconButton(
                tooltip: 'تحديث',
                onPressed: _loading ? null : _load,
                icon: const Icon(Icons.refresh_rounded),
              ),
            ],
          ),
          const SizedBox(height: 12),
          if (_loading)
            const Padding(
              padding: EdgeInsets.all(36),
              child: Center(child: CircularProgressIndicator()),
            )
          else if (_error != null)
            _Message(text: _error!, onRetry: _load)
          else if (_items.isEmpty)
            const _Message(text: 'لا توجد اختصاصات متاحة حالياً.')
          else
            ..._items.map(
              (item) => Padding(
                padding: const EdgeInsets.only(bottom: 10),
                child: _SpecializationTile(item: item),
              ),
            ),
        ],
      ),
    ),
  );
}

class _SpecializationTile extends StatelessWidget {
  const _SpecializationTile({required this.item});
  final Specialization item;

  @override
  Widget build(BuildContext context) => Card(
    child: ListTile(
      leading: const CircleAvatar(
        backgroundColor: AppColors.softBlue,
        child: Icon(Icons.medical_services_outlined, color: AppColors.primary),
      ),
      title: Text(
        item.name,
        style: const TextStyle(fontWeight: FontWeight.w900),
      ),
      trailing: const Icon(Icons.arrow_back_ios_new_rounded, size: 16),
      onTap: () => context.go('/search?specialization=${item.id}'),
    ),
  );
}

class _Message extends StatelessWidget {
  const _Message({required this.text, this.onRetry});
  final String text;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) => Card(
    child: Padding(
      padding: const EdgeInsets.all(22),
      child: Column(
        children: [
          const Icon(
            Icons.medical_services_outlined,
            size: 42,
            color: AppColors.muted,
          ),
          const SizedBox(height: 10),
          Text(text, textAlign: TextAlign.center),
          if (onRetry != null)
            TextButton(onPressed: onRetry, child: const Text('إعادة المحاولة')),
        ],
      ),
    ),
  );
}
