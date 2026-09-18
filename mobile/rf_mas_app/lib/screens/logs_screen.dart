import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../models/event_log.dart';
import '../providers/logs_provider.dart';
import '../theme/app_colors.dart';
import '../widgets/error_view.dart';

class LogsScreen extends StatelessWidget {
  const LogsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Centralized Audit & Event Logs'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Refresh Logs',
            onPressed: () => context.read<LogsProvider>().loadLogs(),
          ),
        ],
      ),
      body: Consumer<LogsProvider>(
        builder: (context, provider, _) {
          return Column(
            children: [
              // Category Filter Bar
              _buildCategoryBar(provider),

              // Event Logs Stream List
              Expanded(
                child: _buildLogsList(context, provider),
              ),
            ],
          );
        },
      ),
    );
  }

  Widget _buildCategoryBar(LogsProvider provider) {
    final categories = ['ALL', 'COMMAND', 'ALERT', 'AUTOMATION', 'DEVICE', 'SYSTEM'];

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      color: AppColors.darkSurface,
      child: SingleChildScrollView(
        scrollDirection: Axis.horizontal,
        child: Row(
          children: categories.map((cat) {
            final isSelected = provider.selectedCategory == cat;
            return Padding(
              padding: const EdgeInsets.only(right: 6),
              child: ChoiceChip(
                label: Text(cat, style: TextStyle(fontSize: 11, color: isSelected ? Colors.black : Colors.white70, fontWeight: FontWeight.bold)),
                selected: isSelected,
                selectedColor: AppColors.primaryTeal,
                backgroundColor: AppColors.darkCard,
                onSelected: (_) => provider.setCategory(cat),
              ),
            );
          }).toList(),
        ),
      ),
    );
  }

  Widget _buildLogsList(BuildContext context, LogsProvider provider) {
    if (provider.isLoading && provider.logs.isEmpty) {
      return const Center(child: CircularProgressIndicator(color: AppColors.primaryTeal));
    }

    if (provider.errorMessage != null && provider.logs.isEmpty) {
      return ErrorRetryView(
        message: provider.errorMessage!,
        onRetry: provider.loadLogs,
      );
    }

    if (provider.logs.isEmpty) {
      return const Center(
        child: Text('No log entries match the selected filter.', style: TextStyle(color: Colors.white38)),
      );
    }

    return RefreshIndicator(
      color: AppColors.primaryTeal,
      onRefresh: provider.loadLogs,
      child: ListView.separated(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
        itemCount: provider.logs.length,
        separatorBuilder: (_, __) => const Divider(height: 1),
        itemBuilder: (context, index) {
          final log = provider.logs[index];
          return _buildLogTile(log);
        },
      ),
    );
  }

  Widget _buildLogTile(EventLogModel log) {
    final color = AppColors.getSeverityColor(log.severity);
    final timeStr = DateFormat('HH:mm:ss.SSS').format(log.timestamp.toLocal());
    final dateStr = DateFormat('yyyy-MM-dd').format(log.timestamp.toLocal());

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      color: AppColors.darkCard,
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Timestamp & Date
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                timeStr,
                style: const TextStyle(
                  color: Colors.white70,
                  fontSize: 11,
                  fontFamily: 'monospace',
                  fontWeight: FontWeight.bold,
                ),
              ),
              Text(
                dateStr,
                style: const TextStyle(color: Colors.white24, fontSize: 9, fontFamily: 'monospace'),
              ),
            ],
          ),
          const SizedBox(width: 12),

          // Category Badge
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
            decoration: BoxDecoration(
              color: AppColors.darkSurface,
              borderRadius: BorderRadius.circular(4),
              border: Border.all(color: AppColors.darkBorder),
            ),
            child: Text(
              log.category,
              style: const TextStyle(color: AppColors.primaryTeal, fontSize: 9, fontWeight: FontWeight.bold),
            ),
          ),
          const SizedBox(width: 10),

          // Event & Details
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      '${log.deviceId ?? 'SYSTEM'} • ${log.eventName}',
                      style: TextStyle(color: color, fontSize: 12, fontWeight: FontWeight.bold),
                    ),
                  ],
                ),
                const SizedBox(height: 2),
                Text(
                  log.details,
                  style: const TextStyle(color: Colors.white70, fontSize: 11),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
