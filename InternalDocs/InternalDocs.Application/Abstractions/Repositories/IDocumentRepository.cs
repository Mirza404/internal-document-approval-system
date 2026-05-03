using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Abstractions.Repositories;

public interface IDocumentRepository
{
    Task<List<Document>> GetAllAsync(CancellationToken cancellationToken);
    Task<List<Document>> GetByCreatedByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Document?> GetByIdAndCreatedByUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    void Add(Document document);
    void Remove(Document document);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
