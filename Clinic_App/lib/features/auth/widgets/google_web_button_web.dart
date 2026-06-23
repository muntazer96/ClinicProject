import 'package:flutter/material.dart';
import 'package:google_sign_in_web/web_only.dart' as google_web;

import '../../../core/app_theme.dart';

Widget buildGoogleSignInWebButton() => Builder(
  builder: (context) => SizedBox(
    width: double.infinity,
    child: Center(
      child: google_web.renderButton(
        configuration: google_web.GSIButtonConfiguration(
          type: google_web.GSIButtonType.standard,
          theme: context.isDark
              ? google_web.GSIButtonTheme.filledBlack
              : google_web.GSIButtonTheme.outline,
          size: google_web.GSIButtonSize.large,
          text: google_web.GSIButtonText.continueWith,
          shape: google_web.GSIButtonShape.pill,
          logoAlignment: google_web.GSIButtonLogoAlignment.left,
          minimumWidth: 320,
          locale: 'ar',
        ),
      ),
    ),
  ),
);
