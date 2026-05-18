using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Approvals;

public sealed record PendingApprovalItemDto(
    Guid DocumentId,
    string Title,
    Guid DocumentTypeId,
    string DocumentTypeName,
    Guid CreatorId,
    string CreatorFullName,
    DateTime CreatedAt,
    string Status)
{
    public static PendingApprovalItemDto FromEntity(Document document)
    {
        return new PendingApprovalItemDto(
            document.Id,
            document.Title,
            document.DocumentTypeId,
            document.DocumentType.Name,
            document.CreatedByUserId,
            document.CreatedByUser.FullName,
            document.CreatedAt,
            document.Status);
    }
}
