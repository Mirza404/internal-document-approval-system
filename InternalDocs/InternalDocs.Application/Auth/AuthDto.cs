namespace InternalDocs.Application.Auth;

public sealed record AuthDto(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    string AccessToken);
