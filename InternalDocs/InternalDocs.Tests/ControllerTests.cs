namespace InternalDocs.Tests;

using System.Reflection;
using InternalDocs.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

/// <summary>
/// TEST-2: Guards the main HTTP mappings and authorization rules without
/// requiring a running API or database.
/// </summary>
public sealed class ControllerTests
{
    [Theory]
    [InlineData(typeof(AuthController), nameof(AuthController.MicrosoftRegister), "POST", "microsoft/register")]
    [InlineData(typeof(AuthController), nameof(AuthController.MicrosoftLogin), "POST", "microsoft/login")]
    [InlineData(typeof(AuthController), nameof(AuthController.LocalRegister), "POST", "local/register")]
    [InlineData(typeof(AuthController), nameof(AuthController.LocalLogin), "POST", "local/login")]
    [InlineData(typeof(AuthController), nameof(AuthController.Me), "GET", "me")]
    [InlineData(typeof(DocumentsController), nameof(DocumentsController.Create), "POST", null)]
    [InlineData(typeof(ApprovalsController), nameof(ApprovalsController.GetPendingQueue), "GET", "pending")]
    [InlineData(typeof(ApprovalsController), nameof(ApprovalsController.Approve), "POST", "{documentId:guid}/approve")]
    [InlineData(typeof(ApprovalsController), nameof(ApprovalsController.Reject), "POST", "{documentId:guid}/reject")]
    [InlineData(typeof(ApprovalsController), nameof(ApprovalsController.RequestChanges), "POST", "{documentId:guid}/request-changes")]
    [InlineData(typeof(DocumentCatalogController), nameof(DocumentCatalogController.GetDocumentTypes), "GET", "document-types")]
    [InlineData(typeof(DocumentCatalogController), nameof(DocumentCatalogController.GetDocumentTypeById), "GET", "document-types/{id:guid}")]
    [InlineData(typeof(DocumentCatalogController), nameof(DocumentCatalogController.CreateDocumentType), "POST", "document-types")]
    [InlineData(typeof(DocumentCatalogController), nameof(DocumentCatalogController.UpdateDocumentType), "PUT", "document-types/{id:guid}")]
    [InlineData(typeof(DocumentCatalogController), nameof(DocumentCatalogController.DeleteDocumentType), "DELETE", "document-types/{id:guid}")]
    public void ChecklistEndpoint_UsesExpectedHttpMapping(
        Type controllerType,
        string actionName,
        string expectedMethod,
        string? expectedTemplate)
    {
        var action = GetAction(controllerType, actionName);
        var mapping = Assert.Single(action.GetCustomAttributes().OfType<HttpMethodAttribute>());

        Assert.Equal(expectedTemplate, mapping.Template);
        Assert.Equal([expectedMethod], mapping.HttpMethods);
    }

    [Theory]
    [InlineData(typeof(DocumentsController), "documents")]
    [InlineData(typeof(ApprovalsController), "approvals")]
    public void Controller_UsesExpectedRoutePrefix(Type controllerType, string expectedPrefix)
    {
        var route = Assert.Single(controllerType.GetCustomAttributes<RouteAttribute>());

        Assert.Equal(expectedPrefix, route.Template);
    }

    [Theory]
    [InlineData(typeof(DocumentsController))]
    [InlineData(typeof(ApprovalsController))]
    [InlineData(typeof(DocumentCatalogController))]
    public void ProtectedController_RequiresAuthenticatedUser(Type controllerType)
    {
        var authorize = Assert.Single(controllerType.GetCustomAttributes<AuthorizeAttribute>());

        Assert.Null(authorize.Roles);
    }

    [Theory]
    [InlineData(typeof(AuthController), nameof(AuthController.Me), null)]
    [InlineData(typeof(DocumentsController), nameof(DocumentsController.Create), "Employee")]
    [InlineData(typeof(ApprovalsController), nameof(ApprovalsController.GetPendingQueue), "Approver,Admin")]
    [InlineData(typeof(ApprovalsController), nameof(ApprovalsController.Approve), "Approver,Admin")]
    [InlineData(typeof(ApprovalsController), nameof(ApprovalsController.Reject), "Approver,Admin")]
    [InlineData(typeof(ApprovalsController), nameof(ApprovalsController.RequestChanges), "Approver,Admin")]
    [InlineData(typeof(DocumentCatalogController), nameof(DocumentCatalogController.CreateDocumentType), "Admin")]
    [InlineData(typeof(DocumentCatalogController), nameof(DocumentCatalogController.UpdateDocumentType), "Admin")]
    [InlineData(typeof(DocumentCatalogController), nameof(DocumentCatalogController.DeleteDocumentType), "Admin")]
    public void RestrictedEndpoint_UsesExpectedAuthorizationRoles(
        Type controllerType,
        string actionName,
        string? expectedRoles)
    {
        var authorize = Assert.Single(GetAction(controllerType, actionName)
            .GetCustomAttributes<AuthorizeAttribute>());

        Assert.Equal(expectedRoles, authorize.Roles);
    }

    [Theory]
    [InlineData(typeof(AuthController), nameof(AuthController.MicrosoftRegister), 201, 400, 401, 409)]
    [InlineData(typeof(AuthController), nameof(AuthController.MicrosoftLogin), 200, 400, 401, 404)]
    [InlineData(typeof(AuthController), nameof(AuthController.LocalLogin), 200, 400, 401)]
    [InlineData(typeof(AuthController), nameof(AuthController.Me), 200, 401)]
    [InlineData(typeof(DocumentsController), nameof(DocumentsController.Create), 201, 400)]
    [InlineData(typeof(ApprovalsController), nameof(ApprovalsController.GetPendingQueue), 200)]
    [InlineData(typeof(ApprovalsController), nameof(ApprovalsController.Approve), 200, 400, 404, 409)]
    [InlineData(typeof(ApprovalsController), nameof(ApprovalsController.Reject), 200, 400, 404, 409)]
    [InlineData(typeof(ApprovalsController), nameof(ApprovalsController.RequestChanges), 200, 400, 404, 409)]
    [InlineData(typeof(DocumentCatalogController), nameof(DocumentCatalogController.GetDocumentTypes), 200)]
    [InlineData(typeof(DocumentCatalogController), nameof(DocumentCatalogController.GetDocumentTypeById), 200, 404)]
    [InlineData(typeof(DocumentCatalogController), nameof(DocumentCatalogController.CreateDocumentType), 201, 400)]
    [InlineData(typeof(DocumentCatalogController), nameof(DocumentCatalogController.UpdateDocumentType), 200, 400, 404)]
    [InlineData(typeof(DocumentCatalogController), nameof(DocumentCatalogController.DeleteDocumentType), 204, 400, 404)]
    public void ChecklistEndpoint_DeclaresExpectedResponseStatuses(
        Type controllerType,
        string actionName,
        params int[] expectedStatuses)
    {
        var statuses = GetAction(controllerType, actionName)
            .GetCustomAttributes<ProducesResponseTypeAttribute>()
            .Select(attribute => attribute.StatusCode)
            .Order()
            .ToArray();

        Assert.Equal(expectedStatuses.Order().ToArray(), statuses);
    }

    private static MethodInfo GetAction(Type controllerType, string actionName)
    {
        return controllerType.GetMethod(actionName)
            ?? throw new InvalidOperationException($"{controllerType.Name}.{actionName} was not found.");
    }
}
