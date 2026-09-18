using RFMAS.Core.Enums;

namespace RFMAS.Core.Models;

/// <summary>
/// Request payload to inject a specific simulation scenario into a device.
/// </summary>
public record SimulationScenarioRequest
{
    public SimulationScenarioType Scenario { get; init; } = SimulationScenarioType.NORMAL;
    public double? CustomValue { get; init; }
    public string? Reason { get; init; }
}
