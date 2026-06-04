import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

abstract final class AppColors {
  static const primary = Color(0xFF0F766E);
  static const primaryDark = Color(0xFF155E75);
  static const accent = Color(0xFFF59E0B);
  static const coral = Color(0xFFEF6461);
  static const success = Color(0xFF16A34A);
  static const danger = Color(0xFFDC2626);
  static const warning = accent;
  static const background = Color(0xFFF7FAF9);
  static const surface = Colors.white;
  static const surfaceMuted = Color(0xFFF0F6F4);
  static const text = Color(0xFF17201F);
  static const muted = Color(0xFF6B7C78);
  static const border = Color(0xFFE0E8E5);
  static const softBlue = Color(0xFFE5F2F6);
  static const softAmber = Color(0xFFFFF4DB);
  static const softCoral = Color(0xFFFFECEB);
}

ThemeData buildAppTheme() {
  final base = ThemeData(
    colorScheme: ColorScheme.fromSeed(
      seedColor: AppColors.primary,
      primary: AppColors.primary,
      secondary: AppColors.accent,
      error: AppColors.danger,
      surface: AppColors.surface,
    ),
    scaffoldBackgroundColor: AppColors.background,
    useMaterial3: true,
  );
  final textTheme = GoogleFonts.cairoTextTheme(
    base.textTheme,
  ).apply(bodyColor: AppColors.text, displayColor: AppColors.text);

  final rounded8 = RoundedRectangleBorder(
    borderRadius: BorderRadius.circular(8),
  );

  return base.copyWith(
    textTheme: textTheme,
    visualDensity: VisualDensity.standard,
    appBarTheme: AppBarTheme(
      backgroundColor: Colors.white,
      foregroundColor: AppColors.text,
      surfaceTintColor: Colors.transparent,
      elevation: 0,
      centerTitle: false,
      titleTextStyle: GoogleFonts.cairo(
        color: AppColors.text,
        fontSize: 18,
        fontWeight: FontWeight.w800,
      ),
    ),
    cardTheme: CardThemeData(
      color: Colors.white,
      surfaceTintColor: Colors.transparent,
      elevation: 0,
      margin: EdgeInsets.zero,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(8),
        side: const BorderSide(color: AppColors.border),
      ),
    ),
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        minimumSize: const Size.fromHeight(52),
        textStyle: GoogleFonts.cairo(fontSize: 14, fontWeight: FontWeight.w800),
        shape: rounded8,
      ),
    ),
    outlinedButtonTheme: OutlinedButtonThemeData(
      style: OutlinedButton.styleFrom(
        foregroundColor: AppColors.primary,
        minimumSize: const Size.fromHeight(48),
        side: const BorderSide(color: AppColors.border),
        textStyle: GoogleFonts.cairo(fontSize: 14, fontWeight: FontWeight.w800),
        shape: rounded8,
      ),
    ),
    textButtonTheme: TextButtonThemeData(
      style: TextButton.styleFrom(
        foregroundColor: AppColors.primary,
        textStyle: GoogleFonts.cairo(fontSize: 13, fontWeight: FontWeight.w800),
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: Colors.white,
      hintStyle: const TextStyle(color: AppColors.muted),
      labelStyle: const TextStyle(color: AppColors.muted),
      prefixIconColor: AppColors.primary,
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 15),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: BorderSide.none,
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: const BorderSide(color: AppColors.border),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: const BorderSide(color: AppColors.primary, width: 1.5),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: const BorderSide(color: AppColors.danger),
      ),
    ),
    chipTheme: base.chipTheme.copyWith(
      backgroundColor: AppColors.surfaceMuted,
      selectedColor: AppColors.softBlue,
      side: const BorderSide(color: AppColors.border),
      labelStyle: GoogleFonts.cairo(
        color: AppColors.text,
        fontSize: 12,
        fontWeight: FontWeight.w700,
      ),
      shape: rounded8,
    ),
    navigationBarTheme: NavigationBarThemeData(
      height: 72,
      backgroundColor: Colors.white,
      indicatorColor: AppColors.softBlue,
      labelTextStyle: WidgetStatePropertyAll(
        GoogleFonts.cairo(fontSize: 11, fontWeight: FontWeight.w800),
      ),
      iconTheme: const WidgetStatePropertyAll(
        IconThemeData(color: AppColors.primary),
      ),
    ),
    snackBarTheme: SnackBarThemeData(
      backgroundColor: AppColors.text,
      contentTextStyle: GoogleFonts.cairo(color: Colors.white),
      behavior: SnackBarBehavior.floating,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
    ),
    bottomSheetTheme: const BottomSheetThemeData(
      backgroundColor: Colors.white,
      surfaceTintColor: Colors.transparent,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(8)),
      ),
    ),
  );
}
