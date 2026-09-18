using RFMAS.Core.Entities;
using RFMAS.Core.Models;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Evaluates incoming telemetry against safety thresholds to determine anomalies and state transitions.
/// </summary>
public interface IThresholdEvaluator
{
    IReadOnlyList<Alert> Evaluate(Device device, TelemetryData telemetry, ThresholdConfig thresholds);
}
