using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Abstractions.Services;

namespace InternalDocs.Application.DocumentCatalog;

public sealed class DocumentCatalogService(IDocumentTypeRepository documentTypes) : IDocumentCatalogService
{
    public async Task<IReadOnlyList<DocumentCategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var categories = await documentTypes.GetCategoriesAsync(cancellationToken);
        return categories.Select(DocumentCategoryDto.FromEntity).ToList();
    }

    public async Task<IReadOnlyList<DocumentTypeDto>> GetDocumentTypesAsync(
        CancellationToken cancellationToken)
    {
        var result = await documentTypes.GetAllAsync(cancellationToken);
        return result.Select(DocumentTypeDto.FromEntity).ToList();
    }
}
