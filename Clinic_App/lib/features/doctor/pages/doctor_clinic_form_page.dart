import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
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
    child: DoctorPage(
      children: [
        TextField(
          controller: _name,
          decoration: const InputDecoration(labelText: 'اسم العيادة'),
        ),
        const SizedBox(height: 10),
        DropdownButtonFormField<int>(
          value: _provinces.any((item) => item.id == _provinceId)
              ? _provinceId
              : null,
          decoration: InputDecoration(
            labelText: 'المحافظة',
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
        const SizedBox(height: 10),
        TextField(
          controller: _address,
          decoration: const InputDecoration(labelText: 'العنوان'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _phone,
          keyboardType: TextInputType.phone,
          decoration: const InputDecoration(labelText: 'رقم الهاتف'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _price,
          keyboardType: TextInputType.number,
          decoration: const InputDecoration(labelText: 'سعر الكشف'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _mapUrl,
          decoration: const InputDecoration(labelText: 'رابط الخريطة'),
        ),
        const SizedBox(height: 12),
        DoctorSwitchTile(
          title: 'إظهار السعر',
          value: _showPrice,
          onChanged: (value) => setState(() => _showPrice = value),
        ),
        const SizedBox(height: 10),
        DoctorSwitchTile(
          title: 'العيادة ظاهرة',
          value: _visible,
          onChanged: (value) => setState(() => _visible = value),
        ),
        const SizedBox(height: 16),
        DoctorPrimaryButton(
          label: _saving ? 'جاري الحفظ...' : 'حفظ العيادة',
          icon: Icons.save_rounded,
          onPressed: _saving ? null : _save,
        ),
      ],
    ),
  );
}
