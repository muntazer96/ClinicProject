import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorProfileEditPage extends StatefulWidget {
  const DoctorProfileEditPage({super.key, required this.profile});

  final DoctorProfile profile;

  @override
  State<DoctorProfileEditPage> createState() => _DoctorProfileEditPageState();
}

class _DoctorProfileEditPageState extends State<DoctorProfileEditPage> {
  late final DoctorService _service;
  late final TextEditingController _name;
  late final TextEditingController _description;
  late final TextEditingController _phone;
  late final TextEditingController _location;
  bool _saving = false;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);
    _name = TextEditingController(text: widget.profile.name);
    _description = TextEditingController(text: widget.profile.description);
    _phone = TextEditingController(text: widget.profile.phoneNumber);
    _location = TextEditingController(text: widget.profile.location);
  }

  @override
  void dispose() {
    _name.dispose();
    _description.dispose();
    _phone.dispose();
    _location.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    setState(() => _saving = true);
    try {
      await _service.updateProfile(
        widget.profile,
        name: _name.text,
        description: _description.text,
        phoneNumber: _phone.text,
        location: _location.text,
      );
      if (!mounted) return;
      showAppSnackBar(context, 'تم حفظ بيانات الطبيب.');
      context.pop();
    } catch (error) {
      if (mounted) showAppSnackBar(context, ApiClient.errorMessage(error));
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
    title: 'تعديل بيانات الطبيب',
    showBackButton: true,
    backRoute: '/doctor/profile',
    child: ListView(
      padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
      children: [
        TextField(
          controller: _name,
          decoration: const InputDecoration(labelText: 'اسم الطبيب'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _description,
          minLines: 3,
          maxLines: 5,
          decoration: const InputDecoration(labelText: 'النبذة'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _phone,
          keyboardType: TextInputType.phone,
          decoration: const InputDecoration(labelText: 'رقم الهاتف'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _location,
          decoration: const InputDecoration(labelText: 'الموقع'),
        ),
        const SizedBox(height: 16),
        FilledButton.icon(
          onPressed: _saving ? null : _save,
          icon: const Icon(Icons.save_rounded),
          label: Text(_saving ? 'جاري الحفظ...' : 'حفظ'),
        ),
      ],
    ),
  );
}
