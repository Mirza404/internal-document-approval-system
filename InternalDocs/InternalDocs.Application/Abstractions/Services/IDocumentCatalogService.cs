using InternalDocs.Application.Common;
using InternalDocs.Application.DocumentCatalog;

namespace InternalDocs.Application.Abstractions.Services;

public interface IDocumentCatalogService
{
    Task<IReadOnlyList<DocumentCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentTypeDto>> GetDocumentTypesAsync(CancellationToken cancellationToken);

    Task<ServiceResult<DocumentTypeDto>> GetDocumentTypeByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<ServiceResult<DocumentCategoryDto>> CreateCategoryAsync(
        CreateDocumentCategoryCommand command,
        CancellationToken cancellationToken);

    Task<ServiceResult<DocumentCategoryDto>> UpdateCategoryAsync(
        Guid id,
        UpdateDocumentCategoryCommand command,
        CancellationToken cancellationToken);

    Task<ServiceResult> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken);

    Task<ServiceResult<DocumentTypeDto>> CreateDocumentTypeAsync(
        CreateDocumentTypeCommand command,
        CancellationToken cancellationToken);

    Task<ServiceResult<DocumentTypeDto>> UpdateDocumentTypeAsync(
        Guid id,
        UpdateDocumentTypeCommand command,
        CancellationToken cancellationToken);

    Task<ServiceResult> DeleteDocumentTypeAsync(Guid id, CancellationToken cancellationToken);
}
