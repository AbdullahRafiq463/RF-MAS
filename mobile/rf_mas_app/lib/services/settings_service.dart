import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Manages local client application configuration persisted in SharedPreferences.
class SettingsService extends ChangeNotifier {
  static const String keyBaseUrl = 'rfmas_base_url';
  static const String keyRefreshIntervalSeconds = 'rfmas_refresh_interval';
  static const String keyAutoRefreshEnabled = 'rfmas_auto_refresh';

  static const String defaultEmulatorUrl = 'http://10.0.2.2:5000';
  static const String defaultLocalUrl = 'http://localhost:5000';

  static String get platformDefaultUrl {
    if (kIsWeb) return defaultLocalUrl;
    if (defaultTargetPlatform == TargetPlatform.android) {
      return defaultEmulatorUrl;
    }
    return defaultLocalUrl;
  }

  String _baseUrl = platformDefaultUrl;
  int _refreshIntervalSeconds = 4;
  bool _autoRefreshEnabled = true;

  String get baseUrl => _baseUrl;
  int get refreshIntervalSeconds => _refreshIntervalSeconds;
  bool get autoRefreshEnabled => _autoRefreshEnabled;

  Future<void> loadSettings() async {
    final prefs = await SharedPreferences.getInstance();
    _baseUrl = prefs.getString(keyBaseUrl) ?? platformDefaultUrl;
    _refreshIntervalSeconds = prefs.getInt(keyRefreshIntervalSeconds) ?? 4;
    _autoRefreshEnabled = prefs.getBool(keyAutoRefreshEnabled) ?? true;
    notifyListeners();
  }

  Future<void> setBaseUrl(String newUrl) async {
    var sanitized = newUrl.trim();
    if (sanitized.endsWith('/')) {
      sanitized = sanitized.substring(0, sanitized.length - 1);
    }
    if (!sanitized.startsWith('http://') && !sanitized.startsWith('https://')) {
      sanitized = 'http://$sanitized';
    }
    _baseUrl = sanitized;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(keyBaseUrl, _baseUrl);
    notifyListeners();
  }

  Future<void> setRefreshInterval(int seconds) async {
    _refreshIntervalSeconds = seconds.clamp(2, 30);
    final prefs = await SharedPreferences.getInstance();
    await prefs.setInt(keyRefreshIntervalSeconds, _refreshIntervalSeconds);
    notifyListeners();
  }

  Future<void> setAutoRefresh(bool enabled) async {
    _autoRefreshEnabled = enabled;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(keyAutoRefreshEnabled, _autoRefreshEnabled);
    notifyListeners();
  }
}
