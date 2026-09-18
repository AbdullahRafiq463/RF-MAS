using RFMAS.Core.Enums;

namespace RFMAS.Api.DTOs;

public record InjectScenarioDto
{
    public SimulationScenarioType Scenario { get; init; } = SimulationScenarioType.NORMAL;
    public double? CustomValue { get; init; }
    public string? Reason { get; init; }
}

public record SimulationDeviceStatusDto
{
    public string DeviceId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string ActiveScenario { get; init; } = string.Empty;
    public bool IsConnected { get; init; }
    public string Status { get; init; } = string.Empty;
}
