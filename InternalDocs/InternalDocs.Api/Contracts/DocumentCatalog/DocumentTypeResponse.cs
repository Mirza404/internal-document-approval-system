using InternalDocs.Application.DocumentCatalog;

namespace InternalDocs.Api.Contracts.DocumentCatalog;

public sealed class DocumentTypeResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public bool RequiresApproval { get; init; }
    public DateTime CreatedAt { get; init; }

    public static DocumentTypeResponse FromDto(DocumentTypeDto documentType)
    {
        return new DocumentTypeResponse
        {
            Id = documentType.Id,
            Name = documentType.Name,
            Description = documentType.Description,
            CategoryId = documentType.CategoryId,
            CategoryName = documentType.CategoryName,
            RequiresApproval = documentType.RequiresApproval,
            CreatedAt = documentType.CreatedAt
        };
    }
}
