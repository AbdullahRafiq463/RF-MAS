using Microsoft.AspNetCore.Mvc;
using RFMAS.Api.DTOs;
using RFMAS.Core.Enums;
using RFMAS.Core.Interfaces;

namespace RFMAS.Api.Controllers;

[ApiController]
[Route("api/devices/{id}")]
[Produces("application/json")]
public class CommandsController : ControllerBase
{
    private readonly ICommandService _commandService;

    public CommandsController(ICommandService commandService)
    {
        _commandService = commandService;
    }

    /// <summary>
    /// Starts the specified RF instrument and transitions status to ONLINE.
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(CommandResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CommandResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommandResponseDto>> StartDevice(string id, CancellationToken cancellationToken)
    {
        var result = await _commandService.ExecuteCommandAsync(id, CommandType.START, cancellationToken: cancellationToken);
        var dto = CommandResponseDto.FromResult(result);
        return result.Success ? Ok(dto) : BadRequest(dto);
    }

    /// <summary>
    /// Stops the specified RF instrument and transitions status to OFFLINE.
    /// </summary>
    [HttpPost("stop")]
    [ProducesResponseType(typeof(CommandResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CommandResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommandResponseDto>> StopDevice(string id, CancellationToken cancellationToken)
    {
        var result = await _commandService.ExecuteCommandAsync(id, CommandType.STOP, cancellationToken: cancellationToken);
        var dto = CommandResponseDto.FromResult(result);
        return result.Success ? Ok(dto) : BadRequest(dto);
    }

    /// <summary>
    /// Resets the specified RF instrument to default nominal operating values.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(CommandResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CommandResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommandResponseDto>> ResetDevice(string id, CancellationToken cancellationToken)
    {
        var result = await _commandService.ExecuteCommandAsync(id, CommandType.RESET, cancellationToken: cancellationToken);
        var dto = CommandResponseDto.FromResult(result);
        return result.Success ? Ok(dto) : BadRequest(dto);
    }

    /// <summary>
    /// Configures the RF operating frequency within safe instrument limits.
    /// </summary>
    [HttpPost("frequency")]
    [ProducesResponseType(typeof(CommandResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CommandResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommandResponseDto>> SetFrequency(
        string id,
        [FromBody] SetFrequencyRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _commandService.SetFrequencyAsync(id, request.FrequencyMHz, cancellationToken);
        var dto = CommandResponseDto.FromResult(result);
        return result.Success ? Ok(dto) : BadRequest(dto);
    }

    /// <summary>
    /// Configures the RF output signal power within safe operational limits (-100 to +30 dBm).
    /// </summary>
    [HttpPost("power")]
    [ProducesResponseType(typeof(CommandResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CommandResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommandResponseDto>> SetPower(
        string id,
        [FromBody] SetPowerRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _commandService.SetPowerAsync(id, request.PowerDbm, cancellationToken);
        var dto = CommandResponseDto.FromResult(result);
        return result.Success ? Ok(dto) : BadRequest(dto);
    }

    /// <summary>
    /// Dispatches a generic command to the device (ENTER_MAINTENANCE, EXIT_MAINTENANCE, RESTART, etc.)
    /// </summary>
    [HttpPost("command")]
    [ProducesResponseType(typeof(CommandResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CommandResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommandResponseDto>> GenericCommand(
        string id,
        [FromBody] GenericCommandRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _commandService.ExecuteCommandAsync(id, request.CommandType, request.Parameters, cancellationToken);
        var dto = CommandResponseDto.FromResult(result);
        return result.Success ? Ok(dto) : BadRequest(dto);
    }
}
