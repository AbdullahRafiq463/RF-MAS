# System Architecture & Technical Design

## 1. Executive Summary & Overview
The **RF Device Monitoring & Automation System (RF-MAS)** is an educational full-stack software simulation designed to model real-time monitoring, telemetry acquisition, threshold evaluation, alarm generation, hardware command dispatching, and automated test execution across simulated RF (Radio Frequency) laboratory instruments.

```
+-------------------------------------------------------------------------+
|                        Flutter Mobile Dashboard                         |
|  (Material 3, Provider State Management, fl_chart Real-Time Graphs)    |
+-------------------------------------------------------------------------+
                                    │
                                    │  HTTP / REST (JSON API)
                                    ▼
+-------------------------------------------------------------------------+
|                      ASP.NET Core Web API Host                          |
|  Controllers  •  Global Exception Middleware  •  Swagger / OpenAPI      |
+-------------------------------------------------------------------------+
         │                                       │
         │ (Scoped DbContext)                    │ (In-Memory Stream)
         ▼                                       ▼
+-----------------------+              +----------------------------------+
|  EF Core & SQLite     |              |  Background Monitoring Service   |
|  (Local Persistence)  |              |  (Hosted Continuous Daemon)      |
|  - Devices            |              +----------------------------------+
|  - TelemetryRecords   |                                │
|  - Alerts             |                                ▼
|  - DeviceCommands     |              +----------------------------------+
|  - EventLogs          |              |  Alert Engine & Health Evaluator |
|  - TestResults        |              |  - Threshold Checking            |
+-----------------------+              |  - State Transitions             |
                                       +----------------------------------+
                                                         │
                                                         ▼
                                       +----------------------------------+
                                       |  TCP Device Communication Layer  |
                                       |  - ITelemetryParser Protocol     |
                                       +----------------------------------+
                                                         │
                                                         ▼
                                       +----------------------------------+
                                       |  RF Device Simulator Engine      |
                                       |  (Concurrent In-Memory Devices)  |
                                       +----------------------------------+
```

---

## 2. Simulated Hardware Profiles

The simulation platform provisions 5 independent RF instruments with realistic frequency bands, power outputs, and thermal envelopes:

| Device ID | Instrument Name | Type | Manufacturer | Model | Base Frequency | Safe Power Range |
|---|---|---|---|---|---|---|
| **RF-001** | Signal Generator Simulator | Signal Generator | AeroTech Instruments | SG-2500 | 2450.0 MHz (2.45 GHz) | -50 to +10 dBm |
| **RF-002** | Spectrum Analyzer Simulator | Spectrum Analyzer | NovaWave Labs | SA-4000 | 5200.0 MHz (5.2 GHz) | -70 to +0 dBm |
| **RF-003** | Power Amplifier Simulator | Power Amplifier | Apex Quantum | PA-1200 | 915.0 MHz (ISM Band) | -20 to +30 dBm |
| **RF-004** | Transceiver Module Simulator | RF Transceiver | Vectron Systems | RA-6000 | 9350.0 MHz (X-Band) | -60 to +15 dBm |
| **RF-005** | Telemetry Sensor Simulator | Telemetry Receiver | OmniWave Dynamics | TS-8000 | 1450.0 MHz (L-Band) | -75 to -10 dBm |

---

## 3. Concurrency & Multithreading Architecture

### Asynchronous Programming (`async`/`await`)
- Non-blocking I/O operations are used throughout the HTTP controllers, EF Core database operations, and inter-thread simulation exchanges.
- Prevents thread starvation on the ASP.NET Core thread pool under high-frequency telemetry polling.

### `BackgroundService` Hosted Daemon
- `DeviceMonitoringBackgroundService` runs as a singleton background worker managed by the .NET Generic Host lifecycle.
- Operates inside an asynchronous `while (!stoppingToken.IsCancellationRequested)` loop.
- Uses `IServiceScopeFactory` to safely create scoped lifetimes for the database context (`RFMASDbContext`), ensuring that long-lived singleton services do not cause DbContext concurrency leaks or stale tracking bugs.

