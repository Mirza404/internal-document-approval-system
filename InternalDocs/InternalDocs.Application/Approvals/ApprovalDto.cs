using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Approvals;

public sealed record ApprovalDto(
    Guid Id,
    Guid DocumentId,
    Guid ApproverId,
    string ApproverFullName,
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
            approvalAction.ApprovedByUser.FullName,
            approvalAction.Action.ToLowerInvariant(),
            approvalAction.Comments,
            approvalAction.CreatedAt);
    }
}
