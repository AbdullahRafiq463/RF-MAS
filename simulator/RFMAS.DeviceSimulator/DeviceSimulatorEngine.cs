using System.Collections.Concurrent;
using RFMAS.Core.Enums;
using RFMAS.Core.Models;

namespace RFMAS.DeviceSimulator;

/// <summary>
/// Thread-safe multi-device simulation engine.
/// Manages simulated RF instruments, continuous telemetry stream generation,
/// scenario injections, and command dispatch.
/// </summary>
public class DeviceSimulatorEngine
{
    private readonly ConcurrentDictionary<string, SimulatedDevice> _devices = new();

    public DeviceSimulatorEngine()
    {
        InitializeDefaultDevices();
    }

    private void InitializeDefaultDevices()
    {
        // 5 Simulated RF Devices per specification
        var devices = new[]
        {
            new SimulatedDevice(
                deviceId: "RF-001",
                name: "Signal Generator Simulator",
                deviceType: "Signal Generator",
                manufacturer: "AeroTech Instruments",
                model: "SG-2500",
                ipAddress: "127.0.0.1",
                port: 9001,
                baseFrequencyMHz: 2450.0,
                baseSignalPowerDbm: -30.0,
                baseTemperatureC: 41.5,
                baseVoltageV: 12.0,
                baseCurrentA: 1.8),

            new SimulatedDevice(
                deviceId: "RF-002",
                name: "Spectrum Analyzer Simulator",
                deviceType: "Spectrum Analyzer",
                manufacturer: "NovaWave Labs",
                model: "SA-4000",
                ipAddress: "127.0.0.1",
                port: 9002,
                baseFrequencyMHz: 5200.0,
                baseSignalPowerDbm: -25.0,
                baseTemperatureC: 44.0,
                baseVoltageV: 12.1,
                baseCurrentA: 2.1),

            new SimulatedDevice(
                deviceId: "RF-003",
                name: "Power Amplifier Simulator",
                deviceType: "Power Amplifier",
                manufacturer: "Apex Quantum",
                model: "PA-1200",
                ipAddress: "127.0.0.1",
                port: 9003,
                baseFrequencyMHz: 915.0,
                baseSignalPowerDbm: -15.0,
                baseTemperatureC: 47.8,
                baseVoltageV: 11.9,
                baseCurrentA: 2.8),

            new SimulatedDevice(
                deviceId: "RF-004",
                name: "Transceiver Module Simulator",
                deviceType: "RF Transceiver",
                manufacturer: "Vectron Systems",
                model: "RA-6000",
                ipAddress: "127.0.0.1",
                port: 9004,
                baseFrequencyMHz: 9350.0,
                baseSignalPowerDbm: -35.0,
                baseTemperatureC: 38.5,
                baseVoltageV: 12.0,
                baseCurrentA: 1.6),

            new SimulatedDevice(
                deviceId: "RF-005",
                name: "Telemetry Sensor Simulator",
                deviceType: "Telemetry Receiver",
                manufacturer: "OmniWave Dynamics",
                model: "TS-8000",
                ipAddress: "127.0.0.1",
                port: 9005,
                baseFrequencyMHz: 1450.0,
                baseSignalPowerDbm: -40.0,
                baseTemperatureC: 36.2,
                baseVoltageV: 12.2,
                baseCurrentA: 1.2)
        };

        foreach (var dev in devices)
        {
            _devices[dev.DeviceId] = dev;
        }
    }

    public IReadOnlyCollection<SimulatedDevice> GetAllDevices() => _devices.Values.ToList();

    public SimulatedDevice? GetDevice(string deviceId)
    {
        _devices.TryGetValue(deviceId, out var dev);
        return dev;
    }

    public TelemetryData? GenerateTelemetryForDevice(string deviceId)
    {
        if (_devices.TryGetValue(deviceId, out var dev))
        {
            return dev.GenerateTelemetry();
        }
        return null;
    }

    public IReadOnlyList<TelemetryData> GenerateAllTelemetry()
    {
        var list = new List<TelemetryData>(_devices.Count);
        foreach (var dev in _devices.Values)
        {
            list.Add(dev.GenerateTelemetry());
        }
        return list;
    }

    public CommandResult ExecuteCommand(string deviceId, CommandType command, Dictionary<string, object>? parameters)
    {
        if (_devices.TryGetValue(deviceId, out var dev))
        {
            return dev.ExecuteCommand(command, parameters);
        }
        return CommandResult.Failed(deviceId, command, $"Device {deviceId} not found in simulator engine.");
    }

    public bool InjectScenario(string deviceId, SimulationScenarioType scenario, double? customValue = null)
    {
        if (_devices.TryGetValue(deviceId, out var dev))
        {
            dev.InjectScenario(scenario, customValue);
            return true;
        }
        return false;
    }
}
