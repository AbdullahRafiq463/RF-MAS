using Microsoft.AspNetCore.Mvc;
using RFMAS.Api.DTOs;
using RFMAS.Core.Interfaces;

namespace RFMAS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DevicesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public DevicesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Returns a list of all simulated RF devices with current operating status and health.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DeviceSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DeviceSummaryDto>>> GetDevices(CancellationToken cancellationToken)
    {
        var devices = await _unitOfWork.Devices.GetAllAsync(cancellationToken);
        var dtos = devices.Select(DeviceSummaryDto.FromEntity);
        return Ok(dtos);
    }

    /// <summary>
    /// Returns detailed hardware specifications, threshold rules, and recent telemetry for a specific device.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DeviceDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceDetailDto>> GetDevice(string id, CancellationToken cancellationToken)
    {
        var device = await _unitOfWork.Devices.GetByIdWithThresholdsAsync(id, cancellationToken);
        if (device == null)
        {
            return NotFound(new { message = $"Device with ID '{id}' was not found." });
        }

        var recentTelemetry = await _unitOfWork.Telemetry.GetRecentByDeviceAsync(id, 60, cancellationToken);
        var recentAlerts = await _unitOfWork.Alerts.GetRecentByDeviceAsync(id, 20, cancellationToken);

        return Ok(DeviceDetailDto.FromEntity(device, recentTelemetry, recentAlerts));
    }

    /// <summary>
    /// Returns recent historical telemetry data points for visualization and time-series charting.
    /// </summary>
    [HttpGet("{id}/telemetry")]
    [ProducesResponseType(typeof(IEnumerable<TelemetryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TelemetryDto>>> GetDeviceTelemetry(
        string id,
        [FromQuery] int count = 50,
        CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.Devices.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            return NotFound(new { message = $"Device with ID '{id}' was not found." });
        }

        var records = await _unitOfWork.Telemetry.GetRecentByDeviceAsync(id, Math.Clamp(count, 5, 200), cancellationToken);
        return Ok(records.Select(TelemetryDto.FromEntity));
    }

    /// <summary>
    /// Returns alerts associated with a specific device.
    /// </summary>
    [HttpGet("{id}/alerts")]
    [ProducesResponseType(typeof(IEnumerable<AlertDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetDeviceAlerts(
        string id,
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        var alerts = await _unitOfWork.Alerts.GetRecentByDeviceAsync(id, count, cancellationToken);
        return Ok(alerts.Select(AlertDto.FromEntity));
    }
}
