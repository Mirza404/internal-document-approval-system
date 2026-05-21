using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Common;

namespace InternalDocs.Application.Users;

public sealed class AdminUserService(IUserRepository users) : IAdminUserService
{
    private static readonly Dictionary<string, string> AllowedRoles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Employee"] = "Employee",
            ["Approver"] = "Approver",
            ["Admin"] = "Admin"
        };

    public async Task<IReadOnlyList<AdminUserDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await users.GetAllAsync(cancellationToken);
        return result.Select(AdminUserDto.FromEntity).ToList();
    }

    public async Task<ServiceResult<AdminUserDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        return user is null
            ? ServiceResult<AdminUserDto>.Failure("User was not found.", ServiceErrorType.NotFound)
            : ServiceResult<AdminUserDto>.Success(AdminUserDto.FromEntity(user));
    }

    public async Task<ServiceResult<AdminUserDto>> UpdateRoleAsync(
        Guid id,
        UpdateUserRoleCommand command,
        CancellationToken cancellationToken)
    {
        if (!TryNormalizeRole(command.Role, out var normalizedRole))
        {
            return ServiceResult<AdminUserDto>.Failure(
                "Role must be Employee, Approver, or Admin.",
                ServiceErrorType.Validation);
        }

        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<AdminUserDto>.Failure("User was not found.", ServiceErrorType.NotFound);
        }

        if (!IsAllowedIusEmail(user.Email))
        {
            return ServiceResult<AdminUserDto>.Failure(
                "Only IUS accounts can be promoted to Admin or Approver.",
                ServiceErrorType.Validation);
        }

        if (!string.Equals(user.Role, normalizedRole, StringComparison.Ordinal))
        {
            user.Role = normalizedRole;
            await users.UpdateAsync(user, cancellationToken);
        }

        return ServiceResult<AdminUserDto>.Success(AdminUserDto.FromEntity(user));
    }

    public async Task<ServiceResult<AdminUserDto>> SetActiveAsync(
        Guid id,
        SetUserActiveCommand command,
        CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<AdminUserDto>.Failure("User was not found.", ServiceErrorType.NotFound);
        }

        if (user.IsActive != command.IsActive)
        {
            user.IsActive = command.IsActive;
            await users.UpdateAsync(user, cancellationToken);
        }

        return ServiceResult<AdminUserDto>.Success(AdminUserDto.FromEntity(user));
    }

    private static bool TryNormalizeRole(string? rawRole, out string normalizedRole)
    {
        if (string.IsNullOrWhiteSpace(rawRole))
        {
            normalizedRole = string.Empty;
            return false;
        }

        return AllowedRoles.TryGetValue(rawRole.Trim(), out normalizedRole!);
    }

    private static bool IsAllowedIusEmail(string email) =>
        email.EndsWith("@ius.edu.ba", StringComparison.OrdinalIgnoreCase) ||
        email.EndsWith("@student.ius.edu.ba", StringComparison.OrdinalIgnoreCase);
}
