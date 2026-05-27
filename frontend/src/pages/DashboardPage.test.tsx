import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type { Document } from "../api/documents";
import DashboardPage from "./DashboardPage";

const scrollIntoView = vi.fn();

const documents: Document[] = [
  {
    id: "document-new",
    title: "Current transcript request",
    description: "Current document details",
    documentTypeId: "transcript",
    documentTypeName: "Transcript",
    documentCategoryName: "Academic Records",
    createdByUserId: "employee-1",
    status: "PendingApproval",
    priority: "High",
    createdAt: "2026-06-01T10:00:00Z",
  },
  {
    id: "document-old",
    title: "Older certificate request",
    description: "Older document details",
    documentTypeId: "certificate",
    documentTypeName: "Certificate",
    documentCategoryName: "Student Services",
    createdByUserId: "employee-1",
    status: "Approved",
    priority: "Normal",
    createdAt: "2026-05-20T10:00:00Z",
  },
];

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
      {
        id: "certificate",
        name: "Certificate",
        categoryName: "Student Services",
      },
    ],
    isError: false,
  }),
}));

vi.mock("../hooks/useDocuments", () => ({
  useDocuments: () => ({ data: documents, isLoading: false }),
  useCreateDocument: () => ({ mutateAsync: vi.fn(), isPending: false }),
  useUpdateDocument: () => ({ mutateAsync: vi.fn(), isPending: false }),
}));

vi.mock("../hooks/useNotifications", () => ({
  useNotifications: () => ({ data: [], isLoading: false }),
  useMarkNotificationRead: () => ({ mutate: vi.fn(), isPending: false }),
  useMarkAllNotificationsRead: () => ({ mutate: vi.fn(), isPending: false }),
}));

describe("EmployeeDashboard document detail", () => {
  beforeEach(() => {
    scrollIntoView.mockClear();
    Object.defineProperty(Element.prototype, "scrollIntoView", {
      configurable: true,
      value: scrollIntoView,
    });
  });

  const renderDashboard = (isNarrow: boolean) => {
    Object.defineProperty(window, "matchMedia", {
      configurable: true,
      value: vi.fn().mockReturnValue({
        matches: isNarrow,
        media: "(max-width: 1279px)",
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
      }),
    });

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

  it("keeps the detail card sticky and allows collapsing its content", async () => {
    const user = userEvent.setup();
    renderDashboard(false);

    const collapseButton = screen.getByRole("button", {
      name: "Collapse detail",
    });

    expect(collapseButton.closest("section")).toHaveClass("xl:sticky");
    expect(screen.getByText("Current document details")).toBeInTheDocument();

    await user.click(collapseButton);

    expect(
      screen.queryByText("Current document details"),
    ).not.toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Expand detail" }),
    ).toBeInTheDocument();
  });

  it("selects a submission and scrolls the detail card into view on narrow screens", async () => {
    const user = userEvent.setup();
    renderDashboard(true);

    await user.click(
      screen.getByRole("button", {
        name: "Show details for Older certificate request",
      }),
    );

    expect(screen.getByText("Older document details")).toBeInTheDocument();
    await waitFor(() => {
      expect(scrollIntoView).toHaveBeenCalledWith({
        behavior: "smooth",
        block: "start",
      });
    });
  });
});
