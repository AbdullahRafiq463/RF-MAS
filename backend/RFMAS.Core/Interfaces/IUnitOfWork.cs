namespace RFMAS.Core.Interfaces;

/// <summary>
/// Unit of Work pattern interface for committing database transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IDeviceRepository Devices { get; }
    ITelemetryRepository Telemetry { get; }
    IAlertRepository Alerts { get; }
    IDeviceCommandRepository Commands { get; }
    IEventLogRepository Logs { get; }
    IAutomationRepository Automation { get; }
    IThresholdRepository Thresholds { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
