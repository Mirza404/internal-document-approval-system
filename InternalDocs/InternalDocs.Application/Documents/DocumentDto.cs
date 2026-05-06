using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Documents;

public sealed record DocumentDto(
    Guid Id,
    string Title,
    string Description,
    Guid DocumentTypeId,
    Guid CreatedByUserId,
    string Status,
    string Priority,
    string? LeaveType,
    DateOnly? LeaveStartDate,
    DateOnly? LeaveEndDate,
    decimal? Amount,
    string? BudgetCode,
    string? Counterparty,
    string? AttachmentNote,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ApprovedAt)
{
    public static DocumentDto FromEntity(Document document)
    {
        return new DocumentDto(
            document.Id,
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
            document.CreatedAt,
            document.UpdatedAt,
            document.ApprovedAt);
    }
}
