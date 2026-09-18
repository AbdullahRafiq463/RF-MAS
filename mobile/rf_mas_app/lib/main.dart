import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'services/settings_service.dart';
import 'services/api_service.dart';
import 'providers/dashboard_provider.dart';
import 'providers/devices_provider.dart';
import 'providers/alerts_provider.dart';
import 'providers/automation_provider.dart';
import 'providers/logs_provider.dart';
import 'screens/main_navigation_screen.dart';
import 'theme/app_theme.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  final settingsService = SettingsService();
  await settingsService.loadSettings();

  final apiService = ApiService(settingsService);

  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider.value(value: settingsService),
        Provider.value(value: apiService),
        ChangeNotifierProvider(create: (_) => DashboardProvider(apiService, settingsService)),
        ChangeNotifierProvider(create: (_) => DevicesProvider(apiService, settingsService)),
        ChangeNotifierProvider(create: (_) => AlertsProvider(apiService)),
        ChangeNotifierProvider(create: (_) => AutomationProvider(apiService)),
        ChangeNotifierProvider(create: (_) => LogsProvider(apiService)),
      ],
      child: const RFMASApp(),
    ),
  );
}

class RFMASApp extends StatelessWidget {
  const RFMASApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'RF Device Monitoring & Automation System (RF-MAS)',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.darkTheme,
      home: const MainNavigationScreen(),
    );
  }
}
