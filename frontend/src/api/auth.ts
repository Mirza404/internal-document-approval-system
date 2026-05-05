import { apiClient } from "./client";

export interface AuthResponse {
  userId: string;
  email: string;
  fullName: string;
  role: string;
}

export type CurrentUserResponse = AuthResponse;

export const getCurrentUser = (): Promise<CurrentUserResponse> =>
  apiClient.get("/auth/me");

export const microsoftLogin = (accessToken: string): Promise<AuthResponse> =>
  apiClient.post("/auth/microsoft/login", { accessToken });

export const microsoftRegister = (accessToken: string): Promise<AuthResponse> =>
  apiClient.post("/auth/microsoft/register", { accessToken });
