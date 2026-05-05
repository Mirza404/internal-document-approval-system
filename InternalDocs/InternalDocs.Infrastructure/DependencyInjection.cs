using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Infrastructure.Auth;
using InternalDocs.Infrastructure.Data;
using InternalDocs.Infrastructure.Repositories;
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

        // Auth services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();

        // Named HttpClient for Microsoft Graph calls
        services.AddHttpClient("MicrosoftGraph", client =>
        {
            client.BaseAddress = new Uri("https://graph.microsoft.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}
