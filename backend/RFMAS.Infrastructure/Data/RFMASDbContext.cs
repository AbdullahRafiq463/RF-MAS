using Microsoft.EntityFrameworkCore;
using RFMAS.Core.Entities;
using RFMAS.Core.Enums;

namespace RFMAS.Infrastructure.Data;

/// <summary>
/// Entity Framework Core SQLite Database Context for RF-MAS.
/// </summary>
public class RFMASDbContext : DbContext
{
    public RFMASDbContext(DbContextOptions<RFMASDbContext> options) : base(options)
    {
    }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<TelemetryRecord> TelemetryRecords => Set<TelemetryRecord>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<DeviceCommand> DeviceCommands => Set<DeviceCommand>();
    public DbSet<EventLog> EventLogs => Set<EventLog>();
    public DbSet<AutomationTestResult> AutomationTestResults => Set<AutomationTestResult>();
    public DbSet<ThresholdConfig> ThresholdConfigs => Set<ThresholdConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Device configuration
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).HasMaxLength(32);
            entity.Property(d => d.Name).HasMaxLength(128).IsRequired();
            entity.Property(d => d.DeviceType).HasMaxLength(64).IsRequired();
            entity.Property(d => d.Manufacturer).HasMaxLength(128).IsRequired();
            entity.Property(d => d.Model).HasMaxLength(64).IsRequired();
            entity.Property(d => d.IpAddress).HasMaxLength(64).IsRequired();
            entity.Property(d => d.FirmwareVersion).HasMaxLength(32);

            entity.HasOne(d => d.Thresholds)
                  .WithOne(t => t.Device)
                  .HasForeignKey<ThresholdConfig>(t => t.DeviceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.TelemetryRecords)
                  .WithOne(t => t.Device)
                  .HasForeignKey(t => t.DeviceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.Alerts)
                  .WithOne(a => a.Device)
                  .HasForeignKey(a => a.DeviceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.Commands)
                  .WithOne(c => c.Device)
                  .HasForeignKey(c => c.DeviceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.EventLogs)
                  .WithOne(l => l.Device)
                  .HasForeignKey(l => l.DeviceId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Telemetry configuration
        modelBuilder.Entity<TelemetryRecord>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => new { t.DeviceId, t.Timestamp });
        });

        // Alert configuration
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => new { a.DeviceId, a.IsAcknowledged });
            entity.Property(a => a.AlertType).HasMaxLength(64).IsRequired();
            entity.Property(a => a.Message).HasMaxLength(512).IsRequired();
        });

        // DeviceCommand configuration
        modelBuilder.Entity<DeviceCommand>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.ResultMessage).HasMaxLength(512);
        });

        // EventLog configuration
        modelBuilder.Entity<EventLog>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasIndex(l => l.Timestamp);
            entity.Property(l => l.Category).HasMaxLength(64).IsRequired();
            entity.Property(l => l.EventName).HasMaxLength(128).IsRequired();
            entity.Property(l => l.Details).HasMaxLength(1024).IsRequired();
        });

        // AutomationTestResult configuration
        modelBuilder.Entity<AutomationTestResult>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.ExecutedAt);
            entity.Property(r => r.TestName).HasMaxLength(128).IsRequired();
            entity.Property(r => r.DeviceId).HasMaxLength(32).IsRequired();
        });

        // ThresholdConfig configuration
        modelBuilder.Entity<ThresholdConfig>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.DeviceId).IsUnique();
        });

        // Seed 5 Realistic Simulated RF Devices
        SeedDefaultData(modelBuilder);
    }

    private static void SeedDefaultData(ModelBuilder modelBuilder)
    {
        var devices = new[]
        {
            new Device
            {
                Id = "RF-001",
                Name = "Signal Generator Simulator",
                DeviceType = "Signal Generator",
                Manufacturer = "AeroTech Instruments",
                Model = "SG-2500",
                IpAddress = "127.0.0.1",
                Port = 9001,
                ConnectionStatus = true,
                OperationalStatus = DeviceStatus.ONLINE,
                FrequencyMHz = 2450.0,
                SignalPowerDbm = -30.0,
                TemperatureC = 42.0,
                VoltageV = 12.0,
                CurrentA = 1.8,
                HealthPercentage = 100.0,
                FirmwareVersion = "v2.1.0-sim",
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LastCommunicationTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Device
            {
                Id = "RF-002",
                Name = "Spectrum Analyzer Simulator",
                DeviceType = "Spectrum Analyzer",
                Manufacturer = "NovaWave Labs",
                Model = "SA-4000",
                IpAddress = "127.0.0.1",
                Port = 9002,
                ConnectionStatus = true,
                OperationalStatus = DeviceStatus.ONLINE,
                FrequencyMHz = 5200.0,
                SignalPowerDbm = -25.0,
                TemperatureC = 45.5,
                VoltageV = 12.1,
                CurrentA = 2.1,
                HealthPercentage = 100.0,
                FirmwareVersion = "v3.0.4-sim",
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LastCommunicationTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Device
            {
                Id = "RF-003",
                Name = "Power Amplifier Simulator",
                DeviceType = "Power Amplifier",
                Manufacturer = "Apex Quantum",
                Model = "PA-1200",
                IpAddress = "127.0.0.1",
                Port = 9003,
                ConnectionStatus = true,
                OperationalStatus = DeviceStatus.ONLINE,
                FrequencyMHz = 915.0,
                SignalPowerDbm = -15.0,
                TemperatureC = 48.2,
                VoltageV = 11.9,
                CurrentA = 2.8,
                HealthPercentage = 98.0,
                FirmwareVersion = "v1.8.2-sim",
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LastCommunicationTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Device
            {
                Id = "RF-004",
                Name = "Transceiver Module Simulator",
                DeviceType = "RF Transceiver",
                Manufacturer = "Vectron Systems",
                Model = "RA-6000",
                IpAddress = "127.0.0.1",
                Port = 9004,
                ConnectionStatus = true,
                OperationalStatus = DeviceStatus.ONLINE,
                FrequencyMHz = 9350.0,
                SignalPowerDbm = -35.0,
                TemperatureC = 39.0,
                VoltageV = 12.0,
                CurrentA = 1.6,
                HealthPercentage = 100.0,
                FirmwareVersion = "v4.2.1-sim",
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LastCommunicationTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Device
            {
                Id = "RF-005",
                Name = "Telemetry Sensor Simulator",
                DeviceType = "Telemetry Receiver",
                Manufacturer = "OmniWave Dynamics",
                Model = "TS-8000",
                IpAddress = "127.0.0.1",
                Port = 9005,
                ConnectionStatus = true,
                OperationalStatus = DeviceStatus.ONLINE,
                FrequencyMHz = 1450.0,
                SignalPowerDbm = -40.0,
                TemperatureC = 36.8,
                VoltageV = 12.2,
                CurrentA = 1.2,
                HealthPercentage = 100.0,
                FirmwareVersion = "v2.0.5-sim",
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LastCommunicationTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        var thresholds = new[]
        {
            new ThresholdConfig
            {
                Id = 1,
                DeviceId = "RF-001",
                TemperatureWarning = 60.0,
                TemperatureCritical = 75.0,
                SignalPowerWarning = -50.0,
                SignalPowerCritical = -70.0,
                VoltageMin = 11.0,
                VoltageMax = 13.0,
                FrequencyMin = 2400.0,
                FrequencyMax = 2500.0,
                CurrentMaxA = 3.0,
                TimeoutSeconds = 10
            },
            new ThresholdConfig
            {
                Id = 2,
                DeviceId = "RF-002",
                TemperatureWarning = 60.0,
                TemperatureCritical = 75.0,
                SignalPowerWarning = -50.0,
                SignalPowerCritical = -70.0,
                VoltageMin = 11.0,
                VoltageMax = 13.0,
                FrequencyMin = 5150.0,
                FrequencyMax = 5850.0,
                CurrentMaxA = 3.5,
                TimeoutSeconds = 10
            },
            new ThresholdConfig
            {
                Id = 3,
                DeviceId = "RF-003",
                TemperatureWarning = 62.0,
                TemperatureCritical = 78.0,
                SignalPowerWarning = -50.0,
                SignalPowerCritical = -70.0,
                VoltageMin = 11.0,
                VoltageMax = 13.0,
                FrequencyMin = 800.0,
                FrequencyMax = 960.0,
                CurrentMaxA = 4.0,
                TimeoutSeconds = 10
            },
            new ThresholdConfig
            {
                Id = 4,
                DeviceId = "RF-004",
                TemperatureWarning = 60.0,
                TemperatureCritical = 75.0,
                SignalPowerWarning = -50.0,
                SignalPowerCritical = -70.0,
                VoltageMin = 11.0,
                VoltageMax = 13.0,
                FrequencyMin = 9200.0,
                FrequencyMax = 9600.0,
                CurrentMaxA = 3.0,
                TimeoutSeconds = 10
            },
            new ThresholdConfig
            {
                Id = 5,
                DeviceId = "RF-005",
                TemperatureWarning = 58.0,
                TemperatureCritical = 72.0,
                SignalPowerWarning = -55.0,
                SignalPowerCritical = -75.0,
                VoltageMin = 11.0,
                VoltageMax = 13.0,
                FrequencyMin = 1420.0,
                FrequencyMax = 1600.0,
                CurrentMaxA = 2.5,
                TimeoutSeconds = 10
            }
        };

        modelBuilder.Entity<Device>().HasData(devices);
        modelBuilder.Entity<ThresholdConfig>().HasData(thresholds);
    }
}
