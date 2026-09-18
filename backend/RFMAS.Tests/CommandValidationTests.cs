using Moq;
using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;
using RFMAS.Core.Models;
using RFMAS.Infrastructure.Services;
using Xunit;

namespace RFMAS.Tests;

public class CommandValidationTests
{
    private readonly Mock<IUnitOfWork> _mockUow = new();
    private readonly Mock<IDeviceCommunicationService> _mockComm = new();
    private readonly Mock<ILogService> _mockLog = new();
    private readonly CommandService _commandService;

    public CommandValidationTests()
    {
        _commandService = new CommandService(_mockUow.Object, _mockComm.Object, _mockLog.Object);
    }

    [Fact]
    public async Task SetFrequency_WhenWithinAllowedThresholdRange_ShouldSucceed()
    {
        // Arrange
        var device = new Device
        {
            Id = "RF-001",
            Name = "Signal Generator Simulator",
            Thresholds = new ThresholdConfig
            {
                DeviceId = "RF-001",
                FrequencyMin = 2400.0,
                FrequencyMax = 2500.0
            }
        };

        _mockUow.Setup(u => u.Devices.GetByIdWithThresholdsAsync("RF-001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(device);

        _mockComm.Setup(c => c.SendCommandAsync("RF-001", CommandType.SET_FREQUENCY, It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(CommandResult.Successful("RF-001", CommandType.SET_FREQUENCY, "Frequency updated"));

        _mockUow.Setup(u => u.Commands.AddAsync(It.IsAny<DeviceCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        _mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

        // Act
        var result = await _commandService.SetFrequencyAsync("RF-001", 2450.0);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(CommandStatus.SUCCESS, result.Status);
    }

    [Theory]
    [InlineData(2300.0)] // Below 2400
    [InlineData(2600.0)] // Above 2500
    public async Task SetFrequency_WhenOutOfRange_ShouldRejectAsInvalid(double invalidFreq)
    {
        // Arrange
        var device = new Device
        {
            Id = "RF-001",
            Name = "Signal Generator Simulator",
            Thresholds = new ThresholdConfig
            {
                DeviceId = "RF-001",
                FrequencyMin = 2400.0,
                FrequencyMax = 2500.0
            }
        };

        _mockUow.Setup(u => u.Devices.GetByIdWithThresholdsAsync("RF-001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(device);

        _mockUow.Setup(u => u.Commands.AddAsync(It.IsAny<DeviceCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        _mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

        // Act
        var result = await _commandService.SetFrequencyAsync("RF-001", invalidFreq);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(CommandStatus.INVALID, result.Status);
        Assert.Contains("out of allowed instrument range", result.Message);
    }

    [Theory]
    [InlineData(-120.0)] // Less than -100 dBm
    [InlineData(45.0)]   // Greater than +30 dBm
    public async Task SetPower_WhenOutOfOperationalLimit_ShouldRejectAsInvalid(double invalidPower)
    {
        // Arrange
        var device = new Device
        {
            Id = "RF-001",
            Name = "Signal Generator Simulator",
            Thresholds = new ThresholdConfig { DeviceId = "RF-001" }
        };

        _mockUow.Setup(u => u.Devices.GetByIdWithThresholdsAsync("RF-001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(device);

        _mockUow.Setup(u => u.Commands.AddAsync(It.IsAny<DeviceCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        // Act
        var result = await _commandService.SetPowerAsync("RF-001", invalidPower);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(CommandStatus.INVALID, result.Status);
        Assert.Contains("out of safe operational limit", result.Message);
    }
}
