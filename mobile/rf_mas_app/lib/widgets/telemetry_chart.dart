import 'package:fl_chart/fl_chart.dart';
import 'package:flutter/material.dart';
import '../models/telemetry.dart';
import '../theme/app_colors.dart';

class TelemetryChart extends StatelessWidget {
  final String title;
  final String unit;
  final Color lineColor;
  final List<TelemetryModel> telemetryData;
  final double Function(TelemetryModel) valueSelector;

  const TelemetryChart({
    super.key,
    required this.title,
    required this.unit,
    required this.lineColor,
    required this.telemetryData,
    required this.valueSelector,
  });

  @override
  Widget build(BuildContext context) {
    if (telemetryData.isEmpty) {
      return Container(
        height: 180,
        alignment: Alignment.center,
        child: const Text(
          'Awaiting telemetry stream...',
          style: TextStyle(color: Colors.white38, fontSize: 12),
        ),
      );
    }

    final spots = <FlSpot>[];
    for (int i = 0; i < telemetryData.length; i++) {
      spots.add(FlSpot(i.toDouble(), valueSelector(telemetryData[i])));
    }

    final values = spots.map((s) => s.y).toList();
    final minY = values.reduce((a, b) => a < b ? a : b);
    final maxY = values.reduce((a, b) => a > b ? a : b);
    final padding = (maxY - minY).abs() * 0.15;
    final chartMinY = (minY - padding).isFinite ? minY - padding : 0.0;
    final chartMaxY = (maxY + padding).isFinite && (maxY + padding) > chartMinY ? maxY + padding : chartMinY + 10.0;

    final latestVal = values.isNotEmpty ? values.last : 0.0;

    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppColors.darkCard,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.darkBorder),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                title.toUpperCase(),
                style: const TextStyle(
                  color: Colors.white70,
                  fontSize: 12,
                  fontWeight: FontWeight.w600,
                  letterSpacing: 0.5,
                ),
              ),
              Row(
                children: [
                  Text(
                    latestVal.toStringAsFixed(2),
                    style: TextStyle(
                      color: lineColor,
                      fontSize: 15,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(width: 4),
                  Text(
                    unit,
                    style: const TextStyle(color: Colors.white54, fontSize: 11),
                  ),
                ],
              ),
            ],
          ),
          const SizedBox(height: 16),
          SizedBox(
            height: 140,
            child: LineChart(
              LineChartData(
                minY: chartMinY,
                maxY: chartMaxY,
                gridData: const FlGridData(
                  show: true,
                  drawVerticalLine: false,
                  getDrawingHorizontalLine: _getHorizontalLine,
                ),
                titlesData: const FlTitlesData(show: false),
                borderData: FlBorderData(show: false),
                lineBarsData: [
                  LineChartBarData(
                    spots: spots,
                    isCurved: true,
                    curveSmoothness: 0.25,
                    color: lineColor,
                    barWidth: 2,
                    isStrokeCapRound: true,
                    dotData: const FlDotData(show: false),
                    belowBarData: BarAreaData(
                      show: true,
                      gradient: LinearGradient(
                        colors: [
                          lineColor.withValues(alpha: 0.25),
                          lineColor.withValues(alpha: 0.0),
                        ],
                        begin: Alignment.topCenter,
                        end: Alignment.bottomCenter,
                      ),
                    ),
                  ),
                ],
                lineTouchData: LineTouchData(
                  touchTooltipData: LineTouchTooltipData(
                    getTooltipColor: (_) => AppColors.darkSurface,
                    getTooltipItems: (touchedSpots) {
                      return touchedSpots.map((spot) {
                        return LineTooltipItem(
                          '${spot.y.toStringAsFixed(2)} $unit',
                          TextStyle(color: lineColor, fontWeight: FontWeight.bold, fontSize: 11),
                        );
                      }).toList();
                    },
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  static FlLine _getHorizontalLine(double value) {
    return const FlLine(
      color: AppColors.darkBorder,
      strokeWidth: 0.8,
    );
  }
}
