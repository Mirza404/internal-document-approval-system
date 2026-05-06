using InternalDocs.Application.Approvals;

namespace InternalDocs.Api.Contracts.Approvals;

public sealed class PendingApprovalResponse
{
    public Guid DocumentId { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid DocumentTypeId { get; init; }
    public string DocumentTypeName { get; init; } = string.Empty;
    public Guid CreatorId { get; init; }
    public string CreatorFullName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string Status { get; init; } = string.Empty;

    public static PendingApprovalResponse FromDto(PendingApprovalItemDto pending)
    {
        return new PendingApprovalResponse
        {
            DocumentId = pending.DocumentId,
            Title = pending.Title,
            DocumentTypeId = pending.DocumentTypeId,
            DocumentTypeName = pending.DocumentTypeName,
            CreatorId = pending.CreatorId,
            CreatorFullName = pending.CreatorFullName,
            CreatedAt = pending.CreatedAt,
            Status = pending.Status
        };
    }
}
