namespace InternalDocs.Application.Abstractions.Repositories;

public interface IDocumentTypeRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
}
