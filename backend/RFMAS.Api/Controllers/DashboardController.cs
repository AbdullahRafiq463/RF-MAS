using Microsoft.AspNetCore.Mvc;
using RFMAS.Api.DTOs;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;
using RFMAS.Core.Models;

namespace RFMAS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Returns aggregated high-level telemetry and status metrics for the mobile dashboard.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var devices = await _unitOfWork.Devices.GetAllAsync(cancellationToken);
        var totalDevices = devices.Count;
        var online = devices.Count(d => d.OperationalStatus == DeviceStatus.ONLINE);
        var warning = devices.Count(d => d.OperationalStatus == DeviceStatus.WARNING);
        var error = devices.Count(d => d.OperationalStatus == DeviceStatus.ERROR);
        var offline = devices.Count(d => d.OperationalStatus == DeviceStatus.OFFLINE);
        var maintenance = devices.Count(d => d.OperationalStatus == DeviceStatus.MAINTENANCE);

        var activeAlerts = await _unitOfWork.Alerts.CountActiveAsync(cancellationToken);
        var criticalAlerts = await _unitOfWork.Alerts.CountActiveBySeverityAsync(AlertSeverity.CRITICAL, cancellationToken);
        var warningAlerts = await _unitOfWork.Alerts.CountActiveBySeverityAsync(AlertSeverity.WARNING, cancellationToken);

        var (totalTests, passed, failed, errored) = await _unitOfWork.Automation.GetSummaryStatsAsync(cancellationToken);

        var avgHealth = devices.Count > 0 ? devices.Average(d => d.HealthPercentage) : 100.0;

        var summary = new DashboardSummary
        {
            TotalDevices = totalDevices,
            OnlineDevices = online,
            WarningDevices = warning,
            ErrorDevices = error,
            OfflineDevices = offline,
            MaintenanceDevices = maintenance,
            TotalAlerts = activeAlerts,
            ActiveAlerts = activeAlerts,
            CriticalAlerts = criticalAlerts,
            WarningAlerts = warningAlerts,
            TestsExecuted = totalTests,
            TestsPassed = passed,
            TestsFailed = failed,
            TestsErrored = errored,
            AverageSystemHealth = avgHealth,
            ServerTimeUtc = DateTime.UtcNow
        };

        return Ok(DashboardSummaryDto.FromModel(summary));
    }
}
