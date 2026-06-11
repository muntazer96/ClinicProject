import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorFeaturesPage extends StatefulWidget {
  const DoctorFeaturesPage({super.key, required this.profile});

  final DoctorProfile profile;

  @override
  State<DoctorFeaturesPage> createState() => _DoctorFeaturesPageState();
}

class _DoctorFeaturesPageState extends State<DoctorFeaturesPage> {
  late final DoctorService _service;
  List<DoctorFeatureItem> _items = [];
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
      _items = await _service.getDoctorFeatures(widget.profile.id);
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _toggle(DoctorFeatureItem item) async {
    try {
      await _service.toggleFeature(item.id);
      await _load();
    } catch (error) {
      if (mounted) {
        showAppSnackBar(
          context,
          ApiClient.errorMessage(error),
          type: AppSnackBarType.warning,
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
    title: 'إدارة المميزات',
    showBackButton: true,
    backRoute: '/doctor/profile',
    child: RefreshIndicator(
      onRefresh: _load,
      child: _loading
          ? const Center(child: CircularProgressIndicator())
          : _items.isEmpty
              ? const DoctorEmptyState(
                  icon: Icons.toggle_off_outlined,
                  message:
                      'لا توجد مميزات متاحة أو أن الـ API لا يسمح للطبيب بعرضها.',
                )
              : ListView.builder(
                  padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
                  itemCount: _items.length,
                  itemBuilder: (context, index) {
                    final item = _items[index];
                    return Padding(
                      padding: const EdgeInsets.only(bottom: 8),
                      child: DoctorSectionCard(
                        child: SwitchListTile(
                          contentPadding: EdgeInsets.zero,
                          value: item.isEnabled,
                          onChanged: (_) => _toggle(item),
                          title: Text(item.name),
                          subtitle: Text(
                            item.description ?? item.normalizedName,
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                      ),
                    );
                  },
                ),
    ),
  );
}
