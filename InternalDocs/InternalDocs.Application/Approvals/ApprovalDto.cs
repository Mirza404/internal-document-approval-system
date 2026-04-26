using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Approvals;

public sealed record ApprovalDto(
    Guid Id,
    Guid DocumentId,
    Guid ApproverId,
    string Status,
    string? Comments,
    DateTime CreatedAt)
{
    public static ApprovalDto FromEntity(ApprovalAction approvalAction)
    {
        return new ApprovalDto(
            approvalAction.Id,
            approvalAction.DocumentId,
            approvalAction.ApprovedByUserId,
            approvalAction.Action.ToString().ToLowerInvariant(),
            approvalAction.Comments,
            approvalAction.CreatedAt);
    }
}
