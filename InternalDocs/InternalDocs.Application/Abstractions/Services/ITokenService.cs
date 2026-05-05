using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Abstractions.Services;

public interface ITokenService
{
    string GenerateJwt(User user);
}
