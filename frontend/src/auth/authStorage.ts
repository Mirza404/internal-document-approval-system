import { jwtDecode } from "jwt-decode";

export interface AuthUser {
  userId: string;
  email: string;
  fullName: string;
  role: string;
}

interface JwtPayload {
  exp?: number;
}

export interface AuthSession {
  token: string;
  user: AuthUser;
}

const authTokenKey = "authToken";
const authUserKey = "authUser";

const isTokenCurrent = (token: string) => {
  try {
    const { exp } = jwtDecode<JwtPayload>(token);
    return typeof exp === "number" && exp * 1000 > Date.now();
  } catch {
    return false;
  }
};

export const clearAuthSession = () => {
  localStorage.removeItem(authTokenKey);
  localStorage.removeItem(authUserKey);
};

export const loadAuthToken = (): string | null => {
  const token = localStorage.getItem(authTokenKey);
  if (!token) {
    return null;
  }

  if (!isTokenCurrent(token)) {
    clearAuthSession();
    return null;
  }

  return token;
};

export const loadAuthUser = (): AuthUser | null => {
  const raw = localStorage.getItem(authUserKey);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as AuthUser;
  } catch {
    localStorage.removeItem(authUserKey);
    return null;
  }
};

export const loadAuthSession = (): AuthSession | null => {
  const token = loadAuthToken();
  const user = loadAuthUser();

  if (!token || !user) {
    clearAuthSession();
    return null;
  }

  return { token, user };
};

export const saveAuthSession = (token: string, user: AuthUser) => {
  localStorage.setItem(authTokenKey, token);
  localStorage.setItem(authUserKey, JSON.stringify(user));
};
