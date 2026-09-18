using Microsoft.EntityFrameworkCore;
using RFMAS.Core.Entities;
using RFMAS.Core.Interfaces;

namespace RFMAS.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of IDeviceRepository.
/// </summary>
public class DeviceRepository : IDeviceRepository
{
    private readonly RFMASDbContext _context;

    public DeviceRepository(RFMASDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Device>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Include(d => d.Thresholds)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Device?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Include(d => d.Thresholds)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Device?> GetByIdWithThresholdsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Include(d => d.Thresholds)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task AddAsync(Device device, CancellationToken cancellationToken = default)
    {
        await _context.Devices.AddAsync(device, cancellationToken);
    }

    public Task UpdateAsync(Device device, CancellationToken cancellationToken = default)
    {
        _context.Devices.Update(device);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Devices.AnyAsync(d => d.Id == id, cancellationToken);
    }
}
