import 'dart:async';
import 'package:flutter/foundation.dart';
import '../models/dashboard_summary.dart';
import '../models/alert.dart';
import '../models/event_log.dart';
import '../services/api_service.dart';
import '../services/settings_service.dart';

class DashboardProvider extends ChangeNotifier {
  final ApiService _apiService;
  final SettingsService _settings;

  DashboardSummaryModel? _summary;
  List<AlertModel> _activeAlerts = [];
  List<EventLogModel> _recentActivity = [];
  bool _isLoading = false;
  String? _errorMessage;
  Timer? _pollingTimer;

  DashboardProvider(this._apiService, this._settings) {
    loadDashboard();
    _initPolling();
    _settings.addListener(_onSettingsChanged);
  }

  DashboardSummaryModel? get summary => _summary;
  List<AlertModel> get activeAlerts => _activeAlerts;
  List<EventLogModel> get recentActivity => _recentActivity;
  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;
  bool get hasError => _errorMessage != null;

  void _onSettingsChanged() {
    _initPolling();
    loadDashboard();
  }

  void _initPolling() {
    _pollingTimer?.cancel();
    if (_settings.autoRefreshEnabled) {
      _pollingTimer = Timer.periodic(
        Duration(seconds: _settings.refreshIntervalSeconds),
        (_) => refreshSilently(),
      );
    }
  }

  Future<void> loadDashboard() async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      final summaryFuture = _apiService.getDashboardSummary();
      final alertsFuture = _apiService.getAlerts(activeOnly: true);
      final logsFuture = _apiService.getLogs(limit: 10);

      final results = await Future.wait([summaryFuture, alertsFuture, logsFuture]);

      _summary = results[0] as DashboardSummaryModel;
      _activeAlerts = results[1] as List<AlertModel>;
      _recentActivity = results[2] as List<EventLogModel>;
      _errorMessage = null;
    } catch (e) {
      _errorMessage = e.toString().replaceAll('Exception: ', '');
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> refreshSilently() async {
    try {
      final summary = await _apiService.getDashboardSummary();
      final alerts = await _apiService.getAlerts(activeOnly: true);
      final logs = await _apiService.getLogs(limit: 10);

      _summary = summary;
      _activeAlerts = alerts;
      _recentActivity = logs;
      _errorMessage = null;
      notifyListeners();
    } catch (_) {
      // Don't flash error banners on periodic background ticks to keep UI smooth
    }
  }

  @override
  void dispose() {
    _pollingTimer?.cancel();
    _settings.removeListener(_onSettingsChanged);
    super.dispose();
  }
}
