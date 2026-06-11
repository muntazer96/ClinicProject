import 'package:flutter/material.dart';

import '../widgets/doctor_scaffold.dart';

class DoctorNotificationsScreen extends StatelessWidget {
  const DoctorNotificationsScreen({super.key});

  @override
  Widget build(BuildContext context) => const DoctorScaffold(
    title: 'الإشعارات',
    child: DoctorEmptyState(
      icon: Icons.notifications_none_rounded,
      message: 'لا توجد قائمة إشعارات من الـ API حالياً. إشعارات push ستصل للجهاز عند تفعيلها.',
    ),
  );
}
