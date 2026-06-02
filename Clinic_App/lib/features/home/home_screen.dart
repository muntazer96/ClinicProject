import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../core/app_theme.dart';
import '../../widgets/app_scaffold.dart';

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) => AppScaffold(
    child: ListView(
      padding: const EdgeInsets.fromLTRB(18, 12, 18, 28),
      children: [
        const Text(
          'أهلاً بك في عيادتي',
          style: TextStyle(fontSize: 27, fontWeight: FontWeight.w900),
        ),
        const SizedBox(height: 3),
        const Text(
          'صحتك تبدأ بخطوة بسيطة',
          style: TextStyle(color: AppColors.muted, fontSize: 14),
        ),
        const SizedBox(height: 18),
        _SearchBar(onTap: () => context.go('/search')),
        const SizedBox(height: 28),
        _SectionTitle(
          title: 'الخدمات',
          action: 'عرض الكل',
          onAction: () => context.go('/search'),
        ),
        const SizedBox(height: 13),
        Row(
          children: [
            _ServiceCard(
              icon: Icons.medical_services_rounded,
              title: 'الأطباء',
              onTap: () => context.go('/search'),
            ),
            const SizedBox(width: 10),
            _ServiceCard(
              icon: Icons.event_available_rounded,
              title: 'حجز موعد',
              color: const Color(0xFF7659F6),
              onTap: () => context.go('/search'),
            ),
            const SizedBox(width: 10),
            _ServiceCard(
              icon: Icons.manage_search_rounded,
              title: 'متابعة حجز',
              color: const Color(0xFF48BFD9),
              onTap: () => context.go('/guest-booking'),
            ),
          ],
        ),
        const SizedBox(height: 28),
        _SectionTitle(
          title: 'احجز طبيبك بسهولة',
          action: 'ابدأ البحث',
          onAction: () => context.go('/search'),
        ),
        const SizedBox(height: 13),
        _BookingBanner(onTap: () => context.go('/search')),
        const SizedBox(height: 28),
        const _SectionTitle(title: 'لماذا عيادتي؟'),
        const SizedBox(height: 13),
        const Row(
          children: [
            Expanded(
              child: _FeatureCard(
                icon: Icons.verified_user_rounded,
                title: 'أطباء موثوقون',
                text: 'ملفات واضحة وتقييمات حقيقية',
              ),
            ),
            SizedBox(width: 10),
            Expanded(
              child: _FeatureCard(
                icon: Icons.schedule_rounded,
                title: 'حجز أسرع',
                text: 'اختر يومك واستلم رقم الدور',
              ),
            ),
          ],
        ),
      ],
    ),
  );
}

class _SearchBar extends StatelessWidget {
  const _SearchBar({required this.onTap});
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => InkWell(
    onTap: onTap,
    borderRadius: BorderRadius.circular(20),
    child: Container(
      height: 58,
      padding: const EdgeInsets.symmetric(horizontal: 16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        boxShadow: const [
          BoxShadow(
            color: Color(0x140F1F4B),
            blurRadius: 22,
            offset: Offset(0, 8),
          ),
        ],
      ),
      child: const Row(
        children: [
          Icon(Icons.search_rounded, color: AppColors.primary),
          SizedBox(width: 10),
          Expanded(
            child: Text(
              'ابحث عن طبيب أو اختصاص',
              style: TextStyle(color: AppColors.muted),
            ),
          ),
          Icon(Icons.tune_rounded, color: AppColors.text),
        ],
      ),
    ),
  );
}

class _SectionTitle extends StatelessWidget {
  const _SectionTitle({required this.title, this.action, this.onAction});
  final String title;
  final String? action;
  final VoidCallback? onAction;

  @override
  Widget build(BuildContext context) => Row(
    children: [
      Expanded(
        child: Text(
          title,
          style: const TextStyle(fontSize: 19, fontWeight: FontWeight.w900),
        ),
      ),
      if (action != null)
        TextButton(
          onPressed: onAction,
          child: Text(action!, style: const TextStyle(fontSize: 12)),
        ),
    ],
  );
}

class _ServiceCard extends StatelessWidget {
  const _ServiceCard({
    required this.icon,
    required this.title,
    required this.onTap,
    this.color = AppColors.primary,
  });

  final IconData icon;
  final String title;
  final VoidCallback onTap;
  final Color color;

  @override
  Widget build(BuildContext context) => Expanded(
    child: InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(19),
      child: Container(
        height: 112,
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 14),
        decoration: BoxDecoration(
          color: color,
          borderRadius: BorderRadius.circular(19),
          boxShadow: [
            BoxShadow(
              color: color.withValues(alpha: .22),
              blurRadius: 15,
              offset: const Offset(0, 7),
            ),
          ],
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(icon, color: Colors.white, size: 34),
            const SizedBox(height: 8),
            Text(
              title,
              textAlign: TextAlign.center,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 12,
                fontWeight: FontWeight.w800,
              ),
            ),
          ],
        ),
      ),
    ),
  );
}

class _BookingBanner extends StatelessWidget {
  const _BookingBanner({required this.onTap});
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(20),
    decoration: BoxDecoration(
      gradient: const LinearGradient(
        colors: [AppColors.primary, AppColors.primaryDark],
        begin: Alignment.topRight,
        end: Alignment.bottomLeft,
      ),
      borderRadius: BorderRadius.circular(25),
      boxShadow: const [
        BoxShadow(
          color: Color(0x334267F5),
          blurRadius: 20,
          offset: Offset(0, 10),
        ),
      ],
    ),
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          width: 47,
          height: 47,
          decoration: BoxDecoration(
            color: Colors.white.withValues(alpha: .17),
            borderRadius: BorderRadius.circular(15),
          ),
          child: const Icon(
            Icons.health_and_safety_rounded,
            color: Colors.white,
          ),
        ),
        const SizedBox(height: 15),
        const Text(
          'اختصر وقت الانتظار',
          style: TextStyle(
            color: Colors.white,
            fontSize: 22,
            fontWeight: FontWeight.w900,
          ),
        ),
        const SizedBox(height: 5),
        const Text(
          'اختر الطبيب والعيادة واليوم المناسب، ثم استلم رقم دورك مباشرة.',
          style: TextStyle(color: Color(0xFFDDE5FF), height: 1.7),
        ),
        const SizedBox(height: 15),
        FilledButton.icon(
          style: FilledButton.styleFrom(
            minimumSize: const Size(150, 45),
            backgroundColor: Colors.white,
            foregroundColor: AppColors.primary,
          ),
          onPressed: onTap,
          icon: const Icon(Icons.arrow_back_rounded),
          label: const Text('احجز الآن'),
        ),
      ],
    ),
  );
}

class _FeatureCard extends StatelessWidget {
  const _FeatureCard({
    required this.icon,
    required this.title,
    required this.text,
  });

  final IconData icon;
  final String title;
  final String text;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(14),
    decoration: BoxDecoration(
      color: Colors.white,
      borderRadius: BorderRadius.circular(20),
      border: Border.all(color: AppColors.border),
    ),
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          width: 42,
          height: 42,
          decoration: BoxDecoration(
            color: AppColors.softBlue,
            borderRadius: BorderRadius.circular(14),
          ),
          child: Icon(icon, color: AppColors.primary),
        ),
        const SizedBox(height: 13),
        Text(title, style: const TextStyle(fontWeight: FontWeight.w900)),
        const SizedBox(height: 4),
        Text(
          text,
          style: const TextStyle(
            color: AppColors.muted,
            fontSize: 12,
            height: 1.6,
          ),
        ),
      ],
    ),
  );
}
