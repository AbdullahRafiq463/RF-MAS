# RF Device Monitoring & Automation System (RF-MAS)

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Flutter](https://img.shields.io/badge/Flutter-3.24+-02569B?logo=flutter)](https://flutter.dev/)
[![xUnit Tests](https://img.shields.io/badge/Tests-27%20Passed%20(100%25)-00E676)](https://xunit.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A software-based real-time monitoring and automation platform that simulates multiple RF (Radio Frequency) devices, collects continuous telemetry, detects abnormal operating conditions, executes device control commands, maintains event logs, and provides a cross-platform mobile dashboard for monitoring and control.

> [!IMPORTANT]
> **Educational Software Simulation Notice**:
> This project is strictly a software simulation developed for portfolio, educational, and interview demonstration purposes. It does **NOT** interface with, control, or claim to represent actual military, defense, or classified RF hardware. All instruments, manufacturers, models, and telemetry patterns are fictional.

---

## 1. Project Highlights & Key Features

- **Concurrent Real-Time Monitoring**: Hosted `BackgroundService` ingests live telemetry from multiple simulated instruments concurrently using `async`/`await` and `CancellationToken`.
- **Thread-Safe Architecture**: Utilizes `ConcurrentDictionary` and `SemaphoreSlim` to eliminate race conditions without coarse locking bottlenecks.
- **Simulated TCP/IP Networking Layer**: Implements a dedicated communication abstraction (`IDeviceCommunicationService`) and serialization parser (`ITelemetryParser`) modeling socket-based instrument exchange.
- **Dynamic Threshold & Alert Engine**: Automatically evaluates thermal limits, RF power margins, and supply voltage stability with smart deduplication.
- **Multi-Factor Device Health Scoring**: Algorithmic health calculation (0–100%) incorporating thermal margin, voltage regulation, signal adequacy, and active alarms.
- **Automated Hardware Verification Suite**: Automated test runner executing 6 compliance checks (Connectivity, Frequency Range, Signal Power, Temperature Safety, Voltage Range, Response Time Echo).
- **Material 3 Mobile Dashboard (Flutter)**: Engineering dashboard with time-series charts (`fl_chart`), bottom navigation, parameter tuning sliders, active alarm badges, and offline fault-tolerance.
- **Interactive Failure Scenario Injection**: Inject demo scenarios (`HIGH_TEMPERATURE`, `LOW_SIGNAL`, `LOW_VOLTAGE`, `DISCONNECT`, `COMMUNICATION_TIMEOUT`) to demonstrate real-time fault detection and test failures during presentations.
- **100% Test Coverage on Core Domain**: 27 unit tests across threshold rules, health calculators, protocol parsers, command validators, and automation routines.

---

## 2. System Architecture

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

## 3. Technology Stack

| Layer | Technologies |
|---|---|
| **Backend** | C# 13, .NET 9, ASP.NET Core Web API, Entity Framework Core 9, SQLite |
| **Concurrency & Async** | `BackgroundService`, `Task`, `async`/`await`, `CancellationToken`, `ConcurrentDictionary`, `SemaphoreSlim` |
| **API & Documentation** | RESTful JSON APIs, Swagger / OpenAPI 3.0, Custom Exception Middleware |
| **Unit Testing** | xUnit, Moq, FluentAssertions |
| **Mobile Frontend** | Flutter 3.24+, Dart 3.5+, Material 3 |
| **State Management** | `Provider` (`ChangeNotifier`) |
| **Mobile Visualization** | `fl_chart` (time-series graphing), `shared_preferences` (settings persistence), `intl` |

---

## 4. Simulated Hardware Profiles

The platform provisions 5 simulated RF instruments out of the box:

1. **RF-001**: *SG-2500* (Signal Generator Simulator, AeroTech Instruments, 2400–2500 MHz, 127.0.0.1:9001)
2. **RF-002**: *SA-4000* (Spectrum Analyzer Simulator, NovaWave Labs, 5150–5850 MHz, 127.0.0.1:9002)
3. **RF-003**: *PA-1200* (Power Amplifier Simulator, Apex Quantum, 800–960 MHz, 127.0.0.1:9003)
4. **RF-004**: *RA-6000* (RF Transceiver Simulator, Vectron Systems, 9200–9600 MHz, 127.0.0.1:9004)
5. **RF-005**: *TS-8000* (Telemetry Receiver Simulator, OmniWave Dynamics, 1420–1600 MHz, 127.0.0.1:9005)

---

## 5. Quick Start & Execution Guide

### Step 1: Start the ASP.NET Core Backend
```bash
# Navigate to API project
cd backend/RFMAS.Api

# Start Web API (binds to http://localhost:5000)
dotnet run --urls "http://0.0.0.0:5000"
```
- Open **Swagger UI**: [http://localhost:5000/swagger](http://localhost:5000/swagger)

### Step 2: Run Backend Unit Tests
```bash
# Run all 27 unit tests
dotnet test backend/RFMAS.Tests/RFMAS.Tests.csproj --verbosity normal
```

### Step 3: Launch the Flutter Mobile App
```bash
# Navigate to mobile app
cd mobile/rf_mas_app

# Run Flutter tests
flutter test

# Run application on emulator, connected phone, or Chrome
flutter run
```

> **Network Configuration Note**:
> - **Android Emulator**: Uses `http://10.0.2.2:5000` (preconfigured default).
> - **Physical Phone**: In the app's **Settings** screen, enter your computer's local Wi-Fi IP (e.g. `http://192.168.1.100:5000`).

---

## 6. Project Structure

```text
RF-MAS/
├── backend/
│   ├── RFMAS.Core/             # Domain entities, enums, interfaces, and records
│   ├── RFMAS.Infrastructure/   # EF Core SQLite DbContext, repositories, alert engine, command service
│   ├── RFMAS.Api/              # ASP.NET Core Web API, controllers, background monitoring service
│   └── RFMAS.Tests/            # xUnit tests for thresholds, health scores, parsers, and test suites
├── simulator/
│   └── RFMAS.DeviceSimulator/  # In-memory RF instrument simulator engine with scenario injectors
├── mobile/
│   └── rf_mas_app/             # Flutter Material 3 cross-platform mobile application
├── docs/
│   ├── architecture.md         # In-depth system architecture & data pipeline specs
│   ├── api.md                  # Complete REST API endpoint reference
│   ├── interview-notes.md      # Comprehensive interview Q&A preparation guide
│   └── setup.md                # Detailed local environment setup instructions
├── README.md
└── .gitignore
```

---

## 7. Documentation Index

- [System Architecture & Data Pipeline Specification](file:///d:/RF-DMAS/docs/architecture.md)
- [REST API Reference & Payload Specs](file:///d:/RF-DMAS/docs/api.md)
- [Technical Interview Q&A Preparation Notes](file:///d:/RF-DMAS/docs/interview-notes.md)
- [Setup & Troubleshooting Guide](file:///d:/RF-DMAS/docs/setup.md)
