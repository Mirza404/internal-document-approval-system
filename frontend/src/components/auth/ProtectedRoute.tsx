import type { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";

interface ProtectedRouteProps {
  children: ReactNode;
  roles?: string[];
  redirectTo?: string;
}

const isRoleAllowed = (role: string | undefined, roles?: string[]) => {
  if (!roles || roles.length === 0) {
    return true;
  }

  if (!role) {
    return false;
  }

  return roles.map((item) => item.toLowerCase()).includes(role.toLowerCase());
};

const ProtectedRoute = ({
  children,
  roles,
  redirectTo = "/auth",
}: ProtectedRouteProps) => {
  const { isAuthenticated, user } = useAuth();

  if (!isAuthenticated || !user) {
    return <Navigate to={redirectTo} replace />;
  }

  if (!isRoleAllowed(user.role, roles)) {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
};

export default ProtectedRoute;
