# InternalDocs

InternalDocs is an internal document approval system for submitting, reviewing, and tracking organization documents. It includes role-based access, approval decisions, document catalog management, notifications, and seeded demo data for local testing.

## What It Does

- Employees submit and update documents with type-specific metadata.
- Approvers review pending documents and approve, reject, or request changes.
- Admins manage users, roles, document categories, and document types.
- Users receive notifications as documents move through the workflow.
- The API exposes Swagger documentation and JWT-protected endpoints.

## Tech Stack

| Area | Technology |
| --- | --- |
| Frontend | React 19, TypeScript, Vite, TanStack Query, Axios, Tailwind CSS |
| Backend | ASP.NET Core, .NET 10, EF Core, Swagger/OpenAPI |
| Database | PostgreSQL |
| Auth | Local JWT auth, optional Microsoft auth through MSAL |
| Tests | Vitest, Testing Library, xUnit, Moq, EF Core InMemory |
| Local deployment | Docker Compose |

## Repository Layout

```text
.
|-- frontend/                         # React/Vite application
|-- InternalDocs/
|   |-- InternalDocs.Api/             # ASP.NET Core controllers and startup
|   |-- InternalDocs.Application/     # Use cases, DTOs, service contracts
|   |-- InternalDocs.Domain/          # Core domain entities
|   |-- InternalDocs.Infrastructure/  # EF Core, repositories, auth, seeding
|   `-- InternalDocs.Tests/           # Backend test suite
|-- docker-compose.yml                # PostgreSQL + API + frontend
|-- .env.example                      # Compose environment template
`-- InternalDocs.slnx                 # .NET solution
```

## Quick Start With Docker

### Prerequisites

- Docker Engine with the Compose plugin

### Run The Full Stack

```powershell
Copy-Item .env.example .env
docker compose up --build
```

Open:

- Frontend: http://localhost:5173
- API Swagger: http://localhost:5210/swagger
- API health check: http://localhost:5210/health

On startup, the API waits for PostgreSQL, applies EF Core migrations, seeds default users, and optionally creates demo workflow data when `SEED_DEMO_DATA=true`.

The production-style frontend container serves the built React app through nginx. Browser calls to `/api/*` are proxied to the API container, so Docker setup does not need separate CORS configuration.

### Stop The Stack

```powershell
docker compose down
```

To remove the PostgreSQL volume and start from a clean database next time:

```powershell
docker compose down -v
```

## Seeded Accounts

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@internaldocs.local` | `AdminPass123!` |
| Employee | `employee@internaldocs.local` | `EmployeePass123!` |
| Approver | `approver@internaldocs.local` | `ApproverPass123!` |

The approver account is included when demo data seeding is enabled. New Microsoft/OAuth users are created as `Employee`; use the admin dashboard to elevate users when needed.

## Configuration

Copy `.env.example` to `.env` for Docker Compose.

| Variable | Purpose | Default |
| --- | --- | --- |
| `FRONTEND_PORT` | Host port for the frontend | `5173` |
| `API_PORT` | Host port for the API | `5210` |
| `DB_PORT` | Host port for PostgreSQL | `5432` |
| `POSTGRES_USER` | PostgreSQL username | `postgres` |
| `POSTGRES_PASSWORD` | PostgreSQL password | `postgres` |
| `POSTGRES_DB` | PostgreSQL database name | `internaldocs` |
| `JWT_SECRET` | JWT signing key, at least 32 characters | development placeholder |
| `JWT_ISSUER` | JWT issuer claim | `InternalDocs` |
| `JWT_AUDIENCE` | JWT audience claim | `InternalDocs` |
| `JWT_EXPIRY_MINUTES` | JWT lifetime in minutes | `60` |
| `SEED_DEMO_DATA` | Seed demo documents and workflow history | `true` |

Optional Microsoft login settings used by the frontend Docker build:

| Variable | Purpose |
| --- | --- |
| `VITE_MICROSOFT_CLIENT_ID` | Microsoft application client ID |
| `VITE_MICROSOFT_TENANT_ID` | Microsoft tenant ID |
| `VITE_MICROSOFT_REDIRECT_URI` | Redirect URI, usually the frontend origin |
| `VITE_MICROSOFT_API_SCOPE` | API scope requested by MSAL |

## Local Development Without Docker

You can run the database in Docker and run the API/frontend directly on your machine.

### Backend

Prerequisites:

- .NET 10 SDK
- PostgreSQL, or the Compose database service

Start PostgreSQL:

```powershell
docker compose up db
```

In another terminal, configure the local connection string and run the API:

```powershell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=internaldocs;Username=postgres;Password=postgres"
$env:Jwt__Secret="local-dev-jwt-secret-change-me-min-32-chars"
$env:Jwt__Issuer="InternalDocs"
$env:Jwt__Audience="InternalDocs"
$env:Jwt__ExpiryMinutes="60"
$env:SEED_DEMO_DATA="true"
dotnet run --project InternalDocs/InternalDocs.Api
```

The API runs at http://localhost:5210 by default when using the included launch settings.

### Frontend

Prerequisites:

- Node.js
- npm

```powershell
cd frontend
npm install
npm run dev
```

The frontend defaults to `http://localhost:5210` for API calls. Override it when needed:

