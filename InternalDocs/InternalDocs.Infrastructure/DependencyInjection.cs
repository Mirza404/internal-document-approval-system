using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Approvals;
using InternalDocs.Application.DocumentCatalog;
using InternalDocs.Application.Documents;
using InternalDocs.Application.Notifications;
using InternalDocs.Application.Users;
using InternalDocs.Infrastructure.Auth;
using InternalDocs.Infrastructure.Data;
using InternalDocs.Infrastructure.Repositories;
using InternalDocs.Infrastructure.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InternalDocs.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IApprovalActionRepository, ApprovalActionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDocumentTypeRepository, DocumentTypeRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Auth services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();

        // Application services
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentCatalogService, DocumentCatalogService>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Admin user management
        services.AddScoped<IAdminUserService, AdminUserService>();

        // Named HttpClient for Microsoft Graph calls
        services.AddHttpClient("MicrosoftGraph", client =>
        {
            client.BaseAddress = new Uri("https://graph.microsoft.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
        await DatabaseSeeder.SeedLocalUsersAsync(dbContext);
        await DatabaseSeeder.SeedDocumentTypesAsync(dbContext);

        if (string.Equals(
            Environment.GetEnvironmentVariable("SEED_DEMO_DATA"),
            "true",
            StringComparison.OrdinalIgnoreCase))
        {
            await DatabaseSeeder.SeedDemoDataAsync(dbContext);
        }
    }
}
