using Microsoft.EntityFrameworkCore;
using RFMAS.Core.Entities;
using RFMAS.Core.Interfaces;

namespace RFMAS.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of ITelemetryRepository.
/// </summary>
public class TelemetryRepository : ITelemetryRepository
{
    private readonly RFMASDbContext _context;

    public TelemetryRepository(RFMASDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(TelemetryRecord record, CancellationToken cancellationToken = default)
    {
        await _context.TelemetryRecords.AddAsync(record, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<TelemetryRecord> records, CancellationToken cancellationToken = default)
    {
        await _context.TelemetryRecords.AddRangeAsync(records, cancellationToken);
    }

    public async Task<IReadOnlyList<TelemetryRecord>> GetRecentByDeviceAsync(string deviceId, int count = 50, CancellationToken cancellationToken = default)
    {
        return await _context.TelemetryRecords
            .Where(t => t.DeviceId == deviceId)
            .OrderByDescending(t => t.Timestamp)
            .Take(count)
            .OrderBy(t => t.Timestamp) // return in ascending chronological order for charts
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<TelemetryRecord?> GetLatestByDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        return await _context.TelemetryRecords
            .Where(t => t.DeviceId == deviceId)
            .OrderByDescending(t => t.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task PruneOldRecordsAsync(TimeSpan olderThan, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow - olderThan;
        var old = await _context.TelemetryRecords
            .Where(t => t.Timestamp < cutoff)
            .ToListAsync(cancellationToken);

        if (old.Count > 0)
        {
            _context.TelemetryRecords.RemoveRange(old);
        }
    }
}
