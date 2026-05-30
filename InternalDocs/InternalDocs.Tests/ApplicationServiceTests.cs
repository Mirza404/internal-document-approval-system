namespace InternalDocs.Tests;

using InternalDocs.Application.Abstractions.Repositories;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Approvals;
using InternalDocs.Application.Common;
using InternalDocs.Application.Documents;
using InternalDocs.Domain.Entities;
using Moq;
using Xunit;

public sealed class ApplicationServiceTests
{
    private static readonly INotificationService NotificationService =
        Mock.Of<INotificationService>();

    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TranscriptDocumentTypeId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid CertificateDocumentTypeId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid InternshipDocumentTypeId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid GenericDocumentTypeId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid PaymentDocumentTypeId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

    [Fact]
    public async Task SubmitDocumentAsync_RequiresExplicitDocumentTypeAndCreator()
    {
        var service = new DocumentService(
            new FakeDocumentRepository(),
            new FakeDocumentTypeRepository(),
            new FakeUserRepository(),
            NotificationService);

        var result = await service.SubmitAsync(
            SubmitCommand("Request", createdByUserId: UserId),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
        Assert.Equal("DocumentTypeId is required.", result.Error);
    }

    [Fact]
    public async Task SubmitDocumentAsync_PaymentProcedureRequiresAmountAndPaymentReference()
    {
        var service = new DocumentService(
            new FakeDocumentRepository(),
            new FakeDocumentTypeRepository(CreateDocumentType(PaymentDocumentTypeId, "Payment Procedure", "Payments")),
            new FakeUserRepository(userExists: true),
            NotificationService);

        var result = await service.SubmitAsync(
            SubmitCommand("Payment request", documentTypeId: PaymentDocumentTypeId, createdByUserId: UserId),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
        Assert.Equal("Payment Procedure documents require Amount greater than 0.", result.Error);
    }

    [Fact]
    public async Task SubmitDocumentAsync_InternshipSubmissionRequiresOrganizationOrSupportingNote()
    {
        var service = new DocumentService(
            new FakeDocumentRepository(),
            new FakeDocumentTypeRepository(CreateDocumentType(InternshipDocumentTypeId, "Internship Submission", "Internships")),
            new FakeUserRepository(userExists: true),
            NotificationService);

        var result = await service.SubmitAsync(
            SubmitCommand("Internship request", documentTypeId: InternshipDocumentTypeId, createdByUserId: UserId),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
        Assert.Equal("Internship Submission documents require Counterparty or AttachmentNote.", result.Error);
    }

    [Fact]
    public async Task SubmitDocumentAsync_TranscriptDocumentsDoNotRequireMetadata()
    {
        var documentRepository = new FakeDocumentRepository();
        var service = new DocumentService(
            documentRepository,
            new FakeDocumentTypeRepository(CreateDocumentType(TranscriptDocumentTypeId, "Transcript", "Academic Records")),
            new FakeUserRepository(userExists: true),
            NotificationService);

        var result = await service.SubmitAsync(
            SubmitCommand("Transcript request", "Description", TranscriptDocumentTypeId, UserId),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(documentRepository.Document);
        Assert.Equal("Transcript request", documentRepository.Document.Title);
        Assert.Equal("PendingApproval", documentRepository.Document.Status);
        var version = Assert.Single(documentRepository.Document.Versions);
        Assert.Equal(1, version.VersionNumber);
        Assert.Equal(1, version.MajorVersion);
        Assert.Equal(0, version.MinorVersion);
        Assert.Contains("\"Title\":\"Transcript request\"", version.Content);
        Assert.Equal("Initial submission", version.ChangeNotes);
    }

    [Fact]
    public async Task SubmitDocumentAsync_CertificateDocumentsDoNotRequireMetadata()
    {
        var documentRepository = new FakeDocumentRepository();
        var service = new DocumentService(
            documentRepository,
            new FakeDocumentTypeRepository(CreateDocumentType(CertificateDocumentTypeId, "Certificate", "Student Services")),
            new FakeUserRepository(userExists: true),
            NotificationService);

        var result = await service.SubmitAsync(
            SubmitCommand("Certificate request", "Description", CertificateDocumentTypeId, UserId),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(documentRepository.Document);
        Assert.Equal("Certificate request", documentRepository.Document.Title);
        Assert.Equal("PendingApproval", documentRepository.Document.Status);
        var version = Assert.Single(documentRepository.Document.Versions);
        Assert.Equal(1, version.VersionNumber);
        Assert.Equal(1, version.MajorVersion);
        Assert.Equal(0, version.MinorVersion);
        Assert.Contains("\"Title\":\"Certificate request\"", version.Content);
        Assert.Equal("Initial submission", version.ChangeNotes);
    }

    [Fact]
    public async Task UpdateDocumentAsync_NonTitleEditIncrementsMinorVersion()
    {
        var documentId = Guid.NewGuid();
        var documentRepository = new FakeDocumentRepository
        {
            Document = CreateDocumentWithVersion(documentId, "Transcript request")
        };
        var service = new DocumentService(
            documentRepository,
            new FakeDocumentTypeRepository(CreateDocumentType(TranscriptDocumentTypeId, "Transcript", "Academic Records")),
            new FakeUserRepository(),
            NotificationService);

        var result = await service.UpdateAsync(
            documentId,
            UpdateCommand(description: "Updated description"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(2, documentRepository.Document.Versions.Count);
        var version = documentRepository.Document.Versions.OrderBy(x => x.VersionNumber).Last();
        Assert.Equal(2, version.VersionNumber);
        Assert.Equal(1, version.MajorVersion);
        Assert.Equal(1, version.MinorVersion);
        Assert.Equal("Document updated", version.ChangeNotes);
        Assert.Equal("v1.1", result.Value?.LatestVersionLabel);
    }

    [Fact]
    public async Task UpdateDocumentAsync_TitleEditIncrementsMajorVersionAndResetsMinorVersion()
    {
        var documentId = Guid.NewGuid();
        var documentRepository = new FakeDocumentRepository
        {
            Document = CreateDocumentWithVersion(documentId, "Transcript request", minorVersion: 2)
        };
        var service = new DocumentService(
            documentRepository,
            new FakeDocumentTypeRepository(CreateDocumentType(TranscriptDocumentTypeId, "Transcript", "Academic Records")),
            new FakeUserRepository(),
            NotificationService);

        var result = await service.UpdateAsync(
            documentId,
            UpdateCommand(title: "Official transcript request"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(2, documentRepository.Document.Versions.Count);
        var version = documentRepository.Document.Versions.OrderBy(x => x.VersionNumber).Last();
        Assert.Equal(2, version.VersionNumber);
        Assert.Equal(2, version.MajorVersion);
        Assert.Equal(0, version.MinorVersion);
        Assert.Equal("Title changed", version.ChangeNotes);
        Assert.Equal("v2.0", result.Value?.LatestVersionLabel);
    }

    [Fact]
    public async Task UpdateDocumentAsync_ChangingCategoryValidatesMetadata()
    {
        var documentId = Guid.NewGuid();
        var documentRepository = new FakeDocumentRepository
        {
            Document = new Document
            {
                Id = documentId,
                Title = "Existing document",
                Description = "Description",
                DocumentTypeId = TranscriptDocumentTypeId,
                CreatedByUserId = UserId
            }
        };
        var service = new DocumentService(
            documentRepository,
            new FakeDocumentTypeRepository(
                CreateDocumentType(TranscriptDocumentTypeId, "Transcript", "Academic Records"),
                CreateDocumentType(PaymentDocumentTypeId, "Payment Procedure", "Payments")),
            new FakeUserRepository(),
            NotificationService);

        var result = await service.UpdateAsync(
            documentId,
            UpdateCommand(documentTypeId: PaymentDocumentTypeId),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
        Assert.Equal("Payment Procedure documents require Amount greater than 0.", result.Error);
    }

    [Fact]
    public async Task UpdateDocumentAsync_ResubmitRequiresChangesRequestedStatus()
    {
        var documentId = Guid.NewGuid();
        var documentRepository = new FakeDocumentRepository
        {
            Document = new Document
            {
                Id = documentId,
                Title = "Existing document",
                Description = "Description",
                DocumentTypeId = GenericDocumentTypeId,
                CreatedByUserId = UserId,
                Status = "PendingApproval"
            }
        };
        var service = new DocumentService(
            documentRepository,
            new FakeDocumentTypeRepository(CreateDocumentType(GenericDocumentTypeId, "Generic", "Generic")),
            new FakeUserRepository(),
            NotificationService);

        var result = await service.UpdateAsync(
            documentId,
            UpdateCommand(status: "PendingApproval"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
        Assert.Equal("Documents can only be resubmitted when changes are requested.", result.Error);
        Assert.Empty(documentRepository.Document.Versions);
    }

    [Fact]
    public async Task UpdateDocumentAsync_ResubmitCreatesNewVersionAndSetsPendingApproval()
    {
        var documentId = Guid.NewGuid();
        var documentRepository = new FakeDocumentRepository
        {
            Document = new Document
            {
                Id = documentId,
                Title = "Existing document",
                Description = "Description",
                DocumentTypeId = GenericDocumentTypeId,
                CreatedByUserId = UserId,
                Status = "ChangesRequested",
                Versions =
                [
                    new DocumentVersion
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = documentId,
                        VersionNumber = 1,
                        Content = "original",
                        ChangeNotes = "Initial submission",
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    }
                ]
            }
        };
        var service = new DocumentService(
            documentRepository,
            new FakeDocumentTypeRepository(CreateDocumentType(GenericDocumentTypeId, "Generic", "Generic")),
            new FakeUserRepository(),
            NotificationService);

        var result = await service.UpdateAsync(
            documentId,
            UpdateCommand(
                title: "Updated document",
                description: "Updated description",
                status: "PendingApproval",
                changeNotes: "Fixed requested changes"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("PendingApproval", documentRepository.Document.Status);
        Assert.Equal("Updated document", documentRepository.Document.Title);
        Assert.Equal(2, documentRepository.Document.Versions.Count);

        var version = documentRepository.Document.Versions.OrderBy(x => x.VersionNumber).Last();
        Assert.Equal(2, version.VersionNumber);
        Assert.Contains("\"Title\":\"Updated document\"", version.Content);
        Assert.Equal("Fixed requested changes", version.ChangeNotes);
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
            new FakeUserRepository(),
            NotificationService);

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

        public Task<List<Document>> GetPendingApprovalQueueAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
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

        public Task<List<ApprovalAction>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    private sealed class FakeDocumentTypeRepository : IDocumentTypeRepository
    {
        private readonly IReadOnlyList<DocumentType> documentTypes;

        public FakeDocumentTypeRepository(params DocumentType[] documentTypes)
        {
            this.documentTypes = documentTypes;
        }

        public Task<IReadOnlyList<DocumentCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<DocumentCategory>>(
                documentTypes.Select(x => x.Category).DistinctBy(x => x.Id).ToList());
        }

        public Task<IReadOnlyList<DocumentType>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(documentTypes);
        }

        public Task<DocumentType?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(documentTypes.SingleOrDefault(x => x.Id == id));
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(documentTypes.Any(x => x.Id == id));
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

    private static DocumentType CreateDocumentType(Guid id, string typeName, string categoryName)
    {
        var category = new DocumentCategory
        {
            Id = Guid.NewGuid(),
            Name = categoryName,
            Description = $"{categoryName} category"
        };

        return new DocumentType
        {
            Id = id,
            Name = typeName,
            Description = $"{typeName} document type",
            CategoryId = category.Id,
            Category = category
        };
    }

    private static Document CreateDocumentWithVersion(
        Guid id,
        string title,
        int versionNumber = 1,
        int majorVersion = 1,
        int minorVersion = 0)
    {
        var document = new Document
        {
            Id = id,
            Title = title,
            Description = "Description",
            DocumentTypeId = TranscriptDocumentTypeId,
            CreatedByUserId = UserId,
            Status = "PendingApproval",
            Priority = "Normal",
            CreatedAt = DateTime.UtcNow
        };

        document.Versions.Add(new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = id,
            VersionNumber = versionNumber,
            MajorVersion = majorVersion,
            MinorVersion = minorVersion,
            Content = "{}",
            ChangeNotes = "Initial submission",
            CreatedAt = document.CreatedAt
        });

        return document;
    }

    private static SubmitDocumentCommand SubmitCommand(
        string title,
        string? description = null,
        Guid? documentTypeId = null,
        Guid? createdByUserId = null,
        string? priority = null,
        string? leaveType = null,
        DateOnly? leaveStartDate = null,
        DateOnly? leaveEndDate = null,
        decimal? amount = null,
        string? budgetCode = null,
        string? counterparty = null,
        string? attachmentNote = null)
    {
        return new SubmitDocumentCommand(
            title,
            description,
            documentTypeId,
            createdByUserId ?? Guid.Empty,
            priority,
            leaveType,
            leaveStartDate,
            leaveEndDate,
            amount,
            budgetCode,
            counterparty,
            attachmentNote);
    }

    private static UpdateDocumentCommand UpdateCommand(
        Guid? userId = null,
        string? title = null,
        string? description = null,
        Guid? documentTypeId = null,
        string? status = null,
        string? priority = null,
        DateTime? approvedAt = null,
        string? leaveType = null,
        DateOnly? leaveStartDate = null,
        DateOnly? leaveEndDate = null,
        decimal? amount = null,
        string? budgetCode = null,
        string? counterparty = null,
        string? attachmentNote = null,
        string? changeNotes = null)
    {
        return new UpdateDocumentCommand(
            userId ?? UserId,
            title,
            description,
            documentTypeId,
            status,
            priority,
            approvedAt,
            leaveType,
            leaveStartDate,
            leaveEndDate,
            amount,
            budgetCode,
            counterparty,
            attachmentNote,
            changeNotes);
    }
}
