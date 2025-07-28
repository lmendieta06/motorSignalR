using Moq;
using Xunit;
using MotorControl.Application.Services;
using MotorControl.Application.Interfaces;
using MotorControl.Domain.Entities;
using MotorControl.Domain.Enums;
using MotorControl.Domain.Interfaces;
using MotorControl.Domain.Exceptions;
using MotorControl.Application.DTOs.Requests;

namespace MotorControl.Application.Tests;

public class MotorControlServiceTests
{
    private readonly Mock<IMotorRepository> _mockMotorRepository;
    private readonly Mock<ITelemetryService> _mockTelemetryService;
    private readonly MotorControlService _service;
    private readonly Motor _testMotor;

    public MotorControlServiceTests()
    {
        _mockMotorRepository = new Mock<IMotorRepository>();
        _mockTelemetryService = new Mock<ITelemetryService>();
        _service = new MotorControlService(_mockMotorRepository.Object, _mockTelemetryService.Object);
        _testMotor = new Motor();
    }

    [Fact]
    public async Task SetSpeedAsync_WithValidSpeed_ShouldReturnSuccessResponse()
    {
        // Arrange
        var request = new SetSpeedRequest(50);
        _mockMotorRepository.Setup(x => x.GetMotorAsync()).ReturnsAsync(_testMotor);
        _mockMotorRepository.Setup(x => x.UpdateMotorAsync(It.IsAny<Motor>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.SetSpeedAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Speed set to 50", result.Message);
        Assert.NotNull(result.Data);
        _mockMotorRepository.Verify(x => x.GetMotorAsync(), Times.Once);
        _mockMotorRepository.Verify(x => x.UpdateMotorAsync(_testMotor), Times.Once);
    }

    [Fact]
    public async Task SetSpeedAsync_WithInvalidSpeed_ShouldReturnFailureResponse()
    {
        // Arrange
        var request = new SetSpeedRequest(-10);
        _mockMotorRepository.Setup(x => x.GetMotorAsync()).ReturnsAsync(_testMotor);

        // Act
        var result = await _service.SetSpeedAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid speed value", result.Message);
        Assert.Null(result.Data);
        _mockMotorRepository.Verify(x => x.GetMotorAsync(), Times.Once);
        _mockMotorRepository.Verify(x => x.UpdateMotorAsync(It.IsAny<Motor>()), Times.Never);
    }

    [Theory]
    [InlineData("Eco")]
    [InlineData("Normal")]
    [InlineData("Sport")]
    public async Task ChangeModeAsync_WithValidMode_ShouldReturnSuccessResponse(string mode)
    {
        // Arrange
        var request = new ChangeModeRequest(mode);
        _mockMotorRepository.Setup(x => x.GetMotorAsync()).ReturnsAsync(_testMotor);
        _mockMotorRepository.Setup(x => x.UpdateMotorAsync(It.IsAny<Motor>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.ChangeModeAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Contains($"Driving mode changed to {mode}", result.Message);
        Assert.NotNull(result.Data);
        _mockMotorRepository.Verify(x => x.GetMotorAsync(), Times.Once);
        _mockMotorRepository.Verify(x => x.UpdateMotorAsync(_testMotor), Times.Once);
    }

    [Fact]
    public async Task ChangeModeAsync_WithInvalidMode_ShouldReturnFailureResponse()
    {
        // Arrange
        var request = new ChangeModeRequest("InvalidMode");
        _mockMotorRepository.Setup(x => x.GetMotorAsync()).ReturnsAsync(_testMotor);

        // Act
        var result = await _service.ChangeModeAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid driving mode", result.Message);
        Assert.Null(result.Data);
        _mockMotorRepository.Verify(x => x.GetMotorAsync(), Times.Once);
        _mockMotorRepository.Verify(x => x.UpdateMotorAsync(It.IsAny<Motor>()), Times.Never);
    }

    [Fact]
    public async Task EmergencyStopAsync_ShouldReturnSuccessAndSendAlert()
    {
        // Arrange
        _mockMotorRepository.Setup(x => x.GetMotorAsync()).ReturnsAsync(_testMotor);
        _mockMotorRepository.Setup(x => x.UpdateMotorAsync(It.IsAny<Motor>())).Returns(Task.CompletedTask);
        _mockTelemetryService.Setup(x => x.SendOverheatingAlertAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.EmergencyStopAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Emergency stop activated", result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(MotorStatus.Emergency.ToString(), result.Data.Status);
        _mockMotorRepository.Verify(x => x.GetMotorAsync(), Times.Once);
        _mockMotorRepository.Verify(x => x.UpdateMotorAsync(_testMotor), Times.Once);
        _mockTelemetryService.Verify(x => x.SendOverheatingAlertAsync("Emergency stop activated"), Times.Once);
    }

    [Fact]
    public async Task GetStatusAsync_ShouldReturnMotorStatus()
    {
        // Arrange
        _testMotor.SetSpeed(75);
        _testMotor.SetMode(DrivingMode.Sport);
        _mockMotorRepository.Setup(x => x.GetMotorAsync()).ReturnsAsync(_testMotor);

        // Act
        var result = await _service.GetStatusAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_testMotor.Id, result.Id);
        Assert.Equal(75, result.TargetSpeed);
        Assert.Equal("Sport", result.Mode);
        _mockMotorRepository.Verify(x => x.GetMotorAsync(), Times.Once);
    }

    [Fact]
    public async Task SetSpeedAsync_WhenRepositoryThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var request = new SetSpeedRequest(50);
        _mockMotorRepository.Setup(x => x.GetMotorAsync()).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.SetSpeedAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Error setting speed", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ChangeModeAsync_WhenRepositoryThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var request = new ChangeModeRequest("Normal");
        _mockMotorRepository.Setup(x => x.GetMotorAsync()).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.ChangeModeAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Error changing mode", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task EmergencyStopAsync_WhenTelemetryServiceFails_ShouldStillReturnSuccess()
    {
        // Arrange
        _mockMotorRepository.Setup(x => x.GetMotorAsync()).ReturnsAsync(_testMotor);
        _mockMotorRepository.Setup(x => x.UpdateMotorAsync(It.IsAny<Motor>())).Returns(Task.CompletedTask);
        _mockTelemetryService.Setup(x => x.SendOverheatingAlertAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("SignalR error"));

        // Act
        var result = await _service.EmergencyStopAsync();

        // Assert
        Assert.False(result.Success); // Should fail if telemetry fails
        Assert.Contains("Error during emergency stop", result.Message);
    }
}