using RFMAS.Core.Enums;
using RFMAS.DeviceSimulator;
using Xunit;

namespace RFMAS.Tests;

public class DeviceSimulatorTests
{
    [Fact]
    public void DeviceSimulatorEngine_Initialization_ShouldContainFiveDefaultDevices()
    {
        // Arrange & Act
        var engine = new DeviceSimulatorEngine();
        var devices = engine.GetAllDevices();

        // Assert
        Assert.Equal(5, devices.Count);
        Assert.Contains(devices, d => d.DeviceId == "RF-001");
        Assert.Contains(devices, d => d.DeviceId == "RF-002");
        Assert.Contains(devices, d => d.DeviceId == "RF-003");
        Assert.Contains(devices, d => d.DeviceId == "RF-004");
        Assert.Contains(devices, d => d.DeviceId == "RF-005");
    }

    [Fact]
    public void InjectScenario_HighTemperature_ShouldProduceTelemetryWithCriticalTemperature()
    {
        // Arrange
        var engine = new DeviceSimulatorEngine();

        // Act
        bool injected = engine.InjectScenario("RF-001", SimulationScenarioType.HIGH_TEMPERATURE, 78.5);
        var telemetry = engine.GenerateTelemetryForDevice("RF-001");

        // Assert
        Assert.True(injected);
        Assert.NotNull(telemetry);
        Assert.Equal(78.5, telemetry.TemperatureC);
        Assert.Equal(DeviceStatus.ERROR, telemetry.Status);
    }

    [Fact]
    public void ExecuteCommand_StartAndStop_ShouldToggleDeviceOperationalStatus()
    {
        // Arrange
        var engine = new DeviceSimulatorEngine();

        // Act - Stop
        var stopResult = engine.ExecuteCommand("RF-001", CommandType.STOP, null);
        var telemetryStopped = engine.GenerateTelemetryForDevice("RF-001");

        // Assert Stop
        Assert.True(stopResult.Success);
        Assert.Equal(DeviceStatus.OFFLINE, telemetryStopped?.Status);

        // Act - Start
        var startResult = engine.ExecuteCommand("RF-001", CommandType.START, null);
        var telemetryStarted = engine.GenerateTelemetryForDevice("RF-001");

        // Assert Start
        Assert.True(startResult.Success);
        Assert.Equal(DeviceStatus.ONLINE, telemetryStarted?.Status);
    }
}
