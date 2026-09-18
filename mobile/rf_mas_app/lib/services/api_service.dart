import 'dart:async';
import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/device.dart';
import '../models/telemetry.dart';
import '../models/alert.dart';
import '../models/command_result.dart';
import '../models/automation_test.dart';
import '../models/event_log.dart';
import '../models/dashboard_summary.dart';
import '../models/simulation_scenario.dart';
import 'settings_service.dart';

/// Central HTTP REST API client for communicating with the ASP.NET Core backend.
class ApiService {
  final SettingsService _settings;
  final http.Client _client;

  ApiService(this._settings, [http.Client? client]) : _client = client ?? http.Client();

  String get _baseUrl => _settings.baseUrl;

  Map<String, String> get _headers => {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      };

  // ==========================================
  // DASHBOARD
  // ==========================================
  Future<DashboardSummaryModel> getDashboardSummary() async {
    final uri = Uri.parse('$_baseUrl/api/dashboard/summary');
    final res = await _client.get(uri, headers: _headers).timeout(const Duration(seconds: 8));

    if (res.statusCode == 200) {
      final json = jsonDecode(res.body) as Map<String, dynamic>;
      return DashboardSummaryModel.fromJson(json);
    }
    throw _handleError(res);
  }

  // ==========================================
  // DEVICES & TELEMETRY
  // ==========================================
  Future<List<DeviceModel>> getDevices() async {
    final uri = Uri.parse('$_baseUrl/api/devices');
    final res = await _client.get(uri, headers: _headers).timeout(const Duration(seconds: 8));

    if (res.statusCode == 200) {
      final list = jsonDecode(res.body) as List<dynamic>;
      return list.map((e) => DeviceModel.fromJson(e as Map<String, dynamic>)).toList();
    }
    throw _handleError(res);
  }

  Future<DeviceModel> getDevice(String id) async {
    final uri = Uri.parse('$_baseUrl/api/devices/$id');
    final res = await _client.get(uri, headers: _headers).timeout(const Duration(seconds: 8));

    if (res.statusCode == 200) {
      final json = jsonDecode(res.body) as Map<String, dynamic>;
      return DeviceModel.fromJson(json);
    }
    throw _handleError(res);
  }

  Future<List<TelemetryModel>> getDeviceTelemetry(String id, {int count = 50}) async {
    final uri = Uri.parse('$_baseUrl/api/devices/$id/telemetry?count=$count');
    final res = await _client.get(uri, headers: _headers).timeout(const Duration(seconds: 8));

    if (res.statusCode == 200) {
      final list = jsonDecode(res.body) as List<dynamic>;
      return list.map((e) => TelemetryModel.fromJson(e as Map<String, dynamic>)).toList();
    }
    throw _handleError(res);
  }

  // ==========================================
  // COMMANDS
  // ==========================================
  Future<CommandResultModel> startDevice(String id) async {
    final uri = Uri.parse('$_baseUrl/api/devices/$id/start');
    final res = await _client.post(uri, headers: _headers).timeout(const Duration(seconds: 8));
    return _parseCommandResponse(res);
  }

  Future<CommandResultModel> stopDevice(String id) async {
    final uri = Uri.parse('$_baseUrl/api/devices/$id/stop');
    final res = await _client.post(uri, headers: _headers).timeout(const Duration(seconds: 8));
    return _parseCommandResponse(res);
  }

  Future<CommandResultModel> resetDevice(String id) async {
    final uri = Uri.parse('$_baseUrl/api/devices/$id/reset');
    final res = await _client.post(uri, headers: _headers).timeout(const Duration(seconds: 8));
    return _parseCommandResponse(res);
  }

  Future<CommandResultModel> setFrequency(String id, double frequencyMHz) async {
    final uri = Uri.parse('$_baseUrl/api/devices/$id/frequency');
    final body = jsonEncode({'frequencyMHz': frequencyMHz});
    final res = await _client.post(uri, headers: _headers, body: body).timeout(const Duration(seconds: 8));
    return _parseCommandResponse(res);
  }

  Future<CommandResultModel> setPower(String id, double powerDbm) async {
    final uri = Uri.parse('$_baseUrl/api/devices/$id/power');
    final body = jsonEncode({'powerDbm': powerDbm});
    final res = await _client.post(uri, headers: _headers, body: body).timeout(const Duration(seconds: 8));
    return _parseCommandResponse(res);
  }

  Future<CommandResultModel> genericCommand(String id, String commandType, [Map<String, dynamic>? parameters]) async {
    final uri = Uri.parse('$_baseUrl/api/devices/$id/command');
    final body = jsonEncode({
      'commandType': commandType,
      'parameters': parameters,
    });
    final res = await _client.post(uri, headers: _headers, body: body).timeout(const Duration(seconds: 8));
    return _parseCommandResponse(res);
  }

  // ==========================================
  // ALERTS
  // ==========================================
  Future<List<AlertModel>> getAlerts({bool? activeOnly, String? severity}) async {
    var path = '$_baseUrl/api/alerts';
    final params = <String>[];
    if (activeOnly != null) params.add('activeOnly=$activeOnly');
    if (severity != null && severity.isNotEmpty && severity != 'ALL') {
      params.add('severity=$severity');
    }
    if (params.isNotEmpty) {
      path += '?${params.join('&')}';
    }

    final uri = Uri.parse(path);
    final res = await _client.get(uri, headers: _headers).timeout(const Duration(seconds: 8));

    if (res.statusCode == 200) {
      final list = jsonDecode(res.body) as List<dynamic>;
      return list.map((e) => AlertModel.fromJson(e as Map<String, dynamic>)).toList();
    }
    throw _handleError(res);
  }

