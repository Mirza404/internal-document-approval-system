namespace InternalDocs.Api.Contracts.Auth;

public sealed record LocalRegisterRequest(string Email, string FullName, string Password);
