using RFMAS.Core.Entities;
using RFMAS.Core.Models;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Calculates device health percentage (0-100%) and detailed component scores based on telemetry and operational alerts.
/// </summary>
public interface IDeviceHealthCalculator
{
    DeviceHealthScore Calculate(Device device, TelemetryData? telemetry, ThresholdConfig? thresholds, int activeAlertsCount);
}
