import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../widgets/app_scaffold.dart';

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) => AppScaffold(
    child: ListView(
      padding: const EdgeInsets.fromLTRB(18, 14, 18, 24),
      children: [
        _HeroCard(onStart: () => context.go('/register')),
        const SizedBox(height: 18),
        const Text(
          'ابحث عن طبيبك',
          style: TextStyle(fontSize: 20, fontWeight: FontWeight.w800),
        ),
        const SizedBox(height: 5),
        const Text(
          'اختر التفاصيل المناسبة وسنعرض لك أفضل النتائج.',
          style: TextStyle(color: Color(0xFF78908D)),
        ),
        const SizedBox(height: 12),
        _SearchCard(onSearch: () => context.go('/search')),
        const SizedBox(height: 22),
        const _SectionTitle(
          title: 'الخدمات الأكثر استخداماً',
          action: 'عرض الكل',
        ),
        const SizedBox(height: 11),
        const Row(
          children: [
            Expanded(
              child: _ServiceCard(
                icon: Icons.medical_services_outlined,
                title: 'الأطباء',
                color: Color(0xFF147D72),
                background: Color(0xFFE4F5F1),
              ),
            ),
            SizedBox(width: 10),
            Expanded(
              child: _ServiceCard(
                icon: Icons.calendar_month_outlined,
                title: 'حجز دور',
                color: Color(0xFFB16A2B),
                background: Color(0xFFFFF0DF),
              ),
            ),
            SizedBox(width: 10),
            Expanded(
              child: _ServiceCard(
                icon: Icons.location_on_outlined,
                title: 'قريب منك',
                color: Color(0xFF4C69B3),
                background: Color(0xFFEAF0FF),
              ),
            ),
          ],
        ),
        const SizedBox(height: 22),
        const _SectionTitle(title: 'لماذا عيادتي؟'),
        const SizedBox(height: 10),
        const _BenefitTile(
          icon: Icons.verified_user_outlined,
          title: 'حساب محمي',
          text: 'بياناتك محفوظة وتسجيل الدخول آمن.',
        ),
        const SizedBox(height: 9),
        const _BenefitTile(
          icon: Icons.schedule_outlined,
          title: 'وقتك أهم',
          text: 'احجز دورك مسبقاً وتجنب الانتظار الطويل.',
        ),
      ],
    ),
  );
}

class _HeroCard extends StatelessWidget {
  const _HeroCard({required this.onStart});
  final VoidCallback onStart;
  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(22),
    decoration: BoxDecoration(
      gradient: const LinearGradient(
        colors: [Color(0xFF126E65), Color(0xFF26988D)],
        begin: Alignment.topRight,
        end: Alignment.bottomLeft,
      ),
      borderRadius: BorderRadius.circular(24),
    ),
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
          decoration: BoxDecoration(
            color: Colors.white.withValues(alpha: .15),
            borderRadius: BorderRadius.circular(30),
          ),
          child: const Text(
            'دليل طبي وحجز إلكتروني',
            style: TextStyle(
              color: Colors.white,
              fontSize: 12,
              fontWeight: FontWeight.w700,
            ),
          ),
        ),
        const SizedBox(height: 18),
        const Text(
          'طبيبك المناسب،\nأقرب إليك.',
          style: TextStyle(
            color: Colors.white,
            fontSize: 30,
            height: 1.18,
            fontWeight: FontWeight.w900,
          ),
        ),
        const SizedBox(height: 10),
        const Text(
          'اعثر على الطبيب واحجز دورك بخطوات واضحة وسريعة.',
          style: TextStyle(color: Color(0xFFD8F1EE), height: 1.6),
        ),
        const SizedBox(height: 16),
        FilledButton.icon(
          style: FilledButton.styleFrom(
            backgroundColor: Colors.white,
            foregroundColor: const Color(0xFF126E65),
            minimumSize: const Size(138, 46),
          ),
          onPressed: onStart,
          icon: const Icon(Icons.arrow_back),
          label: const Text('ابدأ الآن'),
        ),
      ],
    ),
  );
}

