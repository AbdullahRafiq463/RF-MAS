using Microsoft.EntityFrameworkCore;
using RFMAS.Core.Entities;
using RFMAS.Core.Interfaces;

namespace RFMAS.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of IDeviceCommandRepository.
/// </summary>
public class DeviceCommandRepository : IDeviceCommandRepository
{
    private readonly RFMASDbContext _context;

    public DeviceCommandRepository(RFMASDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DeviceCommand command, CancellationToken cancellationToken = default)
    {
        await _context.DeviceCommands.AddAsync(command, cancellationToken);
    }

    public async Task<IReadOnlyList<DeviceCommand>> GetRecentByDeviceAsync(string deviceId, int count = 20, CancellationToken cancellationToken = default)
    {
        return await _context.DeviceCommands
            .Where(c => c.DeviceId == deviceId)
            .OrderByDescending(c => c.IssuedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(DeviceCommand command, CancellationToken cancellationToken = default)
    {
        _context.DeviceCommands.Update(command);
        return Task.CompletedTask;
    }
}
