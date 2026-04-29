using InternalDocs.Application.Approvals;

namespace InternalDocs.Api.Contracts.Approvals;

public sealed class ApprovalResponse
{
    public Guid Id { get; init; }
    public Guid DocumentId { get; init; }
    public Guid ApproverId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Comments { get; init; }
    public DateTime CreatedAt { get; init; }

    public static ApprovalResponse FromDto(ApprovalDto approvalAction)
    {
        return new ApprovalResponse
        {
            Id = approvalAction.Id,
            DocumentId = approvalAction.DocumentId,
            ApproverId = approvalAction.ApproverId,
            Status = approvalAction.Status,
            Comments = approvalAction.Comments,
            CreatedAt = approvalAction.CreatedAt
        };
    }
}
