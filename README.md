# POC-SensorPlatform

A local proof of concept sensor platform with:
- Backend API for login, ingest, and sensor history
- Blazor WebAssembly frontend for login and dashboard
- SQLite for local data storage
- HMAC verification for device ingest
- JWT authentication for user access to protected measurement APIs

## 1. Architecture

Projects:
- SensorPlatform.Api: ASP.NET Core API (auth, ingest, protected measurement endpoints)
- SensorPlatform.Application: EF Core DbContext, services, security helpers, seed logic
- SensorPlatform.Domain: entities and core domain model
- SensorPlatform.Web: Blazor WebAssembly client

Separation of concerns:
- Transport logic (HTTP controllers) is in SensorPlatform.Api.
- Domain and application logic (ingest validation, auth validation, query logic) is in SensorPlatform.Application.
- This makes it possible to add MQTT as another transport without duplicating core ingest validation logic.

## 2. Data model

Entities:
- User
  - Id, Username (unique), PasswordHash, CreatedAt
- Device
  - Id, DeviceId (unique), Name, SensorType, HmacSecret, IsActive, UserId
- SensorReading
  - Id, DeviceId, Value, Timestamp (device timestamp), ReceivedAt
- IngestMessage
  - Id, DeviceId, Timestamp, Nonce, Signature, PayloadHash, IsAccepted, RejectReason, ReceivedAt
  - Unique index on (DeviceId, Nonce) for replay protection

## 3. Security model

Two separate authentication mechanisms are used:

1. User authentication (frontend -> backend)
- Username/password login endpoint: POST /api/auth/login
- Password stored as PBKDF2 hash (not plain text)
- JWT token returned and used as Bearer token
- Protected measurement endpoints require JWT

2. Device authentication (device/simulator -> backend)
- HMAC signature required for ingest endpoint: POST /api/ingest
- Signature is computed over this exact payload string:
  - deviceId|value|timestamp|nonce
- Server verifies HMAC with device secret before accepting data

## 4. Replay and duplicate handling

Ingest processing includes:
- Required field and value validation
- Stale timestamp check (clock skew > 120 seconds is rejected)
- Replay protection via unique (DeviceId, Nonce)
- Duplicate reading check for same DeviceId + Timestamp + Value
- IngestMessage audit rows for accepted and rejected requests

## 5. Local run instructions

Prerequisites:
- .NET SDK 9.0+
- (Optional) jq and openssl for script-based ingest tests

From repo root:

1. Restore and build
- dotnet restore SensorPlatform.sln
- dotnet build SensorPlatform.sln

2. Start backend API
- dotnet run --project SensorPlatform.Api
- API default: http://localhost:5143

3. Start frontend
- dotnet run --project SensorPlatform.Web
- Web default: http://localhost:5190

First startup behavior:
- Database is migrated automatically.
- Seed data is added automatically:
  - User: demo / demo123
  - Devices:
    - sensor-temp-01 (secret: temp-01-secret)
    - sensor-hum-01 (secret: hum-01-secret)
    - sensor-co2-01 (secret: co2-01-secret)
    - sensor-press-01 (secret: press-01-secret)
  - Initial sample readings for each sensor

## 6. Manual test steps

### A. Login and protected API

1. Open web app at http://localhost:5190
2. Login with demo / demo123
3. Open dashboard and verify latest values + history per sensor

### B. API login

POST /api/auth/login with:

```json
{
  "username": "demo",
  "password": "demo123"
}
```

Use returned token for:
- GET /api/measurements/latest
- GET /api/measurements/sensors
- GET /api/measurements/history/{deviceId}?take=100

### C. Ingest valid data with simulator script

Run from repo root:

```bash
./scripts/send-sample.sh
```

Optional environment overrides:

```bash
API_BASE_URL=http://localhost:5143 \
DEVICE_ID=sensor-temp-01 \
SECRET=temp-01-secret \
VALUE=23.7 \
./scripts/send-sample.sh
```

Expected:
- Valid signature accepted (200)
- Data appears in dashboard history

### D. Ingest invalid signature

Change SECRET to a wrong value and run script again.
Expected: 401 Unauthorized

### E. Replay-like request

Reuse exactly the same nonce and timestamp.
Expected: 409 Conflict

## 7. MQTT extension strategy (without duplicating core logic)

How to add MQTT in same architecture:
- Keep current ingest rules in SensorPlatform.Application.IngestService.
- Add a new transport adapter in API or a worker service:
  - MQTT subscriber receives payload from topic
  - Adapter maps MQTT message to IngestService input
  - Calls IngestService.ProcessAsync(...) exactly like HTTP controller
- Result:
  - Same validation, HMAC verification, replay checks, and persistence
  - No duplication of business/security logic
  - HTTP and MQTT become two transport entry points over same core service

## 8. Assumptions and limitations

Assumptions:
- POC is single-node local runtime with SQLite.
- One local demo user is enough for demonstration.

Limitations:
- Secrets are hardcoded seed values for local demo only.
- No refresh-token flow.
- No advanced observability/monitoring.
- Basic dashboard table visualization (no advanced charting).
