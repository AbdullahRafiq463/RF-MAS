using RFMAS.Core.Entities;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Repository interface for Device persistence.
/// </summary>
public interface IDeviceRepository
{
    Task<IReadOnlyList<Device>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Device?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Device?> GetByIdWithThresholdsAsync(string id, CancellationToken cancellationToken = default);
    Task AddAsync(Device device, CancellationToken cancellationToken = default);
    Task UpdateAsync(Device device, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}
