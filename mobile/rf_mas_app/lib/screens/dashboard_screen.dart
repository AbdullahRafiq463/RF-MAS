import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../providers/dashboard_provider.dart';
import '../providers/alerts_provider.dart';
import '../theme/app_colors.dart';
import '../widgets/summary_card.dart';
import '../widgets/error_view.dart';
import 'settings_screen.dart';

class DashboardScreen extends StatelessWidget {
  const DashboardScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Row(
          children: [
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
              decoration: BoxDecoration(
                color: AppColors.primaryTeal.withValues(alpha: 0.15),
                borderRadius: BorderRadius.circular(6),
                border: Border.all(color: AppColors.primaryTeal.withValues(alpha: 0.5)),
              ),
              child: const Text(
                'RF-MAS',
                style: TextStyle(
                  color: AppColors.primaryTeal,
                  fontSize: 14,
                  fontWeight: FontWeight.w900,
                  letterSpacing: 1,
                ),
              ),
            ),
            const SizedBox(width: 10),
            const Text('System Monitor'),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.settings_outlined),
            tooltip: 'Settings',
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const SettingsScreen()),
              );
            },
          ),
        ],
      ),
      body: Consumer<DashboardProvider>(
        builder: (context, provider, _) {
          if (provider.isLoading && provider.summary == null) {
            return const Center(child: CircularProgressIndicator(color: AppColors.primaryTeal));
          }

          if (provider.hasError && provider.summary == null) {
            return ErrorRetryView(
              message: provider.errorMessage!,
              onRetry: provider.loadDashboard,
            );
          }

          final summary = provider.summary;
          if (summary == null) {
            return const Center(child: Text('No telemetry available'));
          }

          return RefreshIndicator(
            color: AppColors.primaryTeal,
            onRefresh: provider.loadDashboard,
            child: ListView(
              padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
              children: [
                // Simulation Mode Banner
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                  decoration: BoxDecoration(
                    color: AppColors.primaryBlue.withValues(alpha: 0.12),
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: AppColors.primaryBlue.withValues(alpha: 0.3)),
                  ),
                  child: const Row(
                    children: [
                      Icon(Icons.info_outline, color: AppColors.primaryBlue, size: 16),
                      SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          'DEMO / SIMULATION MODE • Real-time telemetry emulation active',
                          style: TextStyle(color: Colors.white70, fontSize: 11, fontWeight: FontWeight.w500),
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 14),

                // High-level Grid Metrics
                GridView.count(
                  crossAxisCount: 2,
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  crossAxisSpacing: 10,
                  mainAxisSpacing: 10,
                  childAspectRatio: 1.6,
                  children: [
                    SummaryCard(
                      title: 'Total Instruments',
                      value: '${summary.totalDevices}',
                      icon: Icons.developer_board,
                      color: AppColors.primaryTeal,
                      subtitle: '${summary.onlineDevices} Online • ${summary.warningDevices} Warning',
                    ),
                    SummaryCard(
                      title: 'Active Alarms',
                      value: '${summary.activeAlerts}',
                      icon: Icons.warning_amber_rounded,
                      color: summary.criticalAlerts > 0 ? AppColors.criticalRed : (summary.activeAlerts > 0 ? AppColors.warningAmber : AppColors.onlineGreen),
                      subtitle: '${summary.criticalAlerts} Critical • ${summary.warningAlerts} Warning',
                    ),
                    SummaryCard(
                      title: 'Avg Fleet Health',
                      value: '${summary.averageSystemHealth.toStringAsFixed(0)}%',
                      icon: Icons.health_and_safety_outlined,
                      color: summary.averageSystemHealth >= 80 ? AppColors.onlineGreen : AppColors.warningAmber,
                      subtitle: 'Weighted multi-factor score',
                    ),
                    SummaryCard(
                      title: 'Automated Tests',
                      value: '${summary.testsExecuted}',
                      icon: Icons.fact_check_outlined,
                      color: AppColors.powerBlue,
                      subtitle: '${summary.testsPassed} Pass • ${summary.testsFailed} Fail',
                    ),
                  ],
                ),
                const SizedBox(height: 18),

                // Active Alerts Section
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'ACTIVE ALERTS',
                      style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: Colors.white54, letterSpacing: 0.5),
                    ),
                    Text(
                      '${provider.activeAlerts.length} Open',
                      style: const TextStyle(fontSize: 11, color: AppColors.primaryTeal),
                    ),
                  ],
                ),
                const SizedBox(height: 8),
                if (provider.activeAlerts.isEmpty)
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: AppColors.darkCard,
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(color: AppColors.darkBorder),
                    ),
                    child: const Row(
                      children: [
                        Icon(Icons.check_circle_outline, color: AppColors.onlineGreen, size: 20),
                        SizedBox(width: 10),
                        Text(
                          'All parameters operating within safe thresholds.',
                          style: TextStyle(color: Colors.white70, fontSize: 12),
                        ),
                      ],
                    ),
                  )
                else
                  ...provider.activeAlerts.take(3).map((alert) {
                    final color = AppColors.getSeverityColor(alert.severity);
                    return Card(
                      margin: const EdgeInsets.only(bottom: 8),
                      child: ListTile(
                        leading: Icon(
                          alert.severity == 'CRITICAL' ? Icons.error : Icons.warning,
                          color: color,
                        ),
                        title: Text(
                          '${alert.deviceId} • ${alert.alertType}',
                          style: TextStyle(color: color, fontSize: 13, fontWeight: FontWeight.bold),
                        ),
                        subtitle: Text(
                          alert.message,
                          style: const TextStyle(fontSize: 11, color: Colors.white70),
                        ),
                        trailing: TextButton(
                          onPressed: () {
                            context.read<AlertsProvider>().acknowledgeAlert(alert.id);
                            provider.refreshSilently();
                          },
                          child: const Text('ACK', style: TextStyle(fontSize: 11, color: AppColors.primaryTeal)),
                        ),
                      ),
                    );
                  }),
                const SizedBox(height: 18),

                // Recent System Activity
                const Text(
                  'RECENT ACTIVITY & AUDIT LOGS',
                  style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: Colors.white54, letterSpacing: 0.5),
                ),
                const SizedBox(height: 8),
                Container(
                  decoration: BoxDecoration(
                    color: AppColors.darkCard,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: AppColors.darkBorder),
                  ),
                  child: ListView.separated(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    itemCount: provider.recentActivity.length,
                    separatorBuilder: (_, __) => const Divider(),
                    itemBuilder: (context, idx) {
                      final log = provider.recentActivity[idx];
                      final timeStr = DateFormat('HH:mm:ss').format(log.timestamp.toLocal());
                      return ListTile(
                        dense: true,
                        leading: Text(
                          timeStr,
                          style: const TextStyle(color: Colors.white38, fontSize: 11, fontFamily: 'monospace'),
                        ),
                        title: Text(
                          '${log.deviceId ?? 'SYSTEM'} • ${log.eventName}',
                          style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w600),
                        ),
                        subtitle: Text(
                          log.details,
                          style: const TextStyle(fontSize: 11, color: Colors.white54),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      );
                    },
                  ),
                ),
                const SizedBox(height: 24),
              ],
            ),
          );
        },
      ),
    );
  }
}
