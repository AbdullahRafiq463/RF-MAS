using Microsoft.AspNetCore.Mvc;
using RFMAS.Api.DTOs;
using RFMAS.DeviceSimulator;

namespace RFMAS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SimulationController : ControllerBase
{
    private readonly DeviceSimulatorEngine _simulatorEngine;

    public SimulationController(DeviceSimulatorEngine simulatorEngine)
    {
        _simulatorEngine = simulatorEngine;
    }

    /// <summary>
    /// Returns the live simulator status and active scenario for all devices.
    /// </summary>
    [HttpGet("devices")]
    [ProducesResponseType(typeof(IEnumerable<SimulationDeviceStatusDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<SimulationDeviceStatusDto>> GetSimulationDevices()
    {
        var devices = _simulatorEngine.GetAllDevices();
        var dtos = devices.Select(d => new SimulationDeviceStatusDto
        {
            DeviceId = d.DeviceId,
            Name = d.Name,
            ActiveScenario = d.ActiveScenario.ToString(),
            IsConnected = d.IsConnected,
            Status = d.Status.ToString()
        });
        return Ok(dtos);
    }

    /// <summary>
    /// Injects a failure or operating condition scenario into a simulated device for live demonstrations.
    /// Scenarios: NORMAL, HIGH_TEMPERATURE, LOW_SIGNAL, LOW_VOLTAGE, DISCONNECT, COMMUNICATION_TIMEOUT, ERROR
    /// </summary>
    [HttpPost("devices/{deviceId}/scenario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult InjectScenario(string deviceId, [FromBody] InjectScenarioDto request)
    {
        var success = _simulatorEngine.InjectScenario(deviceId, request.Scenario, request.CustomValue);
        if (!success)
        {
            return NotFound(new { message = $"Simulated device '{deviceId}' not found." });
        }

        return Ok(new
        {
            message = $"Simulation scenario '{request.Scenario}' successfully injected into device '{deviceId}'.",
            deviceId,
            scenario = request.Scenario.ToString(),
            injectedAt = DateTime.UtcNow
        });
    }
}
