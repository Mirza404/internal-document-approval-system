namespace InternalDocs.Application.Documents;

public sealed record UpdateDocumentCommand(
    Guid UserId,
    string? Title,
    string? Description,
    Guid? DocumentTypeId,
    string? Status,
    string? Priority,
    DateTime? ApprovedAt,
    string? LeaveType,
    DateOnly? LeaveStartDate,
    DateOnly? LeaveEndDate,
    decimal? Amount,
    string? BudgetCode,
    string? Counterparty,
    string? AttachmentNote);