### Thread-Safe In-Memory State
- `ConcurrentDictionary<string, SimulatedDevice>` stores the active simulated instrument states.
- `SemaphoreSlim` synchronizes simulated network socket read/write buffers, preventing race conditions during concurrent telemetry generation and command execution.

---

## 4. Telemetry Pipeline & Protocol Serialization

### Protocol Specification
Telemetry is serialized across the communication boundary using a compact, pipe-delimited ASCII protocol:
```text
deviceId|frequencyMHz|signalPowerDbm|temperatureC|voltageV|currentA|status|timestamp
```
*Example:*
```text
RF-001|2450.25|-30.12|41.80|12.04|1.82|ONLINE|2026-09-18T19:00:00.0000000Z
```

### Telemetry Processing Steps:
1. **Generation**: `SimulatedDevice` applies mathematical sinusoidal drift and Gaussian noise to baseline values.
2. **Serialization**: `ITelemetryParser.Format()` produces the wire packet.
3. **Transmission**: `TcpDeviceCommunicationService` simulates network latency and socket transmission.
4. **Deserialization & Validation**: `ITelemetryParser.Parse()` extracts values using `CultureInfo.InvariantCulture`.
5. **Threshold Inspection**: `IThresholdEvaluator` inspects values against warning/critical cutoffs.
6. **Alert Deduplication**: `IAlertEngine` suppresses repeated alarms for already active unacknowledged conditions.
7. **Health Index Calculation**: `IDeviceHealthCalculator` calculates a weighted 0–100% health score.
8. **Persistence & Audit**: EF Core persists time-series telemetry records and logs state changes.

---

## 5. Threshold & Safety Logic

| Metric | Normal Range | Warning Threshold | Critical / Fault Threshold |
|---|---|---|---|
| **Temperature** | < 60.0 °C | >= 60.0 °C | >= 75.0 °C |
| **RF Signal Power** | >= -50.0 dBm | < -50.0 dBm | <= -70.0 dBm |
| **Supply Voltage** | 11.0V to 13.0V | < 11.0V or > 13.0V | < 9.5V or > 14.5V |
| **Frequency Deviation** | Inside Passband | < Min or > Max Band | Disconnect / Unlock |
| **Current Draw** | Nominal Draw | > Configured Max A | Overcurrent Trip |

---

## 6. Automated Test Engine Strategy

The automated test framework executes 6 verification checks:
1. **Device Connectivity Test**: Verifies socket ping and active TCP connection state.
2. **Frequency Range Test**: Validates active RF output frequency is strictly within the hardware model's band.
3. **Signal Power Threshold Test**: Validates output level meets minimal signal sensitivity specs.
4. **Temperature Safety Test**: Validates thermal sensor reading remains below critical threshold.
5. **Voltage Range Test**: Validates DC rail voltage is within ±1.0V of nominal 12.0V supply.
6. **Device Response Test**: Measures roundtrip command echo latency (ms) and success status.

---

## 7. Production Hardware Transition Roadmap

While RF-MAS is strictly an educational software simulation, the architecture was intentionally designed to support physical laboratory hardware with zero frontend or domain changes:

```
[Flutter Mobile UI]  <--->  [ASP.NET Core API]  <--->  [IDeviceCommunicationService]
                                                              │
                                    ┌─────────────────────────┴─────────────────────────┐
                                    ▼                                                   ▼
                        [TcpDeviceCommunicationService]                     [ScpiHardwareAdapterService]
                        (Current Software Simulator)                         (Future Real Laboratory Hardware)
                                                                             - SCPI commands over TCP/IP
                                                                             - VISA / GPIB / USB / Serial
                                                                             - Keysight / Rohde & Schwarz SDKs
```
Because the communication layer is hidden behind `IDeviceCommunicationService`, swapping the simulated backend for real SCPI-compliant instruments requires only writing a concrete adapter implementing `IDeviceCommunicationService`.
