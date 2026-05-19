using InternalDocs.Application.Users;

namespace InternalDocs.Api.Contracts.AdminUsers;

public sealed record AdminUserResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    bool IsActive)
{
    public static AdminUserResponse FromDto(AdminUserDto dto) =>
        new(dto.Id, dto.Email, dto.FullName, dto.Role, dto.IsActive);
}
