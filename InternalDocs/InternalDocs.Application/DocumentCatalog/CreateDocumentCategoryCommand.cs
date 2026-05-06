namespace InternalDocs.Application.DocumentCatalog;

public sealed record CreateDocumentCategoryCommand(
    string Name,
    string? Description);
