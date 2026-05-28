import { lazy, Suspense } from "react";
import { useMsal } from "@azure/msal-react";
import { Navigate, Route, Routes, useNavigate } from "react-router-dom";
import { useAuth } from "./hooks/useAuth";
import ProtectedRoute from "./components/auth/ProtectedRoute";

const AuthPage = lazy(() => import("./pages/AuthPage"));
const DashboardPage = lazy(() => import("./pages/DashboardPage"));
const AdminDashboard = lazy(() => import("./pages/AdminDashboard"));

const roleRedirect = (role?: string) => {
  switch ((role ?? "").toLowerCase()) {
    case "admin":
      return "/approvals";
    case "approver":
      return "/approvals";
    default:
      return "/dashboard";
  }
};

function App() {
  const { isAuthenticated, user, clearSession } = useAuth();
  const { instance } = useMsal();
  const navigate = useNavigate();
  const defaultRoute =
    isAuthenticated && user ? roleRedirect(user.role) : "/auth";

  const handleLogout = () => {
    const account =
      instance.getActiveAccount() ?? instance.getAllAccounts()[0];

    clearSession();
    sessionStorage.removeItem("authMode");
    void instance.clearCache({ account }).finally(() => {
      navigate("/auth", { replace: true });
    });
  };

  const dashboardElement =
    isAuthenticated && user ? (
      <DashboardPage authUser={user} onLogout={handleLogout} />
    ) : (
      <Navigate to="/auth" replace />
    );

  const adminElement =
    isAuthenticated && user ? (
      <AdminDashboard authUser={user} onLogout={handleLogout} />
    ) : (
      <Navigate to="/auth" replace />
    );

  const authElement =
    isAuthenticated && user ? (
      <Navigate to={roleRedirect(user.role)} replace />
    ) : (
      <AuthPage />
    );

  return (
    <Suspense
      fallback={
        <main className="grid min-h-screen place-items-center bg-background text-sm font-medium text-muted-foreground">
          Loading...
        </main>
      }
    >
      <Routes>
        <Route path="/" element={<Navigate to={defaultRoute} replace />} />
        <Route path="/auth" element={authElement} />
        <Route
          path="/dashboard"
          element={<ProtectedRoute>{dashboardElement}</ProtectedRoute>}
        />
        <Route
          path="/admin"
          element={
            <ProtectedRoute roles={["Admin"]}>
              {adminElement}
            </ProtectedRoute>
          }
        />
        <Route
          path="/approvals"
          element={
            <ProtectedRoute roles={["Approver", "Admin"]}>
              {dashboardElement}
            </ProtectedRoute>
          }
        />
        <Route path="*" element={<Navigate to={defaultRoute} replace />} />
      </Routes>
    </Suspense>
  );
}

export default App;