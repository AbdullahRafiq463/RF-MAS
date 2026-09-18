using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;
using RFMAS.Core.Models;
using RFMAS.DeviceSimulator;

namespace RFMAS.Infrastructure.Communication;

/// <summary>
/// Device communication service implementing network protocol exchange over TCP/IP sockets or simulated stream abstractions.
/// Handles connection lifecycle, packet serialization/deserialization via ITelemetryParser, and thread-safe dispatch.
/// </summary>
public class TcpDeviceCommunicationService : IDeviceCommunicationService
{
    private readonly DeviceSimulatorEngine _simulatorEngine;
    private readonly ITelemetryParser _parser;
    private readonly SemaphoreSlim _networkLock = new(1, 1);

    public TcpDeviceCommunicationService(
        DeviceSimulatorEngine simulatorEngine,
        ITelemetryParser parser)
    {
        _simulatorEngine = simulatorEngine;
        _parser = parser;
    }

    public Task<bool> ConnectAsync(Device device, CancellationToken cancellationToken = default)
    {
        var sim = _simulatorEngine.GetDevice(device.Id);
        if (sim == null) return Task.FromResult(false);

        sim.InjectScenario(SimulationScenarioType.NORMAL);
        return Task.FromResult(true);
    }

    public Task<bool> DisconnectAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        var sim = _simulatorEngine.GetDevice(deviceId);
        if (sim == null) return Task.FromResult(false);

        sim.InjectScenario(SimulationScenarioType.DISCONNECT);
        return Task.FromResult(true);
    }

    public Task<bool> IsConnectedAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        var sim = _simulatorEngine.GetDevice(deviceId);
        return Task.FromResult(sim?.IsConnected ?? false);
    }

    public async Task<CommandResult> SendCommandAsync(
        string deviceId,
        CommandType commandType,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        await _networkLock.WaitAsync(cancellationToken);
        try
        {
            // Simulate realistic network roundtrip latency (15-35 ms)
            await Task.Delay(25, cancellationToken);
            return _simulatorEngine.ExecuteCommand(deviceId, commandType, parameters);
        }
        finally
        {
            _networkLock.Release();
        }
    }

    public async Task<TelemetryData?> ReceiveTelemetryAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        await _networkLock.WaitAsync(cancellationToken);
        try
        {
            var rawData = _simulatorEngine.GenerateTelemetryForDevice(deviceId);
            if (rawData == null) return null;

            // Round-trip through protocol serialization/deserialization to guarantee TCP packet parity
            var formattedPacket = _parser.Format(rawData);
            return _parser.Parse(formattedPacket);
        }
        finally
        {
            _networkLock.Release();
        }
    }

    public async Task<IReadOnlyList<TelemetryData>> ReceiveAllTelemetryAsync(CancellationToken cancellationToken = default)
    {
        await _networkLock.WaitAsync(cancellationToken);
        try
        {
            var all = _simulatorEngine.GenerateAllTelemetry();
            var list = new List<TelemetryData>(all.Count);
            foreach (var t in all)
            {
                var packet = _parser.Format(t);
                list.Add(_parser.Parse(packet));
            }
            return list;
        }
        finally
        {
            _networkLock.Release();
        }
    }
}
