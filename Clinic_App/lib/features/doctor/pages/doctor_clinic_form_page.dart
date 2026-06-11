import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorClinicFormPage extends StatefulWidget {
  const DoctorClinicFormPage({super.key, this.clinic});

  final DoctorClinic? clinic;

  @override
  State<DoctorClinicFormPage> createState() => _DoctorClinicFormPageState();
}

class _DoctorClinicFormPageState extends State<DoctorClinicFormPage> {
  late final DoctorService _service;

  final _name = TextEditingController();
  final _address = TextEditingController();
  final _phone = TextEditingController();
  final _price = TextEditingController();
  final _mapUrl = TextEditingController();

  List<ProvinceItem> _provinces = [];
  int? _provinceId;

  bool _loadingProvinces = true;
  bool _showPrice = false;
  bool _visible = true;
  bool _saving = false;

  bool get _editing => widget.clinic != null;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);

    final clinic = widget.clinic;
    if (clinic != null) {
      _name.text = clinic.name;
      _provinceId = clinic.iraqiProvince;
      _address.text = clinic.address;
      _phone.text = clinic.phoneNumber ?? '';
      _price.text = clinic.consultationPrice?.toStringAsFixed(0) ?? '';
      _mapUrl.text = clinic.mapUrl ?? '';
      _showPrice = clinic.showConsultationPrice;
      _visible = clinic.isVisible;
    }

    _loadProvinces();
  }

  @override
  void dispose() {
    _name.dispose();
    _address.dispose();
    _phone.dispose();
    _price.dispose();
    _mapUrl.dispose();
    super.dispose();
  }

  Future<void> _loadProvinces() async {
    try {
      final items = await _service.getProvinces();
      if (!mounted) return;

      setState(() {
        _provinces = items;
        _provinceId ??= items.isNotEmpty ? items.first.id : null;
        _loadingProvinces = false;
      });
    } catch (error) {
      if (!mounted) return;
      setState(() => _loadingProvinces = false);
      showAppSnackBar(context, ApiClient.errorMessage(error));
    }
  }

  Future<void> _save() async {
    if (_name.text.trim().isEmpty ||
        _address.text.trim().isEmpty ||
        _provinceId == null) {
      showAppSnackBar(context, 'أكمل بيانات العيادة.');
      return;
    }

    setState(() => _saving = true);

    final data = {
      if (_editing) 'id': widget.clinic!.id,
      'name': _name.text.trim(),
      'iraqiProvince': _provinceId,
      'address': _address.text.trim(),
      'mapUrl': _mapUrl.text.trim().isEmpty ? null : _mapUrl.text.trim(),
      'phoneNumber': _phone.text.trim(),
      'consultationPrice': double.tryParse(_price.text.trim()),
      'showConsultationPrice': _showPrice,
      'isVisible': _visible,
    };

    try {
      if (_editing) {
        await _service.updateClinic(data);
      } else {
        await _service.addClinic(data);
      }

      if (!mounted) return;
      showAppSnackBar(context, 'تم حفظ العيادة.');
      context.pop();
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
        title: _editing ? 'تعديل عيادة' : 'إضافة عيادة',
        showBackButton: true,
        backRoute: '/doctor/clinics',
        child: ListView(
          padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
          children: [
            _HeaderCard(editing: _editing),

            const SizedBox(height: 14),

            _FormSection(
              title: 'معلومات العيادة',
              icon: Icons.local_hospital_rounded,
              children: [
                _AppTextField(
                  controller: _name,
                  label: 'اسم العيادة',
                  icon: Icons.apartment_rounded,
                ),
                const SizedBox(height: 12),
                DropdownButtonFormField<int>(
                  value: _provinces.any((item) => item.id == _provinceId)
                      ? _provinceId
                      : null,
                  decoration: InputDecoration(
                    labelText: 'المحافظة',
                    prefixIcon: const Icon(Icons.map_rounded),
                    suffixIcon: _loadingProvinces
                        ? const Padding(
                            padding: EdgeInsets.all(12),
                            child: SizedBox(
                              width: 18,
                              height: 18,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            ),
                          )
                        : null,
                  ),
                  items: _provinces
                      .map(
                        (province) => DropdownMenuItem<int>(
                          value: province.id,
                          child: Text(province.name),
                        ),
                      )
                      .toList(),
                  onChanged: _loadingProvinces
                      ? null
                      : (value) => setState(() => _provinceId = value),
                ),
                const SizedBox(height: 12),
                _AppTextField(
                  controller: _address,
                  label: 'العنوان التفصيلي',
                  icon: Icons.place_rounded,
                ),
                const SizedBox(height: 12),
                _AppTextField(
                  controller: _phone,
                  label: 'رقم هاتف العيادة',
                  icon: Icons.phone_rounded,
                  keyboardType: TextInputType.phone,
                ),
              ],
            ),

            const SizedBox(height: 14),

            _FormSection(
              title: 'السعر والموقع',
              icon: Icons.payments_rounded,
              children: [
                _AppTextField(
                  controller: _price,
                  label: 'سعر الكشف',
                  icon: Icons.payments_outlined,
                  keyboardType: TextInputType.number,
                ),
                const SizedBox(height: 12),
                _AppTextField(
                  controller: _mapUrl,
                  label: 'رابط الخريطة',
                  icon: Icons.location_on_rounded,
                  keyboardType: TextInputType.url,
                ),
              ],
            ),

            const SizedBox(height: 14),

            _FormSection(
              title: 'إعدادات الظهور',
              icon: Icons.tune_rounded,
              children: [
                _PrettySwitchTile(
                  title: 'إظهار السعر للمستخدمين',
                  subtitle: 'يعرض سعر الكشف داخل صفحة العيادة',
                  icon: Icons.visibility_rounded,
                  value: _showPrice,
                  onChanged: (value) => setState(() => _showPrice = value),
                ),
                const SizedBox(height: 10),
                _PrettySwitchTile(
                  title: 'العيادة ظاهرة',
                  subtitle: 'تظهر العيادة للمرضى ضمن التطبيق',
                  icon: Icons.storefront_rounded,
                  value: _visible,
                  onChanged: (value) => setState(() => _visible = value),
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
                  _saving ? 'جاري الحفظ...' : 'حفظ العيادة',
                  style: const TextStyle(
                    fontSize: 15,
                    fontWeight: FontWeight.w900,
                  ),
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

class _HeaderCard extends StatelessWidget {
  const _HeaderCard({required this.editing});

  final bool editing;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [Color(0xFF0F7F73), Color(0xFF106B64)],
          begin: Alignment.topRight,
          end: Alignment.bottomLeft,
        ),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(
        children: [
          Container(
            width: 52,
            height: 52,
            decoration: BoxDecoration(
              color: Colors.white.withOpacity(.14),
              borderRadius: BorderRadius.circular(16),
            ),
            child: Icon(
              editing ? Icons.edit_location_alt_rounded : Icons.add_business_rounded,
              color: Colors.white,
              size: 29,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              editing
                  ? 'عدّل بيانات العيادة ومعلومات الظهور'
                  : 'أضف عيادة جديدة لحساب الطبيب',
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

class _FormSection extends StatelessWidget {
  const _FormSection({
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
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: const Color(0xFFDDE9E7)),
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
      decoration: InputDecoration(
        labelText: label,
        prefixIcon: Icon(icon),
      ),
    );
  }
}

class _PrettySwitchTile extends StatelessWidget {
  const _PrettySwitchTile({
    required this.title,
    required this.subtitle,
    required this.icon,
    required this.value,
    required this.onChanged,
  });

  final String title;
  final String subtitle;
  final IconData icon;
  final bool value;
  final ValueChanged<bool> onChanged;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 11),
      decoration: BoxDecoration(
        color: const Color(0xFFF7FAFA),
        borderRadius: BorderRadius.circular(15),
        border: Border.all(color: const Color(0xFFE3ECEA)),
      ),
      child: Row(
        children: [
          Icon(icon, color: AppColors.primary, size: 22),
          const SizedBox(width: 10),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: const TextStyle(
                    fontSize: 13.5,
                    fontWeight: FontWeight.w900,
                  ),
                ),
                const SizedBox(height: 3),
                Text(
                  subtitle,
                  style: TextStyle(
                    fontSize: 11.5,
                    color: Colors.grey.shade600,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ],
            ),
          ),
          Switch(
            value: value,
            activeColor: AppColors.primary,
            onChanged: onChanged,
          ),
        ],
      ),
    );
  }
}