namespace InternalDocs.Tests;

using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Application.Documents;
using InternalDocs.Domain.Entities;
using InternalDocs.Infrastructure.Data;
using InternalDocs.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

public sealed class DocumentRepositoryTests
{
    [Fact]
    public async Task UpdateAsync_ResubmissionInsertsNewVersion()
    {
        var userId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        context.AddRange(
            new User
            {
                Id = userId,
                Email = "employee@internaldocs.local",
                FullName = "Demo Employee",
                PasswordHash = "hash",
                Role = "Employee"
            },
            new User
            {
                Id = approverId,
                Email = "approver@internaldocs.local",
                FullName = "Demo Approver",
                PasswordHash = "hash",
                Role = "Approver"
            },
            new DocumentCategory
            {
                Id = categoryId,
                Name = "Student Services"
            },
            new DocumentType
            {
                Id = documentTypeId,
                CategoryId = categoryId,
                Name = "Certificate"
            },
            new Document
            {
                Id = documentId,
                Title = "Enrollment certificate request",
                Description = "Certificate required for a scholarship application.",
                DocumentTypeId = documentTypeId,
                CreatedByUserId = userId,
                Status = "ChangesRequested",
                Versions =
                [
                    new DocumentVersion
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = documentId,
                        VersionNumber = 1,
                        MajorVersion = 1,
                        MinorVersion = 0,
                        Content = "original"
                    }
                ]
            });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var notificationService = new Mock<INotificationService>();
        var service = new DocumentService(
            new DocumentRepository(context),
            new DocumentTypeRepository(context),
            new UserRepository(context),
            notificationService.Object);

        var result = await service.UpdateAsync(
            documentId,
            new UpdateDocumentCommand(
                userId,
                "Enrollment certificate request updated",
                null,
                documentTypeId,
                "PendingApproval",
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                "Fixed requested changes"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("PendingApproval", result.Value?.Status);
        Assert.Equal(2, await context.DocumentVersions.CountAsync());
        Assert.All(await context.DocumentVersions.ToListAsync(), version => Assert.NotEqual(Guid.Empty, version.Id));
        notificationService.Verify(service => service.NotifyUsersAsync(
            It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(new[] { approverId })),
            "Document resubmitted",
            "Document 'Enrollment certificate request updated' was resubmitted and is waiting for approval.",
            "Info",
            CancellationToken.None));
    }
}
