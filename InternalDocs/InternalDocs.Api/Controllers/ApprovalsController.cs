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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApprovalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApprovalResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await approvalService.GetByIdAsync(id, cancellationToken);
        return ToApprovalResponse(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApprovalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApprovalResponse>> Create(
        [FromBody] CreateApprovalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateApprovalCommand(
            request.DocumentId,
            request.ApproverId,
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
    [ProducesResponseType(typeof(ApprovalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApprovalResponse>> Update(
        Guid id,
        [FromBody] UpdateApprovalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateApprovalCommand(request.Status, request.Comments);
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
}
