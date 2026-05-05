namespace InternalDocs.Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid DocumentTypeId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, InReview, Approved, Rejected
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Foreign keys
    public DocumentType DocumentType { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;

    // Navigation properties
    public ICollection<DocumentVersion> Versions { get; set; } = [];
    public ICollection<ApprovalAction> ApprovalActions { get; set; } = [];
}
