using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Abstractions.Repositories;

public interface IDocumentTypeRepository
{
    Task<IReadOnlyList<DocumentCategory>> GetCategoriesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentType>> GetAllAsync(CancellationToken cancellationToken);

    Task<DocumentType?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
}
