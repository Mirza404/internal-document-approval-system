import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type { AuthUser } from "../auth/authStorage";
import type { Document } from "../api/documents";
import type { DocumentType } from "../api/documentCatalog";
import type { PendingApprovalItem } from "../api/approvals";
import DashboardPage from "./DashboardPage";

const mocks = vi.hoisted(() => ({
  createDocument: vi.fn(),
  decideApproval: vi.fn(),
  getApprovalHistory: vi.fn(),
  markAllNotificationsRead: vi.fn(),
  markNotificationRead: vi.fn(),
  updateAdminUserRole: vi.fn(),
  updateAdminUserStatus: vi.fn(),
  updateDocument: vi.fn(),
  useAdminUsers: vi.fn(),
  useApprovalDecision: vi.fn(),
  useDocumentTypes: vi.fn(),
  useDocuments: vi.fn(),
  useNotifications: vi.fn(),
  usePendingApprovals: vi.fn(),
}));

vi.mock("../hooks/useDocumentCatalog", () => ({
  useDocumentTypes: () => mocks.useDocumentTypes(),
}));

vi.mock("../hooks/useDocuments", () => ({
  useCreateDocument: () => ({
    isPending: false,
    mutateAsync: mocks.createDocument,
  }),
  useDocuments: () => mocks.useDocuments(),
  useUpdateDocument: () => ({
    isPending: false,
    mutateAsync: mocks.updateDocument,
  }),
}));

vi.mock("../hooks/useApprovals", () => ({
  useApprovalDecision: () => ({
    isPending: false,
    mutateAsync: mocks.decideApproval,
  }),
  usePendingApprovals: () => mocks.usePendingApprovals(),
}));

vi.mock("../hooks/useAdminUsers", () => ({
  useAdminUsers: () => mocks.useAdminUsers(),
  useUpdateAdminUserRole: () => ({
    mutateAsync: mocks.updateAdminUserRole,
  }),
  useUpdateAdminUserStatus: () => ({
    mutateAsync: mocks.updateAdminUserStatus,
  }),
}));

vi.mock("../hooks/useNotifications", () => ({
  useMarkAllNotificationsRead: () => ({
    isPending: false,
    mutate: mocks.markAllNotificationsRead,
  }),
  useMarkNotificationRead: () => ({
    mutate: mocks.markNotificationRead,
  }),
  useNotifications: () => mocks.useNotifications(),
}));

vi.mock("../components/documents/DocumentPreview", () => ({
  default: ({ title = "Document preview" }: { title?: string }) => (
    <section aria-label={title}>{title}</section>
  ),
}));

vi.mock("../api/documents", () => ({
  getApprovalHistory: mocks.getApprovalHistory,
}));

const employee: AuthUser = {
  userId: "employee-1",
  email: "employee@example.com",
  fullName: "Employee One",
  role: "Employee",
};

const approver: AuthUser = {
  userId: "approver-1",
  email: "approver@example.com",
  fullName: "Approver One",
  role: "Approver",
};

const admin: AuthUser = {
  userId: "admin-1",
  email: "admin@example.com",
  fullName: "Admin One",
  role: "Admin",
};

const documentTypes: DocumentType[] = [
  {
    id: "type-payment",
    name: "Payment Procedure",
    description: "Payment workflow",
    categoryId: "category-payments",
    categoryName: "Payments",
    requiresApproval: true,
    createdAt: "2026-05-01T08:00:00Z",
  },
  {
    id: "type-leave",
    name: "Leave Request",
    description: "Leave workflow",
    categoryId: "category-hr",
    categoryName: "Human Resources",
    requiresApproval: true,
    createdAt: "2026-05-01T08:00:00Z",
  },
];

const makeDocument = (overrides: Partial<Document>): Document => ({
  id: "document-1",
  title: "Payment request",
  description: "Pay the vendor",
  documentTypeId: "type-payment",
  documentTypeName: "Payment Procedure",
  documentCategoryName: "Payments",
  createdByUserId: employee.userId,
  status: "PendingApproval",
  priority: "Normal",
  createdAt: "2026-05-10T08:00:00Z",
  updatedAt: "2026-05-10T08:00:00Z",
  ...overrides,
});

const pendingApproval: PendingApprovalItem = {
  documentId: "approval-document-1",
  title: "Laptop purchase",
  description: "Replacement equipment",
  documentTypeId: "type-payment",
  documentTypeName: "Payment Procedure",
  documentTypeDescription: "Payment workflow",
  documentCategoryName: "Payments",
  creatorId: employee.userId,
  creatorFullName: employee.fullName,
  creatorEmail: employee.email,
  priority: "High",
  amount: 1800,
  budgetCode: "IT-2026",
  createdAt: "2026-05-15T08:00:00Z",
  status: "PendingApproval",
};

const renderDashboard = (authUser = employee) =>
  render(<DashboardPage authUser={authUser} onLogout={vi.fn()} />);