```powershell
$env:VITE_API_BASE_URL="http://localhost:5210"
npm run dev
```

## Common Commands

Backend:

```powershell
dotnet build InternalDocs.slnx
dotnet test InternalDocs/InternalDocs.Tests/InternalDocs.Tests.csproj
dotnet run --project InternalDocs/InternalDocs.Api
```

Frontend:

```powershell
cd frontend
npm run dev
npm run build
npm run lint
npm run format:check
npm test
```

Docker:

```powershell
docker compose up --build
docker compose down
docker compose down -v
```

## Approval Workflow

The workflow is intentionally explicit and reviewer-driven:

1. An employee submits a document.
2. The document enters `PendingApproval`.
3. An approver reviews the pending item.
4. The approver chooses `Approved`, `Rejected`, or `ChangesRequested`.
5. If changes are requested, the employee can update and resubmit the document.

The system does not automatically approve documents based on type, metadata, validation, or version changes. `Approved` and `Rejected` are terminal review decisions.

## Main API Areas

| Area | Representative routes |
| --- | --- |
| Auth | `POST /auth/local/login`, `POST /auth/local/register`, `GET /auth/me` |
| Documents | `GET /documents`, `GET /documents/my`, `POST /documents`, `PUT /documents/{id}` |
| Approvals | `GET /approvals/pending`, `POST /approvals/{documentId}/approve`, `POST /approvals/{documentId}/reject` |
| Catalog | `GET /document-categories`, `POST /document-types`, `PUT /document-types/{id}` |
| Admin users | `GET /admin/users`, `PUT /admin/users/{id}/role`, `PUT /admin/users/{id}/status` |
| Notifications | `GET /notifications`, `POST /notifications/{id}/read`, `POST /notifications/read-all` |

Use Swagger at `/swagger` for the full request and response contract.

## Architecture

The backend follows a Clean Architecture-style dependency flow:

```text
InternalDocs.Domain
  <- InternalDocs.Application
      <- InternalDocs.Infrastructure
      <- InternalDocs.Api
```

- `Domain` contains core entities such as `Document`, `User`, `ApprovalAction`, and `Notification`.
- `Application` contains business use cases, DTOs, validation, and service/repository interfaces.
- `Infrastructure` implements EF Core persistence, PostgreSQL repositories, JWT generation, password hashing, Microsoft auth support, migrations, and seed data.
- `Api` exposes HTTP controllers, authentication/authorization, Swagger, CORS, and startup wiring.

The frontend is organized into:

- `auth/` for session state, token storage, and Microsoft auth setup.
- `api/` for typed HTTP calls.
- `hooks/` for TanStack Query fetch/mutation logic.
- `pages/` and `components/` for the user interface.

## Testing

Backend tests cover auth behavior, document submission rules, approval transitions, repository versioning, DTO mapping, and controller metadata:

```powershell
dotnet test InternalDocs/InternalDocs.Tests/InternalDocs.Tests.csproj
```

Frontend tests cover auth protection, dashboard behavior, admin flows, and document preview logic:

```powershell
cd frontend
npm test
```

## Notes For Contributors

- Keep controllers thin; put business behavior in application services.
- Keep EF Core and external service details in Infrastructure.
- Do not accept user IDs such as `CreatedByUserId` or `ApproverId` from request bodies; derive them from the authenticated JWT.
- Preserve explicit approval decisions. Do not add automatic approval based only on metadata or validation.
- Run the relevant backend and frontend tests before opening a PR.
