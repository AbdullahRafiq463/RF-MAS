using RFMAS.Core.Entities;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Repository interface for EventLog persistence.
/// </summary>
public interface IEventLogRepository
{
    Task AddAsync(EventLog log, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventLog>> GetLogsAsync(string? deviceId = null, string? category = null, int limit = 100, CancellationToken cancellationToken = default);
}
