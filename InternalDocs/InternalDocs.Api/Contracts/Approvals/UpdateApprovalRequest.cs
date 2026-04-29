namespace InternalDocs.Api.Contracts.Approvals;

public sealed class UpdateApprovalRequest
{
    public string? Status { get; set; }
    public string? Comments { get; set; }
}