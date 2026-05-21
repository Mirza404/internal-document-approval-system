namespace InternalDocs.Tests;

using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Common;
using InternalDocs.Application.Users;
using InternalDocs.Domain.Entities;
using Xunit;

public sealed class AdminUserServiceTests
{
    [Fact]
    public async Task UpdateRoleAsync_RejectsNonIusEmails()
    {
        var repository = new FakeUserRepository
        {
            UserById = new User
            {
                Id = Guid.NewGuid(),
                Email = "external@example.com",
                FullName = "External User",
                Role = "Employee",
                IsActive = true
            }
        };
        var service = new AdminUserService(repository);

        var result = await service.UpdateRoleAsync(
            repository.UserById.Id,
            new UpdateUserRoleCommand("Approver"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
        Assert.Equal("Employee", repository.UserById.Role);
    }

    [Fact]
    public async Task UpdateRoleAsync_AllowsApproverPromotion()
    {
        var repository = new FakeUserRepository
        {
            UserById = new User
            {
                Id = Guid.NewGuid(),
                Email = "user@ius.edu.ba",
                FullName = "IUS User",
                Role = "Employee",
                IsActive = true
            }
        };
        var service = new AdminUserService(repository);

        var result = await service.UpdateRoleAsync(
            repository.UserById.Id,
            new UpdateUserRoleCommand("Approver"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal("Approver", result.Value?.Role);
    }

    [Fact]
    public async Task SetActiveAsync_TogglesUserState()
    {
        var repository = new FakeUserRepository
        {
            UserById = new User
            {
                Id = Guid.NewGuid(),
                Email = "user@ius.edu.ba",
                FullName = "IUS User",
                Role = "Employee",
                IsActive = true
            }
        };
        var service = new AdminUserService(repository);

        var result = await service.SetActiveAsync(
            repository.UserById.Id,
            new SetUserActiveCommand(false),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.False(repository.UserById.IsActive);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? UserById { get; set; }

        public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<User> users = UserById is null
                ? new List<User>()
                : new List<User> { UserById };
            return Task.FromResult(users);
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(UserById?.Id == id ? UserById : null);
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(UserById is not null && UserById.Id == id);
        }

        public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return Task.FromResult(UserById?.Email == email ? UserById : null);
        }

        public Task<User?> FindByMicrosoftObjectIdAsync(string microsoftObjectId, CancellationToken cancellationToken)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<User> CreateAsync(User user, CancellationToken cancellationToken)
        {
            UserById = user;
            return Task.FromResult(user);
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
