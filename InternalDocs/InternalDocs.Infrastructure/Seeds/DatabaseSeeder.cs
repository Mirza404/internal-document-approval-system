using InternalDocs.Domain.Entities;
using InternalDocs.Infrastructure.Data;
using BC = BCrypt.Net.BCrypt;

namespace InternalDocs.Infrastructure.Seeds;

public static class DatabaseSeeder
{
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
}
