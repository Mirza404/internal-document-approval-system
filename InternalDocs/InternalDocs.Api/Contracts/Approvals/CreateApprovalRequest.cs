namespace InternalDocs.Api.Contracts.Approvals;

public sealed class CreateApprovalRequest
{
    public Guid DocumentId { get; set; }
    public Guid ApproverId { get; set; }
    public string? Status { get; set; }
    public string? Comments { get; set; }
}