using Moq;
using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;
using RFMAS.Core.Models;
using RFMAS.Infrastructure.Services;
using Xunit;

namespace RFMAS.Tests;

public class AutomationEngineTests
{
    private readonly Mock<IUnitOfWork> _mockUow = new();
    private readonly Mock<IDeviceCommunicationService> _mockComm = new();
    private readonly Mock<ILogService> _mockLog = new();
    private readonly AutomationEngine _engine;

    public AutomationEngineTests()
    {
        _engine = new AutomationEngine(_mockUow.Object, _mockComm.Object, _mockLog.Object);
    }

    [Fact]
    public async Task RunSingleTest_FrequencyRangeTest_WhenInBand_ShouldPass()
    {
        // Arrange
        var device = new Device
        {
            Id = "RF-001",
            Name = "Signal Generator Simulator",
            FrequencyMHz = 2450.0,
            Thresholds = new ThresholdConfig
            {
                DeviceId = "RF-001",
                FrequencyMin = 2400.0,
                FrequencyMax = 2500.0
            }
        };

        var telemetry = new TelemetryData
        {
            DeviceId = "RF-001",
            FrequencyMHz = 2450.0,
            SignalPowerDbm = -30.0,
            TemperatureC = 42.0,
            VoltageV = 12.0,
            CurrentA = 1.8
        };

        _mockUow.Setup(u => u.Devices.GetByIdWithThresholdsAsync("RF-001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(device);

        _mockComm.Setup(c => c.ReceiveTelemetryAsync("RF-001", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(telemetry);

        _mockUow.Setup(u => u.Automation.AddAsync(It.IsAny<AutomationTestResult>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        // Act
        var result = await _engine.RunSingleTestAsync("Frequency Range Test", "RF-001");

        // Assert
        Assert.Equal(TestResultStatus.PASS, result.Status);
        Assert.Equal("2450.00 MHz", result.ActualValue);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task RunSingleTest_FrequencyRangeTest_WhenOutOfBand_ShouldFail()
    {
        // Arrange
        var device = new Device
        {
            Id = "RF-001",
            Name = "Signal Generator Simulator",
            FrequencyMHz = 2550.0,
            Thresholds = new ThresholdConfig
            {
                DeviceId = "RF-001",
                FrequencyMin = 2400.0,
                FrequencyMax = 2500.0
            }
        };

        var telemetry = new TelemetryData
        {
            DeviceId = "RF-001",
            FrequencyMHz = 2550.0,
            SignalPowerDbm = -30.0,
            TemperatureC = 42.0,
            VoltageV = 12.0,
            CurrentA = 1.8
        };

        _mockUow.Setup(u => u.Devices.GetByIdWithThresholdsAsync("RF-001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(device);

        _mockComm.Setup(c => c.ReceiveTelemetryAsync("RF-001", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(telemetry);

        _mockUow.Setup(u => u.Automation.AddAsync(It.IsAny<AutomationTestResult>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        // Act
        var result = await _engine.RunSingleTestAsync("Frequency Range Test", "RF-001");

        // Assert
        Assert.Equal(TestResultStatus.FAIL, result.Status);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("outside acceptable band", result.ErrorMessage);
    }

    [Fact]
    public async Task RunSingleTest_TemperatureSafetyTest_WhenThermalExceeded_ShouldFail()
    {
        // Arrange
        var device = new Device
        {
            Id = "RF-001",
            Name = "Signal Generator Simulator",
            Thresholds = new ThresholdConfig
            {
                DeviceId = "RF-001",
                TemperatureWarning = 60.0
            }
        };

        var telemetry = new TelemetryData
        {
            DeviceId = "RF-001",
            TemperatureC = 68.5,
            Status = DeviceStatus.WARNING
        };

        _mockUow.Setup(u => u.Devices.GetByIdWithThresholdsAsync("RF-001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(device);

        _mockComm.Setup(c => c.ReceiveTelemetryAsync("RF-001", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(telemetry);

        _mockUow.Setup(u => u.Automation.AddAsync(It.IsAny<AutomationTestResult>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        // Act
        var result = await _engine.RunSingleTestAsync("Temperature Safety Test", "RF-001");

        // Assert
        Assert.Equal(TestResultStatus.FAIL, result.Status);
        Assert.Contains("exceeds safe limit", result.ErrorMessage);
    }
}