  Future<bool> acknowledgeAlert(int alertId, {String? acknowledgedBy}) async {
    final uri = Uri.parse('$_baseUrl/api/alerts/$alertId/acknowledge');
    final body = jsonEncode({'acknowledgedBy': acknowledgedBy ?? 'Mobile User'});
    final res = await _client.post(uri, headers: _headers, body: body).timeout(const Duration(seconds: 8));
    return res.statusCode == 200;
  }

  // ==========================================
  // AUTOMATION
  // ==========================================
  Future<List<AutomationTestResultModel>> getAutomationTests({int count = 50}) async {
    final uri = Uri.parse('$_baseUrl/api/automation/tests?count=$count');
    final res = await _client.get(uri, headers: _headers).timeout(const Duration(seconds: 8));

    if (res.statusCode == 200) {
      final list = jsonDecode(res.body) as List<dynamic>;
      return list.map((e) => AutomationTestResultModel.fromJson(e as Map<String, dynamic>)).toList();
    }
    throw _handleError(res);
  }

  Future<List<AutomationTestResultModel>> runAllTests() async {
    final uri = Uri.parse('$_baseUrl/api/automation/tests/run');
    final res = await _client.post(uri, headers: _headers).timeout(const Duration(seconds: 30));

    if (res.statusCode == 200) {
      final list = jsonDecode(res.body) as List<dynamic>;
      return list.map((e) => AutomationTestResultModel.fromJson(e as Map<String, dynamic>)).toList();
    }
    throw _handleError(res);
  }

  Future<List<AutomationTestResultModel>> runDeviceTests(String deviceId) async {
    final uri = Uri.parse('$_baseUrl/api/automation/devices/$deviceId/run');
    final res = await _client.post(uri, headers: _headers).timeout(const Duration(seconds: 20));

    if (res.statusCode == 200) {
      final list = jsonDecode(res.body) as List<dynamic>;
      return list.map((e) => AutomationTestResultModel.fromJson(e as Map<String, dynamic>)).toList();
    }
    throw _handleError(res);
  }

  Future<AutomationTestResultModel> runSingleTest(String testName, String deviceId) async {
    final uri = Uri.parse('$_baseUrl/api/automation/test/run-single');
    final body = jsonEncode({'testName': testName, 'deviceId': deviceId});
    final res = await _client.post(uri, headers: _headers, body: body).timeout(const Duration(seconds: 15));

    if (res.statusCode == 200) {
      final json = jsonDecode(res.body) as Map<String, dynamic>;
      return AutomationTestResultModel.fromJson(json);
    }
    throw _handleError(res);
  }

  // ==========================================
  // EVENT LOGS
  // ==========================================
  Future<List<EventLogModel>> getLogs({String? deviceId, String? category, int limit = 100}) async {
    var path = '$_baseUrl/api/logs?limit=$limit';
    if (deviceId != null && deviceId.isNotEmpty) path += '&deviceId=$deviceId';
    if (category != null && category.isNotEmpty && category != 'ALL') path += '&category=$category';

    final uri = Uri.parse(path);
    final res = await _client.get(uri, headers: _headers).timeout(const Duration(seconds: 8));

    if (res.statusCode == 200) {
      final list = jsonDecode(res.body) as List<dynamic>;
      return list.map((e) => EventLogModel.fromJson(e as Map<String, dynamic>)).toList();
    }
    throw _handleError(res);
  }

  // ==========================================
  // SIMULATION SCENARIO INJECTION
  // ==========================================
  Future<List<SimulationDeviceModel>> getSimulationDevices() async {
    final uri = Uri.parse('$_baseUrl/api/simulation/devices');
    final res = await _client.get(uri, headers: _headers).timeout(const Duration(seconds: 8));

    if (res.statusCode == 200) {
      final list = jsonDecode(res.body) as List<dynamic>;
      return list.map((e) => SimulationDeviceModel.fromJson(e as Map<String, dynamic>)).toList();
    }
    throw _handleError(res);
  }

  Future<bool> injectSimulationScenario(
    String deviceId,
    String scenario, {
    double? customValue,
    String? reason,
  }) async {
    final uri = Uri.parse('$_baseUrl/api/simulation/devices/$deviceId/scenario');
    final body = jsonEncode({
      'scenario': scenario,
      'customValue': customValue,
      'reason': reason,
    });
    final res = await _client.post(uri, headers: _headers, body: body).timeout(const Duration(seconds: 8));
    return res.statusCode == 200;
  }

  // ==========================================
  // HELPERS
  // ==========================================
  CommandResultModel _parseCommandResponse(http.Response res) {
    try {
      final json = jsonDecode(res.body) as Map<String, dynamic>;
      return CommandResultModel.fromJson(json);
    } catch (_) {
      return CommandResultModel(
        success: res.statusCode >= 200 && res.statusCode < 300,
        status: res.statusCode >= 200 && res.statusCode < 300 ? 'SUCCESS' : 'FAILED',
        message: 'HTTP ${res.statusCode}: ${res.reasonPhrase}',
        deviceId: '',
        commandType: 'UNKNOWN',
        executedAt: DateTime.now(),
      );
    }
  }

  Exception _handleError(http.Response res) {
    try {
      final json = jsonDecode(res.body) as Map<String, dynamic>;
      final msg = json['message'] ?? json['error'] ?? 'HTTP ${res.statusCode}';
      return Exception(msg);
    } catch (_) {
      return Exception('HTTP ${res.statusCode}: ${res.reasonPhrase}');
    }
  }
}
