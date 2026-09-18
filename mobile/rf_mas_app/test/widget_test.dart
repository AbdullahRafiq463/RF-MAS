import 'package:flutter_test/flutter_test.dart';
import 'package:rf_mas_app/models/device.dart';
import 'package:rf_mas_app/models/telemetry.dart';

void main() {
  test('DeviceModel deserializes correctly from json', () {
    final json = {
      'id': 'RF-001',
      'name': 'Signal Generator Simulator',
      'deviceType': 'Signal Generator',
      'manufacturer': 'AeroTech Instruments',
      'model': 'SG-2500',
      'ipAddress': '127.0.0.1',
      'port': 9001,
      'connectionStatus': true,
      'operationalStatus': 'ONLINE',
      'frequencyMHz': 2450.0,
      'signalPowerDbm': -30.0,
      'temperatureC': 42.0,
      'voltageV': 12.0,
      'currentA': 1.8,
      'healthPercentage': 100.0,
    };

    final device = DeviceModel.fromJson(json);

    expect(device.id, 'RF-001');
    expect(device.frequencyMHz, 2450.0);
    expect(device.healthPercentage, 100.0);
    expect(device.operationalStatus, 'ONLINE');
  });

  test('TelemetryModel deserializes correctly from json', () {
    final json = {
      'deviceId': 'RF-002',
      'frequencyMHz': 5200.0,
      'signalPowerDbm': -25.0,
      'temperatureC': 44.0,
      'voltageV': 12.1,
      'currentA': 2.1,
      'status': 'ONLINE',
    };

    final telemetry = TelemetryModel.fromJson(json);

    expect(telemetry.deviceId, 'RF-002');
    expect(telemetry.frequencyMHz, 5200.0);
    expect(telemetry.status, 'ONLINE');
  });
}
