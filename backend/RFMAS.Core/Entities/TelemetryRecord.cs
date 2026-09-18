using RFMAS.Core.Enums;

namespace RFMAS.Core.Entities;

/// <summary>
/// Historical telemetry sample captured from a device.
/// </summary>
public class TelemetryRecord
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public double FrequencyMHz { get; set; }
    public double SignalPowerDbm { get; set; }
    public double TemperatureC { get; set; }
    public double VoltageV { get; set; }
    public double CurrentA { get; set; }
    public DeviceStatus Status { get; set; } = DeviceStatus.ONLINE;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Device? Device { get; set; }
}
