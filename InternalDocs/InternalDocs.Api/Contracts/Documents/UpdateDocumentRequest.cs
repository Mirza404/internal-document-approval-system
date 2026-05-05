namespace InternalDocs.Api.Contracts.Documents;

public sealed class UpdateDocumentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
