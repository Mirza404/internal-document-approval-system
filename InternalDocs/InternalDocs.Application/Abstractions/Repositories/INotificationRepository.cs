using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Abstractions.Repositories;

public interface INotificationRepository
{
    Task<List<Notification>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<Notification?> GetByIdAndUserIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken);

    void Add(Notification notification);

    void AddRange(IEnumerable<Notification> notifications);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}