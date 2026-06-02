using System.Text.Json;
using InternalDocs.Domain.Entities;
using InternalDocs.Infrastructure.Data;
using BC = BCrypt.Net.BCrypt;

namespace InternalDocs.Infrastructure.Seeds;

public static class DatabaseSeeder
{
    private const string DefaultAdminEmail = "admin@internaldocs.local";
    private const string DefaultAdminPassword = "AdminPass123!";
    private const string DefaultEmployeeEmail = "employee@internaldocs.local";
    private const string DefaultEmployeePassword = "EmployeePass123!";
    private const string DefaultApproverEmail = "approver@internaldocs.local";
    private const string DefaultApproverPassword = "ApproverPass123!";
    private const string DefaultReviewerEmail = "reviewer@internaldocs.local";
    private const string DefaultReviewerPassword = "ReviewerPass123!";
    private const string DefaultSecondEmployeeEmail = "sara.hadzic@internaldocs.local";
    private const string DefaultSecondEmployeePassword = "DemoPass123!";

    private static readonly DocumentCategory[] SeedCategories =
    [
        new()
        {
            Id = new Guid("11111111-1111-1111-1111-111111111111"),
            Name = "Academic Records",
            Description = "Official student record requests, including transcript processing.",
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = new Guid("22222222-2222-2222-2222-222222222222"),
            Name = "Student Services",
            Description = "Student certificate and administrative service requests.",
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = new Guid("33333333-3333-3333-3333-333333333333"),
            Name = "Internships",
            Description = "Internship submissions, placements, and supporting documents.",
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = new Guid("44444444-4444-4444-4444-444444444444"),
            Name = "Payments",
            Description = "Student payment procedure and finance office requests.",
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = new Guid("55555555-5555-5555-5555-555555555555"),
            Name = "Human Resources",
            Description = "Employee leave, absence, and workplace administration requests.",
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        }
    ];

    private static readonly DocumentType[] SeedDocumentTypes =
    [
        new()
        {
            Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Name = "Transcript",
            Description = "Official academic transcript request.",
            CategoryId = new Guid("11111111-1111-1111-1111-111111111111"),
            RequiresApproval = true,
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Name = "Certificate",
            Description = "Enrollment, status, and other student certificate requests.",
            CategoryId = new Guid("22222222-2222-2222-2222-222222222222"),
            RequiresApproval = true,
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Name = "Internship Submission",
            Description = "Internship approval and supporting document submission.",
            CategoryId = new Guid("33333333-3333-3333-3333-333333333333"),
            RequiresApproval = true,
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            Name = "Payment Procedure",
            Description = "Payment procedure request requiring amount and student finance reference.",
            CategoryId = new Guid("44444444-4444-4444-4444-444444444444"),
            RequiresApproval = true,
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            Name = "Leave Request",
            Description = "Employee leave request requiring leave type and date range.",
            CategoryId = new Guid("55555555-5555-5555-5555-555555555555"),
            RequiresApproval = true,
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        }
    ];

    public static async Task SeedLocalUsersAsync(AppDbContext context)
    {
        await SeedLocalUserAsync(
            context,
            Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? DefaultAdminEmail,
            Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? DefaultAdminPassword,
            "System Administrator",
            "Admin");

        await SeedLocalUserAsync(
            context,
            Environment.GetEnvironmentVariable("EMPLOYEE_EMAIL") ?? DefaultEmployeeEmail,
            Environment.GetEnvironmentVariable("EMPLOYEE_PASSWORD") ?? DefaultEmployeePassword,
            "Demo Employee",
            "Employee");
    }

