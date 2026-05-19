using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Users;

public sealed record AdminUserDto(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    bool IsActive)
{
    public static AdminUserDto FromEntity(User user) =>
        new(user.Id, user.Email, user.FullName, user.Role, user.IsActive);
}
