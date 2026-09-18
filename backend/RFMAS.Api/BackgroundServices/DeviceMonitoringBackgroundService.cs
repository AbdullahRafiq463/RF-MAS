using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;

namespace RFMAS.Api.BackgroundServices;

/// <summary>
/// Background monitoring service that runs continuously as a hosted daemon in ASP.NET Core.
///
/// WHY THIS SERVICE EXISTS:
/// Real-time RF monitoring systems need to continuously ingest telemetry, evaluate safety thresholds,
/// detect device failures, and trigger alerts in the background without waiting for user HTTP requests.
///
/// HOW IT WORKS:
/// 1. Runs asynchronously as a singleton BackgroundService managed by the .NET Generic Host.
/// 2. Loops periodically (every 2.5 seconds) while observing the CancellationToken.
/// 3. Injects IServiceScopeFactory because EF Core's DbContext is scoped, while this background service is singleton.
/// 4. Collects simulated TCP telemetry from IDeviceCommunicationService.
/// 5. Evaluates threshold alerts, updates device health, records historical telemetry, and persists changes.
/// </summary>
public class DeviceMonitoringBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDeviceCommunicationService _deviceComm;
    private readonly ILogger<DeviceMonitoringBackgroundService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromMilliseconds(2500);

    public DeviceMonitoringBackgroundService(
        IServiceScopeFactory scopeFactory,
        IDeviceCommunicationService deviceComm,
        ILogger<DeviceMonitoringBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _deviceComm = deviceComm;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RF-MAS Background Device Monitoring Service starting up...");

        // Initial delay to let Web API finish initializing and seeding database
        await Task.Delay(1500, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful cancellation requested during shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in device monitoring cycle: {Message}", ex.Message);
            }

            try
            {
                await Task.Delay(_pollingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("RF-MAS Background Device Monitoring Service gracefully stopped.");
    }

    private async Task MonitorCycleAsync(CancellationToken stoppingToken)
    {
        // Create an explicit dependency injection scope to resolve scoped DbContext and repositories
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var alertEngine = scope.ServiceProvider.GetRequiredService<IAlertEngine>();
        var healthCalc = scope.ServiceProvider.GetRequiredService<IDeviceHealthCalculator>();

        var devices = await unitOfWork.Devices.GetAllAsync(stoppingToken);
        if (devices.Count == 0) return;

        var allTelemetry = await _deviceComm.ReceiveAllTelemetryAsync(stoppingToken);
        var telemetryDict = allTelemetry.ToDictionary(t => t.DeviceId);

        var telemetryRecordsToSave = new List<TelemetryRecord>();

        foreach (var device in devices)
        {
            var thresh = device.Thresholds ?? new ThresholdConfig();
            telemetryDict.TryGetValue(device.Id, out var telemetry);

            if (telemetry != null)
            {
                // Update live device entity metrics in database
                device.FrequencyMHz = telemetry.FrequencyMHz;
                device.SignalPowerDbm = telemetry.SignalPowerDbm;
                device.TemperatureC = telemetry.TemperatureC;
                device.VoltageV = telemetry.VoltageV;
                device.CurrentA = telemetry.CurrentA;
                device.OperationalStatus = telemetry.Status;
                device.ConnectionStatus = telemetry.Status != DeviceStatus.OFFLINE;
                device.LastCommunicationTime = telemetry.Timestamp;

                // Evaluate thresholds & raise deduplicated alerts
                await alertEngine.ProcessTelemetryAsync(device, telemetry, thresh, stoppingToken);

                // Compute updated device health score
                int activeAlertsCount = await unitOfWork.Alerts.CountActiveAsync(stoppingToken);
                var healthScore = healthCalc.Calculate(device, telemetry, thresh, activeAlertsCount);
                device.HealthPercentage = healthScore.TotalHealthPercentage;

                // Append telemetry record for historical charts
                telemetryRecordsToSave.Add(new TelemetryRecord
                {
                    DeviceId = device.Id,
                    FrequencyMHz = telemetry.FrequencyMHz,
                    SignalPowerDbm = telemetry.SignalPowerDbm,
                    TemperatureC = telemetry.TemperatureC,
                    VoltageV = telemetry.VoltageV,
                    CurrentA = telemetry.CurrentA,
                    Status = telemetry.Status,
                    Timestamp = telemetry.Timestamp
                });

                await unitOfWork.Devices.UpdateAsync(device, stoppingToken);
            }
            else
            {
                // Communication timeout detection
                if ((DateTime.UtcNow - device.LastCommunicationTime).TotalSeconds > thresh.TimeoutSeconds)
                {
                    device.OperationalStatus = DeviceStatus.WARNING;
                    device.HealthPercentage = Math.Max(20.0, device.HealthPercentage - 15.0);
                    await unitOfWork.Devices.UpdateAsync(device, stoppingToken);
                }
            }
        }

        if (telemetryRecordsToSave.Count > 0)
        {
            await unitOfWork.Telemetry.AddRangeAsync(telemetryRecordsToSave, stoppingToken);
        }

        await unitOfWork.SaveChangesAsync(stoppingToken);
    }
}
