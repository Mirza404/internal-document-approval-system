import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getAdminUsers,
  updateAdminUserRole,
  updateAdminUserStatus,
  type AdminUser,
} from "../api/adminUsers";

export const useAdminUsers = () =>
  useQuery({
    queryKey: ["admin", "users"],
    queryFn: getAdminUsers,
  });

export const useUpdateAdminUserRole = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, role }: { id: string; role: string }) =>
      updateAdminUserRole(id, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "users"] });
    },
  });
};

export const useUpdateAdminUserStatus = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      updateAdminUserStatus(id, isActive),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "users"] });
    },
  });
};
