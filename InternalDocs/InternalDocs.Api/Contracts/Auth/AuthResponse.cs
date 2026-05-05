using InternalDocs.Application.Auth;

namespace InternalDocs.Api.Contracts.Auth;

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    string AccessToken)
{
    public static AuthResponse FromDto(AuthDto dto) =>
        new(dto.UserId, dto.Email, dto.FullName, dto.Role, dto.AccessToken);
}
