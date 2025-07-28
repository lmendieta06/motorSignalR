namespace MotorControl.Application.DTOs.Responses;

public record MotorResponse(
    bool Success,
    string Message,
    MotorStatusResponse? Data = null
);