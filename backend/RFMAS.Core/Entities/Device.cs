using RFMAS.Core.Enums;

namespace RFMAS.Core.Entities;

/// <summary>
/// Represents a simulated RF device/instrument managed by RF-MAS.
/// </summary>
public class Device
{
    public string Id { get; set; } = string.Empty; // e.g. RF-001
    public string Name { get; set; } = string.Empty; // e.g. Signal Generator Simulator
    public string DeviceType { get; set; } = string.Empty; // Signal Generator, Spectrum Analyzer, etc.
    public string Manufacturer { get; set; } = string.Empty; // AeroTech Instruments
    public string Model { get; set; } = string.Empty; // SG-2500
    public string IpAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 9001;
    public bool ConnectionStatus { get; set; } = true;
    public DeviceStatus OperationalStatus { get; set; } = DeviceStatus.ONLINE;
    public double FrequencyMHz { get; set; } = 2450.0;
    public double SignalPowerDbm { get; set; } = -30.0;
    public double TemperatureC { get; set; } = 42.0;
    public double VoltageV { get; set; } = 12.0;
    public double CurrentA { get; set; } = 1.8;
    public double HealthPercentage { get; set; } = 100.0;
    public DateTime LastCommunicationTime { get; set; } = DateTime.UtcNow;
    public string FirmwareVersion { get; set; } = "v2.1.0-sim";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<TelemetryRecord> TelemetryRecords { get; set; } = new List<TelemetryRecord>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<DeviceCommand> Commands { get; set; } = new List<DeviceCommand>();
    public ICollection<EventLog> EventLogs { get; set; } = new List<EventLog>();
    public ThresholdConfig? Thresholds { get; set; }
}
