using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Abstractions.Repositories;

public interface IDocumentTypeRepository
{
    Task<IReadOnlyList<DocumentCategory>> GetCategoriesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentType>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task<DocumentCategory?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<DocumentType?> GetDocumentTypeByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> CategoryHasDocumentTypesAsync(Guid categoryId, CancellationToken cancellationToken);

    Task<bool> DocumentTypeHasDocumentsAsync(Guid documentTypeId, CancellationToken cancellationToken);

    void AddCategory(DocumentCategory category);

    void AddDocumentType(DocumentType documentType);

    void RemoveCategory(DocumentCategory category);

    void RemoveDocumentType(DocumentType documentType);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
