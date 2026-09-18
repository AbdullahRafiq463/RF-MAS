using RFMAS.Core.Entities;
using RFMAS.Core.Interfaces;
using RFMAS.Core.Models;

namespace RFMAS.Infrastructure.Services;

/// <summary>
/// Core alert processing engine that continuously inspects incoming telemetry,
/// evaluates threshold violations, deduplicates active alerts, and persists new alarms.
/// </summary>
public class AlertEngine : IAlertEngine
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IThresholdEvaluator _thresholdEvaluator;
    private readonly ILogService _logService;

    public AlertEngine(
        IUnitOfWork unitOfWork,
        IThresholdEvaluator thresholdEvaluator,
        ILogService logService)
    {
        _unitOfWork = unitOfWork;
        _thresholdEvaluator = thresholdEvaluator;
        _logService = logService;
    }

    public async Task<IReadOnlyList<Alert>> ProcessTelemetryAsync(
        Device device,
        TelemetryData telemetry,
        ThresholdConfig thresholds,
        CancellationToken cancellationToken = default)
    {
        var evaluatedAlerts = _thresholdEvaluator.Evaluate(device, telemetry, thresholds);
        var newlyRaisedAlerts = new List<Alert>();

        foreach (var candidate in evaluatedAlerts)
        {
            // Deduplication: Only create a new alert if an unacknowledged alert of this type doesn't already exist for this device
            bool alreadyActive = await _unitOfWork.Alerts.HasActiveAlertOfTypeAsync(device.Id, candidate.AlertType, cancellationToken);
            if (!alreadyActive)
            {
                await _unitOfWork.Alerts.AddAsync(candidate, cancellationToken);
                newlyRaisedAlerts.Add(candidate);

                // Audit log the alert
                await _logService.LogAsync(
                    deviceId: device.Id,
                    category: "ALERT",
                    eventName: candidate.AlertType,
                    details: candidate.Message,
                    severity: candidate.Severity,
                    cancellationToken: cancellationToken);
            }
        }

        if (newlyRaisedAlerts.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return newlyRaisedAlerts;
    }

    public async Task<bool> AcknowledgeAlertAsync(long alertId, string? acknowledgedBy = null, CancellationToken cancellationToken = default)
    {
        var alert = await _unitOfWork.Alerts.GetByIdAsync(alertId, cancellationToken);
        if (alert == null || alert.IsAcknowledged)
        {
            return false;
        }

        alert.IsAcknowledged = true;
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.AcknowledgedBy = acknowledgedBy ?? "Operator";

        await _unitOfWork.Alerts.UpdateAsync(alert, cancellationToken);

        await _logService.LogAsync(
            deviceId: alert.DeviceId,
            category: "ALERT",
            eventName: "ALERT_ACKNOWLEDGED",
            details: $"Alert #{alertId} ({alert.AlertType}) acknowledged by {alert.AcknowledgedBy}",
            severity: Core.Enums.AlertSeverity.INFO,
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
