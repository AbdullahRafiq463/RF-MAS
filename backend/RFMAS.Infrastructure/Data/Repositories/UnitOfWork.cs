using RFMAS.Core.Interfaces;

namespace RFMAS.Infrastructure.Data.Repositories;

/// <summary>
/// Coordinates work across multiple repository instances within a single database transaction context.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly RFMASDbContext _context;

    public UnitOfWork(RFMASDbContext context)
    {
        _context = context;
        Devices = new DeviceRepository(_context);
        Telemetry = new TelemetryRepository(_context);
        Alerts = new AlertRepository(_context);
        Commands = new DeviceCommandRepository(_context);
        Logs = new EventLogRepository(_context);
        Automation = new AutomationRepository(_context);
        Thresholds = new ThresholdRepository(_context);
    }

    public IDeviceRepository Devices { get; }
    public ITelemetryRepository Telemetry { get; }
    public IAlertRepository Alerts { get; }
    public IDeviceCommandRepository Commands { get; }
    public IEventLogRepository Logs { get; }
    public IAutomationRepository Automation { get; }
    public IThresholdRepository Thresholds { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
