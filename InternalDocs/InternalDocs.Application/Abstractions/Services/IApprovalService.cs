using InternalDocs.Application.Approvals;
using InternalDocs.Application.Common;

namespace InternalDocs.Application.Abstractions.Services;

public interface IApprovalService
{
    Task<IReadOnlyList<ApprovalDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ServiceResult<ApprovalDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<ApprovalDto>> CreateAsync(CreateApprovalCommand command, CancellationToken cancellationToken);
    Task<ServiceResult<ApprovalDto>> UpdateAsync(Guid id, UpdateApprovalCommand command, CancellationToken cancellationToken);
}
