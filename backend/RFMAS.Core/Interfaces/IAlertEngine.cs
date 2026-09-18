using RFMAS.Core.Entities;
using RFMAS.Core.Models;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Core engine for inspecting incoming telemetry streams, deduplicating alerts, and raising new notifications.
/// </summary>
public interface IAlertEngine
{
    Task<IReadOnlyList<Alert>> ProcessTelemetryAsync(Device device, TelemetryData telemetry, ThresholdConfig thresholds, CancellationToken cancellationToken = default);
    Task<bool> AcknowledgeAlertAsync(long alertId, string? acknowledgedBy = null, CancellationToken cancellationToken = default);
}
