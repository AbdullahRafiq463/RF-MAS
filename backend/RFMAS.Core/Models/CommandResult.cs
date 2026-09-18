using RFMAS.Core.Enums;

namespace RFMAS.Core.Models;

/// <summary>
/// Outcome of a device command execution.
/// </summary>
public record CommandResult
{
    public bool Success { get; init; }
    public CommandStatus Status { get; init; }
    public string Message { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
    public CommandType CommandType { get; init; }
    public DateTime ExecutedAt { get; init; } = DateTime.UtcNow;

    public static CommandResult Successful(string deviceId, CommandType type, string message) =>
        new() { Success = true, Status = CommandStatus.SUCCESS, DeviceId = deviceId, CommandType = type, Message = message };

    public static CommandResult Failed(string deviceId, CommandType type, string message) =>
        new() { Success = false, Status = CommandStatus.FAILED, DeviceId = deviceId, CommandType = type, Message = message };

    public static CommandResult Invalid(string deviceId, CommandType type, string message) =>
        new() { Success = false, Status = CommandStatus.INVALID, DeviceId = deviceId, CommandType = type, Message = message };

    public static CommandResult TimedOut(string deviceId, CommandType type, string message) =>
        new() { Success = false, Status = CommandStatus.TIMEOUT, DeviceId = deviceId, CommandType = type, Message = message };
}
