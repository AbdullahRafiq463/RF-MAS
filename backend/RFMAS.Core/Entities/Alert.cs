using RFMAS.Core.Enums;

namespace RFMAS.Core.Entities;

/// <summary>
/// Represents an alert triggered by abnormal telemetry or device state.
/// </summary>
public class Alert
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; } = AlertSeverity.WARNING;
    public string AlertType { get; set; } = string.Empty; // e.g., HIGH_TEMPERATURE, LOW_SIGNAL_POWER, VOLTAGE_OUT_OF_RANGE, DISCONNECTED
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsAcknowledged { get; set; } = false;
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }

    // Navigation property
    public Device? Device { get; set; }
}
