using RFMAS.Core.Models;

namespace RFMAS.Api.DTOs;

public record DashboardSummaryDto
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
    public DateTime ServerTimeUtc { get; init; }

    public static DashboardSummaryDto FromModel(DashboardSummary m) => new()
    {
        TotalDevices = m.TotalDevices,
        OnlineDevices = m.OnlineDevices,
        WarningDevices = m.WarningDevices,
        ErrorDevices = m.ErrorDevices,
        OfflineDevices = m.OfflineDevices,
        MaintenanceDevices = m.MaintenanceDevices,
        TotalAlerts = m.TotalAlerts,
        ActiveAlerts = m.ActiveAlerts,
        CriticalAlerts = m.CriticalAlerts,
        WarningAlerts = m.WarningAlerts,
        TestsExecuted = m.TestsExecuted,
        TestsPassed = m.TestsPassed,
        TestsFailed = m.TestsFailed,
        TestsErrored = m.TestsErrored,
        AverageSystemHealth = Math.Round(m.AverageSystemHealth, 1),
        ServerTimeUtc = m.ServerTimeUtc
    };
}
