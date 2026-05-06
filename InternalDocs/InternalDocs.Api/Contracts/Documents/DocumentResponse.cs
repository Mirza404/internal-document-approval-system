using InternalDocs.Application.Documents;

namespace InternalDocs.Api.Contracts.Documents;

public sealed class DocumentResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid DocumentTypeId { get; init; }
    public string DocumentTypeName { get; init; } = string.Empty;
    public string DocumentCategoryName { get; init; } = string.Empty;
    public Guid CreatedByUserId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string? LeaveType { get; init; }
    public DateOnly? LeaveStartDate { get; init; }
    public DateOnly? LeaveEndDate { get; init; }
    public decimal? Amount { get; init; }
    public string? BudgetCode { get; init; }
    public string? Counterparty { get; init; }
    public string? AttachmentNote { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public int? LatestVersionNumber { get; init; }
    public DateTime? LatestVersionCreatedAt { get; init; }
    public string? LatestVersionChangeNotes { get; init; }

    public static DocumentResponse FromDto(DocumentDto document)
    {
        return new DocumentResponse
        {
            Id = document.Id,
            Title = document.Title,
            Description = document.Description,
            DocumentTypeId = document.DocumentTypeId,
            DocumentTypeName = document.DocumentTypeName,
            DocumentCategoryName = document.DocumentCategoryName,
            CreatedByUserId = document.CreatedByUserId,
            Status = document.Status,
            Priority = document.Priority,
            LeaveType = document.LeaveType,
            LeaveStartDate = document.LeaveStartDate,
            LeaveEndDate = document.LeaveEndDate,
            Amount = document.Amount,
            BudgetCode = document.BudgetCode,
            Counterparty = document.Counterparty,
            AttachmentNote = document.AttachmentNote,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            ApprovedAt = document.ApprovedAt,
            LatestVersionNumber = document.LatestVersionNumber,
            LatestVersionCreatedAt = document.LatestVersionCreatedAt,
            LatestVersionChangeNotes = document.LatestVersionChangeNotes
        };
    }
}
