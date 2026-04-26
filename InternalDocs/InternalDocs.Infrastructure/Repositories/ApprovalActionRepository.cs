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
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<ApprovalAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.ApprovalActions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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
