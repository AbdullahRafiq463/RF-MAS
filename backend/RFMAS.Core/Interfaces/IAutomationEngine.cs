using RFMAS.Core.Entities;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Executes automated verification test suites across simulated RF hardware.
/// </summary>
public interface IAutomationEngine
{
    Task<IReadOnlyList<AutomationTestResult>> RunAllTestsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AutomationTestResult>> RunDeviceTestsAsync(string deviceId, CancellationToken cancellationToken = default);
    Task<AutomationTestResult> RunSingleTestAsync(string testName, string deviceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AutomationTestResult>> GetLatestResultsAsync(int count = 50, CancellationToken cancellationToken = default);
}
