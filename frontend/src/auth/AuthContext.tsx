import { useCallback, useMemo, useState, type ReactNode } from "react";
import {
  clearAuthSession,
  loadAuthSession,
  saveAuthSession,
  type AuthUser,
} from "./authStorage";
import { AuthContext } from "./authContext";

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [token, setToken] = useState<string | null>(
    () => loadAuthSession()?.token ?? null,
  );
  const [user, setUser] = useState<AuthUser | null>(
    () => loadAuthSession()?.user ?? null,
  );

  const setSession = useCallback((nextToken: string, nextUser: AuthUser) => {
    setToken(nextToken);
    setUser(nextUser);
    saveAuthSession(nextToken, nextUser);
  }, []);

  const clearSession = useCallback(() => {
    setToken(null);
    setUser(null);
    clearAuthSession();
  }, []);

  const value = useMemo(
    () => ({
      token,
      user,
      isAuthenticated: Boolean(token && user),
      setSession,
      clearSession,
    }),
    [token, user, setSession, clearSession],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
