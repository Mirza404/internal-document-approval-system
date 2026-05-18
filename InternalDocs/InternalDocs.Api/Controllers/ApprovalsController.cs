using InternalDocs.Api.Authentication;
using InternalDocs.Api.Contracts.Approvals;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Approvals;
using InternalDocs.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternalDocs.Api.Controllers;

[ApiController]
[Authorize]
[Route("approvals")]
public sealed class ApprovalsController(IApprovalService approvalService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ApprovalResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ApprovalResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var approvals = await approvalService.GetAllAsync(cancellationToken);
        return Ok(approvals.Select(ApprovalResponse.FromDto).ToList());
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Approver,Admin")]
    [ProducesResponseType(typeof(List<PendingApprovalResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PendingApprovalResponse>>> GetPendingQueue(
        CancellationToken cancellationToken)
    {
        var pending = await approvalService.GetPendingQueueAsync(cancellationToken);
        return Ok(pending.Select(PendingApprovalResponse.FromDto).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApprovalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApprovalResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await approvalService.GetByIdAsync(id, cancellationToken);
        return ToApprovalResponse(result);
    }

    [HttpPost]
    [Authorize(Roles = "Approver")]
    [ProducesResponseType(typeof(ApprovalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApprovalResponse>> Create(
        [FromBody] CreateApprovalRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var command = new CreateApprovalCommand(
            request.DocumentId,
            userId.Value,
            request.Status,
            request.Comments);

        var result = await approvalService.CreateAsync(command, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return ToApprovalResponse(result);
        }

        var response = ApprovalResponse.FromDto(result.Value);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Approver")]
    [ProducesResponseType(typeof(ApprovalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApprovalResponse>> Update(
        Guid id,
        [FromBody] UpdateApprovalRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var command = new UpdateApprovalCommand(userId.Value, request.Status, request.Comments);
        var result = await approvalService.UpdateAsync(id, command, cancellationToken);
        return ToApprovalResponse(result);
    }

    private ActionResult<ApprovalResponse> ToApprovalResponse(ServiceResult<ApprovalDto> result)
    {
        if (result.Succeeded && result.Value is not null)
        {
            return Ok(ApprovalResponse.FromDto(result.Value));
        }

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(result.Error),
            ServiceErrorType.Validation => BadRequest(result.Error),
            ServiceErrorType.Conflict => Conflict(result.Error),
            _ => BadRequest(result.Error)
        };
    }

    [HttpPost("{documentId:guid}/approve")]
[Authorize(Roles = "Approver,Admin")]
[ProducesResponseType(typeof(ApprovalResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public async Task<ActionResult<ApprovalResponse>> Approve(
    Guid documentId,
    [FromBody] ApprovalDecisionRequest request,
    CancellationToken cancellationToken)
{
    var userId = User.GetUserId();
    if (userId is null)
    {
        return Unauthorized();
    }

    var command = new ApprovalDecisionCommand(documentId, userId.Value, request.Comments);
    var result = await approvalService.DecideAsync(command, "approve", cancellationToken);
    return ToApprovalResponse(result);
}

[HttpPost("{documentId:guid}/reject")]
[Authorize(Roles = "Approver,Admin")]
[ProducesResponseType(typeof(ApprovalResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public async Task<ActionResult<ApprovalResponse>> Reject(
    Guid documentId,
    [FromBody] ApprovalDecisionRequest request,
    CancellationToken cancellationToken)
{
    var userId = User.GetUserId();
    if (userId is null)
    {
        return Unauthorized();
    }

    var command = new ApprovalDecisionCommand(documentId, userId.Value, request.Comments);
    var result = await approvalService.DecideAsync(command, "reject", cancellationToken);
    return ToApprovalResponse(result);
}

[HttpPost("{documentId:guid}/request-changes")]
[Authorize(Roles = "Approver,Admin")]
[ProducesResponseType(typeof(ApprovalResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public async Task<ActionResult<ApprovalResponse>> RequestChanges(
    Guid documentId,
    [FromBody] ApprovalDecisionRequest request,
    CancellationToken cancellationToken)
{
    var userId = User.GetUserId();
    if (userId is null)
    {
        return Unauthorized();
    }

    var command = new ApprovalDecisionCommand(documentId, userId.Value, request.Comments);
    var result = await approvalService.DecideAsync(command, "request-changes", cancellationToken);
    return ToApprovalResponse(result);
}
}
