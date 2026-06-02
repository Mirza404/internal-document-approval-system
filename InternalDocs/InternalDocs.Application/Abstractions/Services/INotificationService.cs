using InternalDocs.Application.Common;
using InternalDocs.Application.Notifications;

namespace InternalDocs.Application.Abstractions.Services;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<ServiceResult<NotificationDto>> MarkAsReadAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken);

    Task NotifyUserAsync(
        Guid userId,
        string title,
        string message,
        string type,
        CancellationToken cancellationToken);

    Task NotifyUsersAsync(
        IEnumerable<Guid> userIds,
        string title,
        string message,
        string type,
        CancellationToken cancellationToken);
}
