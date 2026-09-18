using Microsoft.AspNetCore.Mvc;
using RFMAS.Api.DTOs;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;

namespace RFMAS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AlertsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAlertEngine _alertEngine;

    public AlertsController(IUnitOfWork unitOfWork, IAlertEngine alertEngine)
    {
        _unitOfWork = unitOfWork;
        _alertEngine = alertEngine;
    }

    /// <summary>
    /// Returns alerts across all devices with optional filtering for active/acknowledged state and severity.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AlertDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetAlerts(
        [FromQuery] bool? activeOnly = null,
        [FromQuery] AlertSeverity? severity = null,
        CancellationToken cancellationToken = default)
    {
        var alerts = await _unitOfWork.Alerts.GetAllAsync(activeOnly, severity, cancellationToken);
        return Ok(alerts.Select(AlertDto.FromEntity));
    }

    /// <summary>
    /// Acknowledges an active alert.
    /// </summary>
    [HttpPost("{id}/acknowledge")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcknowledgeAlert(
        long id,
        [FromBody] AcknowledgeAlertRequestDto? request,
        CancellationToken cancellationToken)
    {
        var success = await _alertEngine.AcknowledgeAlertAsync(id, request?.AcknowledgedBy ?? "Operator", cancellationToken);
        if (!success)
        {
            return NotFound(new { message = $"Alert #{id} was not found or has already been acknowledged." });
        }

        return Ok(new { message = $"Alert #{id} acknowledged successfully." });
    }
}
