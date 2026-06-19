import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart';

import '../core/api_client.dart';
import '../core/app_theme.dart';
import '../core/app_version_service.dart';

class AppVersionGate extends StatefulWidget {
  const AppVersionGate({super.key, required this.child});

  final Widget child;

  @override
  State<AppVersionGate> createState() => _AppVersionGateState();
}

class _AppVersionGateState extends State<AppVersionGate> {
  late final AppVersionService _service;
  bool _checked = false;
  AppVersionInfo? _version;

  @override
  void initState() {
    super.initState();
    _service = AppVersionService(context.read<ApiClient>());
    WidgetsBinding.instance.addPostFrameCallback((_) => _checkVersion());
  }

  @override
  Widget build(BuildContext context) => Stack(
    children: [
      widget.child,
      if (_version != null)
        Positioned.fill(
          child: _UpdateOverlay(
            version: _version!,
            onLater: _version!.updateRequired
                ? null
                : () => setState(() => _version = null),
            onUpdate: () => _openUpdateUrl(_version!),
          ),
        ),
    ],
  );

  Future<void> _checkVersion() async {
    if (_checked || !mounted) return;
    _checked = true;

    try {
      final version = await _service.checkForUpdate();
      if (!mounted ||
          version == null ||
          (!version.updateAvailable && !version.updateRequired)) {
        return;
      }
      setState(() => _version = version);
    } catch (error) {
      debugPrint('App version check failed: $error');
      // Version checks should never block normal app usage when the server is unreachable.
    }
  }

  Future<void> _openUpdateUrl(AppVersionInfo version) async {
    final updateUrl = version.updateUrl?.trim();
    if (updateUrl == null || updateUrl.isEmpty) return;

    final uri = Uri.tryParse(updateUrl);
    if (uri == null) return;

    await launchUrl(uri, mode: LaunchMode.externalApplication);
  }
}

class _UpdateOverlay extends StatelessWidget {
  const _UpdateOverlay({
    required this.version,
    required this.onUpdate,
    this.onLater,
  });

  final AppVersionInfo version;
  final VoidCallback onUpdate;
  final VoidCallback? onLater;

  @override
  Widget build(BuildContext context) {
    final hasUpdateUrl = version.updateUrl?.trim().isNotEmpty == true;
    final muted = context.appMuted;

    return PopScope(
      canPop: !version.updateRequired,
      child: Material(
        color: Colors.black.withValues(alpha: .42),
        child: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 420),
              child: Card(
                margin: const EdgeInsets.all(18),
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(18, 20, 18, 16),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Icon(
                        version.updateRequired
                            ? Icons.system_update_alt_rounded
                            : Icons.new_releases_outlined,
                        color: version.updateRequired
                            ? AppColors.warning
                            : AppColors.primary,
                        size: 42,
                      ),
                      const SizedBox(height: 12),
                      Text(
                        version.title,
                        textAlign: TextAlign.center,
                        style: Theme.of(context).textTheme.titleLarge?.copyWith(
                          fontWeight: FontWeight.w900,
                        ),
                      ),
                      const SizedBox(height: 10),
                      Text(
                        version.message,
                        textAlign: TextAlign.center,
                        style: TextStyle(
                          color: muted,
                          fontWeight: FontWeight.w700,
                          height: 1.6,
                        ),
                      ),
                      const SizedBox(height: 14),
                      Container(
                        padding: const EdgeInsets.all(10),
                        decoration: BoxDecoration(
                          color: context.appSoftBlue,
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Text(
                          'نسختك الحالية ${version.currentVersion}+${version.currentBuildNumber}\nالنسخة المتوفرة ${version.latestVersion}+${version.latestBuildNumber}',
                          textAlign: TextAlign.center,
                          style: TextStyle(
                            color: muted,
                            fontWeight: FontWeight.w800,
                            height: 1.5,
                          ),
                        ),
                      ),
                      if (!hasUpdateUrl) ...[
                        const SizedBox(height: 10),
                        const Text(
                          'لم يتم تحديد رابط التحديث بعد.',
                          textAlign: TextAlign.center,
                          style: TextStyle(
                            color: AppColors.danger,
                            fontWeight: FontWeight.w800,
                          ),
                        ),
                      ],
                      const SizedBox(height: 16),
                      Row(
                        children: [
                          if (onLater != null) ...[
                            Expanded(
                              child: TextButton(
                                onPressed: onLater,
                                child: const Text('لاحقاً'),
                              ),
                            ),
                            const SizedBox(width: 8),
                          ],
                          Expanded(
                            child: FilledButton.icon(
                              onPressed: hasUpdateUrl ? onUpdate : null,
                              icon: const Icon(Icons.open_in_new_rounded),
                              label: const Text('تحديث التطبيق'),
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
