import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../models/device.dart';
import '../providers/devices_provider.dart';
import '../theme/app_colors.dart';
import '../widgets/status_chip.dart';
import '../widgets/health_gauge.dart';
import '../widgets/telemetry_chart.dart';
import '../widgets/command_dialog.dart';
import '../widgets/scenario_dialog.dart';

class DeviceDetailScreen extends StatefulWidget {
  final String deviceId;

  const DeviceDetailScreen({super.key, required this.deviceId});

  @override
  State<DeviceDetailScreen> createState() => _DeviceDetailScreenState();
}

class _DeviceDetailScreenState extends State<DeviceDetailScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<DevicesProvider>().selectDevice(widget.deviceId);
    });
  }

  void _showResultSnackbar(String message, bool success) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: success ? AppColors.onlineGreen : AppColors.criticalRed,
        duration: const Duration(seconds: 3),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          '${widget.deviceId} Control & Telemetry',
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.bug_report_outlined),
            tooltip: 'Inject Scenario',
            onPressed: () {
              final provider = context.read<DevicesProvider>();
              ScenarioDialog.show(context, widget.deviceId, (scen, val) {
                provider.injectScenario(widget.deviceId, scen, customValue: val);
              });
            },
          ),
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Refresh',
            onPressed: () => context.read<DevicesProvider>().selectDevice(widget.deviceId),
          ),
        ],
      ),
      body: Consumer<DevicesProvider>(
        builder: (context, provider, _) {
          final device = provider.selectedDevice;
          final telemetry = provider.selectedDeviceTelemetry;

          if (provider.isLoading && device == null) {
            return const Center(child: CircularProgressIndicator(color: AppColors.primaryTeal));
          }

          if (device == null) {
            return const Center(child: Text('Device not found'));
          }

          return SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Device Header Card
                _buildHeaderCard(device),
                const SizedBox(height: 14),

                // Device Controls Section
                _buildControlsSection(device, provider),
                const SizedBox(height: 18),

                // Telemetry Charts Section
                const Text(
                  'REAL-TIME TELEMETRY CHARTS',
                  style: TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.bold,
                    color: Colors.white54,
                    letterSpacing: 0.5,
                  ),
                ),
                const SizedBox(height: 8),

                // Temperature Chart
                TelemetryChart(
                  title: 'Thermal Headroom (Temperature)',
                  unit: '°C',
                  lineColor: AppColors.temperatureOrange,
                  telemetryData: telemetry,
                  valueSelector: (t) => t.temperatureC,
                ),
                const SizedBox(height: 12),

                // Signal Power Chart
                TelemetryChart(
                  title: 'RF Signal Power Output',
                  unit: 'dBm',
                  lineColor: AppColors.powerBlue,
                  telemetryData: telemetry,
                  valueSelector: (t) => t.signalPowerDbm,
                ),
                const SizedBox(height: 12),

                // Frequency Chart
                TelemetryChart(
                  title: 'Operating Center Frequency',
                  unit: 'MHz',
                  lineColor: AppColors.frequencyGreen,
                  telemetryData: telemetry,
                  valueSelector: (t) => t.frequencyMHz,
                ),
                const SizedBox(height: 18),

                // Configured Safety Thresholds
                _buildThresholdsCard(device),
                const SizedBox(height: 24),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildHeaderCard(DeviceModel device) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.darkCard,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.darkBorder),
      ),
      child: Column(
        children: [
          Row(
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
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(width: 8),
                        StatusChip(status: device.operationalStatus),
                      ],
                    ),
                    const SizedBox(height: 4),
                    Text(
                      device.name,
                      style: const TextStyle(fontSize: 14, fontWeight: FontWeight.bold, color: Colors.white),
                    ),
                    Text(
                      '${device.manufacturer} • ${device.model} (${device.deviceType})',
                      style: const TextStyle(fontSize: 11, color: Colors.white38),
                    ),
                    const SizedBox(height: 2),
                    Text(
                      'Address: ${device.ipAddress}:${device.port} • Firmware: ${device.firmwareVersion ?? "v2.0-sim"}',
                      style: const TextStyle(fontSize: 11, color: Colors.white54, fontFamily: 'monospace'),
                    ),
                  ],
                ),
              ),
              HealthGauge(percentage: device.healthPercentage, size: 62),
            ],
          ),
          const SizedBox(height: 12),
          const Divider(),
          const SizedBox(height: 8),
          Row(
            children: [
              Expanded(
                child: _specItem('Last Comm', DateFormat('HH:mm:ss').format(device.lastCommunicationTime.toLocal())),
              ),
              Expanded(
                child: _specItem('Voltage Rail', '${device.voltageV.toStringAsFixed(2)} V'),
              ),
              Expanded(
                child: _specItem('Current Draw', '${device.currentA.toStringAsFixed(2)} A'),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildControlsSection(DeviceModel device, DevicesProvider provider) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppColors.darkCard,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.darkBorder),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'INSTRUMENT CONTROLS & COMMANDS',
            style: TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.bold,
              color: Colors.white54,
              letterSpacing: 0.5,
            ),
          ),
          const SizedBox(height: 12),

          // Primary state command buttons: START, STOP, RESET
          Row(
            children: [
              Expanded(
                child: ElevatedButton.icon(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.onlineGreen.withValues(alpha: 0.2),
                    foregroundColor: AppColors.onlineGreen,
                    side: BorderSide(color: AppColors.onlineGreen.withValues(alpha: 0.6)),
                  ),
                  icon: const Icon(Icons.play_arrow, size: 16),
                  label: const Text('Start', style: TextStyle(fontWeight: FontWeight.bold)),
                  onPressed: () {
                    CommandDialog.showConfirmCommandDialog(
                      context,
                      device.id,
                      'START',
                      'Initiate instrument operations and telemetry capture.',
                      () async {
                        final res = await provider.startDevice(device.id);
                        _showResultSnackbar(res.message, res.success);
                      },
                    );
                  },
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: ElevatedButton.icon(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.criticalRed.withValues(alpha: 0.2),
                    foregroundColor: AppColors.criticalRed,
                    side: BorderSide(color: AppColors.criticalRed.withValues(alpha: 0.6)),
                  ),
                  icon: const Icon(Icons.stop, size: 16),
                  label: const Text('Stop', style: TextStyle(fontWeight: FontWeight.bold)),
                  onPressed: () {
                    CommandDialog.showConfirmCommandDialog(
                      context,
                      device.id,
                      'STOP',
                      'Halt RF output and transition instrument to OFFLINE mode.',
                      () async {
                        final res = await provider.stopDevice(device.id);
                        _showResultSnackbar(res.message, res.success);
                      },
                    );
                  },
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: ElevatedButton.icon(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.warningAmber.withValues(alpha: 0.2),
                    foregroundColor: AppColors.warningAmber,
                    side: BorderSide(color: AppColors.warningAmber.withValues(alpha: 0.6)),
                  ),
                  icon: const Icon(Icons.restart_alt, size: 16),
                  label: const Text('Reset', style: TextStyle(fontWeight: FontWeight.bold)),
                  onPressed: () {
                    CommandDialog.showConfirmCommandDialog(
                      context,
                      device.id,
                      'RESET',
                      'Reset operating parameters to nominal defaults and clear latched faults.',
                      () async {
                        final res = await provider.resetDevice(device.id);
                        _showResultSnackbar(res.message, res.success);
                      },
                    );
                  },
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),

          // Parameter tuning buttons: Tune Frequency, Set Power
          Row(
            children: [
              Expanded(
                child: OutlinedButton.icon(
                  style: OutlinedButton.styleFrom(
                    foregroundColor: AppColors.frequencyGreen,
                    side: const BorderSide(color: AppColors.darkBorder),
                  ),
                  icon: const Icon(Icons.tune, size: 16),
                  label: const Text('Tune Frequency', style: TextStyle(fontSize: 12)),
                  onPressed: () {
                    CommandDialog.showFrequencyDialog(context, device, (freq) async {
                      final res = await provider.setFrequency(device.id, freq);
                      _showResultSnackbar(res.message, res.success);
                    });
                  },
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: OutlinedButton.icon(
                  style: OutlinedButton.styleFrom(
                    foregroundColor: AppColors.powerBlue,
                    side: const BorderSide(color: AppColors.darkBorder),
                  ),
                  icon: const Icon(Icons.speed, size: 16),
                  label: const Text('Set Signal Power', style: TextStyle(fontSize: 12)),
                  onPressed: () {
                    CommandDialog.showPowerDialog(context, device, (pwr) async {
                      final res = await provider.setPower(device.id, pwr);
                      _showResultSnackbar(res.message, res.success);
                    });
                  },
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildThresholdsCard(DeviceModel device) {
    final t = device.thresholds;
    if (t == null) return const SizedBox.shrink();

    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppColors.darkCard,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.darkBorder),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'CONFIGURED SAFETY THRESHOLDS',
            style: TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.bold,
              color: Colors.white54,
              letterSpacing: 0.5,
            ),
          ),
          const SizedBox(height: 10),
          _thresholdRow('Temperature Alarm', 'Warn: >= ${t.temperatureWarning}°C  •  Crit: >= ${t.temperatureCritical}°C'),
          _thresholdRow('RF Power Limit', 'Warn: <= ${t.signalPowerWarning} dBm  •  Crit: <= ${t.signalPowerCritical} dBm'),
          _thresholdRow('Voltage Window', '${t.voltageMin} V  to  ${t.voltageMax} V'),
          _thresholdRow('Frequency Passband', '${t.frequencyMin} MHz  to  ${t.frequencyMax} MHz'),
          _thresholdRow('Max Current Draw', '${t.currentMaxA} A'),
        ],
      ),
    );
  }

  Widget _thresholdRow(String label, String val) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            flex: 4,
            child: Text(
              label,
              style: const TextStyle(fontSize: 12, color: Colors.white70),
            ),
          ),
          const SizedBox(width: 8),
          Expanded(
            flex: 6,
            child: Text(
              val,
              textAlign: TextAlign.end,
              style: const TextStyle(fontSize: 11, color: Colors.white38, fontFamily: 'monospace'),
            ),
          ),
        ],
      ),
    );
  }

  Widget _specItem(String label, String val) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(label, style: const TextStyle(fontSize: 10, color: Colors.white38)),
        const SizedBox(height: 2),
        Text(val, style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w600, color: Colors.white70)),
      ],
    );
  }
}
