using InternalDocs.Domain.Entities;

namespace InternalDocs.Api.Contracts.Documents;

public sealed class DocumentResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid DocumentTypeId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? ApprovedAt { get; init; }

    public static DocumentResponse FromEntity(Document document)
    {
        return new DocumentResponse
        {
            Id = document.Id,
            Title = document.Title,
            Description = document.Description,
            DocumentTypeId = document.DocumentTypeId,
            CreatedByUserId = document.CreatedByUserId,
            Status = document.Status,
            Priority = document.Priority,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            ApprovedAt = document.ApprovedAt
        };
    }
}