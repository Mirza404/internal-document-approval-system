namespace InternalDocs.Api.Contracts.DocumentCatalog;

public sealed class CreateDocumentCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
