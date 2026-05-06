import { createContext } from "react";
import type { AuthUser } from "./authStorage";

export interface AuthContextValue {
  token: string | null;
  user: AuthUser | null;
  isAuthenticated: boolean;
  setSession: (token: string, user: AuthUser) => void;
  clearSession: () => void;
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);
