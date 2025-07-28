namespace MotorControl.Application.DTOs.Responses;

public record MotorTelemetryData(
    double CurrentSpeed,
    double TargetSpeed,
    string Mode,
    double Temperature,
    double RPM,
    double PowerOutput,
    string Status,
    DateTime Timestamp,
    bool IsOverheating
);