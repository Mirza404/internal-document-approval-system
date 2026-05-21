namespace InternalDocs.Application.Auth;

public sealed record LocalLoginCommand(string Email, string Password);
