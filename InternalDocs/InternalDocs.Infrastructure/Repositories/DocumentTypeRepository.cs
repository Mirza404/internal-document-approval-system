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
}
