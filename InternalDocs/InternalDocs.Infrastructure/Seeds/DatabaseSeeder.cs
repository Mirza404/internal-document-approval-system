using InternalDocs.Domain.Entities;
using InternalDocs.Infrastructure.Data;
using BC = BCrypt.Net.BCrypt;

namespace InternalDocs.Infrastructure.Seeds;

public static class DatabaseSeeder
{
    private static readonly DocumentCategory[] SeedCategories =
    [
        new()
        {
            Id = new Guid("11111111-1111-1111-1111-111111111111"),
            Name = "HR",
            Description = "People operations, onboarding, leave, and employment documents.",
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = new Guid("22222222-2222-2222-2222-222222222222"),
            Name = "Finance",
            Description = "Budgets, invoices, reimbursements, procurement, and financial approvals.",
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = new Guid("33333333-3333-3333-3333-333333333333"),
            Name = "Contract",
            Description = "Vendor, partner, service, and legal agreement documents.",
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = new Guid("44444444-4444-4444-4444-444444444444"),
            Name = "Generic",
            Description = "General internal documents that do not need a specialized category.",
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        }
    ];

    private static readonly DocumentType[] SeedDocumentTypes =
    [
        new()
        {
            Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Name = "Leave Request",
            Description = "Employee leave, vacation, and time-off requests.",
            CategoryId = new Guid("11111111-1111-1111-1111-111111111111"),
            RequiresApproval = true,
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Name = "Expense Reimbursement",
            Description = "Employee reimbursement requests and supporting finance documents.",
            CategoryId = new Guid("22222222-2222-2222-2222-222222222222"),
            RequiresApproval = true,
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Name = "Vendor Contract",
            Description = "Vendor, partner, and service agreement submissions.",
            CategoryId = new Guid("33333333-3333-3333-3333-333333333333"),
            RequiresApproval = true,
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            Name = "General Document",
            Description = "General internal document submissions.",
            CategoryId = new Guid("44444444-4444-4444-4444-444444444444"),
            RequiresApproval = true,
            CreatedAt = new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc)
        }
    ];

    public static async Task SeedAdminUserAsync(AppDbContext context)
    {
        // Check if admin user already exists
        if (context.Users.Any(u => u.Email == "admin@internaldocs.local"))
        {
            return;
        }

        // Get password from environment variable
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
            ?? throw new InvalidOperationException("ADMIN_PASSWORD environment variable is not set");

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@internaldocs.local",
            FullName = "System Administrator",
            PasswordHash = BC.HashPassword(adminPassword),
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }

    public static async Task SeedDocumentCategoriesAsync(AppDbContext context)
    {
        foreach (var category in SeedCategories)
        {
            if (context.DocumentCategories.Any(x => x.Id == category.Id || x.Name == category.Name))
            {
                continue;
            }

            context.DocumentCategories.Add(category);
        }

        await context.SaveChangesAsync();
    }

    public static async Task SeedDocumentTypesAsync(AppDbContext context)
    {
        await SeedDocumentCategoriesAsync(context);

        foreach (var documentType in SeedDocumentTypes)
        {
            if (context.DocumentTypes.Any(x =>
                    x.Id == documentType.Id
                    || (x.Name == documentType.Name && x.CategoryId == documentType.CategoryId)))
            {
                continue;
            }

            context.DocumentTypes.Add(documentType);
        }

        await context.SaveChangesAsync();
    }
}
