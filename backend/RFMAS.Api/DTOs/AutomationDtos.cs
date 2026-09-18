using RFMAS.Core.Entities;

namespace RFMAS.Api.DTOs;

public record AutomationTestResultDto
{
    public long Id { get; init; }
    public string TestName { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
    public string ExpectedValue { get; init; } = string.Empty;
    public string ActualValue { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public long ExecutionDurationMs { get; init; }
    public DateTime ExecutedAt { get; init; }
    public string? ErrorMessage { get; init; }

    public static AutomationTestResultDto FromEntity(AutomationTestResult r) => new()
    {
        Id = r.Id,
        TestName = r.TestName,
        DeviceId = r.DeviceId,
        ExpectedValue = r.ExpectedValue,
        ActualValue = r.ActualValue,
        Status = r.Status.ToString(),
        ExecutionDurationMs = r.ExecutionDurationMs,
        ExecutedAt = r.ExecutedAt,
        ErrorMessage = r.ErrorMessage
    };
}

public record RunTestRequestDto
{
    public string TestName { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
}
