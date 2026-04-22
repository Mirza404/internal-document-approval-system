using Microsoft.EntityFrameworkCore;
using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Document> Documents { get; }
    DbSet<DocumentType> DocumentTypes { get; }
    DbSet<DocumentVersion> DocumentVersions { get; }
    DbSet<ApprovalAction> ApprovalActions { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}