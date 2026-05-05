namespace InternalDocs.Api.Contracts.Auth;

public sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Role);
