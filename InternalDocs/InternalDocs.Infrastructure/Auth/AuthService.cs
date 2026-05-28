using System.Net.Http.Headers;
using System.Text.Json;
using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Auth;
using InternalDocs.Application.Common;
using InternalDocs.Domain.Entities;
using BC = BCrypt.Net.BCrypt;

namespace InternalDocs.Infrastructure.Auth;

public sealed class AuthService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IHttpClientFactory httpClientFactory) : IAuthService
{
    private const string GraphMeEndpoint = "https://graph.microsoft.com/v1.0/me";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // -------------------------------------------------------------------------
    // Current user
    // -------------------------------------------------------------------------

    public async Task<ServiceResult<AuthDto>> GetOrCreateMicrosoftUserAsync(
        MicrosoftUserClaims claims,
        CancellationToken cancellationToken = default)
    {
        if (!IsAllowedIusEmail(claims.Email))
        {
            return ServiceResult<AuthDto>.Failure(
                "Registration is restricted to IUS accounts (@ius.edu.ba).",
                ServiceErrorType.Validation);
        }

        var user = await userRepository.FindByMicrosoftObjectIdAsync(
            claims.MicrosoftObjectId,
            cancellationToken);

        if (user is null)
        {
            user = await userRepository.FindByEmailAsync(claims.Email, cancellationToken);
            if (user is not null)
            {
                user.MicrosoftObjectId = claims.MicrosoftObjectId;
                await userRepository.UpdateAsync(user, cancellationToken);
            }
        }

        if (user is null)
        {
            user = await userRepository.CreateAsync(
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = claims.Email,
                    FullName = claims.FullName,
                    PasswordHash = string.Empty,
                    MicrosoftObjectId = claims.MicrosoftObjectId,
                    Role = "Employee",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                cancellationToken);
        }

        if (!user.IsActive)
        {
            return ServiceResult<AuthDto>.Failure(
                "This account is inactive.",
                ServiceErrorType.Validation);
        }

        if (user.FullName != claims.FullName && !string.IsNullOrWhiteSpace(claims.FullName))
        {
            user.FullName = claims.FullName;
            await userRepository.UpdateAsync(user, cancellationToken);
        }

        return ServiceResult<AuthDto>.Success(
            new AuthDto(user.Id, user.Email, user.FullName, user.Role, string.Empty));
    }

    // -------------------------------------------------------------------------
    // Register
    // -------------------------------------------------------------------------

    public async Task<ServiceResult<AuthDto>> MicrosoftRegisterAsync(
        MicrosoftRegisterCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate the token and fetch the caller's Microsoft profile
        var profile = await GetMicrosoftProfileAsync(command.MicrosoftAccessToken, cancellationToken);
        if (profile is null)
        {
            return ServiceResult<AuthDto>.Failure(
                "The provided Microsoft access token is invalid or expired.",
                ServiceErrorType.Validation);
        }

        // 2. Reject if an account already exists for this Microsoft identity
        var existing = await userRepository.FindByMicrosoftObjectIdAsync(profile.Id, cancellationToken);
        if (existing is not null)
        {
            return ServiceResult<AuthDto>.Failure(
                "An account for this Microsoft identity already exists. Please log in instead.",
                ServiceErrorType.Conflict);
        }

        // 3. Resolve the e-mail address (required)
        var email = profile.Mail ?? profile.UserPrincipalName;
        if (string.IsNullOrWhiteSpace(email))
        {
            return ServiceResult<AuthDto>.Failure(
                "Microsoft Graph did not return an e-mail address for this account.",
                ServiceErrorType.Validation);
        }

        if (!IsAllowedIusEmail(email))
        {
            return ServiceResult<AuthDto>.Failure(
                "Registration is restricted to IUS accounts (@ius.edu.ba).",
                ServiceErrorType.Validation);
        }

        // 4. Reject if the e-mail is already taken by a different account
        var emailOwner = await userRepository.FindByEmailAsync(email, cancellationToken);
        if (emailOwner is not null)
        {
            return ServiceResult<AuthDto>.Failure(
                "An account with this e-mail address already exists.",
                ServiceErrorType.Conflict);
        }

        // 5. Create the new local user. Elevated roles must be assigned by an admin-controlled flow.
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = profile.DisplayName ?? email,
            PasswordHash = string.Empty, // OAuth-only users have no local password
            MicrosoftObjectId = profile.Id,
            Role = "Employee",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await userRepository.CreateAsync(newUser, cancellationToken);

        // 6. Issue a JWT so the client is immediately authenticated after registration
        var jwt = tokenService.GenerateJwt(createdUser);
        var dto = new AuthDto(createdUser.Id, createdUser.Email, createdUser.FullName, createdUser.Role, jwt);
        return ServiceResult<AuthDto>.Success(dto);
    }

