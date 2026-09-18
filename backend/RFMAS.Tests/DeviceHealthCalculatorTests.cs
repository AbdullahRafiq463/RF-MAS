using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Models;
using RFMAS.Infrastructure.Services;
using Xunit;

namespace RFMAS.Tests;

public class DeviceHealthCalculatorTests
{
    private readonly DeviceHealthCalculator _calculator = new();

    private Device CreateTestDevice() => new()
    {
        Id = "RF-001",
        Name = "Signal Generator Simulator",
        OperationalStatus = DeviceStatus.ONLINE,
        ConnectionStatus = true
    };

    private ThresholdConfig CreateThresholds() => new()
    {
        DeviceId = "RF-001",
        TemperatureWarning = 60.0,
        TemperatureCritical = 75.0,
        SignalPowerWarning = -50.0,
        SignalPowerCritical = -70.0,
        VoltageMin = 11.0,
        VoltageMax = 13.0,
        FrequencyMin = 2400.0,
        FrequencyMax = 2500.0
    };

    [Fact]
    public void Calculate_WhenDeviceIsOffline_ShouldReturnZeroHealth()
    {
        // Arrange
        var device = CreateTestDevice();
        var telemetry = new TelemetryData
        {
            DeviceId = "RF-001",
            Status = DeviceStatus.OFFLINE
        };

        // Act
        var result = _calculator.Calculate(device, telemetry, CreateThresholds(), 0);

        // Assert
        Assert.Equal(0.0, result.TotalHealthPercentage);
        Assert.Equal("OFFLINE", result.Assessment);
    }

    [Fact]
    public void Calculate_WhenOperatingNominally_ShouldReturnHighHealth()
    {
        // Arrange
        var device = CreateTestDevice();
        var telemetry = new TelemetryData
        {
            DeviceId = "RF-001",
            FrequencyMHz = 2450.0,
            SignalPowerDbm = -30.0,
            TemperatureC = 41.0,
            VoltageV = 12.0,
            CurrentA = 1.8,
            Status = DeviceStatus.ONLINE
        };

        // Act
        var result = _calculator.Calculate(device, telemetry, CreateThresholds(), 0);

        // Assert
        Assert.True(result.TotalHealthPercentage >= 95.0, $"Expected health >= 95%, got {result.TotalHealthPercentage}%");
        Assert.Equal("EXCELLENT", result.Assessment);
    }

    [Fact]
    public void Calculate_WhenHighTemperatureAndAlerts_ShouldDegradeHealthScore()
    {
        // Arrange
        var device = CreateTestDevice();
        var telemetry = new TelemetryData
        {
            DeviceId = "RF-001",
            FrequencyMHz = 2450.0,
            SignalPowerDbm = -30.0,
            TemperatureC = 72.0, // High temperature
            VoltageV = 12.0,
            CurrentA = 2.0,
            Status = DeviceStatus.WARNING
        };

        // Act
        var normalResult = _calculator.Calculate(device, telemetry, CreateThresholds(), 0);
        var alertedResult = _calculator.Calculate(device, telemetry, CreateThresholds(), 2);

        // Assert
        Assert.True(normalResult.TotalHealthPercentage < 90.0, "High temp should degrade score below 90%");
        Assert.True(alertedResult.TotalHealthPercentage < normalResult.TotalHealthPercentage, "Active alerts should apply penalties");
    }
}
