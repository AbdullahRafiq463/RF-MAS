import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/alerts_provider.dart';
import '../theme/app_colors.dart';
import 'dashboard_screen.dart';
import 'devices_screen.dart';
import 'alerts_screen.dart';
import 'automation_screen.dart';
import 'logs_screen.dart';

class MainNavigationScreen extends StatefulWidget {
  const MainNavigationScreen({super.key});

  @override
  State<MainNavigationScreen> createState() => _MainNavigationScreenState();
}

class _MainNavigationScreenState extends State<MainNavigationScreen> {
  int _currentIndex = 0;

  final List<Widget> _screens = const [
    DashboardScreen(),
    DevicesScreen(),
    AlertsScreen(),
    AutomationScreen(),
    LogsScreen(),
  ];

  @override
  Widget build(BuildContext context) {
    final activeAlertsCount = context.watch<AlertsProvider>().alerts.where((a) => !a.isAcknowledged).length;

    return Scaffold(
      body: IndexedStack(
        index: _currentIndex,
        children: _screens,
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _currentIndex,
        onTap: (index) {
          setState(() {
            _currentIndex = index;
          });
        },
        items: [
          const BottomNavigationBarItem(
            icon: Icon(Icons.dashboard_outlined),
            activeIcon: Icon(Icons.dashboard),
            label: 'Dashboard',
          ),
          const BottomNavigationBarItem(
            icon: Icon(Icons.developer_board_outlined),
            activeIcon: Icon(Icons.developer_board),
            label: 'Instruments',
          ),
          BottomNavigationBarItem(
            icon: Badge(
              isLabelVisible: activeAlertsCount > 0,
              label: Text('$activeAlertsCount'),
              backgroundColor: AppColors.criticalRed,
              child: const Icon(Icons.warning_amber_outlined),
            ),
            activeIcon: Badge(
              isLabelVisible: activeAlertsCount > 0,
              label: Text('$activeAlertsCount'),
              backgroundColor: AppColors.criticalRed,
              child: const Icon(Icons.warning),
            ),
            label: 'Alerts',
          ),
          const BottomNavigationBarItem(
            icon: Icon(Icons.fact_check_outlined),
            activeIcon: Icon(Icons.fact_check),
            label: 'Automation',
          ),
          const BottomNavigationBarItem(
            icon: Icon(Icons.list_alt_outlined),
            activeIcon: Icon(Icons.list_alt),
            label: 'Logs',
          ),
        ],
      ),
    );
  }
}
