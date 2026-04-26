namespace InternalDocs.Tests;

using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Approvals;
using InternalDocs.Application.Common;
using InternalDocs.Application.Documents;
using InternalDocs.Domain.Entities;
using InternalDocs.Domain.Enums;
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
            new CreateDocumentCommand("Request", null, null, null, null),
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
                Action = ApprovalActionType.Pending,
                CreatedAt = DateTime.UtcNow
            }
        };

        var service = new ApprovalService(
            approvalRepository,
            new FakeDocumentRepository(),
            new FakeUserRepository());

        var result = await service.UpdateAsync(
            approvalId,
            new UpdateApprovalCommand("typo", null),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
        Assert.Equal(ApprovalActionType.Pending, approvalRepository.Approval.Action);
    }

    [Fact]
    public async Task CreateDocumentAsync_UsesStrongDefaultPriority()
    {
        var documentTypeId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var documentRepository = new FakeDocumentRepository();
        var service = new DocumentService(
            documentRepository,
            new FakeDocumentTypeRepository(documentTypeId),
            new FakeUserRepository(creatorId));

        var result = await service.CreateAsync(
            new CreateDocumentCommand("Request", null, documentTypeId, creatorId, null),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(documentRepository.AddedDocument);
        Assert.Equal(DocumentStatus.Draft, documentRepository.AddedDocument.Status);
        Assert.Equal(DocumentPriority.Normal, documentRepository.AddedDocument.Priority);
        Assert.Equal("Draft", result.Value!.Status);
        Assert.Equal("Normal", result.Value.Priority);
    }

    [Fact]
    public async Task CreateApprovalAsync_ParsesStatusIntoStrongActionType()
    {
        var documentId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var approvalRepository = new FakeApprovalActionRepository();
        var service = new ApprovalService(
            approvalRepository,
            new FakeDocumentRepository(new Document { Id = documentId }),
            new FakeUserRepository(approverId));

        var result = await service.CreateAsync(
            new CreateApprovalCommand(documentId, approverId, "approved", "Looks good"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(approvalRepository.AddedApproval);
        Assert.Equal(ApprovalActionType.Approved, approvalRepository.AddedApproval.Action);
        Assert.Equal("approved", result.Value!.Status);
    }

    private sealed class FakeDocumentRepository : IDocumentRepository
    {
        public FakeDocumentRepository(Document? document = null)
        {
            Document = document;
        }

        public Document? Document { get; }
        public Document? AddedDocument { get; private set; }

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
            AddedDocument = document;
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
        public ApprovalAction? AddedApproval { get; private set; }

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
            AddedApproval = approvalAction;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDocumentTypeRepository : IDocumentTypeRepository
    {
        private readonly Guid? existingId;

        public FakeDocumentTypeRepository(Guid? existingId = null)
        {
            this.existingId = existingId;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(existingId == id);
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly Guid? existingId;

        public FakeUserRepository(Guid? existingId = null)
        {
            this.existingId = existingId;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(existingId == id);
        }
    }
}
