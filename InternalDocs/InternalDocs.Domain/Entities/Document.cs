namespace InternalDocs.Domain.Entities;

using InternalDocs.Domain.Enums;

public class Document
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid DocumentTypeId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public DocumentPriority Priority { get; set; } = DocumentPriority.Normal;
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
