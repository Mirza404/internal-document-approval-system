using InternalDocs.Api.Contracts.AdminUsers;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Common;
using InternalDocs.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternalDocs.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("admin/users")]
public sealed class AdminUsersController(IAdminUserService userService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<AdminUserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AdminUserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return Ok(users.Select(AdminUserResponse.FromDto).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await userService.GetByIdAsync(id, cancellationToken);
        return ToUserResponse(result);
    }

    [HttpPut("{id:guid}/role")]
    [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserResponse>> UpdateRole(
        Guid id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserRoleCommand(request.Role);
        var result = await userService.UpdateRoleAsync(id, command, cancellationToken);
        return ToUserResponse(result);
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserResponse>> UpdateStatus(
        Guid id,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetUserActiveCommand(request.IsActive);
        var result = await userService.SetActiveAsync(id, command, cancellationToken);
        return ToUserResponse(result);
    }

    private ActionResult<AdminUserResponse> ToUserResponse(ServiceResult<AdminUserDto> result)
    {
        if (result.Succeeded && result.Value is not null)
        {
            return Ok(AdminUserResponse.FromDto(result.Value));
        }

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(result.Error),
            ServiceErrorType.Validation => BadRequest(result.Error),
            ServiceErrorType.Conflict => Conflict(result.Error),
            _ => BadRequest(result.Error)
        };
    }
}
