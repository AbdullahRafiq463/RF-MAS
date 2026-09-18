using RFMAS.Core.Entities;
using RFMAS.Core.Models;

namespace RFMAS.Api.DTOs;

public record TelemetryDto
{
    public long Id { get; init; }
    public string DeviceId { get; init; } = string.Empty;
    public double FrequencyMHz { get; init; }
    public double SignalPowerDbm { get; init; }
    public double TemperatureC { get; init; }
    public double VoltageV { get; init; }
    public double CurrentA { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }

    public static TelemetryDto FromEntity(TelemetryRecord t) => new()
    {
        Id = t.Id,
        DeviceId = t.DeviceId,
        FrequencyMHz = Math.Round(t.FrequencyMHz, 2),
        SignalPowerDbm = Math.Round(t.SignalPowerDbm, 2),
        TemperatureC = Math.Round(t.TemperatureC, 2),
        VoltageV = Math.Round(t.VoltageV, 2),
        CurrentA = Math.Round(t.CurrentA, 2),
        Status = t.Status.ToString(),
        Timestamp = t.Timestamp
    };

    public static TelemetryDto FromModel(TelemetryData t) => new()
    {
        DeviceId = t.DeviceId,
        FrequencyMHz = Math.Round(t.FrequencyMHz, 2),
        SignalPowerDbm = Math.Round(t.SignalPowerDbm, 2),
        TemperatureC = Math.Round(t.TemperatureC, 2),
        VoltageV = Math.Round(t.VoltageV, 2),
        CurrentA = Math.Round(t.CurrentA, 2),
        Status = t.Status.ToString(),
        Timestamp = t.Timestamp
    };
}
