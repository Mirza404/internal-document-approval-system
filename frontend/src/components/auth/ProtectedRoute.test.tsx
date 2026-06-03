import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import ProtectedRoute from "./ProtectedRoute";
import type { AuthUser } from "../../auth/authStorage";

const useAuthMock = vi.fn();

vi.mock("../../hooks/useAuth", () => ({
  useAuth: () => useAuthMock(),
}));

const user: AuthUser = {
  userId: "user-1",
  email: "employee@example.com",
  fullName: "Employee One",
  role: "Employee",
};

const renderProtectedRoute = (roles?: string[]) =>
  render(
    <MemoryRouter initialEntries={["/admin"]}>
      <Routes>
        <Route
          path="/admin"
          element={
            <ProtectedRoute roles={roles}>
              <div>Protected content</div>
            </ProtectedRoute>
          }
        />
        <Route path="/auth" element={<div>Auth page</div>} />
        <Route path="/dashboard" element={<div>Dashboard page</div>} />
      </Routes>
    </MemoryRouter>,
  );

describe("ProtectedRoute", () => {
  beforeEach(() => {
    useAuthMock.mockReset();
  });

  it("redirects anonymous users to auth", () => {
    useAuthMock.mockReturnValue({
      isAuthenticated: false,
      user: null,
    });

    renderProtectedRoute();

    expect(screen.getByText("Auth page")).toBeInTheDocument();
    expect(screen.queryByText("Protected content")).not.toBeInTheDocument();
  });

  it("redirects authenticated users without the required role", () => {
    useAuthMock.mockReturnValue({
      isAuthenticated: true,
      user,
    });

    renderProtectedRoute(["Admin"]);

    expect(screen.getByText("Dashboard page")).toBeInTheDocument();
    expect(screen.queryByText("Protected content")).not.toBeInTheDocument();
  });

  it("renders protected content when the role matches case-insensitively", () => {
    useAuthMock.mockReturnValue({
      isAuthenticated: true,
      user: { ...user, role: "admin" },
    });

    renderProtectedRoute(["Admin"]);

    expect(screen.getByText("Protected content")).toBeInTheDocument();
  });
});
