import 'package:flutter/foundation.dart';

class NotificationCenter extends ChangeNotifier {
  int _patientVersion = 0;
  int _doctorVersion = 0;

  int versionFor({required bool doctor}) =>
      doctor ? _doctorVersion : _patientVersion;

  void notifyChanged({required bool doctor}) {
    if (doctor) {
      _doctorVersion++;
    } else {
      _patientVersion++;
    }
    notifyListeners();
  }
}
