using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Models;

namespace RFMAS.Api.DTOs;

public record DeviceSummaryDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string DeviceType { get; init; } = string.Empty;
    public string Manufacturer { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
    public int Port { get; init; }
    public bool ConnectionStatus { get; init; }
    public string OperationalStatus { get; init; } = string.Empty;
    public double FrequencyMHz { get; init; }
    public double SignalPowerDbm { get; init; }
    public double TemperatureC { get; init; }
    public double VoltageV { get; init; }
    public double CurrentA { get; init; }
    public double HealthPercentage { get; init; }
    public DateTime LastCommunicationTime { get; init; }

    public static DeviceSummaryDto FromEntity(Device d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        DeviceType = d.DeviceType,
        Manufacturer = d.Manufacturer,
        Model = d.Model,
        IpAddress = d.IpAddress,
        Port = d.Port,
        ConnectionStatus = d.ConnectionStatus,
        OperationalStatus = d.OperationalStatus.ToString(),
        FrequencyMHz = Math.Round(d.FrequencyMHz, 2),
        SignalPowerDbm = Math.Round(d.SignalPowerDbm, 2),
        TemperatureC = Math.Round(d.TemperatureC, 2),
        VoltageV = Math.Round(d.VoltageV, 2),
        CurrentA = Math.Round(d.CurrentA, 2),
        HealthPercentage = Math.Round(d.HealthPercentage, 1),
        LastCommunicationTime = d.LastCommunicationTime
    };
}

public record DeviceDetailDto : DeviceSummaryDto
{
    public string FirmwareVersion { get; init; } = string.Empty;
    public DateTime CreatedDate { get; init; }
    public ThresholdConfigDto? Thresholds { get; init; }
    public IReadOnlyList<TelemetryDto> RecentTelemetry { get; init; } = Array.Empty<TelemetryDto>();
    public IReadOnlyList<AlertDto> RecentAlerts { get; init; } = Array.Empty<AlertDto>();

    public static DeviceDetailDto FromEntity(
        Device d,
        IEnumerable<TelemetryRecord>? telemetry = null,
        IEnumerable<Alert>? alerts = null) => new()
    {
        Id = d.Id,
        Name = d.Name,
        DeviceType = d.DeviceType,
        Manufacturer = d.Manufacturer,
        Model = d.Model,
        IpAddress = d.IpAddress,
        Port = d.Port,
        ConnectionStatus = d.ConnectionStatus,
        OperationalStatus = d.OperationalStatus.ToString(),
        FrequencyMHz = Math.Round(d.FrequencyMHz, 2),
        SignalPowerDbm = Math.Round(d.SignalPowerDbm, 2),
        TemperatureC = Math.Round(d.TemperatureC, 2),
        VoltageV = Math.Round(d.VoltageV, 2),
        CurrentA = Math.Round(d.CurrentA, 2),
        HealthPercentage = Math.Round(d.HealthPercentage, 1),
        LastCommunicationTime = d.LastCommunicationTime,
        FirmwareVersion = d.FirmwareVersion,
        CreatedDate = d.CreatedDate,
        Thresholds = d.Thresholds != null ? ThresholdConfigDto.FromEntity(d.Thresholds) : null,
        RecentTelemetry = telemetry?.Select(TelemetryDto.FromEntity).ToList() ?? new List<TelemetryDto>(),
        RecentAlerts = alerts?.Select(AlertDto.FromEntity).ToList() ?? new List<AlertDto>()
    };
}

public record ThresholdConfigDto
{
    public double TemperatureWarning { get; init; }
    public double TemperatureCritical { get; init; }
    public double SignalPowerWarning { get; init; }
    public double SignalPowerCritical { get; init; }
    public double VoltageMin { get; init; }
    public double VoltageMax { get; init; }
    public double FrequencyMin { get; init; }
    public double FrequencyMax { get; init; }
    public double CurrentMaxA { get; init; }

    public static ThresholdConfigDto FromEntity(ThresholdConfig t) => new()
    {
        TemperatureWarning = t.TemperatureWarning,
        TemperatureCritical = t.TemperatureCritical,
        SignalPowerWarning = t.SignalPowerWarning,
        SignalPowerCritical = t.SignalPowerCritical,
        VoltageMin = t.VoltageMin,
        VoltageMax = t.VoltageMax,
        FrequencyMin = t.FrequencyMin,
        FrequencyMax = t.FrequencyMax,
        CurrentMaxA = t.CurrentMaxA
    };
}
