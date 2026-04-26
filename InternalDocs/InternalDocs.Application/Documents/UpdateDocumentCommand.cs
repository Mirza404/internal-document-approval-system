namespace InternalDocs.Application.Documents;

public sealed record UpdateDocumentCommand(
    string? Title,
    string? Description,
    Guid? DocumentTypeId,
    Guid? CreatedByUserId,
    string? Status,
    string? Priority,
    DateTime? ApprovedAt);
