import 'telemetry.dart';
import 'alert.dart';

class ThresholdConfigModel {
  final double temperatureWarning;
  final double temperatureCritical;
  final double signalPowerWarning;
  final double signalPowerCritical;
  final double voltageMin;
  final double voltageMax;
  final double frequencyMin;
  final double frequencyMax;
  final double currentMaxA;

  ThresholdConfigModel({
    required this.temperatureWarning,
    required this.temperatureCritical,
    required this.signalPowerWarning,
    required this.signalPowerCritical,
    required this.voltageMin,
    required this.voltageMax,
    required this.frequencyMin,
    required this.frequencyMax,
    required this.currentMaxA,
  });

  factory ThresholdConfigModel.fromJson(Map<String, dynamic> json) {
    return ThresholdConfigModel(
      temperatureWarning: (json['temperatureWarning'] as num?)?.toDouble() ?? 60.0,
      temperatureCritical: (json['temperatureCritical'] as num?)?.toDouble() ?? 75.0,
      signalPowerWarning: (json['signalPowerWarning'] as num?)?.toDouble() ?? -50.0,
      signalPowerCritical: (json['signalPowerCritical'] as num?)?.toDouble() ?? -70.0,
      voltageMin: (json['voltageMin'] as num?)?.toDouble() ?? 11.0,
      voltageMax: (json['voltageMax'] as num?)?.toDouble() ?? 13.0,
      frequencyMin: (json['frequencyMin'] as num?)?.toDouble() ?? 2400.0,
      frequencyMax: (json['frequencyMax'] as num?)?.toDouble() ?? 2500.0,
      currentMaxA: (json['currentMaxA'] as num?)?.toDouble() ?? 3.0,
    );
  }
}

class DeviceModel {
  final String id;
  final String name;
  final String deviceType;
  final String manufacturer;
  final String model;
  final String ipAddress;
  final int port;
  final bool connectionStatus;
  final String operationalStatus;
  final double frequencyMHz;
  final double signalPowerDbm;
  final double temperatureC;
  final double voltageV;
  final double currentA;
  final double healthPercentage;
  final DateTime lastCommunicationTime;
  final String? firmwareVersion;
  final ThresholdConfigModel? thresholds;
  final List<TelemetryModel> recentTelemetry;
  final List<AlertModel> recentAlerts;

  DeviceModel({
    required this.id,
    required this.name,
    required this.deviceType,
    required this.manufacturer,
    required this.model,
    required this.ipAddress,
    required this.port,
    required this.connectionStatus,
    required this.operationalStatus,
    required this.frequencyMHz,
    required this.signalPowerDbm,
    required this.temperatureC,
    required this.voltageV,
    required this.currentA,
    required this.healthPercentage,
    required this.lastCommunicationTime,
    this.firmwareVersion,
    this.thresholds,
    this.recentTelemetry = const [],
    this.recentAlerts = const [],
  });

  factory DeviceModel.fromJson(Map<String, dynamic> json) {
    return DeviceModel(
      id: json['id'] as String? ?? '',
      name: json['name'] as String? ?? '',
      deviceType: json['deviceType'] as String? ?? '',
      manufacturer: json['manufacturer'] as String? ?? '',
      model: json['model'] as String? ?? '',
      ipAddress: json['ipAddress'] as String? ?? '127.0.0.1',
      port: (json['port'] as num?)?.toInt() ?? 9001,
      connectionStatus: json['connectionStatus'] as bool? ?? false,
      operationalStatus: json['operationalStatus'] as String? ?? 'OFFLINE',
      frequencyMHz: (json['frequencyMHz'] as num?)?.toDouble() ?? 0.0,
      signalPowerDbm: (json['signalPowerDbm'] as num?)?.toDouble() ?? 0.0,
      temperatureC: (json['temperatureC'] as num?)?.toDouble() ?? 0.0,
      voltageV: (json['voltageV'] as num?)?.toDouble() ?? 0.0,
      currentA: (json['currentA'] as num?)?.toDouble() ?? 0.0,
      healthPercentage: (json['healthPercentage'] as num?)?.toDouble() ?? 0.0,
      lastCommunicationTime: json['lastCommunicationTime'] != null
          ? DateTime.tryParse(json['lastCommunicationTime'].toString()) ?? DateTime.now()
          : DateTime.now(),
      firmwareVersion: json['firmwareVersion'] as String?,
      thresholds: json['thresholds'] != null ? ThresholdConfigModel.fromJson(json['thresholds']) : null,
      recentTelemetry: (json['recentTelemetry'] as List<dynamic>?)
              ?.map((e) => TelemetryModel.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      recentAlerts: (json['recentAlerts'] as List<dynamic>?)
              ?.map((e) => AlertModel.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}