    private static async Task SeedLocalUserAsync(
        AppDbContext context,
        string email,
        string password,
        string fullName,
        string role)
    {
        var existingUser = context.Users.FirstOrDefault(u => u.Email == email);
        if (existingUser is not null)
        {
            existingUser.FullName = fullName;
            existingUser.PasswordHash = BC.HashPassword(password);
            existingUser.Role = role;
            existingUser.IsActive = true;
            existingUser.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = fullName,
            PasswordHash = BC.HashPassword(password),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    public static async Task SeedDocumentCategoriesAsync(AppDbContext context)
    {
        foreach (var seedCategory in SeedCategories)
        {
            var category = context.DocumentCategories.FirstOrDefault(x => x.Id == seedCategory.Id);
            if (category is null)
            {
                if (context.DocumentCategories.Any(x => x.Name == seedCategory.Name))
                {
                    continue;
                }

                context.DocumentCategories.Add(seedCategory);
                continue;
            }

            category.Name = seedCategory.Name;
            category.Description = seedCategory.Description;
        }

        await context.SaveChangesAsync();
    }

    public static async Task SeedDocumentTypesAsync(AppDbContext context)
    {
        await SeedDocumentCategoriesAsync(context);

        foreach (var seedDocumentType in SeedDocumentTypes)
        {
            var documentType = context.DocumentTypes.FirstOrDefault(x => x.Id == seedDocumentType.Id);
            if (documentType is null)
            {
                if (context.DocumentTypes.Any(x =>
                        x.Name == seedDocumentType.Name && x.CategoryId == seedDocumentType.CategoryId))
                {
                    continue;
                }

                context.DocumentTypes.Add(seedDocumentType);
                continue;
            }

            documentType.Name = seedDocumentType.Name;
            documentType.Description = seedDocumentType.Description;
            documentType.CategoryId = seedDocumentType.CategoryId;
            documentType.RequiresApproval = seedDocumentType.RequiresApproval;
        }

        await context.SaveChangesAsync();
    }

    public static async Task SeedDemoDataAsync(AppDbContext context)
    {
        await SeedLocalUserAsync(
            context,
            Environment.GetEnvironmentVariable("APPROVER_EMAIL") ?? DefaultApproverEmail,
            Environment.GetEnvironmentVariable("APPROVER_PASSWORD") ?? DefaultApproverPassword,
            "Demo Approver",
            "Approver");

        await SeedLocalUserAsync(
            context,
            DefaultReviewerEmail,
            DefaultReviewerPassword,
            "Lejla Reviewer",
            "Approver");

        await SeedLocalUserAsync(
            context,
            DefaultSecondEmployeeEmail,
            DefaultSecondEmployeePassword,
            "Sara Hadzic",
            "Employee");

        var employeeEmail = Environment.GetEnvironmentVariable("EMPLOYEE_EMAIL") ?? DefaultEmployeeEmail;
        var approverEmail = Environment.GetEnvironmentVariable("APPROVER_EMAIL") ?? DefaultApproverEmail;
        var employee = context.Users.First(x => x.Email == employeeEmail);
        var approver = context.Users.First(x => x.Email == approverEmail);
        var reviewer = context.Users.First(x => x.Email == DefaultReviewerEmail);
        var secondEmployee = context.Users.First(x => x.Email == DefaultSecondEmployeeEmail);
        var now = DateTime.UtcNow;

        var documents = new[]
        {
            CreateDemoDocument(
                "10000000-0000-0000-0000-000000000001",
                "Transcript request for graduate application",
                "Please issue an official transcript for a graduate program application.",
                "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                employee.Id,
                "PendingApproval",
                "High",
                now.AddDays(-1)),
            CreateDemoDocument(
                "10000000-0000-0000-0000-000000000002",
                "Spring semester payment procedure",
                "Requesting processing instructions for the spring semester payment.",
                "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
                employee.Id,
                "PendingApproval",
                "Normal",
                now.AddHours(-10),
                amount: 1250m,
                budgetCode: "PAY-2026-014"),
            CreateDemoDocument(
                "10000000-0000-0000-0000-000000000003",
                "Software engineering internship submission",
                "Internship placement documentation for review.",
                "cccccccc-cccc-cccc-cccc-cccccccccccc",
                employee.Id,
                "PendingApproval",
                "Normal",
                now.AddHours(-4),
                counterparty: "Northwind Labs",
                attachmentNote: "Signed internship agreement attached."),
            CreateDemoDocument(
                "10000000-0000-0000-0000-000000000004",
                "Enrollment certificate request",
                "Certificate required for a scholarship application.",
                "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
                employee.Id,
                "ChangesRequested",
                "High",
                now.AddDays(-5),
                updatedAt: now.AddDays(-3)),
            CreateDemoDocument(
                "10000000-0000-0000-0000-000000000005",
                "Official transcript for exchange program",
                "Transcript required for exchange program registration.",
                "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                employee.Id,
                "Approved",
                "Normal",
                now.AddDays(-12),
                updatedAt: now.AddDays(-10),
                approvedAt: now.AddDays(-10)),
            CreateDemoDocument(
                "10000000-0000-0000-0000-000000000006",
                "Late fee payment review",
                "Requesting review of a late fee payment procedure.",
                "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
                employee.Id,
                "Rejected",
                "Urgent",
                now.AddDays(-8),
                updatedAt: now.AddDays(-7),
                amount: 75m,
                budgetCode: "PAY-2026-009"),
            CreateDemoDocument(
                "10000000-0000-0000-0000-000000000007",
                "Student status certificate draft",
                "Draft request for a student status certificate.",
                "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
                employee.Id,
                "Draft",
                "Low",
                now.AddHours(-2)),
            CreateDemoDocument(
                "10000000-0000-0000-0000-000000000008",
                "Annual leave request for July",
                "Annual leave request for the first week of July.",
                "ffffffff-ffff-ffff-ffff-ffffffffffff",
                employee.Id,
                "PendingApproval",
                "Normal",
                now.AddHours(-6),
                leaveType: "Annual leave",
                leaveStartDate: new DateOnly(2026, 7, 6),
                leaveEndDate: new DateOnly(2026, 7, 10)),
            CreateDemoDocument(
                "10000000-0000-0000-0000-000000000009",
                "Conference registration payment",
                "Payment procedure for the annual engineering conference registration.",
                "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
                employee.Id,
                "Approved",
                "High",
                now.AddDays(-16),
                updatedAt: now.AddDays(-14),
                approvedAt: now.AddDays(-14),
                amount: 420m,
                budgetCode: "CONF-2026-031"),
            CreateDemoDocument(
                "10000000-0000-0000-0000-000000000010",
                "Summer internship placement update",
                "Updated internship placement submission for review.",
                "cccccccc-cccc-cccc-cccc-cccccccccccc",
                employee.Id,
                "ChangesRequested",
                "Normal",
                now.AddDays(-4),
                updatedAt: now.AddDays(-2),
                counterparty: "Contoso Engineering",
                attachmentNote: "Placement confirmation is attached."),
            CreateDemoDocument(
                "10000000-0000-0000-0000-000000000011",
                "Transcript request for mobility program",
                "Official transcript needed for mobility program enrollment.",
                "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                secondEmployee.Id,
                "PendingApproval",
                "High",
                now.AddHours(-8)),
            CreateDemoDocument(
                "10000000-0000-0000-0000-000000000012",
                "Medical leave request",
                "Medical leave request with supporting documentation.",
                "ffffffff-ffff-ffff-ffff-ffffffffffff",
                secondEmployee.Id,
                "PendingApproval",
                "Urgent",
                now.AddHours(-3),
                leaveType: "Medical leave",
                leaveStartDate: new DateOnly(2026, 6, 3),
                leaveEndDate: new DateOnly(2026, 6, 5)),
            CreateDemoDocument(
                "10000000-0000-0000-0000-000000000013",
                "Enrollment certificate for housing application",
                "Enrollment certificate required by the housing office.",
                "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
                secondEmployee.Id,
                "Approved",
                "Normal",
                now.AddDays(-9),
                updatedAt: now.AddDays(-8),
                approvedAt: now.AddDays(-8))
        };

        foreach (var document in documents)
        {
            if (context.Documents.Any(x => x.Id == document.Id))
            {
                continue;
            }

            context.Documents.Add(document);
        }

        await context.SaveChangesAsync();

        var approvals = new[]
        {
            CreateDemoApproval(
                "20000000-0000-0000-0000-000000000001",
                documents[3].Id,
                approver.Id,
                "ChangesRequested",
                "Please include the scholarship organization name.",
                now.AddDays(-3)),
            CreateDemoApproval(
                "20000000-0000-0000-0000-000000000002",
                documents[4].Id,
                approver.Id,
                "Approved",
                "Verified and approved.",
                now.AddDays(-10)),
            CreateDemoApproval(
                "20000000-0000-0000-0000-000000000003",
                documents[5].Id,
                approver.Id,
                "Rejected",
                "The payment reference does not match the submitted request.",
                now.AddDays(-7)),
            CreateDemoApproval(
                "20000000-0000-0000-0000-000000000004",
                documents[8].Id,
                reviewer.Id,
                "Approved",
                "Conference registration details verified.",
                now.AddDays(-14)),
            CreateDemoApproval(
                "20000000-0000-0000-0000-000000000005",
                documents[9].Id,
                reviewer.Id,
                "ChangesRequested",
                "Please attach the updated internship agreement.",
                now.AddDays(-2)),
            CreateDemoApproval(
                "20000000-0000-0000-0000-000000000006",
                documents[12].Id,
                reviewer.Id,
                "Approved",
                "Enrollment status verified.",
                now.AddDays(-8))
        };

        foreach (var approval in approvals)
        {
            if (context.ApprovalActions.Any(x => x.Id == approval.Id))
            {
                continue;
            }

            context.ApprovalActions.Add(approval);
        }

        await context.SaveChangesAsync();

        var notifications = new[]
        {
            CreateDemoNotification(
                "40000000-0000-0000-0000-000000000001",
                employee.Id,
                "Changes requested",
                "Your summer internship placement update needs a revised agreement.",
                "Warning",
                now.AddDays(-2)),
            CreateDemoNotification(
                "40000000-0000-0000-0000-000000000002",
                employee.Id,
                "Payment procedure approved",
                "Your conference registration payment procedure was approved.",
                "Success",
                now.AddDays(-14),
                isRead: true),
            CreateDemoNotification(
                "40000000-0000-0000-0000-000000000003",
                employee.Id,
                "Leave request submitted",
                "Your annual leave request is waiting for review.",
                "Info",
                now.AddHours(-6)),
            CreateDemoNotification(
                "40000000-0000-0000-0000-000000000004",
                approver.Id,
                "New urgent leave request",
                "Sara Hadzic submitted a medical leave request for review.",
                "Warning",
                now.AddHours(-3)),
            CreateDemoNotification(
                "40000000-0000-0000-0000-000000000005",
                approver.Id,
                "Approval queue updated",
                "New transcript and leave requests are ready for review.",
                "Info",
                now.AddHours(-8)),
            CreateDemoNotification(
                "40000000-0000-0000-0000-000000000006",
                secondEmployee.Id,
                "Certificate approved",
                "Your enrollment certificate for the housing application was approved.",
                "Success",
                now.AddDays(-8),
                isRead: true)
        };

        foreach (var notification in notifications)
        {
            if (context.Notifications.Any(x => x.Id == notification.Id))
            {
                continue;
            }

            context.Notifications.Add(notification);
        }

        await context.SaveChangesAsync();
    }

    private static Document CreateDemoDocument(
        string id,
        string title,
        string description,
        string documentTypeId,
        Guid employeeId,
        string status,
        string priority,
        DateTime createdAt,
        DateTime? updatedAt = null,
        DateTime? approvedAt = null,
        decimal? amount = null,
        string? budgetCode = null,
        string? counterparty = null,
        string? attachmentNote = null,
        string? leaveType = null,
        DateOnly? leaveStartDate = null,
        DateOnly? leaveEndDate = null)
    {
        var document = new Document
        {
            Id = new Guid(id),
            Title = title,
            Description = description,
            DocumentTypeId = new Guid(documentTypeId),
            CreatedByUserId = employeeId,
            Status = status,
            Priority = priority,
            Amount = amount,
            BudgetCode = budgetCode,
            Counterparty = counterparty,
            AttachmentNote = attachmentNote,
            LeaveType = leaveType,
            LeaveStartDate = leaveStartDate,
            LeaveEndDate = leaveEndDate,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            ApprovedAt = approvedAt
        };

        document.Versions.Add(new DocumentVersion
        {
            Id = new Guid(id.Replace("10000000", "30000000", StringComparison.Ordinal)),
            DocumentId = document.Id,
            VersionNumber = 1,
            MajorVersion = 1,
            MinorVersion = 0,
            Content = JsonSerializer.Serialize(new { title, description }),
            ChangeNotes = "Initial submission",
            CreatedAt = createdAt
        });

        return document;
    }

    private static ApprovalAction CreateDemoApproval(
        string id,
        Guid documentId,
        Guid approverId,
        string action,
        string comments,
        DateTime createdAt)
    {
        return new ApprovalAction
        {
            Id = new Guid(id),
            DocumentId = documentId,
            ApprovedByUserId = approverId,
            Action = action,
            Comments = comments,
            CreatedAt = createdAt
        };
    }

    private static Notification CreateDemoNotification(
        string id,
        Guid userId,
        string title,
        string message,
        string type,
        DateTime createdAt,
        bool isRead = false)
    {
        return new Notification
        {
            Id = new Guid(id),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = isRead,
            CreatedAt = createdAt,
            ReadAt = isRead ? createdAt.AddHours(1) : null
        };
    }
}
