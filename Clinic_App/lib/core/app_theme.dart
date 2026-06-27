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

abstract final class AppDarkColors {
  static const background = Color(0xFF071312);
  static const surface = Color(0xFF101C1B);
  static const surfaceRaised = Color(0xFF172725);
  static const surfaceMuted = Color(0xFF1D302E);
  static const text = Color(0xFFEAF5F2);
  static const muted = Color(0xFF9DB2AD);
  static const border = Color(0xFF29413E);
  static const primary = Color(0xFF2DD4BF);
  static const primaryDeep = Color(0xFF0F766E);
  static const softBlue = Color(0xFF12363D);
  static const softAmber = Color(0xFF3F2E12);
  static const softCoral = Color(0xFF462220);
}

extension AppThemeX on BuildContext {
  bool get isDark => Theme.of(this).brightness == Brightness.dark;
  Color get appSurface =>
      isDark ? AppDarkColors.surfaceRaised : AppColors.surface;
  Color get appSurfaceMuted =>
      isDark ? AppDarkColors.surfaceMuted : AppColors.surfaceMuted;
  Color get appBorder => isDark ? AppDarkColors.border : AppColors.border;
  Color get appMuted => isDark ? AppDarkColors.muted : AppColors.muted;
  Color get appText => isDark ? AppDarkColors.text : AppColors.text;
  Color get appSoftBlue => isDark ? AppDarkColors.softBlue : AppColors.softBlue;
  Color get appSoftAmber =>
      isDark ? AppDarkColors.softAmber : AppColors.softAmber;
  Color get appSoftCoral =>
      isDark ? AppDarkColors.softCoral : AppColors.softCoral;
}

class AppPageTransitionsBuilder extends PageTransitionsBuilder {
  const AppPageTransitionsBuilder();

  @override
  Widget buildTransitions<T>(
    PageRoute<T> route,
    BuildContext context,
    Animation<double> animation,
    Animation<double> secondaryAnimation,
    Widget child,
  ) {
    final curved = CurvedAnimation(
      parent: animation,
      curve: Curves.easeOutCubic,
      reverseCurve: Curves.easeInCubic,
    );

    return FadeTransition(
      opacity: curved,
      child: SlideTransition(
        position: Tween<Offset>(
          begin: const Offset(0.035, 0),
          end: Offset.zero,
        ).animate(curved),
        child: child,
      ),
    );
  }
}

