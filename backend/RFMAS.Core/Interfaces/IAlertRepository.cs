using RFMAS.Core.Entities;
using RFMAS.Core.Enums;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Repository interface for Alerts.
/// </summary>
public interface IAlertRepository
{
    Task AddAsync(Alert alert, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Alert>> GetAllAsync(bool? activeOnly = null, AlertSeverity? severity = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Alert>> GetRecentByDeviceAsync(string deviceId, int count = 20, CancellationToken cancellationToken = default);
    Task<Alert?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> HasActiveAlertOfTypeAsync(string deviceId, string alertType, CancellationToken cancellationToken = default);
    Task UpdateAsync(Alert alert, CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task<int> CountActiveBySeverityAsync(AlertSeverity severity, CancellationToken cancellationToken = default);
}
