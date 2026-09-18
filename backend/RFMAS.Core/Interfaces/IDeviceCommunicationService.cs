using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Models;

namespace RFMAS.Core.Interfaces;

/// <summary>
/// Abstraction for communicating with devices (simulated TCP/IP sockets or physical hardware interfaces).
/// </summary>
public interface IDeviceCommunicationService
{
    Task<bool> ConnectAsync(Device device, CancellationToken cancellationToken = default);
    Task<bool> DisconnectAsync(string deviceId, CancellationToken cancellationToken = default);
    Task<bool> IsConnectedAsync(string deviceId, CancellationToken cancellationToken = default);
    Task<CommandResult> SendCommandAsync(string deviceId, CommandType commandType, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);
    Task<TelemetryData?> ReceiveTelemetryAsync(string deviceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TelemetryData>> ReceiveAllTelemetryAsync(CancellationToken cancellationToken = default);
}
