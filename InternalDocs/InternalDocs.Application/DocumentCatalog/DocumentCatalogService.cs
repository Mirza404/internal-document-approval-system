using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Common;
using InternalDocs.Domain.Entities;

namespace InternalDocs.Application.DocumentCatalog;

public sealed class DocumentCatalogService(IDocumentTypeRepository documentTypes) : IDocumentCatalogService
{
    public async Task<IReadOnlyList<DocumentCategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var categories = await documentTypes.GetCategoriesAsync(cancellationToken);
        return categories.Select(DocumentCategoryDto.FromEntity).ToList();
    }

    public async Task<IReadOnlyList<DocumentTypeDto>> GetDocumentTypesAsync(
        CancellationToken cancellationToken)
    {
        var result = await documentTypes.GetAllAsync(cancellationToken);
        return result.Select(DocumentTypeDto.FromEntity).ToList();
    }

    public async Task<ServiceResult<DocumentTypeDto>> GetDocumentTypeByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var documentType = await documentTypes.GetByIdWithCategoryAsync(id, cancellationToken);
        if (documentType is null)
        {
            return ServiceResult<DocumentTypeDto>.Failure(
                "Document type was not found.",
                ServiceErrorType.NotFound);
        }

        return ServiceResult<DocumentTypeDto>.Success(DocumentTypeDto.FromEntity(documentType));
    }

    public async Task<ServiceResult<DocumentCategoryDto>> CreateCategoryAsync(
        CreateDocumentCategoryCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Validation<DocumentCategoryDto>("Name is required.");
        }

        var category = new DocumentCategory
        {
            Id = Guid.NewGuid(),
            Name = command.Name.Trim(),
            Description = command.Description?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        documentTypes.AddCategory(category);
        await documentTypes.SaveChangesAsync(cancellationToken);

        return ServiceResult<DocumentCategoryDto>.Success(DocumentCategoryDto.FromEntity(category));
    }

    public async Task<ServiceResult<DocumentCategoryDto>> UpdateCategoryAsync(
        Guid id,
        UpdateDocumentCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var category = await documentTypes.GetCategoryByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return ServiceResult<DocumentCategoryDto>.Failure(
                "Document category was not found.",
                ServiceErrorType.NotFound);
        }

        if (command.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                return Validation<DocumentCategoryDto>("Name cannot be empty.");
            }

            category.Name = command.Name.Trim();
        }

        if (command.Description is not null)
        {
            category.Description = command.Description.Trim();
        }

        await documentTypes.SaveChangesAsync(cancellationToken);
        return ServiceResult<DocumentCategoryDto>.Success(DocumentCategoryDto.FromEntity(category));
    }

    public async Task<ServiceResult> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await documentTypes.GetCategoryByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return ServiceResult.Failure("Document category was not found.", ServiceErrorType.NotFound);
        }

        if (await documentTypes.CategoryHasDocumentTypesAsync(id, cancellationToken))
        {
            return ServiceResult.Failure(
                "Document category has document types assigned.",
                ServiceErrorType.Validation);
        }

        documentTypes.RemoveCategory(category);
        await documentTypes.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<DocumentTypeDto>> CreateDocumentTypeAsync(
        CreateDocumentTypeCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Validation<DocumentTypeDto>("Name is required.");
        }

        if (command.CategoryId == Guid.Empty)
        {
            return Validation<DocumentTypeDto>("CategoryId is required.");
        }

        var category = await documentTypes.GetCategoryByIdAsync(command.CategoryId, cancellationToken);
        if (category is null)
        {
            return Validation<DocumentTypeDto>("CategoryId does not exist.");
        }

        var documentType = new DocumentType
        {
            Id = Guid.NewGuid(),
            Name = command.Name.Trim(),
            Description = command.Description?.Trim() ?? string.Empty,
            CategoryId = command.CategoryId,
            RequiresApproval = command.RequiresApproval,
            CreatedAt = DateTime.UtcNow,
            Category = category
        };

        documentTypes.AddDocumentType(documentType);
        await documentTypes.SaveChangesAsync(cancellationToken);

        var created = await documentTypes.GetDocumentTypeByIdAsync(documentType.Id, cancellationToken);
        return ServiceResult<DocumentTypeDto>.Success(
            DocumentTypeDto.FromEntity(created ?? documentType));
    }

    public async Task<ServiceResult<DocumentTypeDto>> UpdateDocumentTypeAsync(
        Guid id,
        UpdateDocumentTypeCommand command,
        CancellationToken cancellationToken)
    {
        var documentType = await documentTypes.GetDocumentTypeByIdAsync(id, cancellationToken);
        if (documentType is null)
        {
            return ServiceResult<DocumentTypeDto>.Failure(
                "Document type was not found.",
                ServiceErrorType.NotFound);
        }

        if (command.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                return Validation<DocumentTypeDto>("Name cannot be empty.");
            }

            documentType.Name = command.Name.Trim();
        }

        if (command.Description is not null)
        {
            documentType.Description = command.Description.Trim();
        }

        if (command.CategoryId.HasValue)
        {
            if (await documentTypes.GetCategoryByIdAsync(command.CategoryId.Value, cancellationToken) is null)
            {
                return Validation<DocumentTypeDto>("CategoryId does not exist.");
            }

            documentType.CategoryId = command.CategoryId.Value;
        }

        if (command.RequiresApproval.HasValue)
        {
            documentType.RequiresApproval = command.RequiresApproval.Value;
        }

        await documentTypes.SaveChangesAsync(cancellationToken);
        var updated = await documentTypes.GetDocumentTypeByIdAsync(id, cancellationToken);
        return ServiceResult<DocumentTypeDto>.Success(
            DocumentTypeDto.FromEntity(updated ?? documentType));
    }

    public async Task<ServiceResult> DeleteDocumentTypeAsync(Guid id, CancellationToken cancellationToken)
    {
        var documentType = await documentTypes.GetDocumentTypeByIdAsync(id, cancellationToken);
        if (documentType is null)
        {
            return ServiceResult.Failure("Document type was not found.", ServiceErrorType.NotFound);
        }

        if (await documentTypes.DocumentTypeHasDocumentsAsync(id, cancellationToken))
        {
            return ServiceResult.Failure(
                "Document type has documents assigned.",
                ServiceErrorType.Validation);
        }

        documentTypes.RemoveDocumentType(documentType);
        await documentTypes.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    private static ServiceResult<T> Validation<T>(string message)
    {
        return ServiceResult<T>.Failure(message, ServiceErrorType.Validation);
    }
}
