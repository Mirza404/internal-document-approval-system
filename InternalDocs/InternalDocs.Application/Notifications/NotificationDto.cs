using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Notifications;

public sealed record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt)
{
    public static NotificationDto FromEntity(Notification notification)
    {
        return new NotificationDto(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Type,
            notification.IsRead,
            notification.CreatedAt);
    }
}