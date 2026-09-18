class SimulationDeviceModel {
  final String deviceId;
  final String name;
  final String activeScenario;
  final bool isConnected;
  final String status;

  SimulationDeviceModel({
    required this.deviceId,
    required this.name,
    required this.activeScenario,
    required this.isConnected,
    required this.status,
  });

  factory SimulationDeviceModel.fromJson(Map<String, dynamic> json) {
    return SimulationDeviceModel(
      deviceId: json['deviceId'] as String? ?? '',
      name: json['name'] as String? ?? '',
      activeScenario: json['activeScenario'] as String? ?? 'NORMAL',
      isConnected: json['isConnected'] as bool? ?? true,
      status: json['status'] as String? ?? 'ONLINE',
    );
  }
}
