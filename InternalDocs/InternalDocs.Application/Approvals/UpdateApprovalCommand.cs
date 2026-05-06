namespace InternalDocs.Application.Approvals;

public sealed record UpdateApprovalCommand(
    Guid ApproverId,
    string? Status,
    string? Comments);
