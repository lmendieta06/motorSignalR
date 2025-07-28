using Microsoft.AspNetCore.SignalR;
using MotorControl.Application.DTOs.Responses;
using MotorControl.Application.Interfaces;
using MotorControl.Infrastructure.Hubs;

namespace MotorControl.Infrastructure.Services;

public class SignalRTelemetryService : ITelemetryService
{
    private readonly IHubContext<MotorTelemetryHub> _hubContext;

    public SignalRTelemetryService(IHubContext<MotorTelemetryHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastMotorDataAsync(MotorTelemetryData data)
    {
        await _hubContext.Clients.Group("MotorClients")
            .SendAsync("MotorData", data); 
    }

    public async Task SendOverheatingAlertAsync(string message)
    {
        await _hubContext.Clients.Group("MotorClients")
            .SendAsync("overheatingalert", message); 
    }
}