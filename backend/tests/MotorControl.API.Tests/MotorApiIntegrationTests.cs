using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using MotorControl.Application.DTOs.Requests;
using MotorControl.Application.DTOs.Responses;

namespace MotorControl.API.Tests;

public class MotorApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MotorApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMotorStatus_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/motor/status");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
        
        var status = await response.Content.ReadFromJsonAsync<MotorStatusResponse>();
        Assert.NotNull(status);
        Assert.NotEqual(Guid.Empty, status.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public async Task SetMotorSpeed_WithValidSpeed_ShouldReturnOk(double speed)
    {
        // Arrange
        var request = new SetSpeedRequest(speed);

        // Act
        var response = await _client.PostAsJsonAsync("/api/motor/speed", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<MotorResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains($"Speed set to {speed}", result.Message);
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(150)]
    public async Task SetMotorSpeed_WithInvalidSpeed_ShouldReturnBadRequest(double invalidSpeed)
    {
        // Arrange
        var request = new SetSpeedRequest(invalidSpeed);

        // Act
        var response = await _client.PostAsJsonAsync("/api/motor/speed", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<MotorResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    [Theory]
    [InlineData("Eco")]
    [InlineData("Normal")]
    [InlineData("Sport")]
    public async Task ChangeMotorMode_WithValidMode_ShouldReturnOk(string mode)
    {
        // Arrange
        var request = new ChangeModeRequest(mode);

        // Act
        var response = await _client.PostAsJsonAsync("/api/motor/mode", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<MotorResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains($"Driving mode changed to {mode}", result.Message);
    }

    [Fact]
    public async Task ChangeMotorMode_WithInvalidMode_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ChangeModeRequest("InvalidMode");

        // Act
        var response = await _client.PostAsJsonAsync("/api/motor/mode", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<MotorResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("Invalid driving mode", result.Message);
    }

    [Fact]
    public async Task EmergencyStop_ShouldReturnOk()
    {
        // Act
        var response = await _client.PostAsync("/api/motor/stop", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<MotorResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("Emergency stop activated", result.Message);
    }

    [Fact]
    public async Task EmergencyStop_ThenSetSpeed_ShouldReturnBadRequestDuringCooldown()
    {
        // Arrange - First trigger emergency stop
        await _client.PostAsync("/api/motor/stop", null);

        // Act - Try to set speed immediately
        var speedRequest = new SetSpeedRequest(50);
        var response = await _client.PostAsJsonAsync("/api/motor/speed", speedRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<MotorResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("emergency cooldown", result.Message.ToLower());
    }

    [Fact]
    public async Task CompleteWorkflow_ShouldWork()
    {
        // 1. Get initial status
        var statusResponse = await _client.GetAsync("/api/motor/status");
        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);

        // 2. Set speed
        var speedRequest = new SetSpeedRequest(60);
        var speedResponse = await _client.PostAsJsonAsync("/api/motor/speed", speedRequest);
        Assert.Equal(HttpStatusCode.OK, speedResponse.StatusCode);

        // 3. Change mode
        var modeRequest = new ChangeModeRequest("Sport");
        var modeResponse = await _client.PostAsJsonAsync("/api/motor/mode", modeRequest);
        Assert.Equal(HttpStatusCode.OK, modeResponse.StatusCode);

        // 4. Get status again to verify changes
        var finalStatusResponse = await _client.GetAsync("/api/motor/status");
        Assert.Equal(HttpStatusCode.OK, finalStatusResponse.StatusCode);
        
        var finalStatus = await finalStatusResponse.Content.ReadFromJsonAsync<MotorStatusResponse>();
        Assert.NotNull(finalStatus);
        Assert.Equal(60, finalStatus.TargetSpeed);
        Assert.Equal("Sport", finalStatus.Mode);

        // 5. Emergency stop
        var stopResponse = await _client.PostAsync("/api/motor/stop", null);
        Assert.Equal(HttpStatusCode.OK, stopResponse.StatusCode);
    }
}