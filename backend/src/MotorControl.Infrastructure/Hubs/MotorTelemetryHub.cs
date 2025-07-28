using Microsoft.AspNetCore.SignalR;

namespace MotorControl.Infrastructure.Hubs;

public class MotorTelemetryHub : Hub
{
    public async Task JoinMotorGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "MotorClients");
        await Clients.Caller.SendAsync("StatusMessage", "Connected to motor telemetry");
    }

    public async Task LeaveMotorGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "MotorClients");
    }

    public override async Task OnConnectedAsync()
    {
        await JoinMotorGroup();
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await LeaveMotorGroup();
        await base.OnDisconnectedAsync(exception);
    }
}