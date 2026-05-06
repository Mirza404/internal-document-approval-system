namespace InternalDocs.Api.Contracts.DocumentCatalog;

public sealed class CreateDocumentTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public bool RequiresApproval { get; set; }
}
