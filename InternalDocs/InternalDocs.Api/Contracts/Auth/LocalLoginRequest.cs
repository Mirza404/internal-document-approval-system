namespace InternalDocs.Api.Contracts.Auth;

public sealed record LocalLoginRequest(string Email, string Password);
