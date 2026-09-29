import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../services/settings_service.dart';
import '../theme/app_colors.dart';

class SettingsScreen extends StatefulWidget {
  const SettingsScreen({super.key});

  @override
  State<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends State<SettingsScreen> {
  late TextEditingController _urlController;

  @override
  void initState() {
    super.initState();
    final settings = context.read<SettingsService>();
    _urlController = TextEditingController(text: settings.baseUrl);
  }

  @override
  void dispose() {
    _urlController.dispose();
    super.dispose();
  }

  void _saveUrl(SettingsService settings) {
    settings.setBaseUrl(_urlController.text);
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Backend URL saved successfully.'),
        backgroundColor: AppColors.onlineGreen,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('System Settings & Connectivity'),
      ),
      body: Consumer<SettingsService>(
        builder: (context, settings, _) {
          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              // Educational Simulation Disclaimer Card
              Container(
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(
                  color: AppColors.primaryBlue.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: AppColors.primaryBlue.withValues(alpha: 0.4)),
                ),
                child: const Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Icon(Icons.school_outlined, color: AppColors.primaryBlue, size: 18),
                        SizedBox(width: 8),
                        Expanded(
                          child: Text(
                            'EDUCATIONAL SIMULATION NOTICE',
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            style: TextStyle(
                              color: AppColors.primaryBlue,
                              fontWeight: FontWeight.bold,
                              fontSize: 12,
                              letterSpacing: 0.5,
                            ),
                          ),
                        ),
                      ],
                    ),
                    SizedBox(height: 6),
                    Text(
                      'RF-MAS is a software-based engineering monitoring and automated testing simulation. It simulates RF instruments, telemetry generation, and command validation without interfacing with classified or production RF hardware.',
                      style: TextStyle(color: Colors.white70, fontSize: 11, height: 1.4),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 20),

              // Network Configuration Section
              const Text(
                'BACKEND NETWORK CONFIGURATION',
                style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: Colors.white54, letterSpacing: 0.5),
              ),
              const SizedBox(height: 8),
              Container(
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(
                  color: AppColors.darkCard,
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: AppColors.darkBorder),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    TextField(
                      controller: _urlController,
                      decoration: const InputDecoration(
                        labelText: 'ASP.NET Core Backend Base URL',
                        hintText: 'http://192.168.0.107:5000',
                        prefixIcon: Icon(Icons.dns_outlined, size: 20),
                      ),
                    ),
                    const SizedBox(height: 12),
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton.icon(
                        style: ElevatedButton.styleFrom(
                          backgroundColor: AppColors.primaryTeal,
                          foregroundColor: Colors.black,
                        ),
                        icon: const Icon(Icons.save, size: 16),
                        label: const Text('Save & Apply Backend URL', style: TextStyle(fontWeight: FontWeight.bold)),
                        onPressed: () => _saveUrl(settings),
                      ),
                    ),
                    const SizedBox(height: 14),
                    const Text('Quick Connection Presets:', style: TextStyle(fontSize: 11, color: Colors.white38)),
                    const SizedBox(height: 6),
                    Wrap(
                      spacing: 8,
                      runSpacing: 6,
                      children: [
                        OutlinedButton(
                          onPressed: () {
                            _urlController.text = 'http://192.168.0.107:5000';
                            _saveUrl(settings);
                          },
                          child: const Text('Wi-Fi Phone (192.168.0.107)', style: TextStyle(fontSize: 11)),
                        ),
                        OutlinedButton(
                          onPressed: () {
                            _urlController.text = 'http://localhost:5000';
                            _saveUrl(settings);
                          },
                          child: const Text('Localhost (Windows / Web)', style: TextStyle(fontSize: 11)),
                        ),
                        OutlinedButton(
                          onPressed: () {
                            _urlController.text = 'http://10.0.2.2:5000';
                            _saveUrl(settings);
                          },
                          child: const Text('Android Emulator (10.0.2.2)', style: TextStyle(fontSize: 11)),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 20),

              // Polling & Refresh Settings
              const Text(
                'TELEMETRY POLLING PREFERENCES',
                style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: Colors.white54, letterSpacing: 0.5),
              ),
              const SizedBox(height: 8),
              Container(
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(
                  color: AppColors.darkCard,
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: AppColors.darkBorder),
                ),
                child: Column(
                  children: [
                    SwitchListTile(
                      contentPadding: EdgeInsets.zero,
                      title: const Text('Auto-Refresh Telemetry', style: TextStyle(fontSize: 13, fontWeight: FontWeight.w600)),
                      subtitle: const Text('Periodically poll server for live instrument metrics', style: TextStyle(fontSize: 11, color: Colors.white38)),
                      value: settings.autoRefreshEnabled,
                      activeTrackColor: AppColors.primaryTeal,
                      onChanged: (val) => settings.setAutoRefresh(val),
                    ),
                    if (settings.autoRefreshEnabled) ...[
                      const Divider(),
                      const SizedBox(height: 8),
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          const Text('Polling Interval', style: TextStyle(fontSize: 12, color: Colors.white70)),
                          Text('${settings.refreshIntervalSeconds} seconds', style: const TextStyle(fontSize: 12, color: AppColors.primaryTeal, fontWeight: FontWeight.bold)),
                        ],
                      ),
                      Slider(
                        value: settings.refreshIntervalSeconds.toDouble(),
                        min: 2,
                        max: 15,
                        divisions: 13,
                        activeColor: AppColors.primaryTeal,
                        inactiveColor: AppColors.darkBorder,
                        onChanged: (val) => settings.setRefreshInterval(val.toInt()),
                      ),
                    ],
                  ],
                ),
              ),
              const SizedBox(height: 24),
            ],
          );
        },
      ),
    );
  }
}
