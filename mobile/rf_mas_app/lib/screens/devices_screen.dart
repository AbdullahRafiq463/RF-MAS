import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../models/device.dart';
import '../providers/devices_provider.dart';
import '../theme/app_colors.dart';
import '../widgets/status_chip.dart';
import '../widgets/health_gauge.dart';
import '../widgets/scenario_dialog.dart';
import '../widgets/error_view.dart';
import 'device_detail_screen.dart';

class DevicesScreen extends StatelessWidget {
  const DevicesScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('RF Instruments & Devices'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Refresh Instruments',
            onPressed: () => context.read<DevicesProvider>().loadDevices(),
          ),
        ],
      ),
      body: Consumer<DevicesProvider>(
        builder: (context, provider, _) {
          if (provider.isLoading && provider.devices.isEmpty) {
            return const Center(child: CircularProgressIndicator(color: AppColors.primaryTeal));
          }

          if (provider.errorMessage != null && provider.devices.isEmpty) {
            return ErrorRetryView(
              message: provider.errorMessage!,
              onRetry: provider.loadDevices,
            );
          }

          return RefreshIndicator(
            color: AppColors.primaryTeal,
            onRefresh: provider.loadDevices,
            child: ListView.builder(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 10),
              itemCount: provider.devices.length,
              itemBuilder: (context, index) {
                final device = provider.devices[index];
                return _buildDeviceCard(context, device, provider);
              },
            ),
          );
        },
      ),
    );
  }

  Widget _buildDeviceCard(BuildContext context, DeviceModel device, DevicesProvider provider) {
    return Card(
      margin: const EdgeInsets.symmetric(vertical: 6, horizontal: 4),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (_) => DeviceDetailScreen(deviceId: device.id),
            ),
          );
        },
        child: Padding(
          padding: const EdgeInsets.all(14.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Header row: ID, Name, Status, Health
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Text(
                              device.id,
                              style: const TextStyle(
                                color: AppColors.primaryTeal,
                                fontSize: 15,
                                fontWeight: FontWeight.bold,
                                letterSpacing: 0.5,
                              ),
                            ),
                            const SizedBox(width: 8),
                            StatusChip(status: device.operationalStatus),
                          ],
                        ),
                        const SizedBox(height: 3),
                        Text(
                          device.name,
                          style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600, color: Colors.white),
                        ),
                        Text(
                          '${device.manufacturer} • ${device.model}',
                          style: const TextStyle(fontSize: 11, color: Colors.white38),
                        ),
                      ],
                    ),
                  ),
                  HealthGauge(percentage: device.healthPercentage, size: 52),
                ],
              ),
              const SizedBox(height: 12),
              const Divider(),
              const SizedBox(height: 8),

              // Live Telemetry Row
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  _metricItem('FREQUENCY', '${device.frequencyMHz.toStringAsFixed(2)} MHz', AppColors.frequencyGreen),
                  _metricItem('POWER', '${device.signalPowerDbm.toStringAsFixed(1)} dBm', AppColors.powerBlue),
                  _metricItem('TEMPERATURE', '${device.temperatureC.toStringAsFixed(1)} °C', AppColors.temperatureOrange),
                  _metricItem('VOLTAGE', '${device.voltageV.toStringAsFixed(2)} V', AppColors.voltageYellow),
                ],
              ),
              const SizedBox(height: 10),

              // Bottom Actions: Quick Demo Scenario & Details
              Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: [
                  OutlinedButton.icon(
                    style: OutlinedButton.styleFrom(
                      foregroundColor: AppColors.primaryTeal,
                      side: const BorderSide(color: AppColors.darkBorder),
                      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                      visualDensity: VisualDensity.compact,
                    ),
                    icon: const Icon(Icons.bug_report_outlined, size: 14),
                    label: const Text('Simulate Scenario', style: TextStyle(fontSize: 11)),
                    onPressed: () {
                      ScenarioDialog.show(context, device.id, (scen, customVal) {
                        provider.injectScenario(device.id, scen, customValue: customVal);
                      });
                    },
                  ),
                  const SizedBox(width: 8),
                  ElevatedButton.icon(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.darkSurface,
                      foregroundColor: Colors.white,
                      side: const BorderSide(color: AppColors.darkBorder),
                      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                      visualDensity: VisualDensity.compact,
                    ),
                    icon: const Icon(Icons.tune, size: 14),
                    label: const Text('Details & Control', style: TextStyle(fontSize: 11)),
                    onPressed: () {
                      Navigator.push(
                        context,
                        MaterialPageRoute(
                          builder: (_) => DeviceDetailScreen(deviceId: device.id),
                        ),
                      );
                    },
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _metricItem(String label, String val, Color color) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          label,
          style: const TextStyle(fontSize: 9, color: Colors.white38, fontWeight: FontWeight.w600),
        ),
        const SizedBox(height: 2),
        Text(
          val,
          style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: color),
        ),
      ],
    );
  }
}
