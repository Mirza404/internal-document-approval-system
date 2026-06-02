namespace InternalDocs.Tests;

using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Approvals;
using InternalDocs.Application.Common;
using InternalDocs.Domain.Entities;
using Moq;
using Xunit;

/// <summary>
/// Tests for the core document approval workflow.
/// Verifies that approval decisions (Approve, Reject, RequestChanges) correctly update document status.
/// </summary>
public sealed class DocumentApprovalFlowTests
{
    private static readonly INotificationService NotificationService =
        Mock.Of<INotificationService>();

    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ApproverId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid GenericDocumentTypeId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    [Fact]
    public async Task DecideAsync_ApproveAction_SetsApprovedStatus()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var documentRepository = new FakeDocumentRepository
        {
            Document = new Document
            {
                Id = documentId,
                Title = "Document to approve",
                Description = "Test document for approval",
                DocumentTypeId = GenericDocumentTypeId,
                CreatedByUserId = UserId,
                Status = "PendingApproval",
                Priority = "Normal",
                CreatedAt = DateTime.UtcNow
            }
        };

        var approvalRepository = new FakeApprovalActionRepository();
        var service = new ApprovalService(
            approvalRepository,
            documentRepository,
            new FakeUserRepository(userExists: true),
            NotificationService);

        var command = new ApprovalDecisionCommand(documentId, ApproverId, "Approved as requested");

        // Act
        var result = await service.DecideAsync(command, "approve", CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("approved", result.Value.Status);
        Assert.Equal("Approved", documentRepository.Document.Status);
        Assert.NotNull(documentRepository.Document.ApprovedAt);
        Assert.NotNull(approvalRepository.Approval);
        Assert.Equal("Approved", approvalRepository.Approval.Action);
        Assert.Equal(ApproverId, approvalRepository.Approval.ApprovedByUserId);
        Assert.Equal("Approved as requested", approvalRepository.Approval.Comments);
    }

    [Fact]
    public async Task DecideAsync_RejectAction_SetsRejectedStatus()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var documentRepository = new FakeDocumentRepository
        {
            Document = new Document
            {
                Id = documentId,
                Title = "Document to reject",
                Description = "Test document for rejection",
                DocumentTypeId = GenericDocumentTypeId,
                CreatedByUserId = UserId,
                Status = "PendingApproval",
                Priority = "Normal",
                CreatedAt = DateTime.UtcNow
            }
        };

        var approvalRepository = new FakeApprovalActionRepository();
        var service = new ApprovalService(
            approvalRepository,
            documentRepository,
            new FakeUserRepository(userExists: true),
            NotificationService);

        var command = new ApprovalDecisionCommand(documentId, ApproverId, "Missing supporting documentation");

        // Act
        var result = await service.DecideAsync(command, "reject", CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("rejected", result.Value.Status);
        Assert.Equal("Rejected", documentRepository.Document.Status);
        Assert.Null(documentRepository.Document.ApprovedAt);
        Assert.NotNull(approvalRepository.Approval);
        Assert.Equal("Rejected", approvalRepository.Approval.Action);
        Assert.Equal(ApproverId, approvalRepository.Approval.ApprovedByUserId);
        Assert.Equal("Missing supporting documentation", approvalRepository.Approval.Comments);
    }

    [Fact]
    public async Task DecideAsync_RequestChangesAction_SetsChangesRequestedStatus()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var documentRepository = new FakeDocumentRepository
        {
            Document = new Document
            {
                Id = documentId,
                Title = "Document needing changes",
                Description = "Test document for requested changes",
                DocumentTypeId = GenericDocumentTypeId,
                CreatedByUserId = UserId,
                Status = "PendingApproval",
                Priority = "Normal",
                CreatedAt = DateTime.UtcNow
            }
        };

        var approvalRepository = new FakeApprovalActionRepository();
        var service = new ApprovalService(
            approvalRepository,
            documentRepository,
            new FakeUserRepository(userExists: true),
            NotificationService);

        var command = new ApprovalDecisionCommand(documentId, ApproverId, "Please update section 3 with more details");

        // Act
        var result = await service.DecideAsync(command, "request-changes", CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("changesrequested", result.Value.Status);
        Assert.Equal("ChangesRequested", documentRepository.Document.Status);
        Assert.Null(documentRepository.Document.ApprovedAt);
        Assert.NotNull(approvalRepository.Approval);
        Assert.Equal("ChangesRequested", approvalRepository.Approval.Action);
        Assert.Equal(ApproverId, approvalRepository.Approval.ApprovedByUserId);
        Assert.Equal("Please update section 3 with more details", approvalRepository.Approval.Comments);
    }

