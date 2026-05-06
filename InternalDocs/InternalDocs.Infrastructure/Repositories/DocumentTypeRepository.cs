using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Domain.Entities;
using InternalDocs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternalDocs.Infrastructure.Repositories;

public sealed class DocumentTypeRepository(AppDbContext dbContext) : IDocumentTypeRepository
{
    public async Task<IReadOnlyList<DocumentCategory>> GetCategoriesAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.DocumentCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentType>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.DocumentTypes
            .AsNoTracking()
            .Include(x => x.Category)
            .OrderBy(x => x.Category.Name)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.DocumentTypes.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public Task<DocumentCategory?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.DocumentCategories
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<DocumentType?> GetDocumentTypeByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.DocumentTypes
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> CategoryHasDocumentTypesAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        return dbContext.DocumentTypes
            .AnyAsync(x => x.CategoryId == categoryId, cancellationToken);
    }

    public Task<bool> DocumentTypeHasDocumentsAsync(Guid documentTypeId, CancellationToken cancellationToken)
    {
        return dbContext.Documents
            .AnyAsync(x => x.DocumentTypeId == documentTypeId, cancellationToken);
    }

    public void AddCategory(DocumentCategory category)
    {
        dbContext.DocumentCategories.Add(category);
    }

    public void AddDocumentType(DocumentType documentType)
    {
        dbContext.DocumentTypes.Add(documentType);
    }

    public void RemoveCategory(DocumentCategory category)
    {
        dbContext.DocumentCategories.Remove(category);
    }

    public void RemoveDocumentType(DocumentType documentType)
    {
        dbContext.DocumentTypes.Remove(documentType);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
