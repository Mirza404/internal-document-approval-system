using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Abstractions.Repositories;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetBySubmitterAsync(Guid submitterId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetPendingForApprovalAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetFilteredAsync(DocumentStatusFilter filter, CancellationToken cancellationToken = default);
    Task AddAsync(Document document, CancellationToken cancellationToken = default);
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record DocumentStatusFilter(
    string? Status,
    Guid? DocumentTypeId,
    Guid? SubmittedById,
    bool IncludeNavigationData = true);
