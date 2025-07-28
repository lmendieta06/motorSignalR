using Microsoft.AspNetCore.Mvc;
using MotorControl.Application.DTOs.Requests;
using MotorControl.Application.DTOs.Responses;
using MotorControl.Application.Interfaces;

namespace MotorControl.API.Controllers;

[ApiController]
[Route("api/motor")]
public class MotorController : ControllerBase
{
    private readonly IMotorControlService _motorControlService;
    private readonly ILogger<MotorController> _logger;

    public MotorController(IMotorControlService motorControlService, ILogger<MotorController> logger)
    {
        _motorControlService = motorControlService;
        _logger = logger;
    }

    /// <summary>
    /// Sets the motor speed
    /// </summary>
    /// <param name="request">Speed value between 0 and 100</param>
    /// <returns>Motor response with current status</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/motor/speed
    ///     {
    ///        "speed": 50
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Speed set successfully</response>
    /// <response code="400">Invalid speed value</response>
    [HttpPost("speed")]
    public async Task<ActionResult<MotorResponse>> SetSpeed([FromBody] SetSpeedRequest request)
    {
        try
        {
            _logger.LogInformation("Setting motor speed to {Speed}", request.Speed);
            
            var result = await _motorControlService.SetSpeedAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting motor speed");
            return StatusCode(500, new MotorResponse(false, "Internal server error"));
        }
    }

    /// <summary>
    /// Changes the driving mode
    /// </summary>
    /// <param name="request">Driving mode: Eco, Normal, or Sport</param>
    /// <returns>Motor response with current status</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/motor/mode
    ///     {
    ///        "mode": "Sport"
    ///     }
    ///
    /// Valid modes: Eco, Normal, Sport
    /// </remarks>
    /// <response code="200">Mode changed successfully</response>
    /// <response code="400">Invalid mode</response>
    [HttpPost("mode")]
    public async Task<ActionResult<MotorResponse>> ChangeMode([FromBody] ChangeModeRequest request)
    {
        try
        {
            _logger.LogInformation("Changing motor mode to {Mode}", request.Mode);
            
            var result = await _motorControlService.ChangeModeAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing motor mode");
            return StatusCode(500, new MotorResponse(false, "Internal server error"));
        }
    }

    /// <summary>
    /// Gets the current motor status
    /// </summary>
    /// <returns>Current motor status and telemetry</returns>
    /// <remarks>
    /// Returns real-time motor data including:
    /// - Current and target speed
    /// - Temperature and RPM
    /// - Driving mode and status
    /// - Power output
    /// </remarks>
    /// <response code="200">Motor status retrieved successfully</response>
    [HttpGet("status")]
    public async Task<ActionResult<MotorStatusResponse>> GetStatus()
    {
        try
        {
            _logger.LogInformation("Getting motor status");
            
            var result = await _motorControlService.GetStatusAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting motor status");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Emergency stop - immediately stops the motor
    /// </summary>
    /// <returns>Motor response confirming emergency stop</returns>
    /// <remarks>
    /// EMERGENCY STOP: Immediately sets speed to 0 and puts motor in emergency state.
    /// Motor will not respond to other commands until restarted.
    /// 
    /// No request body required.
    /// </remarks>
    /// <response code="200">Emergency stop activated successfully</response>
    [HttpPost("stop")]
    public async Task<ActionResult<MotorResponse>> EmergencyStop()
    {
        try
        {
            _logger.LogWarning("Emergency stop activated");
            
            var result = await _motorControlService.EmergencyStopAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during emergency stop");
            return StatusCode(500, new MotorResponse(false, "Internal server error"));
        }
    }
}