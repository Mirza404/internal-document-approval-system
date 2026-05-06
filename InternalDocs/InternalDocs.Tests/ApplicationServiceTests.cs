namespace InternalDocs.Tests;

using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Approvals;
using InternalDocs.Application.Common;
using InternalDocs.Application.Documents;
using InternalDocs.Domain.Entities;
using Xunit;

public sealed class ApplicationServiceTests
{
    [Fact]
    public async Task CreateDocumentAsync_RequiresExplicitDocumentTypeAndCreator()
    {
        var service = new DocumentService(
            new FakeDocumentRepository(),
            new FakeDocumentTypeRepository(),
            new FakeUserRepository());

        var result = await service.CreateAsync(
            new CreateDocumentCommand("Request", null, null, Guid.NewGuid(), null),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
        Assert.Equal("DocumentTypeId is required.", result.Error);
    }

    [Fact]
    public async Task UpdateApprovalAsync_RejectsInvalidStatus()
    {
        var approvalId = Guid.NewGuid();
        var approvalRepository = new FakeApprovalActionRepository
        {
            Approval = new ApprovalAction
            {
                Id = approvalId,
                DocumentId = Guid.NewGuid(),
                ApprovedByUserId = Guid.NewGuid(),
                Action = "Pending",
                CreatedAt = DateTime.UtcNow
            }
        };

        var service = new ApprovalService(
            approvalRepository,
            new FakeDocumentRepository(),
            new FakeUserRepository());

        var result = await service.UpdateAsync(
            approvalId,
            new UpdateApprovalCommand(approvalRepository.Approval.ApprovedByUserId, "typo", null),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
        Assert.Equal("Pending", approvalRepository.Approval.Action);
    }

    private sealed class FakeDocumentRepository : IDocumentRepository
    {
        public Document? Document { get; init; }

        public Task<List<Document>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Document is null ? [] : new List<Document> { Document });
        }

        public Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Document?.Id == id ? Document : null);
        }

        public void Add(Document document)
        {
        }

        public void Remove(Document document)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeApprovalActionRepository : IApprovalActionRepository
    {
        public ApprovalAction Approval { get; init; } = new();

        public Task<List<ApprovalAction>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<ApprovalAction> { Approval });
        }

        public Task<ApprovalAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Approval.Id == id ? Approval : null);
        }

        public void Add(ApprovalAction approvalAction)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDocumentTypeRepository : IDocumentTypeRepository
    {
        public Task<IReadOnlyList<DocumentCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<DocumentCategory>>([]);
        }

        public Task<IReadOnlyList<DocumentType>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<DocumentType>>([]);
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<DocumentCategory?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult<DocumentCategory?>(null);
        }

        public Task<DocumentType?> GetDocumentTypeByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult<DocumentType?>(null);
        }

        public Task<bool> CategoryHasDocumentTypesAsync(Guid categoryId, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<bool> DocumentTypeHasDocumentsAsync(Guid documentTypeId, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public void AddCategory(DocumentCategory category)
        {
        }

        public void AddDocumentType(DocumentType documentType)
        {
        }

        public void RemoveCategory(DocumentCategory category)
        {
        }

        public void RemoveDocumentType(DocumentType documentType)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(false);

        public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
            => Task.FromResult<User?>(null);

        public Task<User?> FindByMicrosoftObjectIdAsync(string microsoftObjectId, CancellationToken cancellationToken)
            => Task.FromResult<User?>(null);

        public Task<User> CreateAsync(User user, CancellationToken cancellationToken)
            => Task.FromResult(user);

        public Task UpdateAsync(User user, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
