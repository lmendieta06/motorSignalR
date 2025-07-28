namespace MotorControl.Domain.Enums;

public enum MotorStatus
{
    Stopped = 0,
    Starting = 1,
    Running = 2,
    Stopping = 3,
    Emergency = 4,
    Overheating = 5
}