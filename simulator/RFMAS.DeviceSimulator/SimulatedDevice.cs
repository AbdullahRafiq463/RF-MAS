using RFMAS.Core.Enums;
using RFMAS.Core.Models;

namespace RFMAS.DeviceSimulator;

/// <summary>
/// Simulates an individual RF device with realistic continuous telemetry physics,
/// parameter fluctuations, state machines, and failure scenario injections.
/// </summary>
public class SimulatedDevice
{
    private readonly Random _rand;
    private double _timeStep;

    public string DeviceId { get; }
    public string Name { get; }
    public string DeviceType { get; }
    public string Manufacturer { get; }
    public string Model { get; }
    public string IpAddress { get; }
    public int Port { get; }

    // Configured setpoints
    public double BaseFrequencyMHz { get; private set; }
    public double BaseSignalPowerDbm { get; private set; }
    public double BaseTemperatureC { get; private set; }
    public double BaseVoltageV { get; private set; }
    public double BaseCurrentA { get; private set; }

    // Dynamic state
    public double CurrentFrequencyMHz { get; private set; }
    public double CurrentSignalPowerDbm { get; private set; }
    public double CurrentTemperatureC { get; private set; }
    public double CurrentVoltageV { get; private set; }
    public double CurrentCurrentA { get; private set; }
    public DeviceStatus Status { get; private set; }
    public bool IsConnected { get; private set; }
    public DateTime LastCommunicationTime { get; private set; }

    // Active injected simulation scenario
    public SimulationScenarioType ActiveScenario { get; private set; } = SimulationScenarioType.NORMAL;
    private double? _scenarioCustomValue;

    public SimulatedDevice(
        string deviceId,
        string name,
        string deviceType,
        string manufacturer,
        string model,
        string ipAddress,
        int port,
        double baseFrequencyMHz,
        double baseSignalPowerDbm,
        double baseTemperatureC = 40.0,
        double baseVoltageV = 12.0,
        double baseCurrentA = 1.8)
    {
        DeviceId = deviceId;
        Name = name;
        DeviceType = deviceType;
        Manufacturer = manufacturer;
        Model = model;
        IpAddress = ipAddress;
        Port = port;

        BaseFrequencyMHz = baseFrequencyMHz;
        BaseSignalPowerDbm = baseSignalPowerDbm;
        BaseTemperatureC = baseTemperatureC;
        BaseVoltageV = baseVoltageV;
        BaseCurrentA = baseCurrentA;

        CurrentFrequencyMHz = baseFrequencyMHz;
        CurrentSignalPowerDbm = baseSignalPowerDbm;
        CurrentTemperatureC = baseTemperatureC;
        CurrentVoltageV = baseVoltageV;
        CurrentCurrentA = baseCurrentA;

        Status = DeviceStatus.ONLINE;
        IsConnected = true;
        LastCommunicationTime = DateTime.UtcNow;

        _rand = new Random(deviceId.GetHashCode());
        _timeStep = _rand.NextDouble() * 10.0;
    }

