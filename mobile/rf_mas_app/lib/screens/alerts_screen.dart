import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../models/alert.dart';
import '../providers/alerts_provider.dart';
import '../theme/app_colors.dart';
import '../widgets/error_view.dart';

class AlertsScreen extends StatelessWidget {
  const AlertsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Safety Alarms & Alerts'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Refresh Alarms',
            onPressed: () => context.read<AlertsProvider>().loadAlerts(),
          ),
        ],
      ),
      body: Consumer<AlertsProvider>(
        builder: (context, provider, _) {
          return Column(
            children: [
              // Filter Chips Row
              _buildFilterHeader(context, provider),

              // Alerts List
              Expanded(
                child: _buildAlertsContent(context, provider),
              ),
            ],
          );
        },
      ),
    );
  }

  Widget _buildFilterHeader(BuildContext context, AlertsProvider provider) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      color: AppColors.darkSurface,
      child: SingleChildScrollView(
        scrollDirection: Axis.horizontal,
        child: Row(
          children: [
            _severityFilterChip(provider, 'ALL', 'All'),
            const SizedBox(width: 8),
            _severityFilterChip(provider, 'CRITICAL', 'Critical', color: AppColors.criticalRed),
            const SizedBox(width: 8),
            _severityFilterChip(provider, 'WARNING', 'Warning', color: AppColors.warningAmber),
            const SizedBox(width: 8),
            _severityFilterChip(provider, 'INFO', 'Info', color: AppColors.primaryBlue),
            const SizedBox(width: 12),
            FilterChip(
              label: const Text('Active Only', style: TextStyle(fontSize: 11)),
              selected: provider.activeOnlyFilter == true,
              selectedColor: AppColors.primaryTeal.withValues(alpha: 0.2),
              checkmarkColor: AppColors.primaryTeal,
              onSelected: (val) {
                provider.setActiveFilter(val ? true : null);
              },
            ),
          ],
        ),
      ),
    );
  }

  Widget _severityFilterChip(AlertsProvider provider, String key, String label, {Color? color}) {
    final isSelected = provider.selectedSeverity == key;
    return ChoiceChip(
      label: Text(label, style: TextStyle(fontSize: 11, color: isSelected ? Colors.black : Colors.white70, fontWeight: FontWeight.bold)),
      selected: isSelected,
      selectedColor: color ?? AppColors.primaryTeal,
      backgroundColor: AppColors.darkCard,
      onSelected: (_) => provider.setSeverityFilter(key),
    );
  }

  Widget _buildAlertsContent(BuildContext context, AlertsProvider provider) {
    if (provider.isLoading && provider.alerts.isEmpty) {
      return const Center(child: CircularProgressIndicator(color: AppColors.primaryTeal));
    }

    if (provider.errorMessage != null && provider.alerts.isEmpty) {
      return ErrorRetryView(
        message: provider.errorMessage!,
        onRetry: provider.loadAlerts,
      );
    }

    if (provider.alerts.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.shield_outlined, size: 48, color: AppColors.onlineGreen.withValues(alpha: 0.6)),
            const SizedBox(height: 12),
            const Text(
              'No Alerts In Selected Category',
              style: TextStyle(color: Colors.white70, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 4),
            const Text(
              'All monitored instrument parameters are nominal.',
              style: TextStyle(color: Colors.white38, fontSize: 12),
            ),
          ],
        ),
      );
    }

    return RefreshIndicator(
      color: AppColors.primaryTeal,
      onRefresh: provider.loadAlerts,
      child: ListView.builder(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
        itemCount: provider.alerts.length,
        itemBuilder: (context, idx) {
          final alert = provider.alerts[idx];
          return _buildAlertCard(context, alert, provider);
        },
      ),
    );
  }

  Widget _buildAlertCard(BuildContext context, AlertModel alert, AlertsProvider provider) {
    final color = AppColors.getSeverityColor(alert.severity);
    final timeStr = DateFormat('yyyy-MM-dd HH:mm:ss').format(alert.timestamp.toLocal());

    return Card(
      margin: const EdgeInsets.symmetric(vertical: 5),
      child: Padding(
        padding: const EdgeInsets.all(12.0),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: color.withValues(alpha: 0.15),
                shape: BoxShape.circle,
              ),
              child: Icon(
                alert.severity == 'CRITICAL' ? Icons.error_outline : (alert.severity == 'WARNING' ? Icons.warning_amber_rounded : Icons.info_outline),
                color: color,
                size: 20,
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Expanded(
                        child: Text(
                          '${alert.deviceId} • ${alert.alertType}',
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: TextStyle(
                            color: color,
                            fontSize: 13,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                      const SizedBox(width: 6),
                      Text(
                        timeStr,
                        style: const TextStyle(color: Colors.white38, fontSize: 10, fontFamily: 'monospace'),
                      ),
                    ],
                  ),
                  const SizedBox(height: 4),
                  Text(
                    alert.message,
                    style: const TextStyle(color: Colors.white, fontSize: 12),
                  ),
                  const SizedBox(height: 6),
                  Wrap(
                    spacing: 8,
                    runSpacing: 4,
                    crossAxisAlignment: WrapCrossAlignment.center,
                    alignment: WrapAlignment.spaceBetween,
                    children: [
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                        decoration: BoxDecoration(
                          color: alert.isAcknowledged ? AppColors.onlineGreen.withValues(alpha: 0.15) : AppColors.warningAmber.withValues(alpha: 0.15),
                          borderRadius: BorderRadius.circular(4),
                        ),
                        child: Text(
                          alert.isAcknowledged ? 'ACKNOWLEDGED' : 'ACTIVE / UNACKNOWLEDGED',
                          style: TextStyle(
                            color: alert.isAcknowledged ? AppColors.onlineGreen : AppColors.warningAmber,
                            fontSize: 9,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                      if (!alert.isAcknowledged)
                        TextButton.icon(
                          style: TextButton.styleFrom(
                            foregroundColor: AppColors.primaryTeal,
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                            visualDensity: VisualDensity.compact,
                          ),
                          icon: const Icon(Icons.check, size: 14),
                          label: const Text('Acknowledge', style: TextStyle(fontSize: 11)),
                          onPressed: () => provider.acknowledgeAlert(alert.id),
                        ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
