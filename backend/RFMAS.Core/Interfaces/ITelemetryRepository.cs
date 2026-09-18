using RFMAS.Core.Entities;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Repository interface for Telemetry persistence and historical query.
/// </summary>
public interface ITelemetryRepository
{
    Task AddAsync(TelemetryRecord record, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TelemetryRecord> records, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TelemetryRecord>> GetRecentByDeviceAsync(string deviceId, int count = 50, CancellationToken cancellationToken = default);
    Task<TelemetryRecord?> GetLatestByDeviceAsync(string deviceId, CancellationToken cancellationToken = default);
    Task PruneOldRecordsAsync(TimeSpan olderThan, CancellationToken cancellationToken = default);
}
