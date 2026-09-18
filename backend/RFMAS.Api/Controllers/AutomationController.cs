using Microsoft.AspNetCore.Mvc;
using RFMAS.Api.DTOs;
using RFMAS.Core.Interfaces;

namespace RFMAS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AutomationController : ControllerBase
{
    private readonly IAutomationEngine _automationEngine;

    public AutomationController(IAutomationEngine automationEngine)
    {
        _automationEngine = automationEngine;
    }

    /// <summary>
    /// Returns recent automation test execution records.
    /// </summary>
    [HttpGet("tests")]
    [ProducesResponseType(typeof(IEnumerable<AutomationTestResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AutomationTestResultDto>>> GetRecentTests(
        [FromQuery] int count = 50,
        CancellationToken cancellationToken = default)
    {
        var results = await _automationEngine.GetLatestResultsAsync(count, cancellationToken);
        return Ok(results.Select(AutomationTestResultDto.FromEntity));
    }

    /// <summary>
    /// Executes the full test suite (all 6 tests across all simulated devices).
    /// </summary>
    [HttpPost("tests/run")]
    [ProducesResponseType(typeof(IEnumerable<AutomationTestResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AutomationTestResultDto>>> RunAllTests(CancellationToken cancellationToken)
    {
        var results = await _automationEngine.RunAllTestsAsync(cancellationToken);
        return Ok(results.Select(AutomationTestResultDto.FromEntity));
    }

    /// <summary>
    /// Executes test suite for a specific device.
    /// </summary>
    [HttpPost("devices/{deviceId}/run")]
    [ProducesResponseType(typeof(IEnumerable<AutomationTestResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AutomationTestResultDto>>> RunDeviceTests(
        string deviceId,
        CancellationToken cancellationToken)
    {
        var results = await _automationEngine.RunDeviceTestsAsync(deviceId, cancellationToken);
        return Ok(results.Select(AutomationTestResultDto.FromEntity));
    }

    /// <summary>
    /// Executes a single specific test on a device.
    /// </summary>
    [HttpPost("test/run-single")]
    [ProducesResponseType(typeof(AutomationTestResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AutomationTestResultDto>> RunSingleTest(
        [FromBody] RunTestRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _automationEngine.RunSingleTestAsync(request.TestName, request.DeviceId, cancellationToken);
        return Ok(AutomationTestResultDto.FromEntity(result));
    }
}
