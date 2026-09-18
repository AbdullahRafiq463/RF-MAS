using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RFMAS.Api.BackgroundServices;
using RFMAS.Api.Middleware;
using RFMAS.Core.Interfaces;
using RFMAS.DeviceSimulator;
using RFMAS.Infrastructure.Communication;
using RFMAS.Infrastructure.Data;
using RFMAS.Infrastructure.Data.Repositories;
using RFMAS.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// 1. DATABASE CONFIGURATION (Entity Framework Core with SQLite)
// ============================================================================
// SQLite provides lightweight, zero-configuration local persistence.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=rfmas.db";
builder.Services.AddDbContext<RFMASDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

// ============================================================================
// 2. DEPENDENCY INJECTION & SERVICE LIFETIMES
// ============================================================================
// SINGLETON: Only one instance exists for the application lifetime.
// Used for the in-memory device simulator engine and communication service to maintain state across requests.
builder.Services.AddSingleton<DeviceSimulatorEngine>();
builder.Services.AddSingleton<ITelemetryParser, TelemetryMessageParser>();
builder.Services.AddSingleton<IDeviceCommunicationService, TcpDeviceCommunicationService>();

// SCOPED: A new instance is created per HTTP request or background work scope.
// Required for database repositories and unit-of-work to safely bind with EF Core DbContext.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAlertEngine, AlertEngine>();
builder.Services.AddScoped<ICommandService, CommandService>();
builder.Services.AddScoped<IAutomationEngine, AutomationEngine>();
builder.Services.AddScoped<ILogService, LogService>();

// TRANSIENT: A fresh lightweight instance created whenever requested.
builder.Services.AddTransient<IThresholdEvaluator, ThresholdEvaluator>();
builder.Services.AddTransient<IDeviceHealthCalculator, DeviceHealthCalculator>();

// ============================================================================
// 3. BACKGROUND MONITORING WORKER
// ============================================================================
// Runs continuously in the background to ingest telemetry and detect threshold alerts.
builder.Services.AddHostedService<DeviceMonitoringBackgroundService>();

// ============================================================================
// 4. CONTROLLERS & JSON SERIALIZATION
// ============================================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as human-readable strings (e.g., "ONLINE", "WARNING") instead of raw numbers
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ============================================================================
// 5. CORS (Cross-Origin Resource Sharing)
// ============================================================================
// Essential for allowing the Flutter mobile app (Android Emulator 10.0.2.2, Web, Physical devices) to access the API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============================================================================
// 6. SWAGGER / OPENAPI DOCUMENTATION
// ============================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RF Device Monitoring & Automation System (RF-MAS) API",
        Version = "v1",
        Description = "REST API for RF-MAS: Simulates RF devices, streams real-time telemetry, handles commands, evaluates safety thresholds, and runs automated hardware verification test suites.\n\n*Note: This is an educational software simulation and does not control real military or classified RF hardware.*"
    });
});

var app = builder.Build();

// ============================================================================
// 7. DATABASE INITIALIZATION & SEEDING
// ============================================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RFMASDbContext>();
    db.Database.EnsureCreated();
}

// ============================================================================
// 8. HTTP REQUEST PIPELINE (MIDDLEWARE)
// ============================================================================
// Global custom exception handler (returns friendly JSON errors without stack traces)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable Swagger UI in all environments for ease of testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RF-MAS API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

app.UseRouting();

app.MapControllers();

app.Run();
