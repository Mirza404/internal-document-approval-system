import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type { Document } from "../api/documents";
import DashboardPage from "./DashboardPage";

const mocks = vi.hoisted(() => ({
  createDocument: vi.fn(),
  documents: [] as Document[],
}));

const createDocument = (
  id: string,
  title: string,
  status: string,
): Document => ({
  id,
  title,
  description: `${title} details`,
  documentTypeId: "transcript",
  documentTypeName: "Transcript",
  documentCategoryName: "Academic Records",
  createdByUserId: "employee-1",
  status,
  priority: "Normal",
  createdAt: "2026-06-01T10:00:00Z",
});

vi.mock("../api/documents", async (importOriginal) => {
  const original = await importOriginal<typeof import("../api/documents")>();

  return {
    ...original,
    getApprovalHistory: vi.fn().mockResolvedValue([]),
  };
});

vi.mock("../hooks/useDocumentCatalog", () => ({
  useDocumentTypes: () => ({
    data: [
      {
        id: "transcript",
        name: "Transcript",
        categoryName: "Academic Records",
      },
    ],
    isError: false,
  }),
}));

vi.mock("../hooks/useDocuments", () => ({
  useDocuments: () => ({ data: mocks.documents, isLoading: false }),
  useCreateDocument: () => ({
    mutateAsync: mocks.createDocument,
    isPending: false,
  }),
  useUpdateDocument: () => ({ mutateAsync: vi.fn(), isPending: false }),
}));

vi.mock("../hooks/useNotifications", () => ({
  useNotifications: () => ({ data: [], isLoading: false }),
  useMarkNotificationRead: () => ({ mutate: vi.fn(), isPending: false }),
  useMarkAllNotificationsRead: () => ({ mutate: vi.fn(), isPending: false }),
}));

const renderDashboard = () => {
  render(
    <DashboardPage
      authUser={{
        userId: "employee-1",
        email: "employee@internaldocs.local",
        fullName: "Demo Employee",
        role: "Employee",
      }}
      onLogout={vi.fn()}
    />,
  );
};

describe("EmployeeDashboard states", () => {
  beforeEach(() => {
    mocks.createDocument.mockReset();
    mocks.documents = [];
  });

  it("blocks submission while required fields are empty", async () => {
    const user = userEvent.setup();
    renderDashboard();

    await user.click(screen.getByRole("button", { name: "Submit" }));

    expect(mocks.createDocument).not.toHaveBeenCalled();
    expect(screen.getByLabelText("Document type")).toBeRequired();
    expect(screen.getByLabelText("Title")).toBeRequired();
  });

  it("renders readable labels for each supported document status", () => {
    mocks.documents = [
      createDocument("draft", "Draft certificate", "Draft"),
      createDocument("pending", "Pending transcript", "PendingApproval"),
      createDocument("changes", "Returned transcript", "ChangesRequested"),
      createDocument("approved", "Approved transcript", "Approved"),
      createDocument("rejected", "Rejected transcript", "Rejected"),
    ];

    renderDashboard();

    expect(screen.getAllByText("Draft").length).toBeGreaterThan(0);
    expect(screen.getAllByText("Pending Approval").length).toBeGreaterThan(0);
    expect(screen.getAllByText("Changes Requested").length).toBeGreaterThan(0);
    expect(screen.getAllByText("Approved").length).toBeGreaterThan(0);
    expect(screen.getAllByText("Rejected").length).toBeGreaterThan(0);
  });

  it("shows useful empty states when the employee has no submissions", () => {
    renderDashboard();

    expect(
      screen.getByText("Submit a document to start building your history."),
    ).toBeInTheDocument();
    expect(screen.getAllByText("No submissions yet.")).toHaveLength(1);
  });
});
