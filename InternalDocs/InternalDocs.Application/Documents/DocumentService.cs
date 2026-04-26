using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Common;
using InternalDocs.Domain.Entities;
using InternalDocs.Domain.Enums;

namespace InternalDocs.Application.Documents;

public sealed class DocumentService(
    IDocumentRepository documents,
    IDocumentTypeRepository documentTypes,
    IUserRepository users) : IDocumentService
{
    private static readonly Dictionary<string, DocumentStatus> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(DocumentStatus.Draft)] = DocumentStatus.Draft,
        [nameof(DocumentStatus.InReview)] = DocumentStatus.InReview,
        [nameof(DocumentStatus.PendingApproval)] = DocumentStatus.PendingApproval,
        [nameof(DocumentStatus.UnderReview)] = DocumentStatus.UnderReview,
        [nameof(DocumentStatus.ChangesRequested)] = DocumentStatus.ChangesRequested,
        [nameof(DocumentStatus.Approved)] = DocumentStatus.Approved,
        [nameof(DocumentStatus.Rejected)] = DocumentStatus.Rejected
    };

    private static readonly Dictionary<string, DocumentPriority> AllowedPriorities = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(DocumentPriority.Low)] = DocumentPriority.Low,
        [nameof(DocumentPriority.Normal)] = DocumentPriority.Normal,
        [nameof(DocumentPriority.High)] = DocumentPriority.High,
        [nameof(DocumentPriority.Urgent)] = DocumentPriority.Urgent
    };

    public async Task<IReadOnlyList<DocumentDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await documents.GetAllAsync(cancellationToken);
        return result.Select(DocumentDto.FromEntity).ToList();
    }

    public async Task<ServiceResult<DocumentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, cancellationToken);
        return document is null
            ? ServiceResult<DocumentDto>.Failure("Document was not found.", ServiceErrorType.NotFound)
            : ServiceResult<DocumentDto>.Success(DocumentDto.FromEntity(document));
    }

    public async Task<ServiceResult<DocumentDto>> CreateAsync(
        CreateDocumentCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return Validation("Title is required.");
        }

        if (command.DocumentTypeId is null)
        {
            return Validation("DocumentTypeId is required.");
        }

        if (!await documentTypes.ExistsAsync(command.DocumentTypeId.Value, cancellationToken))
        {
            return Validation("DocumentTypeId does not exist.");
        }

        if (command.CreatedByUserId is null)
        {
            return Validation("CreatedByUserId is required.");
        }

        if (!await users.ExistsAsync(command.CreatedByUserId.Value, cancellationToken))
        {
            return Validation("CreatedByUserId does not exist.");
        }

        if (!TryNormalizePriority(command.Priority, out var priority))
        {
            return Validation("Priority must be Low, Normal, High, or Urgent.");
        }

        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = command.Title.Trim(),
            Description = command.Description?.Trim() ?? string.Empty,
            DocumentTypeId = command.DocumentTypeId.Value,
            CreatedByUserId = command.CreatedByUserId.Value,
            Status = DocumentStatus.Draft,
            Priority = priority,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            ApprovedAt = null
        };

        documents.Add(document);
        await documents.SaveChangesAsync(cancellationToken);

        return ServiceResult<DocumentDto>.Success(DocumentDto.FromEntity(document));
    }

    public async Task<ServiceResult<DocumentDto>> UpdateAsync(
        Guid id,
        UpdateDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, cancellationToken);
        if (document is null)
        {
            return ServiceResult<DocumentDto>.Failure("Document was not found.", ServiceErrorType.NotFound);
        }

        if (command.DocumentTypeId.HasValue)
        {
            if (!await documentTypes.ExistsAsync(command.DocumentTypeId.Value, cancellationToken))
            {
                return Validation("DocumentTypeId does not exist.");
            }

            document.DocumentTypeId = command.DocumentTypeId.Value;
        }

        if (command.CreatedByUserId.HasValue)
        {
            if (!await users.ExistsAsync(command.CreatedByUserId.Value, cancellationToken))
            {
                return Validation("CreatedByUserId does not exist.");
            }

            document.CreatedByUserId = command.CreatedByUserId.Value;
        }

        if (command.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(command.Title))
            {
                return Validation("Title cannot be empty.");
            }

            document.Title = command.Title.Trim();
        }

        if (command.Description is not null)
        {
            document.Description = command.Description.Trim();
        }

        if (command.Status is not null)
        {
            if (!TryNormalizeStatus(command.Status, out var status))
            {
                return Validation("Status must be Draft, InReview, PendingApproval, UnderReview, ChangesRequested, Approved, or Rejected.");
            }

            document.Status = status;
        }

        if (command.Priority is not null)
        {
            if (!TryNormalizePriority(command.Priority, out var priority))
            {
                return Validation("Priority must be Low, Normal, High, or Urgent.");
            }

            document.Priority = priority;
        }

        if (command.ApprovedAt.HasValue)
        {
            document.ApprovedAt = command.ApprovedAt.Value;
        }

        document.UpdatedAt = DateTime.UtcNow;

        await documents.SaveChangesAsync(cancellationToken);
        return ServiceResult<DocumentDto>.Success(DocumentDto.FromEntity(document));
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, cancellationToken);
        if (document is null)
        {
            return ServiceResult.Failure("Document was not found.", ServiceErrorType.NotFound);
        }

        documents.Remove(document);
        await documents.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    private static ServiceResult<DocumentDto> Validation(string message)
    {
        return ServiceResult<DocumentDto>.Failure(message, ServiceErrorType.Validation);
    }

    private static bool TryNormalizeStatus(string rawStatus, out DocumentStatus status)
    {
        return AllowedStatuses.TryGetValue(rawStatus.Trim(), out status!);
    }

    private static bool TryNormalizePriority(string? rawPriority, out DocumentPriority priority)
    {
        if (string.IsNullOrWhiteSpace(rawPriority))
        {
            priority = DocumentPriority.Normal;
            return true;
        }

        return AllowedPriorities.TryGetValue(rawPriority.Trim(), out priority!);
    }
}
