using RFMAS.Core.Entities;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Repository interface for Device Threshold configurations.
/// </summary>
public interface IThresholdRepository
{
    Task<ThresholdConfig?> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default);
    Task UpdateAsync(ThresholdConfig config, CancellationToken cancellationToken = default);
}
