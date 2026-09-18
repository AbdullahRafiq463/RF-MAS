using RFMAS.Core.Enums;

namespace RFMAS.Core.Entities;

/// <summary>
/// Record of commands issued to RF devices and their execution result.
/// </summary>
public class DeviceCommand
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public CommandType CommandType { get; set; }
    public string? ParametersJson { get; set; }
    public CommandStatus Status { get; set; } = CommandStatus.PENDING;
    public string? ResultMessage { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExecutedAt { get; set; }

    // Navigation property
    public Device? Device { get; set; }
}
