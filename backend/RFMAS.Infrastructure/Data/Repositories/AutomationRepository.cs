using Microsoft.EntityFrameworkCore;
using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;

namespace RFMAS.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of IAutomationRepository.
/// </summary>
public class AutomationRepository : IAutomationRepository
{
    private readonly RFMASDbContext _context;

    public AutomationRepository(RFMASDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AutomationTestResult result, CancellationToken cancellationToken = default)
    {
        await _context.AutomationTestResults.AddAsync(result, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<AutomationTestResult> results, CancellationToken cancellationToken = default)
    {
        await _context.AutomationTestResults.AddRangeAsync(results, cancellationToken);
    }

    public async Task<IReadOnlyList<AutomationTestResult>> GetLatestResultsAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        return await _context.AutomationTestResults
            .OrderByDescending(r => r.ExecutedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AutomationTestResult>> GetResultsByDeviceAsync(string deviceId, int count = 20, CancellationToken cancellationToken = default)
    {
        return await _context.AutomationTestResults
            .Where(r => r.DeviceId == deviceId)
            .OrderByDescending(r => r.ExecutedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<(int total, int passed, int failed, int errored)> GetSummaryStatsAsync(CancellationToken cancellationToken = default)
    {
        var total = await _context.AutomationTestResults.CountAsync(cancellationToken);
        var passed = await _context.AutomationTestResults.CountAsync(r => r.Status == TestResultStatus.PASS, cancellationToken);
        var failed = await _context.AutomationTestResults.CountAsync(r => r.Status == TestResultStatus.FAIL, cancellationToken);
        var errored = await _context.AutomationTestResults.CountAsync(r => r.Status == TestResultStatus.ERROR, cancellationToken);

        return (total, passed, failed, errored);
    }
}
