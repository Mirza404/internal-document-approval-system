namespace InternalDocs.Application.Approvals;

public sealed record ApprovalDecisionCommand(
    Guid DocumentId,
    Guid ApproverId,
    string? Comments);