class AutomationTestResultModel {
  final int id;
  final String testName;
  final String deviceId;
  final String expectedValue;
  final String actualValue;
  final String status;
  final int executionDurationMs;
  final DateTime executedAt;
  final String? errorMessage;

  AutomationTestResultModel({
    required this.id,
    required this.testName,
    required this.deviceId,
    required this.expectedValue,
    required this.actualValue,
    required this.status,
    required this.executionDurationMs,
    required this.executedAt,
    this.errorMessage,
  });

  factory AutomationTestResultModel.fromJson(Map<String, dynamic> json) {
    return AutomationTestResultModel(
      id: (json['id'] as num?)?.toInt() ?? 0,
      testName: json['testName'] as String? ?? '',
      deviceId: json['deviceId'] as String? ?? '',
      expectedValue: json['expectedValue'] as String? ?? '',
      actualValue: json['actualValue'] as String? ?? '',
      status: json['status'] as String? ?? 'ERROR',
      executionDurationMs: (json['executionDurationMs'] as num?)?.toInt() ?? 0,
      executedAt: json['executedAt'] != null
          ? DateTime.tryParse(json['executedAt'].toString()) ?? DateTime.now()
          : DateTime.now(),
      errorMessage: json['errorMessage'] as String?,
    );
  }
}
