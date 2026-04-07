namespace InternalDocs.Domain.Entities;

public class ApprovalAction
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid ApprovedByUserId { get; set; }
    public string Action { get; set; } = "Pending"; // Approved, Rejected, Pending
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign keys
    public Document Document { get; set; } = null!;
    public User ApprovedByUser { get; set; } = null!;
}
