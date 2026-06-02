import 'package:flutter/material.dart';
import '../../widgets/app_scaffold.dart';
class MyBookingsPlaceholder extends StatelessWidget {
  const MyBookingsPlaceholder({super.key});
  @override Widget build(BuildContext context) => const AppScaffold(title: 'حجوزاتي', child: Center(child: Padding(padding: EdgeInsets.all(24), child: Text('تم تجهيز المسار وحمايته بالحساب. سنبني إدارة الحجوزات في الخطوة الرابعة.'))));
}
