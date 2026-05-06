namespace InternalDocs.Application.Documents;

public sealed record CreateDocumentCommand(
    string Title,
    string? Description,
    Guid? DocumentTypeId,
    Guid? CreatedByUserId,
    string? Priority,
    string? LeaveType,
    DateOnly? LeaveStartDate,
    DateOnly? LeaveEndDate,
    decimal? Amount,
    string? BudgetCode,
    string? Counterparty,
    string? AttachmentNote);
