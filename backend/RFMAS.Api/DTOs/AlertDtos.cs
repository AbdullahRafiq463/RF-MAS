using RFMAS.Core.Entities;

namespace RFMAS.Api.DTOs;

public record AlertDto
{
    public long Id { get; init; }
    public string DeviceId { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string AlertType { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public bool IsAcknowledged { get; init; }
    public DateTime? AcknowledgedAt { get; init; }
    public string? AcknowledgedBy { get; init; }

    public static AlertDto FromEntity(Alert a) => new()
    {
        Id = a.Id,
        DeviceId = a.DeviceId,
        Severity = a.Severity.ToString(),
        AlertType = a.AlertType,
        Message = a.Message,
        Timestamp = a.Timestamp,
        IsAcknowledged = a.IsAcknowledged,
        AcknowledgedAt = a.AcknowledgedAt,
        AcknowledgedBy = a.AcknowledgedBy
    };
}

public record AcknowledgeAlertRequestDto
{
    public string? AcknowledgedBy { get; init; }
}
