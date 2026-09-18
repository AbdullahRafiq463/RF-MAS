using System.Globalization;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;
using RFMAS.Core.Models;

namespace RFMAS.Infrastructure.Communication;

/// <summary>
/// Telemetry serialization and parsing protocol service.
/// Protocol format: "deviceId|frequencyMHz|signalPowerDbm|temperatureC|voltageV|currentA|status|timestamp"
/// Example: "RF-001|2450.25|-32.5|41.3|12.1|1.8|ONLINE|2026-09-18T19:00:00Z"
/// Also supports compact 6-segment format: "RF-001|2450.25|-32.5|41.3|12.1|1.8"
/// </summary>
public class TelemetryMessageParser : ITelemetryParser
{
    private const char Delimiter = '|';

    public TelemetryData Parse(string rawMessage)
    {
        if (string.IsNullOrWhiteSpace(rawMessage))
        {
            throw new ArgumentException("Raw telemetry message cannot be empty", nameof(rawMessage));
        }

        var parts = rawMessage.Trim().Split(Delimiter);
        if (parts.Length < 6)
        {
            throw aerialFormatException(rawMessage, $"Expected at least 6 delimited segments, found {parts.Length}");
        }

        try
        {
            var deviceId = parts[0].Trim();
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new FormatException("Device ID segment cannot be empty");
            }

            var freq = double.Parse(parts[1], CultureInfo.InvariantCulture);
            var power = double.Parse(parts[2], CultureInfo.InvariantCulture);
            var temp = double.Parse(parts[3], CultureInfo.InvariantCulture);
            var volt = double.Parse(parts[4], CultureInfo.InvariantCulture);
            var current = double.Parse(parts[5], CultureInfo.InvariantCulture);

            var status = DeviceStatus.ONLINE;
            if (parts.Length >= 7 && Enum.TryParse<DeviceStatus>(parts[6], true, out var parsedStatus))
            {
                status = parsedStatus;
            }

            var timestamp = DateTime.UtcNow;
            if (parts.Length >= 8 && DateTime.TryParse(parts[7], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var parsedTime))
            {
                timestamp = parsedTime;
            }

            return new TelemetryData
            {
                DeviceId = deviceId,
                FrequencyMHz = freq,
                SignalPowerDbm = power,
                TemperatureC = temp,
                VoltageV = volt,
                CurrentA = current,
                Status = status,
                Timestamp = timestamp
            };
        }
        catch (Exception ex) when (ex is not FormatException)
        {
            throw aerialFormatException(rawMessage, ex.Message);
        }
    }

    public bool TryParse(string rawMessage, out TelemetryData? telemetryData, out string? errorMessage)
    {
        telemetryData = null;
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(rawMessage))
        {
            errorMessage = "Telemetry message is empty";
            return false;
        }

        try
        {
            telemetryData = Parse(rawMessage);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    public string Format(TelemetryData data)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}|{1:F2}|{2:F2}|{3:F2}|{4:F2}|{5:F2}|{6}|{7:O}",
            data.DeviceId,
            data.FrequencyMHz,
            data.SignalPowerDbm,
            data.TemperatureC,
            data.VoltageV,
            data.CurrentA,
            data.Status,
            data.Timestamp);
    }

    private static FormatException aerialFormatException(string raw, string details)
    {
        return new FormatException($"Malformed telemetry message: '{raw}'. Error: {details}");
    }
}
