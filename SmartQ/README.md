# SmartQ — Multilingual Smart Token & Queue Management System

Bank-style queue management: kiosk token issuance, staff counter console, public display board with voice announcements, and admin dashboards. All business data is loaded from SQL Server via the .NET 8 API.

## Prerequisites

- Visual Studio 2022
- .NET 8 SDK
- SQL Server 2017+
- SSMS 22
- Node.js 20+
- Angular CLI (`npm install -g @angular/cli`)

## Project Structure

```
SmartQ/
  backend/          # .NET 8 Clean Architecture API
  frontend/smartq-web/  # Angular SPA
```

## Database Setup

1. Create database `SmartQDB` in SSMS (or let EF create it).
2. Update connection string in `backend/SmartQ.API/appsettings.json`:

```json
"DefaultConnection": "Server=localhost;Database=SmartQDB;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=True"
```

**Windows Authentication alternative:**
```
Server=localhost;Database=SmartQDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

3. Apply migrations:

```bash
cd backend
dotnet ef database update --project SmartQ.Infrastructure --startup-project SmartQ.API
```

Seed data includes languages (EN/SI/TA), 5 services, 19 sub-services, 5 counters, staff users, voice templates, and system settings.

## Run Backend

```bash
cd backend/SmartQ.API
dotnet run
```

- **Visual Studio (https profile):** https://localhost:7287/swagger
- **dotnet http profile:** http://localhost:5105/swagger
- SignalR hub: same host as API + `/hubs/queue`

**Important:** Frontend `environment.ts` must use the **same port** as your backend. Default is `https://localhost:7287` (VS https profile). If you run with the **http** profile, change `apiUrl` / `hubUrl` to `http://localhost:5105`.

Or open `SmartQ.slnx` in Visual Studio and run **SmartQ.API**.

## Run Frontend

```bash
cd frontend/smartq-web
npm install
ng serve
```

Open http://localhost:4200

## Application URLs

| Screen | URL |
|--------|-----|
| Kiosk – Language | `/customer/language` |
| Kiosk – Services | `/customer/services` |
| Kiosk – Sub-services | `/customer/services/:id/sub-services` |
| Token success | `/customer/token-success/:tokenId` |
| Public display | `/display/queue-board` |
| Staff console | `/staff/console` |
| Admin dashboard | `/admin/dashboard` |
| Service management | `/admin/services` |
| Counter management | `/admin/counters` |
| Reports | `/admin/reports` |

## No Hardcoding Policy

- Services, sub-services, languages, counters, token prefixes, wait times, branch ID, and voice templates come from **SQL Server** via API.
- Angular components only render API responses.
- Token numbers are generated server-side using `DailyTokenSequence` (format `PREFIX-001`, daily reset per sub-service).

## SignalR Flow

1. Staff clicks **Call Next** → `POST /api/counters/{id}/call-next`
2. API updates token status, counter status, saves history (transactional)
3. Hub broadcasts `TokenCalled`, `QueueUpdated`, `DisplayUpdated`
4. Public display (`/display/queue-board`) receives events and refreshes board
5. `VoiceAnnouncementService` speaks token using `VoiceTemplate` from database

## Token Generation Flow

1. Customer selects language → service → sub-service
2. `POST /api/tokens/generate` with `{ languageId, serviceId, subServiceId }`
3. Server increments daily sequence, creates `WAITING` token, returns token details
4. Kiosk navigates to success screen (token already saved; print uses browser print on receipt area)

## Print Flow

Token success page uses CSS `@media print` with class `print-area` so only the receipt section prints.

## Default Staff Counter

Staff console uses **Counter 02** (Cash Services) by default — matches seed data for Officer Sarah.

## API Overview

- `GET /api/languages`
- `GET /api/services?languageCode=EN`
- `GET /api/services/{id}/sub-services?languageCode=EN`
- `POST /api/tokens/generate`
- `GET /api/display/board`
- `POST /api/counters/{id}/call-next`
- Admin endpoints under `/api/admin/*`
