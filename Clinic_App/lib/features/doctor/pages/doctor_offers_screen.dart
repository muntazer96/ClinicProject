import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../../core/app_theme.dart';
import '../../../core/app_snack_bar.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorOffersScreen extends StatefulWidget {
  const DoctorOffersScreen({super.key});

  @override
  State<DoctorOffersScreen> createState() => _DoctorOffersScreenState();
}

class _DoctorOffersScreenState extends State<DoctorOffersScreen> {
  late final DoctorService _service;
  List<DoctorOfferManage> _items = [];
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
      _items = await _service.getOffers();
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _deleteOffer(DoctorOfferManage item) async {
    await _service.deleteOffer(item.id);
    await _load();
    if (mounted) showAppSnackBar(context, 'تم حذف العرض.');
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
    title: 'العروض',
    child: Column(
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(16, 12, 16, 0),
          child: SizedBox(
            width: double.infinity,
            child: FilledButton.icon(
              onPressed: () async {
                await context.push('/doctor/offers/form');
                await _load();
              },
              icon: const Icon(Icons.add_rounded),
              label: const Text('إضافة عرض جديد'),
            ),
          ),
        ),
        Expanded(
          child: RefreshIndicator(
      onRefresh: _load,
      child: _loading
          ? const Center(child: CircularProgressIndicator())
          : _items.isEmpty
              ? const DoctorEmptyState(
                  icon: Icons.local_offer_outlined,
                  message: 'لا توجد عروض حالياً.',
                )
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
                                    item.title,
                                    style: const TextStyle(
                                      fontWeight: FontWeight.w900,
                                      fontSize: 16,
                                    ),
                                  ),
                                ),
                                Chip(
                                  label: Text(item.isCurrentlyVisible ? 'ظاهر' : 'مخفي'),
                                ),
                              ],
                            ),
                            if (item.description?.isNotEmpty == true) ...[
                              const SizedBox(height: 6),
                              Text(item.description!),
                            ],
                            const SizedBox(height: 8),
                            Text(
                              '${DateFormat('yyyy/MM/dd').format(item.startsAt)} - ${DateFormat('yyyy/MM/dd').format(item.endsAt)}',
                              style: const TextStyle(color: AppColors.muted),
                            ),
                            Text(
                              item.clinicName ?? 'كل العيادات',
                              style: const TextStyle(color: AppColors.muted),
                            ),
                            const SizedBox(height: 10),
                            Wrap(
                              spacing: 8,
                              runSpacing: 8,
                              children: [
                                OutlinedButton.icon(
                                  onPressed: () async {
                                    await context.push(
                                      '/doctor/offers/form',
                                      extra: item,
                                    );
                                    await _load();
                                  },
                                  icon: const Icon(Icons.edit_outlined),
                                  label: const Text('تعديل'),
                                ),
                                OutlinedButton.icon(
                                  onPressed: () => _deleteOffer(item),
                                  icon: const Icon(Icons.delete_outline),
                                  label: const Text('حذف'),
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                ),
          ),
        ),
      ],
    ),
  );
}
