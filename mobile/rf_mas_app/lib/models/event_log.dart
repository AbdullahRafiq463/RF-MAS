class EventLogModel {
  final int id;
  final DateTime timestamp;
  final String? deviceId;
  final String category;
  final String eventName;
  final String details;
  final String severity;

  EventLogModel({
    required this.id,
    required this.timestamp,
    this.deviceId,
    required this.category,
    required this.eventName,
    required this.details,
    required this.severity,
  });

  factory EventLogModel.fromJson(Map<String, dynamic> json) {
    return EventLogModel(
      id: (json['id'] as num?)?.toInt() ?? 0,
      timestamp: json['timestamp'] != null
          ? DateTime.tryParse(json['timestamp'].toString()) ?? DateTime.now()
          : DateTime.now(),
      deviceId: json['deviceId'] as String?,
      category: json['category'] as String? ?? 'SYSTEM',
      eventName: json['eventName'] as String? ?? '',
      details: json['details'] as String? ?? '',
      severity: json['severity'] as String? ?? 'INFO',
    );
  }
}
