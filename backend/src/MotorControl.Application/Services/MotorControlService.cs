using MotorControl.Application.DTOs.Requests;
using MotorControl.Application.DTOs.Responses;
using MotorControl.Application.Interfaces;
using MotorControl.Domain.Entities;
using MotorControl.Domain.Enums;
using MotorControl.Domain.Exceptions;
using MotorControl.Domain.Interfaces;

namespace MotorControl.Application.Services;

public class MotorControlService : IMotorControlService
{
    private readonly IMotorRepository _motorRepository;
    private readonly ITelemetryService _telemetryService;

    public MotorControlService(IMotorRepository motorRepository, ITelemetryService telemetryService)
    {
        _motorRepository = motorRepository;
        _telemetryService = telemetryService;
    }

    public async Task<MotorResponse> SetSpeedAsync(SetSpeedRequest request)
    {
        try
        {
            var motor = await _motorRepository.GetMotorAsync();
            
            motor.SetSpeed(request.Speed);
            await _motorRepository.UpdateMotorAsync(motor);

            var statusResponse = MapToStatusResponse(motor);
            
            return new MotorResponse(
                Success: true,
                Message: $"Speed set to {request.Speed}",
                Data: statusResponse
            );
        }
        catch (InvalidSpeedException ex)
        {
            return new MotorResponse(
                Success: false,
                Message: ex.Message
            );
        }
        catch (Exception ex)
        {
            return new MotorResponse(
                Success: false,
                Message: $"Error setting speed: {ex.Message}"
            );
        }
    }

    public async Task<MotorResponse> ChangeModeAsync(ChangeModeRequest request)
    {
        try
        {
            var motor = await _motorRepository.GetMotorAsync();
            
            if (!Enum.TryParse<DrivingMode>(request.Mode, true, out var mode))
            {
                return new MotorResponse(
                    Success: false,
                    Message: $"Invalid driving mode: {request.Mode}. Valid modes are: Eco, Normal, Sport"
                );
            }

            motor.SetMode(mode);
            await _motorRepository.UpdateMotorAsync(motor);

            var statusResponse = MapToStatusResponse(motor);
            
            return new MotorResponse(
                Success: true,
                Message: $"Driving mode changed to {request.Mode}",
                Data: statusResponse
            );
        }
        catch (Exception ex)
        {
            return new MotorResponse(
                Success: false,
                Message: $"Error changing mode: {ex.Message}"
            );
        }
    }

    public async Task<MotorResponse> EmergencyStopAsync()
    {
        try
        {
            var motor = await _motorRepository.GetMotorAsync();
            
            motor.EmergencyStop();
            await _motorRepository.UpdateMotorAsync(motor);

            // Notificar vía SignalR
            await _telemetryService.SendOverheatingAlertAsync("Emergency stop activated");

            var statusResponse = MapToStatusResponse(motor);
            
            return new MotorResponse(
                Success: true,
                Message: "Emergency stop activated",
                Data: statusResponse
            );
        }
        catch (Exception ex)
        {
            return new MotorResponse(
                Success: false,
                Message: $"Error during emergency stop: {ex.Message}"
            );
        }
    }

    public async Task<MotorStatusResponse> GetStatusAsync()
    {
        var motor = await _motorRepository.GetMotorAsync();
        return MapToStatusResponse(motor);
    }

    private static MotorStatusResponse MapToStatusResponse(Motor motor)
    {
        return new MotorStatusResponse(
            Id: motor.Id,
            CurrentSpeed: motor.CurrentSpeed,
            TargetSpeed: motor.TargetSpeed,
            Mode: motor.Mode.ToString(),
            Temperature: motor.Temperature,
            RPM: motor.RPM,
            PowerOutput: motor.PowerOutput,
            Status: motor.Status.ToString(),
            LastUpdated: motor.LastUpdated
        );
    }
}