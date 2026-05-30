using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Common;
using InternalDocs.Domain.Entities;


namespace InternalDocs.Application.Approvals;

public sealed class ApprovalService(
    IApprovalActionRepository approvals,
    IDocumentRepository documents,
    IUserRepository users,
    INotificationService notificationService) : IApprovalService
{
    private static readonly Dictionary<string, string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Pending"] = "Pending",
        ["Approved"] = "Approved",
        ["Rejected"] = "Rejected",
        ["ChangesRequested"] = "ChangesRequested"
    };

    public async Task<IReadOnlyList<ApprovalDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await approvals.GetAllAsync(cancellationToken);
        return result.Select(ApprovalDto.FromEntity).ToList();
    }

    public async Task<IReadOnlyList<PendingApprovalItemDto>> GetPendingQueueAsync(
        CancellationToken cancellationToken)
    {
        var pendingDocuments = await documents.GetPendingApprovalQueueAsync(cancellationToken);
        return pendingDocuments.Select(PendingApprovalItemDto.FromEntity).ToList();
    }

    public async Task<IReadOnlyList<ApprovalDto>> GetByDocumentIdAsync(
    Guid documentId,
    CancellationToken cancellationToken)
    {
        var result = await approvals.GetByDocumentIdAsync(
            documentId,
            cancellationToken);

        return result.Select(ApprovalDto.FromEntity).ToList();
    }

    public async Task<ServiceResult<ApprovalDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var approval = await approvals.GetByIdAsync(id, cancellationToken);
        return approval is null
            ? ServiceResult<ApprovalDto>.Failure("Approval action was not found.", ServiceErrorType.NotFound)
            : ServiceResult<ApprovalDto>.Success(ApprovalDto.FromEntity(approval));
    }

    public async Task<ServiceResult<ApprovalDto>> CreateAsync(
        CreateApprovalCommand command,
        CancellationToken cancellationToken)
    {
        if (command.DocumentId == Guid.Empty)
        {
            return Validation("DocumentId is required.");
        }

        if (await documents.GetByIdAsync(command.DocumentId, cancellationToken) is null)
        {
            return Validation("DocumentId does not exist.");
        }

        if (command.ApproverId == Guid.Empty)
        {
            return Validation("ApproverId is required.");
        }

        if (!await users.ExistsAsync(command.ApproverId, cancellationToken))
        {
            return Validation("ApproverId does not exist.");
        }

        if (!TryNormalizeStatus(command.Status, out var status))
        {
            return Validation("Status must be Pending, Approved, or Rejected.");
        }

        var approval = new ApprovalAction
        {
            Id = Guid.NewGuid(),
            DocumentId = command.DocumentId,
            ApprovedByUserId = command.ApproverId,
            Action = status,
            Comments = command.Comments?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        approvals.Add(approval);
        await approvals.SaveChangesAsync(cancellationToken);

        return ServiceResult<ApprovalDto>.Success(ApprovalDto.FromEntity(approval));
    }

    public async Task<ServiceResult<ApprovalDto>> UpdateAsync(
        Guid id,
        UpdateApprovalCommand command,
        CancellationToken cancellationToken)
    {
        var approval = await approvals.GetByIdAsync(id, cancellationToken);
        if (approval is null)
        {
            return ServiceResult<ApprovalDto>.Failure("Approval action was not found.", ServiceErrorType.NotFound);
        }

        if (approval.ApprovedByUserId != command.ApproverId)
        {
            return Validation("You can only update your own approval actions.");
        }

        if (command.Status is not null)
        {
            if (!TryNormalizeStatus(command.Status, out var status))
            {
                return Validation("Status must be Pending, Approved, or Rejected.");
            }

            approval.Action = status;
        }

        if (command.Comments is not null)
        {
            approval.Comments = command.Comments.Trim();
        }

        await approvals.SaveChangesAsync(cancellationToken);
        return ServiceResult<ApprovalDto>.Success(ApprovalDto.FromEntity(approval));
    }

    private static ServiceResult<ApprovalDto> Validation(string message)
    {
        return ServiceResult<ApprovalDto>.Failure(message, ServiceErrorType.Validation);
    }

    private static bool TryNormalizeStatus(string? rawStatus, out string status)
    {
        if (string.IsNullOrWhiteSpace(rawStatus))
        {
            status = "Pending";
            return true;
        }

        return AllowedStatuses.TryGetValue(rawStatus.Trim(), out status!);
    }

    public async Task<ServiceResult<ApprovalDto>> DecideAsync(
        ApprovalDecisionCommand command,
        string action,
        CancellationToken cancellationToken)
    {
        if (command.DocumentId == Guid.Empty)
        {
            return Validation("DocumentId is required.");
        }

        if (command.ApproverId == Guid.Empty)
        {
            return Validation("ApproverId is required.");
        }

        if (!await users.ExistsAsync(command.ApproverId, cancellationToken))
        {
            return Validation("ApproverId does not exist.");
        }

        var document = await documents.GetByIdAsync(command.DocumentId, cancellationToken);
        if (document is null)
        {
            return ServiceResult<ApprovalDto>.Failure(
                "Document was not found.",
                ServiceErrorType.NotFound);
        }

        if (!string.Equals(document.Status, "PendingApproval", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<ApprovalDto>.Failure(
                "Only documents pending approval can be approved, rejected, or sent back for changes.",
                ServiceErrorType.Conflict);
        }

        var normalizedAction = action switch
        {
            "approve" => "Approved",
            "reject" => "Rejected",
            "request-changes" => "ChangesRequested",
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(normalizedAction))
        {
            return Validation("Invalid approval action.");
        }

        var now = DateTime.UtcNow;

        var approval = new ApprovalAction
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            ApprovedByUserId = command.ApproverId,
            Action = normalizedAction,
            Comments = command.Comments?.Trim(),
            CreatedAt = now
        };

        document.Status = normalizedAction;
        document.UpdatedAt = now;
        document.ApprovedAt = normalizedAction == "Approved" ? now : null;

        approvals.Add(approval);
await approvals.SaveChangesAsync(cancellationToken);

await notificationService.NotifyUserAsync(
    document.CreatedByUserId,
    $"Document {normalizedAction}",
    $"Your document '{document.Title}' was {normalizedAction.ToLowerInvariant()}.",
    normalizedAction == "Approved" ? "Success" : "Warning",
    cancellationToken);

return ServiceResult<ApprovalDto>.Success(ApprovalDto.FromEntity(approval));
    }



}
