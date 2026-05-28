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
}
