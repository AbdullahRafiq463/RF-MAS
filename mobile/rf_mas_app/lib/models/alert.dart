class AlertModel {
  final int id;
  final String deviceId;
  final String severity;
  final String alertType;
  final String message;
  final DateTime timestamp;
  final bool isAcknowledged;
  final DateTime? acknowledgedAt;
  final String? acknowledgedBy;

  AlertModel({
    required this.id,
    required this.deviceId,
    required this.severity,
    required this.alertType,
    required this.message,
    required this.timestamp,
    required this.isAcknowledged,
    this.acknowledgedAt,
    this.acknowledgedBy,
  });

  factory AlertModel.fromJson(Map<String, dynamic> json) {
    return AlertModel(
      id: (json['id'] as num?)?.toInt() ?? 0,
      deviceId: json['deviceId'] as String? ?? '',
      severity: json['severity'] as String? ?? 'INFO',
      alertType: json['alertType'] as String? ?? '',
      message: json['message'] as String? ?? '',
      timestamp: json['timestamp'] != null
          ? DateTime.tryParse(json['timestamp'].toString()) ?? DateTime.now()
          : DateTime.now(),
      isAcknowledged: json['isAcknowledged'] as bool? ?? false,
      acknowledgedAt: json['acknowledgedAt'] != null
          ? DateTime.tryParse(json['acknowledgedAt'].toString())
          : null,
      acknowledgedBy: json['acknowledgedBy'] as String?,
    );
  }
}
