import 'package:flutter/material.dart';
import '../models/device.dart';
import '../theme/app_colors.dart';

class CommandDialog {
  static Future<void> showFrequencyDialog(
    BuildContext context,
    DeviceModel device,
    Function(double) onConfirm,
  ) async {
    final controller = TextEditingController(text: device.frequencyMHz.toStringAsFixed(2));
    final minFreq = device.thresholds?.frequencyMin ?? 100.0;
    final maxFreq = device.thresholds?.frequencyMax ?? 10000.0;

    await showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text('Tune Frequency - ${device.id}'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Allowed Band: ${minFreq.toStringAsFixed(1)} - ${maxFreq.toStringAsFixed(1)} MHz',
              style: const TextStyle(color: Colors.white54, fontSize: 12),
            ),
            const SizedBox(height: 16),
            TextField(
              controller: controller,
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              decoration: const InputDecoration(
                labelText: 'Target Frequency (MHz)',
                suffixText: 'MHz',
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Cancel', style: TextStyle(color: Colors.white54)),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.primaryTeal),
            onPressed: () {
              final val = double.tryParse(controller.text);
              if (val != null) {
                Navigator.pop(ctx);
                onConfirm(val);
              }
            },
            child: const Text('Set Frequency', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }

  static Future<void> showPowerDialog(
    BuildContext context,
    DeviceModel device,
    Function(double) onConfirm,
  ) async {
    final controller = TextEditingController(text: device.signalPowerDbm.toStringAsFixed(2));

    await showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text('Set Signal Power - ${device.id}'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Safe Limit: -100.0 to +30.0 dBm',
              style: TextStyle(color: Colors.white54, fontSize: 12),
            ),
            const SizedBox(height: 16),
            TextField(
              controller: controller,
              keyboardType: const TextInputType.numberWithOptions(decimal: true, signed: true),
              decoration: const InputDecoration(
                labelText: 'Output Power (dBm)',
                suffixText: 'dBm',
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Cancel', style: TextStyle(color: Colors.white54)),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.powerBlue),
            onPressed: () {
              final val = double.tryParse(controller.text);
              if (val != null) {
                Navigator.pop(ctx);
                onConfirm(val);
              }
            },
            child: const Text('Set Power', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }

  static Future<void> showConfirmCommandDialog(
    BuildContext context,
    String deviceId,
    String commandName,
    String description,
    VoidCallback onConfirm,
  ) async {
    await showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text('Execute $commandName?'),
        content: Text('Are you sure you want to execute command "$commandName" on device $deviceId?\n\n$description'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Cancel', style: TextStyle(color: Colors.white54)),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.primaryTeal),
            onPressed: () {
              Navigator.pop(ctx);
              onConfirm();
            },
            child: const Text('Confirm', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }
}
