# InternalDocs

## Run with Docker Compose

The whole stack — PostgreSQL, the .NET API, and the React frontend — runs as three
containers with a single command.

### Prerequisites

- Docker Engine with the Compose plugin (`docker compose version`)

### Start

```bash
cp .env.example .env        # adjust ports / secrets if needed
docker compose up --build
```

On first start the API waits for PostgreSQL to be healthy, applies all EF Core
migrations, and seeds the login accounts (plus demo documents when
`SEED_DEMO_DATA=true`). Then open:

- Frontend: http://localhost:5173
- API + Swagger: http://localhost:5210/swagger
- API health probe: http://localhost:5210/health

The browser talks only to the frontend origin; nginx reverse-proxies `/api/*`
to the API container, so there is no CORS configuration to manage.

### Seeded logins

| Role     | Email                          | Password         |
| -------- | ------------------------------ | ---------------- |
| Admin    | `admin@internaldocs.local`     | `AdminPass123!`  |
| Employee | `employee@internaldocs.local`  | `EmployeePass123!` |
| Approver | `approver@internaldocs.local`  | `ApproverPass123!` |

### Configuration

All settings are environment variables (see `.env.example`):

| Variable | Purpose | Default |
| --- | --- | --- |
| `FRONTEND_PORT` / `API_PORT` / `DB_PORT` | Host port mappings | `5173` / `5210` / `5432` |
| `POSTGRES_USER` / `POSTGRES_PASSWORD` / `POSTGRES_DB` | Database credentials | `postgres` / `postgres` / `internaldocs` |
| `JWT_SECRET` | JWT signing key (**min 32 chars**) | dev placeholder |
| `JWT_ISSUER` / `JWT_AUDIENCE` / `JWT_EXPIRY_MINUTES` | JWT claims/lifetime | `InternalDocs` / `InternalDocs` / `60` |
| `SEED_DEMO_DATA` | Seed demo documents | `true` |

The API connection string is composed from the `POSTGRES_*` values and points at
the `db` service on the compose network — no manual connection string needed.

### Teardown

```bash
docker compose down       # stop and remove containers (keeps the database)
docker compose down -v    # also delete the PostgreSQL volume (fresh DB next time)
```

## Backend Auth and Roles

The API uses JWT bearer authentication. Configure these settings in `InternalDocs/InternalDocs.Api/appsettings.json` or `.env`:

- `Jwt:Secret` (min 32 chars)
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:ExpiryMinutes`

### Roles

- `Admin` manages setup data (document categories and document types).
- `Employee` submits, updates, and deletes their own documents.
- `Approver` creates and updates approval actions.

### Admin User Management

Seeded admins can promote users to `Approver` (or `Admin`) and activate/deactivate accounts.
To create the first approver in development:

1. Sign in as the seeded admin (`admin@internaldocs.local` / `AdminPass123!`).
2. Open **Manage users** in the admin dashboard.
3. Find the employee account and change the role to `Approver`.

### Local Development Accounts

Development startup seeds two password-login accounts:

- Admin: `admin@internaldocs.local` / `AdminPass123!`
- Employee: `employee@internaldocs.local` / `EmployeePass123!`

Both local password login and Microsoft OAuth use the user's persisted role. New Microsoft/OAuth users are created as `Employee`; elevated access should be assigned from the seeded admin account.

### Optional Demo Data

Set `SEED_DEMO_DATA=true` in `InternalDocs/InternalDocs.Api/.env` to seed an idempotent development dataset with documents across the approval workflow, approval history entries, notifications, and an HR leave-request example. This also adds:

- Approver: `approver@internaldocs.local` / `ApproverPass123!`
- Reviewer: `reviewer@internaldocs.local` / `ReviewerPass123!`
- Employee: `sara.hadzic@internaldocs.local` / `DemoPass123!`

The demo records are created only when missing, so restarting the API does not duplicate or overwrite them.

## Approval Workflow Decision

The app uses manual approval with limited automatic status movement.

- Employee submit creates a document in `PendingApproval`.
- Employee resubmit after requested changes returns the document to `PendingApproval`.
- The system must not automatically accept or approve a document based on document type, metadata, version changes, or validation passing.
- A document is accepted only when an `Approver` explicitly approves it and the document status becomes `Approved`.
- `Approved` and `Rejected` are terminal reviewer decisions. `ChangesRequested` returns work to the employee for revision.
- Keep this as a simple status-machine flow with explicit endpoints; do not add a PR-like branch/review/merge model or configurable workflow engine.

### Role-protected endpoints

Document catalog (Admin only):

- `POST /document-categories`
- `PUT /document-categories/{id}`
- `DELETE /document-categories/{id}`
- `POST /document-types`
- `PUT /document-types/{id}`
- `DELETE /document-types/{id}`

Documents (Employee only):

- `POST /documents`
- `PUT /documents/{id}`
- `DELETE /documents/{id}`

Approvals (Approver only):

- `POST /approvals`
- `PUT /approvals/{id}`

Notes:

- The API reads the current user id from the JWT and does not accept `CreatedByUserId` or `ApproverId` in request bodies.
- `GET /document-categories` and `GET /document-types` are authenticated but available to any role.
