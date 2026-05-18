using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Domain.Entities;
using InternalDocs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternalDocs.Infrastructure.Repositories;

public sealed class DocumentRepository(AppDbContext dbContext) : IDocumentRepository
{
    public Task<List<Document>> GetAllAsync(CancellationToken cancellationToken)
    {
        return dbContext.Documents
            .AsNoTracking()
            .Include(x => x.Versions)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Documents
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Add(Document document)
    {
        dbContext.Documents.Add(document);
    }

    public void Remove(Document document)
    {
        dbContext.Documents.Remove(document);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
