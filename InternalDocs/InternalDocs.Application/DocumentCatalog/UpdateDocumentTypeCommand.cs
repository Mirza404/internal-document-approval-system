namespace InternalDocs.Application.DocumentCatalog;

public sealed record UpdateDocumentTypeCommand(
    string? Name,
    string? Description,
    Guid? CategoryId,
    bool? RequiresApproval);
