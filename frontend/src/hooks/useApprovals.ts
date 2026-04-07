import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  getApprovals,
  getApproval,
  createApproval,
  updateApproval,
  type Approval,
} from "../api/approvals";

// Query hooks
export const useApprovals = () => {
  return useQuery({
    queryKey: ["approvals"],
    queryFn: getApprovals,
  });
};

export const useApproval = (id: string) => {
  return useQuery({
    queryKey: ["approvals", id],
    queryFn: () => getApproval(id),
    enabled: !!id,
  });
};

// Mutation hooks
export const useCreateApproval = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createApproval,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["approvals"] });
    },
  });
};

export const useUpdateApproval = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Approval> }) =>
      updateApproval(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["approvals"] });
    },
  });
};
