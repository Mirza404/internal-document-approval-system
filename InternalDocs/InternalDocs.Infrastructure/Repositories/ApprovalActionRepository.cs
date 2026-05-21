using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Domain.Entities;
using InternalDocs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternalDocs.Infrastructure.Repositories;

public sealed class ApprovalActionRepository(AppDbContext dbContext) : IApprovalActionRepository
{
    public Task<List<ApprovalAction>> GetAllAsync(CancellationToken cancellationToken)
    {
        return dbContext.ApprovalActions
            .AsNoTracking()
            .Include(x => x.ApprovedByUser)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<ApprovalAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.ApprovalActions
            .Include(x => x.ApprovedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<ApprovalAction>> GetByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        return dbContext.ApprovalActions
            .AsNoTracking()
            .Include(x => x.ApprovedByUser)
            .Where(x => x.DocumentId == documentId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Add(ApprovalAction approvalAction)
    {
        dbContext.ApprovalActions.Add(approvalAction);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
