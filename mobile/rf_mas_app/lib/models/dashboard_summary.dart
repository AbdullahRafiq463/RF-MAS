class DashboardSummaryModel {
  final int totalDevices;
  final int onlineDevices;
  final int warningDevices;
  final int errorDevices;
  final int offlineDevices;
  final int maintenanceDevices;
  final int totalAlerts;
  final int activeAlerts;
  final int criticalAlerts;
  final int warningAlerts;
  final int testsExecuted;
  final int testsPassed;
  final int testsFailed;
  final int testsErrored;
  final double averageSystemHealth;
  final DateTime serverTimeUtc;

  DashboardSummaryModel({
    required this.totalDevices,
    required this.onlineDevices,
    required this.warningDevices,
    required this.errorDevices,
    required this.offlineDevices,
    required this.maintenanceDevices,
    required this.totalAlerts,
    required this.activeAlerts,
    required this.criticalAlerts,
    required this.warningAlerts,
    required this.testsExecuted,
    required this.testsPassed,
    required this.testsFailed,
    required this.testsErrored,
    required this.averageSystemHealth,
    required this.serverTimeUtc,
  });

  factory DashboardSummaryModel.fromJson(Map<String, dynamic> json) {
    return DashboardSummaryModel(
      totalDevices: (json['totalDevices'] as num?)?.toInt() ?? 0,
      onlineDevices: (json['onlineDevices'] as num?)?.toInt() ?? 0,
      warningDevices: (json['warningDevices'] as num?)?.toInt() ?? 0,
      errorDevices: (json['errorDevices'] as num?)?.toInt() ?? 0,
      offlineDevices: (json['offlineDevices'] as num?)?.toInt() ?? 0,
      maintenanceDevices: (json['maintenanceDevices'] as num?)?.toInt() ?? 0,
      totalAlerts: (json['totalAlerts'] as num?)?.toInt() ?? 0,
      activeAlerts: (json['activeAlerts'] as num?)?.toInt() ?? 0,
      criticalAlerts: (json['criticalAlerts'] as num?)?.toInt() ?? 0,
      warningAlerts: (json['warningAlerts'] as num?)?.toInt() ?? 0,
      testsExecuted: (json['testsExecuted'] as num?)?.toInt() ?? 0,
      testsPassed: (json['testsPassed'] as num?)?.toInt() ?? 0,
      testsFailed: (json['testsFailed'] as num?)?.toInt() ?? 0,
      testsErrored: (json['testsErrored'] as num?)?.toInt() ?? 0,
      averageSystemHealth: (json['averageSystemHealth'] as num?)?.toDouble() ?? 100.0,
      serverTimeUtc: json['serverTimeUtc'] != null
          ? DateTime.tryParse(json['serverTimeUtc'].toString()) ?? DateTime.now()
          : DateTime.now(),
    );
  }
}
