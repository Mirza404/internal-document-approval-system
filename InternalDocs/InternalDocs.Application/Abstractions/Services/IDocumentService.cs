using InternalDocs.Application.Common;
using InternalDocs.Application.Documents;

namespace InternalDocs.Application.Abstractions.Services;

public interface IDocumentService
{
    Task<IReadOnlyList<DocumentDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ServiceResult<DocumentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<DocumentDto>> CreateAsync(CreateDocumentCommand command, CancellationToken cancellationToken);
    Task<ServiceResult<DocumentDto>> UpdateAsync(Guid id, UpdateDocumentCommand command, CancellationToken cancellationToken);
    Task<ServiceResult> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
