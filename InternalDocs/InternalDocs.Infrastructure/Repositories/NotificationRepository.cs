using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Domain.Entities;
using InternalDocs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternalDocs.Infrastructure.Repositories;

public sealed class NotificationRepository(AppDbContext dbContext)
    : INotificationRepository
{
    public Task<List<Notification>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Notification?> GetByIdAndUserIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return dbContext.Notifications
            .FirstOrDefaultAsync(
                notification =>
                    notification.Id == id && notification.UserId == userId,
                cancellationToken);
    }

    public void Add(Notification notification)
    {
        dbContext.Notifications.Add(notification);
    }

    public void AddRange(IEnumerable<Notification> notifications)
    {
        dbContext.Notifications.AddRange(notifications);
    }

    public async Task MarkAllAsReadAsync(
        Guid userId,
        DateTime readAt,
        CancellationToken cancellationToken)
    {
        await dbContext.Notifications
            .Where(notification => notification.UserId == userId && !notification.IsRead)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(notification => notification.IsRead, true)
                    .SetProperty(notification => notification.ReadAt, readAt),
                cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
