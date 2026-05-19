import { apiClient } from "./client";

export interface AdminUser {
  id: string;
  email: string;
  fullName: string;
  role: string;
  isActive: boolean;
}

export const getAdminUsers = (): Promise<AdminUser[]> =>
  apiClient.get("/admin/users");

export const updateAdminUserRole = (
  id: string,
  role: string,
): Promise<AdminUser> => apiClient.put(`/admin/users/${id}/role`, { role });

export const updateAdminUserStatus = (
  id: string,
  isActive: boolean,
): Promise<AdminUser> =>
  apiClient.put(`/admin/users/${id}/status`, { isActive });
