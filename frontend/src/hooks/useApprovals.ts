import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  approveDocument,
  createApproval,
  getApproval,
  getApprovals,
  getPendingApprovals,
  rejectDocument,
  requestDocumentChanges,
  updateApproval,
  type Approval,
  type ApprovalDecisionRequest,
  type PendingApprovalItem,
} from "../api/approvals";

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

export const usePendingApprovals = () => {
  return useQuery<PendingApprovalItem[]>({
    queryKey: ["approvals", "pending"],
    queryFn: getPendingApprovals,
  });
};

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

const useApprovalDecisionMutation = (
  mutationFn: (documentId: string, data: ApprovalDecisionRequest) => Promise<Approval>,
) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      documentId,
      comments,
    }: {
      documentId: string;
      comments?: string | null;
    }) => mutationFn(documentId, { comments }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["approvals"] });
      queryClient.invalidateQueries({ queryKey: ["approvals", "pending"] });
      queryClient.invalidateQueries({ queryKey: ["documents"] });
    },
  });
};

export const useApproveDocument = () =>
  useApprovalDecisionMutation(approveDocument);

export const useRejectDocument = () =>
  useApprovalDecisionMutation(rejectDocument);

export const useRequestDocumentChanges = () =>
  useApprovalDecisionMutation(requestDocumentChanges);