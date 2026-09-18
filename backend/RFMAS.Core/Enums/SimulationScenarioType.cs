namespace RFMAS.Core.Enums;

/// <summary>
/// Scenarios that can be injected into the RF device simulation engine for testing and demonstrations.
/// </summary>
public enum SimulationScenarioType
{
    NORMAL,
    HIGH_TEMPERATURE,
    LOW_SIGNAL,
    LOW_VOLTAGE,
    DISCONNECT,
    COMMUNICATION_TIMEOUT,
    ERROR
}
