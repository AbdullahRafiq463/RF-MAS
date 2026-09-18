import 'package:flutter/foundation.dart';
import '../models/alert.dart';
import '../services/api_service.dart';

class AlertsProvider extends ChangeNotifier {
  final ApiService _apiService;

  List<AlertModel> _alerts = [];
  bool _isLoading = false;
  String? _errorMessage;
  String _selectedSeverity = 'ALL';
  bool? _activeOnlyFilter;

  AlertsProvider(this._apiService) {
    loadAlerts();
  }

  List<AlertModel> get alerts => _alerts;
  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;
  String get selectedSeverity => _selectedSeverity;
  bool? get activeOnlyFilter => _activeOnlyFilter;

  void setSeverityFilter(String severity) {
    _selectedSeverity = severity;
    loadAlerts();
  }

  void setActiveFilter(bool? activeOnly) {
    _activeOnlyFilter = activeOnly;
    loadAlerts();
  }

  Future<void> loadAlerts() async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      _alerts = await _apiService.getAlerts(
        activeOnly: _activeOnlyFilter,
        severity: _selectedSeverity == 'ALL' ? null : _selectedSeverity,
      );
      _errorMessage = null;
    } catch (e) {
      _errorMessage = e.toString().replaceAll('Exception: ', '');
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> acknowledgeAlert(int alertId) async {
    try {
      final success = await _apiService.acknowledgeAlert(alertId, acknowledgedBy: 'Mobile Operator');
      if (success) {
        // Optimistic local state update
        _alerts = _alerts.map((a) {
          if (a.id == alertId) {
            return AlertModel(
              id: a.id,
              deviceId: a.deviceId,
              severity: a.severity,
              alertType: a.alertType,
              message: a.message,
              timestamp: a.timestamp,
              isAcknowledged: true,
              acknowledgedAt: DateTime.now(),
              acknowledgedBy: 'Mobile Operator',
            );
          }
          return a;
        }).toList();
        notifyListeners();
      }
      return success;
    } catch (_) {
      return false;
    }
  }
}
