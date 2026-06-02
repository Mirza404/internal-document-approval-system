using InternalDocs.Api.Authentication;
using InternalDocs.Api.Contracts.Notifications;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Common;
using InternalDocs.Application.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternalDocs.Api.Controllers;

[ApiController]
[Authorize]
[Route("notifications")]
public sealed class NotificationsController(
    INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NotificationResponse>>> GetMy(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var notifications = await notificationService.GetForUserAsync(
            userId.Value,
            cancellationToken);

        return Ok(notifications.Select(NotificationResponse.FromDto).ToList());
    }

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponse>> MarkAsRead(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await notificationService.MarkAsReadAsync(
            id,
            userId.Value,
            cancellationToken);

        if (result.Succeeded && result.Value is not null)
        {
            return Ok(NotificationResponse.FromDto(result.Value));
        }

        return result.ErrorType == ServiceErrorType.NotFound
            ? NotFound(result.Error)
            : BadRequest(result.Error);
    }

    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> MarkAllAsRead(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        await notificationService.MarkAllAsReadAsync(
            userId.Value,
            cancellationToken);

        return NoContent();
    }
}
