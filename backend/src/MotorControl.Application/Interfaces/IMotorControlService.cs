using MotorControl.Application.DTOs.Requests;
using MotorControl.Application.DTOs.Responses;

namespace MotorControl.Application.Interfaces;

public interface IMotorControlService
{
    Task<MotorResponse> SetSpeedAsync(SetSpeedRequest request);
    Task<MotorResponse> ChangeModeAsync(ChangeModeRequest request);
    Task<MotorResponse> EmergencyStopAsync();
    Task<MotorStatusResponse> GetStatusAsync();
}