    // -------------------------------------------------------------------------
    // Login
    // -------------------------------------------------------------------------

    public async Task<ServiceResult<AuthDto>> MicrosoftLoginAsync(
        MicrosoftLoginCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate the token and fetch the caller's Microsoft profile
        var profile = await GetMicrosoftProfileAsync(command.MicrosoftAccessToken, cancellationToken);
        if (profile is null)
        {
            return ServiceResult<AuthDto>.Failure(
                "The provided Microsoft access token is invalid or expired.",
                ServiceErrorType.Validation);
        }

        // 2. Look up the existing local user — login is only allowed after registration
        var user = await userRepository.FindByMicrosoftObjectIdAsync(profile.Id, cancellationToken);

        if (user is null)
        {
            // Fall back to e-mail match for accounts created before OAuth was added
            var email = profile.Mail ?? profile.UserPrincipalName;
            if (!string.IsNullOrWhiteSpace(email))
            {
                user = await userRepository.FindByEmailAsync(email, cancellationToken);
                if (user is not null)
                {
                    // Link the existing account to the Microsoft identity going forward
                    user.MicrosoftObjectId = profile.Id;
                    await userRepository.UpdateAsync(user, cancellationToken);
                }
            }
        }

        if (user is null)
        {
            return ServiceResult<AuthDto>.Failure(
                "No account found for this Microsoft identity. Please register first.",
                ServiceErrorType.NotFound);
        }

        if (!user.IsActive)
        {
            return ServiceResult<AuthDto>.Failure(
                "This account is inactive.",
                ServiceErrorType.Validation);
        }

        // 3. Keep the display name in sync
        if (user.FullName != profile.DisplayName && profile.DisplayName is not null)
        {
            user.FullName = profile.DisplayName;
            await userRepository.UpdateAsync(user, cancellationToken);
        }

        // 4. Issue our own JWT
        var jwt = tokenService.GenerateJwt(user);
        var dto = new AuthDto(user.Id, user.Email, user.FullName, user.Role, jwt);
        return ServiceResult<AuthDto>.Success(dto);
    }

    public async Task<ServiceResult<AuthDto>> LocalLoginAsync(
        LocalLoginCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
        {
            return ServiceResult<AuthDto>.Failure(
                "Email and password are required.",
                ServiceErrorType.Validation);
        }

        var user = await userRepository.FindByEmailAsync(command.Email, cancellationToken);
        if (user is null)
        {
            return ServiceResult<AuthDto>.Failure(
                "Invalid email or password.",
                ServiceErrorType.Validation);
        }

        if (!user.IsActive)
        {
            return ServiceResult<AuthDto>.Failure(
                "This account is inactive.",
                ServiceErrorType.Validation);
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return ServiceResult<AuthDto>.Failure(
                "Password login is not available for this account.",
                ServiceErrorType.Validation);
        }

        if (!BC.Verify(command.Password, user.PasswordHash))
        {
            return ServiceResult<AuthDto>.Failure(
                "Invalid email or password.",
                ServiceErrorType.Validation);
        }

        var jwt = tokenService.GenerateJwt(user);
        var dto = new AuthDto(user.Id, user.Email, user.FullName, user.Role, jwt);
        return ServiceResult<AuthDto>.Success(dto);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<MicrosoftProfile?> GetMicrosoftProfileAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient("MicrosoftGraph");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.GetAsync(GraphMeEndpoint, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<MicrosoftProfile>(
            stream,
            JsonOptions,
            cancellationToken);
    }

    private static bool IsAllowedIusEmail(string email) =>
        email.EndsWith("@ius.edu.ba", StringComparison.OrdinalIgnoreCase) ||
        email.EndsWith("@student.ius.edu.ba", StringComparison.OrdinalIgnoreCase);


    // Minimal projection of the fields we use from /me
    private sealed class MicrosoftProfile
    {
        public string Id { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Mail { get; set; }
        public string? UserPrincipalName { get; set; }
    }
}
