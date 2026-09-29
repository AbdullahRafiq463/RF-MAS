import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../models/automation_test.dart';
import '../providers/automation_provider.dart';
import '../theme/app_colors.dart';
import '../widgets/error_view.dart';

class AutomationScreen extends StatelessWidget {
  const AutomationScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Automated RF Test Suite'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Refresh Results',
            onPressed: () => context.read<AutomationProvider>().loadTestHistory(),
          ),
        ],
      ),
      body: Consumer<AutomationProvider>(
        builder: (context, provider, _) {
          return Column(
            children: [
              // Top Action & Metrics Card
              _buildControlBanner(context, provider),

              // Test Results List
              Expanded(
                child: _buildResultsList(context, provider),
              ),
            ],
          );
        },
      ),
    );
  }

  Widget _buildControlBanner(BuildContext context, AutomationProvider provider) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: const BoxDecoration(
        color: AppColors.darkSurface,
        border: Border(bottom: BorderSide(color: AppColors.darkBorder)),
      ),
      child: Column(
        children: [
          // Run All CTA Button
          SizedBox(
            width: double.infinity,
            height: 44,
            child: ElevatedButton.icon(
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.primaryTeal,
                foregroundColor: Colors.black,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              ),
              icon: provider.isRunningSuite
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2, color: Colors.black),
                    )
                  : const Icon(Icons.play_circle_fill, size: 20),
              label: Text(
                provider.isRunningSuite ? 'Executing Test Suite...' : 'Run All Automated Tests',
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
              ),
              onPressed: provider.isRunningSuite
                  ? null
                  : () async {
                      final res = await provider.runAllTests();
                      if (context.mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(
                            content: Text('Test suite completed. ${res.length} tests executed.'),
                            backgroundColor: AppColors.onlineGreen,
                          ),
                        );
                      }
                    },
            ),
          ),
          const SizedBox(height: 12),

          // Pass / Fail Counters
          Row(
            children: [
              Expanded(child: _statItem('Total Executed', '${provider.testHistory.length}', Colors.white70)),
              Expanded(child: _statItem('Passed', '${provider.passCount}', AppColors.onlineGreen)),
              Expanded(child: _statItem('Failed', '${provider.failCount}', AppColors.criticalRed)),
              Expanded(child: _statItem('Errors', '${provider.errorCount}', AppColors.warningAmber)),
            ],
          ),
        ],
      ),
    );
  }

  Widget _statItem(String label, String value, Color color) {
    return Column(
      children: [
        Text(value, style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: color)),
        const SizedBox(height: 2),
        Text(label, maxLines: 1, overflow: TextOverflow.ellipsis, style: const TextStyle(fontSize: 10, color: Colors.white38)),
      ],
    );
  }

  Widget _buildResultsList(BuildContext context, AutomationProvider provider) {
    if (provider.isLoading && provider.testHistory.isEmpty) {
      return const Center(child: CircularProgressIndicator(color: AppColors.primaryTeal));
    }

    if (provider.errorMessage != null && provider.testHistory.isEmpty) {
      return ErrorRetryView(
        message: provider.errorMessage!,
        onRetry: provider.loadTestHistory,
      );
    }

    if (provider.testHistory.isEmpty) {
      return const Center(
        child: Text(
          'No automated test runs on record.\nTap "Run All Automated Tests" above to start.',
          textAlign: TextAlign.center,
          style: TextStyle(color: Colors.white38, fontSize: 13),
        ),
      );
    }

    return RefreshIndicator(
      color: AppColors.primaryTeal,
      onRefresh: provider.loadTestHistory,
      child: ListView.builder(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
        itemCount: provider.testHistory.length,
        itemBuilder: (context, index) {
          final test = provider.testHistory[index];
          return _buildTestResultCard(test);
        },
      ),
    );
  }

  Widget _buildTestResultCard(AutomationTestResultModel test) {
    Color statusColor;
    IconData statusIcon;

    switch (test.status.toUpperCase()) {
      case 'PASS':
        statusColor = AppColors.onlineGreen;
        statusIcon = Icons.check_circle_outline;
        break;
      case 'FAIL':
        statusColor = AppColors.criticalRed;
        statusIcon = Icons.cancel_outlined;
        break;
      default:
        statusColor = AppColors.warningAmber;
        statusIcon = Icons.error_outline;
        break;
    }

    final timeStr = DateFormat('yyyy-MM-dd HH:mm:ss').format(test.executedAt.toLocal());

    return Card(
      margin: const EdgeInsets.symmetric(vertical: 5),
      child: Padding(
        padding: const EdgeInsets.all(12.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Top row: Test Name, Device, Status Badge
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Row(
                    children: [
                      Icon(statusIcon, color: statusColor, size: 18),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          '${test.deviceId} • ${test.testName}',
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 6),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                  decoration: BoxDecoration(
                    color: statusColor.withValues(alpha: 0.15),
                    borderRadius: BorderRadius.circular(4),
                    border: Border.all(color: statusColor.withValues(alpha: 0.5)),
                  ),
                  child: Text(
                    test.status,
                    style: TextStyle(color: statusColor, fontSize: 10, fontWeight: FontWeight.bold),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            const Divider(),
            const SizedBox(height: 6),

            // Expected vs Actual
            Row(
              children: [
                const Text('Expected: ', style: TextStyle(fontSize: 11, color: Colors.white38)),
                Expanded(
                  child: Text(
                    test.expectedValue,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(fontSize: 11, color: Colors.white70),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 2),
            Row(
              children: [
                const Text('Actual:     ', style: TextStyle(fontSize: 11, color: Colors.white38)),
                Expanded(
                  child: Text(
                    test.actualValue,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: TextStyle(fontSize: 11, color: statusColor, fontWeight: FontWeight.w600),
                  ),
                ),
              ],
            ),

            if (test.errorMessage != null && test.errorMessage!.isNotEmpty) ...[
              const SizedBox(height: 4),
              Text(
                'Failure Reason: ${test.errorMessage}',
                style: const TextStyle(color: AppColors.criticalRed, fontSize: 11),
              ),
            ],

            const SizedBox(height: 6),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(timeStr, style: const TextStyle(fontSize: 10, color: Colors.white38, fontFamily: 'monospace')),
                Text('${test.executionDurationMs} ms', style: const TextStyle(fontSize: 10, color: Colors.white38)),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
