using Microsoft.EntityFrameworkCore;
using RFMAS.Core.Entities;
using RFMAS.Core.Interfaces;

namespace RFMAS.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of IThresholdRepository.
/// </summary>
public class ThresholdRepository : IThresholdRepository
{
    private readonly RFMASDbContext _context;

    public ThresholdRepository(RFMASDbContext context)
    {
        _context = context;
    }

    public async Task<ThresholdConfig?> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        return await _context.ThresholdConfigs
            .FirstOrDefaultAsync(t => t.DeviceId == deviceId, cancellationToken);
    }

    public Task UpdateAsync(ThresholdConfig config, CancellationToken cancellationToken = default)
    {
        _context.ThresholdConfigs.Update(config);
        return Task.CompletedTask;
    }
}
