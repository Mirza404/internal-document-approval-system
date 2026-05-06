namespace InternalDocs.Api.Contracts.Documents;

public sealed class CreateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public string? Priority { get; set; }
}
