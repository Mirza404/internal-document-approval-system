using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Common;
using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Documents;

public sealed class DocumentService(
    IDocumentRepository documents,
    IDocumentTypeRepository documentTypes,
    IUserRepository users) : IDocumentService
{
    private static readonly Dictionary<string, string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Draft"] = "Draft",
        ["InReview"] = "InReview",
        ["PendingApproval"] = "PendingApproval",
        ["UnderReview"] = "UnderReview",
        ["ChangesRequested"] = "ChangesRequested",
        ["Approved"] = "Approved",
        ["Rejected"] = "Rejected"
    };

    private static readonly Dictionary<string, string> AllowedPriorities = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Low"] = "Low",
        ["Normal"] = "Normal",
        ["High"] = "High",
        ["Urgent"] = "Urgent"
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

        var documentType = await documentTypes.GetByIdWithCategoryAsync(command.DocumentTypeId.Value, cancellationToken);
        if (documentType is null)
        {
            return Validation("DocumentTypeId does not exist.");
        }

        if (command.CreatedByUserId == Guid.Empty)
        {
            return Validation("CreatedByUserId is required.");
        }

        if (!await users.ExistsAsync(command.CreatedByUserId, cancellationToken))
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
            CreatedByUserId = command.CreatedByUserId,
            Status = "Draft",
            Priority = priority,
            LeaveType = NormalizeOptional(command.LeaveType),
            LeaveStartDate = command.LeaveStartDate,
            LeaveEndDate = command.LeaveEndDate,
            Amount = command.Amount,
            BudgetCode = NormalizeOptional(command.BudgetCode),
            Counterparty = NormalizeOptional(command.Counterparty),
            AttachmentNote = NormalizeOptional(command.AttachmentNote),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            ApprovedAt = null
        };

        var metadataValidation = ValidateDocumentMetadata(document, documentType);
        if (metadataValidation is not null)
        {
            return Validation(metadataValidation);
        }

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

        if (document.CreatedByUserId != command.UserId)
        {
            return Validation("You can only update your own documents.");
        }

        DocumentType? documentType = null;
        if (command.DocumentTypeId.HasValue)
        {
            documentType = await documentTypes.GetByIdWithCategoryAsync(command.DocumentTypeId.Value, cancellationToken);
            if (documentType is null)
            {
                return Validation("DocumentTypeId does not exist.");
            }

            document.DocumentTypeId = command.DocumentTypeId.Value;
        }
        else
        {
            documentType = await documentTypes.GetByIdWithCategoryAsync(document.DocumentTypeId, cancellationToken);
            if (documentType is null)
            {
                return Validation("DocumentTypeId does not exist.");
            }
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

        ApplyMetadataUpdates(document, command);

        var metadataValidation = ValidateDocumentMetadata(document, documentType);
        if (metadataValidation is not null)
        {
            return Validation(metadataValidation);
        }

        document.UpdatedAt = DateTime.UtcNow;

        await documents.SaveChangesAsync(cancellationToken);
        return ServiceResult<DocumentDto>.Success(DocumentDto.FromEntity(document));
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, cancellationToken);
        if (document is null)
        {
            return ServiceResult.Failure("Document was not found.", ServiceErrorType.NotFound);
        }

        if (document.CreatedByUserId != userId)
        {
            return ServiceResult.Failure("You can only delete your own documents.", ServiceErrorType.Validation);
        }

        documents.Remove(document);
        await documents.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    private static ServiceResult<DocumentDto> Validation(string message)
    {
        return ServiceResult<DocumentDto>.Failure(message, ServiceErrorType.Validation);
    }

    private static bool TryNormalizeStatus(string rawStatus, out string status)
    {
        return AllowedStatuses.TryGetValue(rawStatus.Trim(), out status!);
    }

    private static bool TryNormalizePriority(string? rawPriority, out string priority)
    {
        if (string.IsNullOrWhiteSpace(rawPriority))
        {
            priority = "Normal";
            return true;
        }

        return AllowedPriorities.TryGetValue(rawPriority.Trim(), out priority!);
    }

    private static void ApplyMetadataUpdates(Document document, UpdateDocumentCommand command)
    {
        if (command.LeaveType is not null)
        {
            document.LeaveType = NormalizeOptional(command.LeaveType);
        }

        if (command.LeaveStartDate.HasValue)
        {
            document.LeaveStartDate = command.LeaveStartDate.Value;
        }

        if (command.LeaveEndDate.HasValue)
        {
            document.LeaveEndDate = command.LeaveEndDate.Value;
        }

        if (command.Amount.HasValue)
        {
            document.Amount = command.Amount.Value;
        }

        if (command.BudgetCode is not null)
        {
            document.BudgetCode = NormalizeOptional(command.BudgetCode);
        }

        if (command.Counterparty is not null)
        {
            document.Counterparty = NormalizeOptional(command.Counterparty);
        }

        if (command.AttachmentNote is not null)
        {
            document.AttachmentNote = NormalizeOptional(command.AttachmentNote);
        }
    }

    private static string? ValidateDocumentMetadata(Document document, DocumentType documentType)
    {
        return documentType.Category.Name.Trim().ToUpperInvariant() switch
        {
            "HR" => ValidateHrDocument(document),
            "FINANCE" => ValidateFinanceDocument(document),
            "CONTRACT" => ValidateContractDocument(document),
            "GENERIC" => null,
            _ => $"Document type category '{documentType.Category.Name}' is not supported."
        };
    }

    private static string? ValidateHrDocument(Document document)
    {
        if (string.IsNullOrWhiteSpace(document.LeaveType))
        {
            return "HR documents require LeaveType.";
        }

        if (!document.LeaveStartDate.HasValue || !document.LeaveEndDate.HasValue)
        {
            return "HR documents require LeaveStartDate and LeaveEndDate.";
        }

        if (document.LeaveEndDate.Value < document.LeaveStartDate.Value)
        {
            return "HR document LeaveEndDate cannot be before LeaveStartDate.";
        }

        return null;
    }

    private static string? ValidateFinanceDocument(Document document)
    {
        if (!document.Amount.HasValue || document.Amount.Value <= 0)
        {
            return "Finance documents require Amount greater than 0.";
        }

        if (string.IsNullOrWhiteSpace(document.BudgetCode))
        {
            return "Finance documents require BudgetCode.";
        }

        return null;
    }

    private static string? ValidateContractDocument(Document document)
    {
        if (string.IsNullOrWhiteSpace(document.Counterparty)
            && string.IsNullOrWhiteSpace(document.AttachmentNote))
        {
            return "Contract documents require Counterparty or AttachmentNote.";
        }

        return null;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
