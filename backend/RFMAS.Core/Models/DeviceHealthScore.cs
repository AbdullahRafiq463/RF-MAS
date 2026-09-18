namespace RFMAS.Core.Models;

/// <summary>
/// Detailed breakdown of calculated device health metrics.
/// </summary>
public record DeviceHealthScore
{
    public string DeviceId { get; init; } = string.Empty;
    public double TotalHealthPercentage { get; init; }
    public double TemperatureScore { get; init; }
    public double VoltageScore { get; init; }
    public double SignalPowerScore { get; init; }
    public double ConnectivityScore { get; init; }
    public double AlertPenaltyScore { get; init; }
    public string Assessment { get; init; } = "EXCELLENT";
}