describe("DashboardPage", () => {
  beforeEach(() => {
    Object.values(mocks).forEach((mock) => mock.mockReset());

    mocks.createDocument.mockResolvedValue(makeDocument({}));
    mocks.decideApproval.mockResolvedValue({});
    mocks.getApprovalHistory.mockImplementation(() => new Promise(() => {}));
    mocks.updateDocument.mockResolvedValue(makeDocument({}));
    mocks.useAdminUsers.mockReturnValue({
      data: [],
      isError: false,
      isLoading: false,
    });
    mocks.useDocumentTypes.mockReturnValue({
      data: documentTypes,
      isError: false,
    });
    mocks.useDocuments.mockReturnValue({
      data: [],
      isLoading: false,
    });
    mocks.useNotifications.mockReturnValue({
      data: [],
      isLoading: false,
    });
    mocks.usePendingApprovals.mockReturnValue({
      data: [],
      isLoading: false,
    });
  });

  it("renders employee empty states when there are no submissions", () => {
    renderDashboard();

    expect(screen.getByText("No submissions yet")).toBeInTheDocument();
    expect(screen.getByText("No submissions yet.")).toBeInTheDocument();
    expect(
      screen.getByText("Submit a document to start building your history."),
    ).toBeInTheDocument();
  });

  it("renders the notifications empty state", async () => {
    const user = userEvent.setup();

    renderDashboard();

    await user.click(screen.getByRole("button", { name: "Notifications" }));

    expect(screen.getByText("All caught up")).toBeInTheDocument();
    expect(screen.getByText("No unread notifications.")).toBeInTheDocument();
  });

  it("renders unread notifications and handles read actions", async () => {
    const user = userEvent.setup();
    mocks.useNotifications.mockReturnValue({
      data: [
        {
          id: "notification-1",
          title: "Approval requested",
          message: "Review the payment request.",
          type: "Approval",
          isRead: false,
          createdAt: "2026-05-15T08:00:00Z",
        },
        {
          id: "notification-2",
          title: "Changes requested",
          message: "Update the travel form.",
          type: "Document",
          isRead: false,
          createdAt: "2026-05-16T08:00:00Z",
        },
        {
          id: "notification-3",
          title: "Already read",
          message: "This should stay out of the unread menu.",
          type: "Document",
          isRead: true,
          createdAt: "2026-05-17T08:00:00Z",
        },
      ],
      isLoading: false,
    });

    renderDashboard();

    await user.click(screen.getByRole("button", { name: "Notifications" }));
    await user.click(screen.getAllByRole("button", { name: "Mark read" })[0]);
    await user.click(screen.getByRole("button", { name: "Mark all read" }));

    expect(screen.getByText("2 unread")).toBeInTheDocument();
    expect(screen.getByText("Approval requested")).toBeInTheDocument();
    expect(screen.getByText("Changes requested")).toBeInTheDocument();
    expect(screen.queryByText("Already read")).not.toBeInTheDocument();
    expect(mocks.markNotificationRead).toHaveBeenCalledWith("notification-1");
    expect(mocks.markAllNotificationsRead).toHaveBeenCalledTimes(1);
  });

  it("renders document statuses and only shows the signed-in user's documents", () => {
    mocks.useDocuments.mockReturnValue({
      data: [
        makeDocument({
          id: "document-old",
          title: "Conference payment",
          status: "PendingApproval",
          createdAt: "2026-05-10T08:00:00Z",
          updatedAt: "2026-05-10T08:00:00Z",
        }),
        makeDocument({
          id: "document-new",
          title: "Returned travel form",
          status: "ChangesRequested",
          createdAt: "2026-05-20T08:00:00Z",
          latestVersionLabel: "v1.1",
          latestVersionNumber: 2,
          updatedAt: "2026-05-21T08:00:00Z",
        }),
        makeDocument({
          id: "document-other",
          title: "Other employee request",
          createdByUserId: "employee-2",
        }),
      ],
      isLoading: false,
    });

    renderDashboard();

    expect(screen.getAllByText("Changes Requested").length).toBeGreaterThan(0);
    expect(screen.getAllByText("Pending Approval").length).toBeGreaterThan(0);
    expect(screen.getAllByText("Returned travel form").length).toBeGreaterThan(
      0,
    );
    expect(screen.getByText("Conference payment")).toBeInTheDocument();
    expect(
      screen.queryByText("Other employee request"),
    ).not.toBeInTheDocument();
    expect(screen.getAllByText("v1.1").length).toBeGreaterThan(0);
  });

  it("blocks an empty submission and marks required fields", async () => {
    const user = userEvent.setup();

    renderDashboard();

    await user.click(screen.getByRole("button", { name: "Submit" }));

    expect(mocks.createDocument).not.toHaveBeenCalled();
    expect(screen.getByLabelText("Document type")).toBeRequired();
    expect(screen.getByLabelText("Title")).toBeRequired();
  });

  it("submits a valid payment document with normalized payload values", async () => {
    const user = userEvent.setup();

    renderDashboard();

    await user.selectOptions(screen.getByLabelText("Document type"), [
      "type-payment",
    ]);
    await user.selectOptions(screen.getByLabelText("Priority"), ["High"]);
    await user.type(screen.getByLabelText("Title"), "Vendor invoice");
    await user.type(screen.getByLabelText("Details"), "Monthly cloud bill");
    await user.type(screen.getByLabelText("Amount"), "125.50");
    await user.type(screen.getByLabelText("Payment reference"), "FIN-42");
    await user.click(screen.getByRole("button", { name: "Submit" }));

    await waitFor(() => {
      expect(mocks.createDocument).toHaveBeenCalledWith({
        amount: 125.5,
        attachmentNote: null,
        budgetCode: "FIN-42",
        counterparty: null,
        description: "Monthly cloud bill",
        documentTypeId: "type-payment",
        leaveEndDate: null,
        leaveStartDate: null,
        leaveType: null,
        priority: "High",
        title: "Vendor invoice",
      });
    });
  });

  it("resubmits a returned document with change notes", async () => {
    const user = userEvent.setup();
    mocks.useDocuments.mockReturnValue({
      data: [
        makeDocument({
          id: "document-returned",
          title: "Returned travel form",
          status: "ChangesRequested",
          amount: 250,
          budgetCode: "TRAVEL-1",
          createdAt: "2026-05-20T08:00:00Z",
          updatedAt: "2026-05-21T08:00:00Z",
        }),
      ],
      isLoading: false,
    });

    renderDashboard();

    await user.click(screen.getByRole("button", { name: "Edit and resubmit" }));
    const resubmitForm = screen
      .getByRole("button", { name: "Submit changes" })
      .closest("form");

    expect(resubmitForm).not.toBeNull();

    const form = within(resubmitForm as HTMLFormElement);
    await user.clear(form.getByLabelText("Title"));
    await user.type(form.getByLabelText("Title"), "Updated travel form");
    await user.type(form.getByLabelText("Change notes"), "Added itinerary.");
    await user.click(form.getByRole("button", { name: "Submit changes" }));

    await waitFor(() => {
      expect(mocks.updateDocument).toHaveBeenCalledWith({
        id: "document-returned",
        data: expect.objectContaining({
          changeNotes: "Added itinerary.",
          status: "PendingApproval",
          title: "Updated travel form",
        }),
      });
    });
  });

  it("renders approver queue empty state", () => {
    renderDashboard(approver);

    expect(
      screen.getByText("No pending approval requests."),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        "Pending requests will appear here when they are assigned.",
      ),
    ).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Manage users" })).toBeNull();
  });

  it("renders pending approval details for approvers", () => {
    mocks.usePendingApprovals.mockReturnValue({
      data: [pendingApproval],
      isLoading: false,
    });

    renderDashboard(approver);

    expect(screen.getAllByText("Laptop purchase").length).toBeGreaterThan(0);
    expect(screen.getAllByText("Pending Approval").length).toBeGreaterThan(0);
    expect(screen.getByText("Submitted by Employee One")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Approve" })).toBeEnabled();
  });

  it("submits approval decisions with reviewer notes", async () => {
    const user = userEvent.setup();
    mocks.usePendingApprovals.mockReturnValue({
      data: [pendingApproval],
      isLoading: false,
    });

    renderDashboard(approver);

    await user.type(screen.getByLabelText("Reviewer notes"), "Needs receipt.");
    await user.click(screen.getByRole("button", { name: "Request changes" }));

    await waitFor(() => {
      expect(mocks.decideApproval).toHaveBeenCalledWith({
        documentId: "approval-document-1",
        action: "request-changes",
        comments: "Needs receipt.",
      });
    });
  });

  it("lets admins manage users from the approval workspace modal", async () => {
    const user = userEvent.setup();
    mocks.useAdminUsers.mockReturnValue({
      data: [
        {
          id: "employee-2",
          email: "two@example.com",
          fullName: "Employee Two",
          role: "Employee",
          isActive: true,
        },
      ],
      isError: false,
      isLoading: false,
    });

    renderDashboard(admin);

    await user.click(screen.getByRole("button", { name: "Manage users" }));
    await user.selectOptions(screen.getByDisplayValue("Employee"), "Approver");
    await user.click(screen.getByRole("button", { name: "Active" }));

    expect(screen.getByText("User management")).toBeInTheDocument();
    expect(mocks.updateAdminUserRole).toHaveBeenCalledWith({
      id: "employee-2",
      role: "Approver",
    });
    expect(mocks.updateAdminUserStatus).toHaveBeenCalledWith({
      id: "employee-2",
      isActive: false,
    });

    await user.click(screen.getByRole("button", { name: "Close" }));
    expect(screen.queryByText("User management")).not.toBeInTheDocument();
  });
});
