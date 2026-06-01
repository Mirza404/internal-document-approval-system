namespace InternalDocs.Application.Auth;

public sealed record LocalRegisterCommand(string Email, string FullName, string Password);
