using MotorControl.Domain.Entities;

namespace MotorControl.Application.Interfaces;

public interface IMotorSimulator
{
    Task<Motor> GetCurrentMotorStateAsync();
    Task UpdateMotorSimulationAsync();
    Task StartSimulationAsync();
    Task StopSimulationAsync();
}