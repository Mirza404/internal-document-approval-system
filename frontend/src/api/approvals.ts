import { apiClient } from "./client";

export interface Approval {
  id: string;
  documentId: string;
  approverId: string;
  approverFullName?: string;
  status: string;
  comments?: string | null;
  createdAt: string;
}

export interface CreateApprovalRequest {
  documentId: string;
  status?: string;
  comments?: string;
}

export interface ApprovalDecisionRequest {
  comments?: string | null;
}

export interface PendingApprovalItem {
  documentId: string;
  title: string;
  documentTypeId: string;
  documentTypeName: string;
  creatorId: string;
  creatorFullName: string;
  createdAt: string;
  status: string;
}

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

export const getPendingApprovals = (): Promise<PendingApprovalItem[]> =>
  apiClient.get("/approvals/pending");

export const approveDocument = (
  documentId: string,
  data: ApprovalDecisionRequest,
): Promise<Approval> =>
  apiClient.post(`/approvals/${documentId}/approve`, data);

export const rejectDocument = (
  documentId: string,
  data: ApprovalDecisionRequest,
): Promise<Approval> => apiClient.post(`/approvals/${documentId}/reject`, data);

export const requestDocumentChanges = (
  documentId: string,
  data: ApprovalDecisionRequest,
): Promise<Approval> =>
  apiClient.post(`/approvals/${documentId}/request-changes`, data);
