# 📄 Internal Document Approval System — Full Project Plan
> **IUS Campus Management Platform** | Tech Stack: .NET Core · React (Vite + TS) · PostgreSQL

---

## 📌 System Summary

A workflow-based platform for internal university documents (requests, approvals, administrative forms).  
**Employees** create document requests → **Approvers** review and act on them → **Admins** oversee the entire process, manage users, and view analytics.

---

## 👥 Roles

| Role | Key Responsibilities |
|---|---|
| **Admin** | Manage users, document types, view all documents, audit logs, analytics dashboard |
| **Employee** | Submit document requests, track status, respond to change requests, view version history |
| **Approver** | Review assigned documents, approve/reject/request changes, leave comments |

---

## 🗄️ Entities (6 total — exceeds the 5 minimum)

### 1. `User`
```
Id, FullName, Email, PasswordHash, Role (enum: Admin|Employee|Approver),
Department, IsActive, CreatedAt
```

### 2. `DocumentType`
```
Id, Name, Description, CreatedAt
```
> Examples: "Leave Request", "Budget Approval", "Policy Change", "Academic Form"

### 3. `Document`
```
Id, Title, Description, DocumentTypeId (FK), SubmittedById (FK → User),
CurrentStatus (enum), CurrentVersion (int), CreatedAt, UpdatedAt
```

**Status enum values:** `Draft → PendingApproval → UnderReview → ChangesRequested → Approved → Rejected`

### 4. `DocumentVersion`
```
Id, DocumentId (FK), VersionNumber, Content (text or file path),
SubmittedById (FK), CreatedAt, ChangeNote
```
> Every time an Employee edits and resubmits, a new version is created. This satisfies the **version history** requirement.

### 5. `ApprovalAction`
```
Id, DocumentId (FK), PerformedById (FK → User),
Action (enum: Approved|Rejected|ChangesRequested|Commented),
Comment (text), CreatedAt, VersionNumber (which version was acted on)
```
> This is your **multi-step approval workflow** + **comments** table in one.

### 6. `Notification`
```
Id, UserId (FK), DocumentId (FK), Message, IsRead, CreatedAt
```

### 7. `AuditLog` *(bonus — easy, great for analytics)*
```
Id, DocumentId (FK), PerformedById (FK), Action (string), Details, Timestamp
```

---

## 🔄 Core Workflow (the "complex workflow" requirement)

```
Employee fills out a document request
           ↓
   Status: "PendingApproval"
   Notification → all Approvers (or a specific one)
           ↓
   Approver opens document → Status: "UnderReview"
           ↓
   Approver can:
   ├── ✅ Approve     → Status: "Approved"   (terminal) → notify Employee
   ├── ❌ Reject      → Status: "Rejected"   (terminal) → notify Employee
   └── 🔄 Request Changes → Status: "ChangesRequested" → notify Employee
                ↓
         Employee revises → new DocumentVersion created
         Status resets to "PendingApproval"
         Loop back to Approver
           ↓
   Every action is written to AuditLog
   Every state change triggers a Notification
```

---

## 🏗️ Architecture

### Backend — .NET Core Web API

```
/src
  /Controllers        ← HTTP endpoints
  /Services           ← Business logic (never put logic in controllers)
  /DTOs               ← What the API sends/receives (never expose raw DB models)
  /Models             ← EF Core entity classes
  /Data               ← AppDbContext
  /Middleware         ← JWT config, global error handler
  /Helpers            ← JwtService, PasswordHelper, etc.
```

**NuGet Packages needed:**
```
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.EntityFrameworkCore.Design
Npgsql.EntityFrameworkCore.PostgreSQL
BCrypt.Net-Next
AutoMapper.Extensions.Microsoft.DependencyInjection
```

### Frontend — React + Vite + TypeScript

```
/src
  /api               ← axios instance + one file per entity (documentsApi.ts, authApi.ts…)
  /components        ← reusable UI (StatusBadge, DocumentCard, CommentThread…)
  /pages             ← one folder per role (admin/, employee/, approver/)
  /context           ← AuthContext.tsx (JWT + current user)
  /hooks             ← useAuth, useDocuments, useNotifications
  /types             ← TypeScript interfaces mirroring backend DTOs
  /utils             ← formatDate, statusColor, roleGuard
```

