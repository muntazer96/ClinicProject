import 'dart:async';

import 'package:flutter/material.dart';

import '../core/api_client.dart';
import '../core/app_theme.dart';
import 'app_logo.dart';

class OfflineGate extends StatefulWidget {
  const OfflineGate({super.key, required this.child});

  final Widget child;

  @override
  State<OfflineGate> createState() => _OfflineGateState();
}

class _OfflineGateState extends State<OfflineGate> {
  Timer? _connectivityTimer;
  Timer? _debounceTimer;
  bool _showOverlay = false;

  @override
  void initState() {
    super.initState();
    ApiClient.connectionAvailable.addListener(_onConnectionChanged);
    _connectivityTimer = Timer.periodic(const Duration(seconds: 15), (_) async {
      if (ApiClient.connectionAvailable.value == false) {
        await ApiClient.checkServerAvailability();
      }
    });
  }

  void _onConnectionChanged() {
    if (ApiClient.connectionAvailable.value == false) {
      _debounceTimer ??= Timer(const Duration(seconds: 4), () async {
        if (ApiClient.connectionAvailable.value == false) {
          await ApiClient.checkServerAvailability();
        }
        if (mounted && ApiClient.connectionAvailable.value == false) {
          setState(() => _showOverlay = true);
        }
        _debounceTimer = null;
      });
    } else {
      _debounceTimer?.cancel();
      _debounceTimer = null;
      if (_showOverlay) {
        setState(() => _showOverlay = false);
      }
    }
  }

  @override
  void dispose() {
    ApiClient.connectionAvailable.removeListener(_onConnectionChanged);
    _connectivityTimer?.cancel();
    _debounceTimer?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return ValueListenableBuilder<bool>(
      valueListenable: ApiClient.connectionAvailable,
      builder: (context, available, _) {
        if (available && _showOverlay) {
          WidgetsBinding.instance.addPostFrameCallback((_) {
            if (mounted) setState(() => _showOverlay = false);
          });
        }
        return Stack(
          children: [
            widget.child,
            if (!available && _showOverlay)
              const Positioned.fill(child: _OfflineScreen()),
          ],
        );
      },
    );
  }
}

class _OfflineScreen extends StatefulWidget {
  const _OfflineScreen();

  @override
  State<_OfflineScreen> createState() => _OfflineScreenState();
}

class _OfflineScreenState extends State<_OfflineScreen> {
  bool _checking = false;
  String? _message;

  Future<void> _retry() async {
    if (_checking) return;
    setState(() {
      _checking = true;
      _message = null;
    });
    final available = await ApiClient.checkServerAvailability();
    if (!mounted) return;
    setState(() {
      _checking = false;
      _message = available
          ? null
          : 'ما زال الاتصال غير متاح. تحقق من الانترنت أو تشغيل السيرفر.';
    });
  }

  @override
  Widget build(BuildContext context) {
    final text = context.appText;
    final muted = context.appMuted;
    return Material(
      color: Theme.of(context).scaffoldBackgroundColor,
      child: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            children: [
              const Spacer(),
              const AppLogo(size: 92),
              const SizedBox(height: 28),
              const Icon(
                Icons.wifi_off_rounded,
                color: AppColors.primary,
                size: 54,
              ),
              const SizedBox(height: 16),
              Text(
                'لا يوجد اتصال',
                textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                  fontWeight: FontWeight.w900,
                  color: text,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                'تعذر الوصول إلى الانترنت أو السيرفر. سنرجعك للتطبيق مباشرة عند نجاح إعادة المحاولة.',
                textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: muted,
                  height: 1.7,
                  fontWeight: FontWeight.w700,
                ),
              ),
              if (_message != null) ...[
                const SizedBox(height: 14),
                Text(
                  _message!,
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    color: AppColors.danger,
                    fontWeight: FontWeight.w800,
                  ),
                ),
              ],
              const SizedBox(height: 28),
              SizedBox(
                width: double.infinity,
                child: FilledButton.icon(
                  onPressed: _checking ? null : _retry,
                  icon: _checking
                      ? const SizedBox(
                          width: 20,
                          height: 20,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Icon(Icons.refresh_rounded),
                  label: Text(_checking ? 'جاري الفحص' : 'إعادة المحاولة'),
                ),
              ),
              const Spacer(),
            ],
          ),
        ),
      ),
    );
  }
}
