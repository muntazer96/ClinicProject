import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
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
      setState(() => start ? _startsAt = picked : _endsAt = picked);
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
        TextField(
          controller: _title,
          decoration: const InputDecoration(labelText: 'عنوان العرض'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _description,
          minLines: 3,
          maxLines: 5,
          decoration: const InputDecoration(labelText: 'الوصف'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _originalPrice,
          keyboardType: TextInputType.number,
          decoration: const InputDecoration(labelText: 'السعر الأصلي'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _offerPrice,
          keyboardType: TextInputType.number,
          decoration: const InputDecoration(labelText: 'سعر العرض'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _discount,
          keyboardType: TextInputType.number,
          decoration: const InputDecoration(labelText: 'نسبة الخصم'),
        ),
        const SizedBox(height: 10),
        Row(
          children: [
            Expanded(
              child: OutlinedButton(
                onPressed: () => _pickDate(true),
                child: Text('البداية: ${_startsAt!.year}/${_startsAt!.month}/${_startsAt!.day}'),
              ),
            ),
            const SizedBox(width: 8),
            Expanded(
              child: OutlinedButton(
                onPressed: () => _pickDate(false),
                child: Text('النهاية: ${_endsAt!.year}/${_endsAt!.month}/${_endsAt!.day}'),
              ),
            ),
          ],
        ),
        SwitchListTile(
          value: _active,
          onChanged: (value) => setState(() => _active = value),
          title: const Text('العرض فعال'),
        ),
        const SizedBox(height: 16),
        FilledButton.icon(
          onPressed: _saving ? null : _save,
          icon: const Icon(Icons.save_rounded),
          label: Text(_saving ? 'جاري الحفظ...' : 'حفظ العرض'),
        ),
      ],
    ),
  );
}
