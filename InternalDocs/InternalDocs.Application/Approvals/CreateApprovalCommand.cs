namespace InternalDocs.Application.Approvals;

public sealed record CreateApprovalCommand(
    Guid DocumentId,
    Guid ApproverId,
    string? Status,
    string? Comments);
