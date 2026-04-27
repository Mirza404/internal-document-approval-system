using InternalDocs.Application.Auth;
using InternalDocs.Application.Common;

namespace InternalDocs.Application.Abstractions.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthDto>> MicrosoftRegisterAsync(
        MicrosoftRegisterCommand command,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AuthDto>> MicrosoftLoginAsync(
        MicrosoftLoginCommand command,
        CancellationToken cancellationToken = default);
}
