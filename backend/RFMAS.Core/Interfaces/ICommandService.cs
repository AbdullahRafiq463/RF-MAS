using RFMAS.Core.Enums;
using RFMAS.Core.Models;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Orchestrates, validates, logs, and dispatches commands to RF devices.
/// </summary>
public interface ICommandService
{
    Task<CommandResult> ExecuteCommandAsync(string deviceId, CommandType type, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);
    Task<CommandResult> SetFrequencyAsync(string deviceId, double frequencyMHz, CancellationToken cancellationToken = default);
    Task<CommandResult> SetPowerAsync(string deviceId, double powerDbm, CancellationToken cancellationToken = default);
}
