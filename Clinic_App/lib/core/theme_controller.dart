import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class ThemeController extends ChangeNotifier {
  ThemeController({FlutterSecureStorage? storage})
    : _storage = storage ?? const FlutterSecureStorage();

  static const _storageKey = 'clinic_app_theme_mode';
  final FlutterSecureStorage _storage;
  ThemeMode _mode = ThemeMode.system;

  ThemeMode get mode => _mode;

  Future<void> restore() async {
    final value = await _storage.read(key: _storageKey);
    _mode = switch (value) {
      'light' => ThemeMode.light,
      'dark' => ThemeMode.dark,
      _ => ThemeMode.system,
    };
    notifyListeners();
  }

  Future<void> setMode(ThemeMode mode) async {
    if (_mode == mode) return;
    _mode = mode;
    await _storage.write(
      key: _storageKey,
      value: switch (mode) {
        ThemeMode.light => 'light',
        ThemeMode.dark => 'dark',
        ThemeMode.system => 'system',
      },
    );
    notifyListeners();
  }
}
