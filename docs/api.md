# RF-MAS REST API Reference

The ASP.NET Core Web API exposes clean, standardized RESTful endpoints with OpenAPI / Swagger documentation accessible at `/swagger`.

Base URL: `http://localhost:5000` (or `http://10.0.2.2:5000` in Android Emulator)

---

## 1. Dashboard

### `GET /api/dashboard/summary`
Returns system-wide aggregated telemetry, operational counters, and fleet health indices.

**Response `200 OK`**:
```json
{
  "totalDevices": 5,
  "onlineDevices": 5,
  "warningDevices": 0,
  "errorDevices": 0,
  "offlineDevices": 0,
  "maintenanceDevices": 0,
  "totalAlerts": 0,
  "activeAlerts": 0,
  "criticalAlerts": 0,
  "warningAlerts": 0,
  "testsExecuted": 6,
  "testsPassed": 6,
  "testsFailed": 0,
  "testsErrored": 0,
  "averageSystemHealth": 99.4,
  "serverTimeUtc": "2026-09-18T20:15:00.000Z"
}
```

---

## 2. Devices & Telemetry

### `GET /api/devices`
Returns summary list of all simulated instruments.

**Response `200 OK`**:
```json
[
  {
    "id": "RF-001",
    "name": "Signal Generator Simulator",
    "deviceType": "Signal Generator",
    "manufacturer": "AeroTech Instruments",
    "model": "SG-2500",
    "ipAddress": "127.0.0.1",
    "port": 9001,
    "connectionStatus": true,
    "operationalStatus": "ONLINE",
    "frequencyMHz": 2450.25,
    "signalPowerDbm": -30.12,
    "temperatureC": 41.80,
    "voltageV": 12.04,
    "currentA": 1.82,
    "healthPercentage": 99.0,
    "lastCommunicationTime": "2026-09-18T20:14:58.000Z"
  }
]
```

### `GET /api/devices/{id}`
Returns complete hardware profile, threshold limits, and recent telemetry history.

### `GET /api/devices/{id}/telemetry?count=50`
Returns recent historical telemetry time-series points for charting.

---

## 3. Device Controls & Commands

### `POST /api/devices/{id}/start`
Starts device and sets operational status to `ONLINE`.

### `POST /api/devices/{id}/stop`
Stops device and transitions status to `OFFLINE`.

### `POST /api/devices/{id}/reset`
Resets operating parameters to factory baseline.

### `POST /api/devices/{id}/frequency`
Tunes operating frequency within validated hardware limits.

**Request Body**:
```json
{
  "frequencyMHz": 2455.50
}
```

### `POST /api/devices/{id}/power`
Sets RF output power within validated safety bounds (-100 to +30 dBm).

**Request Body**:
```json
{
  "powerDbm": -25.0
}
```

---

## 4. Safety Alarms & Alerts

### `GET /api/alerts?activeOnly=true&severity=CRITICAL`
Returns alarm log filtered by status and severity.

### `POST /api/alerts/{id}/acknowledge`
Acknowledges an active alarm.

**Request Body**:
```json
{
  "acknowledgedBy": "Lead RF Test Engineer"
}
```

---

## 5. Automated Testing Suite

### `GET /api/automation/tests?count=50`
Returns history of automated verification runs.

### `POST /api/automation/tests/run`
Executes all 6 tests across all 5 instruments in parallel/sequential batches.

### `POST /api/automation/devices/{deviceId}/run`
Executes verification test suite on a single device.

---

## 6. Audit & Event Logs

### `GET /api/logs?category=COMMAND&limit=100`
Returns centralized system audit events. Categories: `SYSTEM`, `DEVICE`, `TELEMETRY`, `COMMAND`, `ALERT`, `AUTOMATION`.

---

## 7. Simulation Scenarios Injection (Demo Controls)

### `GET /api/simulation/devices`
Returns live simulation status for all instruments.

### `POST /api/simulation/devices/{id}/scenario`
Injects an operational failure or condition for live demonstrations.

**Request Body**:
```json
{
  "scenario": "HIGH_TEMPERATURE",
  "customValue": 78.5,
  "reason": "Demonstrate thermal cutoff alert"
}
```
*Supported Scenarios:* `NORMAL`, `HIGH_TEMPERATURE`, `LOW_SIGNAL`, `LOW_VOLTAGE`, `DISCONNECT`, `COMMUNICATION_TIMEOUT`, `ERROR`.
