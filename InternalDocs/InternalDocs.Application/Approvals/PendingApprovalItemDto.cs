using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Approvals;

public sealed record PendingApprovalItemDto(
    Guid DocumentId,
    string Title,
    string Description,
    Guid DocumentTypeId,
    string DocumentTypeName,
    string DocumentTypeDescription,
    string DocumentCategoryName,
    Guid CreatorId,
    string CreatorFullName,
    string CreatorEmail,
    string Priority,
    string? LeaveType,
    DateOnly? LeaveStartDate,
    DateOnly? LeaveEndDate,
    decimal? Amount,
    string? BudgetCode,
    string? Counterparty,
    string? AttachmentNote,
    DateTime CreatedAt,
    string Status)
{
    public static PendingApprovalItemDto FromEntity(Document document)
    {
        return new PendingApprovalItemDto(
            document.Id,
            document.Title,
            document.Description,
            document.DocumentTypeId,
            document.DocumentType.Name,
            document.DocumentType.Description,
            document.DocumentType.Category.Name,
            document.CreatedByUserId,
            document.CreatedByUser.FullName,
            document.CreatedByUser.Email,
            document.Priority,
            document.LeaveType,
            document.LeaveStartDate,
            document.LeaveEndDate,
            document.Amount,
            document.BudgetCode,
            document.Counterparty,
            document.AttachmentNote,
            document.CreatedAt,
            document.Status);
    }
}
