using InternalDocs.Api.Contracts.DocumentCatalog;
using InternalDocs.Application.Abstractions.Services;
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
}
