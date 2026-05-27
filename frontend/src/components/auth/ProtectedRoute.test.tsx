import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { AuthContext, type AuthContextValue } from "../../auth/authContext";
import ProtectedRoute from "./ProtectedRoute";

const authenticatedContext = (role: string): AuthContextValue => ({
  token: "token",
  user: {
    userId: "user-1",
    email: "user@internaldocs.local",
    fullName: "Test User",
    role,
  },
  isAuthenticated: true,
  setSession: vi.fn(),
  clearSession: vi.fn(),
});

const renderProtectedRoute = (context: AuthContextValue) => {
  render(
    <AuthContext.Provider value={context}>
      <MemoryRouter initialEntries={["/admin"]}>
        <Routes>
          <Route path="/auth" element={<p>Authentication page</p>} />
          <Route path="/dashboard" element={<p>Dashboard page</p>} />
          <Route
            path="/admin"
            element={
              <ProtectedRoute roles={["Admin"]}>
                <p>Admin page</p>
              </ProtectedRoute>
            }
          />
        </Routes>
      </MemoryRouter>
    </AuthContext.Provider>,
  );
};

describe("ProtectedRoute", () => {
  it("renders protected content for an allowed role without case sensitivity", () => {
    renderProtectedRoute(authenticatedContext("admin"));

    expect(screen.getByText("Admin page")).toBeInTheDocument();
  });

  it("redirects unauthenticated users to the authentication page", () => {
    renderProtectedRoute({
      token: null,
      user: null,
      isAuthenticated: false,
      setSession: vi.fn(),
      clearSession: vi.fn(),
    });

    expect(screen.getByText("Authentication page")).toBeInTheDocument();
  });

  it("redirects users without the required role to the dashboard", () => {
    renderProtectedRoute(authenticatedContext("Employee"));

    expect(screen.getByText("Dashboard page")).toBeInTheDocument();
  });
});
