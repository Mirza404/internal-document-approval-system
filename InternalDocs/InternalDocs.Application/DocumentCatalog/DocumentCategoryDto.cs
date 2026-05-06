using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.DocumentCatalog;

public sealed record DocumentCategoryDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt)
{
    public static DocumentCategoryDto FromEntity(DocumentCategory category)
    {
        return new DocumentCategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.CreatedAt);
    }
}
