import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';
import 'package:provider/provider.dart';

import '../../../core/api_client.dart';
import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
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
  late DoctorProfile _profile;
  bool _saving = false;
  bool _uploadingImage = false;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);
    _profile = widget.profile;
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
        _profile,
        name: _name.text.trim(),
        description: _description.text.trim(),
        phoneNumber: _phone.text.trim(),
        location: _location.text.trim(),
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

  Future<void> _changeDoctorImage() async {
    if (_uploadingImage) return;
    final image = await ImagePicker().pickImage(
      source: ImageSource.gallery,
      imageQuality: 85,
      maxWidth: 1200,
    );
    if (image == null || !mounted) return;
    final imageBytes = await image.readAsBytes();

    await showModalBottomSheet<void>(
      context: context,
      showDragHandle: true,
      builder: (sheetContext) {
        var uploading = false;
        var sheetOpen = true;
        return StatefulBuilder(
          builder: (context, setSheetState) => SafeArea(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(16, 6, 16, 18),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  ClipRRect(
                    borderRadius: BorderRadius.circular(16),
                    child: Image.memory(
                      imageBytes,
                      height: 220,
                      fit: BoxFit.cover,
                    ),
                  ),
                  const SizedBox(height: 14),
                  FilledButton.icon(
                    onPressed: uploading
                        ? null
                        : () async {
                            setSheetState(() => uploading = true);
                            setState(() => _uploadingImage = true);
                            try {
                              await _service.updateProfileImage(
                                fileName: image.name,
                                bytes: imageBytes,
                              );
                              final updated = await _service.getProfile();
                              if (!mounted) return;
                              setState(() => _profile = updated);
                              sheetOpen = false;
                              Navigator.of(sheetContext).pop();
                              showAppSnackBar(
                                context,
                                'تم تحديث صورة الطبيب بنجاح.',
                                type: AppSnackBarType.success,
                              );
                            } catch (error) {
                              if (mounted) {
                                showAppSnackBar(
                                  context,
                                  ApiClient.errorMessage(error),
                                  type: AppSnackBarType.error,
                                );
                              }
                            } finally {
                              if (mounted) {
                                setState(() => _uploadingImage = false);
                              }
                              if (sheetOpen) {
                                setSheetState(() => uploading = false);
                              }
                            }
                          },
                    icon: uploading
                        ? const SizedBox(
                            width: 18,
                            height: 18,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Icon(Icons.cloud_upload_rounded),
                    label: Text(
                      uploading ? 'جاري رفع الصورة...' : 'رفع الصورة',
                    ),
                  ),
                  const SizedBox(height: 8),
                  OutlinedButton.icon(
                    onPressed: uploading
                        ? null
                        : () {
                            sheetOpen = false;
                            Navigator.of(sheetContext).pop();
                          },
                    icon: const Icon(Icons.close_rounded),
                    label: const Text('إلغاء الصورة المختارة'),
                  ),
                ],
              ),
            ),
          ),
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
    title: 'تعديل بيانات الطبيب',
    showBackButton: true,
    backRoute: '/doctor/profile',
    child: ListView(
      padding: const EdgeInsets.fromLTRB(16, 14, 16, 28),
      children: [
        _DoctorEditHero(
          profile: _profile,
          uploadingImage: _uploadingImage,
          onChangeImage: _changeDoctorImage,
        ),

        const SizedBox(height: 14),

        _FormSection(
          title: 'المعلومات الأساسية',
          icon: Icons.badge_rounded,
          children: [
            _AppTextField(
              controller: _name,
              label: 'اسم الطبيب',
              icon: Icons.person_rounded,
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _description,
              minLines: 3,
              maxLines: 5,
              decoration: const InputDecoration(
                labelText: 'النبذة التعريفية',
                prefixIcon: Icon(Icons.description_outlined),
                alignLabelWithHint: true,
              ),
            ),
          ],
        ),

        const SizedBox(height: 14),

        _FormSection(
          title: 'معلومات التواصل',
          icon: Icons.contact_phone_rounded,
          children: [
            _AppTextField(
              controller: _phone,
              label: 'رقم الهاتف',
              icon: Icons.phone_rounded,
              keyboardType: TextInputType.phone,
            ),
            const SizedBox(height: 12),
            _AppTextField(
              controller: _location,
              label: 'الموقع',
              icon: Icons.location_on_rounded,
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
              _saving ? 'جاري الحفظ...' : 'حفظ بيانات الطبيب',
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

class _DoctorEditHero extends StatelessWidget {
  const _DoctorEditHero({
    required this.profile,
    required this.uploadingImage,
    required this.onChangeImage,
  });

  final DoctorProfile profile;
  final bool uploadingImage;
  final VoidCallback onChangeImage;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [Color(0xFF0F7F73), Color(0xFF0D625C)],
          begin: Alignment.topRight,
          end: Alignment.bottomLeft,
        ),
        borderRadius: BorderRadius.circular(22),
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
          Stack(
            clipBehavior: Clip.none,
            children: [
              CircleAvatar(
                radius: 34,
                backgroundColor: Colors.white.withOpacity(.18),
                child: ClipOval(
                  child: profile.imageUrl == null
                      ? const _DoctorImageFallback()
                      : Image.network(
                          profile.imageUrl!,
                          width: 68,
                          height: 68,
                          fit: BoxFit.cover,
                          errorBuilder: (_, __, ___) =>
                              const _DoctorImageFallback(),
                        ),
                ),
              ),
              Positioned(
                bottom: -4,
                left: -4,
                child: Material(
                  color: Colors.white,
                  shape: const CircleBorder(),
                  child: InkWell(
                    customBorder: const CircleBorder(),
                    onTap: uploadingImage ? null : onChangeImage,
                    child: Padding(
                      padding: const EdgeInsets.all(7),
                      child: uploadingImage
                          ? const SizedBox(
                              width: 16,
                              height: 16,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Icon(
                              Icons.camera_alt_rounded,
                              size: 16,
                              color: AppColors.primary,
                            ),
                    ),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              'حدّث بياناتك لتظهر للمرضى بشكل أوضح واحترافي',
              style: TextStyle(
                color: Colors.white.withOpacity(.96),
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
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(.025),
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

class _DoctorImageFallback extends StatelessWidget {
  const _DoctorImageFallback();

  @override
  Widget build(BuildContext context) => Container(
    width: 68,
    height: 68,
    alignment: Alignment.center,
    color: Colors.white.withOpacity(.12),
    child: const Icon(
      Icons.medical_services_rounded,
      color: Colors.white,
      size: 31,
    ),
  );
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
