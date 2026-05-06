import { apiClient } from "./client";

// Types (placeholders)
export interface Approval {
  id: string;
  documentId: string;
  approverId: string;
  status: "pending" | "approved" | "rejected";
  // Add more fields
}

export interface CreateApprovalRequest {
  documentId: string;
  status?: "pending" | "approved" | "rejected";
  comments?: string;
}

// API functions
export const getApprovals = (): Promise<Approval[]> =>
  apiClient.get("/approvals");

export const getApproval = (id: string): Promise<Approval> =>
  apiClient.get(`/approvals/${id}`);

export const createApproval = (
  data: CreateApprovalRequest,
): Promise<Approval> => apiClient.post("/approvals", data);

export const updateApproval = (
  id: string,
  data: Partial<Approval>,
): Promise<Approval> => apiClient.put(`/approvals/${id}`, data);
