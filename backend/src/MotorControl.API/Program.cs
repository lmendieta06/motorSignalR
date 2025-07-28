using System.Reflection;
using DotNetEnv;
using MotorControl.Application.Interfaces;
using MotorControl.Application.Services;
using MotorControl.Domain.Interfaces;
using MotorControl.Infrastructure.Hubs;
using MotorControl.Infrastructure.Repositories;
using MotorControl.Infrastructure.Services;

// Environment variables
Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Motor Control API", Version = "v1" });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// SignalR
builder.Services.AddSignalR();

// Add CORS - Using environment variable with fallback
var corsOriginsEnv = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") ?? "http://localhost:3000";
var corsOrigins = corsOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});

// Register Domain services
builder.Services.AddSingleton<IMotorRepository, InMemoryMotorRepository>();

// Register Application services
builder.Services.AddScoped<IMotorControlService, MotorControlService>();

// Register Infrastructure services
builder.Services.AddScoped<ITelemetryService, SignalRTelemetryService>();
builder.Services.AddScoped<IMotorSimulator, MotorSimulator>();

// Register Background services
builder.Services.AddHostedService<MotorSimulationService>();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Motor Control API v1");
        c.RoutePrefix = "swagger";
    });
}

// Use CORS - Apply default policy
app.UseCors();

app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map SignalR hub with fallback
var signalRPath = Environment.GetEnvironmentVariable("SIGNALR_HUB_PATH") ?? "/motorhub";
app.MapHub<MotorTelemetryHub>(signalRPath);

app.Run();

public partial class Program { }
// This partial class is used for testing purposes
// It allows the Program class to be instantiated in tests without needing to run the entire application