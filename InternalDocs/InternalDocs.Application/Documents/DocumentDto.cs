using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.Documents;

public sealed record DocumentDto(
    Guid Id,
    string Title,
    string Description,
    Guid DocumentTypeId,
    string DocumentTypeName,
    string DocumentCategoryName,
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
    DateTime? ApprovedAt,
    int? LatestVersionNumber,
    DateTime? LatestVersionCreatedAt,
    string? LatestVersionChangeNotes)
{
    public static DocumentDto FromEntity(Document document)
    {
        var latestVersion = document.Versions
            .OrderByDescending(version => version.VersionNumber)
            .FirstOrDefault();

        return new DocumentDto(
            document.Id,
            document.Title,
            document.Description,
            document.DocumentTypeId,
            document.DocumentType?.Name ?? string.Empty,
            document.DocumentType?.Category?.Name ?? string.Empty,
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
            document.ApprovedAt,
            latestVersion?.VersionNumber,
            latestVersion?.CreatedAt,
            latestVersion?.ChangeNotes);
    }
}
