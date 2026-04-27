namespace InternalDocs.Api.Contracts.Auth;

public sealed record MicrosoftRegisterRequest(string AccessToken, string Role = "Employee");