    [Fact]
    public async Task DecideAsync_FailsWhenDocumentNotFound()
    {
        // Arrange
        var documentRepository = new FakeDocumentRepository();
        var approvalRepository = new FakeApprovalActionRepository();
        var service = new ApprovalService(
            approvalRepository,
            documentRepository,
            new FakeUserRepository(userExists: true),
            NotificationService);

        var command = new ApprovalDecisionCommand(Guid.NewGuid(), ApproverId, "Approval comment");

        // Act
        var result = await service.DecideAsync(command, "approve", CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.NotFound, result.ErrorType);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DecideAsync_FailsWhenDocumentNotPendingApproval()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var documentRepository = new FakeDocumentRepository
        {
            Document = new Document
            {
                Id = documentId,
                Title = "Already approved document",
                Description = "Document with non-pending status",
                DocumentTypeId = GenericDocumentTypeId,
                CreatedByUserId = UserId,
                Status = "Approved",
                Priority = "Normal",
                CreatedAt = DateTime.UtcNow
            }
        };

        var approvalRepository = new FakeApprovalActionRepository();
        var service = new ApprovalService(
            approvalRepository,
            documentRepository,
            new FakeUserRepository(userExists: true),
            NotificationService);

        var command = new ApprovalDecisionCommand(documentId, ApproverId, "Cannot approve again");

        // Act
        var result = await service.DecideAsync(command, "approve", CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.Conflict, result.ErrorType);
        Assert.Contains("pending approval", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Approved", documentRepository.Document.Status);
    }

    [Fact]
    public async Task DecideAsync_FailsWhenApproverId_IsEmpty()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var documentRepository = new FakeDocumentRepository
        {
            Document = new Document
            {
                Id = documentId,
                Title = "Document for approval",
                Description = "Test document",
                DocumentTypeId = GenericDocumentTypeId,
                CreatedByUserId = UserId,
                Status = "PendingApproval",
                Priority = "Normal",
                CreatedAt = DateTime.UtcNow
            }
        };

        var approvalRepository = new FakeApprovalActionRepository();
        var service = new ApprovalService(
            approvalRepository,
            documentRepository,
            new FakeUserRepository(userExists: true),
            NotificationService);

        var command = new ApprovalDecisionCommand(documentId, Guid.Empty, "Approval comment");

        // Act
        var result = await service.DecideAsync(command, "approve", CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
        Assert.Contains("ApproverId", result.Error);
    }

    [Fact]
    public async Task DecideAsync_FailsWhenApproverNotFound()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var documentRepository = new FakeDocumentRepository
        {
            Document = new Document
            {
                Id = documentId,
                Title = "Document for approval",
                Description = "Test document",
                DocumentTypeId = GenericDocumentTypeId,
                CreatedByUserId = UserId,
                Status = "PendingApproval",
                Priority = "Normal",
                CreatedAt = DateTime.UtcNow
            }
        };

        var approvalRepository = new FakeApprovalActionRepository();
        var service = new ApprovalService(
            approvalRepository,
            documentRepository,
            new FakeUserRepository(userExists: false),
            NotificationService);

        var command = new ApprovalDecisionCommand(documentId, ApproverId, "Approval comment");

        // Act
        var result = await service.DecideAsync(command, "approve", CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
        Assert.Contains("ApproverId", result.Error);
    }

    private sealed class FakeDocumentRepository : IDocumentRepository
    {
        public Document? Document { get; set; }

        public Task<List<Document>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Document is null ? [] : new List<Document> { Document });
        }

        public Task<List<Document>> GetByCreatedByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Document?.CreatedByUserId == userId ? new List<Document> { Document } : []);
        }

        public Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Document?.Id == id ? Document : null);
        }

        public Task<Document?> GetByIdAndCreatedByUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Document?.Id == id && Document.CreatedByUserId == userId ? Document : null);
        }

        public void Add(Document document)
        {
            Document = document;
        }

        public void Remove(Document document)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<List<Document>> GetPendingApprovalQueueAsync(CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }

    private sealed class FakeApprovalActionRepository : IApprovalActionRepository
    {
        public ApprovalAction? Approval { get; private set; }

        public Task<List<ApprovalAction>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Approval is null ? new List<ApprovalAction>() : new List<ApprovalAction> { Approval });
        }

        public Task<ApprovalAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Approval?.Id == id ? Approval : null);
        }

        public void Add(ApprovalAction approvalAction)
        {
            Approval = approvalAction;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<List<ApprovalAction>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly bool userExists;

        public FakeUserRepository(bool userExists = false)
        {
            this.userExists = userExists;
        }

        public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<User>>(new List<User>());

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult<User?>(null);

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(userExists);

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
