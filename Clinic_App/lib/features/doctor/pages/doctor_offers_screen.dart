import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../../core/app_snack_bar.dart';
import '../../../core/app_theme.dart';
import '../../auth/auth_controller.dart';
import '../models/doctor_models.dart';
import '../services/doctor_service.dart';
import '../widgets/doctor_scaffold.dart';

class DoctorOffersScreen extends StatefulWidget {
  const DoctorOffersScreen({super.key});

  @override
  State<DoctorOffersScreen> createState() => _DoctorOffersScreenState();
}

class _DoctorOffersScreenState extends State<DoctorOffersScreen> {
  late final DoctorService _service;
  List<DoctorOfferManage> _items = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _service = DoctorService(context.read<AuthController>().api);
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      _items = await _service.getOffers();
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _deleteOffer(DoctorOfferManage item) async {
    await _service.deleteOffer(item.id);
    await _load();
    if (mounted) showAppSnackBar(context, 'تم حذف العرض.');
  }

  @override
  Widget build(BuildContext context) => DoctorScaffold(
    title: 'العروض',
    child: Stack(
      children: [
        RefreshIndicator(
          onRefresh: _load,
          child: _loading
              ? const Center(child: CircularProgressIndicator())
              : _items.isEmpty
              ? const DoctorEmptyState(
                  icon: Icons.local_offer_outlined,
                  message: 'لا توجد عروض حالياً.',
                )
              : ListView.builder(
                  padding: const EdgeInsets.fromLTRB(16, 14, 16, 86),
                  itemCount: _items.length,
                  itemBuilder: (context, index) => _OfferCard(
                    item: _items[index],
                    onEdit: () async {
                      await context.push('/doctor/offers/form', extra: _items[index]);
                      await _load();
                    },
                    onDelete: () => _deleteOffer(_items[index]),
                  ),
                ),
        ),
        PositionedDirectional(
          start: 18,
          bottom: 16,
          child: FloatingActionButton(
            heroTag: 'doctor-add-offer',
            backgroundColor: AppColors.primary,
            foregroundColor: Colors.white,
            tooltip: 'إضافة عرض',
            onPressed: () async {
              await context.push('/doctor/offers/form');
              await _load();
            },
            child: const Icon(Icons.add_rounded),
          ),
        ),
      ],
    ),
  );
}

class _OfferCard extends StatelessWidget {
  const _OfferCard({
    required this.item,
    required this.onEdit,
    required this.onDelete,
  });

  final DoctorOfferManage item;
  final VoidCallback onEdit;
  final VoidCallback onDelete;

