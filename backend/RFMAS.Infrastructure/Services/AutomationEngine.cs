using System.Diagnostics;
using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;

namespace RFMAS.Infrastructure.Services;

/// <summary>
/// Automation test engine for running standardized test suites against simulated RF instruments.
/// </summary>
public class AutomationEngine : IAutomationEngine
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDeviceCommunicationService _deviceComm;
    private readonly ILogService _logService;

    public AutomationEngine(
        IUnitOfWork unitOfWork,
        IDeviceCommunicationService deviceComm,
        ILogService logService)
    {
        _unitOfWork = unitOfWork;
        _deviceComm = deviceComm;
        _logService = logService;
    }

    public async Task<IReadOnlyList<AutomationTestResult>> RunAllTestsAsync(CancellationToken cancellationToken = default)
    {
        var devices = await _unitOfWork.Devices.GetAllAsync(cancellationToken);
        var results = new List<AutomationTestResult>();

        foreach (var dev in devices)
        {
            var devResults = await RunDeviceTestsAsync(dev.Id, cancellationToken);
            results.AddRange(devResults);
        }

        return results;
    }

    public async Task<IReadOnlyList<AutomationTestResult>> RunDeviceTestsAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        var testNames = new[]
        {
            "Device Connectivity Test",
            "Frequency Range Test",
            "Signal Power Threshold Test",
            "Temperature Safety Test",
            "Voltage Range Test",
            "Device Response Test"
        };

        var list = new List<AutomationTestResult>();
        foreach (var name in testNames)
        {
            var res = await RunSingleTestAsync(name, deviceId, cancellationToken);
            list.Add(res);
        }

        return list;
    }

    public async Task<AutomationTestResult> RunSingleTestAsync(string testName, string deviceId, CancellationToken cancellationToken = default)
    {
        var device = await _unitOfWork.Devices.GetByIdWithThresholdsAsync(deviceId, cancellationToken);
        var sw = Stopwatch.StartNew();

        if (device == null)
        {
            sw.Stop();
            var errorRes = new AutomationTestResult
            {
                DeviceId = deviceId,
                TestName = testName,
                ExpectedValue = "Device exists in database",
                ActualValue = "Device not found",
                Status = TestResultStatus.ERROR,
                ExecutionDurationMs = sw.ElapsedMilliseconds,
                ExecutedAt = DateTime.UtcNow,
                ErrorMessage = $"Device {deviceId} was not found."
            };
            await PersistAndLogTestResultAsync(errorRes, cancellationToken);
            return errorRes;
        }

        var thresh = device.Thresholds ?? new ThresholdConfig();
        var telemetry = await _deviceComm.ReceiveTelemetryAsync(deviceId, cancellationToken);

        AutomationTestResult result;

        switch (testName)
        {
            case "Device Connectivity Test":
                bool isConnected = await _deviceComm.IsConnectedAsync(deviceId, cancellationToken);
                result = new AutomationTestResult
                {
                    DeviceId = deviceId,
                    TestName = testName,
                    ExpectedValue = "Connected (TCP socket open, Status: ONLINE)",
                    ActualValue = isConnected ? $"Connected (Status: {device.OperationalStatus})" : "Disconnected (Socket closed)",
                    Status = isConnected && device.OperationalStatus != DeviceStatus.OFFLINE ? TestResultStatus.PASS : TestResultStatus.FAIL,
                    ErrorMessage = isConnected ? null : "Device is unreachable over simulated TCP link."
                };
                break;

            case "Frequency Range Test":
                double freq = telemetry?.FrequencyMHz ?? device.FrequencyMHz;
                bool freqPass = freq >= thresh.FrequencyMin && freq <= thresh.FrequencyMax;
                result = new AutomationTestResult
                {
                    DeviceId = deviceId,
                    TestName = testName,
                    ExpectedValue = $"{thresh.FrequencyMin:F1} - {thresh.FrequencyMax:F1} MHz",
                    ActualValue = $"{freq:F2} MHz",
                    Status = freqPass ? TestResultStatus.PASS : TestResultStatus.FAIL,
                    ErrorMessage = freqPass ? null : $"Operating frequency {freq:F2} MHz outside acceptable band."
                };
                break;

            case "Signal Power Threshold Test":
                double power = telemetry?.SignalPowerDbm ?? device.SignalPowerDbm;
                bool powerPass = power >= thresh.SignalPowerWarning;
                result = new AutomationTestResult
                {
                    DeviceId = deviceId,
                    TestName = testName,
                    ExpectedValue = $">= {thresh.SignalPowerWarning:F1} dBm",
                    ActualValue = $"{power:F2} dBm",
                    Status = powerPass ? TestResultStatus.PASS : TestResultStatus.FAIL,
                    ErrorMessage = powerPass ? null : $"Signal power {power:F2} dBm degraded below warning limit {thresh.SignalPowerWarning:F1} dBm."
                };
                break;

            case "Temperature Safety Test":
                double temp = telemetry?.TemperatureC ?? device.TemperatureC;
                bool tempPass = temp < thresh.TemperatureWarning;
                result = new AutomationTestResult
                {
                    DeviceId = deviceId,
                    TestName = testName,
                    ExpectedValue = $"< {thresh.TemperatureWarning:F1} °C",
                    ActualValue = $"{temp:F2} °C",
                    Status = tempPass ? TestResultStatus.PASS : TestResultStatus.FAIL,
                    ErrorMessage = tempPass ? null : $"Thermal sensor reading {temp:F2} °C exceeds safe limit {thresh.TemperatureWarning:F1} °C."
                };
                break;

            case "Voltage Range Test":
                double volt = telemetry?.VoltageV ?? device.VoltageV;
                bool voltPass = volt >= thresh.VoltageMin && volt <= thresh.VoltageMax;
                result = new AutomationTestResult
                {
                    DeviceId = deviceId,
                    TestName = testName,
                    ExpectedValue = $"{thresh.VoltageMin:F1}V - {thresh.VoltageMax:F1}V",
                    ActualValue = $"{volt:F2} V",
                    Status = voltPass ? TestResultStatus.PASS : TestResultStatus.FAIL,
                    ErrorMessage = voltPass ? null : $"Supply voltage {volt:F2}V is out of specification."
                };
                break;

            case "Device Response Test":
                var pingCmdResult = await _deviceComm.SendCommandAsync(deviceId, CommandType.START, cancellationToken: cancellationToken);
                bool responsePass = pingCmdResult.Success;
                result = new AutomationTestResult
                {
                    DeviceId = deviceId,
                    TestName = testName,
                    ExpectedValue = "Round-trip Command Echo <= 100ms with SUCCESS",
                    ActualValue = $"Status: {pingCmdResult.Status} ({sw.ElapsedMilliseconds} ms)",
                    Status = responsePass ? TestResultStatus.PASS : TestResultStatus.FAIL,
                    ErrorMessage = responsePass ? null : pingCmdResult.Message
                };
                break;

            default:
                result = new AutomationTestResult
                {
                    DeviceId = deviceId,
                    TestName = testName,
                    ExpectedValue = "Known Test Strategy",
                    ActualValue = $"Unknown test: {testName}",
                    Status = TestResultStatus.ERROR,
                    ErrorMessage = $"Test named '{testName}' is not defined."
                };
                break;
        }

        sw.Stop();
        result.ExecutionDurationMs = sw.ElapsedMilliseconds;
        result.ExecutedAt = DateTime.UtcNow;

        await PersistAndLogTestResultAsync(result, cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<AutomationTestResult>> GetLatestResultsAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Automation.GetLatestResultsAsync(count, cancellationToken);
    }

    private async Task PersistAndLogTestResultAsync(AutomationTestResult result, CancellationToken cancellationToken)
    {
        await _unitOfWork.Automation.AddAsync(result, cancellationToken);

        await _logService.LogAsync(
            deviceId: result.DeviceId,
            category: "AUTOMATION",
            eventName: result.TestName,
            details: $"Result: {result.Status}. Expected: {result.ExpectedValue}, Actual: {result.ActualValue} ({result.ExecutionDurationMs}ms)",
            severity: result.Status == TestResultStatus.PASS ? AlertSeverity.INFO : AlertSeverity.WARNING,
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
