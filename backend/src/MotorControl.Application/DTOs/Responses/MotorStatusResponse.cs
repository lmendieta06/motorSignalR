namespace MotorControl.Application.DTOs.Responses;

public record MotorStatusResponse(
    Guid Id,
    double CurrentSpeed,
    double TargetSpeed,
    string Mode,
    double Temperature,
    double RPM,
    double PowerOutput,
    string Status,
    DateTime LastUpdated
);