import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorSubscriptionScreen extends StatefulWidget {
  const DoctorSubscriptionScreen({super.key});

  @override
  State<DoctorSubscriptionScreen> createState() =>
      _DoctorSubscriptionScreenState();
}

class _DoctorSubscriptionScreenState extends State<DoctorSubscriptionScreen> {
  late final DoctorService _service;
  List<SubscriptionPackage> _items = [];
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
      _items = await _service.getSubscriptionPackages();
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
    title: 'الاشتراكات',
    child: RefreshIndicator(
      onRefresh: _load,
      child: _loading
          ? const Center(child: CircularProgressIndicator())
          : ListView.builder(
              padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
              itemCount: _items.length,
              itemBuilder: (context, index) {
                final item = _items[index];
                return Padding(
                  padding: const EdgeInsets.only(bottom: 10),
                  child: DoctorSectionCard(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Expanded(
                              child: Text(
                                item.name,
                                style: const TextStyle(
                                  fontSize: 18,
                                  fontWeight: FontWeight.w900,
                                ),
                              ),
                            ),
                            Text(
                              '${item.price.toStringAsFixed(0)} د.ع',
                              style: const TextStyle(
                                color: AppColors.primary,
                                fontWeight: FontWeight.w900,
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 8),
                        Wrap(
                          spacing: 8,
                          runSpacing: 8,
                          children: item.featureLabels
                              .map((label) => Chip(label: Text(label)))
                              .toList(),
                        ),
                      ],
                    ),
                  ),
                );
              },
            ),
    ),
  );
}
