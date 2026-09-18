using Microsoft.EntityFrameworkCore;
using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;

namespace RFMAS.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of IAlertRepository.
/// </summary>
public class AlertRepository : IAlertRepository
{
    private readonly RFMASDbContext _context;

    public AlertRepository(RFMASDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        await _context.Alerts.AddAsync(alert, cancellationToken);
    }

    public async Task<IReadOnlyList<Alert>> GetAllAsync(bool? activeOnly = null, AlertSeverity? severity = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Alerts.AsQueryable();

        if (activeOnly.HasValue)
        {
            query = query.Where(a => a.IsAcknowledged == !activeOnly.Value);
        }

        if (severity.HasValue)
        {
            query = query.Where(a => a.Severity == severity.Value);
        }

        return await query
            .OrderByDescending(a => a.Timestamp)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Alert>> GetRecentByDeviceAsync(string deviceId, int count = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Alerts
            .Where(a => a.DeviceId == deviceId)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Alert?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Alerts.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<bool> HasActiveAlertOfTypeAsync(string deviceId, string alertType, CancellationToken cancellationToken = default)
    {
        return await _context.Alerts
            .AnyAsync(a => a.DeviceId == deviceId && a.AlertType == alertType && !a.IsAcknowledged, cancellationToken);
    }

    public Task UpdateAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        _context.Alerts.Update(alert);
        return Task.CompletedTask;
    }

    public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Alerts.CountAsync(a => !a.IsAcknowledged, cancellationToken);
    }

    public async Task<int> CountActiveBySeverityAsync(AlertSeverity severity, CancellationToken cancellationToken = default)
    {
        return await _context.Alerts.CountAsync(a => !a.IsAcknowledged && a.Severity == severity, cancellationToken);
    }
}
