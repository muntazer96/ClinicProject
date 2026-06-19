import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorOfferFormPage extends StatefulWidget {
  const DoctorOfferFormPage({super.key, this.offer});

  final DoctorOfferManage? offer;

  @override
  State<DoctorOfferFormPage> createState() => _DoctorOfferFormPageState();
}

class _DoctorOfferFormPageState extends State<DoctorOfferFormPage> {
  late final DoctorService _service;

  final _title = TextEditingController();
  final _description = TextEditingController();
  final _originalPrice = TextEditingController();
  final _offerPrice = TextEditingController();
  final _discount = TextEditingController();

  bool _active = true;
  DateTime? _startsAt;
  DateTime? _endsAt;
  bool _saving = false;

  bool get _editing => widget.offer != null;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);

    final offer = widget.offer;
    if (offer != null) {
      _title.text = offer.title;
      _description.text = offer.description ?? '';
      _originalPrice.text = offer.originalPrice?.toStringAsFixed(0) ?? '';
      _offerPrice.text = offer.offerPrice?.toStringAsFixed(0) ?? '';
      _discount.text = offer.discountPercent?.toStringAsFixed(0) ?? '';
      _active = offer.isActive;
      _startsAt = offer.startsAt;
      _endsAt = offer.endsAt;
    } else {
      _startsAt = DateTime.now();
      _endsAt = DateTime.now().add(const Duration(days: 7));
    }
  }

  @override
  void dispose() {
    _title.dispose();
    _description.dispose();
    _originalPrice.dispose();
    _offerPrice.dispose();
    _discount.dispose();
    super.dispose();
  }

  Future<void> _pickDate(bool start) async {
    final picked = await showDatePicker(
      context: context,
      firstDate: DateTime(2020),
      lastDate: DateTime.now().add(const Duration(days: 365)),
      initialDate: (start ? _startsAt : _endsAt) ?? DateTime.now(),
    );

    if (picked != null) {
      setState(() {
        if (start) {
          _startsAt = picked;
        } else {
          _endsAt = picked;
        }
      });
    }
  }

  Future<void> _save() async {
    if (_title.text.trim().isEmpty || _startsAt == null || _endsAt == null) {
      showAppSnackBar(context, 'أكمل بيانات العرض.');
      return;
    }

    setState(() => _saving = true);

    final data = {
      if (_editing) 'id': widget.offer!.id,
      'clinicId': null,
      'appliesToAllClinics': true,
      'title': _title.text.trim(),
      'description': _description.text.trim(),
      'offerType': 0,
      'originalPrice': double.tryParse(_originalPrice.text.trim()),
      'offerPrice': double.tryParse(_offerPrice.text.trim()),
      'discountPercent': double.tryParse(_discount.text.trim()),
      'startsAt': _startsAt!.toIso8601String(),
      'endsAt': _endsAt!.toIso8601String(),
      'isActive': _active,
    };

    try {
      if (_editing) {
        await _service.updateOffer(data);
      } else {
        await _service.addOffer(data);
      }

      if (!mounted) return;
      showAppSnackBar(context, 'تم حفظ العرض.');
      context.pop();
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
    title: _editing ? 'تعديل عرض' : 'إضافة عرض',
    showBackButton: true,
    backRoute: '/doctor/offers',
    child: ListView(
      padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
      children: [
        _HeroOfferCard(active: _active, editing: _editing),

        const SizedBox(height: 14),

        _SectionCard(
          title: 'معلومات العرض',
          icon: Icons.local_offer_rounded,
          children: [
            _AppTextField(
              controller: _title,
              label: 'عنوان العرض',
              icon: Icons.title_rounded,
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _description,
              minLines: 3,
              maxLines: 5,
              decoration: const InputDecoration(
                labelText: 'وصف العرض',
                prefixIcon: Icon(Icons.description_outlined),
                alignLabelWithHint: true,
              ),
            ),
          ],
        ),

        const SizedBox(height: 14),

        _SectionCard(
          title: 'تفاصيل السعر',
          icon: Icons.payments_rounded,
          children: [
            Row(
              children: [
                Expanded(
                  child: _AppTextField(
                    controller: _originalPrice,
                    label: 'السعر الأصلي',
                    icon: Icons.payments_outlined,
                    keyboardType: TextInputType.number,
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: _AppTextField(
                    controller: _discount,
                    label: 'نسبة الخصم',
                    icon: Icons.percent_rounded,
                    keyboardType: TextInputType.number,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            _AppTextField(
              controller: _offerPrice,
              label: 'سعر العرض - اختياري',
              icon: Icons.sell_outlined,
              keyboardType: TextInputType.number,
            ),
          ],
        ),

        const SizedBox(height: 14),

        _SectionCard(
          title: 'مدة العرض',
          icon: Icons.date_range_rounded,
          children: [
            Row(
              children: [
                Expanded(
                  child: _DateBox(
                    title: 'بداية العرض',
                    date: _startsAt!,
                    icon: Icons.play_circle_outline_rounded,
                    onTap: () => _pickDate(true),
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: _DateBox(
                    title: 'نهاية العرض',
                    date: _endsAt!,
                    icon: Icons.stop_circle_outlined,
                    onTap: () => _pickDate(false),
                  ),
                ),
              ],
            ),
          ],
        ),

        const SizedBox(height: 14),

        _SectionCard(
          title: 'حالة العرض',
          icon: Icons.tune_rounded,
          children: [
            _ActiveSwitchCard(
              value: _active,
              onChanged: (value) => setState(() => _active = value),
            ),
          ],
        ),

        const SizedBox(height: 18),

        SizedBox(
          height: 52,
          child: ElevatedButton.icon(
            onPressed: _saving ? null : _save,
            icon: _saving
                ? const SizedBox(
                    width: 18,
                    height: 18,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      color: Colors.white,
                    ),
                  )
                : const Icon(Icons.save_rounded),
            label: Text(
              _saving ? 'جاري الحفظ...' : 'حفظ العرض',
              style: const TextStyle(fontSize: 15, fontWeight: FontWeight.w900),
            ),
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.primary,
              foregroundColor: Colors.white,
              disabledBackgroundColor: AppColors.primary.withOpacity(.55),
              elevation: 0,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(16),
              ),
            ),
          ),
        ),
      ],
    ),
  );
}

class _HeroOfferCard extends StatelessWidget {
  const _HeroOfferCard({required this.active, required this.editing});

  final bool active;
  final bool editing;

  @override
  Widget build(BuildContext context) {
    return AnimatedContainer(
      duration: const Duration(milliseconds: 220),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: active
              ? const [Color(0xFF0F7F73), Color(0xFF0D625C)]
              : const [Color(0xFF7B8583), Color(0xFF5F6967)],
          begin: Alignment.topRight,
          end: Alignment.bottomLeft,
        ),
        borderRadius: BorderRadius.circular(22),
        boxShadow: [
          BoxShadow(
            color: (active ? AppColors.primary : AppColors.muted).withOpacity(
              .18,
            ),
            blurRadius: 18,
            offset: const Offset(0, 9),
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            width: 58,
            height: 58,
            decoration: BoxDecoration(
              color: Colors.white.withOpacity(.16),
              borderRadius: BorderRadius.circular(18),
            ),
            child: Icon(
              active ? Icons.local_offer_rounded : Icons.visibility_off_rounded,
              color: Colors.white,
              size: 31,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              editing
                  ? 'عدّل تفاصيل العرض ومدة ظهوره للمستخدمين'
                  : 'أنشئ عرضاً جذاباً يظهر للمستخدمين داخل التطبيق',
              style: const TextStyle(
                color: Colors.white,
                fontSize: 15,
                height: 1.5,
                fontWeight: FontWeight.w900,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _SectionCard extends StatelessWidget {
  const _SectionCard({
    required this.title,
    required this.icon,
    required this.children,
  });

  final String title;
  final IconData icon;
  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: context.appSurface,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: context.appBorder),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(context.isDark ? .18 : .025),
            blurRadius: 12,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              Icon(icon, color: AppColors.primary, size: 20),
              const SizedBox(width: 7),
              Text(
                title,
                style: const TextStyle(
                  fontSize: 15,
                  fontWeight: FontWeight.w900,
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),
          ...children,
        ],
      ),
    );
  }
}

class _AppTextField extends StatelessWidget {
  const _AppTextField({
    required this.controller,
    required this.label,
    required this.icon,
    this.keyboardType,
  });

  final TextEditingController controller;
  final String label;
  final IconData icon;
  final TextInputType? keyboardType;

  @override
  Widget build(BuildContext context) {
    return TextField(
      controller: controller,
      keyboardType: keyboardType,
      decoration: InputDecoration(labelText: label, prefixIcon: Icon(icon)),
    );
  }
}

class _DateBox extends StatelessWidget {
  const _DateBox({
    required this.title,
    required this.date,
    required this.icon,
    required this.onTap,
  });

  final String title;
  final DateTime date;
  final IconData icon;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: context.appSurfaceMuted,
      borderRadius: BorderRadius.circular(16),
      child: InkWell(
        borderRadius: BorderRadius.circular(16),
        onTap: onTap,
        child: Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(16),
            border: Border.all(color: context.appBorder),
          ),
          child: Column(
            children: [
              Icon(icon, color: AppColors.primary, size: 24),
              const SizedBox(height: 8),
              Text(
                title,
                style: TextStyle(
                  fontSize: 11.5,
                  color: context.appMuted,
                  fontWeight: FontWeight.w700,
                ),
              ),
              const SizedBox(height: 5),
              Text(
                DateFormat('yyyy/MM/dd').format(date),
                textAlign: TextAlign.center,
                style: const TextStyle(
                  fontSize: 13,
                  fontWeight: FontWeight.w900,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _ActiveSwitchCard extends StatelessWidget {
  const _ActiveSwitchCard({required this.value, required this.onChanged});

  final bool value;
  final ValueChanged<bool> onChanged;

  @override
  Widget build(BuildContext context) {
    final color = value ? AppColors.primary : AppColors.muted;

    return AnimatedContainer(
      duration: const Duration(milliseconds: 200),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 12),
      decoration: BoxDecoration(
        color: color.withOpacity(.08),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: color.withOpacity(.18)),
      ),
      child: Row(
        children: [
          Icon(
            value ? Icons.check_circle_rounded : Icons.pause_circle_rounded,
            color: color,
            size: 24,
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              value ? 'العرض فعال ويظهر للمستخدمين' : 'العرض متوقف حالياً',
              style: TextStyle(
                fontSize: 14,
                fontWeight: FontWeight.w900,
                color: color,
              ),
            ),
          ),
          Switch(value: value, activeColor: color, onChanged: onChanged),
        ],
      ),
    );
  }
}
