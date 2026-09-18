using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Models;
using RFMAS.Infrastructure.Services;
using Xunit;

namespace RFMAS.Tests;

public class ThresholdEvaluatorTests
{
    private readonly ThresholdEvaluator _evaluator = new();

    private Device CreateTestDevice() => new()
    {
        Id = "RF-001",
        Name = "Signal Generator Simulator",
        OperationalStatus = DeviceStatus.ONLINE,
        ConnectionStatus = true
    };

    private ThresholdConfig CreateDefaultThresholds() => new()
    {
        DeviceId = "RF-001",
        TemperatureWarning = 60.0,
        TemperatureCritical = 75.0,
        SignalPowerWarning = -50.0,
        SignalPowerCritical = -70.0,
        VoltageMin = 11.0,
        VoltageMax = 13.0,
        FrequencyMin = 2400.0,
        FrequencyMax = 2500.0,
        CurrentMaxA = 3.0
    };

    [Fact]
    public void Evaluate_WhenAllMetricsNominal_ShouldReturnNoAlerts()
    {
        // Arrange
        var device = CreateTestDevice();
        var thresholds = CreateDefaultThresholds();
        var telemetry = new TelemetryData
        {
            DeviceId = "RF-001",
            FrequencyMHz = 2450.0,
            SignalPowerDbm = -30.0,
            TemperatureC = 45.0,
            VoltageV = 12.0,
            CurrentA = 1.8,
            Status = DeviceStatus.ONLINE
        };

        // Act
        var alerts = _evaluator.Evaluate(device, telemetry, thresholds);

        // Assert
        Assert.Empty(alerts);
    }

    [Theory]
    [InlineData(60.5, AlertSeverity.WARNING, "TEMPERATURE_WARNING")]
    [InlineData(76.0, AlertSeverity.CRITICAL, "TEMPERATURE_CRITICAL")]
    public void Evaluate_TemperatureThresholds_ShouldTriggerAppropriateAlert(double temp, AlertSeverity expectedSeverity, string expectedType)
    {
        // Arrange
        var device = CreateTestDevice();
        var thresholds = CreateDefaultThresholds();
        var telemetry = new TelemetryData
        {
            DeviceId = "RF-001",
            FrequencyMHz = 2450.0,
            SignalPowerDbm = -30.0,
            TemperatureC = temp,
            VoltageV = 12.0,
            CurrentA = 1.8,
            Status = DeviceStatus.ONLINE
        };

        // Act
        var alerts = _evaluator.Evaluate(device, telemetry, thresholds);

        // Assert
        Assert.Contains(alerts, a => a.Severity == expectedSeverity && a.AlertType == expectedType);
    }

    [Theory]
    [InlineData(-55.0, AlertSeverity.WARNING, "SIGNAL_POWER_WARNING")]
    [InlineData(-75.0, AlertSeverity.CRITICAL, "SIGNAL_POWER_CRITICAL")]
    public void Evaluate_SignalPowerThresholds_ShouldTriggerAppropriateAlert(double power, AlertSeverity expectedSeverity, string expectedType)
    {
        // Arrange
        var device = CreateTestDevice();
        var thresholds = CreateDefaultThresholds();
        var telemetry = new TelemetryData
        {
            DeviceId = "RF-001",
            FrequencyMHz = 2450.0,
            SignalPowerDbm = power,
            TemperatureC = 42.0,
            VoltageV = 12.0,
            CurrentA = 1.8,
            Status = DeviceStatus.ONLINE
        };

        // Act
        var alerts = _evaluator.Evaluate(device, telemetry, thresholds);

        // Assert
        Assert.Contains(alerts, a => a.Severity == expectedSeverity && a.AlertType == expectedType);
    }

    [Theory]
    [InlineData(10.2)] // Below 11.0V
    [InlineData(13.8)] // Above 13.0V
    public void Evaluate_VoltageOutOfRange_ShouldTriggerVoltageAlert(double voltage)
    {
        // Arrange
        var device = CreateTestDevice();
        var thresholds = CreateDefaultThresholds();
        var telemetry = new TelemetryData
        {
            DeviceId = "RF-001",
            FrequencyMHz = 2450.0,
            SignalPowerDbm = -30.0,
            TemperatureC = 42.0,
            VoltageV = voltage,
            CurrentA = 1.8,
            Status = DeviceStatus.ONLINE
        };

        // Act
        var alerts = _evaluator.Evaluate(device, telemetry, thresholds);

        // Assert
        Assert.Contains(alerts, a => a.AlertType == "VOLTAGE_OUT_OF_RANGE" && a.Severity == AlertSeverity.WARNING);
    }

    [Theory]
    [InlineData(2350.0)] // Below 2400 MHz
    [InlineData(2550.0)] // Above 2500 MHz
    public void Evaluate_FrequencyOutOfBand_ShouldTriggerFrequencyAlert(double frequency)
    {
        // Arrange
        var device = CreateTestDevice();
        var thresholds = CreateDefaultThresholds();
        var telemetry = new TelemetryData
        {
            DeviceId = "RF-001",
            FrequencyMHz = frequency,
            SignalPowerDbm = -30.0,
            TemperatureC = 42.0,
            VoltageV = 12.0,
            CurrentA = 1.8,
            Status = DeviceStatus.ONLINE
        };

        // Act
        var alerts = _evaluator.Evaluate(device, telemetry, thresholds);

        // Assert
        Assert.Contains(alerts, a => a.AlertType == "FREQUENCY_OUT_OF_RANGE" && a.Severity == AlertSeverity.WARNING);
    }
}
