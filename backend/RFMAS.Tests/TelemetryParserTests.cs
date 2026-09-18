using RFMAS.Core.Enums;
using RFMAS.Core.Models;
using RFMAS.Infrastructure.Communication;
using Xunit;

namespace RFMAS.Tests;

public class TelemetryParserTests
{
    private readonly TelemetryMessageParser _parser = new();

    [Fact]
    public void Parse_ValidCompleteTelemetryMessage_ShouldParseCorrectly()
    {
        // Arrange
        string raw = "RF-001|2450.25|-32.50|41.30|12.10|1.80|ONLINE|2026-09-18T20:00:00.0000000Z";

        // Act
        var result = _parser.Parse(raw);

        // Assert
        Assert.Equal("RF-001", result.DeviceId);
        Assert.Equal(2450.25, result.FrequencyMHz);
        Assert.Equal(-32.50, result.SignalPowerDbm);
        Assert.Equal(41.30, result.TemperatureC);
        Assert.Equal(12.10, result.VoltageV);
        Assert.Equal(1.80, result.CurrentA);
        Assert.Equal(DeviceStatus.ONLINE, result.Status);
    }

    [Fact]
    public void Parse_CompactSixPartMessage_ShouldParseWithDefaults()
    {
        // Arrange
        string raw = "RF-002|5200.00|-25.00|44.50|12.00|2.10";

        // Act
        var result = _parser.Parse(raw);

        // Assert
        Assert.Equal("RF-002", result.DeviceId);
        Assert.Equal(5200.00, result.FrequencyMHz);
        Assert.Equal(-25.00, result.SignalPowerDbm);
        Assert.Equal(44.50, result.TemperatureC);
        Assert.Equal(12.00, result.VoltageV);
        Assert.Equal(2.10, result.CurrentA);
        Assert.Equal(DeviceStatus.ONLINE, result.Status);
    }

    [Fact]
    public void Parse_InvalidSegmentCount_ShouldThrowFormatException()
    {
        // Arrange
        string invalid = "RF-001|2450.25|-32.50";

        // Act & Assert
        Assert.Throws<FormatException>(() => _parser.Parse(invalid));
    }

    [Fact]
    public void Format_ThenParse_ShouldRoundTripLosslessly()
    {
        // Arrange
        var original = new TelemetryData
        {
            DeviceId = "RF-003",
            FrequencyMHz = 915.25,
            SignalPowerDbm = -15.40,
            TemperatureC = 48.10,
            VoltageV = 11.95,
            CurrentA = 2.85,
            Status = DeviceStatus.WARNING,
            Timestamp = DateTime.UtcNow
        };

        // Act
        string formatted = _parser.Format(original);
        var parsed = _parser.Parse(formatted);

        // Assert
        Assert.Equal(original.DeviceId, parsed.DeviceId);
        Assert.Equal(original.FrequencyMHz, parsed.FrequencyMHz);
        Assert.Equal(original.SignalPowerDbm, parsed.SignalPowerDbm);
        Assert.Equal(original.TemperatureC, parsed.TemperatureC);
        Assert.Equal(original.VoltageV, parsed.VoltageV);
        Assert.Equal(original.CurrentA, parsed.CurrentA);
        Assert.Equal(original.Status, parsed.Status);
    }
}
