using MotorControl.Domain.Entities;

namespace MotorControl.Domain.Interfaces;

public interface IMotorRepository
{
    Task<Motor> GetMotorAsync();
    Task UpdateMotorAsync(Motor motor);
}