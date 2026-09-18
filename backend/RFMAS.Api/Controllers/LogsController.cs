using Microsoft.AspNetCore.Mvc;
using RFMAS.Api.DTOs;
using RFMAS.Core.Interfaces;

namespace RFMAS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LogsController : ControllerBase
{
    private readonly ILogService _logService;

    public LogsController(ILogService logService)
    {
        _logService = logService;
    }

    /// <summary>
    /// Returns centralized system audit logs with optional device and category filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventLogDto>>> GetLogs(
        [FromQuery] string? deviceId = null,
        [FromQuery] string? category = null,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var logs = await _logService.GetLogsAsync(deviceId, category, Math.Clamp(limit, 10, 500), cancellationToken);
        return Ok(logs.Select(EventLogDto.FromEntity));
    }
}
