using MotorControl.Domain.Entities;
using MotorControl.Domain.Interfaces;

namespace MotorControl.Infrastructure.Repositories;

public class InMemoryMotorRepository : IMotorRepository
{
    private static Motor _motor = new Motor();
    private static readonly object _lock = new object();

    public Task<Motor> GetMotorAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_motor);
        }
    }

    public Task UpdateMotorAsync(Motor motor)
    {
        lock (_lock)
        {
            _motor = motor;
            return Task.CompletedTask;
        }
    }
}