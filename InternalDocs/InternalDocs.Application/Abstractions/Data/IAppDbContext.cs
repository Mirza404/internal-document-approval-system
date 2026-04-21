using InternalDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InternalDocs.Application.Abstractions.Data;

/// <summary>
/// Abstraction over the EF Core <see cref="DbContext"/> so the Application layer can
/// depend on a contract instead of the infrastructure implementation.
/// </summary>
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
