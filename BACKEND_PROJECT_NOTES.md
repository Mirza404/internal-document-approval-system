# Backend Project Notes

Quick reference for explaining the .NET backend architecture.

## 1. Dependency Flow And Clean Architecture

This backend is split into separate .NET projects:

```txt
InternalDocs.Domain
  <- InternalDocs.Application
      <- InternalDocs.Infrastructure
      <- InternalDocs.Api
```

Actual project references:

- `Domain` references nothing.
- `Application` references `Domain`.
- `Infrastructure` references `Application` and `Domain`.
- `Api` references `Application` and `Infrastructure`.
- `Tests` references all backend projects.

Clean Architecture rule:

> Inner layers should not depend on outer layers.

In this project:

- `Domain` contains core entities only.
- `Application` contains business use cases and interfaces.
- `Infrastructure` contains database, repositories, JWT, password hashing, Microsoft Graph calls, migrations, and seeding.
- `Api` exposes HTTP endpoints and wires everything together.

The important principle is dependency inversion:

- Application defines interfaces like `IDocumentRepository` and `IAuthService`.
- Infrastructure implements those interfaces with EF Core, PostgreSQL, BCrypt, JWT, etc.
- API depends on interfaces/services, not directly on database queries.

## 2. What APIs Are

An API is the backend contract that outside clients use.

In this project, APIs are HTTP endpoints exposed by ASP.NET controllers:

```txt
GET    /documents
POST   /documents
POST   /auth/local/login
GET    /approvals/pending
POST   /approvals/{documentId}/approve
```

The frontend calls these endpoints through `frontend/src/api`.

Backend API flow:

```txt
HTTP request
  -> Controller
  -> Application service
  -> Repository interface
  -> Infrastructure repository
  -> Database
```

Controllers should stay thin: validate route/body/auth basics, call Application services, and translate service results into HTTP responses.

## 3. What The Test Suite Covers

The test project is `InternalDocs.Tests`.

Current verification:

```txt
dotnet test InternalDocs.Tests/InternalDocs.Tests.csproj
Passed: 77, Failed: 0, Skipped: 0
```

There is one analyzer warning about `Notification.IsRead` being explicitly initialized to its default value.

Main coverage:

- `AuthServiceTests`
  - Microsoft registration creates `Employee` users.
  - Microsoft login rejects inactive users.
  - Local login works for valid admin/employee credentials.

- `AdminUserServiceTests`
  - Rejects role updates for non-IUS emails.
  - Allows promotion to `Approver`.
  - Toggles active/inactive user state.

- `ApplicationServiceTests`
  - Document submission validation.
  - Required metadata for payment/internship documents.
  - Transcript/certificate metadata rules.
  - Document versioning rules.
  - Resubmission rules.
  - Approval status validation.
  - Approval decision response data.

- `DocumentApprovalFlowTests`
  - Approve/reject/request-changes transitions.
  - Failure cases for missing document, wrong status, missing approver, invalid approver.

- `DocumentRepositoryTests`
  - EF repository behavior for resubmission creating document versions.

- `PendingApprovalItemDtoTests`
  - Mapping from domain `Document` to pending approval DTO, including optional metadata.

- `ControllerTests`
  - Controller route prefixes.
  - HTTP method mappings.
  - authorization requirements and role restrictions.
  - declared response statuses.

Short version:

> Tests mostly cover business rules, approval/document flows, auth behavior, repository versioning behavior, DTO mapping, and controller metadata.

## 4. Application Layer

The Application layer is the use-case/business layer. It knows the domain entities and defines what the system can do, but it should not know PostgreSQL, EF Core, JWT implementation details, or HTTP details.

### Abstractions

Contains interfaces.

- `Abstractions/Repositories`
  - Contracts for data access, such as `IDocumentRepository`, `IUserRepository`, `IApprovalActionRepository`, `INotificationRepository`.
  - Application services depend on these interfaces.
  - Infrastructure provides the actual EF Core implementations.

- `Abstractions/Services`
  - Contracts for use-case services, such as `IDocumentService`, `IApprovalService`, `IAuthService`, `INotificationService`.
  - API controllers call these service interfaces.

### Approvals

Approval use cases and DTOs:

- Create/update approvals.
- Get approval history.
- Get pending approval queue.
- Approve, reject, or request changes.
- Produces `ApprovalDto` and `PendingApprovalItemDto`.

### Auth

Auth command and response DTO types:

- Local login/register commands.
- Microsoft login/register commands.
- `AuthDto`, returned after successful auth.

Most implementation is in Infrastructure because it uses BCrypt, JWT, and Microsoft Graph.

### Common

Shared result/error types:

- `ServiceResult`
- `ServiceResult<T>`
- `ServiceErrorType`

This avoids throwing exceptions for normal business failures. Services return structured outcomes like validation error, not found, or conflict. Controllers translate those into `400`, `404`, or `409`.

### DocumentCatalog

Document category/type use cases:

- Manage document categories.
- Manage document types.
- Validate whether categories/types can be deleted.
- Return category/type DTOs.

### Documents

Document use cases:

