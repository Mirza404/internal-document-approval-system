using InternalDocs.Application.Approvals;

namespace InternalDocs.Api.Contracts.Approvals;

public sealed class PendingApprovalResponse
{
    public Guid DocumentId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid DocumentTypeId { get; init; }
    public string DocumentTypeName { get; init; } = string.Empty;
    public string DocumentTypeDescription { get; init; } = string.Empty;
    public string DocumentCategoryName { get; init; } = string.Empty;
    public Guid CreatorId { get; init; }
    public string CreatorFullName { get; init; } = string.Empty;
    public string CreatorEmail { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string? LeaveType { get; init; }
    public DateOnly? LeaveStartDate { get; init; }
    public DateOnly? LeaveEndDate { get; init; }
    public decimal? Amount { get; init; }
    public string? BudgetCode { get; init; }
    public string? Counterparty { get; init; }
    public string? AttachmentNote { get; init; }
    public DateTime CreatedAt { get; init; }
    public string Status { get; init; } = string.Empty;

    public static PendingApprovalResponse FromDto(PendingApprovalItemDto pending)
    {
        return new PendingApprovalResponse
        {
            DocumentId = pending.DocumentId,
            Title = pending.Title,
            Description = pending.Description,
            DocumentTypeId = pending.DocumentTypeId,
            DocumentTypeName = pending.DocumentTypeName,
            DocumentTypeDescription = pending.DocumentTypeDescription,
            DocumentCategoryName = pending.DocumentCategoryName,
            CreatorId = pending.CreatorId,
            CreatorFullName = pending.CreatorFullName,
            CreatorEmail = pending.CreatorEmail,
            Priority = pending.Priority,
            LeaveType = pending.LeaveType,
            LeaveStartDate = pending.LeaveStartDate,
            LeaveEndDate = pending.LeaveEndDate,
            Amount = pending.Amount,
            BudgetCode = pending.BudgetCode,
            Counterparty = pending.Counterparty,
            AttachmentNote = pending.AttachmentNote,
            CreatedAt = pending.CreatedAt,
            Status = pending.Status
        };
    }
}
