class TelemetryModel {
  final int? id;
  final String deviceId;
  final double frequencyMHz;
  final double signalPowerDbm;
  final double temperatureC;
  final double voltageV;
  final double currentA;
  final String status;
  final DateTime timestamp;

  TelemetryModel({
    this.id,
    required this.deviceId,
    required this.frequencyMHz,
    required this.signalPowerDbm,
    required this.temperatureC,
    required this.voltageV,
    required this.currentA,
    required this.status,
    required this.timestamp,
  });

  factory TelemetryModel.fromJson(Map<String, dynamic> json) {
    return TelemetryModel(
      id: (json['id'] as num?)?.toInt(),
      deviceId: json['deviceId'] as String? ?? '',
      frequencyMHz: (json['frequencyMHz'] as num?)?.toDouble() ?? 0.0,
      signalPowerDbm: (json['signalPowerDbm'] as num?)?.toDouble() ?? 0.0,
      temperatureC: (json['temperatureC'] as num?)?.toDouble() ?? 0.0,
      voltageV: (json['voltageV'] as num?)?.toDouble() ?? 0.0,
      currentA: (json['currentA'] as num?)?.toDouble() ?? 0.0,
      status: json['status'] as String? ?? 'ONLINE',
      timestamp: json['timestamp'] != null
          ? DateTime.tryParse(json['timestamp'].toString()) ?? DateTime.now()
          : DateTime.now(),
    );
  }
}
