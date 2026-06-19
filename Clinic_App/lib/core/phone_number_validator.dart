import 'package:flutter/services.dart';

const iraqiPhoneError = 'رقم الهاتف يجب أن يكون 11 رقم ويبدأ بـ 07.';

final iraqiPhoneRegExp = RegExp(r'^07\d{9}$');

bool isValidIraqiPhone(String value) => iraqiPhoneRegExp.hasMatch(value.trim());

List<TextInputFormatter> get iraqiPhoneInputFormatters => [
  FilteringTextInputFormatter.digitsOnly,
  LengthLimitingTextInputFormatter(11),
];
