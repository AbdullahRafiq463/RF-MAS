namespace RFMAS.Core.Enums;

/// <summary>
/// Execution status of a device command.
/// </summary>
public enum CommandStatus
{
    PENDING,
    SUCCESS,
    FAILED,
    INVALID,
    TIMEOUT
}
