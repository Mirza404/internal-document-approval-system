using InternalDocs.Application.DocumentCatalog;

namespace InternalDocs.Application.Abstractions.Services;

public interface IDocumentCatalogService
{
    Task<IReadOnlyList<DocumentCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentTypeDto>> GetDocumentTypesAsync(CancellationToken cancellationToken);
}
