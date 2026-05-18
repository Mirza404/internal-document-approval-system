namespace InternalDocs.Domain.Entities;

public class DocumentVersion
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public int MajorVersion { get; set; } = 1;
    public int MinorVersion { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ChangeNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public Document Document { get; set; } = null!;
}
