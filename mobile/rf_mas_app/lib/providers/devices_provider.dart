import 'dart:async';
import 'package:flutter/foundation.dart';
import '../models/device.dart';
import '../models/telemetry.dart';
import '../models/command_result.dart';
import '../services/api_service.dart';
import '../services/settings_service.dart';

class DevicesProvider extends ChangeNotifier {
  final ApiService _apiService;
  final SettingsService _settings;

  List<DeviceModel> _devices = [];
  DeviceModel? _selectedDevice;
  List<TelemetryModel> _selectedDeviceTelemetry = [];
  bool _isLoading = false;
  bool _isCommandExecuting = false;
  String? _errorMessage;
  Timer? _pollingTimer;

  DevicesProvider(this._apiService, this._settings) {
    loadDevices();
    _initPolling();
    _settings.addListener(_onSettingsChanged);
  }

  List<DeviceModel> get devices => _devices;
  DeviceModel? get selectedDevice => _selectedDevice;
  List<TelemetryModel> get selectedDeviceTelemetry => _selectedDeviceTelemetry;
  bool get isLoading => _isLoading;
  bool get isCommandExecuting => _isCommandExecuting;
  String? get errorMessage => _errorMessage;

  void _onSettingsChanged() {
    _initPolling();
    loadDevices();
  }

  void _initPolling() {
    _pollingTimer?.cancel();
    if (_settings.autoRefreshEnabled) {
      _pollingTimer = Timer.periodic(
        Duration(seconds: _settings.refreshIntervalSeconds),
        (_) => _pollUpdates(),
      );
    }
  }

  Future<void> loadDevices() async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      _devices = await _apiService.getDevices();
      _errorMessage = null;
    } catch (e) {
      _errorMessage = e.toString().replaceAll('Exception: ', '');
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> selectDevice(String deviceId) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      final dev = await _apiService.getDevice(deviceId);
      final tele = await _apiService.getDeviceTelemetry(deviceId, count: 40);
      _selectedDevice = dev;
      _selectedDeviceTelemetry = tele;
      _errorMessage = null;
    } catch (e) {
      _errorMessage = e.toString().replaceAll('Exception: ', '');
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> _pollUpdates() async {
    try {
      final updatedList = await _apiService.getDevices();
      _devices = updatedList;

      if (_selectedDevice != null) {
        final currentId = _selectedDevice!.id;
        final updatedDev = updatedList.firstWhere(
          (d) => d.id == currentId,
          orElse: () => _selectedDevice!,
        );
        _selectedDevice = updatedDev;

        final newTele = await _apiService.getDeviceTelemetry(currentId, count: 40);
        _selectedDeviceTelemetry = newTele;
      }
      notifyListeners();
    } catch (_) {
      // Background poll silently ignores transient connection errors
    }
  }

  Future<CommandResultModel> startDevice(String deviceId) async {
    return _executeCommand(() => _apiService.startDevice(deviceId));
  }

  Future<CommandResultModel> stopDevice(String deviceId) async {
    return _executeCommand(() => _apiService.stopDevice(deviceId));
  }

  Future<CommandResultModel> resetDevice(String deviceId) async {
    return _executeCommand(() => _apiService.resetDevice(deviceId));
  }

  Future<CommandResultModel> setFrequency(String deviceId, double freqMHz) async {
    return _executeCommand(() => _apiService.setFrequency(deviceId, freqMHz));
  }

  Future<CommandResultModel> setPower(String deviceId, double powerDbm) async {
    return _executeCommand(() => _apiService.setPower(deviceId, powerDbm));
  }

  Future<bool> injectScenario(String deviceId, String scenario, {double? customValue, String? reason}) async {
    _isCommandExecuting = true;
    notifyListeners();
    try {
      final res = await _apiService.injectSimulationScenario(deviceId, scenario, customValue: customValue, reason: reason);
      await _pollUpdates();
      return res;
    } finally {
      _isCommandExecuting = false;
      notifyListeners();
    }
  }

  Future<CommandResultModel> _executeCommand(Future<CommandResultModel> Function() commandAction) async {
    _isCommandExecuting = true;
    notifyListeners();
    try {
      final result = await commandAction();
      await _pollUpdates();
      return result;
    } finally {
      _isCommandExecuting = false;
      notifyListeners();
    }
  }

  @override
  void dispose() {
    _pollingTimer?.cancel();
    _settings.removeListener(_onSettingsChanged);
    super.dispose();
  }
}
