namespace InternalDocs.Application.Auth;

public sealed record MicrosoftRegisterCommand(string MicrosoftAccessToken, string Role = "Employee");
