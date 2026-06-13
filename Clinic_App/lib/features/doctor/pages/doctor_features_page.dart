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

  int get _enabledCount => _items.where((item) => item.isEnabled).length;

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
                  : ListView(
                      padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
                      children: [
                        _FeaturesHeroCard(
                          doctorName: widget.profile.name,
                          enabled: _enabledCount,
                          total: _items.length,
                        ),
                        const SizedBox(height: 14),
                        const _SectionTitle(title: 'المميزات المتاحة'),
                        const SizedBox(height: 10),
                        ..._items.map(
                          (item) => _FeatureCard(
                            item: item,
                            onToggle: () => _toggle(item),
                          ),
                        ),
                      ],
                    ),
        ),
      );
}

class _FeaturesHeroCard extends StatelessWidget {
  const _FeaturesHeroCard({
    required this.doctorName,
    required this.enabled,
    required this.total,
  });

  final String doctorName;
  final int enabled;
  final int total;

  @override
  Widget build(BuildContext context) {
    final percent = total == 0 ? 0.0 : enabled / total;

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [Color(0xFF0F7F73), Color(0xFF0D625C)],
          begin: Alignment.topRight,
          end: Alignment.bottomLeft,
        ),
        borderRadius: BorderRadius.circular(24),
        boxShadow: [
          BoxShadow(
            color: AppColors.primary.withOpacity(.18),
            blurRadius: 18,
            offset: const Offset(0, 9),
          ),
        ],
      ),
      child: Row(
        children: [
          SizedBox(
            width: 66,
            height: 66,
            child: Stack(
              alignment: Alignment.center,
              children: [
                CircularProgressIndicator(
                  value: percent,
                  strokeWidth: 6,
                  backgroundColor: Colors.white.withOpacity(.20),
                  color: Colors.white,
                ),
                const Icon(
                  Icons.auto_awesome_rounded,
                  color: Colors.white,
                  size: 30,
                ),
              ],
            ),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  doctorName,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 17,
                    fontWeight: FontWeight.w900,
                  ),
                ),
                const SizedBox(height: 6),
                Text(
                  'المفعّل $enabled من أصل $total ميزة',
                  style: TextStyle(
                    color: Colors.white.withOpacity(.88),
                    fontSize: 13,
                    fontWeight: FontWeight.w800,
                  ),
                ),
                const SizedBox(height: 8),
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                  decoration: BoxDecoration(
                    color: Colors.white.withOpacity(.15),
                    borderRadius: BorderRadius.circular(999),
                  ),
                  child: const Text(
                    'تحكم بمميزات ظهور الطبيب داخل التطبيق',
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 11.5,
                      fontWeight: FontWeight.w800,
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _FeatureCard extends StatelessWidget {
  const _FeatureCard({
    required this.item,
    required this.onToggle,
  });

  final DoctorFeatureItem item;
  final VoidCallback onToggle;

  @override
  Widget build(BuildContext context) {
    final enabled = item.isEnabled;
    final color = enabled ? AppColors.primary : AppColors.muted;

    return AnimatedContainer(
      duration: const Duration(milliseconds: 220),
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(
          color: enabled ? const Color(0xFFDDE9E7) : const Color(0xFFE2E6E5),
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(enabled ? .035 : .018),
            blurRadius: 14,
            offset: const Offset(0, 7),
          ),
        ],
      ),
      child: Row(
        children: [
          AnimatedContainer(
            duration: const Duration(milliseconds: 220),
            width: 50,
            height: 50,
            decoration: BoxDecoration(
              color: color.withOpacity(.10),
              borderRadius: BorderRadius.circular(16),
            ),
            child: Icon(
              enabled ? Icons.check_circle_rounded : Icons.lock_outline_rounded,
              color: color,
              size: 28,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  item.name,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontSize: 15.5,
                    fontWeight: FontWeight.w900,
                  ),
                ),
                const SizedBox(height: 5),
                Text(
                  item.description?.isNotEmpty == true
                      ? item.description!
                      : item.normalizedName,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    height: 1.4,
                    fontSize: 12,
                    color: Colors.grey.shade600,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const SizedBox(height: 8),
                _FeatureStatusPill(enabled: enabled),
              ],
            ),
          ),
          const SizedBox(width: 8),
          Switch(
            value: enabled,
            activeColor: AppColors.primary,
            onChanged: (_) => onToggle(),
          ),
        ],
      ),
    );
  }
}

class _FeatureStatusPill extends StatelessWidget {
  const _FeatureStatusPill({required this.enabled});

  final bool enabled;

  @override
  Widget build(BuildContext context) {
    final color = enabled ? AppColors.primary : AppColors.muted;

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 5),
      decoration: BoxDecoration(
        color: color.withOpacity(.09),
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: color.withOpacity(.18)),
      ),
      child: Text(
        enabled ? 'مفعّلة حالياً' : 'غير مفعّلة',
        style: TextStyle(
          color: color,
          fontSize: 11,
          fontWeight: FontWeight.w900,
        ),
      ),
    );
  }
}

class _SectionTitle extends StatelessWidget {
  const _SectionTitle({required this.title});

  final String title;

  @override
  Widget build(BuildContext context) => Text(
        title,
        style: const TextStyle(
          fontSize: 16,
          fontWeight: FontWeight.w900,
        ),
      );
}
