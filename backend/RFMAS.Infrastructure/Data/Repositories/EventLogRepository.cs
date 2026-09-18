using Microsoft.EntityFrameworkCore;
using RFMAS.Core.Entities;
using RFMAS.Core.Interfaces;

namespace RFMAS.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of IEventLogRepository.
/// </summary>
public class EventLogRepository : IEventLogRepository
{
    private readonly RFMASDbContext _context;

    public EventLogRepository(RFMASDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EventLog log, CancellationToken cancellationToken = default)
    {
        await _context.EventLogs.AddAsync(log, cancellationToken);
    }

    public async Task<IReadOnlyList<EventLog>> GetLogsAsync(string? deviceId = null, string? category = null, int limit = 100, CancellationToken cancellationToken = default)
    {
        var query = _context.EventLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(deviceId))
        {
            query = query.Where(l => l.DeviceId == deviceId);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(l => l.Category == category);
        }

        return await query
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
