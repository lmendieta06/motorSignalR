using MotorControl.Application.Interfaces;
using MotorControl.Domain.Entities;
using MotorControl.Domain.Interfaces;

namespace MotorControl.Infrastructure.Services;

public class MotorSimulator : IMotorSimulator
{
    private readonly IMotorRepository _motorRepository;

    public MotorSimulator(IMotorRepository motorRepository)
    {
        _motorRepository = motorRepository;
    }

    public async Task<Motor> GetCurrentMotorStateAsync()
    {
        return await _motorRepository.GetMotorAsync();
    }

    public async Task UpdateMotorSimulationAsync()
    {
        var motor = await _motorRepository.GetMotorAsync();
        
        motor.UpdateSimulation(0.1);
        
        await _motorRepository.UpdateMotorAsync(motor);
    }

    public async Task StartSimulationAsync()
    {
        await Task.CompletedTask;
    }

    public async Task StopSimulationAsync()
    {
        await Task.CompletedTask;
    }
}