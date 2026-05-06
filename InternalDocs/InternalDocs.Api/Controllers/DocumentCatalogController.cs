using InternalDocs.Api.Contracts.DocumentCatalog;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Common;
using InternalDocs.Application.DocumentCatalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternalDocs.Api.Controllers;

[ApiController]
[Authorize]
public sealed class DocumentCatalogController(IDocumentCatalogService documentCatalogService) : ControllerBase
{
    [HttpGet("document-categories")]
    [ProducesResponseType(typeof(List<DocumentCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DocumentCategoryResponse>>> GetCategories(
        CancellationToken cancellationToken)
    {
        var categories = await documentCatalogService.GetCategoriesAsync(cancellationToken);
        return Ok(categories.Select(DocumentCategoryResponse.FromDto).ToList());
    }

    [HttpGet("document-types")]
    [ProducesResponseType(typeof(List<DocumentTypeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DocumentTypeResponse>>> GetDocumentTypes(
        CancellationToken cancellationToken)
    {
        var documentTypes = await documentCatalogService.GetDocumentTypesAsync(cancellationToken);
        return Ok(documentTypes.Select(DocumentTypeResponse.FromDto).ToList());
    }

    [HttpGet("document-types/{id:guid}")]
    [ProducesResponseType(typeof(DocumentTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentTypeResponse>> GetDocumentTypeById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await documentCatalogService.GetDocumentTypeByIdAsync(id, cancellationToken);
        return ToDocumentTypeResponse(result);
    }

    [HttpPost("document-categories")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DocumentCategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentCategoryResponse>> CreateCategory(
        [FromBody] CreateDocumentCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDocumentCategoryCommand(request.Name, request.Description);
        var result = await documentCatalogService.CreateCategoryAsync(command, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return ToCategoryResponse(result);
        }

        var response = DocumentCategoryResponse.FromDto(result.Value);
        return CreatedAtAction(nameof(GetCategories), response);
    }

    [HttpPut("document-categories/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DocumentCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentCategoryResponse>> UpdateCategory(
        Guid id,
        [FromBody] UpdateDocumentCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDocumentCategoryCommand(request.Name, request.Description);
        var result = await documentCatalogService.UpdateCategoryAsync(id, command, cancellationToken);
        return ToCategoryResponse(result);
    }

    [HttpDelete("document-categories/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var result = await documentCatalogService.DeleteCategoryAsync(id, cancellationToken);
        if (result.Succeeded)
        {
            return NoContent();
        }

        return result.ErrorType == ServiceErrorType.NotFound
            ? NotFound(result.Error)
            : BadRequest(result.Error);
    }

    [HttpPost("document-types")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DocumentTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentTypeResponse>> CreateDocumentType(
        [FromBody] CreateDocumentTypeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDocumentTypeCommand(
            request.Name,
            request.Description,
            request.CategoryId,
            request.RequiresApproval);
        var result = await documentCatalogService.CreateDocumentTypeAsync(command, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return ToDocumentTypeResponse(result);
        }

        var response = DocumentTypeResponse.FromDto(result.Value);
        return CreatedAtAction(nameof(GetDocumentTypes), response);
    }

    [HttpPut("document-types/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DocumentTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentTypeResponse>> UpdateDocumentType(
        Guid id,
        [FromBody] UpdateDocumentTypeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDocumentTypeCommand(
            request.Name,
            request.Description,
            request.CategoryId,
            request.RequiresApproval);
        var result = await documentCatalogService.UpdateDocumentTypeAsync(id, command, cancellationToken);
        return ToDocumentTypeResponse(result);
    }

    [HttpDelete("document-types/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteDocumentType(Guid id, CancellationToken cancellationToken)
    {
        var result = await documentCatalogService.DeleteDocumentTypeAsync(id, cancellationToken);
        if (result.Succeeded)
        {
            return NoContent();
        }

        return result.ErrorType == ServiceErrorType.NotFound
            ? NotFound(result.Error)
            : BadRequest(result.Error);
    }

    private ActionResult<DocumentCategoryResponse> ToCategoryResponse(
        ServiceResult<DocumentCategoryDto> result)
    {
        if (result.Succeeded && result.Value is not null)
        {
            return Ok(DocumentCategoryResponse.FromDto(result.Value));
        }

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(result.Error),
            ServiceErrorType.Validation => BadRequest(result.Error),
            ServiceErrorType.Conflict => Conflict(result.Error),
            _ => BadRequest(result.Error)
        };
    }

    private ActionResult<DocumentTypeResponse> ToDocumentTypeResponse(
        ServiceResult<DocumentTypeDto> result)
    {
        if (result.Succeeded && result.Value is not null)
        {
            return Ok(DocumentTypeResponse.FromDto(result.Value));
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
