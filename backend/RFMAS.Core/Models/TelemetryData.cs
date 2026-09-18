using RFMAS.Core.Enums;

namespace RFMAS.Core.Models;

/// <summary>
/// Telemetry data contract passed across the monitoring pipeline.
/// </summary>
public record TelemetryData
{
    public string DeviceId { get; init; } = string.Empty;
    public double FrequencyMHz { get; init; }
    public double SignalPowerDbm { get; init; }
    public double TemperatureC { get; init; }
    public double VoltageV { get; init; }
    public double CurrentA { get; init; }
    public DeviceStatus Status { get; init; } = DeviceStatus.ONLINE;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