  @override
  Widget build(BuildContext context) {
    final visible = item.isCurrentlyVisible;
    final expired = item.remainingDays <= 0 || !visible;
    final accent = visible ? AppColors.primary : AppColors.muted;
    final bg = visible ? const Color(0xFFEAF7F5) : const Color(0xFFF2F4F4);

    return Container(
      margin: const EdgeInsets.only(bottom: 14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(22),
        border: Border.all(
          color: visible ? const Color(0xFFDDE9E7) : const Color(0xFFE1E5E4),
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(.035),
            blurRadius: 16,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Column(
        children: [
          Container(
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              gradient: LinearGradient(
                colors: visible
                    ? const [Color(0xFF0F7F73), Color(0xFF0D625C)]
                    : const [Color(0xFF7B8583), Color(0xFF626B69)],
                begin: Alignment.topRight,
                end: Alignment.bottomLeft,
              ),
              borderRadius: const BorderRadius.vertical(
                top: Radius.circular(22),
              ),
            ),
            child: Row(
              children: [
                Container(
                  width: 58,
                  height: 58,
                  decoration: BoxDecoration(
                    color: Colors.white.withOpacity(.16),
                    borderRadius: BorderRadius.circular(18),
                  ),
                  child: Icon(
                    visible
                        ? Icons.local_offer_rounded
                        : Icons.visibility_off_rounded,
                    color: Colors.white,
                    size: 31,
                  ),
                ),
                const SizedBox(width: 12),

                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        item.title,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 17,
                          fontWeight: FontWeight.w900,
                        ),
                      ),
                      const SizedBox(height: 5),
                      Text(
                        item.description?.isNotEmpty == true
                            ? item.description!
                            : item.offerTypeName,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: TextStyle(
                          color: Colors.white.withOpacity(.86),
                          fontSize: 12.5,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ],
                  ),
                ),

                const SizedBox(width: 8),

                _OfferStatusBadge(
                  text: visible ? 'ظاهر' : 'مخفي',
                  active: visible,
                ),
              ],
            ),
          ),

          Padding(
            padding: const EdgeInsets.all(14),
            child: Column(
              children: [
                Row(
                  children: [
                    Expanded(
                      child: _BigOfferBox(
                        title: 'قيمة العرض',
                        value: _offerValueText(item),
                        icon: Icons.percent_rounded,
                        color: accent,
                        bg: bg,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Expanded(
                      child: _BigOfferBox(
                        title: 'السعر الأصلي',
                        value: item.originalPrice == null? 'غير محدد': '${item.originalPrice!.toStringAsFixed(0)} د.ع',
                        icon: Icons.payments_outlined,
                        color: AppColors.primary,
                        bg: const Color(0xFFEAF7F5),
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 10),

                Row(
                  children: [
                    Expanded(
                      child: _MiniInfoBox(
                        icon: Icons.calendar_month_rounded,
                        title: 'بداية العرض',
                        value: DateFormat('yyyy/MM/dd').format(item.startsAt),
                      ),
                    ),
                    const SizedBox(width: 8),
                    Expanded(
                      child: _MiniInfoBox(
                        icon: Icons.event_busy_rounded,
                        title: 'نهاية العرض',
                        value: DateFormat('yyyy/MM/dd').format(item.endsAt),
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 10),

                Row(
                  children: [
                    Expanded(
                      child: _OfferInfoLine(
                        icon: Icons.local_hospital_rounded,
                        text: item.clinicName == null
    ? 'يشمل كل العيادات'
    : item.clinicName!,),
                    ),
                    const SizedBox(width: 8),
                    Expanded(
                      child: _OfferInfoLine(
                        icon: expired
                            ? Icons.timer_off_rounded
                            : Icons.timelapse_rounded,
                        text: expired
                            ? 'العرض منتهي'
                            : 'متبقي ${item.remainingDays} يوم',
                        color: expired ? AppColors.muted : AppColors.primary,
                      ),
                    ),
                  ],
                ),

                // if (item.terms?.isNotEmpty == true) ...[
                //   const SizedBox(height: 10),
                //   _TermsBox(text: item.terms!),
                // ],

                const SizedBox(height: 14),

                Row(
                  children: [
                    Expanded(
                      child: OutlinedButton.icon(
                        onPressed: onEdit,
                        icon: const Icon(Icons.edit_outlined, size: 18),
                        label: const Text(
                          'تعديل',
                          style: TextStyle(fontWeight: FontWeight.w900),
                        ),
                        style: OutlinedButton.styleFrom(
                          foregroundColor: AppColors.primary,
                          side: const BorderSide(color: Color(0xFFDDE9E7)),
                          padding: const EdgeInsets.symmetric(vertical: 12),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(13),
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(width: 8),
                    Expanded(
                      child: OutlinedButton.icon(
                        onPressed: () =>
                            showAppSnackBar(context, 'اضغط مطولاً للحذف.'),
                        onLongPress: onDelete,
                        icon: const Icon(Icons.delete_outline, size: 18),
                        label: const Text(
                          'حذف',
                          style: TextStyle(fontWeight: FontWeight.w900),
                        ),
                        style: OutlinedButton.styleFrom(
                          foregroundColor: AppColors.danger,
                          side: BorderSide(
                            color: AppColors.danger.withOpacity(.22),
                          ),
                          padding: const EdgeInsets.symmetric(vertical: 12),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(13),
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  String _offerValueText(DoctorOfferManage item) {
    if (item.discountPercent != null && item.discountPercent! > 0) {
      return '${item.discountPercent!.toStringAsFixed(0)}% خصم';
    }

    if (item.offerPrice != null && item.offerPrice! > 0) {
      return '${item.offerPrice!.toStringAsFixed(0)} د.ع';
    }

    return item.offerTypeName;
  }
}

class _OfferStatusBadge extends StatelessWidget {
  const _OfferStatusBadge({
    required this.text,
    required this.active,
  });

  final String text;
  final bool active;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 7),
      decoration: BoxDecoration(
        color: Colors.white.withOpacity(.16),
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: Colors.white.withOpacity(.24)),
      ),
      child: Text(
        text,
        style: const TextStyle(
          color: Colors.white,
          fontSize: 11.5,
          fontWeight: FontWeight.w900,
        ),
      ),
    );
  }
}

class _BigOfferBox extends StatelessWidget {
  const _BigOfferBox({
    required this.title,
    required this.value,
    required this.icon,
    required this.color,
    required this.bg,
  });

  final String title;
  final String value;
  final IconData icon;
  final Color color;
  final Color bg;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(11),
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(15),
        border: Border.all(color: color.withOpacity(.14)),
      ),
      child: Row(
        children: [
          Icon(icon, color: color, size: 22),
          const SizedBox(width: 8),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: TextStyle(
                    fontSize: 11,
                    color: Colors.grey.shade700,
                    fontWeight: FontWeight.w700,
                  ),
                ),
                const SizedBox(height: 3),
                Text(
                  value,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    fontSize: 14,
                    color: color,
                    fontWeight: FontWeight.w900,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _MiniInfoBox extends StatelessWidget {
  const _MiniInfoBox({
    required this.icon,
    required this.title,
    required this.value,
  });

  final IconData icon;
  final String title;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(11),
      decoration: BoxDecoration(
        color: const Color(0xFFF7FAFA),
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: const Color(0xFFE3ECEA)),
      ),
      child: Row(
        children: [
          Icon(icon, size: 18, color: AppColors.primary),
          const SizedBox(width: 7),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: TextStyle(fontSize: 11, color: Colors.grey.shade600),
                ),
                const SizedBox(height: 2),
                Text(
                  value,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontSize: 12.5,
                    fontWeight: FontWeight.w900,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _OfferInfoLine extends StatelessWidget {
  const _OfferInfoLine({
    required this.icon,
    required this.text,
    this.color = AppColors.primary,
  });

  final IconData icon;
  final String text;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 9),
      decoration: BoxDecoration(
        color: color.withOpacity(.08),
        borderRadius: BorderRadius.circular(13),
      ),
      child: Row(
        children: [
          Icon(icon, size: 17, color: color),
          const SizedBox(width: 6),
          Expanded(
            child: Text(
              text,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: TextStyle(
                color: color,
                fontSize: 12,
                fontWeight: FontWeight.w900,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _TermsBox extends StatelessWidget {
  const _TermsBox({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(10),
      decoration: BoxDecoration(
        color: const Color(0xFFFFF8E7),
        borderRadius: BorderRadius.circular(13),
        border: Border.all(color: const Color(0xFFE8CF83)),
      ),
      child: Row(
        children: [
          const Icon(
            Icons.info_outline_rounded,
            size: 18,
            color: Color(0xFFB7791F),
          ),
          const SizedBox(width: 7),
          Expanded(
            child: Text(
              text,
              style: const TextStyle(
                color: Color(0xFF8A5A10),
                fontSize: 12,
                fontWeight: FontWeight.w700,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
