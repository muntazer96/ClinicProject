import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../core/app_theme.dart';
import '../models/directory_models.dart';
import '../specialization_icons.dart';

class SpecialtyCard extends StatelessWidget {
  const SpecialtyCard({
    super.key,
    required this.item,
    required this.index,
    this.compact = false,
  });

  final Specialization item;
  final int index;
  final bool compact;

  @override
  Widget build(BuildContext context) {
    final style = _SpecialtyStyle.at(index);
    final icon = specializationIconFor(item.iconName);

    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: () => context.go('/search?specialization=${item.id}'),
        borderRadius: BorderRadius.circular(8),
        child: Ink(
          padding: EdgeInsets.all(compact ? 10 : 12),
          decoration: BoxDecoration(
            color: context.appSurface,
            borderRadius: BorderRadius.circular(8),
            border: Border.all(color: context.appBorder),
            boxShadow: [
              BoxShadow(
                color: context.isDark
                    ? Colors.black.withValues(alpha: .22)
                    : const Color(0x17155E75),
                blurRadius: compact ? 12 : 18,
                offset: Offset(0, compact ? 5 : 8),
              ),
            ],
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Align(
                alignment: AlignmentDirectional.centerStart,
                child: Container(
                  width: compact ? 38 : 44,
                  height: compact ? 38 : 44,
                  decoration: BoxDecoration(
                    color: context.isDark ? style.darkTint : style.tint,
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Icon(icon, color: style.accent, size: compact ? 21 : 24),
                ),
              ),
              SizedBox(height: compact ? 8 : 12),
              Text(
                item.name,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                textAlign: TextAlign.start,
                style: TextStyle(
                  color: context.appText,
                  fontSize: compact ? 12 : 13,
                  height: 1.25,
                  fontWeight: FontWeight.w900,
                ),
              ),
              if (!compact) ...[
                const SizedBox(height: 6),
                Row(
                  children: [
                    Text(
                      'عرض الأطباء',
                      style: TextStyle(
                        color: style.accent,
                        fontSize: 11,
                        fontWeight: FontWeight.w800,
                      ),
                    ),
                    const SizedBox(width: 4),
                    Icon(
                      Icons.arrow_back_rounded,
                      color: style.accent,
                      size: 15,
                    ),
                  ],
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}

class _SpecialtyStyle {
  const _SpecialtyStyle({
    required this.accent,
    required this.tint,
    required this.darkTint,
  });

  final Color accent;
  final Color tint;
  final Color darkTint;

  static const _items = [
    _SpecialtyStyle(
      accent: AppColors.primary,
      tint: Color(0xFFE5F4F1),
      darkTint: Color(0xFF123A36),
    ),
    _SpecialtyStyle(
      accent: AppColors.primaryDark,
      tint: Color(0xFFE5F2F6),
      darkTint: Color(0xFF12363D),
    ),
    _SpecialtyStyle(
      accent: AppColors.accent,
      tint: Color(0xFFFFF4DB),
      darkTint: Color(0xFF3F2E12),
    ),
    _SpecialtyStyle(
      accent: AppColors.coral,
      tint: Color(0xFFFFECEB),
      darkTint: Color(0xFF462220),
    ),
  ];

  static _SpecialtyStyle at(int index) => _items[index % _items.length];
}