---

## 📋 All Tasks — Detailed Breakdown

---

### 🔧 EPIC 1: Project Setup

---

#### [SETUP-1] Initialize .NET Web API

**What to do:**
- Run `dotnet new webapi -n DocumentApprovalAPI`
- Add all NuGet packages listed above
- Configure `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=docapproval;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Secret": "your-very-long-secret-key-at-least-32-chars",
    "Issuer": "DocumentApprovalAPI",
    "Audience": "DocumentApprovalClient",
    "ExpiresInMinutes": 480
  }
}
```
- Remove the default `WeatherForecast` controller
- Enable CORS for `http://localhost:5173` (Vite default port)

📖 [ASP.NET Core Web API overview](https://learn.microsoft.com/en-us/aspnet/core/web-api/)  
📖 [CORS in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/cors)

**Vibe-codability: 🟡 Medium**  
AI can scaffold this but the JWT secret length, CORS origin, and connection string format need human verification. Don't let teammates blindly copy-paste the JWT config — one wrong field and auth breaks silently.

---

#### [SETUP-2] Initialize React + Vite + TypeScript

**What to do:**
```bash
npm create vite@latest document-approval-client -- --template react-ts
cd document-approval-client
npm install axios react-router-dom @tanstack/react-query
```
- Pick a UI library: recommended **shadcn/ui** (copy-paste components, no bloat) or **Mantine** (full-featured)
- Set up folder structure as described in architecture section
- Create `.env` file:
```
VITE_API_URL=http://localhost:5000/api
```

📖 [Vite + React + TS guide](https://vitejs.dev/guide/)

**Vibe-codability: 🟢 Easy** — Fully scaffoldable. Pure boilerplate.

---

#### [SETUP-3] PostgreSQL + Entity Framework Core Setup

**What to do:**
- Create `AppDbContext.cs` with all DbSets
- Register it in `Program.cs`:
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```
- Run migrations:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```
- Seed one default Admin user in `OnModelCreating` or via a seeder class

📖 [EF Core with PostgreSQL (Npgsql)](https://www.npgsql.org/efcore/)  
📖 [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

**Vibe-codability: 🟡 Medium**  
AI writes the DbContext and entities well. But migration commands must be run manually. The seeder for the Admin user needs BCrypt hashing — make sure that's included, AI sometimes forgets to hash the seed password.

---

#### [SETUP-4] JWT Authentication Middleware

**What to do:**  
In `Program.cs`, add:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
    });

// After builder.Build():
app.UseAuthentication();
app.UseAuthorization();
```

Create a `JwtService.cs` that generates tokens, embedding the user's `Id`, `Email`, and `Role` as claims.

📖 [JWT Authentication in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)  
📖 [Claims-based identity](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/claims)

**Vibe-codability: 🔴 Hard — Do NOT leave this to autopilot.**  
This is the #1 place where AI-generated code silently breaks. Common failure modes:
- Wrong key length (must be ≥ 256 bits for HS256, meaning ≥ 32 characters)
- Forgetting `app.UseAuthentication()` before `app.UseAuthorization()`
- Mismatched Issuer/Audience between token generation and validation
- Not embedding the Role claim correctly (it must use `ClaimTypes.Role`)

Have a human read the final `Program.cs` auth section against the docs.

---

### 🔐 EPIC 2: Authentication & Authorization

---

#### [AUTH-1] Register & Login API Endpoints

**Endpoints:**
```
POST /api/auth/register
POST /api/auth/login
GET  /api/auth/me          ← returns current user info from JWT
```

**DTOs:**
```csharp
// RegisterDto: FullName, Email, Password, Role, Department
// LoginDto: Email, Password
// AuthResponseDto: Token, UserId, FullName, Role
```

**Logic:**
- Register: hash password with BCrypt, save user, return JWT
- Login: find user by email, verify `BCrypt.Verify(password, hash)`, return JWT

📖 [BCrypt.Net-Next usage](https://github.com/BcryptNet/bcrypt.net)

**Vibe-codability: 🟢 Easy** — Standard pattern, AI handles it well once JWT middleware is confirmed working.

---

#### [AUTH-2] Role-Based Authorization on Endpoints

**What to do:**  
Use `[Authorize]` and `[Authorize(Roles = "Admin")]` attributes on controllers/actions:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // All endpoints in this controller require a valid JWT
public class DocumentsController : ControllerBase
{
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]  // Only Admin can delete
    public async Task<IActionResult> Delete(int id) { ... }
}
```

To get the current user's ID from the token inside a controller:
```csharp
var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
```

📖 [Role-based authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles)

**Vibe-codability: 🟢 Easy** — Just attributes. AI applies these correctly.

---

#### [AUTH-3] Frontend Auth Context + Protected Routes

**What to do:**
- `AuthContext.tsx`: stores JWT in `localStorage`, decodes payload (using `jwt-decode` package) to get `{ userId, role, fullName }`
- `ProtectedRoute.tsx`: wrapper that checks auth state and role, redirects to `/login` if not authorized
- On login success → redirect to role-specific dashboard

```tsx
// Usage in router:
<Route path="/admin/*" element={<ProtectedRoute role="Admin"><AdminLayout /></ProtectedRoute>} />
```

**Vibe-codability: 🟢 Easy** — Fully AI-generatable. Standard React pattern.

---

### 📁 EPIC 3: Document Types (Admin)

---

#### [DOCTYPE-1] Document Type CRUD API

**Endpoints (Admin only):**
```
GET    /api/document-types
POST   /api/document-types
PUT    /api/document-types/{id}
DELETE /api/document-types/{id}
```

**Vibe-codability: 🟢 Easy** — Pure CRUD. AI does this in one prompt.

---

#### [DOCTYPE-2] Document Type Management UI (Admin)

- Table listing all document types
- Inline edit or modal form for create/update
- Confirmation dialog for delete

**Vibe-codability: 🟢 Easy**

---

### 📝 EPIC 4: Document Submission (Employee)

---

#### [DOC-1] Submit Document API

**Endpoint:** `POST /api/documents` *(Employee only)*

**What happens server-side:**
1. Create `Document` record with `Status = PendingApproval`, `CurrentVersion = 1`
2. Create `DocumentVersion` record (version 1) with the submitted content
3. Write to `AuditLog`: "Document submitted"
4. Create `Notification` for all Approvers: "New document awaiting review"

**Vibe-codability: 🟡 Medium**  
The multi-table insert (Document + DocumentVersion + AuditLog + Notifications in one request) is where AI sometimes misses a step. Prompt it explicitly: "In a single transaction, create the document, create version 1, write an audit log entry, and notify all Approvers."

---

#### [DOC-2] Get My Documents API (Employee)

```
GET /api/documents/my
```
Returns documents submitted by the authenticated employee, with current status and latest version number.

**Vibe-codability: 🟢 Easy**

---

#### [DOC-3] Resubmit After Changes Requested

**Endpoint:** `PUT /api/documents/{id}/resubmit` *(Employee only, only when status = ChangesRequested)*

**What happens:**
1. Validate status is `ChangesRequested`
2. Increment `Document.CurrentVersion`
3. Create new `DocumentVersion` record
4. Reset `Document.Status = PendingApproval`
5. Write `AuditLog` entry
6. Notify Approvers

**Vibe-codability: 🟡 Medium**  
The status validation ("can only resubmit if ChangesRequested") is easy to forget. Tell AI explicitly to enforce this guard.

---

#### [DOC-4] Employee Dashboard UI

- Table/list of submitted documents
- Color-coded status badges (Draft=gray, Pending=yellow, UnderReview=blue, Approved=green, Rejected=red, ChangesRequested=orange)
- Click a document → Document Detail page
- "New Document" button

**Vibe-codability: 🟢 Easy**

---

#### [DOC-5] Document Detail Page (Employee View)

- Shows current status, all versions, full comment/action history (from `ApprovalAction`)
- If status is `ChangesRequested`: show a "Revise & Resubmit" form
- Timeline-style display of approval history

**Vibe-codability: 🟡 Medium**  
The timeline component and the conditional resubmit form are slightly tricky but AI handles UI components well. Be specific about the data structure when prompting.

---

### ✅ EPIC 5: Approval Workflow (Approver)

---

#### [APPROVAL-1] Get Documents Pending Review API

```
GET /api/documents/pending
```
Returns documents with `Status = PendingApproval`, available to any Approver.

**Vibe-codability: 🟢 Easy**

---

#### [APPROVAL-2] Act on Document API

**Endpoint:** `POST /api/documents/{id}/action` *(Approver only)*

**Request body:**
```json
{
  "action": "Approved" | "Rejected" | "ChangesRequested",
  "comment": "string"
}
```

**What happens server-side:**
1. Validate document exists and status is `PendingApproval` or `UnderReview`
2. Create `ApprovalAction` record
3. Update `Document.Status` based on action
4. Write to `AuditLog`
5. Notify the document's submitter (Employee)

**Vibe-codability: 🟡 Medium**  
The status transition logic and notification step are where AI tends to skip steps. Use a `switch` on the action enum and be explicit in your prompt.

---

#### [APPROVAL-3] Approver Dashboard UI

- Split view: "Pending" | "Reviewed" tabs
- Document cards with title, type, submitter name, submission date
- Click → Document Detail page (Approver view)

**Vibe-codability: 🟢 Easy**

---

#### [APPROVAL-4] Document Detail Page (Approver View)

- Full document content + version history
- Comment thread (all past `ApprovalAction` entries)
- Action panel: buttons for Approve / Reject / Request Changes + comment textarea
- Disable actions if document not in actionable state

**Vibe-codability: 🟡 Medium**  
The disabled-state logic on buttons and the comment thread rendering are slightly nuanced. Clearly describe conditions to AI.

---

### 🛡️ EPIC 6: Admin Panel

---

#### [ADMIN-1] User Management API

```
GET    /api/admin/users
POST   /api/admin/users
PUT    /api/admin/users/{id}
DELETE /api/admin/users/{id}   ← soft delete (set IsActive = false)
```

**Vibe-codability: 🟢 Easy**

---

#### [ADMIN-2] View All Documents API (Admin)

```
GET /api/admin/documents?status=&type=&submittedBy=
```
Filterable list of all documents in the system.

**Vibe-codability: 🟢 Easy**

---

#### [ADMIN-3] Audit Log API

```
GET /api/admin/audit-logs?documentId=&userId=&from=&to=
```

**Vibe-codability: 🟢 Easy**

---

#### [ADMIN-4] Analytics / Reports API *(satisfies the "analytics view" requirement)*

```
GET /api/admin/analytics
```

**Returns:**
```json
{
  "totalDocuments": 120,
  "byStatus": {
    "Approved": 60,
    "Rejected": 15,
    "PendingApproval": 30,
    "ChangesRequested": 15
  },
  "byDocumentType": [...],
  "avgApprovalTimeHours": 18.4,
  "documentsThisMonth": 34
}
```

**Vibe-codability: 🟡 Medium**  
The aggregation queries in EF Core are the trickier part. AI handles `GroupBy` and `Count` but `avgApprovalTimeHours` (comparing timestamps) needs careful prompting.

---

#### [ADMIN-5] Admin Dashboard UI *(satisfies "dashboard by approval stage")*

- Stat cards: Total Documents, Pending, Approved this month, Avg approval time
- Bar or pie chart by status (use `recharts` — already works in React, no setup needed)
- Bar chart by document type
- Recent activity feed (last 10 audit log entries)

**Vibe-codability: 🟡 Medium**  
Recharts is easy to use but the data-wiring from API → chart format needs attention. The stat cards and activity feed are easy.

---

#### [ADMIN-6] Admin User Management UI

- Table of users with Role badge, Department, Active/Inactive status
- Modal for create/edit
- Deactivate button (not hard delete)

**Vibe-codability: 🟢 Easy**

---

### 🔔 EPIC 7: Notifications

---

#### [NOTIF-1] Notification API

```
GET  /api/notifications        ← current user's notifications
PUT  /api/notifications/{id}/read
PUT  /api/notifications/read-all
```

**Vibe-codability: 🟢 Easy**

---

#### [NOTIF-2] Notification Bell UI (all roles)

- Bell icon in navbar with unread count badge
- Dropdown listing recent notifications
- Click notification → navigate to relevant document

**Vibe-codability: 🟢 Easy**

---

### 🕐 EPIC 8: Version History

---

#### [VERSION-1] Get Document Version History API

```
GET /api/documents/{id}/versions
```
Returns all `DocumentVersion` records for a document.

**Vibe-codability: 🟢 Easy**

---

#### [VERSION-2] Version History UI

- Shown inside Document Detail page (all roles)
- List of versions: "Version 1 — submitted Jan 10 by John Doe — [view]"
- Clicking a version shows that version's content (read-only)

**Vibe-codability: 🟢 Easy**

---

## 📊 Vibe-Codability Summary

| Rating | What it means |
|---|---|
| 🟢 Easy | AI can generate 90%+ of this correctly on first prompt. Just review and wire up. |
| 🟡 Medium | AI gets the shape right but misses details. Review carefully, especially business logic. |
| 🔴 Hard | AI frequently gets this wrong in subtle ways. A human should write or closely review this. |

| Task | Rating |
|---|---|
| Project scaffolding (both sides) | 🟢 |
| JWT Middleware setup | 🔴 |
| All CRUD endpoints | 🟢 |
| Role-based auth attributes | 🟢 |
| Frontend auth context + protected routes | 🟢 |
| Document submission (multi-table transaction) | 🟡 |
| Approval action (status transitions + notifications) | 🟡 |
| Employee resubmission flow | 🟡 |
| Analytics aggregation queries | 🟡 |
| Admin dashboard charts | 🟡 |
| All basic UI tables/forms/modals | 🟢 |
| Document detail timeline/comment thread | 🟡 |
| Notifications bell + dropdown | 🟢 |
| Version history display | 🟢 |

**Rule of thumb for your team:** Let AI write all the 🟢 tasks freely. For 🟡 tasks, write the prompt carefully and review the result. For the 🔴 JWT task, have your most experienced person (or the whole team) work through it together with the docs open.

---

## 🗓️ Suggested Sprint Plan (3 sprints)

### Sprint 1 — Foundation (Week 1–2)
- [SETUP-1] through [SETUP-4] — project init + JWT
- [AUTH-1] through [AUTH-3] — login/register + protected routes
- [DOCTYPE-1], [DOCTYPE-2] — document types CRUD

### Sprint 2 — Core Workflow (Week 3–4)
- [DOC-1] through [DOC-5] — employee submission + resubmit + detail page
- [APPROVAL-1] through [APPROVAL-4] — approver dashboard + actions
- [NOTIF-1], [NOTIF-2] — notifications

### Sprint 3 — Admin + Polish (Week 5–6)
- [ADMIN-1] through [ADMIN-6] — full admin panel + analytics
- [VERSION-1], [VERSION-2] — version history
- End-to-end testing of full workflow
- UI polish: consistent status colors, loading states, error messages

---

## ✅ Mandatory Requirements Checklist

| Requirement | How it's met |
|---|---|
| ≥ 3 user roles | ✅ Admin, Employee, Approver |
| JWT auth + authorization | ✅ Epic 2 |
| Role-based dashboards | ✅ Separate dashboards per role |
| ≥ 5 entities with relationships | ✅ User, Document, DocumentType, DocumentVersion, ApprovalAction, Notification, AuditLog (7) |
| Full CRUD | ✅ Users, DocumentTypes, Documents |
| Complex workflow | ✅ Multi-step approval with status machine + versioning |
| Analytics/report view | ✅ Admin analytics dashboard |
