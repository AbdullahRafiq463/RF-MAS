using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;

namespace RFMAS.Infrastructure.Services;

/// <summary>
/// Centralized audit, command, alarm, and system event logging service.
/// </summary>
public class LogService : ILogService
{
    private readonly IUnitOfWork _unitOfWork;

    public LogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task LogAsync(
        string? deviceId,
        string category,
        string eventName,
        string details,
        AlertSeverity severity = AlertSeverity.INFO,
        CancellationToken cancellationToken = default)
    {
        var log = new EventLog
        {
            DeviceId = deviceId,
            Category = category,
            EventName = eventName,
            Details = details,
            Severity = severity,
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.Logs.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EventLog>> GetLogsAsync(
        string? deviceId = null,
        string? category = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Logs.GetLogsAsync(deviceId, category, limit, cancellationToken);
    }
}
