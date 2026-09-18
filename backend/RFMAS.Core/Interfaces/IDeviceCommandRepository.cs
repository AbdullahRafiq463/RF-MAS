using RFMAS.Core.Entities;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Repository interface for Device Commands.
/// </summary>
public interface IDeviceCommandRepository
{
    Task AddAsync(DeviceCommand command, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceCommand>> GetRecentByDeviceAsync(string deviceId, int count = 20, CancellationToken cancellationToken = default);
    Task UpdateAsync(DeviceCommand command, CancellationToken cancellationToken = default);
}
