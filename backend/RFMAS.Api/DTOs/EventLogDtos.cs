using RFMAS.Core.Entities;

namespace RFMAS.Api.DTOs;

public record EventLogDto
{
    public long Id { get; init; }
    public DateTime Timestamp { get; init; }
    public string? DeviceId { get; init; }
    public string Category { get; init; } = string.Empty;
    public string EventName { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;

    public static EventLogDto FromEntity(EventLog l) => new()
    {
        Id = l.Id,
        Timestamp = l.Timestamp,
        DeviceId = l.DeviceId,
        Category = l.Category,
        EventName = l.EventName,
        Details = l.Details,
        Severity = l.Severity.ToString()
    };
}