class _SearchCard extends StatelessWidget {
  const _SearchCard({required this.onSearch});
  final VoidCallback onSearch;
  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(12),
    decoration: BoxDecoration(
      color: Colors.white,
      borderRadius: BorderRadius.circular(18),
      boxShadow: const [
        BoxShadow(
          color: Color(0x100B514A),
          blurRadius: 22,
          offset: Offset(0, 8),
        ),
      ],
    ),
    child: Column(
      children: [
        const _SearchField(
          icon: Icons.location_on_outlined,
          text: 'اختر المحافظة',
        ),
        const Divider(height: 1),
        const _SearchField(
          icon: Icons.medical_services_outlined,
          text: 'اختر الاختصاص',
        ),
        const SizedBox(height: 10),
        FilledButton.icon(
          onPressed: onSearch,
          icon: const Icon(Icons.search),
          label: const Text('البحث عن طبيب'),
        ),
      ],
    ),
  );
}

class _SearchField extends StatelessWidget {
  const _SearchField({required this.icon, required this.text});
  final IconData icon;
  final String text;
  @override
  Widget build(BuildContext context) => ListTile(
    contentPadding: const EdgeInsets.symmetric(horizontal: 3),
    leading: Icon(icon, color: const Color(0xFF147D72)),
    title: Text(text, style: const TextStyle(color: Color(0xFF718985))),
    trailing: const Icon(Icons.keyboard_arrow_down),
  );
}

class _SectionTitle extends StatelessWidget {
  const _SectionTitle({required this.title, this.action});
  final String title;
  final String? action;
  @override
  Widget build(BuildContext context) => Row(
    children: [
      Expanded(
        child: Text(
          title,
          style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w800),
        ),
      ),
      if (action != null)
        Text(
          action!,
          style: const TextStyle(
            color: Color(0xFF147D72),
            fontSize: 13,
            fontWeight: FontWeight.w700,
          ),
        ),
    ],
  );
}

class _ServiceCard extends StatelessWidget {
  const _ServiceCard({
    required this.icon,
    required this.title,
    required this.color,
    required this.background,
  });
  final IconData icon;
  final String title;
  final Color color, background;
  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.symmetric(vertical: 15, horizontal: 7),
    decoration: BoxDecoration(
      color: Colors.white,
      border: Border.all(color: const Color(0xFFE5EFED)),
      borderRadius: BorderRadius.circular(16),
    ),
    child: Column(
      children: [
        Container(
          width: 43,
          height: 43,
          decoration: BoxDecoration(
            color: background,
            borderRadius: BorderRadius.circular(13),
          ),
          child: Icon(icon, color: color),
        ),
        const SizedBox(height: 9),
        Text(
          title,
          style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w700),
        ),
      ],
    ),
  );
}

class _BenefitTile extends StatelessWidget {
  const _BenefitTile({
    required this.icon,
    required this.title,
    required this.text,
  });
  final IconData icon;
  final String title, text;
  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(14),
    decoration: BoxDecoration(
      color: Colors.white,
      border: Border.all(color: const Color(0xFFE5EFED)),
      borderRadius: BorderRadius.circular(16),
    ),
    child: Row(
      children: [
        Container(
          width: 46,
          height: 46,
          decoration: BoxDecoration(
            color: const Color(0xFFE4F5F1),
            borderRadius: BorderRadius.circular(14),
          ),
          child: Icon(icon, color: const Color(0xFF147D72)),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(title, style: const TextStyle(fontWeight: FontWeight.w800)),
              const SizedBox(height: 4),
              Text(
                text,
                style: const TextStyle(color: Color(0xFF78908D), fontSize: 13),
              ),
            ],
          ),
        ),
      ],
    ),
  );
}
