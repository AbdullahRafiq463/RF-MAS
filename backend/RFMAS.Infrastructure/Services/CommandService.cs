using System.Text.Json;
using RFMAS.Core.Entities;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;
using RFMAS.Core.Models;

namespace RFMAS.Infrastructure.Services;

/// <summary>
/// Orchestrates, validates, records, and dispatches control commands to RF devices.
/// Enforces safety validation so client requests can never blindly exceed instrument thresholds.
/// </summary>
public class CommandService : ICommandService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDeviceCommunicationService _deviceComm;
    private readonly ILogService _logService;

    public CommandService(
        IUnitOfWork unitOfWork,
        IDeviceCommunicationService deviceComm,
        ILogService logService)
    {
        _unitOfWork = unitOfWork;
        _deviceComm = deviceComm;
        _logService = logService;
    }

    public async Task<CommandResult> ExecuteCommandAsync(
        string deviceId,
        CommandType type,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var device = await _unitOfWork.Devices.GetByIdWithThresholdsAsync(deviceId, cancellationToken);
        if (device == null)
        {
            return CommandResult.Invalid(deviceId, type, $"Device '{deviceId}' not found.");
        }

        // 1. Safety Validation
        var validationError = ValidateCommandParameters(device, type, parameters);
        if (validationError != null)
        {
            await RecordCommandLogAsync(deviceId, type, parameters, CommandStatus.INVALID, validationError, cancellationToken);
            return CommandResult.Invalid(deviceId, type, validationError);
        }

        // 2. Dispatch to device communication layer
        var result = await _deviceComm.SendCommandAsync(deviceId, type, parameters, cancellationToken);

        // 3. Update device database state if successful
        if (result.Success)
        {
            if (type == CommandType.START || type == CommandType.EXIT_MAINTENANCE || type == CommandType.RESET)
            {
                device.OperationalStatus = DeviceStatus.ONLINE;
                device.ConnectionStatus = true;
            }
            else if (type == CommandType.STOP)
            {
                device.OperationalStatus = DeviceStatus.OFFLINE;
            }
            else if (type == CommandType.ENTER_MAINTENANCE)
            {
                device.OperationalStatus = DeviceStatus.MAINTENANCE;
            }

            if (parameters != null)
            {
                if (parameters.TryGetValue("frequencyMHz", out var fVal) && double.TryParse(fVal.ToString(), out var f))
                {
                    device.FrequencyMHz = f;
                }
                if (parameters.TryGetValue("powerDbm", out var pVal) && double.TryParse(pVal.ToString(), out var p))
                {
                    device.SignalPowerDbm = p;
                }
            }

            await _unitOfWork.Devices.UpdateAsync(device, cancellationToken);
        }

        // 4. Record command history & system event log
        await RecordCommandLogAsync(deviceId, type, parameters, result.Status, result.Message, cancellationToken);

        return result;
    }

    public Task<CommandResult> SetFrequencyAsync(string deviceId, double frequencyMHz, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object>
        {
            { "frequencyMHz", frequencyMHz }
        };
        return ExecuteCommandAsync(deviceId, CommandType.SET_FREQUENCY, parameters, cancellationToken);
    }

    public Task<CommandResult> SetPowerAsync(string deviceId, double powerDbm, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object>
        {
            { "powerDbm", powerDbm }
        };
        return ExecuteCommandAsync(deviceId, CommandType.SET_POWER, parameters, cancellationToken);
    }

    private static string? ValidateCommandParameters(Device device, CommandType type, Dictionary<string, object>? parameters)
    {
        if (type == CommandType.SET_FREQUENCY)
        {
            if (parameters == null || !parameters.TryGetValue("frequencyMHz", out var freqObj) ||
                !double.TryParse(freqObj.ToString(), out var freq))
            {
                return "Frequency value is required and must be numeric.";
            }

            var thresh = device.Thresholds;
            if (thresh != null && (freq < thresh.FrequencyMin || freq > thresh.FrequencyMax))
            {
                return $"Frequency {freq:F2} MHz is out of allowed instrument range ({thresh.FrequencyMin:F1} - {thresh.FrequencyMax:F1} MHz).";
            }
        }

        if (type == CommandType.SET_POWER)
        {
            if (parameters == null || !parameters.TryGetValue("powerDbm", out var pwrObj) ||
                !double.TryParse(pwrObj.ToString(), out var power))
            {
                return "Signal power value is required and must be numeric.";
            }

            if (power < -100.0 || power > 30.0)
            {
                return $"Signal power {power:F2} dBm is out of safe operational limit (-100 dBm to +30 dBm).";
            }
        }

        return null;
    }

    private async Task RecordCommandLogAsync(
        string deviceId,
        CommandType type,
        Dictionary<string, object>? parameters,
        CommandStatus status,
        string message,
        CancellationToken cancellationToken)
    {
        var commandEntity = new DeviceCommand
        {
            DeviceId = deviceId,
            CommandType = type,
            ParametersJson = parameters != null ? JsonSerializer.Serialize(parameters) : null,
            Status = status,
            ResultMessage = message,
            IssuedAt = DateTime.UtcNow,
            ExecutedAt = DateTime.UtcNow
        };

        await _unitOfWork.Commands.AddAsync(commandEntity, cancellationToken);

        await _logService.LogAsync(
            deviceId: deviceId,
            category: "COMMAND",
            eventName: type.ToString(),
            details: $"Status: {status}. {message}",
            severity: status == CommandStatus.SUCCESS ? AlertSeverity.INFO : AlertSeverity.WARNING,
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
