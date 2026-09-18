using RFMAS.Core.Entities;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Repository interface for Automation Test results.
/// </summary>
public interface IAutomationRepository
{
    Task AddAsync(AutomationTestResult result, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<AutomationTestResult> results, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AutomationTestResult>> GetLatestResultsAsync(int count = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AutomationTestResult>> GetResultsByDeviceAsync(string deviceId, int count = 20, CancellationToken cancellationToken = default);
    Task<(int total, int passed, int failed, int errored)> GetSummaryStatsAsync(CancellationToken cancellationToken = default);
}
