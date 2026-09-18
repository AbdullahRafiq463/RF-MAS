import 'package:flutter/foundation.dart';
import '../models/event_log.dart';
import '../services/api_service.dart';

class LogsProvider extends ChangeNotifier {
  final ApiService _apiService;

  List<EventLogModel> _logs = [];
  bool _isLoading = false;
  String? _errorMessage;
  String _selectedCategory = 'ALL';
  String? _selectedDeviceId;

  LogsProvider(this._apiService) {
    loadLogs();
  }

  List<EventLogModel> get logs => _logs;
  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;
  String get selectedCategory => _selectedCategory;
  String? get selectedDeviceId => _selectedDeviceId;

  void setCategory(String category) {
    _selectedCategory = category;
    loadLogs();
  }

  void setDeviceId(String? deviceId) {
    _selectedDeviceId = deviceId;
    loadLogs();
  }

  Future<void> loadLogs() async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      _logs = await _apiService.getLogs(
        deviceId: _selectedDeviceId,
        category: _selectedCategory == 'ALL' ? null : _selectedCategory,
        limit: 150,
      );
      _errorMessage = null;
    } catch (e) {
      _errorMessage = e.toString().replaceAll('Exception: ', '');
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }
}