- Submit documents.
- Update documents.
- Delete documents.
- List all documents or current user's documents.
- Enforce metadata rules.
- Handle document versioning.

### Interfaces

Currently empty. The real interfaces are under `Abstractions`.

### Notifications

Notification use cases:

- Get notifications for a user.
- Mark one notification as read.
- Mark all as read.
- Create notifications for one or many users.

### Users

Admin user management:

- List users.
- Get one user.
- Change role.
- Activate/deactivate users.
- Enforces role/business rules such as IUS email restrictions.

## 5. Domain Layer

The Domain layer contains entities.

Entities are the core business objects:

- `User`
- `Document`
- `DocumentCategory`
- `DocumentType`
- `DocumentVersion`
- `ApprovalAction`
- `Notification`
- `AuditLog`

They are similar to Mongoose models in the sense that they describe the shape of important persisted objects.

But in Clean Architecture, entities are more than database shapes:

> Entities represent core business concepts, relationships, and state that the rest of the app works with.

Example:

- `Document` has title, status, priority, document type, creator, versions, and approval actions.
- `User` has email, name, role, active state, documents, approvals, notifications, and audit logs.
- `ApprovalAction` records who approved/rejected/requested changes on a document.

The Domain layer should stay independent from web/API/database concerns.

## 6. Infrastructure Layer

Infrastructure is the technical implementation layer.

It contains database access, auth implementation, migrations, repository implementations, and seed data.

### Auth

- `AuthService`
  - Implements `IAuthService`.
  - Handles local login/register.
  - Handles Microsoft login/register.
  - Uses BCrypt for passwords.
  - Calls Microsoft Graph to validate Microsoft access tokens.
  - Creates users and enforces IUS email restrictions.

- `TokenService`
  - Implements `ITokenService`.
  - Generates JWT tokens.
  - Reads JWT issuer/audience/secret/expiry from configuration.

### Data

- `AppDbContext`
  - EF Core database context.
  - Defines database tables with `DbSet<T>`.
  - Configures keys, required fields, max lengths, relationships, indexes, delete behavior, precision, and JSON columns.

This is the main data access foundation.

### Migrations

Migrations are EF Core's database change history.

They describe how the database schema changes over time:

- initial table creation.
- adding Microsoft object IDs.
- adding/changing user roles.
- adding document categories.
- adding document metadata fields.
- adding document semantic versions.

`AppDbContextModelSnapshot.cs` is EF Core's current model snapshot used to calculate future migrations.

Short version:

> Migrations are version-controlled database schema updates.

### Repositories

Repository implementations for Application interfaces:

- `DocumentRepository`
- `ApprovalActionRepository`
- `DocumentTypeRepository`
- `NotificationRepository`
- `UserRepository`

They use `AppDbContext` and EF Core queries. This keeps EF Core out of the Application layer.

### Seeds

- `DatabaseSeeder`
  - Creates default local users.
  - Creates default document categories and document types.
  - Optionally creates demo users/documents/approval data when `SEED_DEMO_DATA=true`.

Short version:

> Seeds create baseline/demo data so the app has usable starting records.

### Dependency Injection File

`InternalDocs.Infrastructure/DependencyInjection.cs` exposes `AddInfrastructure(...)`.

It registers:

- `AppDbContext` with PostgreSQL.
- Repository interfaces to repository implementations.
- Auth services.
- Application services.
- Admin user service.
- Named `HttpClient` for Microsoft Graph.

It also has `InitializeDatabaseAsync(...)`, which:

- applies migrations with `MigrateAsync()`.
- seeds default local users.
- seeds document categories/types.
- optionally seeds demo data.

Short version:

> The dependency injection file tells .NET which concrete class to use when something asks for an interface.

## 7. What `.csproj` Files Are For

`.csproj` files are .NET project files.

They define:

- project SDK type, such as web app or class library.
- target framework, here `net10.0`.
- nullable/reference settings.
- NuGet package dependencies.
- references to other projects.
- test project settings.
- build/analyzer settings.

Examples:

- `InternalDocs.Api.csproj`
  - Web API project.
  - References Application and Infrastructure.
  - Adds ASP.NET auth, OpenAPI, Swagger, EF design packages.

- `InternalDocs.Application.csproj`
  - Application class library.
  - References Domain.

- `InternalDocs.Domain.csproj`
  - Core domain class library.
  - References no other project.

- `InternalDocs.Infrastructure.csproj`
  - Infrastructure class library.
  - References Application and Domain.
  - Adds EF Core, PostgreSQL, BCrypt, JWT packages.

- `InternalDocs.Tests.csproj`
  - Test project.
  - Adds xUnit, Moq, EF InMemory, test SDK.
  - References all backend projects.

Short version:

> `.csproj` tells .NET how to build each project, what packages it needs, and which other projects it depends on.

## Best 30-Second Explanation

> The backend follows Clean Architecture. Domain has the core entities. Application has use cases, DTOs, business services, and interfaces. Infrastructure implements database/auth/repository details using EF Core, PostgreSQL, JWT, BCrypt, and Microsoft Graph. API exposes HTTP endpoints through controllers and maps requests/responses. The dependency direction points inward, so business logic does not depend directly on controllers or database technology.
