namespace InternalDocs.Application.Documents;

public sealed record CreateDocumentCommand(
    string Title,
    string? Description,
    Guid? DocumentTypeId,
    Guid CreatedByUserId,
    string? Priority);
