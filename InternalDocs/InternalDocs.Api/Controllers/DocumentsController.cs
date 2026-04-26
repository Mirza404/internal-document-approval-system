using InternalDocs.Api.Contracts.Documents;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Common;
using InternalDocs.Application.Documents;
using Microsoft.AspNetCore.Mvc;

namespace InternalDocs.Api.Controllers;

[ApiController]
[Route("documents")]
public sealed class DocumentsController(IDocumentService documentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<DocumentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DocumentResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var documents = await documentService.GetAllAsync(cancellationToken);
        return Ok(documents.Select(DocumentResponse.FromDto).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await documentService.GetByIdAsync(id, cancellationToken);
        return ToDocumentResponse(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentResponse>> Create(
        [FromBody] CreateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDocumentCommand(
            request.Title,
            request.Description,
            request.DocumentTypeId,
            request.CreatedByUserId,
            request.Priority);

        var result = await documentService.CreateAsync(command, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return ToDocumentResponse(result);
        }

        var response = DocumentResponse.FromDto(result.Value);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
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
        var command = new UpdateDocumentCommand(
            request.Title,
            request.Description,
            request.DocumentTypeId,
            request.CreatedByUserId,
            request.Status,
            request.Priority,
            request.ApprovedAt);

        var result = await documentService.UpdateAsync(id, command, cancellationToken);
        return ToDocumentResponse(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await documentService.DeleteAsync(id, cancellationToken);
        if (result.Succeeded)
        {
            return NoContent();
        }

        return result.ErrorType == ServiceErrorType.NotFound
            ? NotFound(result.Error)
            : BadRequest(result.Error);
    }

    private ActionResult<DocumentResponse> ToDocumentResponse(ServiceResult<DocumentDto> result)
    {
        if (result.Succeeded && result.Value is not null)
        {
            return Ok(DocumentResponse.FromDto(result.Value));
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
