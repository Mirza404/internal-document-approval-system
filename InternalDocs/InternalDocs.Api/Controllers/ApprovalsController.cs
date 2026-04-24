using InternalDocs.Api.Contracts.Approvals;
using InternalDocs.Application.Interfaces;
using InternalDocs.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternalDocs.Api.Controllers;

[ApiController]
[Route("approvals")]
public sealed class ApprovalsController(IAppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ApprovalResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ApprovalResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var approvals = await dbContext.ApprovalActions
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(approvals.Select(ApprovalResponse.FromEntity).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApprovalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApprovalResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var approval = await dbContext.ApprovalActions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (approval is null)
        {
            return NotFound();
        }

        return Ok(ApprovalResponse.FromEntity(approval));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApprovalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApprovalResponse>> Create(
        [FromBody] CreateApprovalRequest request,
        CancellationToken cancellationToken)
    {
        // Guard against foreign-key violations before inserting the action.
        var documentExists = await dbContext.Documents
            .AnyAsync(x => x.Id == request.DocumentId, cancellationToken);

        if (!documentExists)
        {
            return BadRequest("DocumentId does not exist.");
        }

        var approverExists = await dbContext.Users
            .AnyAsync(x => x.Id == request.ApproverId, cancellationToken);

        if (!approverExists)
        {
            return BadRequest("ApproverId does not exist.");
        }

        var approval = new ApprovalAction
        {
            Id = Guid.NewGuid(),
            DocumentId = request.DocumentId,
            ApprovedByUserId = request.ApproverId,
            Action = NormalizeStatus(request.Status),
            Comments = request.Comments?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.ApprovalActions.Add(approval);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = approval.Id }, ApprovalResponse.FromEntity(approval));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApprovalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApprovalResponse>> Update(
        Guid id,
        [FromBody] UpdateApprovalRequest request,
        CancellationToken cancellationToken)
    {
        var approval = await dbContext.ApprovalActions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (approval is null)
        {
            return NotFound();
        }

        if (request.Status is not null)
        {
            approval.Action = NormalizeStatus(request.Status);
        }

        if (request.Comments is not null)
        {
            approval.Comments = request.Comments.Trim();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ApprovalResponse.FromEntity(approval));
    }

    private static string NormalizeStatus(string? rawStatus)
    {
        // Store canonical values in DB while accepting flexible client casing.
        if (string.IsNullOrWhiteSpace(rawStatus))
        {
            return "Pending";
        }

        return rawStatus.Trim().ToLowerInvariant() switch
        {
            "approved" => "Approved",
            "rejected" => "Rejected",
            _ => "Pending"
        };
    }
}