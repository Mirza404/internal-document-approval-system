namespace InternalDocs.Api.Contracts.DocumentCatalog;

public sealed class UpdateDocumentTypeRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? RequiresApproval { get; set; }
}
