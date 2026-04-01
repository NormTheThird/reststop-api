# RestStop API

ASP.NET Core Web API for the RestStop restroom rating and gas station discovery platform.

## Tech Stack

- **Framework** — ASP.NET Core 8
- **Database** — PostgreSQL + PostGIS (geolocation queries)
- **ORM** — Entity Framework Core
- **Auth** — JWT + refresh tokens, OTP via AWS SES/SNS, OAuth (Google, Apple)
- **Infrastructure** — AWS Lambda, RDS, SES, SNS

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for local Postgres)
- AWS credentials configured locally (for SES/SNS in dev)

## Getting Started

### 1. Start local database

```bash
docker-compose up -d
```

This starts PostgreSQL with PostGIS on port `5432`.

### 2. Set up user secrets

```bash
cd src/RestStop.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=reststop;Username=postgres;Password=postgres"
dotnet user-secrets set "Jwt:Secret" "your-local-dev-secret-min-32-chars"
dotnet user-secrets set "Jwt:Issuer" "reststop-dev"
dotnet user-secrets set "Jwt:Audience" "reststop-dev"
```

### 3. Run migrations

```bash
cd src/RestStop.Api
dotnet ef database update
```

### 4. Run the API

```bash
dotnet run
```

API is available at `https://localhost:5001`. Swagger UI at `https://localhost:5001/swagger`.

## Project Structure

```
src/
├── RestStop.Api.sln
├── RestStop.Api/           — main API project
│   ├── Controllers/        — routing and response shaping only
│   ├── Interfaces/         — service and repository contracts
│   ├── Services/           — all business logic
│   ├── Data/               — EF Core DbContext, migrations, repositories
│   ├── Models/             — database entities
│   ├── DTOs/               — request and response shapes
│   ├── Middleware/         — exception handling, rate limiting
│   ├── Helpers/            — geo calculations, trust weight logic
│   └── Infrastructure/     — AWS SES and SNS senders
└── RestStop.Api.Tests/     — unit tests
```

## Running Tests

```bash
cd src/RestStop.Api.Tests
dotnet test
```

## Environment Variables

See `appsettings.json` for all configuration keys. Never commit secrets — use `dotnet user-secrets` locally and AWS Secrets Manager in production.

| Key | Description |
|-----|-------------|
| `ConnectionStrings:Default` | PostgreSQL connection string |
| `Jwt:Secret` | Signing key — min 32 characters |
| `Jwt:Issuer` | JWT issuer claim |
| `Jwt:Audience` | JWT audience claim |
| `Jwt:AccessTokenExpiryMinutes` | Access token lifetime (default 15) |
| `Jwt:RefreshTokenExpiryDays` | Refresh token lifetime (default 30) |
| `Aws:Region` | AWS region for SES and SNS |
| `Aws:SesFromAddress` | Verified SES sender address |
| `Otp:ExpiryMinutes` | OTP code lifetime (default 10) |
| `Otp:MaxAttemptsPerHour` | Rate limit per phone/email per hour |

## API Endpoints

### Auth
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/auth/send-code` | Send OTP to email or phone |
| POST | `/api/auth/verify-code` | Verify OTP, return JWT |
| POST | `/api/auth/login` | Email + password login |
| POST | `/api/auth/register` | Create full account |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/logout` | Revoke refresh token |

### Locations
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/locations/nearby` | Stations near lat/lng |
| GET | `/api/locations/{id}` | Station detail with restroom ratings |

### Reviews
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/reviews` | Submit a review (GPS gated) |
