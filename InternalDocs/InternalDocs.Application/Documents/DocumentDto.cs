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
    DateTime? ApprovedAt,
    int? LatestVersionNumber,
    int? LatestMajorVersion,
    int? LatestMinorVersion,
    string? LatestVersionLabel,
    DateTime? LatestVersionCreatedAt,
    string? LatestVersionChangeNotes)
{
    public static DocumentDto FromEntity(Document document)
    {
        var latestVersion = document.Versions
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefault();

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
            document.ApprovedAt,
            latestVersion?.VersionNumber,
            latestVersion?.MajorVersion,
            latestVersion?.MinorVersion,
            latestVersion is null ? null : $"v{latestVersion.MajorVersion}.{latestVersion.MinorVersion}",
            latestVersion?.CreatedAt,
            latestVersion?.ChangeNotes);
    }
}
