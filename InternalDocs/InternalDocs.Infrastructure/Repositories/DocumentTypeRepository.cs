using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternalDocs.Infrastructure.Repositories;

public sealed class DocumentTypeRepository(AppDbContext dbContext) : IDocumentTypeRepository
{
    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.DocumentTypes.AnyAsync(x => x.Id == id, cancellationToken);
    }
}
