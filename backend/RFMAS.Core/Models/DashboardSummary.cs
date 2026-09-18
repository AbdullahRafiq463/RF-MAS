namespace RFMAS.Core.Models;

/// <summary>
/// Aggregated system-wide telemetry and operational summary for dashboard consumption.
/// </summary>
public record DashboardSummary
{
    public int TotalDevices { get; init; }
    public int OnlineDevices { get; init; }
    public int WarningDevices { get; init; }
    public int ErrorDevices { get; init; }
    public int OfflineDevices { get; init; }
    public int MaintenanceDevices { get; init; }

    public int TotalAlerts { get; init; }
    public int ActiveAlerts { get; init; }
    public int CriticalAlerts { get; init; }
    public int WarningAlerts { get; init; }

    public int TestsExecuted { get; init; }
    public int TestsPassed { get; init; }
    public int TestsFailed { get; init; }
    public int TestsErrored { get; init; }

    public double AverageSystemHealth { get; init; }
    public DateTime ServerTimeUtc { get; init; } = DateTime.UtcNow;
}
