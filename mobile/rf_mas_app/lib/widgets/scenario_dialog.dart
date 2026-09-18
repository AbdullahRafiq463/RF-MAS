import 'package:flutter/material.dart';
import '../theme/app_colors.dart';

class ScenarioDialog {
  static Future<void> show(
    BuildContext context,
    String deviceId,
    Function(String scenario, double? customVal) onSelect,
  ) async {
    final scenarios = [
      {'name': 'NORMAL', 'label': 'Normal Nominal Operation', 'desc': 'Restore normal telemetry stream and clear injected anomalies.'},
      {'name': 'HIGH_TEMPERATURE', 'label': 'High Thermal Runaway (>75°C)', 'desc': 'Triggers temperature warning & critical safety alarms.'},
      {'name': 'LOW_SIGNAL', 'label': 'RF Signal Fade (<-70 dBm)', 'desc': 'Triggers signal power threshold drop alarms.'},
      {'name': 'LOW_VOLTAGE', 'label': 'Rail Voltage Sag (<10V)', 'desc': 'Triggers supply voltage out-of-range alarm.'},
      {'name': 'DISCONNECT', 'label': 'Device Network Disconnection', 'desc': 'Simulates link break, status transitions to OFFLINE.'},
      {'name': 'COMMUNICATION_TIMEOUT', 'label': 'Communication Timeout', 'desc': 'Simulates device packet loss and response freeze.'},
    ];

    await showModalBottomSheet(
      context: context,
      backgroundColor: AppColors.darkSurface,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (ctx) => Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 20),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'DEMO SCENARIOS: $deviceId',
                  style: const TextStyle(
                    color: AppColors.primaryTeal,
                    fontWeight: FontWeight.bold,
                    letterSpacing: 0.5,
                  ),
                ),
                IconButton(
                  icon: const Icon(Icons.close, size: 20),
                  onPressed: () => Navigator.pop(ctx),
                ),
              ],
            ),
            const Text(
              'Inject simulated hardware conditions to demonstrate real-time alerts & automation tests:',
              style: TextStyle(color: Colors.white54, fontSize: 12),
            ),
            const SizedBox(height: 12),
            Flexible(
              child: ListView.builder(
                shrinkWrap: true,
                itemCount: scenarios.length,
                itemBuilder: (context, idx) {
                  final s = scenarios[idx];
                  return ListTile(
                    contentPadding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                    title: Text(s['label']!, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w600)),
                    subtitle: Text(s['desc']!, style: const TextStyle(fontSize: 11, color: Colors.white38)),
                    trailing: const Icon(Icons.play_arrow, color: AppColors.primaryTeal, size: 20),
                    onTap: () {
                      Navigator.pop(ctx);
                      onSelect(s['name']!, null);
                    },
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}
