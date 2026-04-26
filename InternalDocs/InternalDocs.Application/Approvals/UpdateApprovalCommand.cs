namespace InternalDocs.Application.Approvals;

public sealed record UpdateApprovalCommand(
    string? Status,
    string? Comments);
