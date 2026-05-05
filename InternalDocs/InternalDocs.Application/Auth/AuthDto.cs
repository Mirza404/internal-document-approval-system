namespace InternalDocs.Application.Auth;

public sealed record AuthDto(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    string AccessToken);

public sealed record MicrosoftUserClaims(
    string MicrosoftObjectId,
    string Email,
    string FullName);
