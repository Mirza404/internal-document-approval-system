using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> FindByMicrosoftObjectIdAsync(string microsoftObjectId, CancellationToken cancellationToken);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken);
    Task UpdateAsync(User user, CancellationToken cancellationToken);
}
