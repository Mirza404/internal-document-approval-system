using System.Text.Json;
using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Common;
using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Documents;

public sealed class DocumentService(
    IDocumentRepository documents,
    IDocumentTypeRepository documentTypes,
    IUserRepository users,
    INotificationService notificationService) : IDocumentService
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

    public async Task<IReadOnlyList<DocumentDto>> GetByCreatedByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await documents.GetByCreatedByUserIdAsync(userId, cancellationToken);
        return result.Select(DocumentDto.FromEntity).ToList();
    }

    public async Task<ServiceResult<DocumentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, cancellationToken);
        return document is null
            ? ServiceResult<DocumentDto>.Failure("Document was not found.", ServiceErrorType.NotFound)
            : ServiceResult<DocumentDto>.Success(DocumentDto.FromEntity(document));
    }

    public async Task<ServiceResult<DocumentDto>> GetByIdForUserAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAndCreatedByUserIdAsync(id, userId, cancellationToken);
        return document is null
            ? ServiceResult<DocumentDto>.Failure("Document was not found.", ServiceErrorType.NotFound)
            : ServiceResult<DocumentDto>.Success(DocumentDto.FromEntity(document));
    }

    public async Task<ServiceResult<DocumentDto>> CreateAsync(
        CreateDocumentCommand command,
        CancellationToken cancellationToken)
    {
        return await SubmitAsync(
            new SubmitDocumentCommand(
                command.Title,
                command.Description,
                command.DocumentTypeId,
                command.CreatedByUserId,
                command.Priority,
                command.LeaveType,
                command.LeaveStartDate,
                command.LeaveEndDate,
                command.Amount,
                command.BudgetCode,
                command.Counterparty,
                command.AttachmentNote),
            cancellationToken);
    }

    public async Task<ServiceResult<DocumentDto>> SubmitAsync(
        SubmitDocumentCommand command,
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
            Status = "PendingApproval",
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

        document.Versions.Add(new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            VersionNumber = 1,
            MajorVersion = 1,
            MinorVersion = 0,
            Content = CreateVersionSnapshot(document),
            ChangeNotes = "Initial submission",
            CreatedAt = document.CreatedAt
        });

        documents.Add(document);
        await documents.SaveChangesAsync(cancellationToken);

        var approvers = await users.GetAllAsync(cancellationToken);
        var approverIds = approvers
            .Where(user =>
                user.IsActive &&
                (string.Equals(user.Role, "Approver", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase)))
            .Select(user => user.Id)
            .ToList();

        await notificationService.NotifyUsersAsync(
            approverIds,
            "New document submitted",
            $"A new document '{document.Title}' is waiting for approval.",
            "Info",
            cancellationToken);

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

        var isResubmission = false;
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


        var titleChanged = command.Title is not null
            && !string.Equals(document.Title, command.Title.Trim(), StringComparison.Ordinal);

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

            if (status != "PendingApproval")
            {
                return Validation("Employees can only resubmit documents to PendingApproval.");
            }

            if (!string.Equals(document.Status, "ChangesRequested", StringComparison.OrdinalIgnoreCase))
            {
                return Validation("Documents can only be resubmitted when changes are requested.");
            }

            isResubmission = true;
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
        AddDocumentVersion(document, titleChanged, isResubmission, command.ChangeNotes);

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
        return GetDocumentMetadataKind(documentType) switch
        {
            DocumentMetadataKind.Leave => ValidateHrDocument(document),
            DocumentMetadataKind.Payment => ValidatePaymentDocument(document),
            DocumentMetadataKind.Internship => ValidateInternshipDocument(document),
            DocumentMetadataKind.None => null,
            _ => $"Document type category '{documentType.Category.Name}' is not supported."
        };
    }

    private static DocumentMetadataKind GetDocumentMetadataKind(DocumentType documentType)
    {
        var typeName = NormalizeCatalogName(documentType.Name);
        if (typeName is "TRANSCRIPT" or "CERTIFICATE")
        {
            return DocumentMetadataKind.None;
        }

        if (typeName == "INTERNSHIP SUBMISSION")
        {
            return DocumentMetadataKind.Internship;
        }

        if (typeName == "PAYMENT PROCEDURE")
        {
            return DocumentMetadataKind.Payment;
        }

        return NormalizeCatalogName(documentType.Category.Name) switch
        {
            "HR" => DocumentMetadataKind.Leave,
            "FINANCE" => DocumentMetadataKind.Payment,
            "CONTRACT" => DocumentMetadataKind.Internship,
            "GENERIC" or "ACADEMIC RECORDS" or "STUDENT SERVICES" => DocumentMetadataKind.None,
            "INTERNSHIPS" => DocumentMetadataKind.Internship,
            "PAYMENTS" => DocumentMetadataKind.Payment,
            _ => DocumentMetadataKind.Unsupported
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

    private static string? ValidatePaymentDocument(Document document)
    {
        if (!document.Amount.HasValue || document.Amount.Value <= 0)
        {
            return "Payment Procedure documents require Amount greater than 0.";
        }

        if (string.IsNullOrWhiteSpace(document.BudgetCode))
        {
            return "Payment Procedure documents require BudgetCode.";
        }

        return null;
    }

    private static string? ValidateInternshipDocument(Document document)
    {
        if (string.IsNullOrWhiteSpace(document.Counterparty)
            && string.IsNullOrWhiteSpace(document.AttachmentNote))
        {
            return "Internship Submission documents require Counterparty or AttachmentNote.";
        }

        return null;
    }

    private static string NormalizeCatalogName(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void AddDocumentVersion(
        Document document,
        bool titleChanged,
        bool isResubmission,
        string? changeNotes)
    {
        var latestVersion = document.Versions
            .OrderByDescending(version => version.VersionNumber)
            .FirstOrDefault();

        var nextVersionNumber = (latestVersion?.VersionNumber ?? 0) + 1;
        var nextMajorVersion = latestVersion?.MajorVersion ?? 1;
        var nextMinorVersion = latestVersion?.MinorVersion ?? -1;

        if (latestVersion is null)
        {
            nextMinorVersion = 0;
        }
        else if (titleChanged)
        {
            nextMajorVersion++;
            nextMinorVersion = 0;
        }
        else
        {
            nextMinorVersion++;
        }

        document.Versions.Add(new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            VersionNumber = nextVersionNumber,
            MajorVersion = nextMajorVersion,
            MinorVersion = nextMinorVersion,
            Content = CreateVersionSnapshot(document),
            ChangeNotes = GetVersionChangeNotes(titleChanged, isResubmission, changeNotes),
            CreatedAt = document.UpdatedAt ?? DateTime.UtcNow
        });
    }

    private static string GetVersionChangeNotes(bool titleChanged, bool isResubmission, string? changeNotes)
    {
        var normalizedChangeNotes = NormalizeOptional(changeNotes);
        if (normalizedChangeNotes is not null)
        {
            return normalizedChangeNotes;
        }

        if (isResubmission)
        {
            return "Resubmitted after changes requested";
        }

        return titleChanged ? "Title changed" : "Document updated";
    }

    private static string CreateVersionSnapshot(Document document)
    {
        return JsonSerializer.Serialize(new
        {
            document.Title,
            document.Description,
            document.DocumentTypeId,
            document.CreatedByUserId,
            document.Status,
            document.Priority,
            document.LeaveType,
            document.LeaveStartDate,
            document.LeaveEndDate,
            document.Amount,
            document.BudgetCode,
            document.Counterparty,
            document.AttachmentNote,
            document.CreatedAt
        });
    }

    private enum DocumentMetadataKind
    {
        None,
        Leave,
        Payment,
        Internship,
        Unsupported
    }
}
