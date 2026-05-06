export interface AuthUser {
  userId: string;
  email: string;
  fullName: string;
  role: string;
}

const authTokenKey = "authToken";
const authUserKey = "authUser";

export const loadAuthToken = (): string | null =>
  localStorage.getItem(authTokenKey);

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

export const saveAuthSession = (token: string, user: AuthUser) => {
  localStorage.setItem(authTokenKey, token);
  localStorage.setItem(authUserKey, JSON.stringify(user));
};

export const clearAuthSession = () => {
  localStorage.removeItem(authTokenKey);
  localStorage.removeItem(authUserKey);
};
