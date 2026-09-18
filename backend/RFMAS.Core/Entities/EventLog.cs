using RFMAS.Core.Enums;

namespace RFMAS.Core.Entities;

/// <summary>
/// System event and audit log record.
/// </summary>
public class EventLog
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? DeviceId { get; set; }
    public string Category { get; set; } = "SYSTEM"; // SYSTEM, DEVICE, TELEMETRY, COMMAND, ALERT, AUTOMATION
    public string EventName { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; } = AlertSeverity.INFO;

    // Navigation property
    public Device? Device { get; set; }
}
