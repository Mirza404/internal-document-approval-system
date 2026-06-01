namespace InternalDocs.Tests;

using System.Net;
using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Auth;
using InternalDocs.Application.Common;
using InternalDocs.Domain.Entities;
using InternalDocs.Infrastructure.Auth;
using Xunit;
using BC = BCrypt.Net.BCrypt;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task MicrosoftRegisterAsync_CreatesEmployeeRole()
    {
        var repository = new FakeUserRepository();
        var service = CreateService(
            repository,
            """
            {
              "id": "microsoft-user-id",
              "displayName": "OAuth User",
              "mail": "oauth.user@ius.edu.ba"
            }
            """);

        var result = await service.MicrosoftRegisterAsync(
            new MicrosoftRegisterCommand("microsoft-access-token"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(repository.CreatedUser);
        Assert.Equal("Employee", repository.CreatedUser.Role);
        Assert.Equal("Employee", result.Value?.Role);
    }

    [Fact]
    public async Task MicrosoftLoginAsync_RejectsInactiveUsers()
    {
        var repository = new FakeUserRepository
        {
            UserByMicrosoftObjectId = new User
            {
                Id = Guid.NewGuid(),
                Email = "inactive@ius.edu.ba",
                FullName = "Inactive User",
                MicrosoftObjectId = "microsoft-user-id",
                Role = "Employee",
                IsActive = false
            }
        };
        var tokenService = new FakeTokenService();
        var service = CreateService(
            repository,
            """
            {
              "id": "microsoft-user-id",
              "displayName": "Inactive User",
              "mail": "inactive@ius.edu.ba"
            }
            """,
            tokenService);

        var result = await service.MicrosoftLoginAsync(
            new MicrosoftLoginCommand("microsoft-access-token"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Equal(0, tokenService.GenerateJwtCallCount);
    }

    [Fact]
    public async Task LocalLoginAsync_AllowsAdminWithValidPassword()
    {
        var repository = new FakeUserRepository
        {
            UserByEmail = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@internaldocs.local",
                FullName = "System Administrator",
                PasswordHash = BC.HashPassword("AdminPass123!"),
                Role = "Admin",
                IsActive = true
            }
        };
        var tokenService = new FakeTokenService();
        var service = CreateService(repository, "{}", tokenService);

        var result = await service.LocalLoginAsync(
            new LocalLoginCommand("admin@internaldocs.local", "AdminPass123!"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal("Admin", result.Value?.Role);
        Assert.Equal(1, tokenService.GenerateJwtCallCount);
    }

    private static AuthService CreateService(
        FakeUserRepository repository,
        string graphResponse,
        FakeTokenService? tokenService = null)
    {
        return new AuthService(
            repository,
            tokenService ?? new FakeTokenService(),
            new FakeHttpClientFactory(graphResponse));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? CreatedUser { get; private set; }
        public User? UserByEmail { get; init; }
        public User? UserByMicrosoftObjectId { get; init; }

        public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<User>>(new List<User>());
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return Task.FromResult(UserByEmail?.Email == email ? UserByEmail : null);
        }

        public Task<User?> FindByMicrosoftObjectIdAsync(string microsoftObjectId, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                UserByMicrosoftObjectId?.MicrosoftObjectId == microsoftObjectId
                    ? UserByMicrosoftObjectId
                    : null);
        }

        public Task<User> CreateAsync(User user, CancellationToken cancellationToken)
        {
            CreatedUser = user;
            return Task.FromResult(user);
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTokenService : ITokenService
    {
        public int GenerateJwtCallCount { get; private set; }

        public string GenerateJwt(User user)
        {
            GenerateJwtCallCount++;
            return "test-jwt";
        }
    }

    private sealed class FakeHttpClientFactory(string responseBody) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(new FakeHttpMessageHandler(responseBody));
        }
    }

    private sealed class FakeHttpMessageHandler(string responseBody) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody)
            };

            return Task.FromResult(response);
        }
    }
}
