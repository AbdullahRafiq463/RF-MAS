using RFMAS.Core.Models;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Handles serialization and parsing of raw network telemetry stream packets.
/// Protocol format: "deviceId|frequencyMHz|signalPowerDbm|temperatureC|voltageV|currentA|status|timestamp"
/// </summary>
public interface ITelemetryParser
{
    TelemetryData Parse(string rawMessage);
    bool TryParse(string rawMessage, out TelemetryData? telemetryData, out string? errorMessage);
    string Format(TelemetryData data);
}
