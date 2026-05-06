namespace InternalDocs.Application.DocumentCatalog;

public sealed record UpdateDocumentCategoryCommand(
    string? Name,
    string? Description);
