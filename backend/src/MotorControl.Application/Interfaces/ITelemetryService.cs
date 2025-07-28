using MotorControl.Application.DTOs.Responses;

namespace MotorControl.Application.Interfaces;

public interface ITelemetryService
{
    Task BroadcastMotorDataAsync(MotorTelemetryData data);
    Task SendOverheatingAlertAsync(string message);
}