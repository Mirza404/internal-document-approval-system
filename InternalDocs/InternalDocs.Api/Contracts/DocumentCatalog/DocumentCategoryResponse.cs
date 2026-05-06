using InternalDocs.Application.DocumentCatalog;

namespace InternalDocs.Api.Contracts.DocumentCatalog;

public sealed class DocumentCategoryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }

    public static DocumentCategoryResponse FromDto(DocumentCategoryDto category)
    {
        return new DocumentCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt
        };
    }
}
