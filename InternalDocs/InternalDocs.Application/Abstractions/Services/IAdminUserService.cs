using InternalDocs.Application.Common;
using InternalDocs.Application.Users;

namespace InternalDocs.Application.Abstractions.Services;

public interface IAdminUserService
{
    Task<IReadOnlyList<AdminUserDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ServiceResult<AdminUserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<AdminUserDto>> UpdateRoleAsync(
        Guid id,
        UpdateUserRoleCommand command,
        CancellationToken cancellationToken);
    Task<ServiceResult<AdminUserDto>> SetActiveAsync(
        Guid id,
        SetUserActiveCommand command,
        CancellationToken cancellationToken);
}
