using MotorControl.Domain.Enums;
using MotorControl.Domain.Exceptions;

namespace MotorControl.Domain.Entities;

public class Motor
{
    public Guid Id { get; private set; }
    public double CurrentSpeed { get; private set; }
    public double TargetSpeed { get; private set; }
    public DrivingMode Mode { get; private set; }
    public double Temperature { get; private set; }
    public double RPM { get; private set; }
    public double PowerOutput { get; private set; }
    public MotorStatus Status { get; private set; }
    public DateTime LastUpdated { get; private set; }
    public DateTime? EmergencyStopTime { get; private set; } 

    private const double MAX_SPEED = 100.0;
    private const double MAX_TEMPERATURE = 90.0;
    private const double ACCELERATION_RATE = 2.0;
    private const double BASE_TEMPERATURE = 25.0;

    public Motor()
    {
        Id = Guid.NewGuid();
        CurrentSpeed = 0;
        TargetSpeed = 0;
        Mode = DrivingMode.Normal;
        Temperature = BASE_TEMPERATURE;
        RPM = 0;
        PowerOutput = 0;
        Status = MotorStatus.Stopped;
        LastUpdated = DateTime.UtcNow;
    }

    public void SetSpeed(double speed)
    {
        if (speed < 0 || speed > MAX_SPEED)
            throw new InvalidSpeedException(speed);

        // Check emergency cooldown
        if (IsEmergencyCooldownActive())
        {
            throw new MotorException($"Motor in emergency cooldown. Wait {GetCooldownSecondsRemaining()} more seconds.");
        }

        // If cooldown finished, reset emergency status
        if (Status == MotorStatus.Emergency && EmergencyStopTime.HasValue)
        {
            Status = MotorStatus.Stopped;
            EmergencyStopTime = null;
        }

        TargetSpeed = speed;
        
        if (speed > 0 && Status == MotorStatus.Stopped)
        {
            Status = MotorStatus.Starting;
        }
        else if (speed == 0 && Status == MotorStatus.Running)
        {
            Status = MotorStatus.Stopping;
        }

        LastUpdated = DateTime.UtcNow;
    }

    public void SetMode(DrivingMode mode)
    {
        if (Status == MotorStatus.Emergency)
            return;

        Mode = mode;
        LastUpdated = DateTime.UtcNow;
    }

    public void EmergencyStop()
    {
        CurrentSpeed = 0;
        TargetSpeed = 0;
        Status = MotorStatus.Emergency;
        EmergencyStopTime = DateTime.UtcNow;
        LastUpdated = DateTime.UtcNow;
    }

    public void UpdateSimulation(double deltaTimeSeconds)
    {
        // Update speed gradually
        UpdateSpeed(deltaTimeSeconds);
        
        // Calculate RPM based on current speed
        UpdateRPM();
        
        // Calculate temperature based on speed and mode
        UpdateTemperature();
        
        // Calculate power output based on speed and mode
        UpdatePowerOutput();
        
        // Verify overheating conditions
        CheckOverheating();
        
        // Update motor status 
        UpdateStatus();
        
        LastUpdated = DateTime.UtcNow;
    }

    private void UpdateSpeed(double deltaTimeSeconds)
    {
        if (Status == MotorStatus.Emergency)
        {
            CurrentSpeed = 0;
            return;
        }

        double speedDifference = TargetSpeed - CurrentSpeed;
        
        if (Math.Abs(speedDifference) < 0.1)
        {
            CurrentSpeed = TargetSpeed;
            return;
        }

        double accelerationRate = ACCELERATION_RATE * GetModeMultiplier();
        double maxChange = accelerationRate * deltaTimeSeconds;
        
        if (speedDifference > 0)
        {
            CurrentSpeed += Math.Min(maxChange, speedDifference);
        }
        else
        {
            CurrentSpeed += Math.Max(-maxChange, speedDifference);
        }

        CurrentSpeed = Math.Max(0, Math.Min(MAX_SPEED, CurrentSpeed));
    }

    private void UpdateRPM()
    {
        // RPM = CurrentSpeed * 50; // Simple Simulation, 1 unit of speed = 50 RPM
        RPM = CurrentSpeed * 50;
    }

    private void UpdateTemperature()
    {
        double targetTemperature = BASE_TEMPERATURE + (CurrentSpeed * 0.6) + GetModeTemperatureOffset();
        
        double temperatureDifference = targetTemperature - Temperature;
        Temperature += temperatureDifference * 0.1; 
    }

    private void UpdatePowerOutput()
    {
        PowerOutput = CurrentSpeed * GetModeMultiplier() * 1.2;
    }

    private void CheckOverheating()
    {
        if (Temperature > MAX_TEMPERATURE && Status != MotorStatus.Emergency)
        {
            // Reduce speed immediately to 25% to cool down
            double emergencySpeed = CurrentSpeed * 0.25;
            TargetSpeed = emergencySpeed;
            CurrentSpeed = emergencySpeed;
            Status = MotorStatus.Overheating;
            
            throw new OverheatingException(Temperature);
        }
    }

    private void UpdateStatus()
    {
        if (Status == MotorStatus.Emergency)
            return;

        if (CurrentSpeed == 0 && TargetSpeed == 0)
        {
            Status = MotorStatus.Stopped;
        }
        else if (CurrentSpeed > 0 && Math.Abs(CurrentSpeed - TargetSpeed) < 0.1)
        {
            Status = MotorStatus.Running;
        }
        else if (TargetSpeed > CurrentSpeed)
        {
            Status = MotorStatus.Starting;
        }
        else if (TargetSpeed < CurrentSpeed)
        {
            Status = MotorStatus.Stopping;
        }
    }

    private double GetModeMultiplier()
    {
        return Mode switch
        {
            DrivingMode.Eco => 0.7,
            DrivingMode.Normal => 1.0,
            DrivingMode.Sport => 1.4,
            _ => 1.0
        };
    }

    private double GetModeTemperatureOffset()
    {
        return Mode switch
        {
            DrivingMode.Eco => 0,
            DrivingMode.Normal => 5,
            DrivingMode.Sport => 12,
            _ => 5
        };
    }

    public bool IsOverheating()
    {
        return Temperature > MAX_TEMPERATURE;
    }

    public bool CanAcceptCommands()
    {
        return Status != MotorStatus.Emergency;
    }

    public bool IsEmergencyCooldownActive()
    {
        if (Status == MotorStatus.Emergency && EmergencyStopTime.HasValue)
        {
            var timeSinceEmergency = DateTime.UtcNow - EmergencyStopTime.GetValueOrDefault();
            return timeSinceEmergency.TotalSeconds < 5;
        }
        return false;
    }

    public int GetCooldownSecondsRemaining()
    {
        if (IsEmergencyCooldownActive() && EmergencyStopTime.HasValue)
        {
            var timeSinceEmergency = DateTime.UtcNow - EmergencyStopTime.GetValueOrDefault();
            return Math.Max(0, 5 - (int)timeSinceEmergency.TotalSeconds);
        }
        return 0;
    }
}