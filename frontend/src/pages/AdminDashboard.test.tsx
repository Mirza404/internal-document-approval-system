import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type { AuthUser } from "../auth/authStorage";
import AdminDashboard from "./AdminDashboard";

const mocks = vi.hoisted(() => ({
  createDocumentType: vi.fn(),
  deleteDocumentType: vi.fn(),
  navigate: vi.fn(),
  updateAdminUserRole: vi.fn(),
  updateAdminUserStatus: vi.fn(),
  updateDocumentType: vi.fn(),
  useAdminUsers: vi.fn(),
  useDocumentCategories: vi.fn(),
  useDocumentTypes: vi.fn(),
}));

vi.mock("react-router-dom", async (importOriginal) => {
  const actual = await importOriginal<typeof import("react-router-dom")>();

  return {
    ...actual,
    useNavigate: () => mocks.navigate,
  };
});

vi.mock("../hooks/useAdminUsers", () => ({
  useAdminUsers: () => mocks.useAdminUsers(),
  useUpdateAdminUserRole: () => ({
    mutateAsync: mocks.updateAdminUserRole,
  }),
  useUpdateAdminUserStatus: () => ({
    mutateAsync: mocks.updateAdminUserStatus,
  }),
}));

vi.mock("../hooks/useDocumentCatalog", () => ({
  useCreateDocumentType: () => ({
    isPending: false,
    mutateAsync: mocks.createDocumentType,
  }),
  useDeleteDocumentType: () => ({
    isPending: false,
    mutateAsync: mocks.deleteDocumentType,
  }),
  useDocumentCategories: () => mocks.useDocumentCategories(),
  useDocumentTypes: () => mocks.useDocumentTypes(),
  useUpdateDocumentType: () => ({
    isPending: false,
    mutateAsync: mocks.updateDocumentType,
  }),
}));

const admin: AuthUser = {
  userId: "admin-1",
  email: "admin@example.com",
  fullName: "Admin One",
  role: "Admin",
};

const renderAdminDashboard = () =>
  render(<AdminDashboard authUser={admin} onLogout={vi.fn()} />);

describe("AdminDashboard", () => {
  beforeEach(() => {
    Object.values(mocks).forEach((mock) => mock.mockReset());
    vi.spyOn(window, "confirm").mockReturnValue(true);

    mocks.createDocumentType.mockResolvedValue({});
    mocks.deleteDocumentType.mockResolvedValue(undefined);
    mocks.updateAdminUserRole.mockResolvedValue({});
    mocks.updateAdminUserStatus.mockResolvedValue({});
    mocks.updateDocumentType.mockResolvedValue({});
    mocks.useAdminUsers.mockReturnValue({
      data: [
        {
          id: "employee-1",
          email: "employee@example.com",
          fullName: "Employee One",
          role: "Employee",
          isActive: true,
        },
      ],
      isError: false,
      isLoading: false,
    });
    mocks.useDocumentCategories.mockReturnValue({
      data: [
        {
          id: "category-finance",
          name: "Finance",
          description: "Finance documents",
          createdAt: "2026-05-01T08:00:00Z",
        },
      ],
    });
    mocks.useDocumentTypes.mockReturnValue({
      data: [
        {
          id: "type-payment",
          name: "Payment Procedure",
          description: "Payment workflow",
          categoryId: "category-finance",
          categoryName: "Finance",
          requiresApproval: true,
          createdAt: "2026-05-01T08:00:00Z",
        },
      ],
      isLoading: false,
    });
  });

  it("updates user roles and active status", async () => {
    const user = userEvent.setup();

    renderAdminDashboard();

    await user.selectOptions(screen.getByDisplayValue("Employee"), "Admin");
    await user.click(screen.getByRole("button", { name: "Active" }));

    expect(mocks.updateAdminUserRole).toHaveBeenCalledWith({
      id: "employee-1",
      role: "Admin",
    });
    expect(mocks.updateAdminUserStatus).toHaveBeenCalledWith({
      id: "employee-1",
      isActive: false,
    });
  });

  it("creates a document type", async () => {
    const user = userEvent.setup();

    renderAdminDashboard();

    await user.click(screen.getByRole("button", { name: "Document Types" }));
    await user.type(
      screen.getByPlaceholderText("Enter document type name"),
      "Travel Request",
    );
    await user.type(
      screen.getByPlaceholderText("Enter description"),
      "Travel approvals",
    );
    await user.selectOptions(screen.getByDisplayValue("Select a category"), [
      "category-finance",
    ]);
    await user.click(screen.getByRole("button", { name: "Create" }));

    await waitFor(() => {
      expect(mocks.createDocumentType).toHaveBeenCalledWith({
        categoryId: "category-finance",
        description: "Travel approvals",
        name: "Travel Request",
        requiresApproval: true,
      });
    });
  });

  it("edits and cancels document type changes", async () => {
    const user = userEvent.setup();

    renderAdminDashboard();

    await user.click(screen.getByRole("button", { name: "Document Types" }));
    const paymentCard = screen
      .getByRole("heading", { name: "Payment Procedure" })
      .closest("article");

    expect(paymentCard).not.toBeNull();

    await user.click(
      within(paymentCard as HTMLElement).getByRole("button", { name: "Edit" }),
    );
    await user.clear(screen.getByPlaceholderText("Enter document type name"));
    await user.type(
      screen.getByPlaceholderText("Enter document type name"),
      "Payment Procedure Updated",
    );
    await user.click(screen.getByRole("button", { name: "Update" }));

    await waitFor(() => {
      expect(mocks.updateDocumentType).toHaveBeenCalledWith({
        id: "type-payment",
        data: {
          categoryId: "category-finance",
          description: "Payment workflow",
          name: "Payment Procedure Updated",
          requiresApproval: true,
        },
      });
    });

    await user.click(
      within(paymentCard as HTMLElement).getByRole("button", { name: "Edit" }),
    );
    await user.click(screen.getByRole("button", { name: "Cancel" }));

    expect(
      screen.getByRole("heading", { name: "Create Document Type" }),
    ).toBeInTheDocument();
  });

  it("deletes a document type after confirmation", async () => {
    const user = userEvent.setup();

    renderAdminDashboard();

    await user.click(screen.getByRole("button", { name: "Document Types" }));
    await user.click(screen.getByRole("button", { name: "Delete" }));

    expect(window.confirm).toHaveBeenCalledWith(
      "Are you sure you want to delete this document type?",
    );
    expect(mocks.deleteDocumentType).toHaveBeenCalledWith("type-payment");
  });

  it("does not delete a document type when confirmation is cancelled", async () => {
    const user = userEvent.setup();
    vi.mocked(window.confirm).mockReturnValue(false);

    renderAdminDashboard();

    await user.click(screen.getByRole("button", { name: "Document Types" }));
    await user.click(screen.getByRole("button", { name: "Delete" }));

    expect(mocks.deleteDocumentType).not.toHaveBeenCalled();
  });
});
