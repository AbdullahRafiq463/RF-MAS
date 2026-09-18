# Setup & Quick Start Guide

This guide explains how to build, run, and test the **RF Device Monitoring & Automation System (RF-MAS)** locally.

---

## 1. Prerequisites

- **.NET SDK**: .NET 9.0 or newer ([Download .NET](https://dotnet.microsoft.com/download))
- **Flutter SDK**: Flutter 3.24+ / Dart 3.5+ ([Download Flutter](https://flutter.dev/docs/get-started/install))
- **Operating System**: Windows, macOS, or Linux

---

## 2. Running the C# Backend & Simulator

The backend Web API and embedded Simulator Engine run as a single process:

```bash
# Navigate to the backend directory
cd backend/RFMAS.Api

# Run the ASP.NET Core Web API
dotnet run --urls "http://0.0.0.0:5000"
```

Once running:
- **API Base URL**: `http://localhost:5000`
- **Interactive Swagger UI**: [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **Local SQLite Database**: `backend/RFMAS.Api/rfmas.db` (auto-created and seeded on first run)

---

## 3. Running Backend Unit Tests

To run the full xUnit test suite (27 unit & service tests):

```bash
# Navigate to the root directory
dotnet test backend/RFMAS.Tests/RFMAS.Tests.csproj --verbosity normal
```

---

## 4. Running the Flutter Mobile App

### Android Emulator Setup
When running on the Android Emulator, Android routes `10.0.2.2` to the host machine's `localhost`:
- The Flutter app is preconfigured by default to connect to `http://10.0.2.2:5000`.

### Physical Android / iOS Device Setup
1. Find your computer's local Wi-Fi IP address:
   - On Windows: Run `ipconfig` (e.g. `192.168.1.100`)
2. Start the ASP.NET Core backend bound to all interfaces:
   ```bash
   dotnet run --urls "http://0.0.0.0:5000"
   ```
3. Open the Flutter app -> Navigate to **Settings** screen -> Enter `http://192.168.1.100:5000` -> Tap **Save & Apply Backend URL**.

### Launching Flutter:
```bash
# Navigate to the mobile app directory
cd mobile/rf_mas_app

# Run Flutter tests
flutter test

# Run the application (select your device or emulator)
flutter run
```

Or run directly in Chrome for instant testing:
```bash
flutter run -d chrome
```

---

## 5. Demonstrating Failure Scenarios & Alarms (Demo Mode)

During presentations or interviews, you can inject simulated hardware anomalies:
1. Open the Flutter app -> Tap the **Instruments** tab.
2. Tap **Simulate Scenario** on any instrument (e.g. `RF-001`).
3. Select an anomaly:
   - **High Thermal Runaway (>75°C)**: Triggers instant `TEMPERATURE_CRITICAL` alarm and drops device health.
   - **RF Signal Fade (<-70 dBm)**: Triggers `SIGNAL_POWER_CRITICAL` alarm.
   - **Rail Voltage Sag (<10V)**: Triggers `VOLTAGE_OUT_OF_RANGE` alarm.
   - **Device Network Disconnection**: Simulates link break, transitions status to `OFFLINE`.
   - **Normal Nominal Operation**: Restores nominal telemetry stream.
4. Navigate to the **Alerts** tab to observe and acknowledge the alarms.
5. Navigate to the **Automation** tab and tap **Run All Automated Tests** to observe test failures corresponding to the injected faults.
