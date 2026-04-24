using InternalDocs.Api.Contracts.Documents;
using InternalDocs.Application.Interfaces;
using InternalDocs.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternalDocs.Api.Controllers;

[ApiController]
[Route("documents")]
public sealed class DocumentsController(IAppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<DocumentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DocumentResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var documents = await dbContext.Documents
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(documents.Select(DocumentResponse.FromEntity).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var document = await dbContext.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (document is null)
        {
            return NotFound();
        }

        return Ok(DocumentResponse.FromEntity(document));
    }

    [HttpPost]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentResponse>> Create(
        [FromBody] CreateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            ModelState.AddModelError(nameof(request.Title), "Title is required.");
            return ValidationProblem(ModelState);
        }

        // For early development, fall back to the first available type when caller omits it.
        var documentTypeId = request.DocumentTypeId ?? await dbContext.DocumentTypes
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (documentTypeId is null)
        {
            return BadRequest("No document type exists. Create a document type first.");
        }

        // For early development, fall back to the first available user when caller omits it.
        var createdByUserId = request.CreatedByUserId ?? await dbContext.Users
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (createdByUserId is null)
        {
            return BadRequest("No user exists. Create a user first.");
        }

        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            DocumentTypeId = documentTypeId.Value,
            CreatedByUserId = createdByUserId.Value,
            Status = "Draft",
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Normal" : request.Priority.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            ApprovedAt = null
        };

        dbContext.Documents.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = DocumentResponse.FromEntity(document);
        return CreatedAtAction(nameof(GetById), new { id = document.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentResponse>> Update(
        Guid id,
        [FromBody] UpdateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var document = await dbContext.Documents
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (document is null)
        {
            return NotFound();
        }

        if (request.DocumentTypeId.HasValue)
        {
            var documentTypeExists = await dbContext.DocumentTypes
                .AnyAsync(x => x.Id == request.DocumentTypeId.Value, cancellationToken);

            if (!documentTypeExists)
            {
                return BadRequest("DocumentTypeId does not exist.");
            }

            document.DocumentTypeId = request.DocumentTypeId.Value;
        }

        if (request.CreatedByUserId.HasValue)
        {
            var userExists = await dbContext.Users
                .AnyAsync(x => x.Id == request.CreatedByUserId.Value, cancellationToken);

            if (!userExists)
            {
                return BadRequest("CreatedByUserId does not exist.");
            }

            document.CreatedByUserId = request.CreatedByUserId.Value;
        }

        // Only provided fields are updated; omitted fields keep their existing values.
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            document.Title = request.Title.Trim();
        }

        if (request.Description is not null)
        {
            document.Description = request.Description.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            document.Status = request.Status.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            document.Priority = request.Priority.Trim();
        }

        if (request.ApprovedAt.HasValue)
        {
            document.ApprovedAt = request.ApprovedAt.Value;
        }

        // Keep a simple audit timestamp for last mutation.
        document.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(DocumentResponse.FromEntity(document));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var document = await dbContext.Documents
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (document is null)
        {
            return NotFound();
        }

        dbContext.Documents.Remove(document);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}