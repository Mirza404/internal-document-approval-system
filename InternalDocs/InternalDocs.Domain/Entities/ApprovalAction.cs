namespace InternalDocs.Domain.Entities;

using InternalDocs.Domain.Enums;

public class ApprovalAction
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid ApprovedByUserId { get; set; }
    public ApprovalActionType Action { get; set; } = ApprovalActionType.Pending;
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign keys
    public Document Document { get; set; } = null!;
    public User ApprovedByUser { get; set; } = null!;
}
