import 'package:flutter/foundation.dart';
import '../models/automation_test.dart';
import '../services/api_service.dart';

class AutomationProvider extends ChangeNotifier {
  final ApiService _apiService;

  List<AutomationTestResultModel> _testHistory = [];
  bool _isLoading = false;
  bool _isRunningSuite = false;
  String? _errorMessage;

  AutomationProvider(this._apiService) {
    loadTestHistory();
  }

  List<AutomationTestResultModel> get testHistory => _testHistory;
  bool get isLoading => _isLoading;
  bool get isRunningSuite => _isRunningSuite;
  String? get errorMessage => _errorMessage;

  int get passCount => _testHistory.where((t) => t.status == 'PASS').length;
  int get failCount => _testHistory.where((t) => t.status == 'FAIL').length;
  int get errorCount => _testHistory.where((t) => t.status == 'ERROR').length;

  Future<void> loadTestHistory() async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      _testHistory = await _apiService.getAutomationTests(count: 60);
      _errorMessage = null;
    } catch (e) {
      _errorMessage = e.toString().replaceAll('Exception: ', '');
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<List<AutomationTestResultModel>> runAllTests() async {
    _isRunningSuite = true;
    _errorMessage = null;
    notifyListeners();

    try {
      final results = await _apiService.runAllTests();
      await loadTestHistory();
      return results;
    } catch (e) {
      _errorMessage = e.toString().replaceAll('Exception: ', '');
      return [];
    } finally {
      _isRunningSuite = false;
      notifyListeners();
    }
  }

  Future<List<AutomationTestResultModel>> runDeviceTests(String deviceId) async {
    _isRunningSuite = true;
    _errorMessage = null;
    notifyListeners();

    try {
      final results = await _apiService.runDeviceTests(deviceId);
      await loadTestHistory();
      return results;
    } catch (e) {
      _errorMessage = e.toString().replaceAll('Exception: ', '');
      return [];
    } finally {
      _isRunningSuite = false;
      notifyListeners();
    }
  }

  Future<AutomationTestResultModel?> runSingleTest(String testName, String deviceId) async {
    _isRunningSuite = true;
    _errorMessage = null;
    notifyListeners();

    try {
      final result = await _apiService.runSingleTest(testName, deviceId);
      await loadTestHistory();
      return result;
    } catch (e) {
      _errorMessage = e.toString().replaceAll('Exception: ', '');
      return null;
    } finally {
      _isRunningSuite = false;
      notifyListeners();
    }
  }
}
