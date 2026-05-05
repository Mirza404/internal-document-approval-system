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
    public ActionResult<CurrentUserResponse> Me()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? User.FindFirstValue(ClaimTypes.Email);
        var fullName = User.FindFirstValue(JwtRegisteredClaimNames.Name)
            ?? User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (!Guid.TryParse(userIdClaim, out var userId) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(role))
        {
            return Unauthorized();
        }

        return Ok(new CurrentUserResponse(userId, email, fullName, role));
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
