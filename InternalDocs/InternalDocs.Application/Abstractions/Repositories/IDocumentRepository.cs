using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Abstractions.Repositories;

/// <summary>
/// Contract for document aggregate queries and commands.
/// </summary>
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

/// <summary>
/// Parameter object allowing future filtering scenarios to grow without breaking signatures.
/// </summary>
public sealed record DocumentStatusFilter(
    string? Status,
    Guid? DocumentTypeId,
    Guid? SubmittedById,
    bool IncludeNavigationData = true);
