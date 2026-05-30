using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Common;
using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Notifications;

public sealed class NotificationService(INotificationRepository notifications)
    : INotificationService
{
    public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await notifications.GetByUserIdAsync(
            userId,
            cancellationToken);

        return result.Select(NotificationDto.FromEntity).ToList();
    }

    public async Task<ServiceResult<NotificationDto>> MarkAsReadAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var notification = await notifications.GetByIdAndUserIdAsync(
            id,
            userId,
            cancellationToken);

        if (notification is null)
        {
            return ServiceResult<NotificationDto>.Failure(
                "Notification was not found.",
                ServiceErrorType.NotFound);
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await notifications.SaveChangesAsync(cancellationToken);

        return ServiceResult<NotificationDto>.Success(
            NotificationDto.FromEntity(notification));
    }

    public async Task NotifyUserAsync(
        Guid userId,
        string title,
        string message,
        string type,
        CancellationToken cancellationToken)
    {
        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await notifications.SaveChangesAsync(cancellationToken);
    }

    public async Task NotifyUsersAsync(
        IEnumerable<Guid> userIds,
        string title,
        string message,
        string type,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var createdNotifications = userIds
            .Distinct()
            .Select(userId => new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = now
            })
            .ToList();

        if (createdNotifications.Count == 0)
        {
            return;
        }

        notifications.AddRange(createdNotifications);
        await notifications.SaveChangesAsync(cancellationToken);
    }
}
