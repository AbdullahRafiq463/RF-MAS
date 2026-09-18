using RFMAS.Core.Enums;
using RFMAS.Core.Models;

namespace RFMAS.Api.DTOs;

public record SetFrequencyRequestDto
{
    public double FrequencyMHz { get; init; }
}

public record SetPowerRequestDto
{
    public double PowerDbm { get; init; }
}

public record GenericCommandRequestDto
{
    public CommandType CommandType { get; init; }
    public Dictionary<string, object>? Parameters { get; init; }
}

public record CommandResponseDto
{
    public bool Success { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public DateTime ExecutedAt { get; init; }

    public static CommandResponseDto FromResult(CommandResult r) => new()
    {
        Success = r.Success,
        Status = r.Status.ToString(),
        Message = r.Message,
        DeviceId = r.DeviceId,
        CommandType = r.CommandType.ToString(),
        ExecutedAt = r.ExecutedAt
    };
}