    /// <summary>
    /// Generates next telemetry sample applying realistic sinusoidal & random Gaussian noise.
    /// Injected scenarios override or skew telemetry values.
    /// </summary>
    public TelemetryData GenerateTelemetry()
    {
        _timeStep += 0.1;
        LastCommunicationTime = DateTime.UtcNow;

        if (ActiveScenario == SimulationScenarioType.DISCONNECT)
        {
            IsConnected = false;
            Status = DeviceStatus.OFFLINE;
            return new TelemetryData
            {
                DeviceId = DeviceId,
                FrequencyMHz = 0,
                SignalPowerDbm = -100.0,
                TemperatureC = 0,
                VoltageV = 0,
                CurrentA = 0,
                Status = DeviceStatus.OFFLINE,
                Timestamp = DateTime.UtcNow
            };
        }

        if (ActiveScenario == SimulationScenarioType.COMMUNICATION_TIMEOUT)
        {
            // Simulate timeout by not updating LastCommunicationTime
            Status = DeviceStatus.WARNING;
        }

        if (Status == DeviceStatus.OFFLINE)
        {
            return new TelemetryData
            {
                DeviceId = DeviceId,
                FrequencyMHz = 0,
                SignalPowerDbm = -100,
                TemperatureC = 20.0,
                VoltageV = 0,
                CurrentA = 0,
                Status = DeviceStatus.OFFLINE,
                Timestamp = DateTime.UtcNow
            };
        }

        // Normal subtle variations (sinusoidal drift + small random noise)
        double freqNoise = Math.Sin(_timeStep * 0.5) * 0.08 + (_rand.NextDouble() - 0.5) * 0.04;
        double powerNoise = Math.Cos(_timeStep * 0.3) * 0.6 + (_rand.NextDouble() - 0.5) * 0.3;
        double tempNoise = Math.Sin(_timeStep * 0.2) * 1.2 + (_rand.NextDouble() - 0.5) * 0.4;
        double voltNoise = Math.Sin(_timeStep * 0.4) * 0.1 + (_rand.NextDouble() - 0.5) * 0.05;
        double currentNoise = Math.Cos(_timeStep * 0.4) * 0.05 + (_rand.NextDouble() - 0.5) * 0.02;

        CurrentFrequencyMHz = BaseFrequencyMHz + freqNoise;
        CurrentSignalPowerDbm = BaseSignalPowerDbm + powerNoise;
        CurrentTemperatureC = BaseTemperatureC + tempNoise;
        CurrentVoltageV = BaseVoltageV + voltNoise;
        CurrentCurrentA = BaseCurrentA + currentNoise;

        // Apply Scenario Injections
        switch (ActiveScenario)
        {
            case SimulationScenarioType.HIGH_TEMPERATURE:
                CurrentTemperatureC = _scenarioCustomValue ?? (76.5 + Math.Sin(_timeStep) * 2.0);
                Status = DeviceStatus.ERROR;
                break;

            case SimulationScenarioType.LOW_SIGNAL:
                CurrentSignalPowerDbm = _scenarioCustomValue ?? (-74.0 + Math.Sin(_timeStep) * 1.5);
                Status = DeviceStatus.WARNING;
                break;

            case SimulationScenarioType.LOW_VOLTAGE:
                CurrentVoltageV = _scenarioCustomValue ?? (9.8 + Math.Sin(_timeStep) * 0.2);
                Status = DeviceStatus.WARNING;
                break;

            case SimulationScenarioType.ERROR:
                Status = DeviceStatus.ERROR;
                CurrentTemperatureC = 78.0;
                CurrentSignalPowerDbm = -75.0;
                break;

            case SimulationScenarioType.NORMAL:
            default:
                if (Status != DeviceStatus.MAINTENANCE)
                {
                    Status = DeviceStatus.ONLINE;
                }
                break;
        }

        return new TelemetryData
        {
            DeviceId = DeviceId,
            FrequencyMHz = Math.Round(CurrentFrequencyMHz, 2),
            SignalPowerDbm = Math.Round(CurrentSignalPowerDbm, 2),
            TemperatureC = Math.Round(CurrentTemperatureC, 2),
            VoltageV = Math.Round(CurrentVoltageV, 2),
            CurrentA = Math.Round(CurrentCurrentA, 2),
            Status = Status,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Executes a control command on the simulated device.
    /// </summary>
    public CommandResult ExecuteCommand(CommandType command, Dictionary<string, object>? parameters)
    {
        switch (command)
        {
            case CommandType.START:
                if (Status == DeviceStatus.MAINTENANCE)
                {
                    return CommandResult.Invalid(DeviceId, command, "Device is in MAINTENANCE mode. Exit maintenance first.");
                }
                Status = DeviceStatus.ONLINE;
                IsConnected = true;
                ActiveScenario = SimulationScenarioType.NORMAL;
                return CommandResult.Successful(DeviceId, command, $"Device {DeviceId} started successfully.");

            case CommandType.STOP:
                Status = DeviceStatus.OFFLINE;
                return CommandResult.Successful(DeviceId, command, $"Device {DeviceId} stopped.");

            case CommandType.RESET:
            case CommandType.RESTART:
                ActiveScenario = SimulationScenarioType.NORMAL;
                Status = DeviceStatus.ONLINE;
                IsConnected = true;
                CurrentFrequencyMHz = BaseFrequencyMHz;
                CurrentSignalPowerDbm = BaseSignalPowerDbm;
                CurrentTemperatureC = BaseTemperatureC;
                CurrentVoltageV = BaseVoltageV;
                CurrentCurrentA = BaseCurrentA;
                return CommandResult.Successful(DeviceId, command, $"Device {DeviceId} reset to nominal operating parameters.");

            case CommandType.SET_FREQUENCY:
                if (parameters != null && parameters.TryGetValue("frequencyMHz", out var freqObj) &&
                    double.TryParse(freqObj.ToString(), out var newFreq))
                {
                    BaseFrequencyMHz = newFreq;
                    CurrentFrequencyMHz = newFreq;
                    return CommandResult.Successful(DeviceId, command, $"Frequency set to {newFreq:F2} MHz.");
                }
                return CommandResult.Invalid(DeviceId, command, "Missing or invalid 'frequencyMHz' parameter.");

            case CommandType.SET_POWER:
                if (parameters != null && parameters.TryGetValue("powerDbm", out var pwrObj) &&
                    double.TryParse(pwrObj.ToString(), out var newPwr))
                {
                    BaseSignalPowerDbm = newPwr;
                    CurrentSignalPowerDbm = newPwr;
                    return CommandResult.Successful(DeviceId, command, $"Signal power set to {newPwr:F2} dBm.");
                }
                return CommandResult.Invalid(DeviceId, command, "Missing or invalid 'powerDbm' parameter.");

            case CommandType.ENTER_MAINTENANCE:
                Status = DeviceStatus.MAINTENANCE;
                return CommandResult.Successful(DeviceId, command, $"Device {DeviceId} entered MAINTENANCE mode.");

            case CommandType.EXIT_MAINTENANCE:
                Status = DeviceStatus.ONLINE;
                return CommandResult.Successful(DeviceId, command, $"Device {DeviceId} exited MAINTENANCE mode.");

            default:
                return CommandResult.Invalid(DeviceId, command, $"Unsupported command {command}.");
        }
    }

    /// <summary>
    /// Injects an operational failure or test scenario.
    /// </summary>
    public void InjectScenario(SimulationScenarioType scenario, double? customValue = null)
    {
        ActiveScenario = scenario;
        _scenarioCustomValue = customValue;

        if (scenario == SimulationScenarioType.DISCONNECT)
        {
            IsConnected = false;
            Status = DeviceStatus.OFFLINE;
        }
        else if (scenario == SimulationScenarioType.NORMAL)
        {
            IsConnected = true;
            if (Status == DeviceStatus.OFFLINE || Status == DeviceStatus.ERROR)
            {
                Status = DeviceStatus.ONLINE;
            }
        }
    }
}