ThemeData buildAppTheme({Brightness brightness = Brightness.light}) {
  final dark = brightness == Brightness.dark;
  final colors = dark
      ? const ColorScheme.dark(
          primary: AppDarkColors.primary,
          onPrimary: Color(0xFF042F2B),
          secondary: AppColors.accent,
          onSecondary: Color(0xFF271A02),
          error: Color(0xFFF87171),
          onError: Color(0xFF3B0707),
          surface: AppDarkColors.surface,
          onSurface: AppDarkColors.text,
        )
      : ColorScheme.fromSeed(
          seedColor: AppColors.primary,
          primary: AppColors.primary,
          secondary: AppColors.accent,
          error: AppColors.danger,
          surface: AppColors.surface,
        );

  final background = dark ? AppDarkColors.background : AppColors.background;
  final surface = dark ? AppDarkColors.surface : AppColors.surface;
  final surfaceRaised = dark ? AppDarkColors.surfaceRaised : AppColors.surface;
  final surfaceMuted = dark
      ? AppDarkColors.surfaceMuted
      : AppColors.surfaceMuted;
  final text = dark ? AppDarkColors.text : AppColors.text;
  final muted = dark ? AppDarkColors.muted : AppColors.muted;
  final border = dark ? AppDarkColors.border : AppColors.border;
  final primary = dark ? AppDarkColors.primary : AppColors.primary;

  final base = ThemeData(
    brightness: brightness,
    colorScheme: colors,
    scaffoldBackgroundColor: background,
    useMaterial3: true,
  );
  final textTheme = GoogleFonts.cairoTextTheme(
    base.textTheme,
  ).apply(bodyColor: text, displayColor: text);

  final rounded10 = RoundedRectangleBorder(
    borderRadius: BorderRadius.circular(10),
  );

  return base.copyWith(
    textTheme: textTheme,
    visualDensity: VisualDensity.standard,
    canvasColor: background,
    pageTransitionsTheme: const PageTransitionsTheme(
      builders: {
        TargetPlatform.android: AppPageTransitionsBuilder(),
        TargetPlatform.iOS: AppPageTransitionsBuilder(),
        TargetPlatform.macOS: AppPageTransitionsBuilder(),
        TargetPlatform.windows: AppPageTransitionsBuilder(),
        TargetPlatform.linux: AppPageTransitionsBuilder(),
        TargetPlatform.fuchsia: AppPageTransitionsBuilder(),
      },
    ),
    dividerColor: border,
    appBarTheme: AppBarTheme(
      backgroundColor: surface,
      foregroundColor: text,
      surfaceTintColor: Colors.transparent,
      elevation: 0,
      centerTitle: false,
      titleTextStyle: GoogleFonts.cairo(
        color: text,
        fontSize: 18,
        fontWeight: FontWeight.w800,
      ),
      iconTheme: IconThemeData(color: text),
      actionsIconTheme: IconThemeData(color: text),
    ),
    cardTheme: CardThemeData(
      color: surfaceRaised,
      surfaceTintColor: Colors.transparent,
      elevation: 0,
      margin: EdgeInsets.zero,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(14),
        side: BorderSide(color: border),
      ),
    ),
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        backgroundColor: primary,
        foregroundColor: dark ? const Color(0xFF042F2B) : Colors.white,
        disabledBackgroundColor: dark
            ? AppDarkColors.surfaceMuted
            : const Color(0xFFE5E7EB),
        disabledForegroundColor: muted,
        minimumSize: const Size(0, 52),
        textStyle: GoogleFonts.cairo(fontSize: 14, fontWeight: FontWeight.w800),
        shape: rounded10,
      ),
    ),
    outlinedButtonTheme: OutlinedButtonThemeData(
      style: OutlinedButton.styleFrom(
        foregroundColor: primary,
        minimumSize: const Size(0, 48),
        side: BorderSide(color: border),
        textStyle: GoogleFonts.cairo(fontSize: 14, fontWeight: FontWeight.w800),
        shape: rounded10,
      ),
    ),
    textButtonTheme: TextButtonThemeData(
      style: TextButton.styleFrom(
        foregroundColor: primary,
        textStyle: GoogleFonts.cairo(fontSize: 13, fontWeight: FontWeight.w800),
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: surfaceRaised,
      hintStyle: TextStyle(color: muted),
      labelStyle: TextStyle(color: muted),
      prefixIconColor: primary,
      suffixIconColor: muted,
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 15),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(10),
        borderSide: BorderSide.none,
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(10),
        borderSide: BorderSide(color: border),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(10),
        borderSide: BorderSide(color: primary, width: 1.5),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(10),
        borderSide: BorderSide(color: colors.error),
      ),
      focusedErrorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(10),
        borderSide: BorderSide(color: colors.error, width: 1.5),
      ),
    ),
    chipTheme: base.chipTheme.copyWith(
      backgroundColor: surfaceMuted,
      selectedColor: dark ? AppDarkColors.softBlue : AppColors.softBlue,
      side: BorderSide(color: border),
      labelStyle: GoogleFonts.cairo(
        color: text,
        fontSize: 12,
        fontWeight: FontWeight.w700,
      ),
      shape: rounded10,
    ),
    navigationBarTheme: NavigationBarThemeData(
      height: 72,
      backgroundColor: surface,
      surfaceTintColor: Colors.transparent,
      indicatorColor: surfaceMuted,
      labelTextStyle: WidgetStatePropertyAll(
        GoogleFonts.cairo(
          color: text,
          fontSize: 11,
          fontWeight: FontWeight.w800,
        ),
      ),
      iconTheme: WidgetStatePropertyAll(IconThemeData(color: primary)),
    ),
    snackBarTheme: SnackBarThemeData(
      backgroundColor: dark ? const Color(0xFFEAF5F2) : AppColors.text,
      contentTextStyle: GoogleFonts.cairo(
        color: dark ? const Color(0xFF071312) : Colors.white,
      ),
      behavior: SnackBarBehavior.floating,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
    ),
    bottomSheetTheme: BottomSheetThemeData(
      backgroundColor: surface,
      surfaceTintColor: Colors.transparent,
      modalBackgroundColor: surface,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(14)),
      ),
    ),
    dialogTheme: DialogThemeData(
      backgroundColor: surface,
      surfaceTintColor: Colors.transparent,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
    ),
    switchTheme: SwitchThemeData(
      thumbColor: WidgetStateProperty.resolveWith(
        (states) => states.contains(WidgetState.selected) ? primary : muted,
      ),
      trackColor: WidgetStateProperty.resolveWith(
        (states) => states.contains(WidgetState.selected)
            ? primary.withValues(alpha: .32)
            : surfaceMuted,
      ),
    ),
  );
}
