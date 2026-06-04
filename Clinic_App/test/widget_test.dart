import 'package:clinic_app/app.dart';
import 'package:clinic_app/core/api_client.dart';
import 'package:clinic_app/core/app_theme.dart';
import 'package:clinic_app/features/auth/auth_controller.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';

void main() {
  testWidgets('startup splash renders app logo', (tester) async {
    await tester.pumpWidget(
      ChangeNotifierProvider(
        create: (_) => AuthController(ApiClient()),
        child: const ClinicApp(),
      ),
    );
    expect(find.text('عيادتي'), findsWidgets);
    expect(find.byType(Image), findsWidgets);
    await tester.pump(const Duration(milliseconds: 1500));
  });

  testWidgets('the app button theme supports icon buttons inside rows', (
    tester,
  ) async {
    await tester.pumpWidget(
      MaterialApp(
        theme: buildAppTheme(),
        home: Scaffold(
          body: Row(
            children: [
              const Expanded(child: Text('4.8 من 5')),
              FilledButton.icon(
                onPressed: () {},
                icon: const Icon(Icons.rate_review_outlined),
                label: const Text('عرض التقييمات'),
              ),
            ],
          ),
        ),
      ),
    );

    expect(tester.takeException(), isNull);
    expect(find.text('عرض التقييمات'), findsOneWidget);
  });
}
