namespace RFMAS.Core.Entities;

/// <summary>
/// Configurable operating thresholds for an RF device.
/// </summary>
public class ThresholdConfig
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;

    // Temperature thresholds (°C)
    public double TemperatureWarning { get; set; } = 60.0;
    public double TemperatureCritical { get; set; } = 75.0;

    // Signal Power thresholds (dBm)
    public double SignalPowerWarning { get; set; } = -50.0;
    public double SignalPowerCritical { get; set; } = -70.0;

    // Voltage operating window (V)
    public double VoltageMin { get; set; } = 11.0;
    public double VoltageMax { get; set; } = 13.0;

    // Frequency acceptable range (MHz)
    public double FrequencyMin { get; set; } = 2400.0;
    public double FrequencyMax { get; set; } = 2500.0;

    // Current maximum safe draw (A)
    public double CurrentMaxA { get; set; } = 3.5;

    // Communication timeout threshold in seconds
    public int TimeoutSeconds { get; set; } = 10;

    // Navigation property
    public Device? Device { get; set; }
}
