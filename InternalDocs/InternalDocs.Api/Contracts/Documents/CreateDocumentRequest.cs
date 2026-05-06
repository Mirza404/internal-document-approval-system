namespace InternalDocs.Api.Contracts.Documents;

public sealed class CreateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public string? Priority { get; set; }
    public string? LeaveType { get; set; }
    public DateOnly? LeaveStartDate { get; set; }
    public DateOnly? LeaveEndDate { get; set; }
    public decimal? Amount { get; set; }
    public string? BudgetCode { get; set; }
    public string? Counterparty { get; set; }
    public string? AttachmentNote { get; set; }
}
