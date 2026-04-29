import { apiClient } from "./client";

export interface AuthResponse {
  userId: string;
  email: string;
  fullName: string;
  role: string;
  accessToken: string;
}

export type CurrentUserResponse = Omit<AuthResponse, "accessToken">;

export const getCurrentUser = (): Promise<CurrentUserResponse> =>
  apiClient.get("/auth/me");

export const microsoftLogin = (accessToken: string): Promise<AuthResponse> =>
  apiClient.post("/auth/microsoft/login", { accessToken });

export const microsoftRegister = (accessToken: string): Promise<AuthResponse> =>
  apiClient.post("/auth/microsoft/register", { accessToken });
