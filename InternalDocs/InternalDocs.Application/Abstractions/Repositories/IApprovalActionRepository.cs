using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Abstractions.Repositories;

public interface IApprovalActionRepository
{
    Task<List<ApprovalAction>> GetAllAsync(CancellationToken cancellationToken);

    Task<ApprovalAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<ApprovalAction>> GetByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken);

    void Add(ApprovalAction approvalAction);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
