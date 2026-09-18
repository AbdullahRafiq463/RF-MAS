class CommandResultModel {
  final bool success;
  final String status;
  final String message;
  final String deviceId;
  final String commandType;
  final DateTime executedAt;

  CommandResultModel({
    required this.success,
    required this.status,
    required this.message,
    required this.deviceId,
    required this.commandType,
    required this.executedAt,
  });

  factory CommandResultModel.fromJson(Map<String, dynamic> json) {
    return CommandResultModel(
      success: json['success'] as bool? ?? false,
      status: json['status'] as String? ?? 'FAILED',
      message: json['message'] as String? ?? '',
      deviceId: json['deviceId'] as String? ?? '',
      commandType: json['commandType'] as String? ?? '',
      executedAt: json['executedAt'] != null
          ? DateTime.tryParse(json['executedAt'].toString()) ?? DateTime.now()
          : DateTime.now(),
    );
  }
}
