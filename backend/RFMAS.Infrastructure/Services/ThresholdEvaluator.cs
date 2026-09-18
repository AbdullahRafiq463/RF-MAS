using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;
using RFMAS.Core.Models;

namespace RFMAS.Infrastructure.Services;

/// <summary>
/// Evaluates live telemetry values against configured thresholds to detect abnormal operating conditions.
/// </summary>
public class ThresholdEvaluator : IThresholdEvaluator
{
    public IReadOnlyList<Alert> Evaluate(Device device, TelemetryData telemetry, ThresholdConfig thresholds)
    {
        var alerts = new List<Alert>();

        if (telemetry.Status == DeviceStatus.OFFLINE)
        {
            alerts.Add(new Alert
            {
                DeviceId = device.Id,
                Severity = AlertSeverity.CRITICAL,
                AlertType = "DEVICE_OFFLINE",
                Message = $"Device {device.Id} ({device.Name}) is offline or disconnected.",
                Timestamp = DateTime.UtcNow
            });
            return alerts;
        }

        // Temperature evaluation
        if (telemetry.TemperatureC >= thresholds.TemperatureCritical)
        {
            alerts.Add(new Alert
            {
                DeviceId = device.Id,
                Severity = AlertSeverity.CRITICAL,
                AlertType = "TEMPERATURE_CRITICAL",
                Message = $"Critical temperature detected on {device.Id}: {telemetry.TemperatureC:F1}°C (Limit: {thresholds.TemperatureCritical:F1}°C).",
                Timestamp = DateTime.UtcNow
            });
        }
        else if (telemetry.TemperatureC >= thresholds.TemperatureWarning)
        {
            alerts.Add(new Alert
            {
                DeviceId = device.Id,
                Severity = AlertSeverity.WARNING,
                AlertType = "TEMPERATURE_WARNING",
                Message = $"High temperature warning on {device.Id}: {telemetry.TemperatureC:F1}°C (Warning threshold: {thresholds.TemperatureWarning:F1}°C).",
                Timestamp = DateTime.UtcNow
            });
        }

        // Signal Power evaluation
        if (telemetry.SignalPowerDbm <= thresholds.SignalPowerCritical)
        {
            alerts.Add(new Alert
            {
                DeviceId = device.Id,
                Severity = AlertSeverity.CRITICAL,
                AlertType = "SIGNAL_POWER_CRITICAL",
                Message = $"Critical RF power drop on {device.Id}: {telemetry.SignalPowerDbm:F1} dBm (Limit: {thresholds.SignalPowerCritical:F1} dBm).",
                Timestamp = DateTime.UtcNow
            });
        }
        else if (telemetry.SignalPowerDbm <= thresholds.SignalPowerWarning)
        {
            alerts.Add(new Alert
            {
                DeviceId = device.Id,
                Severity = AlertSeverity.WARNING,
                AlertType = "SIGNAL_POWER_WARNING",
                Message = $"Degraded RF power warning on {device.Id}: {telemetry.SignalPowerDbm:F1} dBm (Warning threshold: {thresholds.SignalPowerWarning:F1} dBm).",
                Timestamp = DateTime.UtcNow
            });
        }

        // Voltage evaluation
        if (telemetry.VoltageV < thresholds.VoltageMin || telemetry.VoltageV > thresholds.VoltageMax)
        {
            alerts.Add(new Alert
            {
                DeviceId = device.Id,
                Severity = AlertSeverity.WARNING,
                AlertType = "VOLTAGE_OUT_OF_RANGE",
                Message = $"Abnormal voltage on {device.Id}: {telemetry.VoltageV:F2}V (Nominal: {thresholds.VoltageMin:F1}V - {thresholds.VoltageMax:F1}V).",
                Timestamp = DateTime.UtcNow
            });
        }

        // Frequency operating range evaluation
        if (telemetry.FrequencyMHz < thresholds.FrequencyMin || telemetry.FrequencyMHz > thresholds.FrequencyMax)
        {
            alerts.Add(new Alert
            {
                DeviceId = device.Id,
                Severity = AlertSeverity.WARNING,
                AlertType = "FREQUENCY_OUT_OF_RANGE",
                Message = $"Frequency deviation on {device.Id}: {telemetry.FrequencyMHz:F2} MHz (Allowed: {thresholds.FrequencyMin:F1} - {thresholds.FrequencyMax:F1} MHz).",
                Timestamp = DateTime.UtcNow
            });
        }

        // Current draw evaluation
        if (telemetry.CurrentA > thresholds.CurrentMaxA)
        {
            alerts.Add(new Alert
            {
                DeviceId = device.Id,
                Severity = AlertSeverity.WARNING,
                AlertType = "OVERCURRENT_WARNING",
                Message = $"Overcurrent condition on {device.Id}: {telemetry.CurrentA:F2}A (Max: {thresholds.CurrentMaxA:F1}A).",
                Timestamp = DateTime.UtcNow
            });
        }

        return alerts;
    }
}
