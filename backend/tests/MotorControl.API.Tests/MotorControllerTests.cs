using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MotorControl.API.Controllers;
using MotorControl.Application.Interfaces;
using MotorControl.Application.DTOs.Requests;
using MotorControl.Application.DTOs.Responses;

namespace MotorControl.API.Tests;

public class MotorControllerTests
{
    private readonly Mock<IMotorControlService> _mockMotorControlService;
    private readonly Mock<ILogger<MotorController>> _mockLogger;
    private readonly MotorController _controller;

    public MotorControllerTests()
    {
        _mockMotorControlService = new Mock<IMotorControlService>();
        _mockLogger = new Mock<ILogger<MotorController>>();
        _controller = new MotorController(_mockMotorControlService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SetSpeed_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = new SetSpeedRequest(50);
        var expectedResponse = new MotorResponse(true, "Speed set to 50", CreateMockMotorStatus());
        _mockMotorControlService.Setup(x => x.SetSpeedAsync(request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SetSpeed(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MotorResponse>(okResult.Value);
        Assert.True(response.Success);
        Assert.Contains("Speed set to 50", response.Message);
        _mockMotorControlService.Verify(x => x.SetSpeedAsync(request), Times.Once);
    }

    [Fact]
    public async Task SetSpeed_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new SetSpeedRequest(-10);
        var expectedResponse = new MotorResponse(false, "Invalid speed value");
        _mockMotorControlService.Setup(x => x.SetSpeedAsync(request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SetSpeed(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<MotorResponse>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("Invalid speed value", response.Message);
    }

    [Fact]
    public async Task ChangeMode_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = new ChangeModeRequest("Sport");
        var expectedResponse = new MotorResponse(true, "Driving mode changed to Sport", CreateMockMotorStatus());
        _mockMotorControlService.Setup(x => x.ChangeModeAsync(request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ChangeMode(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MotorResponse>(okResult.Value);
        Assert.True(response.Success);
        Assert.Contains("Driving mode changed to Sport", response.Message);
        _mockMotorControlService.Verify(x => x.ChangeModeAsync(request), Times.Once);
    }

    [Fact]
    public async Task ChangeMode_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ChangeModeRequest("InvalidMode");
        var expectedResponse = new MotorResponse(false, "Invalid driving mode");
        _mockMotorControlService.Setup(x => x.ChangeModeAsync(request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ChangeMode(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<MotorResponse>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("Invalid driving mode", response.Message);
    }

    [Fact]
    public async Task GetStatus_ShouldReturnOkWithMotorStatus()
    {
        // Arrange
        var expectedStatus = CreateMockMotorStatus();
        _mockMotorControlService.Setup(x => x.GetStatusAsync()).ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.GetStatus();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var status = Assert.IsType<MotorStatusResponse>(okResult.Value);
        Assert.Equal(expectedStatus.Id, status.Id);
        Assert.Equal(expectedStatus.CurrentSpeed, status.CurrentSpeed);
        Assert.Equal(expectedStatus.Mode, status.Mode);
        _mockMotorControlService.Verify(x => x.GetStatusAsync(), Times.Once);
    }

    [Fact]
    public async Task EmergencyStop_ShouldReturnOkResult()
    {
        // Arrange
        var expectedResponse = new MotorResponse(true, "Emergency stop activated", CreateMockMotorStatus());
        _mockMotorControlService.Setup(x => x.EmergencyStopAsync()).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.EmergencyStop();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MotorResponse>(okResult.Value);
        Assert.True(response.Success);
        Assert.Contains("Emergency stop activated", response.Message);
        _mockMotorControlService.Verify(x => x.EmergencyStopAsync(), Times.Once);
    }

    [Fact]
    public async Task SetSpeed_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new SetSpeedRequest(50);
        _mockMotorControlService.Setup(x => x.SetSpeedAsync(request))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.SetSpeed(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        var response = Assert.IsType<MotorResponse>(statusCodeResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Internal server error", response.Message);
    }

    [Fact]
    public async Task ChangeMode_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new ChangeModeRequest("Sport");
        _mockMotorControlService.Setup(x => x.ChangeModeAsync(request))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.ChangeMode(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        var response = Assert.IsType<MotorResponse>(statusCodeResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Internal server error", response.Message);
    }

    [Fact]
    public async Task GetStatus_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockMotorControlService.Setup(x => x.GetStatusAsync())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetStatus();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Internal server error", statusCodeResult.Value);
    }

    [Fact]
    public async Task EmergencyStop_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockMotorControlService.Setup(x => x.EmergencyStopAsync())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.EmergencyStop();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        var response = Assert.IsType<MotorResponse>(statusCodeResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Internal server error", response.Message);
    }

    private static MotorStatusResponse CreateMockMotorStatus()
    {
        return new MotorStatusResponse(
            Id: Guid.NewGuid(),
            CurrentSpeed: 50,
            TargetSpeed: 50,
            Mode: "Normal",
            Temperature: 45.0,
            RPM: 2500,
            PowerOutput: 60.0,
            Status: "Running",
            LastUpdated: DateTime.UtcNow
        );
    }
}