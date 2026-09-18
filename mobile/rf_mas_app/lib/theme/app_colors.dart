import 'package:flutter/material.dart';

/// Semantic engineering color palette for RF-MAS.
class AppColors {
  // Backgrounds & Surfaces (Industrial Dark Mode)
  static const Color darkBackground = Color(0xFF0D1117);
  static const Color darkSurface = Color(0xFF161B22);
  static const Color darkCard = Color(0xFF21262D);
  static const Color darkBorder = Color(0xFF30363D);

  // Brand & Accents
  static const Color primaryTeal = Color(0xFF00E5FF);
  static const Color primaryBlue = Color(0xFF2979FF);
  static const Color accentCyan = Color(0xFF18FFFF);

  // Status & Telemetry Semantic Colors
  static const Color onlineGreen = Color(0xFF00E676);
  static const Color warningAmber = Color(0xFFFFB300);
  static const Color criticalRed = Color(0xFFFF5252);
  static const Color offlineGrey = Color(0xFF8B949E);
  static const Color maintenancePurple = Color(0xFFB388FF);

  // Metric Specific Accent Colors
  static const Color temperatureOrange = Color(0xFFFF7043);
  static const Color powerBlue = Color(0xFF40C4FF);
  static const Color frequencyGreen = Color(0xFF69F0AE);
  static const Color voltageYellow = Color(0xFFFFD54F);
  static const Color currentPink = Color(0xFFFF4081);

  static Color getStatusColor(String status) {
    switch (status.toUpperCase()) {
      case 'ONLINE':
        return onlineGreen;
      case 'WARNING':
        return warningAmber;
      case 'ERROR':
      case 'CRITICAL':
        return criticalRed;
      case 'MAINTENANCE':
        return maintenancePurple;
      case 'OFFLINE':
      default:
        return offlineGrey;
    }
  }

  static Color getSeverityColor(String severity) {
    switch (severity.toUpperCase()) {
      case 'CRITICAL':
        return criticalRed;
      case 'WARNING':
        return warningAmber;
      case 'INFO':
      default:
        return primaryBlue;
    }
  }
}
