using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using InternalDocs.Api.Contracts.Auth;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Auth;
using InternalDocs.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternalDocs.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> Me(CancellationToken cancellationToken)
    {
        var userIdValue = FindFirstClaimValue(ClaimTypes.NameIdentifier, JwtRegisteredClaimNames.Sub);
        var email = FindFirstClaimValue(
            JwtRegisteredClaimNames.Email,
            ClaimTypes.Email,
            "preferred_username",
            "upn",
            "unique_name",
            "email",
            ClaimTypes.Upn);
        var fullName = FindFirstClaimValue(JwtRegisteredClaimNames.Name, ClaimTypes.Name) ?? email;
        var role = FindFirstClaimValue(ClaimTypes.Role, "roles");

        if (Guid.TryParse(userIdValue, out var userId) &&
            !string.IsNullOrWhiteSpace(email) &&
            !string.IsNullOrWhiteSpace(fullName) &&
            !string.IsNullOrWhiteSpace(role))
        {
            return Ok(new CurrentUserResponse(userId, email, fullName, role));
        }

        var microsoftObjectId = FindFirstClaimValue(
            "oid",
            "http://schemas.microsoft.com/identity/claims/objectidentifier",
            ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(microsoftObjectId) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(fullName))
        {
            return Unauthorized();
        }

        var result = await authService.GetOrCreateMicrosoftUserAsync(
            new MicrosoftUserClaims(microsoftObjectId, email, fullName),
            cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            return result.ErrorType switch
            {
                ServiceErrorType.Validation => Unauthorized(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(new CurrentUserResponse(
            result.Value.UserId,
            result.Value.Email,
            result.Value.FullName,
            result.Value.Role));
    }

    private string? FindFirstClaimValue(params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = User.FindFirstValue(claimType);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    [HttpPost("microsoft/register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> MicrosoftRegister(
        [FromBody] MicrosoftRegisterRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return BadRequest("AccessToken is required.");
        }

        var command = new MicrosoftRegisterCommand(request.AccessToken);
        var result = await authService.MicrosoftRegisterAsync(command, cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            return result.ErrorType switch
            {
                ServiceErrorType.Conflict => Conflict(result.Error),
                ServiceErrorType.Validation => Unauthorized(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        var response = AuthResponse.FromDto(result.Value);
        return CreatedAtAction(nameof(MicrosoftRegister), response);
    }

    [HttpPost("microsoft/login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthResponse>> MicrosoftLogin(
        [FromBody] MicrosoftLoginRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return BadRequest("AccessToken is required.");
        }

        var command = new MicrosoftLoginCommand(request.AccessToken);
        var result = await authService.MicrosoftLoginAsync(command, cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            return result.ErrorType switch
            {
                ServiceErrorType.NotFound => NotFound(result.Error),
                ServiceErrorType.Validation => Unauthorized(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(AuthResponse.FromDto(result.Value));
    }
}
