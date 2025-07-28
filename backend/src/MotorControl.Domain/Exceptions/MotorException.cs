namespace MotorControl.Domain.Exceptions;

public class MotorException : Exception
{
    public MotorException(string message) : base(message)
    {
    }

    public MotorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class InvalidSpeedException : MotorException
{
    public InvalidSpeedException(double speed) 
        : base($"Invalid speed value: {speed}. Speed must be between 0 and 100.")
    {
    }
}

public class OverheatingException : MotorException
{
    public OverheatingException(double temperature) 
        : base($"Motor overheating detected: {temperature}°C. Maximum safe temperature is 90°C.")
    {
    }
}