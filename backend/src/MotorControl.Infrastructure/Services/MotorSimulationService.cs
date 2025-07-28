using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MotorControl.Application.DTOs.Responses;
using MotorControl.Application.Interfaces;
using MotorControl.Domain.Exceptions;

namespace MotorControl.Infrastructure.Services;

public class MotorSimulationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MotorSimulationService> _logger;

    public MotorSimulationService(IServiceProvider serviceProvider, ILogger<MotorSimulationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Motor Simulation Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var motorSimulator = scope.ServiceProvider.GetRequiredService<IMotorSimulator>();
                var telemetryService = scope.ServiceProvider.GetRequiredService<ITelemetryService>();

                await motorSimulator.UpdateMotorSimulationAsync();

                var motor = await motorSimulator.GetCurrentMotorStateAsync();

                var telemetryData = new MotorTelemetryData(
                    CurrentSpeed: motor.CurrentSpeed,
                    TargetSpeed: motor.TargetSpeed,
                    Mode: motor.Mode.ToString(),
                    Temperature: motor.Temperature,
                    RPM: motor.RPM,
                    PowerOutput: motor.PowerOutput,
                    Status: motor.Status.ToString(),
                    Timestamp: DateTime.UtcNow,
                    IsOverheating: motor.IsOverheating()
                );

                // Broadcast telemetry data every 2 seconds
                if (DateTime.UtcNow.Millisecond % 2000 < 100)
                {
                    await telemetryService.BroadcastMotorDataAsync(telemetryData);
                }

                // Verify if motor is overheating
                if (motor.IsOverheating())
                {
                    await telemetryService.SendOverheatingAlertAsync(
                        $"CRITICAL: Motor overheating detected! Temperature: {motor.Temperature:F1}°C");
                }
            }
            catch (OverheatingException ex)
            {
                _logger.LogWarning("Motor overheating: {Message}", ex.Message);
                
                using var scope = _serviceProvider.CreateScope();
                var telemetryService = scope.ServiceProvider.GetRequiredService<ITelemetryService>();
                await telemetryService.SendOverheatingAlertAsync(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in motor simulation");
            }

            await Task.Delay(100, stoppingToken);
        }

        _logger.LogInformation("Motor Simulation Service stopped");
    }
}