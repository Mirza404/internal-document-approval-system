using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternalDocs.Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Users.AnyAsync(x => x.Id == id, cancellationToken);
    }
}
