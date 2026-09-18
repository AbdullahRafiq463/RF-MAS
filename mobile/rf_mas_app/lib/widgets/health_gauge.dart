import 'package:flutter/material.dart';
import '../theme/app_colors.dart';

class HealthGauge extends StatelessWidget {
  final double percentage;
  final double size;

  const HealthGauge({
    super.key,
    required this.percentage,
    this.size = 56,
  });

  Color _getColor(double p) {
    if (p >= 90) return AppColors.onlineGreen;
    if (p >= 75) return AppColors.warningAmber;
    if (p > 0) return AppColors.criticalRed;
    return AppColors.offlineGrey;
  }

  @override
  Widget build(BuildContext context) {
    final color = _getColor(percentage);
    final clamped = (percentage / 100.0).clamp(0.0, 1.0);

    return SizedBox(
      width: size,
      height: size,
      child: Stack(
        alignment: Alignment.center,
        children: [
          CircularProgressIndicator(
            value: clamped,
            strokeWidth: 4,
            backgroundColor: AppColors.darkBorder,
            valueColor: AlwaysStoppedAnimation<Color>(color),
          ),
          Text(
            '${percentage.toStringAsFixed(0)}%',
            style: TextStyle(
              color: color,
              fontSize: size * 0.28,
              fontWeight: FontWeight.bold,
            ),
          ),
        ],
      ),
    );
  }
}
