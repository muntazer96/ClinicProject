import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'app.dart';
import 'core/api_client.dart';
import 'features/auth/auth_controller.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  final auth = AuthController(ApiClient());
  await auth.restoreSession();
  runApp(ChangeNotifierProvider.value(value: auth, child: const ClinicApp()));
}
