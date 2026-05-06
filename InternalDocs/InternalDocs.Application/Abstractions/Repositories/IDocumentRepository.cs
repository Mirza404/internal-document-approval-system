using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Abstractions.Repositories;

public interface IDocumentRepository
{
    Task<List<Document>> GetAllAsync(CancellationToken cancellationToken);
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Document>> GetPendingApprovalQueueAsync(CancellationToken cancellationToken);
    void Add(Document document);
    void Remove(Document document);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
