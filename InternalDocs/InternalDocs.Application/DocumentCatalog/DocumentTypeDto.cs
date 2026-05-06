using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.DocumentCatalog;

public sealed record DocumentTypeDto(
    Guid Id,
    string Name,
    string Description,
    Guid CategoryId,
    string CategoryName,
    bool RequiresApproval,
    DateTime CreatedAt)
{
    public static DocumentTypeDto FromEntity(DocumentType documentType)
    {
        return new DocumentTypeDto(
            documentType.Id,
            documentType.Name,
            documentType.Description,
            documentType.CategoryId,
            documentType.Category.Name,
            documentType.RequiresApproval,
            documentType.CreatedAt);
    }
}
