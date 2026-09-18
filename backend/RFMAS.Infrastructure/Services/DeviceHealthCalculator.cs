using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;
using RFMAS.Core.Models;

namespace RFMAS.Infrastructure.Services;

/// <summary>
/// Computes comprehensive health indices (0-100%) for simulated RF instruments.
/// Integrates thermal headroom, voltage regulation, RF signal stability, connectivity, and active alarm penalties.
/// </summary>
public class DeviceHealthCalculator : IDeviceHealthCalculator
{
    public DeviceHealthScore Calculate(Device device, TelemetryData? telemetry, ThresholdConfig? thresholds, int activeAlertsCount)
    {
        if (telemetry == null || telemetry.Status == DeviceStatus.OFFLINE)
        {
            return new DeviceHealthScore
            {
                DeviceId = device.Id,
                TotalHealthPercentage = 0.0,
                TemperatureScore = 0.0,
                VoltageScore = 0.0,
                SignalPowerScore = 0.0,
                ConnectivityScore = 0.0,
                AlertPenaltyScore = 0.0,
                Assessment = "OFFLINE"
            };
        }

        var thresh = thresholds ?? new ThresholdConfig();

        // 1. Temperature score (Ideal: ~40°C, Warning: 60°C, Critical: 75°C)
        double tempScore = 100.0;
        if (telemetry.TemperatureC > 50.0)
        {
            double tempExcess = telemetry.TemperatureC - 50.0;
            double tempRange = Math.Max(1.0, thresh.TemperatureCritical - 50.0);
            tempScore = Math.Max(0.0, 100.0 - (tempExcess / tempRange) * 100.0);
        }

        // 2. Voltage stability score (Nominal ~12.0V)
        double nominalVoltage = (thresh.VoltageMin + thresh.VoltageMax) / 2.0;
        double voltDiff = Math.Abs(telemetry.VoltageV - nominalVoltage);
        double maxAllowedDiff = Math.Max(0.5, (thresh.VoltageMax - thresh.VoltageMin) / 2.0);
        double voltScore = Math.Max(0.0, 100.0 - (voltDiff / maxAllowedDiff) * 50.0);

        // 3. Signal Power score (Above -50 dBm is 100%, drops down to -70 dBm)
        double powerScore = 100.0;
        if (telemetry.SignalPowerDbm < thresh.SignalPowerWarning)
        {
            double drop = thresh.SignalPowerWarning - telemetry.SignalPowerDbm;
            double span = Math.Max(1.0, thresh.SignalPowerWarning - thresh.SignalPowerCritical);
            powerScore = Math.Max(0.0, 100.0 - (drop / span) * 100.0);
        }

        // 4. Connectivity score
        double connScore = telemetry.Status == DeviceStatus.ONLINE ? 100.0 : (telemetry.Status == DeviceStatus.WARNING ? 75.0 : 40.0);

        // 5. Alert penalties
        double alertPenalty = activeAlertsCount * 12.0;

        // Weighted sum
        // Weights: Temperature (30%), Signal Power (30%), Voltage (20%), Connectivity (20%)
        double rawHealth = (tempScore * 0.30) + (powerScore * 0.30) + (voltScore * 0.20) + (connScore * 0.20);
        double finalHealth = Math.Clamp(Math.Round(rawHealth - alertPenalty, 1), 0.0, 100.0);

        string assessment = finalHealth switch
        {
            >= 90.0 => "EXCELLENT",
            >= 75.0 => "GOOD",
            >= 50.0 => "DEGRADED",
            > 0.0 => "CRITICAL",
            _ => "OFFLINE"
        };

        return new DeviceHealthScore
        {
            DeviceId = device.Id,
            TotalHealthPercentage = finalHealth,
            TemperatureScore = Math.Round(tempScore, 1),
            VoltageScore = Math.Round(voltScore, 1),
            SignalPowerScore = Math.Round(powerScore, 1),
            ConnectivityScore = Math.Round(connScore, 1),
            AlertPenaltyScore = Math.Round(alertPenalty, 1),
            Assessment = assessment
        };
    }
}
