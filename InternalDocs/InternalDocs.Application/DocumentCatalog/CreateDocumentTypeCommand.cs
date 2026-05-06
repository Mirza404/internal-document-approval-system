namespace InternalDocs.Application.DocumentCatalog;

public sealed record CreateDocumentTypeCommand(
    string Name,
    string? Description,
    Guid CategoryId,
    bool RequiresApproval);
