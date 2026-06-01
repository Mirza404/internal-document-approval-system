import { apiClient } from "./client";

export interface AuthResponse {
  userId: string;
  email: string;
  fullName: string;
  role: string;
  accessToken: string;
}

export interface CurrentUserResponse {
  userId: string;
  email: string;
  fullName: string;
  role: string;
}

export const getCurrentUser = (): Promise<CurrentUserResponse> =>
  apiClient.get("/auth/me");

export const microsoftLogin = (accessToken: string): Promise<AuthResponse> =>
  apiClient.post("/auth/microsoft/login", { accessToken });

export const localLogin = (
  email: string,
  password: string,
): Promise<AuthResponse> =>
  apiClient.post("/auth/local/login", { email, password });

export const localRegister = (
  email: string,
  fullName: string,
  password: string,
): Promise<AuthResponse> =>
  apiClient.post("/auth/local/register", { email, fullName, password });
