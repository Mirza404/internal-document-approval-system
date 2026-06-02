namespace InternalDocs.Tests;

using System;
using InternalDocs.Application.Approvals;
using InternalDocs.Domain.Entities;
using Xunit;

public sealed class PendingApprovalItemDtoTests
{
    private static Document BuildDocument(Action<Document>? configure = null)
    {
        var document = new Document
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Annual leave",
            Description = "Family trip",
            DocumentTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            CreatedByUserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Status = "PendingApproval",
            Priority = "High",
            CreatedAt = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc),
            DocumentType = new DocumentType
            {
                Name = "Leave Request",
                Description = "Employee leave request.",
                Category = new DocumentCategory { Name = "Human Resources" },
            },
            CreatedByUser = new User
            {
                FullName = "Demo Employee",
                Email = "employee@example.com",
            },
        };

        configure?.Invoke(document);
        return document;
    }

    [Fact]
    public void FromEntity_MapsSharedRequestAndRequesterFields()
    {
        var dto = PendingApprovalItemDto.FromEntity(BuildDocument());

        Assert.Equal("Annual leave", dto.Title);
        Assert.Equal("Family trip", dto.Description);
        Assert.Equal("Leave Request", dto.DocumentTypeName);
        Assert.Equal("Employee leave request.", dto.DocumentTypeDescription);
        Assert.Equal("Human Resources", dto.DocumentCategoryName);
        Assert.Equal("Demo Employee", dto.CreatorFullName);
        Assert.Equal("employee@example.com", dto.CreatorEmail);
        Assert.Equal("High", dto.Priority);
        Assert.Equal("PendingApproval", dto.Status);
    }

    [Fact]
    public void FromEntity_MapsLeaveMetadata()
    {
        var document = BuildDocument(d =>
        {
            d.LeaveType = "Annual";
            d.LeaveStartDate = new DateOnly(2026, 6, 10);
            d.LeaveEndDate = new DateOnly(2026, 6, 14);
        });

        var dto = PendingApprovalItemDto.FromEntity(document);

        Assert.Equal("Annual", dto.LeaveType);
        Assert.Equal(new DateOnly(2026, 6, 10), dto.LeaveStartDate);
        Assert.Equal(new DateOnly(2026, 6, 14), dto.LeaveEndDate);
    }

    [Fact]
    public void FromEntity_MapsPaymentMetadata()
    {
        var document = BuildDocument(d =>
        {
            d.Amount = 125.5m;
            d.BudgetCode = "PAY-42";
        });

        var dto = PendingApprovalItemDto.FromEntity(document);

        Assert.Equal(125.5m, dto.Amount);
        Assert.Equal("PAY-42", dto.BudgetCode);
    }

    [Fact]
    public void FromEntity_LeavesUnsetOptionalMetadataNull()
    {
        var dto = PendingApprovalItemDto.FromEntity(BuildDocument());

        Assert.Null(dto.LeaveType);
        Assert.Null(dto.LeaveStartDate);
        Assert.Null(dto.LeaveEndDate);
        Assert.Null(dto.Amount);
        Assert.Null(dto.BudgetCode);
        Assert.Null(dto.Counterparty);
        Assert.Null(dto.AttachmentNote);
    }
}
