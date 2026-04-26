namespace InternalDocs.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
}
