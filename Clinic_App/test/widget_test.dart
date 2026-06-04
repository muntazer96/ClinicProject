import 'package:clinic_app/app.dart';
import 'package:clinic_app/core/api_client.dart';
import 'package:clinic_app/features/auth/auth_controller.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';

void main() {
  testWidgets('home screen renders Arabic introduction', (tester) async {
    await tester.pumpWidget(
      ChangeNotifierProvider(
        create: (_) => AuthController(ApiClient()),
        child: const ClinicApp(),
      ),
    );
    await tester.pumpAndSettle();
    expect(find.byIcon(Icons.search_rounded), findsWidgets);
    expect(find.byIcon(Icons.medical_services_rounded), findsWidgets);
    expect(find.byIcon(Icons.event_available_rounded), findsWidgets);
  });
}
