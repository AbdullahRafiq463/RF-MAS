using RFMAS.Core.Enums;

namespace RFMAS.Core.Entities;

/// <summary>
/// Record of an automated test execution on a simulated RF device.
/// </summary>
public class AutomationTestResult
{
    public long Id { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string ExpectedValue { get; set; } = string.Empty;
    public string ActualValue { get; set; } = string.Empty;
    public TestResultStatus Status { get; set; } = TestResultStatus.PASS;
    public long ExecutionDurationMs { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }

    // Navigation property
    public Device? Device { get; set; }
}
