using RFMAS.Core.Entities;
using RFMAS.Core.Enums;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Centralized audit and operational event logging service.
/// </summary>
public interface ILogService
{
    Task LogAsync(string? deviceId, string category, string eventName, string details, AlertSeverity severity = AlertSeverity.INFO, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventLog>> GetLogsAsync(string? deviceId = null, string? category = null, int limit = 100, CancellationToken cancellationToken = default);
}
